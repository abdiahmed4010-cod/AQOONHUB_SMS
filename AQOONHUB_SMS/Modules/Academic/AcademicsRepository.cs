using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AQOONHUB_SMS.Modules.Academic
{
    /// <summary>
    /// Single data-access layer for the whole Academics module
    /// (Academic Years, Terms, Classes, Sections, Subjects,
    /// Teacher Assignments, Timetable and Promotions).
    /// Direct ADO.NET, fully parameterised.
    /// </summary>
    public sealed class AcademicsRepository
    {
        private readonly string _connectionString;

        public AcademicsRepository()
        {
            ConnectionStringSettings settings =
                ConfigurationManager.ConnectionStrings["AQOONHUB_DB"];
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

        /// <summary>Roles allowed to manage Academics (create/edit).</summary>
        public bool CanManage(string role)
        {
            string r = NormalizeRole(role);
            return r == "superadmin" || r == "admin" || r == "registrar" || r == "academic";
        }

        /// <summary>Roles allowed to view Academics.</summary>
        public bool CanView(string role)
        {
            string r = NormalizeRole(role);
            return CanManage(role) || r == "teacher";
        }

        // ================================================================
        // SUMMARY / OVERVIEW
        // ================================================================
        public DataRow GetAcademicsSummary()
        {
            const string sql = @"
SELECT
  (SELECT TOP 1 YearName FROM AcademicYears WHERE Status='Active' ORDER BY AcademicYearID DESC) AS ActiveYear,
  (SELECT COUNT(*) FROM Classes  WHERE Status='Active' OR Status IS NULL) AS TotalClasses,
  (SELECT COUNT(*) FROM Sections WHERE Status='Active' OR Status IS NULL) AS TotalSections,
  (SELECT COUNT(*) FROM Subjects WHERE IsActive=1) AS TotalSubjects,
  (SELECT COUNT(*) FROM Staff s JOIN Users u ON s.UserID=u.UserID
      WHERE s.Status='Active' AND u.Role='Teacher') AS ActiveTeachers;";
            DataTable t = ExecuteDataTable(sql, null);
            return t.Rows.Count > 0 ? t.Rows[0] : t.NewRow();
        }

        /// <summary>Real student counts per class for the distribution chart.</summary>
        public DataTable GetStudentDistributionByClass(int? academicYearId)
        {
            const string sql = @"
SELECT c.ClassName,
       COUNT(st.StudentID) AS StudentCount
FROM Classes c
LEFT JOIN Sections sec ON sec.ClassID = c.ClassID
LEFT JOIN Students st ON st.SectionID = sec.SectionID
     AND (@yr IS NULL OR st.AcademicYearID = @yr)
     AND (st.Status IS NULL OR st.Status <> 'Withdrawn')
GROUP BY c.ClassID, c.ClassName
ORDER BY c.ClassID;";
            return ExecuteDataTable(sql, new[] { P("@yr", (object)academicYearId ?? DBNull.Value) });
        }

        /// <summary>Upcoming academic events derived from real year/term dates.</summary>
        public DataTable GetUpcomingEvents()
        {
            const string sql = @"
SELECT TOP 8 * FROM (
    SELECT 'Academic Year Start' AS Title, YearName AS Detail, StartDate AS EventDate FROM AcademicYears
    UNION ALL
    SELECT 'Academic Year End', YearName, EndDate FROM AcademicYears
    UNION ALL
    SELECT TermName + ' Begins', TermName, StartDate FROM Terms
    UNION ALL
    SELECT TermName + ' Ends', TermName, EndDate FROM Terms
) e
WHERE e.EventDate >= CAST(GETDATE() AS date)
ORDER BY e.EventDate;";
            return ExecuteDataTable(sql, null);
        }

        // ================================================================
        // ACADEMIC YEARS
        // ================================================================
        public DataTable GetAcademicYears(string search, string status)
        {
            const string sql = @"
SELECT AcademicYearID, YearName, StartDate, EndDate, Status,
       CAST('' AS nvarchar(200)) AS Description
FROM AcademicYears
WHERE (@s = '' OR YearName LIKE '%'+@s+'%')
  AND (@st = '' OR Status = @st)
ORDER BY StartDate DESC;";
            return ExecuteDataTable(sql, new[]
            {
                P("@s", search ?? ""),
                P("@st", status ?? "")
            });
        }

        public DataRow GetAcademicYear(int id)
        {
            DataTable t = ExecuteDataTable(
                "SELECT AcademicYearID, YearName, StartDate, EndDate, Status FROM AcademicYears WHERE AcademicYearID=@id",
                new[] { P("@id", id) });
            return t.Rows.Count > 0 ? t.Rows[0] : null;
        }

        /// <summary>Insert / update an academic year with validation and single-active enforcement.</summary>
        public void SaveAcademicYear(int id, string name, DateTime start, DateTime end, string status)
        {
            name = (name ?? "").Trim();
            status = (status ?? "Draft").Trim();
            if (name.Length == 0) throw new ArgumentException("Academic year name is required.");
            if (end <= start) throw new ArgumentException("End date must be after the start date.");

            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    // Duplicate name
                    if ((int)Scalar(cn, tx, "SELECT COUNT(*) FROM AcademicYears WHERE YearName=@n AND AcademicYearID<>@id",
                            P("@n", name), P("@id", id)) > 0)
                        throw new InvalidOperationException("An academic year with this name already exists.");

                    // Overlapping date range
                    if ((int)Scalar(cn, tx,
                            "SELECT COUNT(*) FROM AcademicYears WITH (UPDLOCK, HOLDLOCK) WHERE AcademicYearID<>@id AND @s <= EndDate AND @e >= StartDate",
                            P("@id", id), P("@s", start), P("@e", end)) > 0)
                        throw new InvalidOperationException("The date range overlaps an existing academic year.");

                    // Only one Active year
                    if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                        NonQuery(cn, tx, "UPDATE AcademicYears WITH (UPDLOCK, HOLDLOCK) SET Status='Completed', UpdatedAt=GETDATE() WHERE Status='Active' AND AcademicYearID<>@id",
                            P("@id", id));

                    if (id > 0)
                        NonQuery(cn, tx,
                            "UPDATE AcademicYears SET YearName=@n, StartDate=@s, EndDate=@e, Status=@st, UpdatedAt=GETDATE() WHERE AcademicYearID=@id",
                            P("@n", name), P("@s", start), P("@e", end), P("@st", status), P("@id", id));
                    else
                        NonQuery(cn, tx,
                            "INSERT INTO AcademicYears (YearName, StartDate, EndDate, Status, CreatedAt, UpdatedAt) VALUES (@n,@s,@e,@st,GETDATE(),GETDATE())",
                            P("@n", name), P("@s", start), P("@e", end), P("@st", status));

                    tx.Commit();
                }
            }
        }

        public void SetAcademicYearStatus(int id, string status)
        {
            status = (status ?? "").Trim();
            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                        NonQuery(cn, tx, "UPDATE AcademicYears WITH (UPDLOCK, HOLDLOCK) SET Status='Completed', UpdatedAt=GETDATE() WHERE Status='Active' AND AcademicYearID<>@id",
                            P("@id", id));
                    NonQuery(cn, tx, "UPDATE AcademicYears SET Status=@st, UpdatedAt=GETDATE() WHERE AcademicYearID=@id",
                        P("@st", status), P("@id", id));
                    tx.Commit();
                }
            }
        }

        // ================================================================
        // TERMS
        // ================================================================
        public DataTable GetTerms(int? academicYearId)
        {
            const string sql = @"
SELECT t.TermID, t.AcademicYearID, y.YearName, t.TermName, t.StartDate, t.EndDate, t.Status, t.IsCurrentTerm
FROM Terms t JOIN AcademicYears y ON t.AcademicYearID=y.AcademicYearID
WHERE (@yr IS NULL OR t.AcademicYearID=@yr)
ORDER BY t.StartDate;";
            return ExecuteDataTable(sql, new[] { P("@yr", (object)academicYearId ?? DBNull.Value) });
        }

        public void SaveTerm(int id, int academicYearId, string name, DateTime start, DateTime end, string status)
        {
            name = (name ?? "").Trim();
            if (name.Length == 0) throw new ArgumentException("Term name is required.");
            if (end <= start) throw new ArgumentException("End date must be after the start date.");

            DataRow yr = GetAcademicYear(academicYearId);
            if (yr == null) throw new InvalidOperationException("Select a valid academic year.");
            DateTime ys = Convert.ToDateTime(yr["StartDate"]), ye = Convert.ToDateTime(yr["EndDate"]);
            if (start < ys || end > ye) throw new InvalidOperationException("Term dates must fall inside the academic year.");

            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    if ((int)Scalar(cn, tx, "SELECT COUNT(*) FROM Terms WHERE AcademicYearID=@y AND TermName=@n AND TermID<>@id",
                            P("@y", academicYearId), P("@n", name), P("@id", id)) > 0)
                        throw new InvalidOperationException("A term with this name already exists for the year.");
                    if ((int)Scalar(cn, tx,
                            "SELECT COUNT(*) FROM Terms WITH (UPDLOCK,HOLDLOCK) WHERE AcademicYearID=@y AND TermID<>@id AND @s <= EndDate AND @e >= StartDate",
                            P("@y", academicYearId), P("@id", id), P("@s", start), P("@e", end)) > 0)
                        throw new InvalidOperationException("Term dates overlap another term in the same year.");

                    if (id > 0)
                        NonQuery(cn, tx, "UPDATE Terms SET AcademicYearID=@y, TermName=@n, StartDate=@s, EndDate=@e, Status=@st WHERE TermID=@id",
                            P("@y", academicYearId), P("@n", name), P("@s", start), P("@e", end), P("@st", status ?? "Active"), P("@id", id));
                    else
                        NonQuery(cn, tx, "INSERT INTO Terms (AcademicYearID,TermName,StartDate,EndDate,Status,IsCurrentTerm) VALUES (@y,@n,@s,@e,@st,0)",
                            P("@y", academicYearId), P("@n", name), P("@s", start), P("@e", end), P("@st", status ?? "Active"));
                    tx.Commit();
                }
            }
        }

        // ================================================================
        // CLASSES
        // ================================================================
        public DataTable GetClasses(int? academicYearId, string search, string status)
        {
            const string sql = @"
SELECT c.ClassID, c.ClassName, c.ClassCode, c.Level, c.Capacity, c.RoomNumber,
       ISNULL(c.Status,'Active') AS Status, c.AcademicYearID,
       (SELECT COUNT(*) FROM Sections s WHERE s.ClassID=c.ClassID) AS SectionCount,
       (SELECT COUNT(*) FROM Students st JOIN Sections s2 ON st.SectionID=s2.SectionID WHERE s2.ClassID=c.ClassID) AS StudentCount
FROM Classes c
WHERE (@yr IS NULL OR c.AcademicYearID=@yr OR c.AcademicYearID IS NULL)
  AND (@s='' OR c.ClassName LIKE '%'+@s+'%' OR c.ClassCode LIKE '%'+@s+'%')
  AND (@st='' OR ISNULL(c.Status,'Active')=@st)
ORDER BY c.ClassID;";
            return ExecuteDataTable(sql, new[]
            {
                P("@yr", (object)academicYearId ?? DBNull.Value),
                P("@s", search ?? ""), P("@st", status ?? "")
            });
        }

        public DataRow GetClass(int id)
        {
            DataTable t = ExecuteDataTable("SELECT * FROM Classes WHERE ClassID=@id", new[] { P("@id", id) });
            return t.Rows.Count > 0 ? t.Rows[0] : null;
        }

        public void SaveClass(int id, string name, string code, string level, int capacity, string room, int? academicYearId, string status)
        {
            name = (name ?? "").Trim(); code = (code ?? "").Trim();
            if (name.Length == 0) throw new ArgumentException("Class name is required.");
            if (code.Length == 0) throw new ArgumentException("Class code is required.");
            if (capacity <= 0) throw new ArgumentException("Capacity must be greater than zero.");

            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    if ((int)Scalar(cn, tx,
                            "SELECT COUNT(*) FROM Classes WHERE ClassCode=@c AND ISNULL(AcademicYearID,0)=ISNULL(@yr,0) AND ClassID<>@id",
                            P("@c", code), P("@yr", (object)academicYearId ?? DBNull.Value), P("@id", id)) > 0)
                        throw new InvalidOperationException("A class with this code already exists for the year.");

                    if (id > 0)
                        NonQuery(cn, tx, "UPDATE Classes SET ClassName=@n, ClassCode=@c, Level=@l, Capacity=@cap, RoomNumber=@r, AcademicYearID=@yr, Status=@st WHERE ClassID=@id",
                            P("@n", name), P("@c", code), P("@l", (object)level ?? DBNull.Value), P("@cap", capacity), P("@r", (object)room ?? DBNull.Value),
                            P("@yr", (object)academicYearId ?? DBNull.Value), P("@st", status ?? "Active"), P("@id", id));
                    else
                        NonQuery(cn, tx, "INSERT INTO Classes (ClassName, ClassCode, Level, Capacity, RoomNumber, AcademicYearID, Status, CreatedAt) VALUES (@n,@c,@l,@cap,@r,@yr,@st,GETDATE())",
                            P("@n", name), P("@c", code), P("@l", (object)level ?? DBNull.Value), P("@cap", capacity), P("@r", (object)room ?? DBNull.Value),
                            P("@yr", (object)academicYearId ?? DBNull.Value), P("@st", status ?? "Active"));
                    tx.Commit();
                }
            }
        }

        // ================================================================
        // SECTIONS
        // ================================================================
        public DataTable GetSections(int classId)
        {
            const string sql = @"
SELECT s.SectionID, s.SectionName, s.ClassID, s.StaffID, s.RoomNumber, s.Capacity,
       ISNULL(s.Status,'Active') AS Status, u.FullName AS TeacherName,
       (SELECT COUNT(*) FROM Students st WHERE st.SectionID=s.SectionID) AS StudentCount
FROM Sections s
LEFT JOIN Staff stf ON s.StaffID=stf.StaffID
LEFT JOIN Users u ON stf.UserID=u.UserID
WHERE s.ClassID=@c
ORDER BY s.SectionName;";
            return ExecuteDataTable(sql, new[] { P("@c", classId) });
        }

        public DataRow GetSection(int id)
        {
            DataTable t = ExecuteDataTable("SELECT * FROM Sections WHERE SectionID=@id", new[] { P("@id", id) });
            return t.Rows.Count > 0 ? t.Rows[0] : null;
        }

        public void SaveSection(int id, int classId, string name, int? staffId, string room, int capacity, int? academicYearId, string status)
        {
            name = (name ?? "").Trim();
            if (classId <= 0) throw new ArgumentException("Select a class.");
            if (name.Length == 0) throw new ArgumentException("Section name is required.");
            if (capacity <= 0) throw new ArgumentException("Capacity must be greater than zero.");

            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    if ((int)Scalar(cn, tx, "SELECT COUNT(*) FROM Sections WHERE ClassID=@c AND SectionName=@n AND SectionID<>@id",
                            P("@c", classId), P("@n", name), P("@id", id)) > 0)
                        throw new InvalidOperationException("A section with this name already exists in the class.");

                    if (staffId.HasValue && staffId.Value > 0 &&
                        (int)Scalar(cn, tx, "SELECT COUNT(*) FROM Staff WHERE StaffID=@t AND Status='Active'", P("@t", staffId.Value)) == 0)
                        throw new InvalidOperationException("The selected class teacher is not an active staff member.");

                    if (id > 0)
                        NonQuery(cn, tx, "UPDATE Sections SET ClassID=@c, SectionName=@n, StaffID=@t, RoomNumber=@r, Capacity=@cap, AcademicYearID=@yr, Status=@st WHERE SectionID=@id",
                            P("@c", classId), P("@n", name), P("@t", (object)(staffId > 0 ? staffId : null) ?? DBNull.Value), P("@r", (object)room ?? DBNull.Value),
                            P("@cap", capacity), P("@yr", (object)academicYearId ?? DBNull.Value), P("@st", status ?? "Active"), P("@id", id));
                    else
                        NonQuery(cn, tx, "INSERT INTO Sections (ClassID, SectionName, StaffID, RoomNumber, Capacity, AcademicYearID, Status) VALUES (@c,@n,@t,@r,@cap,@yr,@st)",
                            P("@c", classId), P("@n", name), P("@t", (object)(staffId > 0 ? staffId : null) ?? DBNull.Value), P("@r", (object)room ?? DBNull.Value),
                            P("@cap", capacity), P("@yr", (object)academicYearId ?? DBNull.Value), P("@st", status ?? "Active"));
                    tx.Commit();
                }
            }
        }

        // ================================================================
        // SUBJECTS
        // ================================================================
        public DataTable GetSubjects(string search, string type, string status, int? classId)
        {
            const string sql = @"
SELECT s.SubjectID, s.SubjectCode, s.SubjectName, s.SubjectType, s.MaxMarks, s.PassMarks,
       s.Description, s.IsActive,
       (SELECT COUNT(DISTINCT sec.ClassID) FROM ClassSubjectTeachers cst JOIN Sections sec ON cst.SectionID=sec.SectionID WHERE cst.SubjectID=s.SubjectID) AS ClassCount,
       (SELECT ISNULL(MAX(WeeklyPeriods),0) FROM ClassSubjectTeachers WHERE SubjectID=s.SubjectID) AS WeeklyPeriods
FROM Subjects s
WHERE (@s='' OR s.SubjectName LIKE '%'+@s+'%' OR s.SubjectCode LIKE '%'+@s+'%')
  AND (@t='' OR s.SubjectType=@t)
  AND (@st='' OR (CASE WHEN s.IsActive=1 THEN 'Active' ELSE 'Inactive' END)=@st)
  AND (@cl IS NULL OR EXISTS (SELECT 1 FROM ClassSubjectTeachers cst2 JOIN Sections sec2 ON cst2.SectionID=sec2.SectionID WHERE cst2.SubjectID=s.SubjectID AND sec2.ClassID=@cl))
ORDER BY s.SubjectCode;";
            return ExecuteDataTable(sql, new[]
            {
                P("@s", search ?? ""), P("@t", type ?? ""), P("@st", status ?? ""), P("@cl", (object)classId ?? DBNull.Value)
            });
        }

        public DataRow GetSubject(int id)
        {
            DataTable t = ExecuteDataTable("SELECT * FROM Subjects WHERE SubjectID=@id", new[] { P("@id", id) });
            return t.Rows.Count > 0 ? t.Rows[0] : null;
        }

        public void SaveSubject(int id, string code, string name, string type, int maxMarks, int passMarks, bool isActive, string description)
        {
            code = (code ?? "").Trim(); name = (name ?? "").Trim();
            if (code.Length == 0) throw new ArgumentException("Subject code is required.");
            if (name.Length == 0) throw new ArgumentException("Subject name is required.");
            if (maxMarks <= 0) throw new ArgumentException("Maximum marks must be greater than zero.");
            if (passMarks < 0) throw new ArgumentException("Pass marks cannot be negative.");
            if (passMarks > maxMarks) throw new ArgumentException("Pass marks cannot exceed maximum marks.");

            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    if ((int)Scalar(cn, tx, "SELECT COUNT(*) FROM Subjects WHERE SubjectCode=@c AND SubjectID<>@id",
                            P("@c", code), P("@id", id)) > 0)
                        throw new InvalidOperationException("A subject with this code already exists.");

                    if (id > 0)
                        NonQuery(cn, tx, "UPDATE Subjects SET SubjectCode=@c, SubjectName=@n, SubjectType=@t, MaxMarks=@mx, PassMarks=@ps, IsActive=@a, Description=@d WHERE SubjectID=@id",
                            P("@c", code), P("@n", name), P("@t", type ?? "Core"), P("@mx", maxMarks), P("@ps", passMarks), P("@a", isActive), P("@d", (object)description ?? DBNull.Value), P("@id", id));
                    else
                        NonQuery(cn, tx, "INSERT INTO Subjects (SubjectCode, SubjectName, SubjectType, MaxMarks, PassMarks, IsActive, Description) VALUES (@c,@n,@t,@mx,@ps,@a,@d)",
                            P("@c", code), P("@n", name), P("@t", type ?? "Core"), P("@mx", maxMarks), P("@ps", passMarks), P("@a", isActive), P("@d", (object)description ?? DBNull.Value));
                    tx.Commit();
                }
            }
        }

        // ================================================================
        // TEACHER ASSIGNMENTS (ClassSubjectTeachers)
        // ================================================================
        public DataTable GetTeacherAssignments(int? academicYearId, int? classId, int? sectionId, int? subjectId, int? staffId, string search)
        {
            const string sql = @"
SELECT cst.CSTID, u.FullName AS TeacherName, sub.SubjectName, c.ClassName, sec.SectionName,
       cst.WeeklyPeriods, cst.IsActive, cst.SectionID, cst.SubjectID, cst.StaffID, cst.AcademicYearID, c.ClassID
FROM ClassSubjectTeachers cst
JOIN Sections sec ON cst.SectionID=sec.SectionID
JOIN Classes c ON sec.ClassID=c.ClassID
JOIN Subjects sub ON cst.SubjectID=sub.SubjectID
JOIN Staff stf ON cst.StaffID=stf.StaffID
JOIN Users u ON stf.UserID=u.UserID
WHERE (@yr IS NULL OR cst.AcademicYearID=@yr)
  AND (@cl IS NULL OR c.ClassID=@cl)
  AND (@se IS NULL OR cst.SectionID=@se)
  AND (@su IS NULL OR cst.SubjectID=@su)
  AND (@stf IS NULL OR cst.StaffID=@stf)
  AND (@s='' OR u.FullName LIKE '%'+@s+'%' OR sub.SubjectName LIKE '%'+@s+'%')
ORDER BY u.FullName, sub.SubjectName;";
            return ExecuteDataTable(sql, new[]
            {
                P("@yr", (object)academicYearId ?? DBNull.Value), P("@cl", (object)classId ?? DBNull.Value),
                P("@se", (object)sectionId ?? DBNull.Value), P("@su", (object)subjectId ?? DBNull.Value),
                P("@stf", (object)staffId ?? DBNull.Value), P("@s", search ?? "")
            });
        }

        public DataRow GetTeacherAssignment(int id)
        {
            const string sql = @"
SELECT cst.CSTID, cst.SectionID, cst.SubjectID, cst.StaffID, cst.AcademicYearID, cst.WeeklyPeriods, cst.IsActive, sec.ClassID
FROM ClassSubjectTeachers cst JOIN Sections sec ON cst.SectionID=sec.SectionID
WHERE cst.CSTID=@id";
            DataTable t = ExecuteDataTable(sql, new[] { P("@id", id) });
            return t.Rows.Count > 0 ? t.Rows[0] : null;
        }

        /// <summary>Subjects already linked to a class (via Stage-3 class-subject rows), for the assign dropdown.</summary>
        public DataTable GetSubjectsForClass(int classId, int academicYearId)
        {
            const string sql = @"
SELECT DISTINCT sub.SubjectID, sub.SubjectName
FROM ClassSubjectTeachers cst
JOIN Sections sec ON cst.SectionID=sec.SectionID
JOIN Subjects sub ON cst.SubjectID=sub.SubjectID
WHERE sec.ClassID=@c AND cst.AcademicYearID=@y
ORDER BY sub.SubjectName;";
            return ExecuteDataTable(sql, new[] { P("@c", classId), P("@y", academicYearId) });
        }

        /// <summary>
        /// Assign / update the teacher on the class-subject row. Because Stage 3 created the
        /// (Section, Subject, Year) row (teacher null), this updates that row rather than inserting
        /// a duplicate. Rejects a subject that is not linked to the class.
        /// </summary>
        public void SaveTeacherAssignment(int sectionId, int subjectId, int staffId, int academicYearId, int weeklyPeriods, bool isActive)
        {
            if (sectionId <= 0 || subjectId <= 0 || staffId <= 0 || academicYearId <= 0)
                throw new ArgumentException("Class/Section, Subject, Teacher and Academic Year are required.");
            if (weeklyPeriods <= 0) throw new ArgumentException("Weekly periods must be greater than zero.");

            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    if ((int)Scalar(cn, tx, "SELECT COUNT(*) FROM Staff WHERE StaffID=@t AND Status='Active'", P("@t", staffId)) == 0)
                        throw new InvalidOperationException("The selected teacher is not active.");

                    string yStatus = Convert.ToString(Scalar(cn, tx, "SELECT Status FROM AcademicYears WHERE AcademicYearID=@y", P("@y", academicYearId)));
                    if (yStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) || yStatus.Equals("Closed", StringComparison.OrdinalIgnoreCase) || yStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Cannot assign to a completed or cancelled academic year.");

                    // Section must belong to the year (Stage-3 rows carry the year)
                    object existing = Scalar(cn, tx,
                        "SELECT CSTID FROM ClassSubjectTeachers WITH (UPDLOCK,HOLDLOCK) WHERE SectionID=@se AND SubjectID=@su AND AcademicYearID=@y",
                        P("@se", sectionId), P("@su", subjectId), P("@y", academicYearId));

                    if (existing == null || existing == DBNull.Value)
                        throw new InvalidOperationException("This subject is not assigned to the selected class. Assign it to the class under Subjects first.");

                    NonQuery(cn, tx, "UPDATE ClassSubjectTeachers SET StaffID=@t, WeeklyPeriods=@w, IsActive=@a WHERE CSTID=@id",
                        P("@t", staffId), P("@w", weeklyPeriods), P("@a", isActive), P("@id", Convert.ToInt32(existing)));
                    tx.Commit();
                }
            }
        }

        /// <summary>Remove a teacher from an assignment while preserving the class-subject link (StaffID = NULL).</summary>
        public void RemoveTeacherAssignment(int id)
        {
            ExecuteNonQuery("UPDATE ClassSubjectTeachers SET StaffID=NULL WHERE CSTID=@id", new[] { P("@id", id) });
        }

        // ================================================================
        // TIMETABLE
        // ================================================================
        /// <summary>Timetable rows for a section (weekly grid / list view).</summary>
        public DataTable GetTimetable(int sectionId, int? academicYearId, int? termId)
        {
            const string sql = @"
SELECT t.TimetableID, t.DayOfWeek, t.PeriodNo, t.StartTime, t.EndTime, t.RoomNumber,
       sub.SubjectName, u.FullName AS TeacherName, t.SubjectID, t.StaffID, t.SectionID, t.AcademicYearID, t.TermID,
       c.ClassName, sec.SectionName, tr.TermName
FROM Timetable t
JOIN Subjects sub ON t.SubjectID=sub.SubjectID
JOIN Staff stf ON t.StaffID=stf.StaffID
JOIN Users u ON stf.UserID=u.UserID
JOIN Sections sec ON t.SectionID=sec.SectionID
JOIN Classes c ON sec.ClassID=c.ClassID
LEFT JOIN Terms tr ON t.TermID=tr.TermID
WHERE t.SectionID=@se AND (@yr IS NULL OR t.AcademicYearID=@yr) AND (@tm IS NULL OR t.TermID=@tm OR t.TermID IS NULL) AND t.IsActive=1
ORDER BY t.DayOfWeek, t.StartTime;";
            return ExecuteDataTable(sql, new[]
            {
                P("@se", sectionId), P("@yr", (object)academicYearId ?? DBNull.Value), P("@tm", (object)termId ?? DBNull.Value)
            });
        }

        public DataRow GetTimetableEntry(int id)
        {
            DataTable t = ExecuteDataTable(
                "SELECT t.*, sec.ClassID FROM Timetable t JOIN Sections sec ON t.SectionID=sec.SectionID WHERE t.TimetableID=@id",
                new[] { P("@id", id) });
            return t.Rows.Count > 0 ? t.Rows[0] : null;
        }

        public void SaveTimetableEntry(int id, int sectionId, int subjectId, int staffId, int dayOfWeek, int periodNo,
            TimeSpan start, TimeSpan end, string room, int academicYearId, int? termId)
        {
            if (sectionId <= 0 || subjectId <= 0 || staffId <= 0 || academicYearId <= 0)
                throw new ArgumentException("Section, Subject, Teacher and Academic Year are required.");
            if (end <= start) throw new ArgumentException("Start time must be before end time.");

            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    if ((int)Scalar(cn, tx, "SELECT COUNT(*) FROM Staff WHERE StaffID=@t AND Status='Active'", P("@t", staffId)) == 0)
                        throw new InvalidOperationException("The selected teacher is not active.");

                    // Section belongs to a class; subject must be linked to that class; teacher must be the assigned teacher.
                    if ((int)Scalar(cn, tx, "SELECT COUNT(*) FROM ClassSubjectTeachers WHERE SectionID=@se AND SubjectID=@su AND AcademicYearID=@y",
                            P("@se", sectionId), P("@su", subjectId), P("@y", academicYearId)) == 0)
                        throw new InvalidOperationException("This subject is not assigned to the selected section. Assign it under Teacher Assignments first.");

                    // Term must belong to the academic year
                    if (termId.HasValue && termId.Value > 0)
                    {
                        object tYear = Scalar(cn, tx, "SELECT AcademicYearID FROM Terms WHERE TermID=@tm", P("@tm", termId.Value));
                        if (tYear == null || tYear == DBNull.Value || Convert.ToInt32(tYear) != academicYearId)
                            throw new InvalidOperationException("The selected term does not belong to the academic year.");
                    }

                    // ---- overlap-based conflict checks (NewStart < ExistingEnd AND NewEnd > ExistingStart) ----
                    string overlap = " AND DayOfWeek=@d AND AcademicYearID=@y AND IsActive=1 AND TimetableID<>@id AND @s < EndTime AND @e > StartTime";

                    if ((int)Scalar(cn, tx, "SELECT COUNT(*) FROM Timetable WITH (UPDLOCK,HOLDLOCK) WHERE SectionID=@se" + overlap,
                            P("@se", sectionId), P("@d", dayOfWeek), P("@y", academicYearId), P("@id", id), P("@s", start), P("@e", end)) > 0)
                        throw new InvalidOperationException("Section already has another lesson at this time.");

                    if ((int)Scalar(cn, tx, "SELECT COUNT(*) FROM Timetable WITH (UPDLOCK,HOLDLOCK) WHERE StaffID=@t" + overlap,
                            P("@t", staffId), P("@d", dayOfWeek), P("@y", academicYearId), P("@id", id), P("@s", start), P("@e", end)) > 0)
                        throw new InvalidOperationException("Teacher already has another lesson at this time.");

                    if (!string.IsNullOrWhiteSpace(room) &&
                        (int)Scalar(cn, tx, "SELECT COUNT(*) FROM Timetable WITH (UPDLOCK,HOLDLOCK) WHERE RoomNumber=@r" + overlap,
                            P("@r", room.Trim()), P("@d", dayOfWeek), P("@y", academicYearId), P("@id", id), P("@s", start), P("@e", end)) > 0)
                        throw new InvalidOperationException("Room is already booked at this time.");

                    if (id > 0)
                        NonQuery(cn, tx, "UPDATE Timetable SET SectionID=@se, SubjectID=@su, StaffID=@t, DayOfWeek=@d, PeriodNo=@p, StartTime=@s, EndTime=@e, RoomNumber=@r, AcademicYearID=@y, TermID=@tm WHERE TimetableID=@id",
                            P("@se", sectionId), P("@su", subjectId), P("@t", staffId), P("@d", dayOfWeek), P("@p", periodNo), P("@s", start), P("@e", end), P("@r", (object)room ?? DBNull.Value), P("@y", academicYearId), P("@tm", (object)(termId > 0 ? termId : null) ?? DBNull.Value), P("@id", id));
                    else
                        NonQuery(cn, tx, "INSERT INTO Timetable (SectionID, SubjectID, StaffID, DayOfWeek, PeriodNo, StartTime, EndTime, RoomNumber, AcademicYearID, TermID, IsActive) VALUES (@se,@su,@t,@d,@p,@s,@e,@r,@y,@tm,1)",
                            P("@se", sectionId), P("@su", subjectId), P("@t", staffId), P("@d", dayOfWeek), P("@p", periodNo), P("@s", start), P("@e", end), P("@r", (object)room ?? DBNull.Value), P("@y", academicYearId), P("@tm", (object)(termId > 0 ? termId : null) ?? DBNull.Value));
                    tx.Commit();
                }
            }
        }

        public void DeleteTimetableEntry(int id)
        {
            ExecuteNonQuery("UPDATE Timetable SET IsActive=0 WHERE TimetableID=@id", new[] { P("@id", id) });
        }

        /// <summary>Subjects assigned to a section WITH a teacher (used to build timetable entries).</summary>
        public DataTable GetSectionAssignedSubjects(int sectionId, int academicYearId)
        {
            const string sql = @"
SELECT cst.SubjectID, sub.SubjectName, cst.StaffID, u.FullName AS TeacherName
FROM ClassSubjectTeachers cst
JOIN Subjects sub ON cst.SubjectID=sub.SubjectID
JOIN Staff stf ON cst.StaffID=stf.StaffID
JOIN Users u ON stf.UserID=u.UserID
WHERE cst.SectionID=@se AND cst.AcademicYearID=@y AND cst.StaffID IS NOT NULL AND cst.IsActive=1
ORDER BY sub.SubjectName;";
            return ExecuteDataTable(sql, new[] { P("@se", sectionId), P("@y", academicYearId) });
        }

        // ================================================================
        // PROMOTIONS
        // ================================================================
        public DataTable GetPromotionCandidates(int fromYearId, int toYearId, int? classId, int? sectionId, string search)
        {
            const string sql = @"
SELECT st.StudentID, st.StudentCode, st.FullName, c.ClassName AS CurrentClass, sec.SectionName AS CurrentSection,
       st.SectionID AS CurrentSectionID, c.ClassID AS CurrentClassID,
       p.Status AS PromotionStatus, p.ActionDate, p.ToSectionID
FROM Students st
JOIN Sections sec ON st.SectionID=sec.SectionID
JOIN Classes c ON sec.ClassID=c.ClassID
-- Latest promotion row for the target year only (students may now have several
-- same-year placement-history rows; this keeps exactly one candidate row each).
OUTER APPLY (SELECT TOP 1 pp.Status, pp.ActionDate, pp.ToSectionID
             FROM StudentPromotions pp
             WHERE pp.StudentID=st.StudentID AND pp.ToAcademicYearID=@to
             ORDER BY pp.ActionDate DESC, pp.CreatedAt DESC, pp.PromotionID DESC) p
WHERE st.AcademicYearID=@from
  AND (@cl IS NULL OR c.ClassID=@cl)
  AND (@se IS NULL OR st.SectionID=@se)
  AND (@s='' OR st.FullName LIKE '%'+@s+'%' OR st.StudentCode LIKE '%'+@s+'%')
ORDER BY c.ClassID, sec.SectionName, st.FullName;";
            return ExecuteDataTable(sql, new[]
            {
                P("@from", fromYearId), P("@to", toYearId),
                P("@cl", (object)classId ?? DBNull.Value), P("@se", (object)sectionId ?? DBNull.Value),
                P("@s", search ?? "")
            });
        }

        public class PromotionItem
        {
            public int StudentID;
            public int? ToSectionID;
            public string Status; // Promoted / Repeated / Graduated / Transferred / Withdrawn / NotEligible
            public string Notes;
        }

        /// <summary>
        /// Bulk promotion in ONE transaction (Serializable). Preserves history in StudentPromotions,
        /// updates the student's current enrolment only for Promoted/Repeated, and rolls the whole
        /// batch back if any selected student fails validation.
        /// </summary>
        public void PromoteStudents(int fromYearId, int toYearId, IList<PromotionItem> items, int? promotedBy)
        {
            if (fromYearId <= 0 || toYearId <= 0) throw new ArgumentException("Both academic years are required.");
            if (fromYearId == toYearId) throw new ArgumentException("The target year must differ from the source year.");
            if (items == null || items.Count == 0) throw new ArgumentException("No students selected.");

            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    // target year must exist and not be completed/cancelled
                    string toStatus = Convert.ToString(Scalar(cn, tx, "SELECT Status FROM AcademicYears WHERE AcademicYearID=@to", P("@to", toYearId)));
                    if (string.IsNullOrEmpty(toStatus)) throw new InvalidOperationException("The target academic year does not exist.");
                    if (toStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Cannot promote into a cancelled academic year.");

                    foreach (PromotionItem it in items)
                    {
                        string status = string.IsNullOrWhiteSpace(it.Status) ? "Promoted" : it.Status.Trim();

                        // lock + reload the student's current row; must belong to the source year
                        object fromSecObj = Scalar(cn, tx,
                            "SELECT SectionID FROM Students WITH (UPDLOCK, HOLDLOCK) WHERE StudentID=@sid AND AcademicYearID=@from",
                            P("@sid", it.StudentID), P("@from", fromYearId));
                        if (fromSecObj == null || fromSecObj == DBNull.Value)
                            throw new InvalidOperationException("A selected student does not belong to the source academic year.");

                        // not already promoted to this target year
                        if ((int)Scalar(cn, tx, "SELECT COUNT(*) FROM StudentPromotions WITH (UPDLOCK, HOLDLOCK) WHERE StudentID=@sid AND ToAcademicYearID=@to",
                                P("@sid", it.StudentID), P("@to", toYearId)) > 0)
                            throw new InvalidOperationException("A selected student has already been promoted to the target year.");

                        int? toSection = (it.ToSectionID.HasValue && it.ToSectionID.Value > 0) ? it.ToSectionID : (int?)null;

                        if (status == "Promoted" || status == "Repeated")
                        {
                            if (!toSection.HasValue)
                                throw new InvalidOperationException("A target section is required for promoted/repeated students.");
                            // section must exist AND belong to the target year (section carries its academic year)
                            object secYear = Scalar(cn, tx, "SELECT AcademicYearID FROM Sections WHERE SectionID=@se", P("@se", toSection.Value));
                            if (secYear == null) throw new InvalidOperationException("An invalid target section was selected.");
                            if (secYear != DBNull.Value && Convert.ToInt32(secYear) != toYearId)
                                throw new InvalidOperationException("The target section does not belong to the target academic year.");
                        }

                        // history row (preserves From year/section, To year/section, status, who, when, notes)
                        NonQuery(cn, tx, "INSERT INTO StudentPromotions (StudentID, FromAcademicYearID, ToAcademicYearID, FromSectionID, ToSectionID, Status, ActionDate, PromotedBy, Notes) VALUES (@sid,@from,@to,@fs,@ts,@st,GETDATE(),@by,@n)",
                            P("@sid", it.StudentID), P("@from", fromYearId), P("@to", toYearId),
                            P("@fs", fromSecObj), P("@ts", (object)toSection ?? DBNull.Value), P("@st", status),
                            P("@by", (object)promotedBy ?? DBNull.Value), P("@n", (object)it.Notes ?? DBNull.Value));

                        // update current enrolment ONLY for Promoted/Repeated; Graduated flags status;
                        // Transferred / Withdrawn / NotEligible keep history only (no enrolment change).
                        if (status == "Promoted" || status == "Repeated")
                            NonQuery(cn, tx, "UPDATE Students SET SectionID=@ts, AcademicYearID=@to, UpdatedAt=GETDATE() WHERE StudentID=@sid",
                                P("@ts", toSection.Value), P("@to", toYearId), P("@sid", it.StudentID));
                        else if (status == "Graduated")
                            NonQuery(cn, tx, "UPDATE Students SET Status='Graduated', UpdatedAt=GETDATE() WHERE StudentID=@sid", P("@sid", it.StudentID));
                        else if (status == "Withdrawn")
                            NonQuery(cn, tx, "UPDATE Students SET Status='Withdrawn', UpdatedAt=GETDATE() WHERE StudentID=@sid", P("@sid", it.StudentID));
                        else if (status == "Transferred")
                            NonQuery(cn, tx, "UPDATE Students SET Status='Transferred', UpdatedAt=GETDATE() WHERE StudentID=@sid", P("@sid", it.StudentID));
                    }
                    tx.Commit();
                }
            }
        }

        /// <summary>Status counts for the summary cards.</summary>
        public DataRow GetPromotionSummary(int fromYearId, int toYearId, int? classId, int? sectionId)
        {
            const string sql = @"
SELECT
  (SELECT COUNT(*) FROM Students st JOIN Sections sec ON st.SectionID=sec.SectionID
      WHERE st.AcademicYearID=@from AND (@cl IS NULL OR sec.ClassID=@cl) AND (@se IS NULL OR st.SectionID=@se)) AS TotalStudents,
  (SELECT COUNT(*) FROM StudentPromotions WHERE ToAcademicYearID=@to AND Status='Promoted') AS Promoted,
  (SELECT COUNT(*) FROM StudentPromotions WHERE ToAcademicYearID=@to AND Status='Repeated') AS Repeated,
  (SELECT COUNT(*) FROM StudentPromotions WHERE ToAcademicYearID=@to AND Status='Graduated') AS Graduated,
  (SELECT COUNT(*) FROM StudentPromotions WHERE ToAcademicYearID=@to AND Status='Transferred') AS Transferred,
  (SELECT COUNT(*) FROM StudentPromotions WHERE ToAcademicYearID=@to AND Status='Withdrawn') AS Withdrawn,
  (SELECT COUNT(*) FROM StudentPromotions WHERE ToAcademicYearID=@to) AS Processed;";
            DataTable t = ExecuteDataTable(sql, new[]
            {
                P("@from", fromYearId), P("@to", toYearId),
                P("@cl", (object)classId ?? DBNull.Value), P("@se", (object)sectionId ?? DBNull.Value)
            });
            return t.Rows[0];
        }

        public DataTable GetTargetSections(int classId) { return GetSectionsLookup(classId); }

        // ================================================================
        // STATUS CHANGES & GUARDED DELETES
        // ================================================================
        public void SetClassStatus(int id, string status)
        {
            ExecuteNonQuery("UPDATE Classes SET Status=@st WHERE ClassID=@id",
                new[] { P("@st", status ?? "Active"), P("@id", id) });
        }

        public void SetSectionStatus(int id, string status)
        {
            ExecuteNonQuery("UPDATE Sections SET Status=@st WHERE SectionID=@id",
                new[] { P("@st", status ?? "Active"), P("@id", id) });
        }

        public void SetSubjectActive(int id, bool active)
        {
            ExecuteNonQuery("UPDATE Subjects SET IsActive=@a WHERE SubjectID=@id",
                new[] { P("@a", active), P("@id", id) });
        }

        /// <summary>True if the class has sections, students, assignments or timetable rows.</summary>
        public bool ClassHasReferences(int classId)
        {
            object o = ExecuteScalar(@"
SELECT CASE WHEN
   EXISTS (SELECT 1 FROM Sections WHERE ClassID=@c)
OR EXISTS (SELECT 1 FROM Students st JOIN Sections s ON st.SectionID=s.SectionID WHERE s.ClassID=@c)
THEN 1 ELSE 0 END", new[] { P("@c", classId) });
            return Convert.ToInt32(o) == 1;
        }

        public bool SectionHasStudents(int sectionId)
        {
            return Convert.ToInt32(ExecuteScalar("SELECT COUNT(*) FROM Students WHERE SectionID=@s", new[] { P("@s", sectionId) })) > 0;
        }

        public bool SubjectHasReferences(int subjectId)
        {
            object o = ExecuteScalar(@"
SELECT CASE WHEN
   EXISTS (SELECT 1 FROM ClassSubjectTeachers WHERE SubjectID=@s)
OR EXISTS (SELECT 1 FROM Timetable WHERE SubjectID=@s)
THEN 1 ELSE 0 END", new[] { P("@s", subjectId) });
            return Convert.ToInt32(o) == 1;
        }

        // ================================================================
        // CLASS-SUBJECT ASSIGNMENT (via ClassSubjectTeachers, teacher optional)
        // ================================================================
        /// <summary>Classes a subject is assigned to (distinct), with weekly-period totals.</summary>
        public DataTable GetSubjectClasses(int subjectId, int? academicYearId)
        {
            const string sql = @"
SELECT c.ClassID, c.ClassName, MAX(cst.AcademicYearID) AS AcademicYearID,
       MAX(cst.WeeklyPeriods) AS WeeklyPeriods, COUNT(*) AS SectionsCovered
FROM ClassSubjectTeachers cst
JOIN Sections sec ON cst.SectionID=sec.SectionID
JOIN Classes c ON sec.ClassID=c.ClassID
WHERE cst.SubjectID=@su AND (@yr IS NULL OR cst.AcademicYearID=@yr)
GROUP BY c.ClassID, c.ClassName
ORDER BY c.ClassID;";
            return ExecuteDataTable(sql, new[] { P("@su", subjectId), P("@yr", (object)academicYearId ?? DBNull.Value) });
        }

        /// <summary>
        /// Assign a subject to every section of a class for a year (teacher left null,
        /// to be set later in Teacher Assignments). Idempotent per section.
        /// </summary>
        public void AssignSubjectToClass(int subjectId, int classId, int academicYearId, int weeklyPeriods)
        {
            if (subjectId <= 0 || classId <= 0 || academicYearId <= 0)
                throw new ArgumentException("Subject, Class and Academic Year are required.");
            if (weeklyPeriods <= 0) throw new ArgumentException("Weekly periods must be greater than zero.");

            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    DataTable sections = new DataTable();
                    using (SqlCommand cmd = new SqlCommand("SELECT SectionID FROM Sections WHERE ClassID=@c", cn, tx))
                    {
                        cmd.Parameters.AddRange(new[] { P("@c", classId) });
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(sections);
                    }
                    if (sections.Rows.Count == 0)
                        throw new InvalidOperationException("This class has no sections yet. Add a section first.");

                    foreach (DataRow r in sections.Rows)
                    {
                        int sectionId = Convert.ToInt32(r["SectionID"]);
                        // update if exists, else insert (teacher untouched)
                        int updated = Convert.ToInt32(Scalar(cn, tx,
                            "UPDATE ClassSubjectTeachers SET WeeklyPeriods=@w, IsActive=1 WHERE SectionID=@se AND SubjectID=@su AND AcademicYearID=@y; SELECT @@ROWCOUNT",
                            P("@w", weeklyPeriods), P("@se", sectionId), P("@su", subjectId), P("@y", academicYearId)));
                        if (updated == 0)
                            NonQuery(cn, tx,
                                "INSERT INTO ClassSubjectTeachers (SectionID, SubjectID, StaffID, AcademicYearID, WeeklyPeriods, IsActive) VALUES (@se,@su,NULL,@y,@w,1)",
                                P("@se", sectionId), P("@su", subjectId), P("@y", academicYearId), P("@w", weeklyPeriods));
                    }
                    tx.Commit();
                }
            }
        }

        public void RemoveSubjectFromClass(int subjectId, int classId, int academicYearId)
        {
            ExecuteNonQuery(@"
DELETE cst FROM ClassSubjectTeachers cst
JOIN Sections sec ON cst.SectionID=sec.SectionID
WHERE cst.SubjectID=@su AND sec.ClassID=@c AND cst.AcademicYearID=@y AND cst.StaffID IS NULL",
                new[] { P("@su", subjectId), P("@c", classId), P("@y", academicYearId) });
        }

        // ================================================================
        // LOOKUPS
        // ================================================================
        public DataTable GetAcademicYearsLookup()
        {
            return ExecuteDataTable("SELECT AcademicYearID, YearName, Status FROM AcademicYears ORDER BY StartDate DESC", null);
        }

        public int GetActiveAcademicYearId()
        {
            object o = ExecuteScalar("SELECT TOP 1 AcademicYearID FROM AcademicYears WHERE Status='Active' ORDER BY AcademicYearID DESC", null);
            return o == null || o == DBNull.Value ? 0 : Convert.ToInt32(o);
        }

        public DataTable GetTermsLookup(int academicYearId)
        {
            return ExecuteDataTable("SELECT TermID, TermName FROM Terms WHERE AcademicYearID=@y ORDER BY StartDate", new[] { P("@y", academicYearId) });
        }

        public DataTable GetClassesLookup()
        {
            return ExecuteDataTable("SELECT ClassID, ClassName FROM Classes WHERE ISNULL(Status,'Active')='Active' ORDER BY ClassID", null);
        }

        public DataTable GetSectionsLookup(int classId)
        {
            return ExecuteDataTable("SELECT SectionID, SectionName FROM Sections WHERE ClassID=@c ORDER BY SectionName", new[] { P("@c", classId) });
        }

        public DataTable GetSubjectsLookup()
        {
            return ExecuteDataTable("SELECT SubjectID, SubjectName FROM Subjects WHERE IsActive=1 ORDER BY SubjectName", null);
        }

        public DataTable GetActiveTeachers()
        {
            return ExecuteDataTable(
                "SELECT s.StaffID, u.FullName FROM Staff s JOIN Users u ON s.UserID=u.UserID WHERE s.Status='Active' AND u.Role='Teacher' ORDER BY u.FullName", null);
        }

        public string GetStaffName(int staffId)
        {
            object o = ExecuteScalar("SELECT u.FullName FROM Staff s JOIN Users u ON s.UserID=u.UserID WHERE s.StaffID=@id", new[] { P("@id", staffId) });
            return o == null || o == DBNull.Value ? "" : Convert.ToString(o);
        }

        /// <summary>All active staff (used for the class-teacher picker).</summary>
        public DataTable GetActiveStaff()
        {
            return ExecuteDataTable(
                "SELECT s.StaffID, u.FullName FROM Staff s JOIN Users u ON s.UserID=u.UserID WHERE s.Status='Active' ORDER BY u.FullName", null);
        }

        // ================================================================
        // ADO helpers
        // ================================================================
        private SqlConnection CreateConnection() { return new SqlConnection(_connectionString); }

        private static SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

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
