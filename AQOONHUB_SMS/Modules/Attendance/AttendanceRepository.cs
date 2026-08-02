using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AQOONHUB_SMS.Modules.Attendance
{
    /// <summary>
    /// Single data-access layer for the whole Attendance module
    /// (AttendanceSessions, AttendanceRecords, AttendanceSettings) plus the
    /// lookups and role checks the pages need. Direct ADO.NET, parameterised.
    /// Stage 1: Overview + Settings. Later stages extend this class.
    /// </summary>
    public sealed class AttendanceRepository
    {
        private readonly string _connectionString;

        public AttendanceRepository()
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["AQOONHUB_DB"];
            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
                throw new ConfigurationErrorsException("Connection string 'AQOONHUB_DB' was not found.");
            _connectionString = settings.ConnectionString;
        }

        // ================================================================
        // ROLES
        // ================================================================
        public string NormalizeRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role)) return string.Empty;
            return role.Trim().Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();
        }

        /// <summary>Full attendance management: reports, settings, reopen, analytics.</summary>
        public bool CanManageAttendance(string role)
        {
            string r = NormalizeRole(role);
            return r == "superadmin" || r == "admin" || r == "academic" || r == "attendanceofficer";
        }

        /// <summary>Mark attendance (managers plus teachers, scoped to assignments).</summary>
        public bool CanMarkAttendance(string role)
        {
            return CanManageAttendance(role) || NormalizeRole(role) == "teacher";
        }

        /// <summary>Only managers may change global settings.</summary>
        public bool CanEditSettings(string role) { return CanManageAttendance(role); }

        /// <summary>View attendance pages (managers, teacher, registrar).</summary>
        public bool CanViewAttendance(string role)
        {
            string r = NormalizeRole(role);
            return CanMarkAttendance(role) || r == "registrar";
        }

        // ================================================================
        // LOOKUPS
        // ================================================================
        public DataTable GetAcademicYears()
        {
            return ExecuteDataTable("SELECT AcademicYearID, YearName, Status FROM AcademicYears ORDER BY StartDate DESC", null);
        }

        public int GetActiveAcademicYearId()
        {
            object o = ExecuteScalar("SELECT TOP 1 AcademicYearID FROM AcademicYears WHERE Status='Active' ORDER BY AcademicYearID DESC", null);
            return o == null || o == DBNull.Value ? 0 : Convert.ToInt32(o);
        }

        public DataTable GetTerms(int academicYearId)
        {
            return ExecuteDataTable("SELECT TermID, TermName FROM Terms WHERE (@y=0 OR AcademicYearID=@y) ORDER BY StartDate", new[] { P("@y", academicYearId) });
        }

        public DataTable GetClasses(int academicYearId)
        {
            return ExecuteDataTable(
                "SELECT ClassID, ClassName FROM Classes WHERE (@y=0 OR AcademicYearID=@y) AND ISNULL(Status,'Active')='Active' ORDER BY ClassName",
                new[] { P("@y", academicYearId) });
        }

        public DataTable GetSectionsForClass(int classId)
        {
            return ExecuteDataTable(
                "SELECT SectionID, SectionName, ClassID FROM Sections WHERE ClassID=@c AND ISNULL(Status,'Active')='Active' ORDER BY SectionName",
                new[] { P("@c", classId) });
        }

        // ================================================================
        // SETTINGS
        // ================================================================
        public DataRow GetAttendanceSettings()
        {
            DataTable t = ExecuteDataTable("SELECT TOP 1 * FROM AttendanceSettings ORDER BY AttendanceSettingsID", null);
            if (t.Rows.Count == 0)
            {
                ExecuteNonQuery("INSERT INTO AttendanceSettings DEFAULT VALUES", null);
                t = ExecuteDataTable("SELECT TOP 1 * FROM AttendanceSettings ORDER BY AttendanceSettingsID", null);
            }
            return t.Rows[0];
        }

        public void SaveAttendanceSettings(
            bool allowTeachers, bool allowEditAfterSubmit, int editWindowHours,
            TimeSpan startTime, TimeSpan endTime, int lateAfterMinutes,
            bool excusedRequiresRemarks, bool includeLateAsAttended, bool excludeExcusedFromRate,
            bool allowFutureDate, bool enableParent, bool enableEmail, bool enableSms,
            int consecutiveAbsenceAlert, decimal lowAttendanceThreshold, int userId)
        {
            if (editWindowHours < 0) throw new ArgumentException("Edit window hours cannot be negative.");
            if (lateAfterMinutes < 0) throw new ArgumentException("Late-after minutes cannot be negative.");
            if (consecutiveAbsenceAlert < 1) throw new ArgumentException("Consecutive absence threshold must be at least 1.");
            if (lowAttendanceThreshold < 0 || lowAttendanceThreshold > 100) throw new ArgumentException("Low attendance threshold must be between 0 and 100.");
            if (endTime <= startTime) throw new ArgumentException("End time must be after start time.");

            const string sql = @"
UPDATE dbo.AttendanceSettings SET
    AllowTeachersToMark=@t, AllowEditAfterSubmission=@e, EditWindowHours=@win,
    AttendanceStartTime=@st, AttendanceEndTime=@en, LateAfterMinutes=@late,
    ExcusedRequiresRemarks=@exr, IncludeLateAsAttended=@incl, ExcludeExcusedFromRate=@exc,
    AllowFutureDate=@fut, EnableParentNotifications=@pn, EnableEmailNotifications=@em, EnableSMSNotifications=@sm,
    ConsecutiveAbsenceAlert=@ca, LowAttendanceThreshold=@lt, UpdatedBy=@u, UpdatedAt=GETDATE()
WHERE AttendanceSettingsID = (SELECT TOP 1 AttendanceSettingsID FROM dbo.AttendanceSettings ORDER BY AttendanceSettingsID);
IF @@ROWCOUNT = 0
    INSERT INTO dbo.AttendanceSettings
        (AllowTeachersToMark, AllowEditAfterSubmission, EditWindowHours, AttendanceStartTime, AttendanceEndTime,
         LateAfterMinutes, ExcusedRequiresRemarks, IncludeLateAsAttended, ExcludeExcusedFromRate, AllowFutureDate,
         EnableParentNotifications, EnableEmailNotifications, EnableSMSNotifications, ConsecutiveAbsenceAlert,
         LowAttendanceThreshold, UpdatedBy, UpdatedAt)
    VALUES (@t,@e,@win,@st,@en,@late,@exr,@incl,@exc,@fut,@pn,@em,@sm,@ca,@lt,@u,GETDATE());";
            ExecuteNonQuery(sql, new[]
            {
                P("@t", allowTeachers), P("@e", allowEditAfterSubmit), P("@win", editWindowHours),
                P("@st", startTime), P("@en", endTime), P("@late", lateAfterMinutes),
                P("@exr", excusedRequiresRemarks), P("@incl", includeLateAsAttended), P("@exc", excludeExcusedFromRate),
                P("@fut", allowFutureDate), P("@pn", enableParent), P("@em", enableEmail), P("@sm", enableSms),
                P("@ca", consecutiveAbsenceAlert), P("@lt", lowAttendanceThreshold), P("@u", userId)
            });
        }

        // ================================================================
        // OVERVIEW DASHBOARD
        // ================================================================
        /// <summary>Totals for the overview cards: enrolled students in the year plus
        /// today's Present/Absent/Late/Excused counts and the settings-aware rate.</summary>
        public DataRow GetAttendanceDashboardSummary(int academicYearId, DateTime date)
        {
            DataRow s = GetAttendanceSettings();
            bool includeLate = Convert.ToBoolean(s["IncludeLateAsAttended"]);
            bool excludeExcused = Convert.ToBoolean(s["ExcludeExcusedFromRate"]);

            const string sql = @"
DECLARE @Present int, @Absent int, @Late int, @Excused int, @Total int;
SELECT @Total = COUNT(*) FROM Students st WHERE (@y=0 OR st.AcademicYearID=@y) AND ISNULL(st.Status,'Active')='Active';

SELECT
    @Present = SUM(CASE WHEN r.AttendanceStatus='Present' THEN 1 ELSE 0 END),
    @Absent  = SUM(CASE WHEN r.AttendanceStatus='Absent'  THEN 1 ELSE 0 END),
    @Late    = SUM(CASE WHEN r.AttendanceStatus='Late'    THEN 1 ELSE 0 END),
    @Excused = SUM(CASE WHEN r.AttendanceStatus='Excused' THEN 1 ELSE 0 END)
FROM AttendanceRecords r
JOIN AttendanceSessions ss ON ss.AttendanceSessionID = r.AttendanceSessionID
WHERE ss.AttendanceDate=@d AND (@y=0 OR ss.AcademicYearID=@y) AND ss.Status <> 'Cancelled';

SELECT
    TotalStudents = ISNULL(@Total,0),
    PresentToday  = ISNULL(@Present,0),
    AbsentToday   = ISNULL(@Absent,0),
    LateToday     = ISNULL(@Late,0),
    ExcusedToday  = ISNULL(@Excused,0);";
            DataRow row = ExecuteDataTable(sql, new[] { P("@y", academicYearId), P("@d", date.Date) }).Rows[0];

            int present = Convert.ToInt32(row["PresentToday"]);
            int absent = Convert.ToInt32(row["AbsentToday"]);
            int late = Convert.ToInt32(row["LateToday"]);
            int excused = Convert.ToInt32(row["ExcusedToday"]);
            decimal rate = ComputeRate(present, absent, late, excused, includeLate, excludeExcused);

            DataTable outT = new DataTable();
            outT.Columns.Add("TotalStudents", typeof(int));
            outT.Columns.Add("PresentToday", typeof(int));
            outT.Columns.Add("AbsentToday", typeof(int));
            outT.Columns.Add("LateToday", typeof(int));
            outT.Columns.Add("ExcusedToday", typeof(int));
            outT.Columns.Add("AttendanceRate", typeof(decimal));
            outT.Rows.Add(Convert.ToInt32(row["TotalStudents"]), present, absent, late, excused, rate);
            return outT.Rows[0];
        }

        /// <summary>Settings-aware attendance-rate formula. Attended = Present (+Late if included);
        /// denominator excludes Excused when configured. Returns a percentage 0-100 (decimal).</summary>
        public decimal ComputeRate(int present, int absent, int late, int excused, bool includeLate, bool excludeExcused)
        {
            decimal attended = present + (includeLate ? late : 0);
            // Late records always remain in the denominator as eligible attendance records.
            decimal denom = present + absent + late + excused - (excludeExcused ? excused : 0);
            if (denom <= 0) return 0m;
            return Math.Round(attended * 100m / denom, 2);
        }

        /// <summary>Per-day attendance rate for the trend chart (last N days up to the given date).</summary>
        public DataTable GetAttendanceTrend(int academicYearId, int days, DateTime endDate)
        {
            DataRow s = GetAttendanceSettings();
            bool includeLate = Convert.ToBoolean(s["IncludeLateAsAttended"]);
            bool excludeExcused = Convert.ToBoolean(s["ExcludeExcusedFromRate"]);

            const string sql = @"
SELECT ss.AttendanceDate,
    SUM(CASE WHEN r.AttendanceStatus='Present' THEN 1 ELSE 0 END) AS P,
    SUM(CASE WHEN r.AttendanceStatus='Absent'  THEN 1 ELSE 0 END) AS A,
    SUM(CASE WHEN r.AttendanceStatus='Late'    THEN 1 ELSE 0 END) AS L,
    SUM(CASE WHEN r.AttendanceStatus='Excused' THEN 1 ELSE 0 END) AS E
FROM AttendanceSessions ss
JOIN AttendanceRecords r ON r.AttendanceSessionID = ss.AttendanceSessionID
WHERE (@y=0 OR ss.AcademicYearID=@y) AND ss.Status <> 'Cancelled'
  AND ss.AttendanceDate BETWEEN @from AND @to
GROUP BY ss.AttendanceDate ORDER BY ss.AttendanceDate;";
            DataTable raw = ExecuteDataTable(sql, new[]
            {
                P("@y", academicYearId), P("@from", endDate.Date.AddDays(-(days - 1))), P("@to", endDate.Date)
            });

            DataTable outT = new DataTable();
            outT.Columns.Add("Day", typeof(string));
            outT.Columns.Add("Rate", typeof(decimal));
            foreach (DataRow r in raw.Rows)
            {
                int p = Convert.ToInt32(r["P"]), a = Convert.ToInt32(r["A"]), l = Convert.ToInt32(r["L"]), e = Convert.ToInt32(r["E"]);
                outT.Rows.Add(Convert.ToDateTime(r["AttendanceDate"]).ToString("ddd"),
                    ComputeRate(p, a, l, e, includeLate, excludeExcused));
            }
            return outT;
        }

        /// <summary>Per-class present/absent/late counts and rate for the given date.</summary>
        public DataTable GetAttendanceByClass(int academicYearId, DateTime date)
        {
            const string sql = @"
SELECT c.ClassID, c.ClassName,
    SUM(CASE WHEN r.AttendanceStatus='Present' THEN 1 ELSE 0 END) AS Present,
    SUM(CASE WHEN r.AttendanceStatus='Absent'  THEN 1 ELSE 0 END) AS Absent,
    SUM(CASE WHEN r.AttendanceStatus='Late'    THEN 1 ELSE 0 END) AS Late,
    SUM(CASE WHEN r.AttendanceStatus='Excused' THEN 1 ELSE 0 END) AS Excused,
    COUNT(r.AttendanceRecordID) AS Total
FROM AttendanceSessions ss
JOIN Classes c ON c.ClassID = ss.ClassID
JOIN AttendanceRecords r ON r.AttendanceSessionID = ss.AttendanceSessionID
WHERE ss.AttendanceDate=@d AND (@y=0 OR ss.AcademicYearID=@y) AND ss.Status <> 'Cancelled'
GROUP BY c.ClassID, c.ClassName ORDER BY c.ClassName;";
            return ExecuteDataTable(sql, new[] { P("@y", academicYearId), P("@d", date.Date) });
        }

        /// <summary>Today's marked students (for the overview "Today's Attendance" table).</summary>
        public DataTable GetTodayAttendance(int academicYearId, DateTime date, int? classId, int? sectionId, string search)
        {
            const string sql = @"
SELECT st.StudentID, st.FullName, st.StudentCode, st.AdmissionNo,
       r.AttendanceStatus, r.CheckInTime, r.LateMinutes, ISNULL(r.Remarks,'') AS Remarks,
       c.ClassName, sec.SectionName
FROM AttendanceSessions ss
JOIN AttendanceRecords r ON r.AttendanceSessionID = ss.AttendanceSessionID
JOIN Students st ON st.StudentID = r.StudentID
JOIN Classes c ON c.ClassID = ss.ClassID
JOIN Sections sec ON sec.SectionID = ss.SectionID
WHERE ss.AttendanceDate=@d AND (@y=0 OR ss.AcademicYearID=@y) AND ss.Status <> 'Cancelled'
  AND (@c IS NULL OR ss.ClassID=@c) AND (@sec IS NULL OR ss.SectionID=@sec)
  AND (@s='' OR st.FullName LIKE '%'+@s+'%' OR st.StudentCode LIKE '%'+@s+'%')
ORDER BY st.FullName;";
            return ExecuteDataTable(sql, new[]
            {
                P("@y", academicYearId), P("@d", date.Date),
                P("@c", (object)classId ?? DBNull.Value), P("@sec", (object)sectionId ?? DBNull.Value),
                P("@s", search ?? "")
            });
        }

        /// <summary>Recent real attendance activity derived from AttendanceSessions (no fabricated log).</summary>
        public DataTable GetRecentAttendanceActivity(int academicYearId, int top)
        {
            string sql = @"
SELECT TOP (" + (top > 0 ? top : 8) + @") ss.AttendanceDate, c.ClassName, sec.SectionName, ss.Status,
       COALESCE(u.FullName, sf.EmployeeID, '—') AS MarkedByName,
       SUM(CASE WHEN r.AttendanceStatus='Present' THEN 1 ELSE 0 END) AS Present,
       SUM(CASE WHEN r.AttendanceStatus='Absent'  THEN 1 ELSE 0 END) AS Absent,
       SUM(CASE WHEN r.AttendanceStatus='Late'    THEN 1 ELSE 0 END) AS Late
FROM AttendanceSessions ss
JOIN Classes c ON c.ClassID = ss.ClassID
JOIN Sections sec ON sec.SectionID = ss.SectionID
LEFT JOIN AttendanceRecords r ON r.AttendanceSessionID = ss.AttendanceSessionID
LEFT JOIN Users u ON u.UserID = ss.MarkedBy
LEFT JOIN Staff sf ON sf.StaffID = ss.MarkedBy
WHERE (@y=0 OR ss.AcademicYearID=@y) AND ss.Status <> 'Cancelled'
GROUP BY ss.AttendanceSessionID, ss.AttendanceDate, c.ClassName, sec.SectionName, ss.Status, u.FullName, sf.EmployeeID
ORDER BY ss.AttendanceDate DESC, ss.AttendanceSessionID DESC;";
            return ExecuteDataTable(sql, new[] { P("@y", academicYearId) });
        }

        // ================================================================
        // STAGE 2 — MARK ATTENDANCE
        // ================================================================

        /// <summary>One posted attendance row (server revalidates everything).</summary>
        public class MarkRow
        {
            public int StudentID { get; set; }
            public string Status { get; set; }        // Present / Late / Absent / Excused / (empty/Not Marked)
            public TimeSpan? CheckInTime { get; set; }
            public string Remarks { get; set; }
        }

        /// <summary>Immutable scope of an attendance session.</summary>
        public class AttendanceScope
        {
            public int AcademicYearID { get; set; }
            public int? TermID { get; set; }
            public DateTime AttendanceDate { get; set; }
            public int ClassID { get; set; }
            public int SectionID { get; set; }
            public int? SubjectID { get; set; }
            public string SessionType { get; set; }
        }

        public static readonly string[] ValidSessionTypes = { "Daily", "Morning", "Afternoon", "Subject" };
        public static readonly string[] ValidStatuses = { "Present", "Late", "Absent", "Excused" };

        /// <summary>Subjects assigned to a class (via its sections in ClassSubjectTeachers).</summary>
        public DataTable GetSubjectsForClass(int classId, int academicYearId)
        {
            const string sql = @"
SELECT DISTINCT sub.SubjectID, sub.SubjectName
FROM ClassSubjectTeachers cst
JOIN Sections sec ON sec.SectionID = cst.SectionID
JOIN Subjects sub ON sub.SubjectID = cst.SubjectID
WHERE sec.ClassID = @c AND (@y=0 OR cst.AcademicYearID=@y) AND ISNULL(cst.IsActive,1)=1 AND ISNULL(sub.IsActive,1)=1
ORDER BY sub.SubjectName";
            return ExecuteDataTable(sql, new[] { P("@c", classId), P("@y", academicYearId) });
        }

        /// <summary>Active students in the section for the given academic year.</summary>
        public DataTable GetEligibleStudents(int academicYearId, int sectionId)
        {
            const string sql = @"
SELECT st.StudentID, st.StudentCode, st.AdmissionNo, st.FullName
FROM Students st
WHERE st.SectionID=@sec AND (@y=0 OR st.AcademicYearID=@y) AND ISNULL(st.Status,'Active')='Active'
ORDER BY st.FullName";
            return ExecuteDataTable(sql, new[] { P("@sec", sectionId), P("@y", academicYearId) });
        }

        /// <summary>Existing non-cancelled session for a scope, or null. Handles nullable SubjectID.</summary>
        public DataRow GetAttendanceSessionByScope(AttendanceScope sc)
        {
            const string sql = @"
SELECT TOP 1 * FROM AttendanceSessions
WHERE AttendanceDate=@d AND AcademicYearID=@y AND ClassID=@c AND SectionID=@sec
  AND SessionType=@type AND ISNULL(SubjectID,0)=ISNULL(@subj,0) AND Status <> 'Cancelled'
ORDER BY AttendanceSessionID DESC";
            DataTable t = ExecuteDataTable(sql, new[]
            {
                P("@d", sc.AttendanceDate.Date), P("@y", sc.AcademicYearID), P("@c", sc.ClassID),
                P("@sec", sc.SectionID), P("@type", sc.SessionType), P("@subj", (object)sc.SubjectID ?? DBNull.Value)
            });
            return t.Rows.Count > 0 ? t.Rows[0] : null;
        }

        public DataRow GetAttendanceSession(int sessionId)
        {
            DataTable t = ExecuteDataTable("SELECT * FROM AttendanceSessions WHERE AttendanceSessionID=@id", new[] { P("@id", sessionId) });
            return t.Rows.Count > 0 ? t.Rows[0] : null;
        }

        public DataTable GetAttendanceRecords(int sessionId)
        {
            return ExecuteDataTable(
                "SELECT StudentID, AttendanceStatus, CheckInTime, LateMinutes, ISNULL(Remarks,'') AS Remarks FROM AttendanceRecords WHERE AttendanceSessionID=@id",
                new[] { P("@id", sessionId) });
        }

        /// <summary>Late minutes measured from the official AttendanceStartTime (never trusts the browser).</summary>
        public int CalculateLateMinutes(TimeSpan checkIn, TimeSpan startTime)
        {
            int mins = (int)Math.Round((checkIn - startTime).TotalMinutes);
            return mins > 0 ? mins : 0;
        }

        // ----- teacher assignment -----
        public int GetStaffIdForUser(int userId)
        {
            object o = ExecuteScalar("SELECT TOP 1 StaffID FROM Staff WHERE UserID=@u AND ISNULL(Status,'Active')='Active'", new[] { P("@u", userId) });
            return o == null || o == DBNull.Value ? 0 : Convert.ToInt32(o);
        }

        public bool UserIsClassTeacher(int userId, int sectionId)
        {
            int staffId = GetStaffIdForUser(userId);
            if (staffId <= 0) return false;
            return Convert.ToInt32(ExecuteScalar("SELECT COUNT(*) FROM Sections WHERE SectionID=@sec AND StaffID=@st", new[] { P("@sec", sectionId), P("@st", staffId) })) > 0;
        }

        public bool UserIsAssignedSubjectTeacher(int userId, int sectionId, int subjectId, int academicYearId)
        {
            int staffId = GetStaffIdForUser(userId);
            if (staffId <= 0) return false;
            return Convert.ToInt32(ExecuteScalar(
                "SELECT COUNT(*) FROM ClassSubjectTeachers WHERE SectionID=@sec AND SubjectID=@subj AND StaffID=@st AND (@y=0 OR AcademicYearID=@y) AND ISNULL(IsActive,1)=1",
                new[] { P("@sec", sectionId), P("@subj", subjectId), P("@st", staffId), P("@y", academicYearId) })) > 0;
        }

        /// <summary>Authoritative server-side check: can this user mark attendance for this scope?</summary>
        public bool UserCanMarkAttendance(int userId, string role, AttendanceScope sc)
        {
            if (CanManageAttendance(role)) return true;
            if (NormalizeRole(role) != "teacher") return false;

            DataRow settings = GetAttendanceSettings();
            if (!Convert.ToBoolean(settings["AllowTeachersToMark"])) return false;

            bool subjectSession = string.Equals(sc.SessionType, "Subject", StringComparison.OrdinalIgnoreCase) || sc.SubjectID.HasValue;
            if (subjectSession)
            {
                if (!sc.SubjectID.HasValue) return false;
                return UserIsAssignedSubjectTeacher(userId, sc.SectionID, sc.SubjectID.Value, sc.AcademicYearID);
            }
            return UserIsClassTeacher(userId, sc.SectionID);
        }

        /// <summary>Validate a scope server-side (dates within year/term, section↔class, subject↔class, future policy).
        /// Returns null when valid, otherwise an error message.</summary>
        public string ValidateAttendanceScope(AttendanceScope sc)
        {
            if (sc.AcademicYearID <= 0) return "Academic year is required.";
            if (sc.ClassID <= 0) return "Class is required.";
            if (sc.SectionID <= 0) return "Section is required.";
            if (Array.IndexOf(ValidSessionTypes, sc.SessionType) < 0) return "Invalid session type.";

            // Academic year bounds
            DataTable yr = ExecuteDataTable("SELECT StartDate, EndDate FROM AcademicYears WHERE AcademicYearID=@y", new[] { P("@y", sc.AcademicYearID) });
            if (yr.Rows.Count == 0) return "The academic year does not exist.";
            DateTime ys = Convert.ToDateTime(yr.Rows[0]["StartDate"]).Date, ye = Convert.ToDateTime(yr.Rows[0]["EndDate"]).Date;
            if (sc.AttendanceDate.Date < ys || sc.AttendanceDate.Date > ye) return "The attendance date falls outside the academic year.";

            // Future-date policy uses server date
            DataRow settings = GetAttendanceSettings();
            bool allowFuture = Convert.ToBoolean(settings["AllowFutureDate"]);
            if (!allowFuture && sc.AttendanceDate.Date > DateTime.Today) return "Future-date attendance is not allowed.";

            // Term belongs to year and (when used) contains the date
            if (sc.TermID.HasValue && sc.TermID.Value > 0)
            {
                DataTable tm = ExecuteDataTable("SELECT AcademicYearID, StartDate, EndDate FROM Terms WHERE TermID=@t", new[] { P("@t", sc.TermID.Value) });
                if (tm.Rows.Count == 0) return "The selected term does not exist.";
                if (Convert.ToInt32(tm.Rows[0]["AcademicYearID"]) != sc.AcademicYearID) return "The term does not belong to the academic year.";
                DateTime ts = Convert.ToDateTime(tm.Rows[0]["StartDate"]).Date, te = Convert.ToDateTime(tm.Rows[0]["EndDate"]).Date;
                if (sc.AttendanceDate.Date < ts || sc.AttendanceDate.Date > te) return "The attendance date falls outside the selected term.";
            }

            // Section belongs to class
            DataTable sec = ExecuteDataTable("SELECT ClassID FROM Sections WHERE SectionID=@sec", new[] { P("@sec", sc.SectionID) });
            if (sec.Rows.Count == 0) return "The section does not exist.";
            if (Convert.ToInt32(sec.Rows[0]["ClassID"]) != sc.ClassID) return "The section does not belong to the class.";

            // Subject rules
            bool subjectSession = string.Equals(sc.SessionType, "Subject", StringComparison.OrdinalIgnoreCase);
            if (subjectSession && !sc.SubjectID.HasValue) return "Subject attendance requires a subject.";
            if (!subjectSession && sc.SubjectID.HasValue) return "Only a Subject session may specify a subject.";
            if (sc.SubjectID.HasValue)
            {
                int ok = Convert.ToInt32(ExecuteScalar(
                    "SELECT COUNT(*) FROM ClassSubjectTeachers cst JOIN Sections s ON s.SectionID=cst.SectionID WHERE s.ClassID=@c AND cst.SubjectID=@subj AND (@y=0 OR cst.AcademicYearID=@y)",
                    new[] { P("@c", sc.ClassID), P("@subj", sc.SubjectID.Value), P("@y", sc.AcademicYearID) }));
                if (ok == 0) return "The subject is not assigned to the class.";
            }
            return null;
        }

        public DataRow GetAttendanceEntrySummary(int academicYearId, int sectionId, DateTime date)
        {
            const string sql = @"
DECLARE @Total int = (SELECT COUNT(*) FROM Students WHERE SectionID=@sec AND (@y=0 OR AcademicYearID=@y) AND ISNULL(Status,'Active')='Active');
SELECT TotalStudents=@Total;";
            return ExecuteDataTable(sql, new[] { P("@sec", sectionId), P("@y", academicYearId) }).Rows[0];
        }

        /// <summary>Save (upsert) a Draft session. Partial marking allowed; unmarked/blank rows are NOT
        /// stored as Absent — cleared rows are removed. One Serializable transaction.</summary>
        public int SaveAttendanceDraft(AttendanceScope sc, System.Collections.Generic.IList<MarkRow> rows, int userId, string role)
        {
            string scopeError = ValidateAttendanceScope(sc);
            if (scopeError != null) throw new InvalidOperationException(scopeError);
            if (!UserCanMarkAttendance(userId, role, sc)) throw new InvalidOperationException("You are not authorized to mark attendance for this scope.");

            DataRow settings = GetAttendanceSettings();
            TimeSpan startTime = (TimeSpan)settings["AttendanceStartTime"];
            bool excusedNeedsRemarks = Convert.ToBoolean(settings["ExcusedRequiresRemarks"]);

            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    int sessionId = LockOrCreateSession(cn, tx, sc, userId, requireDraft: true);

                    // eligible student set (revalidated from DB)
                    var eligible = LoadEligibleSet(cn, tx, sc);

                    foreach (MarkRow r in rows)
                    {
                        bool marked = !string.IsNullOrEmpty(r.Status) &&
                                      Array.IndexOf(ValidStatuses, r.Status) >= 0;
                        if (!eligible.Contains(r.StudentID))
                        {
                            if (marked) throw new InvalidOperationException("A submitted student does not belong to the selected section.");
                            continue;
                        }
                        if (!marked)
                        {
                            // Not Marked / cleared: remove any existing draft record, never store as Absent
                            NonQuery(cn, tx, "DELETE FROM AttendanceRecords WHERE AttendanceSessionID=@s AND StudentID=@st",
                                P("@s", sessionId), P("@st", r.StudentID));
                            continue;
                        }
                        UpsertRecord(cn, tx, sessionId, r, startTime, excusedNeedsRemarks, userId);
                    }

                    NonQuery(cn, tx, "UPDATE AttendanceSessions SET MarkedBy=@u, UpdatedAt=GETDATE() WHERE AttendanceSessionID=@s",
                        P("@u", userId), P("@s", sessionId));
                    tx.Commit();
                    return sessionId;
                }
            }
        }

        /// <summary>Submit: every eligible student must have a valid record. One Serializable transaction.</summary>
        public int SubmitAttendance(AttendanceScope sc, System.Collections.Generic.IList<MarkRow> rows, int userId, string role)
        {
            string scopeError = ValidateAttendanceScope(sc);
            if (scopeError != null) throw new InvalidOperationException(scopeError);
            if (!UserCanMarkAttendance(userId, role, sc)) throw new InvalidOperationException("You are not authorized to mark attendance for this scope.");

            DataRow settings = GetAttendanceSettings();
            TimeSpan startTime = (TimeSpan)settings["AttendanceStartTime"];
            bool excusedNeedsRemarks = Convert.ToBoolean(settings["ExcusedRequiresRemarks"]);

            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    int sessionId = LockOrCreateSession(cn, tx, sc, userId, requireDraft: true);
                    var eligible = LoadEligibleSet(cn, tx, sc);
                    var posted = new System.Collections.Generic.Dictionary<int, MarkRow>();
                    foreach (MarkRow r in rows) posted[r.StudentID] = r;

                    // every eligible student must be validly marked
                    foreach (int sid in eligible)
                    {
                        MarkRow r; posted.TryGetValue(sid, out r);
                        bool marked = r != null && !string.IsNullOrEmpty(r.Status) && Array.IndexOf(ValidStatuses, r.Status) >= 0;
                        if (!marked)
                            throw new InvalidOperationException("Attendance cannot be submitted until every student has a valid status.");
                    }

                    foreach (MarkRow r in rows)
                    {
                        if (!eligible.Contains(r.StudentID)) continue;
                        if (string.IsNullOrEmpty(r.Status) || Array.IndexOf(ValidStatuses, r.Status) < 0) continue;
                        UpsertRecord(cn, tx, sessionId, r, startTime, excusedNeedsRemarks, userId);
                    }

                    NonQuery(cn, tx, @"
UPDATE AttendanceSessions SET Status='Submitted', MarkedBy=ISNULL(MarkedBy,@u), SubmittedBy=@u, SubmittedAt=GETDATE(), UpdatedAt=GETDATE()
WHERE AttendanceSessionID=@s", P("@u", userId), P("@s", sessionId));
                    tx.Commit();
                    return sessionId;
                }
            }
        }

        /// <summary>Reopen a Submitted session back to Draft (managers only). Records preserved.</summary>
        public void ReopenAttendance(int sessionId, string reason, int userId, string role)
        {
            if (!CanManageAttendance(role)) throw new InvalidOperationException("You are not authorized to reopen attendance.");
            if (string.IsNullOrWhiteSpace(reason)) throw new InvalidOperationException("A reason is required to reopen attendance.");

            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    string status = Convert.ToString(Scalar(cn, tx, "SELECT Status FROM AttendanceSessions WITH (UPDLOCK,HOLDLOCK) WHERE AttendanceSessionID=@s", P("@s", sessionId)));
                    if (string.IsNullOrEmpty(status)) throw new InvalidOperationException("The attendance session does not exist.");
                    if (status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("A cancelled session cannot be reopened.");
                    if (!status.Equals("Submitted", StringComparison.OrdinalIgnoreCase) && !status.Equals("Locked", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Only a submitted session can be reopened.");

                    // Preserve SubmittedBy/SubmittedAt as history; record reopen audit.
                    NonQuery(cn, tx, "UPDATE AttendanceSessions SET Status='Draft', ReopenedBy=@u, ReopenedAt=GETDATE(), ReopenReason=@r, UpdatedAt=GETDATE() WHERE AttendanceSessionID=@s",
                        P("@u", userId), P("@r", reason.Trim()), P("@s", sessionId));
                    tx.Commit();
                }
            }
        }

        // ----- transaction-scoped internals -----
        private int LockOrCreateSession(SqlConnection cn, SqlTransaction tx, AttendanceScope sc, int userId, bool requireDraft)
        {
            object idObj = Scalar(cn, tx, @"
SELECT TOP 1 AttendanceSessionID, Status FROM AttendanceSessions WITH (UPDLOCK,HOLDLOCK)
WHERE AttendanceDate=@d AND AcademicYearID=@y AND ClassID=@c AND SectionID=@sec
  AND SessionType=@type AND ISNULL(SubjectID,0)=ISNULL(@subj,0) AND Status <> 'Cancelled'
ORDER BY AttendanceSessionID DESC",
                P("@d", sc.AttendanceDate.Date), P("@y", sc.AcademicYearID), P("@c", sc.ClassID),
                P("@sec", sc.SectionID), P("@type", sc.SessionType), P("@subj", (object)sc.SubjectID ?? DBNull.Value));

            if (idObj != null && idObj != DBNull.Value)
            {
                int existingId = Convert.ToInt32(idObj);
                string status = Convert.ToString(Scalar(cn, tx, "SELECT Status FROM AttendanceSessions WHERE AttendanceSessionID=@s", P("@s", existingId)));
                if (requireDraft && !status.Equals("Draft", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("This attendance session is " + status + " and is locked. Reopen it to make changes.");
                return existingId;
            }

            return Convert.ToInt32(Scalar(cn, tx, @"
INSERT INTO AttendanceSessions (AcademicYearID, TermID, AttendanceDate, ClassID, SectionID, SubjectID, SessionType, Status, MarkedBy, CreatedAt)
VALUES (@y, @tm, @d, @c, @sec, @subj, @type, 'Draft', @u, GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS int)",
                P("@y", sc.AcademicYearID), P("@tm", (object)sc.TermID ?? DBNull.Value), P("@d", sc.AttendanceDate.Date),
                P("@c", sc.ClassID), P("@sec", sc.SectionID), P("@subj", (object)sc.SubjectID ?? DBNull.Value),
                P("@type", sc.SessionType), P("@u", userId)));
        }

        private System.Collections.Generic.HashSet<int> LoadEligibleSet(SqlConnection cn, SqlTransaction tx, AttendanceScope sc)
        {
            var set = new System.Collections.Generic.HashSet<int>();
            using (SqlCommand c = new SqlCommand("SELECT StudentID FROM Students WHERE SectionID=@sec AND (@y=0 OR AcademicYearID=@y) AND ISNULL(Status,'Active')='Active'", cn, tx))
            {
                c.Parameters.Add(P("@sec", sc.SectionID));
                c.Parameters.Add(P("@y", sc.AcademicYearID));
                using (SqlDataReader rd = c.ExecuteReader()) while (rd.Read()) set.Add(rd.GetInt32(0));
            }
            return set;
        }

        private void UpsertRecord(SqlConnection cn, SqlTransaction tx, int sessionId, MarkRow r, TimeSpan startTime, bool excusedNeedsRemarks, int userId)
        {
            string status = r.Status;
            TimeSpan? checkIn = r.CheckInTime;
            int? lateMinutes = null;
            string remarks = string.IsNullOrWhiteSpace(r.Remarks) ? null : r.Remarks.Trim();

            if (status.Equals("Late", StringComparison.OrdinalIgnoreCase))
            {
                if (!checkIn.HasValue) throw new InvalidOperationException("A late student requires a check-in time.");
                lateMinutes = CalculateLateMinutes(checkIn.Value, startTime);   // server-side, browser value ignored
            }
            else if (status.Equals("Present", StringComparison.OrdinalIgnoreCase))
            {
                lateMinutes = 0;
            }
            else // Absent / Excused
            {
                checkIn = null;
                lateMinutes = null;
                if (status.Equals("Excused", StringComparison.OrdinalIgnoreCase) && excusedNeedsRemarks && remarks == null)
                    throw new InvalidOperationException("Excused attendance requires a remark.");
            }

            int updated = Convert.ToInt32(Scalar(cn, tx, @"
UPDATE AttendanceRecords SET AttendanceStatus=@st, CheckInTime=@ci, LateMinutes=@lm, Remarks=@rem, RecordedBy=@u, UpdatedAt=GETDATE()
WHERE AttendanceSessionID=@s AND StudentID=@sid; SELECT @@ROWCOUNT",
                P("@st", status), P("@ci", (object)checkIn ?? DBNull.Value), P("@lm", (object)lateMinutes ?? DBNull.Value),
                P("@rem", (object)remarks ?? DBNull.Value), P("@u", userId), P("@s", sessionId), P("@sid", r.StudentID)));

            if (updated == 0)
                NonQuery(cn, tx, @"
INSERT INTO AttendanceRecords (AttendanceSessionID, StudentID, AttendanceStatus, CheckInTime, LateMinutes, Remarks, RecordedBy, CreatedAt)
VALUES (@s,@sid,@st,@ci,@lm,@rem,@u,GETDATE())",
                    P("@s", sessionId), P("@sid", r.StudentID), P("@st", status), P("@ci", (object)checkIn ?? DBNull.Value),
                    P("@lm", (object)lateMinutes ?? DBNull.Value), P("@rem", (object)remarks ?? DBNull.Value), P("@u", userId));
        }

        // ================================================================
        // STAGE 3 — REPORTS, BY DATE, CALENDAR
        // ================================================================

        /// <summary>Settings-aware rate. attended = Present (+Late if included); denominator excludes Excused when configured.</summary>
        public decimal CalculateAttendanceRate(int present, int absent, int late, int excused)
        {
            DataRow s = GetAttendanceSettings();
            return ComputeRate(present, absent, late, excused,
                Convert.ToBoolean(s["IncludeLateAsAttended"]), Convert.ToBoolean(s["ExcludeExcusedFromRate"]));
        }

        /// <summary>Risk band from the configured LowAttendanceThreshold. At Risk &lt; threshold;
        /// Watch in [threshold, threshold+5); Good &gt;= threshold+5.</summary>
        public string GetRiskStatus(decimal rate, decimal threshold)
        {
            if (rate < threshold) return "At Risk";
            if (rate < threshold + 5m) return "Watch";
            return "Good";
        }

        public decimal LowAttendanceThreshold()
        {
            return Convert.ToDecimal(GetAttendanceSettings()["LowAttendanceThreshold"]);
        }

        /// <summary>Can this user view attendance for the scope? Management/registrar: yes.
        /// Teacher: only assigned section (class teacher) or assigned subject.</summary>
        public bool UserCanViewAttendanceScope(int userId, string role, int classId, int sectionId, int? subjectId, int academicYearId)
        {
            if (CanManageAttendance(role)) return true;
            string r = NormalizeRole(role);
            if (r == "registrar") return true;
            if (r == "teacher")
            {
                if (subjectId.HasValue && subjectId.Value > 0)
                    return UserIsAssignedSubjectTeacher(userId, sectionId, subjectId.Value, academicYearId);
                return UserIsClassTeacher(userId, sectionId);
            }
            return false;
        }

        private static string StatusFilterSql(string sessionStatus, bool managerViewingDraft)
        {
            // Official reports use Submitted/Locked only. Managers may explicitly view a Draft.
            if (string.Equals(sessionStatus, "Draft", StringComparison.OrdinalIgnoreCase) && managerViewingDraft)
                return "ss.Status='Draft'";
            if (!string.IsNullOrEmpty(sessionStatus) && (sessionStatus.Equals("Submitted", StringComparison.OrdinalIgnoreCase) || sessionStatus.Equals("Locked", StringComparison.OrdinalIgnoreCase)))
                return "ss.Status=@onestatus";
            return "ss.Status IN ('Submitted','Locked')";
        }

        /// <summary>Rows for a single date's session scope. sessionStatus optional (managers may request Draft).</summary>
        public DataTable GetAttendanceByDate(int academicYearId, DateTime date, int classId, int sectionId, int? subjectId, string sessionType, string sessionStatus, bool managerViewingDraft)
        {
            string statusSql = StatusFilterSql(sessionStatus, managerViewingDraft);
            string sql = @"
SELECT st.StudentID, st.FullName, st.StudentCode, st.AdmissionNo,
       r.AttendanceStatus, r.CheckInTime, r.LateMinutes, ISNULL(r.Remarks,'') AS Remarks,
       ss.Status AS SessionStatus, ss.AttendanceSessionID,
       COALESCE(u.FullName, sf.EmployeeID, '—') AS MarkedByName
FROM AttendanceSessions ss
JOIN AttendanceRecords r ON r.AttendanceSessionID = ss.AttendanceSessionID
JOIN Students st ON st.StudentID = r.StudentID
LEFT JOIN Users u ON u.UserID = ss.MarkedBy
LEFT JOIN Staff sf ON sf.StaffID = ss.MarkedBy
WHERE ss.AttendanceDate=@d AND ss.AcademicYearID=@y AND ss.ClassID=@c AND ss.SectionID=@sec
  AND ss.SessionType=@type AND ISNULL(ss.SubjectID,0)=ISNULL(@subj,0) AND " + statusSql + @"
ORDER BY st.FullName";
            return ExecuteDataTable(sql, new[]
            {
                P("@d", date.Date), P("@y", academicYearId), P("@c", classId), P("@sec", sectionId),
                P("@type", sessionType), P("@subj", (object)subjectId ?? DBNull.Value), P("@onestatus", sessionStatus ?? "")
            });
        }

        public DataRow GetAttendanceByDateSummary(int academicYearId, DateTime date, int classId, int sectionId, int? subjectId, string sessionType, string sessionStatus, bool managerViewingDraft)
        {
            string statusSql = StatusFilterSql(sessionStatus, managerViewingDraft);
            string sql = @"
SELECT
  (SELECT COUNT(*) FROM Students WHERE SectionID=@sec AND AcademicYearID=@y AND ISNULL(Status,'Active')='Active') AS TotalStudents,
  SUM(CASE WHEN r.AttendanceStatus='Present' THEN 1 ELSE 0 END) AS P,
  SUM(CASE WHEN r.AttendanceStatus='Absent'  THEN 1 ELSE 0 END) AS A,
  SUM(CASE WHEN r.AttendanceStatus='Late'    THEN 1 ELSE 0 END) AS L,
  SUM(CASE WHEN r.AttendanceStatus='Excused' THEN 1 ELSE 0 END) AS E
FROM AttendanceSessions ss
LEFT JOIN AttendanceRecords r ON r.AttendanceSessionID = ss.AttendanceSessionID
WHERE ss.AttendanceDate=@d AND ss.AcademicYearID=@y AND ss.ClassID=@c AND ss.SectionID=@sec
  AND ss.SessionType=@type AND ISNULL(ss.SubjectID,0)=ISNULL(@subj,0) AND " + statusSql;
            DataRow row = ExecuteDataTable(sql, new[]
            {
                P("@d", date.Date), P("@y", academicYearId), P("@c", classId), P("@sec", sectionId),
                P("@type", sessionType), P("@subj", (object)subjectId ?? DBNull.Value), P("@onestatus", sessionStatus ?? "")
            }).Rows[0];

            int p = row["P"] == DBNull.Value ? 0 : Convert.ToInt32(row["P"]);
            int a = row["A"] == DBNull.Value ? 0 : Convert.ToInt32(row["A"]);
            int l = row["L"] == DBNull.Value ? 0 : Convert.ToInt32(row["L"]);
            int e = row["E"] == DBNull.Value ? 0 : Convert.ToInt32(row["E"]);
            DataTable outT = new DataTable();
            outT.Columns.Add("TotalStudents", typeof(int)); outT.Columns.Add("Present", typeof(int));
            outT.Columns.Add("Absent", typeof(int)); outT.Columns.Add("Late", typeof(int));
            outT.Columns.Add("Excused", typeof(int)); outT.Columns.Add("Rate", typeof(decimal));
            outT.Rows.Add(Convert.ToInt32(row["TotalStudents"]), p, a, l, e, CalculateAttendanceRate(p, a, l, e));
            return outT.Rows[0];
        }

        // ----- student report (historical: joins through AttendanceSessions scope, not current SectionID) -----
        public DataTable GetStudentAttendanceReport(int studentId, int academicYearId, int? termId, DateTime from, DateTime to, string sessionType, int? subjectId)
        {
            const string sql = @"
SELECT ss.AttendanceDate, ss.AcademicYearID, y.YearName, c.ClassName, sec.SectionName,
       ISNULL(sub.SubjectName,'—') AS SubjectName, ss.SessionType, r.AttendanceStatus,
       r.CheckInTime, r.LateMinutes, ISNULL(r.Remarks,'') AS Remarks,
       COALESCE(u.FullName, sf.EmployeeID, '—') AS MarkedByName
FROM AttendanceRecords r
JOIN AttendanceSessions ss ON ss.AttendanceSessionID = r.AttendanceSessionID
JOIN Classes c ON c.ClassID = ss.ClassID
JOIN Sections sec ON sec.SectionID = ss.SectionID
LEFT JOIN AcademicYears y ON y.AcademicYearID = ss.AcademicYearID
LEFT JOIN Subjects sub ON sub.SubjectID = ss.SubjectID
LEFT JOIN Users u ON u.UserID = ss.MarkedBy
LEFT JOIN Staff sf ON sf.StaffID = ss.MarkedBy
WHERE r.StudentID=@st AND ss.Status IN ('Submitted','Locked')
  AND (@y=0 OR ss.AcademicYearID=@y) AND (@tm IS NULL OR ss.TermID=@tm)
  AND ss.AttendanceDate BETWEEN @from AND @to
  AND (@type='' OR ss.SessionType=@type) AND (@subj IS NULL OR ss.SubjectID=@subj)
ORDER BY ss.AttendanceDate DESC";
            return ExecuteDataTable(sql, new[]
            {
                P("@st", studentId), P("@y", academicYearId), P("@tm", (object)termId ?? DBNull.Value),
                P("@from", from.Date), P("@to", to.Date), P("@type", sessionType ?? ""), P("@subj", (object)subjectId ?? DBNull.Value)
            });
        }

        public DataRow GetStudentAttendanceSummary(int studentId, int academicYearId, int? termId, DateTime from, DateTime to, string sessionType, int? subjectId)
        {
            const string sql = @"
SELECT
  SUM(CASE WHEN r.AttendanceStatus='Present' THEN 1 ELSE 0 END) AS P,
  SUM(CASE WHEN r.AttendanceStatus='Absent'  THEN 1 ELSE 0 END) AS A,
  SUM(CASE WHEN r.AttendanceStatus='Late'    THEN 1 ELSE 0 END) AS L,
  SUM(CASE WHEN r.AttendanceStatus='Excused' THEN 1 ELSE 0 END) AS E,
  COUNT(*) AS Total
FROM AttendanceRecords r
JOIN AttendanceSessions ss ON ss.AttendanceSessionID = r.AttendanceSessionID
WHERE r.StudentID=@st AND ss.Status IN ('Submitted','Locked')
  AND (@y=0 OR ss.AcademicYearID=@y) AND (@tm IS NULL OR ss.TermID=@tm)
  AND ss.AttendanceDate BETWEEN @from AND @to
  AND (@type='' OR ss.SessionType=@type) AND (@subj IS NULL OR ss.SubjectID=@subj)";
            DataRow row = ExecuteDataTable(sql, new[]
            {
                P("@st", studentId), P("@y", academicYearId), P("@tm", (object)termId ?? DBNull.Value),
                P("@from", from.Date), P("@to", to.Date), P("@type", sessionType ?? ""), P("@subj", (object)subjectId ?? DBNull.Value)
            }).Rows[0];
            int p = N(row["P"]), a = N(row["A"]), l = N(row["L"]), e = N(row["E"]), total = N(row["Total"]);
            DataTable outT = new DataTable();
            foreach (string cn in new[] { "TotalSessions", "Present", "Absent", "Late", "Excused" }) outT.Columns.Add(cn, typeof(int));
            outT.Columns.Add("Percentage", typeof(decimal));
            outT.Rows.Add(total, p, a, l, e, CalculateAttendanceRate(p, a, l, e));
            return outT.Rows[0];
        }

        // ----- class report (historical: aggregates AttendanceRecords by session scope) -----
        public DataTable GetClassAttendanceReport(int academicYearId, int? termId, int classId, int sectionId, DateTime from, DateTime to, string sessionType, int? subjectId)
        {
            const string sql = @"
SELECT st.StudentID, st.FullName, st.StudentCode,
  SUM(CASE WHEN r.AttendanceStatus='Present' THEN 1 ELSE 0 END) AS P,
  SUM(CASE WHEN r.AttendanceStatus='Absent'  THEN 1 ELSE 0 END) AS A,
  SUM(CASE WHEN r.AttendanceStatus='Late'    THEN 1 ELSE 0 END) AS L,
  SUM(CASE WHEN r.AttendanceStatus='Excused' THEN 1 ELSE 0 END) AS E,
  COUNT(*) AS Total
FROM AttendanceRecords r
JOIN AttendanceSessions ss ON ss.AttendanceSessionID = r.AttendanceSessionID
JOIN Students st ON st.StudentID = r.StudentID
WHERE ss.Status IN ('Submitted','Locked') AND ss.ClassID=@c AND ss.SectionID=@sec
  AND (@y=0 OR ss.AcademicYearID=@y) AND (@tm IS NULL OR ss.TermID=@tm)
  AND ss.AttendanceDate BETWEEN @from AND @to
  AND (@type='' OR ss.SessionType=@type) AND (@subj IS NULL OR ss.SubjectID=@subj)
GROUP BY st.StudentID, st.FullName, st.StudentCode
ORDER BY st.FullName";
            DataTable raw = ExecuteDataTable(sql, new[]
            {
                P("@c", classId), P("@sec", sectionId), P("@y", academicYearId), P("@tm", (object)termId ?? DBNull.Value),
                P("@from", from.Date), P("@to", to.Date), P("@type", sessionType ?? ""), P("@subj", (object)subjectId ?? DBNull.Value)
            });

            decimal threshold = LowAttendanceThreshold();
            DataTable outT = new DataTable();
            outT.Columns.Add("StudentID", typeof(int)); outT.Columns.Add("FullName", typeof(string)); outT.Columns.Add("StudentCode", typeof(string));
            foreach (string cn in new[] { "TotalSessions", "Present", "Absent", "Late", "Excused" }) outT.Columns.Add(cn, typeof(int));
            outT.Columns.Add("Percentage", typeof(decimal)); outT.Columns.Add("Risk", typeof(string));
            foreach (DataRow r in raw.Rows)
            {
                int p = N(r["P"]), a = N(r["A"]), l = N(r["L"]), e = N(r["E"]), total = N(r["Total"]);
                decimal rate = CalculateAttendanceRate(p, a, l, e);
                outT.Rows.Add(Convert.ToInt32(r["StudentID"]), Convert.ToString(r["FullName"]), Convert.ToString(r["StudentCode"]),
                    total, p, a, l, e, rate, GetRiskStatus(rate, threshold));
            }
            return outT;
        }

        public DataRow GetClassAttendanceSummary(int academicYearId, int? termId, int classId, int sectionId, DateTime from, DateTime to, string sessionType, int? subjectId)
        {
            const string sql = @"
SELECT
  (SELECT COUNT(*) FROM AttendanceSessions ss WHERE ss.Status IN ('Submitted','Locked') AND ss.ClassID=@c AND ss.SectionID=@sec
     AND (@y=0 OR ss.AcademicYearID=@y) AND (@tm IS NULL OR ss.TermID=@tm) AND ss.AttendanceDate BETWEEN @from AND @to
     AND (@type='' OR ss.SessionType=@type) AND (@subj IS NULL OR ss.SubjectID=@subj)) AS Sessions,
  SUM(CASE WHEN r.AttendanceStatus='Present' THEN 1 ELSE 0 END) AS P,
  SUM(CASE WHEN r.AttendanceStatus='Absent'  THEN 1 ELSE 0 END) AS A,
  SUM(CASE WHEN r.AttendanceStatus='Late'    THEN 1 ELSE 0 END) AS L,
  SUM(CASE WHEN r.AttendanceStatus='Excused' THEN 1 ELSE 0 END) AS E
FROM AttendanceSessions ss
LEFT JOIN AttendanceRecords r ON r.AttendanceSessionID = ss.AttendanceSessionID
WHERE ss.Status IN ('Submitted','Locked') AND ss.ClassID=@c AND ss.SectionID=@sec
  AND (@y=0 OR ss.AcademicYearID=@y) AND (@tm IS NULL OR ss.TermID=@tm)
  AND ss.AttendanceDate BETWEEN @from AND @to
  AND (@type='' OR ss.SessionType=@type) AND (@subj IS NULL OR ss.SubjectID=@subj)";
            DataRow row = ExecuteDataTable(sql, new[]
            {
                P("@c", classId), P("@sec", sectionId), P("@y", academicYearId), P("@tm", (object)termId ?? DBNull.Value),
                P("@from", from.Date), P("@to", to.Date), P("@type", sessionType ?? ""), P("@subj", (object)subjectId ?? DBNull.Value)
            }).Rows[0];
            int p = N(row["P"]), a = N(row["A"]), l = N(row["L"]), e = N(row["E"]);
            DataTable outT = new DataTable();
            outT.Columns.Add("Sessions", typeof(int)); outT.Columns.Add("Present", typeof(int)); outT.Columns.Add("Absent", typeof(int));
            outT.Columns.Add("Late", typeof(int)); outT.Columns.Add("Excused", typeof(int)); outT.Columns.Add("AverageRate", typeof(decimal));
            outT.Rows.Add(Convert.ToInt32(row["Sessions"]), p, a, l, e, CalculateAttendanceRate(p, a, l, e));
            return outT.Rows[0];
        }

        // ----- calendar -----
        /// <summary>Per-day aggregate for a month. Student mode (studentId) returns that student's daily status;
        /// class mode returns aggregate counts per day. Submitted/Locked only.</summary>
        public DataTable GetAttendanceCalendar(int academicYearId, int year, int month, int classId, int sectionId, int? studentId, string sessionType)
        {
            DateTime from = new DateTime(year, month, 1);
            DateTime to = from.AddMonths(1).AddDays(-1);
            string studentFilter = studentId.HasValue && studentId.Value > 0 ? " AND r.StudentID=@st" : "";
            string sql = @"
SELECT ss.AttendanceDate,
  SUM(CASE WHEN r.AttendanceStatus='Present' THEN 1 ELSE 0 END) AS P,
  SUM(CASE WHEN r.AttendanceStatus='Absent'  THEN 1 ELSE 0 END) AS A,
  SUM(CASE WHEN r.AttendanceStatus='Late'    THEN 1 ELSE 0 END) AS L,
  SUM(CASE WHEN r.AttendanceStatus='Excused' THEN 1 ELSE 0 END) AS E,
  COUNT(*) AS Total
FROM AttendanceSessions ss
JOIN AttendanceRecords r ON r.AttendanceSessionID = ss.AttendanceSessionID
WHERE ss.Status IN ('Submitted','Locked') AND ss.ClassID=@c AND ss.SectionID=@sec
  AND (@y=0 OR ss.AcademicYearID=@y) AND ss.AttendanceDate BETWEEN @from AND @to
  AND (@type='' OR ss.SessionType=@type)" + studentFilter + @"
GROUP BY ss.AttendanceDate ORDER BY ss.AttendanceDate";
            return ExecuteDataTable(sql, new[]
            {
                P("@c", classId), P("@sec", sectionId), P("@y", academicYearId),
                P("@from", from), P("@to", to), P("@type", sessionType ?? ""), P("@st", (object)studentId ?? DBNull.Value)
            });
        }

        public DataTable GetStudentsForSectionLookup(int academicYearId, int sectionId)
        {
            return GetEligibleStudents(academicYearId, sectionId);
        }

        /// <summary>Current class/section/name for a student (for prefill + lookups).</summary>
        public DataTable GetStudentBasic(int studentId)
        {
            const string sql = @"
SELECT st.StudentID, st.FullName, st.StudentCode, st.SectionID, sec.ClassID
FROM Students st JOIN Sections sec ON sec.SectionID = st.SectionID
WHERE st.StudentID = @st";
            return ExecuteDataTable(sql, new[] { P("@st", studentId) });
        }

        private static int N(object o) { return o == null || o == DBNull.Value ? 0 : Convert.ToInt32(o); }

        // ================================================================
        // STAGE 4 — IMPORT ATTENDANCE
        // ================================================================

        public class ImportOptions
        {
            public int AcademicYearID { get; set; }
            public int? TermID { get; set; }
            public int ClassID { get; set; }
            public int SectionID { get; set; }
            public int? SubjectID { get; set; }
            public string SessionType { get; set; }
            public bool UseFixedDate { get; set; }
            public DateTime FixedDate { get; set; }
            public bool ImportAsSubmitted { get; set; }
            public bool UpdateExistingDraft { get; set; }
        }

        public class ImportRowResult
        {
            public int RowNumber { get; set; }
            public string AttendanceDate { get; set; }
            public string StudentCode { get; set; }
            public string StudentName { get; set; }
            public string Status { get; set; }
            public string CheckInTime { get; set; }
            public string Remarks { get; set; }
            public string Validation { get; set; }   // Valid / Warning / Error
            public string Message { get; set; }
            // resolved (not shown directly)
            public int StudentID { get; set; }
            public DateTime Date { get; set; }
            public TimeSpan? CheckIn { get; set; }
            public bool Importable { get { return Validation != "Error"; } }
        }

        public class ImportPreview
        {
            public System.Collections.Generic.List<ImportRowResult> Rows = new System.Collections.Generic.List<ImportRowResult>();
            public int Total, Valid, Warning, Error, SessionsToCreate, SessionsToUpdate;
            public bool CanImport;
            public string HeaderError;
        }

        /// <summary>Only management roles may bulk-import (Stage 4 policy: teacher/registrar/parent/student cannot).</summary>
        public bool UserCanImportAttendance(string role) { return CanManageAttendance(role); }

        public DataTable GetStudentByCodeForImport(string code)
        {
            const string sql = @"
SELECT st.StudentID, st.FullName, st.StudentCode, st.SectionID, st.AcademicYearID, ISNULL(st.Status,'Active') AS Status
FROM Students st WHERE st.StudentCode=@c";
            return ExecuteDataTable(sql, new[] { P("@c", code ?? "") });
        }

        public DataTable GetSubjectByCodeForImport(string code)
        {
            return ExecuteDataTable("SELECT SubjectID, SubjectName FROM Subjects WHERE SubjectCode=@c AND ISNULL(IsActive,1)=1", new[] { P("@c", code ?? "") });
        }

        public DataRow GetExistingSessionForImport(int academicYearId, DateTime date, int classId, int sectionId, int? subjectId, string sessionType)
        {
            return GetAttendanceSessionByScope(new AttendanceScope
            { AcademicYearID = academicYearId, AttendanceDate = date, ClassID = classId, SectionID = sectionId, SubjectID = subjectId, SessionType = sessionType });
        }

        public bool AttendanceImportHashExists(string fileHash, int academicYearId, int classId, int sectionId)
        {
            if (string.IsNullOrEmpty(fileHash)) return false;
            return Convert.ToInt32(ExecuteScalar(
                "SELECT COUNT(*) FROM AttendanceImportBatches WHERE FileHash=@h AND AcademicYearID=@y AND ClassID=@c AND SectionID=@sec AND ImportStatus<>'Failed'",
                new[] { P("@h", fileHash), P("@y", academicYearId), P("@c", classId), P("@sec", sectionId) })) > 0;
        }

        /// <summary>Header index map (case-insensitive). Returns null header names as -1.</summary>
        private static System.Collections.Generic.Dictionary<string, int> HeaderMap(string[] header)
        {
            var map = new System.Collections.Generic.Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < header.Length; i++)
            {
                string h = (header[i] ?? "").Trim();
                if (h.Length == 0) continue;
                if (!map.ContainsKey(h)) map[h] = i;   // first wins; duplicates detected separately
            }
            return map;
        }

        /// <summary>Validate the file scope + build a per-row validation preview (no DB writes).</summary>
        public ImportPreview GetAttendanceImportPreview(System.Collections.Generic.List<string[]> csv, ImportOptions opt, int userId, string role)
        {
            var pv = new ImportPreview();
            if (!UserCanImportAttendance(role)) { pv.HeaderError = "You are not authorized to import attendance."; return pv; }
            if (csv == null || csv.Count == 0) { pv.HeaderError = "The uploaded file is empty."; return pv; }

            // scope-level validation (uses a representative date for bounds when fixed)
            var scope0 = new AttendanceScope
            { AcademicYearID = opt.AcademicYearID, TermID = opt.TermID, AttendanceDate = opt.UseFixedDate ? opt.FixedDate : DateTime.Today, ClassID = opt.ClassID, SectionID = opt.SectionID, SubjectID = opt.SubjectID, SessionType = opt.SessionType };
            // validate class/section/subject relationships (skip date check here; per-row dates checked below)
            string scopeErr = ValidateScopeStructureOnly(scope0);
            if (scopeErr != null) { pv.HeaderError = scopeErr; return pv; }
            if (!UserCanMarkAttendance(userId, role, scope0)) { pv.HeaderError = "You are not authorized to import attendance for this class/section/subject."; return pv; }

            string[] header = csv[0];
            // duplicate / empty headers
            var seen = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string h in header)
            {
                string hn = (h ?? "").Trim();
                if (hn.Length == 0) continue;
                if (!seen.Add(hn)) { pv.HeaderError = "Duplicate header: " + hn; return pv; }
            }
            var map = HeaderMap(header);
            if (!map.ContainsKey("StudentCode")) { pv.HeaderError = "Required header StudentCode is missing."; return pv; }
            if (!map.ContainsKey("Status")) { pv.HeaderError = "Required header Status is missing."; return pv; }
            if (!opt.UseFixedDate && !map.ContainsKey("AttendanceDate")) { pv.HeaderError = "Required header AttendanceDate is missing (or select a fixed date)."; return pv; }

            DataRow settings = GetAttendanceSettings();
            TimeSpan startTime = (TimeSpan)settings["AttendanceStartTime"];
            bool excusedNeedsRemarks = Convert.ToBoolean(settings["ExcusedRequiresRemarks"]);

            // per-scope tracking of duplicate student codes in the file
            var scopeStudents = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.HashSet<string>>();
            var scopeKeysCreate = new System.Collections.Generic.HashSet<string>();
            var scopeKeysUpdate = new System.Collections.Generic.HashSet<string>();

            for (int rIdx = 1; rIdx < csv.Count; rIdx++)
            {
                string[] cells = csv[rIdx];
                var rr = new ImportRowResult { RowNumber = rIdx + 1, Validation = "Valid" };
                Func<string, string> col = key => map.ContainsKey(key) && map[key] < cells.Length ? (cells[map[key]] ?? "").Trim() : "";

                rr.StudentCode = col("StudentCode");
                rr.Status = col("Status");
                rr.CheckInTime = col("CheckInTime");
                rr.Remarks = col("Remarks");

                // date
                DateTime date;
                if (opt.UseFixedDate) { date = opt.FixedDate.Date; rr.AttendanceDate = date.ToString("yyyy-MM-dd"); }
                else
                {
                    string dRaw = col("AttendanceDate"); rr.AttendanceDate = dRaw;
                    if (!DateTime.TryParse(dRaw, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
                    { Err(rr, "Invalid attendance date."); AddRow(pv, rr); continue; }
                }
                rr.Date = date;

                // date within year/term + future policy
                var rowScope = new AttendanceScope { AcademicYearID = opt.AcademicYearID, TermID = opt.TermID, AttendanceDate = date, ClassID = opt.ClassID, SectionID = opt.SectionID, SubjectID = opt.SubjectID, SessionType = opt.SessionType };
                string dateErr = ValidateAttendanceScope(rowScope);
                if (dateErr != null) { Err(rr, dateErr); AddRow(pv, rr); continue; }

                // student
                if (rr.StudentCode.Length == 0) { Err(rr, "Student code is required."); AddRow(pv, rr); continue; }
                DataTable st = GetStudentByCodeForImport(rr.StudentCode);
                if (st.Rows.Count == 0) { Err(rr, "Student code was not found."); AddRow(pv, rr); continue; }
                DataRow sr = st.Rows[0];
                rr.StudentID = Convert.ToInt32(sr["StudentID"]); rr.StudentName = Convert.ToString(sr["FullName"]);
                if (!Convert.ToString(sr["Status"]).Equals("Active", StringComparison.OrdinalIgnoreCase)) { Err(rr, "The student is not active."); AddRow(pv, rr); continue; }
                if (Convert.ToInt32(sr["AcademicYearID"]) != opt.AcademicYearID) { Err(rr, "The student is not in the selected academic year."); AddRow(pv, rr); continue; }
                if (Convert.ToInt32(sr["SectionID"]) != opt.SectionID) { Err(rr, "The student does not belong to the selected section."); AddRow(pv, rr); continue; }

                // status
                string status = NormalizeStatus(rr.Status);
                if (status == null) { Err(rr, "Invalid status '" + rr.Status + "'."); AddRow(pv, rr); continue; }
                rr.Status = status;

                // check-in / late / excused
                TimeSpan ci;
                bool hasCi = TimeSpan.TryParse(rr.CheckInTime, System.Globalization.CultureInfo.InvariantCulture, out ci);
                if (rr.CheckInTime.Length > 0 && !hasCi) { Err(rr, "Invalid check-in time."); AddRow(pv, rr); continue; }
                if (status == "Late" && !hasCi) { Err(rr, "Late attendance requires a valid check-in time."); AddRow(pv, rr); continue; }
                if (status == "Late") { rr.CheckIn = ci; rr.CheckInTime = ci.ToString(@"hh\:mm"); }
                else if (status == "Present" && hasCi) { rr.CheckIn = ci; rr.CheckInTime = ci.ToString(@"hh\:mm"); }
                else { rr.CheckIn = null; rr.CheckInTime = ""; }
                if (status == "Excused" && excusedNeedsRemarks && rr.Remarks.Length == 0) { Err(rr, "Excused attendance requires a remark."); AddRow(pv, rr); continue; }

                // duplicate student within same scope in the file
                string scopeKey = date.ToString("yyyyMMdd") + "|" + opt.SessionType + "|" + (opt.SubjectID ?? 0);
                if (!scopeStudents.ContainsKey(scopeKey)) scopeStudents[scopeKey] = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (!scopeStudents[scopeKey].Add(rr.StudentCode)) { Err(rr, "Duplicate student in the same attendance scope."); AddRow(pv, rr); continue; }

                // existing session conflict
                DataRow existing = GetExistingSessionForImport(opt.AcademicYearID, date, opt.ClassID, opt.SectionID, opt.SubjectID, opt.SessionType);
                if (existing != null)
                {
                    string exStatus = Convert.ToString(existing["Status"]);
                    if (exStatus.Equals("Submitted", StringComparison.OrdinalIgnoreCase) || exStatus.Equals("Locked", StringComparison.OrdinalIgnoreCase))
                    { Err(rr, "A " + exStatus.ToLowerInvariant() + " attendance session already exists for this scope."); AddRow(pv, rr); continue; }
                    if (exStatus.Equals("Draft", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!opt.UpdateExistingDraft) { Err(rr, "A draft session exists. Enable 'Update existing Draft' to import into it."); AddRow(pv, rr); continue; }
                        rr.Validation = "Warning"; rr.Message = "Will update existing draft session.";
                        scopeKeysUpdate.Add(scopeKey);
                    }
                }
                else scopeKeysCreate.Add(scopeKey);

                AddRow(pv, rr);
            }

            pv.SessionsToCreate = scopeKeysCreate.Count;
            pv.SessionsToUpdate = scopeKeysUpdate.Count;

            // Submitted import requires completeness: every eligible student per imported scope must be present.
            if (opt.ImportAsSubmitted && pv.Error == 0)
            {
                var eligible = new System.Collections.Generic.HashSet<int>();
                foreach (DataRow r in GetEligibleStudents(opt.AcademicYearID, opt.SectionID).Rows) eligible.Add(Convert.ToInt32(r["StudentID"]));
                foreach (var kv in scopeStudents)
                {
                    var present = new System.Collections.Generic.HashSet<int>();
                    foreach (ImportRowResult r in pv.Rows) if ((r.Date.ToString("yyyyMMdd") + "|" + opt.SessionType + "|" + (opt.SubjectID ?? 0)) == kv.Key && r.Importable) present.Add(r.StudentID);
                    if (present.Count < eligible.Count)
                    { pv.HeaderError = "Submitted import requires every eligible student for each date. Scope " + kv.Key.Split('|')[0] + " is missing " + (eligible.Count - present.Count) + " student(s)."; }
                }
            }

            pv.CanImport = pv.Error == 0 && pv.Valid + pv.Warning > 0 && string.IsNullOrEmpty(pv.HeaderError);
            return pv;
        }

        private static void AddRow(ImportPreview pv, ImportRowResult rr)
        {
            pv.Rows.Add(rr); pv.Total++;
            if (rr.Validation == "Error") pv.Error++;
            else if (rr.Validation == "Warning") pv.Warning++;
            else pv.Valid++;
        }
        private static void Err(ImportRowResult rr, string msg) { rr.Validation = "Error"; rr.Message = msg; }

        private static string NormalizeStatus(string s)
        {
            switch ((s ?? "").Trim().ToLowerInvariant())
            {
                case "present": return "Present";
                case "absent": return "Absent";
                case "late": return "Late";
                case "excused": return "Excused";
                default: return null;
            }
        }

        /// <summary>Scope structure validation without the date bound (per-row dates validated separately).</summary>
        private string ValidateScopeStructureOnly(AttendanceScope sc)
        {
            if (sc.AcademicYearID <= 0) return "Academic year is required.";
            if (sc.ClassID <= 0) return "Class is required.";
            if (sc.SectionID <= 0) return "Section is required.";
            if (Array.IndexOf(ValidSessionTypes, sc.SessionType) < 0) return "Invalid session type.";
            DataTable sec = ExecuteDataTable("SELECT ClassID FROM Sections WHERE SectionID=@sec", new[] { P("@sec", sc.SectionID) });
            if (sec.Rows.Count == 0) return "The section does not exist.";
            if (Convert.ToInt32(sec.Rows[0]["ClassID"]) != sc.ClassID) return "The section does not belong to the class.";
            bool subjectSession = string.Equals(sc.SessionType, "Subject", StringComparison.OrdinalIgnoreCase);
            if (subjectSession && !sc.SubjectID.HasValue) return "Subject attendance requires a subject.";
            if (!subjectSession && sc.SubjectID.HasValue) return "Only a Subject session may specify a subject.";
            if (sc.SubjectID.HasValue)
            {
                int ok = Convert.ToInt32(ExecuteScalar(
                    "SELECT COUNT(*) FROM ClassSubjectTeachers cst JOIN Sections s ON s.SectionID=cst.SectionID WHERE s.ClassID=@c AND cst.SubjectID=@subj AND (@y=0 OR cst.AcademicYearID=@y)",
                    new[] { P("@c", sc.ClassID), P("@subj", sc.SubjectID.Value), P("@y", sc.AcademicYearID) }));
                if (ok == 0) return "The subject is not assigned to the class.";
            }
            return null;
        }

        /// <summary>Import a validated batch in ONE Serializable transaction. Returns the audit batch id.
        /// Rolls the whole batch back on any failure (nothing partially saved).</summary>
        public int ImportAttendanceBatch(System.Collections.Generic.List<string[]> csv, ImportOptions opt, string originalFileName, string fileHash, int userId, string role)
        {
            ImportPreview pv = GetAttendanceImportPreview(csv, opt, userId, role);
            if (!string.IsNullOrEmpty(pv.HeaderError)) throw new InvalidOperationException(pv.HeaderError);
            if (!pv.CanImport) throw new InvalidOperationException("The file has validation errors. No records were saved.");
            if (AttendanceImportHashExists(fileHash, opt.AcademicYearID, opt.ClassID, opt.SectionID))
                throw new InvalidOperationException("This exact file has already been imported for this scope.");

            DataRow settings = GetAttendanceSettings();
            TimeSpan startTime = (TimeSpan)settings["AttendanceStartTime"];
            bool excusedNeedsRemarks = Convert.ToBoolean(settings["ExcusedRequiresRemarks"]);

            int sessionsTouched = 0, recordsTouched = 0;

            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    // group importable rows by date scope
                    var byScope = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<ImportRowResult>>();
                    foreach (ImportRowResult r in pv.Rows)
                    {
                        if (!r.Importable) continue;
                        string key = r.Date.ToString("yyyyMMdd");
                        if (!byScope.ContainsKey(key)) byScope[key] = new System.Collections.Generic.List<ImportRowResult>();
                        byScope[key].Add(r);
                    }

                    foreach (var kv in byScope)
                    {
                        DateTime date = DateTime.ParseExact(kv.Key, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        var sc = new AttendanceScope { AcademicYearID = opt.AcademicYearID, TermID = opt.TermID, AttendanceDate = date, ClassID = opt.ClassID, SectionID = opt.SectionID, SubjectID = opt.SubjectID, SessionType = opt.SessionType };
                        int sessionId = LockOrCreateSession(cn, tx, sc, userId, requireDraft: true);
                        var eligible = LoadEligibleSet(cn, tx, sc);

                        if (opt.ImportAsSubmitted)
                        {
                            var present = new System.Collections.Generic.HashSet<int>();
                            foreach (ImportRowResult r in kv.Value) present.Add(r.StudentID);
                            foreach (int sid in eligible) if (!present.Contains(sid))
                                throw new InvalidOperationException("Submitted import requires every eligible student. Import failed. No records were saved.");
                        }

                        foreach (ImportRowResult r in kv.Value)
                        {
                            if (!eligible.Contains(r.StudentID)) throw new InvalidOperationException("A student does not belong to the selected section. Import failed.");
                            var mr = new MarkRow { StudentID = r.StudentID, Status = r.Status, CheckInTime = r.CheckIn, Remarks = string.IsNullOrWhiteSpace(r.Remarks) ? null : r.Remarks };
                            UpsertRecord(cn, tx, sessionId, mr, startTime, excusedNeedsRemarks, userId);
                            recordsTouched++;
                        }

                        if (opt.ImportAsSubmitted)
                            NonQuery(cn, tx, "UPDATE AttendanceSessions SET Status='Submitted', MarkedBy=ISNULL(MarkedBy,@u), SubmittedBy=@u, SubmittedAt=GETDATE(), UpdatedAt=GETDATE() WHERE AttendanceSessionID=@s", P("@u", userId), P("@s", sessionId));
                        else
                            NonQuery(cn, tx, "UPDATE AttendanceSessions SET MarkedBy=@u, UpdatedAt=GETDATE() WHERE AttendanceSessionID=@s", P("@u", userId), P("@s", sessionId));
                        sessionsTouched++;
                    }

                    int batchId = Convert.ToInt32(Scalar(cn, tx, @"
INSERT INTO AttendanceImportBatches (OriginalFileName, FileHash, AcademicYearID, ClassID, SectionID, SubjectID, SessionType, ImportStatus, TotalRows, ValidRows, ErrorRows, ImportedSessions, ImportedRecords, ImportedBy, ImportedAt, CreatedAt)
VALUES (@fn, @h, @y, @c, @sec, @subj, @type, @st, @total, @valid, @err, @sess, @recs, @u, GETDATE(), GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS int)",
                        P("@fn", (object)originalFileName ?? DBNull.Value), P("@h", (object)fileHash ?? DBNull.Value),
                        P("@y", opt.AcademicYearID), P("@c", opt.ClassID), P("@sec", opt.SectionID), P("@subj", (object)opt.SubjectID ?? DBNull.Value),
                        P("@type", opt.SessionType), P("@st", opt.ImportAsSubmitted ? "Submitted" : "Draft"),
                        P("@total", pv.Total), P("@valid", pv.Valid + pv.Warning), P("@err", pv.Error), P("@sess", sessionsTouched), P("@recs", recordsTouched), P("@u", userId)));

                    tx.Commit();
                    return batchId;
                }
            }
        }

        public DataTable GetAttendanceImportHistory(int top)
        {
            string sql = @"
SELECT TOP (" + (top > 0 ? top : 10) + @") b.AttendanceImportBatchID, b.OriginalFileName, b.SessionType, b.ImportStatus,
       b.TotalRows, b.ValidRows, b.ErrorRows, b.ImportedSessions, b.ImportedRecords, b.ImportedAt,
       c.ClassName, sec.SectionName, COALESCE(u.FullName,'—') AS ImportedByName
FROM AttendanceImportBatches b
JOIN Classes c ON c.ClassID=b.ClassID
JOIN Sections sec ON sec.SectionID=b.SectionID
LEFT JOIN Users u ON u.UserID=b.ImportedBy
ORDER BY b.AttendanceImportBatchID DESC";
            return ExecuteDataTable(sql, null);
        }

        // ================================================================
        // STAGE 5 — ANALYTICS, ALERTS, PARENT ACCESS
        // ================================================================

        public bool UserCanViewAttendanceAnalytics(string role)
        {
            string r = NormalizeRole(role);
            return CanManageAttendance(role) || r == "registrar" || r == "teacher";
        }
        public bool UserCanViewAttendanceAlerts(string role)
        {
            string r = NormalizeRole(role);
            return CanManageAttendance(role) || r == "registrar" || r == "teacher";
        }

        private string AnalyticsWhere()
        {
            return @"ss.Status IN ('Submitted','Locked')
  AND (@y=0 OR ss.AcademicYearID=@y) AND (@tm IS NULL OR ss.TermID=@tm)
  AND (@c IS NULL OR ss.ClassID=@c) AND (@sec IS NULL OR ss.SectionID=@sec)
  AND (@type='' OR ss.SessionType=@type) AND (@subj IS NULL OR ss.SubjectID=@subj)
  AND ss.AttendanceDate BETWEEN @from AND @to";
        }
        private SqlParameter[] AnalyticsParams(int y, int? tm, int? c, int? sec, string type, int? subj, DateTime from, DateTime to)
        {
            return new[]
            {
                P("@y", y), P("@tm", (object)tm ?? DBNull.Value), P("@c", (object)c ?? DBNull.Value), P("@sec", (object)sec ?? DBNull.Value),
                P("@type", type ?? ""), P("@subj", (object)subj ?? DBNull.Value), P("@from", from.Date), P("@to", to.Date)
            };
        }

        public DataRow GetAttendanceAnalyticsSummary(int y, int? tm, int? c, int? sec, string type, int? subj, DateTime from, DateTime to)
        {
            string sql = @"
SELECT
  (SELECT COUNT(*) FROM AttendanceSessions ss WHERE " + AnalyticsWhere() + @") AS Sessions,
  (SELECT COUNT(DISTINCT r.StudentID) FROM AttendanceSessions ss JOIN AttendanceRecords r ON r.AttendanceSessionID=ss.AttendanceSessionID WHERE " + AnalyticsWhere() + @") AS Students,
  SUM(CASE WHEN r.AttendanceStatus='Present' THEN 1 ELSE 0 END) AS P,
  SUM(CASE WHEN r.AttendanceStatus='Absent'  THEN 1 ELSE 0 END) AS A,
  SUM(CASE WHEN r.AttendanceStatus='Late'    THEN 1 ELSE 0 END) AS L,
  SUM(CASE WHEN r.AttendanceStatus='Excused' THEN 1 ELSE 0 END) AS E
FROM AttendanceSessions ss JOIN AttendanceRecords r ON r.AttendanceSessionID=ss.AttendanceSessionID
WHERE " + AnalyticsWhere();
            DataRow row = ExecuteDataTable(sql, AnalyticsParams(y, tm, c, sec, type, subj, from, to)).Rows[0];
            int p = N(row["P"]), a = N(row["A"]), l = N(row["L"]), e = N(row["E"]);
            decimal rate = CalculateAttendanceRate(p, a, l, e);
            int atRisk = GetAtRiskStudents(y, tm, c, sec, type, subj, from, to).Rows.Count;

            DataTable outT = new DataTable();
            outT.Columns.Add("Rate", typeof(decimal)); outT.Columns.Add("Sessions", typeof(int)); outT.Columns.Add("Students", typeof(int));
            outT.Columns.Add("Present", typeof(int)); outT.Columns.Add("Absent", typeof(int)); outT.Columns.Add("Late", typeof(int));
            outT.Columns.Add("Excused", typeof(int)); outT.Columns.Add("AtRisk", typeof(int));
            outT.Rows.Add(rate, N(row["Sessions"]), N(row["Students"]), p, a, l, e, atRisk);
            return outT.Rows[0];
        }

        public DataTable GetAttendanceStatusBreakdown(int y, int? tm, int? c, int? sec, string type, int? subj, DateTime from, DateTime to)
        {
            string sql = @"
SELECT r.AttendanceStatus, COUNT(*) AS Cnt
FROM AttendanceSessions ss JOIN AttendanceRecords r ON r.AttendanceSessionID=ss.AttendanceSessionID
WHERE " + AnalyticsWhere() + " GROUP BY r.AttendanceStatus";
            return ExecuteDataTable(sql, AnalyticsParams(y, tm, c, sec, type, subj, from, to));
        }

        /// <summary>Daily rate for the last N days ending at 'to' (weekly trend uses days=7-ish range).</summary>
        public DataTable GetWeeklyAttendanceTrend(int y, int? tm, int? c, int? sec, string type, int? subj, DateTime from, DateTime to)
        {
            string sql = @"
SELECT ss.AttendanceDate AS Bucket,
  SUM(CASE WHEN r.AttendanceStatus='Present' THEN 1 ELSE 0 END) AS P,
  SUM(CASE WHEN r.AttendanceStatus='Absent'  THEN 1 ELSE 0 END) AS A,
  SUM(CASE WHEN r.AttendanceStatus='Late'    THEN 1 ELSE 0 END) AS L,
  SUM(CASE WHEN r.AttendanceStatus='Excused' THEN 1 ELSE 0 END) AS E
FROM AttendanceSessions ss JOIN AttendanceRecords r ON r.AttendanceSessionID=ss.AttendanceSessionID
WHERE " + AnalyticsWhere() + @"
GROUP BY ss.AttendanceDate ORDER BY ss.AttendanceDate";
            return RateBuckets(ExecuteDataTable(sql, AnalyticsParams(y, tm, c, sec, type, subj, from, to)), "Bucket", "d");
        }

        public DataTable GetMonthlyAttendanceTrend(int y, int? tm, int? c, int? sec, string type, int? subj, DateTime from, DateTime to)
        {
            string sql = @"
SELECT DATEFROMPARTS(YEAR(ss.AttendanceDate), MONTH(ss.AttendanceDate), 1) AS Bucket,
  SUM(CASE WHEN r.AttendanceStatus='Present' THEN 1 ELSE 0 END) AS P,
  SUM(CASE WHEN r.AttendanceStatus='Absent'  THEN 1 ELSE 0 END) AS A,
  SUM(CASE WHEN r.AttendanceStatus='Late'    THEN 1 ELSE 0 END) AS L,
  SUM(CASE WHEN r.AttendanceStatus='Excused' THEN 1 ELSE 0 END) AS E
FROM AttendanceSessions ss JOIN AttendanceRecords r ON r.AttendanceSessionID=ss.AttendanceSessionID
WHERE " + AnalyticsWhere() + @"
GROUP BY DATEFROMPARTS(YEAR(ss.AttendanceDate), MONTH(ss.AttendanceDate), 1) ORDER BY Bucket";
            return RateBuckets(ExecuteDataTable(sql, AnalyticsParams(y, tm, c, sec, type, subj, from, to)), "Bucket", "m");
        }

        private DataTable RateBuckets(DataTable raw, string bucketCol, string mode)
        {
            DataTable outT = new DataTable();
            outT.Columns.Add("Label", typeof(string)); outT.Columns.Add("Rate", typeof(decimal));
            foreach (DataRow r in raw.Rows)
            {
                int p = N(r["P"]), a = N(r["A"]), l = N(r["L"]), e = N(r["E"]);
                DateTime d = Convert.ToDateTime(r[bucketCol]);
                outT.Rows.Add(mode == "m" ? d.ToString("MMM yyyy") : d.ToString("dd MMM"), CalculateAttendanceRate(p, a, l, e));
            }
            return outT;
        }

        private DataTable GroupRate(string groupExpr, string groupName, int y, int? tm, int? c, int? sec, string type, int? subj, DateTime from, DateTime to)
        {
            string sql = @"
SELECT " + groupExpr + @" AS GroupName,
  SUM(CASE WHEN r.AttendanceStatus='Present' THEN 1 ELSE 0 END) AS P,
  SUM(CASE WHEN r.AttendanceStatus='Absent'  THEN 1 ELSE 0 END) AS A,
  SUM(CASE WHEN r.AttendanceStatus='Late'    THEN 1 ELSE 0 END) AS L,
  SUM(CASE WHEN r.AttendanceStatus='Excused' THEN 1 ELSE 0 END) AS E
FROM AttendanceSessions ss
JOIN AttendanceRecords r ON r.AttendanceSessionID=ss.AttendanceSessionID
JOIN Classes cls ON cls.ClassID=ss.ClassID
JOIN Sections secn ON secn.SectionID=ss.SectionID
WHERE " + AnalyticsWhere() + " GROUP BY " + groupExpr + " ORDER BY " + groupExpr;
            DataTable raw = ExecuteDataTable(sql, AnalyticsParams(y, tm, c, sec, type, subj, from, to));
            DataTable outT = new DataTable();
            outT.Columns.Add(groupName, typeof(string)); outT.Columns.Add("Rate", typeof(decimal));
            outT.Columns.Add("Present", typeof(int)); outT.Columns.Add("Absent", typeof(int));
            foreach (DataRow r in raw.Rows)
            {
                int p = N(r["P"]), a = N(r["A"]), l = N(r["L"]), e = N(r["E"]);
                outT.Rows.Add(Convert.ToString(r["GroupName"]), CalculateAttendanceRate(p, a, l, e), p, a);
            }
            return outT;
        }

        public DataTable GetAttendanceByClassAnalytics(int y, int? tm, int? c, int? sec, string type, int? subj, DateTime from, DateTime to)
        { return GroupRate("cls.ClassName", "ClassName", y, tm, c, sec, type, subj, from, to); }
        public DataTable GetAttendanceBySectionAnalytics(int y, int? tm, int? c, int? sec, string type, int? subj, DateTime from, DateTime to)
        { return GroupRate("cls.ClassName + ' / ' + secn.SectionName", "SectionName", y, tm, c, sec, type, subj, from, to); }

        /// <summary>Per-student aggregates (rate/absent/late) for ranking + risk. Historical: joins through sessions.</summary>
        private DataTable StudentAggregates(int y, int? tm, int? c, int? sec, string type, int? subj, DateTime from, DateTime to)
        {
            string sql = @"
SELECT st.StudentID, st.FullName, st.StudentCode,
  SUM(CASE WHEN r.AttendanceStatus='Present' THEN 1 ELSE 0 END) AS P,
  SUM(CASE WHEN r.AttendanceStatus='Absent'  THEN 1 ELSE 0 END) AS A,
  SUM(CASE WHEN r.AttendanceStatus='Late'    THEN 1 ELSE 0 END) AS L,
  SUM(CASE WHEN r.AttendanceStatus='Excused' THEN 1 ELSE 0 END) AS E,
  ISNULL(SUM(r.LateMinutes),0) AS LateMin, COUNT(*) AS Total
FROM AttendanceSessions ss JOIN AttendanceRecords r ON r.AttendanceSessionID=ss.AttendanceSessionID
JOIN Students st ON st.StudentID=r.StudentID
WHERE " + AnalyticsWhere() + @"
GROUP BY st.StudentID, st.FullName, st.StudentCode";
            DataTable raw = ExecuteDataTable(sql, AnalyticsParams(y, tm, c, sec, type, subj, from, to));
            DataTable outT = new DataTable();
            outT.Columns.Add("StudentID", typeof(int)); outT.Columns.Add("FullName", typeof(string)); outT.Columns.Add("StudentCode", typeof(string));
            foreach (string cn in new[] { "Present", "Absent", "Late", "Excused", "LateMinutes", "TotalSessions" }) outT.Columns.Add(cn, typeof(int));
            outT.Columns.Add("Percentage", typeof(decimal));
            foreach (DataRow r in raw.Rows)
            {
                int p = N(r["P"]), a = N(r["A"]), l = N(r["L"]), e = N(r["E"]);
                outT.Rows.Add(Convert.ToInt32(r["StudentID"]), Convert.ToString(r["FullName"]), Convert.ToString(r["StudentCode"]),
                    p, a, l, e, N(r["LateMin"]), N(r["Total"]), CalculateAttendanceRate(p, a, l, e));
            }
            return outT;
        }

        public DataTable GetTopAttendanceStudents(int y, int? tm, int? c, int? sec, string type, int? subj, DateTime from, DateTime to, int top)
        {
            DataTable t = StudentAggregates(y, tm, c, sec, type, subj, from, to);
            DataView dv = t.DefaultView; dv.RowFilter = "TotalSessions >= 1"; dv.Sort = "Percentage DESC, TotalSessions DESC";
            return TopN(dv.ToTable(), top);
        }
        public DataTable GetMostAbsentStudents(int y, int? tm, int? c, int? sec, string type, int? subj, DateTime from, DateTime to, int top)
        {
            DataTable t = StudentAggregates(y, tm, c, sec, type, subj, from, to);
            DataView dv = t.DefaultView; dv.RowFilter = "Absent > 0"; dv.Sort = "Absent DESC, Percentage ASC";
            return TopN(dv.ToTable(), top);
        }
        public DataTable GetFrequentLateStudents(int y, int? tm, int? c, int? sec, string type, int? subj, DateTime from, DateTime to, int top)
        {
            DataTable t = StudentAggregates(y, tm, c, sec, type, subj, from, to);
            DataView dv = t.DefaultView; dv.RowFilter = "Late > 0"; dv.Sort = "Late DESC, LateMinutes DESC";
            return TopN(dv.ToTable(), top);
        }
        public DataTable GetAtRiskStudents(int y, int? tm, int? c, int? sec, string type, int? subj, DateTime from, DateTime to)
        {
            decimal threshold = LowAttendanceThreshold();
            DataTable t = StudentAggregates(y, tm, c, sec, type, subj, from, to);
            DataView dv = t.DefaultView;
            dv.RowFilter = "TotalSessions >= 3 AND Percentage < " + threshold.ToString(System.Globalization.CultureInfo.InvariantCulture);
            dv.Sort = "Percentage ASC";
            return dv.ToTable();
        }
        private static DataTable TopN(DataTable t, int n)
        {
            if (n <= 0 || t.Rows.Count <= n) return t;
            DataTable outT = t.Clone();
            for (int i = 0; i < n; i++) outT.ImportRow(t.Rows[i]);
            return outT;
        }

        // ---------- ALERTS ----------
        public DataRow GetAlertSettings()
        {
            DataRow s = GetAttendanceSettings();
            return s;
        }

        /// <summary>Recompute alerts from current submitted/locked data in one transaction.
        /// Upserts active alerts (updates LastDetectedAt), never deletes resolved history.
        /// Excused breaks the consecutive-absence sequence (documented policy).</summary>
        public int GenerateAttendanceAlerts(int academicYearId)
        {
            DataRow s = GetAttendanceSettings();
            int consecThreshold = Convert.ToInt32(s["ConsecutiveAbsenceAlert"]);
            decimal lowThreshold = Convert.ToDecimal(s["LowAttendanceThreshold"]);
            int lateThreshold = Convert.ToInt32(s["FrequentLateThreshold"]);
            int unsubmittedHours = Convert.ToInt32(s["UnsubmittedSessionAgeHours"]);
            int lookbackDays = Convert.ToInt32(s["AlertLookbackDays"]);
            DateTime from = DateTime.Today.AddDays(-lookbackDays);
            int generated = 0;

            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    // ---- Low attendance + frequent late (per student, from aggregates) ----
                    DataTable perStudent = StudentAggForAlerts(cn, tx, academicYearId, from);
                    foreach (DataRow r in perStudent.Rows)
                    {
                        int sid = Convert.ToInt32(r["StudentID"]); int total = N(r["Total"]);
                        int p = N(r["P"]), a = N(r["A"]), l = N(r["L"]), e = N(r["E"]); int lateMin = N(r["LateMin"]);
                        decimal rate = CalculateAttendanceRate(p, a, l, e);

                        if (total >= 3 && rate < lowThreshold)
                            generated += UpsertAlert(cn, tx, "LowAttendance", "LowAttendance:" + sid, sid, null, null, null,
                                "Low attendance", Convert.ToString(r["FullName"]) + " is at " + rate.ToString("0.0") + "% (below " + lowThreshold.ToString("0.#") + "%).",
                                rate < lowThreshold - 10 ? "Critical" : "Warning", rate, lowThreshold, true);

                        if (l >= lateThreshold)
                            generated += UpsertAlert(cn, tx, "FrequentLate", "FrequentLate:" + sid, sid, null, null, null,
                                "Frequent late arrivals", Convert.ToString(r["FullName"]) + " has " + l + " late arrivals (" + lateMin + " total minutes).",
                                "Warning", l, lateThreshold, true);
                    }

                    // ---- Consecutive absences (Excused breaks the sequence) ----
                    DataTable seq = ExecuteInTx(cn, tx, @"
SELECT r.StudentID, ss.AttendanceDate, r.AttendanceStatus, st.FullName
FROM AttendanceSessions ss JOIN AttendanceRecords r ON r.AttendanceSessionID=ss.AttendanceSessionID
JOIN Students st ON st.StudentID=r.StudentID
WHERE ss.Status IN ('Submitted','Locked') AND (@y=0 OR ss.AcademicYearID=@y) AND ss.AttendanceDate >= @from
ORDER BY r.StudentID, ss.AttendanceDate",
                        P("@y", academicYearId), P("@from", from));
                    int curStudent = -1, run = 0; string curName = "";
                    var flagged = new System.Collections.Generic.HashSet<int>();
                    foreach (DataRow r in seq.Rows)
                    {
                        int sid = Convert.ToInt32(r["StudentID"]);
                        string status = Convert.ToString(r["AttendanceStatus"]);
                        if (sid != curStudent) { curStudent = sid; run = 0; curName = Convert.ToString(r["FullName"]); }
                        if (status == "Absent") run++;
                        else run = 0;   // Present, Late, Excused all break the streak
                        if (run >= consecThreshold && !flagged.Contains(sid))
                        {
                            flagged.Add(sid);
                            generated += UpsertAlert(cn, tx, "ConsecutiveAbsence", "ConsecutiveAbsence:" + sid, sid, null, null, null,
                                "Consecutive absences", curName + " has " + run + " consecutive absences.", "Critical", run, consecThreshold, true);
                        }
                    }

                    // ---- Unsubmitted (old Draft) sessions — operational, NOT visible to parents ----
                    DataTable drafts = ExecuteInTx(cn, tx, @"
SELECT ss.AttendanceSessionID, ss.ClassID, ss.SectionID, ss.AttendanceDate, c.ClassName, sec.SectionName
FROM AttendanceSessions ss JOIN Classes c ON c.ClassID=ss.ClassID JOIN Sections sec ON sec.SectionID=ss.SectionID
WHERE ss.Status='Draft' AND (@y=0 OR ss.AcademicYearID=@y) AND ss.CreatedAt < DATEADD(HOUR, -@h, GETDATE())",
                        P("@y", academicYearId), P("@h", unsubmittedHours));
                    foreach (DataRow r in drafts.Rows)
                    {
                        int did = Convert.ToInt32(r["AttendanceSessionID"]);
                        generated += UpsertAlert(cn, tx, "UnsubmittedSession", "UnsubmittedSession:" + did, null,
                            Convert.ToInt32(r["ClassID"]), Convert.ToInt32(r["SectionID"]), did,
                            "Unsubmitted attendance", Convert.ToString(r["ClassName"]) + " / " + Convert.ToString(r["SectionName"]) + " on " + Convert.ToDateTime(r["AttendanceDate"]).ToString("dd MMM yyyy") + " is still Draft.",
                            "Warning", null, unsubmittedHours, false);
                    }

                    tx.Commit();
                }
            }
            return generated;
        }

        private DataTable StudentAggForAlerts(SqlConnection cn, SqlTransaction tx, int y, DateTime from)
        {
            return ExecuteInTx(cn, tx, @"
SELECT st.StudentID, st.FullName,
  SUM(CASE WHEN r.AttendanceStatus='Present' THEN 1 ELSE 0 END) AS P,
  SUM(CASE WHEN r.AttendanceStatus='Absent'  THEN 1 ELSE 0 END) AS A,
  SUM(CASE WHEN r.AttendanceStatus='Late'    THEN 1 ELSE 0 END) AS L,
  SUM(CASE WHEN r.AttendanceStatus='Excused' THEN 1 ELSE 0 END) AS E,
  ISNULL(SUM(r.LateMinutes),0) AS LateMin, COUNT(*) AS Total
FROM AttendanceSessions ss JOIN AttendanceRecords r ON r.AttendanceSessionID=ss.AttendanceSessionID
JOIN Students st ON st.StudentID=r.StudentID
WHERE ss.Status IN ('Submitted','Locked') AND (@y=0 OR ss.AcademicYearID=@y) AND ss.AttendanceDate >= @from
GROUP BY st.StudentID, st.FullName",
                P("@y", y), P("@from", from));
        }

        private int UpsertAlert(SqlConnection cn, SqlTransaction tx, string type, string key, int? studentId, int? classId, int? sectionId, int? sessionId,
            string title, string desc, string severity, decimal? trigger, decimal? threshold, bool visibleToParent)
        {
            int updated = Convert.ToInt32(Scalar(cn, tx, @"
UPDATE AttendanceAlerts SET LastDetectedAt=GETDATE(), TriggerValue=@tr, Description=@d, Severity=@sev, UpdatedAt=GETDATE()
WHERE AlertKey=@k AND Status IN ('New','Reviewed'); SELECT @@ROWCOUNT",
                P("@tr", (object)trigger ?? DBNull.Value), P("@d", desc), P("@sev", severity), P("@k", key)));
            if (updated > 0) return 0;
            NonQuery(cn, tx, @"
INSERT INTO AttendanceAlerts (AlertType, AlertKey, StudentID, ClassID, SectionID, AttendanceSessionID, Title, Description, Severity, Status, TriggerValue, ThresholdValue, IsVisibleToParent, FirstDetectedAt, LastDetectedAt, CreatedAt)
VALUES (@type,@k,@st,@c,@sec,@ses,@title,@d,@sev,'New',@tr,@th,@vis,GETDATE(),GETDATE(),GETDATE())",
                P("@type", type), P("@k", key), P("@st", (object)studentId ?? DBNull.Value), P("@c", (object)classId ?? DBNull.Value),
                P("@sec", (object)sectionId ?? DBNull.Value), P("@ses", (object)sessionId ?? DBNull.Value), P("@title", title), P("@d", desc),
                P("@sev", severity), P("@tr", (object)trigger ?? DBNull.Value), P("@th", (object)threshold ?? DBNull.Value), P("@vis", visibleToParent));
            return 1;
        }

        public DataTable GetAttendanceAlerts(string type, string status, string severity)
        {
            const string sql = @"
SELECT a.AttendanceAlertID, a.AlertType, a.Title, a.Description, a.Severity, a.Status,
       a.TriggerValue, a.ThresholdValue, a.FirstDetectedAt, a.LastDetectedAt,
       ISNULL(st.FullName,'') AS StudentName, ISNULL(st.StudentCode,'') AS StudentCode,
       ISNULL(c.ClassName,'') AS ClassName, ISNULL(sec.SectionName,'') AS SectionName,
       ISNULL(rv.FullName,'') AS ReviewedByName, a.ReviewedAt, ISNULL(a.ResolutionNotes,'') AS ResolutionNotes
FROM AttendanceAlerts a
LEFT JOIN Students st ON st.StudentID=a.StudentID
LEFT JOIN Classes c ON c.ClassID=a.ClassID
LEFT JOIN Sections sec ON sec.SectionID=a.SectionID
LEFT JOIN Users rv ON rv.UserID=a.ReviewedBy
WHERE (@type='' OR a.AlertType=@type) AND (@status='' OR a.Status=@status) AND (@sev='' OR a.Severity=@sev)
ORDER BY CASE a.Severity WHEN 'Critical' THEN 0 WHEN 'Warning' THEN 1 ELSE 2 END, a.LastDetectedAt DESC";
            return ExecuteDataTable(sql, new[] { P("@type", type ?? ""), P("@status", status ?? ""), P("@sev", severity ?? "") });
        }

        public DataRow GetAttendanceAlert(int id)
        {
            DataTable t = ExecuteDataTable("SELECT * FROM AttendanceAlerts WHERE AttendanceAlertID=@id", new[] { P("@id", id) });
            return t.Rows.Count > 0 ? t.Rows[0] : null;
        }

        public DataRow GetAttendanceAlertSummary()
        {
            const string sql = @"
SELECT
  SUM(CASE WHEN Status='New' THEN 1 ELSE 0 END) AS NewCount,
  SUM(CASE WHEN Status='Reviewed' THEN 1 ELSE 0 END) AS ReviewedCount,
  SUM(CASE WHEN Status='Resolved' THEN 1 ELSE 0 END) AS ResolvedCount,
  SUM(CASE WHEN Severity='Critical' AND Status IN ('New','Reviewed') THEN 1 ELSE 0 END) AS CriticalActive,
  COUNT(*) AS Total
FROM AttendanceAlerts";
            return ExecuteDataTable(sql, null).Rows[0];
        }

        public void UpdateAttendanceAlertStatus(int alertId, string newStatus, int userId, string role)
        {
            if (!CanManageAttendance(role)) throw new InvalidOperationException("You are not authorized to change alert status.");
            if (newStatus != "Reviewed" && newStatus != "Dismissed") throw new InvalidOperationException("Invalid status.");
            ExecuteNonQuery(@"UPDATE AttendanceAlerts SET Status=@s, ReviewedBy=@u, ReviewedAt=GETDATE(), UpdatedAt=GETDATE() WHERE AttendanceAlertID=@id AND Status IN ('New','Reviewed')",
                new[] { P("@s", newStatus), P("@u", userId), P("@id", alertId) });
        }

        public void ResolveAttendanceAlert(int alertId, string notes, int userId, string role)
        {
            if (!CanManageAttendance(role)) throw new InvalidOperationException("You are not authorized to resolve alerts.");
            ExecuteNonQuery(@"UPDATE AttendanceAlerts SET Status='Resolved', ResolvedBy=@u, ResolvedAt=GETDATE(), ResolutionNotes=@n, UpdatedAt=GETDATE() WHERE AttendanceAlertID=@id AND Status IN ('New','Reviewed')",
                new[] { P("@u", userId), P("@n", (object)notes ?? DBNull.Value), P("@id", alertId) });
        }

        // ---------- PARENT ACCESS ----------
        public DataTable GetParentLinkedStudents(int userId)
        {
            const string sql = @"
SELECT DISTINCT st.StudentID, st.FullName, st.StudentCode, st.SectionID, sec.ClassID, ISNULL(c.ClassName,'') AS ClassName,
       ISNULL(sec.SectionName,'') AS SectionName, ISNULL(y.YearName,'') AS YearName
FROM Guardians g
JOIN StudentGuardians sg ON sg.GuardianID=g.GuardianID
JOIN Students st ON st.StudentID=sg.StudentID
LEFT JOIN Sections sec ON sec.SectionID=st.SectionID
LEFT JOIN Classes c ON c.ClassID=sec.ClassID
LEFT JOIN AcademicYears y ON y.AcademicYearID=st.AcademicYearID
WHERE g.UserID=@u AND ISNULL(g.IsActive,1)=1
ORDER BY st.FullName";
            return ExecuteDataTable(sql, new[] { P("@u", userId) });
        }

        /// <summary>Server-side ownership: does this parent user link to this student via StudentGuardians?</summary>
        public bool UserOwnsStudent(int userId, int studentId)
        {
            return Convert.ToInt32(ExecuteScalar(
                "SELECT COUNT(*) FROM Guardians g JOIN StudentGuardians sg ON sg.GuardianID=g.GuardianID WHERE g.UserID=@u AND sg.StudentID=@st AND ISNULL(g.IsActive,1)=1",
                new[] { P("@u", userId), P("@st", studentId) })) > 0;
        }

        public DataRow GetParentAttendanceSummary(int studentId)
        {
            const string sql = @"
SELECT
  SUM(CASE WHEN r.AttendanceStatus='Present' THEN 1 ELSE 0 END) AS P,
  SUM(CASE WHEN r.AttendanceStatus='Absent'  THEN 1 ELSE 0 END) AS A,
  SUM(CASE WHEN r.AttendanceStatus='Late'    THEN 1 ELSE 0 END) AS L,
  SUM(CASE WHEN r.AttendanceStatus='Excused' THEN 1 ELSE 0 END) AS E,
  COUNT(*) AS Total
FROM AttendanceRecords r JOIN AttendanceSessions ss ON ss.AttendanceSessionID=r.AttendanceSessionID
WHERE r.StudentID=@st AND ss.Status IN ('Submitted','Locked')";
            DataRow row = ExecuteDataTable(sql, new[] { P("@st", studentId) }).Rows[0];
            int p = N(row["P"]), a = N(row["A"]), l = N(row["L"]), e = N(row["E"]);
            DataTable outT = new DataTable();
            foreach (string cn in new[] { "TotalSessions", "Present", "Absent", "Late", "Excused" }) outT.Columns.Add(cn, typeof(int));
            outT.Columns.Add("Percentage", typeof(decimal));
            outT.Rows.Add(N(row["Total"]), p, a, l, e, CalculateAttendanceRate(p, a, l, e));
            return outT.Rows[0];
        }

        public DataTable GetParentRecentAttendance(int studentId, int top)
        {
            string sql = @"
SELECT TOP (" + (top > 0 ? top : 15) + @") ss.AttendanceDate, c.ClassName, sec.SectionName, ss.SessionType,
       r.AttendanceStatus, r.CheckInTime, r.LateMinutes
FROM AttendanceRecords r JOIN AttendanceSessions ss ON ss.AttendanceSessionID=r.AttendanceSessionID
JOIN Classes c ON c.ClassID=ss.ClassID JOIN Sections sec ON sec.SectionID=ss.SectionID
WHERE r.StudentID=@st AND ss.Status IN ('Submitted','Locked')
ORDER BY ss.AttendanceDate DESC";
            return ExecuteDataTable(sql, new[] { P("@st", studentId) });
        }

        public DataTable GetParentAttendanceCalendar(int academicYearId, int year, int month, int studentId)
        {
            DateTime from = new DateTime(year, month, 1), to = from.AddMonths(1).AddDays(-1);
            const string sql = @"
SELECT ss.AttendanceDate,
  SUM(CASE WHEN r.AttendanceStatus='Present' THEN 1 ELSE 0 END) AS P,
  SUM(CASE WHEN r.AttendanceStatus='Absent'  THEN 1 ELSE 0 END) AS A,
  SUM(CASE WHEN r.AttendanceStatus='Late'    THEN 1 ELSE 0 END) AS L,
  SUM(CASE WHEN r.AttendanceStatus='Excused' THEN 1 ELSE 0 END) AS E, COUNT(*) AS Total
FROM AttendanceRecords r JOIN AttendanceSessions ss ON ss.AttendanceSessionID=r.AttendanceSessionID
WHERE r.StudentID=@st AND ss.Status IN ('Submitted','Locked') AND ss.AttendanceDate BETWEEN @from AND @to
GROUP BY ss.AttendanceDate ORDER BY ss.AttendanceDate";
            return ExecuteDataTable(sql, new[] { P("@st", studentId), P("@from", from), P("@to", to) });
        }

        /// <summary>Child-safe alerts only: active (New/Reviewed), parent-visible, with safe fields (no internal notes).</summary>
        public DataTable GetParentVisibleAlerts(int studentId)
        {
            const string sql = @"
SELECT a.AlertType, a.Title, a.Description, a.Severity, a.LastDetectedAt
FROM AttendanceAlerts a
WHERE a.StudentID=@st AND a.IsVisibleToParent=1 AND a.Status IN ('New','Reviewed')
ORDER BY a.LastDetectedAt DESC";
            return ExecuteDataTable(sql, new[] { P("@st", studentId) });
        }

        private DataTable ExecuteInTx(SqlConnection cn, SqlTransaction tx, string sql, params SqlParameter[] ps)
        {
            DataTable t = new DataTable();
            using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(t);
            }
            return t;
        }

        // ================================================================
        // ADO.NET HELPERS
        // ================================================================
        private SqlConnection CreateConnection() { return new SqlConnection(_connectionString); }

        private static SqlParameter P(string name, object value) { return new SqlParameter(name, value ?? DBNull.Value); }

        private DataTable ExecuteDataTable(string sql, SqlParameter[] ps)
        {
            DataTable t = new DataTable();
            using (SqlConnection cn = CreateConnection())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(t);
            }
            return t;
        }

        private object ExecuteScalar(string sql, SqlParameter[] ps)
        {
            using (SqlConnection cn = CreateConnection())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                cn.Open();
                return cmd.ExecuteScalar();
            }
        }

        private void ExecuteNonQuery(string sql, SqlParameter[] ps)
        {
            using (SqlConnection cn = CreateConnection())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static object Scalar(SqlConnection cn, SqlTransaction tx, string sql, params SqlParameter[] ps)
        {
            using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                return cmd.ExecuteScalar();
            }
        }

        private static void NonQuery(SqlConnection cn, SqlTransaction tx, string sql, params SqlParameter[] ps)
        {
            using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
