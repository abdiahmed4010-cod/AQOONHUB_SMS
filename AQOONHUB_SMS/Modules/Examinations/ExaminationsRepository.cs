using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AQOONHUB_SMS.Modules.Examinations
{
    /// <summary>
    /// Single data-access layer for the whole Examinations module
    /// (Exams, ExamSubjects, ExamSchedules, ExamRooms, ExamResults,
    /// GradingScale, ResultPublications). Direct ADO.NET, parameterised.
    /// </summary>
    public sealed class ExaminationsRepository
    {
        private readonly string _connectionString;

        public ExaminationsRepository()
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

        /// <summary>Create/schedule exams, manage grading, publish results.</summary>
        public bool CanManage(string role)
        {
            string r = NormalizeRole(role);
            return r == "superadmin" || r == "admin" || r == "academic" || r == "examofficer";
        }

        /// <summary>Enter marks (managers plus teachers).</summary>
        public bool CanEnterMarks(string role)
        {
            return CanManage(role) || NormalizeRole(role) == "teacher";
        }

        /// <summary>View examination pages.</summary>
        public bool CanView(string role)
        {
            string r = NormalizeRole(role);
            return CanEnterMarks(role) || r == "registrar";
        }

        // ================================================================
        // SUMMARY / OVERVIEW
        // ================================================================
        public DataRow GetSummary(int? academicYearId, int? termId)
        {
            const string sql = @"
SELECT
  (SELECT COUNT(*) FROM Exams e WHERE (@yr IS NULL OR e.AcademicYearID=@yr) AND (@tm IS NULL OR e.TermID=@tm)) AS TotalExams,
  (SELECT COUNT(*) FROM Exams e WHERE (@yr IS NULL OR e.AcademicYearID=@yr) AND (@tm IS NULL OR e.TermID=@tm)
       AND e.StartDate >= CAST(GETDATE() AS date) AND ISNULL(e.Status,'') NOT IN ('Cancelled','Completed','Published')) AS UpcomingExams,
  (SELECT COUNT(*) FROM Exams e WHERE (@yr IS NULL OR e.AcademicYearID=@yr) AND (@tm IS NULL OR e.TermID=@tm)
       AND ISNULL(e.Status,'') IN ('Completed','Published')) AS CompletedExams,
  (SELECT COUNT(*) FROM Exams e WHERE (@yr IS NULL OR e.AcademicYearID=@yr) AND (@tm IS NULL OR e.TermID=@tm)
       AND ISNULL(e.Status,'') IN ('Ongoing','Completed','Scheduled')
       AND ISNULL(e.Status,'') <> 'Published') AS PendingMarkEntry,
  (SELECT COUNT(*) FROM Exams e WHERE (@yr IS NULL OR e.AcademicYearID=@yr) AND (@tm IS NULL OR e.TermID=@tm)
       AND ISNULL(e.Status,'')='Published') AS ResultsPublished;";
            DataTable t = ExecuteDataTable(sql, new[] { P("@yr", (object)academicYearId ?? DBNull.Value), P("@tm", (object)termId ?? DBNull.Value) });
            return t.Rows[0];
        }

        /// <summary>Recent exam activity derived from real Exams rows (no fabricated audit log).</summary>
        public DataTable GetExamActivities(int top)
        {
            string sql = @"
SELECT TOP (" + (top > 0 ? top : 8) + @") e.ExamName, e.Status,
       COALESCE(e.PublishedAt, e.UpdatedAt, e.CreatedAt) AS ActivityDate,
       CASE WHEN e.Status='Published' THEN 'Results published'
            WHEN e.Status='Completed' THEN 'Examination completed'
            WHEN e.Status='Ongoing' THEN 'Examination ongoing'
            ELSE 'Examination created' END AS Activity
FROM Exams e
ORDER BY COALESCE(e.PublishedAt, e.UpdatedAt, e.CreatedAt) DESC;";
            return ExecuteDataTable(sql, null);
        }

        public DataTable GetUpcomingExamsList(int? academicYearId, int? termId, int top)
        {
            string sql = @"
SELECT TOP (" + (top > 0 ? top : 6) + @") e.ExamID, e.ExamName, e.ExamType, e.StartDate, e.EndDate, e.Status, t.TermName
FROM Exams e JOIN Terms t ON e.TermID=t.TermID
WHERE (@yr IS NULL OR e.AcademicYearID=@yr) AND (@tm IS NULL OR e.TermID=@tm)
  AND e.EndDate >= CAST(GETDATE() AS date) AND ISNULL(e.Status,'') NOT IN ('Cancelled')
ORDER BY e.StartDate;";
            return ExecuteDataTable(sql, new[] { P("@yr", (object)academicYearId ?? DBNull.Value), P("@tm", (object)termId ?? DBNull.Value) });
        }

        // ================================================================
        // EXAMINATIONS (list — full CRUD arrives in Stage 2)
        // ================================================================
        public DataTable GetExaminations(int? academicYearId, int? termId, int? classId, string status, string search)
        {
            const string sql = @"
SELECT e.ExamID, e.ExamName, e.ExamType, e.TermID, t.TermName, e.AcademicYearID, y.YearName,
       e.StartDate, e.EndDate, ISNULL(e.Status,'Draft') AS Status, e.TotalMarks, e.PassingMark, e.Weight,
       (SELECT COUNT(DISTINCT ClassID) FROM ExamClasses ec WHERE ec.ExamID=e.ExamID) AS ClassCount,
       (SELECT COUNT(*) FROM ExamSubjects es WHERE es.ExamID=e.ExamID) AS SubjectCount
FROM Exams e
JOIN Terms t ON e.TermID=t.TermID
LEFT JOIN AcademicYears y ON e.AcademicYearID=y.AcademicYearID
WHERE (@yr IS NULL OR e.AcademicYearID=@yr)
  AND (@tm IS NULL OR e.TermID=@tm)
  AND (@cl IS NULL OR EXISTS (SELECT 1 FROM ExamClasses ec WHERE ec.ExamID=e.ExamID AND ec.ClassID=@cl))
  AND (@st='' OR ISNULL(e.Status,'Draft')=@st)
  AND (@s='' OR e.ExamName LIKE '%'+@s+'%')
ORDER BY e.StartDate DESC;";
            return ExecuteDataTable(sql, new[]
            {
                P("@yr", (object)academicYearId ?? DBNull.Value), P("@tm", (object)termId ?? DBNull.Value),
                P("@cl", (object)classId ?? DBNull.Value), P("@st", status ?? ""), P("@s", search ?? "")
            });
        }

        public DataRow GetExamination(int id)
        {
            const string sql = @"
SELECT e.*, t.TermName, y.YearName, u.FullName AS CreatedByName
FROM Exams e
JOIN Terms t ON e.TermID=t.TermID
LEFT JOIN AcademicYears y ON e.AcademicYearID=y.AcademicYearID
LEFT JOIN Users u ON e.CreatedBy=u.UserID
WHERE e.ExamID=@id";
            DataTable dt = ExecuteDataTable(sql, new[] { P("@id", id) });
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public DataTable GetExamClasses(int examId)
        {
            const string sql = @"
SELECT ec.ExamClassID, ec.ClassID, ec.SectionID, c.ClassName, s.SectionName
FROM ExamClasses ec
JOIN Classes c ON ec.ClassID=c.ClassID
LEFT JOIN Sections s ON ec.SectionID=s.SectionID
WHERE ec.ExamID=@id ORDER BY c.ClassID";
            return ExecuteDataTable(sql, new[] { P("@id", examId) });
        }

        public DataTable GetExamSubjects(int examId)
        {
            const string sql = @"
SELECT es.ExamSubjectID, es.ClassID, es.SectionID, es.SubjectID, es.TotalMarks, es.PassingMark, sub.SubjectName
FROM ExamSubjects es JOIN Subjects sub ON es.SubjectID=sub.SubjectID
WHERE es.ExamID=@id ORDER BY sub.SubjectName";
            return ExecuteDataTable(sql, new[] { P("@id", examId) });
        }

        /// <summary>Subjects assigned to a class through ClassSubjectTeachers (real junction).</summary>
        public DataTable GetSubjectsForClass(int classId, int academicYearId)
        {
            const string sql = @"
SELECT DISTINCT sub.SubjectID, sub.SubjectName
FROM ClassSubjectTeachers cst
JOIN Sections sec ON cst.SectionID=sec.SectionID
JOIN Subjects sub ON cst.SubjectID=sub.SubjectID
WHERE sec.ClassID=@c AND cst.AcademicYearID=@y
ORDER BY sub.SubjectName";
            return ExecuteDataTable(sql, new[] { P("@c", classId), P("@y", academicYearId) });
        }

        public DataTable GetSectionsForClass(int classId)
        {
            return ExecuteDataTable("SELECT SectionID, SectionName FROM Sections WHERE ClassID=@c ORDER BY SectionName", new[] { P("@c", classId) });
        }

        /// <summary>True if the exam has schedules, marks or publications (blocks edit/delete).</summary>
        public bool ExaminationHasDependencies(int examId)
        {
            object o = ExecuteScalar(@"
SELECT CASE WHEN
   EXISTS (SELECT 1 FROM ExamSchedules WHERE ExamID=@id)
OR EXISTS (SELECT 1 FROM ExamResults WHERE ExamID=@id)
OR EXISTS (SELECT 1 FROM ResultPublications WHERE ExamID=@id)
THEN 1 ELSE 0 END", new[] { P("@id", examId) });
            return Convert.ToInt32(o) == 1;
        }

        /// <summary>
        /// Create or update an examination and its class/subject scope in one transaction.
        /// Returns the ExamID. Editing is only permitted for Draft/Active exams without dependencies.
        /// </summary>
        public int SaveExamination(int examId, string name, int academicYearId, int termId, string examType,
            DateTime start, DateTime end, int classId, int? sectionId, IList<int> subjectIds,
            int passingMark, int totalMarks, int weight, string status, int? currentUserId)
        {
            name = (name ?? "").Trim();
            examType = (examType ?? "").Trim();
            if (name.Length == 0) throw new ArgumentException("Exam name is required.");
            if (academicYearId <= 0) throw new ArgumentException("Academic year is required.");
            if (termId <= 0) throw new ArgumentException("Term is required.");
            if (examType.Length == 0) throw new ArgumentException("Exam type is required.");
            if (end < start) throw new ArgumentException("End date cannot be before the start date.");
            if (classId <= 0) throw new ArgumentException("Class is required.");
            if (subjectIds == null || subjectIds.Count == 0) throw new ArgumentException("Select at least one subject.");
            if (passingMark < 0 || passingMark > 100) throw new ArgumentException("Passing mark must be between 0 and 100.");
            if (totalMarks <= 0) throw new ArgumentException("Total marks must be greater than zero.");
            if (weight < 0 || weight > 100) throw new ArgumentException("Weight must be between 0 and 100.");

            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    // Academic year exists and not cancelled
                    string yStatus = Convert.ToString(Scalar(cn, tx, "SELECT Status FROM AcademicYears WHERE AcademicYearID=@y", P("@y", academicYearId)));
                    if (string.IsNullOrEmpty(yStatus)) throw new InvalidOperationException("The academic year does not exist.");
                    if (yStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Cannot create an exam in a cancelled academic year.");

                    // Year date bounds
                    DataTable yr = new DataTable();
                    using (SqlCommand c = new SqlCommand("SELECT StartDate, EndDate FROM AcademicYears WHERE AcademicYearID=@y", cn, tx))
                    { c.Parameters.Add(P("@y", academicYearId)); using (SqlDataAdapter da = new SqlDataAdapter(c)) da.Fill(yr); }
                    DateTime ys = Convert.ToDateTime(yr.Rows[0]["StartDate"]), ye = Convert.ToDateTime(yr.Rows[0]["EndDate"]);
                    if (start < ys || end > ye) throw new InvalidOperationException("Exam dates must fall within the academic year.");

                    // Term belongs to year
                    object termYear = Scalar(cn, tx, "SELECT AcademicYearID FROM Terms WHERE TermID=@t", P("@t", termId));
                    if (termYear == null || Convert.ToInt32(termYear) != academicYearId)
                        throw new InvalidOperationException("The selected term does not belong to the academic year.");

                    // Section belongs to class
                    if (sectionId.HasValue && sectionId.Value > 0)
                    {
                        object secClass = Scalar(cn, tx, "SELECT ClassID FROM Sections WHERE SectionID=@s", P("@s", sectionId.Value));
                        if (secClass == null || Convert.ToInt32(secClass) != classId)
                            throw new InvalidOperationException("The selected section does not belong to the class.");
                    }

                    // Every subject belongs to the class (via CST)
                    foreach (int sid in subjectIds)
                    {
                        int ok = (int)Scalar(cn, tx,
                            "SELECT COUNT(*) FROM ClassSubjectTeachers cst JOIN Sections sec ON cst.SectionID=sec.SectionID WHERE sec.ClassID=@c AND cst.SubjectID=@su AND cst.AcademicYearID=@y",
                            P("@c", classId), P("@su", sid), P("@y", academicYearId));
                        if (ok == 0) throw new InvalidOperationException("A selected subject is not assigned to the class.");
                    }

                    // Duplicate name within year + term
                    if ((int)Scalar(cn, tx, "SELECT COUNT(*) FROM Exams WHERE ExamName=@n AND AcademicYearID=@y AND TermID=@t AND ExamID<>@id",
                            P("@n", name), P("@y", academicYearId), P("@t", termId), P("@id", examId)) > 0)
                        throw new InvalidOperationException("An exam with this name already exists for the year and term.");

                    if (examId > 0)
                    {
                        // Editing allowed only for Draft/Active and no dependencies
                        string cur = Convert.ToString(Scalar(cn, tx, "SELECT Status FROM Exams WITH (UPDLOCK,HOLDLOCK) WHERE ExamID=@id", P("@id", examId)));
                        if (!cur.Equals("Draft", StringComparison.OrdinalIgnoreCase) && !cur.Equals("Active", StringComparison.OrdinalIgnoreCase))
                            throw new InvalidOperationException("Only Draft or Active exams can be edited.");
                        if ((int)Scalar(cn, tx, "SELECT CASE WHEN EXISTS(SELECT 1 FROM ExamSchedules WHERE ExamID=@id) OR EXISTS(SELECT 1 FROM ExamResults WHERE ExamID=@id) OR EXISTS(SELECT 1 FROM ResultPublications WHERE ExamID=@id) THEN 1 ELSE 0 END", P("@id", examId)) == 1)
                            throw new InvalidOperationException("This exam has schedules or marks and cannot be edited.");

                        NonQuery(cn, tx, "UPDATE Exams SET ExamName=@n, ExamType=@et, TermID=@t, AcademicYearID=@y, StartDate=@s, EndDate=@e, TotalMarks=@tm, PassingMark=@pm, Weight=@w, Status=@st, UpdatedAt=GETDATE() WHERE ExamID=@id",
                            P("@n", name), P("@et", examType), P("@t", termId), P("@y", academicYearId), P("@s", start), P("@e", end),
                            P("@tm", totalMarks), P("@pm", passingMark), P("@w", weight), P("@st", status ?? "Draft"), P("@id", examId));
                        NonQuery(cn, tx, "DELETE FROM ExamSubjects WHERE ExamID=@id", P("@id", examId));
                        NonQuery(cn, tx, "DELETE FROM ExamClasses WHERE ExamID=@id", P("@id", examId));
                    }
                    else
                    {
                        examId = Convert.ToInt32(Scalar(cn, tx,
                            "INSERT INTO Exams (ExamName, ExamType, TermID, AcademicYearID, StartDate, EndDate, TotalMarks, PassingMark, Weight, Status, CreatedBy, CreatedAt) VALUES (@n,@et,@t,@y,@s,@e,@tm,@pm,@w,@st,@by,GETDATE()); SELECT CAST(SCOPE_IDENTITY() AS int)",
                            P("@n", name), P("@et", examType), P("@t", termId), P("@y", academicYearId), P("@s", start), P("@e", end),
                            P("@tm", totalMarks), P("@pm", passingMark), P("@w", weight), P("@st", status ?? "Draft"), P("@by", (object)currentUserId ?? DBNull.Value)));
                    }

                    // scope rows
                    NonQuery(cn, tx, "INSERT INTO ExamClasses (ExamID, ClassID, SectionID) VALUES (@id,@c,@s)",
                        P("@id", examId), P("@c", classId), P("@s", (object)(sectionId > 0 ? sectionId : null) ?? DBNull.Value));

                    foreach (int sid in subjectIds)
                        NonQuery(cn, tx, "INSERT INTO ExamSubjects (ExamID, ClassID, SectionID, SubjectID, TotalMarks, PassingMark) VALUES (@id,@c,@s,@su,@tm,@pm)",
                            P("@id", examId), P("@c", classId), P("@s", (object)(sectionId > 0 ? sectionId : null) ?? DBNull.Value), P("@su", sid), P("@tm", totalMarks), P("@pm", passingMark));

                    tx.Commit();
                    return examId;
                }
            }
        }

        public void SetExaminationStatus(int examId, string newStatus)
        {
            newStatus = (newStatus ?? "").Trim();
            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    string cur = Convert.ToString(Scalar(cn, tx, "SELECT Status FROM Exams WITH (UPDLOCK,HOLDLOCK) WHERE ExamID=@id", P("@id", examId)));
                    if (string.IsNullOrEmpty(cur)) throw new InvalidOperationException("The exam does not exist.");

                    bool ok =
                        (cur.Equals("Draft", StringComparison.OrdinalIgnoreCase) && (newStatus == "Active" || newStatus == "Cancelled")) ||
                        (cur.Equals("Active", StringComparison.OrdinalIgnoreCase) && newStatus == "Cancelled");
                    if (!ok) throw new InvalidOperationException("This status change is not allowed from '" + cur + "'.");

                    NonQuery(cn, tx, "UPDATE Exams SET Status=@st, UpdatedAt=GETDATE() WHERE ExamID=@id", P("@st", newStatus), P("@id", examId));
                    tx.Commit();
                }
            }
        }

        public void DeleteDraftExamination(int examId)
        {
            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    string cur = Convert.ToString(Scalar(cn, tx, "SELECT Status FROM Exams WITH (UPDLOCK,HOLDLOCK) WHERE ExamID=@id", P("@id", examId)));
                    if (string.IsNullOrEmpty(cur)) throw new InvalidOperationException("The exam does not exist.");
                    if (!cur.Equals("Draft", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Only Draft exams can be deleted. Use Cancel instead.");
                    if ((int)Scalar(cn, tx, "SELECT CASE WHEN EXISTS(SELECT 1 FROM ExamSchedules WHERE ExamID=@id) OR EXISTS(SELECT 1 FROM ExamResults WHERE ExamID=@id) OR EXISTS(SELECT 1 FROM ResultPublications WHERE ExamID=@id) THEN 1 ELSE 0 END", P("@id", examId)) == 1)
                        throw new InvalidOperationException("This exam has dependent records and cannot be deleted.");

                    NonQuery(cn, tx, "DELETE FROM ExamSubjects WHERE ExamID=@id", P("@id", examId));
                    NonQuery(cn, tx, "DELETE FROM ExamClasses WHERE ExamID=@id", P("@id", examId));
                    NonQuery(cn, tx, "DELETE FROM Exams WHERE ExamID=@id", P("@id", examId));
                    tx.Commit();
                }
            }
        }

        // ================================================================
        // EXAM SCHEDULES
        // ================================================================
        public DataTable GetExamSchedules(int? academicYearId, int? termId, int? examId, int? classId, int? sectionId, string status, string search)
        {
            const string sql = @"
SELECT sc.ExamScheduleID, sc.ExamID, e.ExamName, sc.ClassID, c.ClassName, sc.SectionID, s.SectionName,
       sc.SubjectID, sub.SubjectName, sc.ExamDate, sc.StartTime, sc.EndTime, sc.ExamRoomID, r.RoomName,
       sc.InvigilatorStaffID, u.FullName AS InvigilatorName, ISNULL(sc.Status,'Scheduled') AS Status, sc.Notes
FROM ExamSchedules sc
JOIN Exams e ON sc.ExamID=e.ExamID
JOIN Classes c ON sc.ClassID=c.ClassID
LEFT JOIN Sections s ON sc.SectionID=s.SectionID
JOIN Subjects sub ON sc.SubjectID=sub.SubjectID
LEFT JOIN ExamRooms r ON sc.ExamRoomID=r.ExamRoomID
LEFT JOIN Staff stf ON sc.InvigilatorStaffID=stf.StaffID
LEFT JOIN Users u ON stf.UserID=u.UserID
WHERE sc.IsActive=1
  AND (@yr IS NULL OR e.AcademicYearID=@yr)
  AND (@tm IS NULL OR e.TermID=@tm)
  AND (@ex IS NULL OR sc.ExamID=@ex)
  AND (@cl IS NULL OR sc.ClassID=@cl)
  AND (@se IS NULL OR sc.SectionID=@se)
  AND (@st='' OR ISNULL(sc.Status,'Scheduled')=@st)
  AND (@s='' OR e.ExamName LIKE '%'+@s+'%' OR sub.SubjectName LIKE '%'+@s+'%')
ORDER BY sc.ExamDate, sc.StartTime;";
            return ExecuteDataTable(sql, new[]
            {
                P("@yr", (object)academicYearId ?? DBNull.Value), P("@tm", (object)termId ?? DBNull.Value),
                P("@ex", (object)examId ?? DBNull.Value), P("@cl", (object)classId ?? DBNull.Value),
                P("@se", (object)sectionId ?? DBNull.Value), P("@st", status ?? ""), P("@s", search ?? "")
            });
        }

        public DataRow GetExamSchedule(int id)
        {
            DataTable t = ExecuteDataTable("SELECT sc.*, e.Status AS ExamStatus FROM ExamSchedules sc JOIN Exams e ON sc.ExamID=e.ExamID WHERE sc.ExamScheduleID=@id", new[] { P("@id", id) });
            return t.Rows.Count > 0 ? t.Rows[0] : null;
        }

        /// <summary>Active exams available for scheduling (excludes Cancelled/Published/Completed).</summary>
        public DataTable GetExamsForScheduling()
        {
            return ExecuteDataTable("SELECT ExamID, ExamName, Status FROM Exams WHERE ISNULL(Status,'')='Active' ORDER BY ExamName", null);
        }

        public DataTable GetClassesForExam(int examId)
        {
            return ExecuteDataTable("SELECT DISTINCT ec.ClassID, c.ClassName FROM ExamClasses ec JOIN Classes c ON ec.ClassID=c.ClassID WHERE ec.ExamID=@e ORDER BY ec.ClassID", new[] { P("@e", examId) });
        }

        public DataTable GetSubjectsForExamClass(int examId, int classId)
        {
            return ExecuteDataTable("SELECT DISTINCT es.SubjectID, sub.SubjectName FROM ExamSubjects es JOIN Subjects sub ON es.SubjectID=sub.SubjectID WHERE es.ExamID=@e AND es.ClassID=@c ORDER BY sub.SubjectName",
                new[] { P("@e", examId), P("@c", classId) });
        }

        public DataTable GetActiveInvigilators()
        {
            return ExecuteDataTable("SELECT s.StaffID, u.FullName, s.EmployeeID, s.Department, s.Position FROM Staff s JOIN Users u ON s.UserID=u.UserID WHERE s.Status='Active' ORDER BY u.FullName", null);
        }

        public int GetSectionStudentCount(int sectionId)
        {
            object o = ExecuteScalar("SELECT COUNT(*) FROM Students WHERE SectionID=@s AND ISNULL(Status,'Active')='Active'", new[] { P("@s", sectionId) });
            return o == null || o == DBNull.Value ? 0 : Convert.ToInt32(o);
        }

        public DataRow GetExamScheduleSummary(int examId)
        {
            const string sql = @"
SELECT
  (SELECT COUNT(*) FROM ExamSubjects WHERE ExamID=@e) AS TotalSubjects,
  (SELECT COUNT(DISTINCT CAST(sc.ClassID AS varchar)+'-'+ISNULL(CAST(sc.SectionID AS varchar),'0')+'-'+CAST(sc.SubjectID AS varchar))
      FROM ExamSchedules sc WHERE sc.ExamID=@e AND sc.IsActive=1) AS ScheduledSubjects,
  (SELECT COUNT(DISTINCT ExamRoomID) FROM ExamSchedules WHERE ExamID=@e AND IsActive=1 AND ExamRoomID IS NOT NULL) AS RoomsAssigned,
  (SELECT COUNT(DISTINCT InvigilatorStaffID) FROM ExamSchedules WHERE ExamID=@e AND IsActive=1 AND InvigilatorStaffID IS NOT NULL) AS InvigilatorsAssigned;";
            DataTable t = ExecuteDataTable(sql, new[] { P("@e", examId) });
            return t.Rows[0];
        }

        public bool ExaminationScheduleIsComplete(int examId)
        {
            // every ExamSubject must have an active, non-draft schedule with room + invigilator
            object o = ExecuteScalar(@"
SELECT CASE WHEN
  (SELECT COUNT(*) FROM ExamSubjects es WHERE es.ExamID=@e) > 0
  AND NOT EXISTS (
     SELECT 1 FROM ExamSubjects es
     WHERE es.ExamID=@e AND NOT EXISTS (
        SELECT 1 FROM ExamSchedules sc
        WHERE sc.ExamID=es.ExamID AND sc.ClassID=es.ClassID AND sc.SubjectID=es.SubjectID
          AND ISNULL(sc.SectionID,0)=ISNULL(es.SectionID,0)
          AND sc.IsActive=1 AND ISNULL(sc.Status,'')<>'Cancelled' AND ISNULL(sc.Status,'')<>'Draft'
          AND sc.ExamRoomID IS NOT NULL AND sc.InvigilatorStaffID IS NOT NULL))
THEN 1 ELSE 0 END", new[] { P("@e", examId) });
            return Convert.ToInt32(o) == 1;
        }

        public void SaveExamSchedule(int id, int examId, int classId, int? sectionId, int subjectId,
            DateTime examDate, TimeSpan start, TimeSpan end, int? roomId, int? invigilatorStaffId, string notes, string status)
        {
            if (examId <= 0 || classId <= 0 || subjectId <= 0) throw new ArgumentException("Examination, class and subject are required.");
            if (end <= start) throw new ArgumentException("Start time must be before end time.");

            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    // lock + revalidate the exam
                    DataTable ex = new DataTable();
                    using (SqlCommand c = new SqlCommand("SELECT Status, StartDate, EndDate FROM Exams WITH (UPDLOCK,HOLDLOCK) WHERE ExamID=@e", cn, tx))
                    { c.Parameters.Add(P("@e", examId)); using (SqlDataAdapter da = new SqlDataAdapter(c)) da.Fill(ex); }
                    if (ex.Rows.Count == 0) throw new InvalidOperationException("The examination does not exist.");
                    string exStatus = Convert.ToString(ex.Rows[0]["Status"]);
                    if (exStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Cannot schedule a cancelled examination.");
                    if (exStatus.Equals("Published", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Cannot edit the schedule of a published examination.");
                    if (exStatus.Equals("Scheduled", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("The schedule is published and locked.");
                    DateTime es = Convert.ToDateTime(ex.Rows[0]["StartDate"]), ee = Convert.ToDateTime(ex.Rows[0]["EndDate"]);
                    if (examDate.Date < es.Date || examDate.Date > ee.Date) throw new InvalidOperationException("The exam date must fall within the examination period.");

                    // class in exam scope
                    if ((int)Scalar(cn, tx, "SELECT COUNT(*) FROM ExamClasses WHERE ExamID=@e AND ClassID=@c", P("@e", examId), P("@c", classId)) == 0)
                        throw new InvalidOperationException("The class is not part of this examination.");
                    // subject in exam scope
                    if ((int)Scalar(cn, tx, "SELECT COUNT(*) FROM ExamSubjects WHERE ExamID=@e AND ClassID=@c AND SubjectID=@su", P("@e", examId), P("@c", classId), P("@su", subjectId)) == 0)
                        throw new InvalidOperationException("The subject is not part of this examination.");
                    // section belongs to class
                    if (sectionId.HasValue && sectionId.Value > 0)
                    {
                        object secClass = Scalar(cn, tx, "SELECT ClassID FROM Sections WHERE SectionID=@s", P("@s", sectionId.Value));
                        if (secClass == null || Convert.ToInt32(secClass) != classId) throw new InvalidOperationException("The section does not belong to the class.");
                    }
                    // room active
                    if (roomId.HasValue && roomId.Value > 0 &&
                        (int)Scalar(cn, tx, "SELECT COUNT(*) FROM ExamRooms WHERE ExamRoomID=@r AND ISNULL(Status,'Active')='Active'", P("@r", roomId.Value)) == 0)
                        throw new InvalidOperationException("The selected room is not available.");
                    // invigilator active
                    if (invigilatorStaffId.HasValue && invigilatorStaffId.Value > 0 &&
                        (int)Scalar(cn, tx, "SELECT COUNT(*) FROM Staff WHERE StaffID=@i AND Status='Active'", P("@i", invigilatorStaffId.Value)) == 0)
                        throw new InvalidOperationException("The selected invigilator is not active.");

                    // duplicate subject schedule
                    if ((int)Scalar(cn, tx, "SELECT COUNT(*) FROM ExamSchedules WHERE ExamID=@e AND ClassID=@c AND ISNULL(SectionID,0)=ISNULL(@s,0) AND SubjectID=@su AND IsActive=1 AND ExamScheduleID<>@id",
                            P("@e", examId), P("@c", classId), P("@s", (object)sectionId ?? DBNull.Value), P("@su", subjectId), P("@id", id)) > 0)
                        throw new InvalidOperationException("This subject is already scheduled for the selected class and section.");

                    string overlap = " AND ExamDate=@d AND IsActive=1 AND ExamScheduleID<>@id AND @st < EndTime AND @et > StartTime";

                    // section conflict
                    if (sectionId.HasValue && sectionId.Value > 0 &&
                        (int)Scalar(cn, tx, "SELECT COUNT(*) FROM ExamSchedules WITH (UPDLOCK,HOLDLOCK) WHERE SectionID=@s" + overlap,
                            P("@s", sectionId.Value), P("@d", examDate.Date), P("@id", id), P("@st", start), P("@et", end)) > 0)
                        throw new InvalidOperationException("This section already has another examination at this time.");
                    // room conflict
                    if (roomId.HasValue && roomId.Value > 0 &&
                        (int)Scalar(cn, tx, "SELECT COUNT(*) FROM ExamSchedules WITH (UPDLOCK,HOLDLOCK) WHERE ExamRoomID=@r" + overlap,
                            P("@r", roomId.Value), P("@d", examDate.Date), P("@id", id), P("@st", start), P("@et", end)) > 0)
                        throw new InvalidOperationException("This room is already booked at this time.");
                    // invigilator conflict
                    if (invigilatorStaffId.HasValue && invigilatorStaffId.Value > 0 &&
                        (int)Scalar(cn, tx, "SELECT COUNT(*) FROM ExamSchedules WITH (UPDLOCK,HOLDLOCK) WHERE InvigilatorStaffID=@i" + overlap,
                            P("@i", invigilatorStaffId.Value), P("@d", examDate.Date), P("@id", id), P("@st", start), P("@et", end)) > 0)
                        throw new InvalidOperationException("This invigilator is already assigned elsewhere at this time.");

                    if (id > 0)
                        NonQuery(cn, tx, "UPDATE ExamSchedules SET ExamID=@e, ClassID=@c, SectionID=@s, SubjectID=@su, ExamDate=@d, StartTime=@st, EndTime=@et, ExamRoomID=@r, InvigilatorStaffID=@i, Notes=@n, Status=@status WHERE ExamScheduleID=@id",
                            P("@e", examId), P("@c", classId), P("@s", (object)(sectionId > 0 ? sectionId : null) ?? DBNull.Value), P("@su", subjectId), P("@d", examDate.Date), P("@st", start), P("@et", end),
                            P("@r", (object)(roomId > 0 ? roomId : null) ?? DBNull.Value), P("@i", (object)(invigilatorStaffId > 0 ? invigilatorStaffId : null) ?? DBNull.Value), P("@n", (object)notes ?? DBNull.Value), P("@status", status ?? "Scheduled"), P("@id", id));
                    else
                        NonQuery(cn, tx, "INSERT INTO ExamSchedules (ExamID, ClassID, SectionID, SubjectID, ExamDate, StartTime, EndTime, ExamRoomID, InvigilatorStaffID, Notes, Status, IsActive) VALUES (@e,@c,@s,@su,@d,@st,@et,@r,@i,@n,@status,1)",
                            P("@e", examId), P("@c", classId), P("@s", (object)(sectionId > 0 ? sectionId : null) ?? DBNull.Value), P("@su", subjectId), P("@d", examDate.Date), P("@st", start), P("@et", end),
                            P("@r", (object)(roomId > 0 ? roomId : null) ?? DBNull.Value), P("@i", (object)(invigilatorStaffId > 0 ? invigilatorStaffId : null) ?? DBNull.Value), P("@n", (object)notes ?? DBNull.Value), P("@status", status ?? "Scheduled"));
                    tx.Commit();
                }
            }
        }

        public void DeleteExamScheduleSafely(int id)
        {
            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    DataTable t = new DataTable();
                    using (SqlCommand c = new SqlCommand("SELECT sc.Status, e.Status AS ExamStatus FROM ExamSchedules sc JOIN Exams e ON sc.ExamID=e.ExamID WHERE sc.ExamScheduleID=@id", cn, tx))
                    { c.Parameters.Add(P("@id", id)); using (SqlDataAdapter da = new SqlDataAdapter(c)) da.Fill(t); }
                    if (t.Rows.Count == 0) throw new InvalidOperationException("The schedule does not exist.");
                    string examStatus = Convert.ToString(t.Rows[0]["ExamStatus"]);
                    if (examStatus.Equals("Published", StringComparison.OrdinalIgnoreCase) || examStatus.Equals("Scheduled", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Cannot delete a schedule of a published examination. Reopen it first.");
                    NonQuery(cn, tx, "UPDATE ExamSchedules SET IsActive=0 WHERE ExamScheduleID=@id", P("@id", id));
                    tx.Commit();
                }
            }
        }

        /// <summary>Publish: verify completeness, lock everything, set schedules + exam to Scheduled.</summary>
        public void PublishExamSchedule(int examId)
        {
            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    string status = Convert.ToString(Scalar(cn, tx, "SELECT Status FROM Exams WITH (UPDLOCK,HOLDLOCK) WHERE ExamID=@e", P("@e", examId)));
                    if (string.IsNullOrEmpty(status)) throw new InvalidOperationException("The examination does not exist.");
                    if (status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Cannot publish a cancelled examination.");
                    if (status.Equals("Published", StringComparison.OrdinalIgnoreCase) || status.Equals("Scheduled", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("The schedule is already published.");

                    if ((int)Scalar(cn, tx, "SELECT COUNT(*) FROM ExamSchedules WITH (UPDLOCK,HOLDLOCK) WHERE ExamID=@e AND IsActive=1", P("@e", examId)) == 0)
                        throw new InvalidOperationException("There are no schedules to publish.");

                    // every subject scheduled with room + invigilator, no draft/cancelled required rows
                    int incomplete = (int)Scalar(cn, tx, @"
SELECT COUNT(*) FROM ExamSubjects es
WHERE es.ExamID=@e AND NOT EXISTS (
   SELECT 1 FROM ExamSchedules sc
   WHERE sc.ExamID=es.ExamID AND sc.ClassID=es.ClassID AND sc.SubjectID=es.SubjectID
     AND ISNULL(sc.SectionID,0)=ISNULL(es.SectionID,0)
     AND sc.IsActive=1 AND ISNULL(sc.Status,'')<>'Cancelled' AND ISNULL(sc.Status,'')<>'Draft'
     AND sc.ExamRoomID IS NOT NULL AND sc.InvigilatorStaffID IS NOT NULL)", P("@e", examId));
                    if (incomplete > 0) throw new InvalidOperationException("Every required subject must have a complete schedule (date, time, room and invigilator) before publishing.");

                    NonQuery(cn, tx, "UPDATE ExamSchedules SET Status='Scheduled' WHERE ExamID=@e AND IsActive=1 AND ISNULL(Status,'')<>'Cancelled'", P("@e", examId));
                    NonQuery(cn, tx, "UPDATE Exams SET Status='Scheduled', UpdatedAt=GETDATE() WHERE ExamID=@e", P("@e", examId));
                    tx.Commit();
                }
            }
        }

        // ================================================================
        // MARKS ENTRY
        // ================================================================
        /// <summary>Exams open for mark entry (Scheduled/Ongoing, not Cancelled/Published).</summary>
        public DataTable GetMarksEntryExams(int? academicYearId, int? termId)
        {
            const string sql = @"
SELECT ExamID, ExamName FROM Exams
WHERE ISNULL(Status,'') IN ('Scheduled','Ongoing')
  AND (@yr IS NULL OR AcademicYearID=@yr) AND (@tm IS NULL OR TermID=@tm)
ORDER BY ExamName";
            return ExecuteDataTable(sql, new[] { P("@yr", (object)academicYearId ?? DBNull.Value), P("@tm", (object)termId ?? DBNull.Value) });
        }

        public DataRow GetExamSubjectRow(int examId, int classId, int subjectId)
        {
            DataTable t = ExecuteDataTable("SELECT TOP 1 * FROM ExamSubjects WHERE ExamID=@e AND ClassID=@c AND SubjectID=@su", new[] { P("@e", examId), P("@c", classId), P("@su", subjectId) });
            return t.Rows.Count > 0 ? t.Rows[0] : null;
        }

        /// <summary>Subjects that have a published schedule for the exam+class (eligible for marks).</summary>
        public DataTable GetSubjectsForMarks(int examId, int classId)
        {
            const string sql = @"
SELECT DISTINCT es.SubjectID, sub.SubjectName, es.TotalMarks
FROM ExamSubjects es
JOIN Subjects sub ON es.SubjectID=sub.SubjectID
WHERE es.ExamID=@e AND es.ClassID=@c
  AND EXISTS (SELECT 1 FROM ExamSchedules sc WHERE sc.ExamID=es.ExamID AND sc.ClassID=es.ClassID AND sc.SubjectID=es.SubjectID AND sc.IsActive=1 AND ISNULL(sc.Status,'')<>'Cancelled')
ORDER BY sub.SubjectName";
            return ExecuteDataTable(sql, new[] { P("@e", examId), P("@c", classId) });
        }

        /// <summary>Eligible students for a section with any existing mark for the exam+subject.</summary>
        public DataTable GetStudentsForMarksEntry(int examId, int sectionId, int subjectId)
        {
            const string sql = @"
SELECT st.StudentID, st.StudentCode, st.AdmissionNo, st.FullName,
       r.Marks, r.Grade, r.Remarks, ISNULL(r.AttendanceStatus,'Present') AS AttendanceStatus, ISNULL(r.Status,'') AS MarkStatus
FROM Students st
LEFT JOIN ExamResults r ON r.StudentID=st.StudentID AND r.ExamID=@e AND r.SubjectID=@su
WHERE st.SectionID=@sec AND ISNULL(st.Status,'Active')='Active'
ORDER BY st.FullName";
            return ExecuteDataTable(sql, new[] { P("@e", examId), P("@su", subjectId), P("@sec", sectionId) });
        }

        public DataRow GetMarkEntrySummary(int examId, int sectionId, int subjectId)
        {
            const string sql = @"
SELECT
  (SELECT COUNT(*) FROM Students WHERE SectionID=@sec AND ISNULL(Status,'Active')='Active') AS TotalStudents,
  (SELECT COUNT(*) FROM ExamResults r JOIN Students st ON r.StudentID=st.StudentID
     WHERE r.ExamID=@e AND r.SubjectID=@su AND st.SectionID=@sec AND (r.Marks IS NOT NULL OR ISNULL(r.AttendanceStatus,'Present')<>'Present')) AS Entered,
  (SELECT COUNT(*) FROM ExamResults r JOIN Students st ON r.StudentID=st.StudentID
     WHERE r.ExamID=@e AND r.SubjectID=@su AND st.SectionID=@sec AND ISNULL(r.Status,'')='Submitted') AS Submitted;";
            DataTable t = ExecuteDataTable(sql, new[] { P("@e", examId), P("@su", subjectId), P("@sec", sectionId) });
            return t.Rows[0];
        }

        public bool ExamSubjectMarksAreSubmitted(int examId, int sectionId, int subjectId)
        {
            object o = ExecuteScalar(@"
SELECT CASE WHEN EXISTS (SELECT 1 FROM ExamResults r JOIN Students st ON r.StudentID=st.StudentID
   WHERE r.ExamID=@e AND r.SubjectID=@su AND st.SectionID=@sec AND ISNULL(r.Status,'')='Submitted') THEN 1 ELSE 0 END",
                new[] { P("@e", examId), P("@su", subjectId), P("@sec", sectionId) });
            return Convert.ToInt32(o) == 1;
        }

        /// <summary>Server-side permission: managers always; a teacher only for their assigned section+subject+year.</summary>
        public bool UserCanEnterSubjectMarks(int userId, string role, int sectionId, int subjectId, int academicYearId)
        {
            if (CanManage(role)) return true;
            if (NormalizeRole(role) != "teacher") return false;
            object o = ExecuteScalar(@"
SELECT COUNT(*) FROM ClassSubjectTeachers cst
JOIN Staff s ON cst.StaffID=s.StaffID
WHERE s.UserID=@u AND cst.SectionID=@sec AND cst.SubjectID=@su AND cst.AcademicYearID=@y AND s.Status='Active'",
                new[] { P("@u", userId), P("@sec", sectionId), P("@su", subjectId), P("@y", academicYearId) });
            return Convert.ToInt32(o) > 0;
        }

        public class MarkRow
        {
            public int StudentID;
            public decimal? Score;
            public string Remarks;
            public string Attendance; // Present / Absent / Excused / Withheld
        }

        /// <summary>Upsert marks. If submit=true, requires a valid state for every student and locks the set.</summary>
        public void SaveMarks(int examId, int classId, int sectionId, int subjectId, int totalMarks,
            IList<MarkRow> rows, int userId, string role, int academicYearId, bool submit)
        {
            if (rows == null) throw new ArgumentException("No marks to save.");

            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    // lock + revalidate exam status
                    string exStatus = Convert.ToString(Scalar(cn, tx, "SELECT Status FROM Exams WITH (UPDLOCK,HOLDLOCK) WHERE ExamID=@e", P("@e", examId)));
                    if (string.IsNullOrEmpty(exStatus)) throw new InvalidOperationException("The examination does not exist.");
                    if (exStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Cannot enter marks for a cancelled examination.");
                    if (exStatus.Equals("Published", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Results are published. Marks are locked.");
                    if (!exStatus.Equals("Scheduled", StringComparison.OrdinalIgnoreCase) && !exStatus.Equals("Ongoing", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("This examination is not open for mark entry.");

                    // already submitted → locked (unless a manager reopen path is used)
                    if ((int)Scalar(cn, tx, "SELECT COUNT(*) FROM ExamResults r JOIN Students st ON r.StudentID=st.StudentID WHERE r.ExamID=@e AND r.SubjectID=@su AND st.SectionID=@sec AND ISNULL(r.Status,'')='Submitted'",
                            P("@e", examId), P("@su", subjectId), P("@sec", sectionId)) > 0)
                        throw new InvalidOperationException("These marks are submitted and locked. Reopen them to make changes.");

                    // grading scale required for submit
                    if (submit && (int)Scalar(cn, tx, "SELECT COUNT(*) FROM GradingScale WHERE AcademicYearID=@y AND ISNULL(Status,'Active')='Active'", P("@y", academicYearId)) == 0)
                        throw new InvalidOperationException("No active grading scale exists for the academic year. Configure grades first.");

                    // eligible student set for this section
                    var eligible = new System.Collections.Generic.HashSet<int>();
                    using (SqlCommand c = new SqlCommand("SELECT StudentID FROM Students WHERE SectionID=@sec AND ISNULL(Status,'Active')='Active'", cn, tx))
                    {
                        c.Parameters.Add(P("@sec", sectionId));
                        using (SqlDataReader rd = c.ExecuteReader()) while (rd.Read()) eligible.Add(rd.GetInt32(0));
                    }

                    var byId = new System.Collections.Generic.Dictionary<int, MarkRow>();
                    foreach (MarkRow r in rows) byId[r.StudentID] = r;

                    // For SUBMIT: every eligible student must have a valid state
                    if (submit)
                    {
                        foreach (int sid in eligible)
                        {
                            MarkRow r; byId.TryGetValue(sid, out r);
                            string att = r != null && !string.IsNullOrEmpty(r.Attendance) ? r.Attendance : "Present";
                            bool hasScore = r != null && r.Score.HasValue;
                            bool absentLike = att == "Absent" || att == "Excused" || att == "Withheld";
                            if (!hasScore && !absentLike)
                                throw new InvalidOperationException("Marks cannot be submitted because one or more required students have no score or valid exam status.");
                        }
                    }

                    // validate + upsert each provided row
                    foreach (MarkRow r in rows)
                    {
                        if (!eligible.Contains(r.StudentID))
                            throw new InvalidOperationException("A submitted student does not belong to the selected section.");

                        string att = string.IsNullOrEmpty(r.Attendance) ? "Present" : r.Attendance;
                        decimal? score = r.Score;
                        if (score.HasValue)
                        {
                            if (score.Value < 0) throw new InvalidOperationException("Score cannot be negative.");
                            if (score.Value > totalMarks) throw new InvalidOperationException("Score cannot exceed the total marks (" + totalMarks + ").");
                        }

                        string grade = null;
                        if (score.HasValue)
                        {
                            decimal pct = totalMarks > 0 ? Math.Round(score.Value * 100m / totalMarks, 2) : 0m;
                            grade = Convert.ToString(Scalar(cn, tx,
                                "SELECT TOP 1 GradeLetter FROM GradingScale WHERE AcademicYearID=@y AND ISNULL(Status,'Active')='Active' AND @p BETWEEN MinMarks AND MaxMarks ORDER BY MinMarks DESC",
                                P("@y", academicYearId), P("@p", pct)));
                        }

                        bool isAbsent = att == "Absent";
                        string rowStatus = submit ? "Submitted" : "Draft";

                        int updated = Convert.ToInt32(Scalar(cn, tx, @"
UPDATE ExamResults SET Marks=@m, TotalMarks=@tm, Grade=@g, Remarks=@rem, AttendanceStatus=@att, IsAbsent=@abs, Status=@st,
    SubmittedBy=CASE WHEN @st='Submitted' THEN @u ELSE SubmittedBy END,
    SubmittedAt=CASE WHEN @st='Submitted' THEN GETDATE() ELSE SubmittedAt END,
    UpdatedAt=GETDATE()
WHERE ExamID=@e AND StudentID=@sid AND SubjectID=@su; SELECT @@ROWCOUNT",
                            P("@m", (object)score ?? DBNull.Value), P("@tm", totalMarks), P("@g", (object)grade ?? DBNull.Value), P("@rem", (object)r.Remarks ?? DBNull.Value),
                            P("@att", att), P("@abs", isAbsent), P("@st", rowStatus), P("@u", userId), P("@e", examId), P("@sid", r.StudentID), P("@su", subjectId)));

                        if (updated == 0)
                            NonQuery(cn, tx, @"
INSERT INTO ExamResults (ExamID, StudentID, SubjectID, Marks, TotalMarks, Grade, Remarks, AttendanceStatus, IsAbsent, Status, EnteredBy, EnteredAt, SubmittedBy, SubmittedAt, UpdatedAt)
VALUES (@e,@sid,@su,@m,@tm,@g,@rem,@att,@abs,@st,@u,GETDATE(), CASE WHEN @st='Submitted' THEN @u ELSE NULL END, CASE WHEN @st='Submitted' THEN GETDATE() ELSE NULL END, GETDATE())",
                                P("@e", examId), P("@sid", r.StudentID), P("@su", subjectId), P("@m", (object)score ?? DBNull.Value), P("@tm", totalMarks),
                                P("@g", (object)grade ?? DBNull.Value), P("@rem", (object)r.Remarks ?? DBNull.Value), P("@att", att), P("@abs", isAbsent), P("@st", rowStatus), P("@u", userId));
                    }

                    // On submit, also flip any remaining eligible (absent-like) students to Submitted
                    if (submit)
                        NonQuery(cn, tx, @"
UPDATE r SET r.Status='Submitted', r.SubmittedBy=@u, r.SubmittedAt=GETDATE(), r.UpdatedAt=GETDATE()
FROM ExamResults r JOIN Students st ON r.StudentID=st.StudentID
WHERE r.ExamID=@e AND r.SubjectID=@su AND st.SectionID=@sec AND ISNULL(r.Status,'')<>'Submitted'",
                            P("@u", userId), P("@e", examId), P("@su", subjectId), P("@sec", sectionId));

                    tx.Commit();
                }
            }
        }

        public void ReopenMarks(int examId, int sectionId, int subjectId, string reason, int userId, string role)
        {
            if (!CanManage(role)) throw new InvalidOperationException("You are not authorized to reopen marks.");
            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    string exStatus = Convert.ToString(Scalar(cn, tx, "SELECT Status FROM Exams WITH (UPDLOCK,HOLDLOCK) WHERE ExamID=@e", P("@e", examId)));
                    if (exStatus.Equals("Published", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Cannot reopen marks after results are published.");
                    if (exStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Cannot reopen marks for a cancelled examination.");

                    NonQuery(cn, tx, @"
UPDATE r SET r.Status='Draft', r.ReopenedBy=@u, r.ReopenedAt=GETDATE(), r.ReopenReason=@reason, r.UpdatedAt=GETDATE()
FROM ExamResults r JOIN Students st ON r.StudentID=st.StudentID
WHERE r.ExamID=@e AND r.SubjectID=@su AND st.SectionID=@sec AND ISNULL(r.Status,'')='Submitted'",
                        P("@u", userId), P("@reason", (object)reason ?? DBNull.Value), P("@e", examId), P("@su", subjectId), P("@sec", sectionId));
                    tx.Commit();
                }
            }
        }

        public string GetGradeForPercentage(int academicYearId, decimal percentage) { return GradeForPercentage(academicYearId, percentage); }

        /// <summary>Exam-wide marks progress for the details page.</summary>
        public DataRow GetExamMarksProgress(int examId)
        {
            const string sql = @"
SELECT
  (SELECT COUNT(*) FROM ExamSubjects WHERE ExamID=@e) AS TotalSubjects,
  (SELECT COUNT(DISTINCT SubjectID) FROM ExamResults WHERE ExamID=@e) AS SubjectsWithMarks,
  (SELECT COUNT(DISTINCT SubjectID) FROM ExamResults WHERE ExamID=@e AND ISNULL(Status,'')='Submitted') AS SubjectsSubmitted,
  (SELECT COUNT(DISTINCT StudentID) FROM ExamResults WHERE ExamID=@e AND (Marks IS NOT NULL OR ISNULL(AttendanceStatus,'Present')<>'Present')) AS StudentsWithMarks;";
            DataTable t = ExecuteDataTable(sql, new[] { P("@e", examId) });
            return t.Rows[0];
        }

        // ================================================================
        // RESULTS  (computed dynamically from ExamResults + ExamSubjects)
        // ================================================================
        public DataTable GetRequiredExamSubjects(int examId)
        {
            return ExecuteDataTable("SELECT SubjectID, TotalMarks FROM ExamSubjects WHERE ExamID=@e", new[] { P("@e", examId) });
        }

        /// <summary>Students in the exam's class/section scope for the exam's academic year.</summary>
        public DataTable GetEligibleStudentsForResults(int examId, int? sectionId)
        {
            const string sql = @"
SELECT DISTINCT st.StudentID, st.StudentCode, st.FullName, sec.SectionID, sec.SectionName, c.ClassID, c.ClassName
FROM Exams e
JOIN ExamClasses ec ON ec.ExamID=e.ExamID
JOIN Sections sec ON sec.ClassID=ec.ClassID AND (ec.SectionID IS NULL OR ec.SectionID=sec.SectionID)
JOIN Classes c ON sec.ClassID=c.ClassID
JOIN Students st ON st.SectionID=sec.SectionID AND st.AcademicYearID=e.AcademicYearID AND ISNULL(st.Status,'Active')='Active'
WHERE e.ExamID=@e AND (@sec IS NULL OR sec.SectionID=@sec)
ORDER BY st.FullName";
            return ExecuteDataTable(sql, new[] { P("@e", examId), P("@sec", (object)sectionId ?? DBNull.Value) });
        }

        /// <summary>All submitted marks for an exam keyed by student+subject.</summary>
        private DataTable GetSubmittedMarks(int examId)
        {
            return ExecuteDataTable("SELECT StudentID, SubjectID, Marks, AttendanceStatus, Status FROM ExamResults WHERE ExamID=@e", new[] { P("@e", examId) });
        }

        public class StudentResult
        {
            public int StudentID { get; set; }
            public string StudentCode { get; set; }
            public string FullName { get; set; }
            public int ClassID { get; set; }
            public string ClassName { get; set; }
            public int SectionID { get; set; }
            public string SectionName { get; set; }
            public decimal TotalObtained { get; set; }
            public int TotalMaximum { get; set; }
            public decimal Average { get; set; }
            public string Grade { get; set; }
            public int Rank { get; set; }
            public string ResultStatus { get; set; }
            public bool Complete { get; set; }
        }

        /// <summary>Compute per-student results (totals, grade, dense rank, status) for an exam scope.</summary>
        public List<StudentResult> ComputeResults(int examId, int? sectionId)
        {
            DataRow exam = GetExamination(examId);
            if (exam == null) return new List<StudentResult>();

            // Published results are immutable: serve the frozen snapshot, not a live recompute.
            if (ResultsArePublished(examId))
                return ReadSnapshotResults(examId, sectionId);
            int year = exam["AcademicYearID"] != DBNull.Value ? Convert.ToInt32(exam["AcademicYearID"]) : 0;
            int passingMark = exam["PassingMark"] != DBNull.Value ? Convert.ToInt32(exam["PassingMark"]) : 40;

            DataTable subjects = GetRequiredExamSubjects(examId);
            int requiredCount = subjects.Rows.Count;
            int totalMax = 0;
            foreach (DataRow s in subjects.Rows) totalMax += Convert.ToInt32(s["TotalMarks"]);

            DataTable marks = GetSubmittedMarks(examId);
            // index marks by student -> (subject -> row)
            var byStudent = new Dictionary<int, Dictionary<int, DataRow>>();
            foreach (DataRow m in marks.Rows)
            {
                int sid = Convert.ToInt32(m["StudentID"]);
                if (!byStudent.ContainsKey(sid)) byStudent[sid] = new Dictionary<int, DataRow>();
                byStudent[sid][Convert.ToInt32(m["SubjectID"])] = m;
            }

            var results = new List<StudentResult>();
            foreach (DataRow st in GetEligibleStudentsForResults(examId, sectionId).Rows)
            {
                int sid = Convert.ToInt32(st["StudentID"]);
                var sr = new StudentResult
                {
                    StudentID = sid, StudentCode = Convert.ToString(st["StudentCode"]), FullName = Convert.ToString(st["FullName"]),
                    ClassID = Convert.ToInt32(st["ClassID"]), ClassName = Convert.ToString(st["ClassName"]),
                    SectionID = Convert.ToInt32(st["SectionID"]), SectionName = Convert.ToString(st["SectionName"]),
                    TotalMaximum = totalMax
                };

                decimal obtained = 0; int submittedSubjects = 0; bool anyWithheld = false; int absentCount = 0;
                Dictionary<int, DataRow> sm = byStudent.ContainsKey(sid) ? byStudent[sid] : null;
                foreach (DataRow subj in subjects.Rows)
                {
                    int subjId = Convert.ToInt32(subj["SubjectID"]);
                    DataRow mr = sm != null && sm.ContainsKey(subjId) ? sm[subjId] : null;
                    if (mr == null || !Convert.ToString(mr["Status"]).Equals("Submitted", StringComparison.OrdinalIgnoreCase)) continue;
                    submittedSubjects++;
                    string att = Convert.ToString(mr["AttendanceStatus"]);
                    if (att.Equals("Withheld", StringComparison.OrdinalIgnoreCase)) anyWithheld = true;
                    if (att.Equals("Absent", StringComparison.OrdinalIgnoreCase) || att.Equals("Excused", StringComparison.OrdinalIgnoreCase)) absentCount++;
                    else if (mr["Marks"] != DBNull.Value) obtained += Convert.ToDecimal(mr["Marks"]);
                }

                sr.TotalObtained = obtained;
                sr.Complete = requiredCount > 0 && submittedSubjects >= requiredCount;
                sr.Average = totalMax > 0 ? Math.Round(obtained * 100m / totalMax, 2) : 0m;
                sr.Grade = sr.Complete ? GradeForPercentage(year, sr.Average) : "";

                if (!sr.Complete) sr.ResultStatus = "Incomplete";
                else if (anyWithheld) sr.ResultStatus = "Withheld";
                else if (absentCount == requiredCount) sr.ResultStatus = "Absent";
                else sr.ResultStatus = sr.Average >= passingMark ? "Passed" : "Failed";

                results.Add(sr);
            }

            // dense ranking by average desc, only for complete non-withheld results, within section
            foreach (var grp in GroupBySection(results))
            {
                var ranked = grp.FindAll(r => r.Complete && r.ResultStatus != "Withheld" && r.ResultStatus != "Absent");
                ranked.Sort((a, b) => b.Average.CompareTo(a.Average));
                int rank = 0; decimal? last = null;
                foreach (var r in ranked)
                {
                    if (last == null || r.Average != last.Value) { rank++; last = r.Average; }
                    r.Rank = rank;
                }
            }
            return results;
        }

        private static IEnumerable<List<StudentResult>> GroupBySection(List<StudentResult> all)
        {
            var map = new Dictionary<int, List<StudentResult>>();
            foreach (var r in all) { if (!map.ContainsKey(r.SectionID)) map[r.SectionID] = new List<StudentResult>(); map[r.SectionID].Add(r); }
            return map.Values;
        }

        /// <summary>Read frozen published results from StudentExamSummaries using the historical class/section,
        /// so promoting or moving a student never changes an already-published result.</summary>
        private List<StudentResult> ReadSnapshotResults(int examId, int? sectionId)
        {
            const string sql = @"
SELECT s.StudentID, st.StudentCode, st.FullName,
       s.ClassID, ISNULL(c.ClassName,'') AS ClassName, s.SectionID, ISNULL(sec.SectionName,'') AS SectionName,
       s.TotalObtained, s.TotalMaximum, s.AveragePercentage, ISNULL(s.OverallGrade,'') AS OverallGrade,
       ISNULL(s.Rank,0) AS Rank, ISNULL(s.ResultStatus,'') AS ResultStatus
FROM StudentExamSummaries s
JOIN Students st ON st.StudentID=s.StudentID
LEFT JOIN Classes c ON c.ClassID=s.ClassID
LEFT JOIN Sections sec ON sec.SectionID=s.SectionID
WHERE s.ExamID=@e AND s.PublicationStatus='Published' AND (@sec IS NULL OR s.SectionID=@sec)
ORDER BY st.FullName";
            var list = new List<StudentResult>();
            foreach (DataRow r in ExecuteDataTable(sql, new[] { P("@e", examId), P("@sec", (object)sectionId ?? DBNull.Value) }).Rows)
            {
                var sr = new StudentResult
                {
                    StudentID = Convert.ToInt32(r["StudentID"]), StudentCode = Convert.ToString(r["StudentCode"]), FullName = Convert.ToString(r["FullName"]),
                    ClassID = r["ClassID"] != DBNull.Value ? Convert.ToInt32(r["ClassID"]) : 0, ClassName = Convert.ToString(r["ClassName"]),
                    SectionID = r["SectionID"] != DBNull.Value ? Convert.ToInt32(r["SectionID"]) : 0, SectionName = Convert.ToString(r["SectionName"]),
                    TotalObtained = Convert.ToDecimal(r["TotalObtained"]), TotalMaximum = Convert.ToInt32(Convert.ToDecimal(r["TotalMaximum"])),
                    Average = Convert.ToDecimal(r["AveragePercentage"]), Grade = Convert.ToString(r["OverallGrade"]),
                    Rank = Convert.ToInt32(r["Rank"]), ResultStatus = Convert.ToString(r["ResultStatus"]),
                    Complete = true
                };
                list.Add(sr);
            }
            return list;
        }

        /// <summary>Ownership check for published student results (student/parent linkage). Managers/registrar always allowed.</summary>
        public bool UserCanViewStudentResult(int userId, string role, int studentId)
        {
            if (CanView(role)) return true; // management, teacher, registrar (page-level scoping applies elsewhere)
            string r = NormalizeRole(role);
            if (r == "parent" || r == "guardian")
                return Convert.ToInt32(ExecuteScalar(@"
SELECT COUNT(*) FROM Guardians g JOIN StudentGuardians sg ON sg.GuardianID=g.GuardianID
WHERE g.UserID=@u AND sg.StudentID=@st AND ISNULL(g.IsActive,1)=1",
                    new[] { P("@u", userId), P("@st", studentId) })) > 0;
            // No Student->User login linkage exists in this schema.
            return false;
        }

        public bool ExaminationResultsAreComplete(int examId)
        {
            foreach (var r in ComputeResults(examId, null)) if (!r.Complete) return false;
            // and at least one eligible student and required subjects
            return GetRequiredExamSubjects(examId).Rows.Count > 0 && GetEligibleStudentsForResults(examId, null).Rows.Count > 0;
        }

        public bool ResultsArePublished(int examId)
        {
            return Convert.ToInt32(ExecuteScalar("SELECT COUNT(*) FROM ResultPublications WHERE ExamID=@e AND ISNULL(Status,'Published')='Published'", new[] { P("@e", examId) })) > 0;
        }

        public DataRow GetResultPublication(int examId)
        {
            DataTable t = ExecuteDataTable("SELECT TOP 1 rp.*, u.FullName AS PublishedByName FROM ResultPublications rp LEFT JOIN Users u ON rp.PublishedBy=u.UserID WHERE rp.ExamID=@e ORDER BY rp.PublicationID DESC", new[] { P("@e", examId) });
            return t.Rows.Count > 0 ? t.Rows[0] : null;
        }

        /// <summary>Publish: verify completeness, lock, insert publication rows per class/section, set exam Published,
        /// and persist an immutable per-student snapshot into StudentExamSummaries.</summary>
        public void PublishResults(int examId, int userId)
        {
            // Compute results on-the-fly (pre-publish) so we can freeze them into the snapshot.
            List<StudentResult> snapshot = ComputeResults(examId, null);

            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    string status = Convert.ToString(Scalar(cn, tx, "SELECT Status FROM Exams WITH (UPDLOCK,HOLDLOCK) WHERE ExamID=@e", P("@e", examId)));
                    if (string.IsNullOrEmpty(status)) throw new InvalidOperationException("The examination does not exist.");
                    if (status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Cannot publish a cancelled examination.");
                    if (status.Equals("Published", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Results are already published.");

                    // every required subject submitted for every eligible student
                    int incomplete = (int)Scalar(cn, tx, @"
SELECT COUNT(*) FROM (
  SELECT st.StudentID
  FROM Exams e
  JOIN ExamClasses ec ON ec.ExamID=e.ExamID
  JOIN Sections sec ON sec.ClassID=ec.ClassID AND (ec.SectionID IS NULL OR ec.SectionID=sec.SectionID)
  JOIN Students st ON st.SectionID=sec.SectionID AND st.AcademicYearID=e.AcademicYearID AND ISNULL(st.Status,'Active')='Active'
  CROSS JOIN ExamSubjects es
  WHERE e.ExamID=@e AND es.ExamID=@e AND es.ClassID=sec.ClassID
    AND NOT EXISTS (SELECT 1 FROM ExamResults r WHERE r.ExamID=@e AND r.StudentID=st.StudentID AND r.SubjectID=es.SubjectID AND ISNULL(r.Status,'')='Submitted')
) q", P("@e", examId));
                    if (incomplete > 0) throw new InvalidOperationException("Results cannot be published because one or more required subjects have incomplete or unsubmitted marks.");

                    // one publication row per class+section in scope
                    NonQuery(cn, tx, @"
INSERT INTO ResultPublications (ExamID, ClassID, SectionID, Status, PublishedBy, PublishedAt, CreatedAt)
SELECT DISTINCT @e, ec.ClassID, ec.SectionID, 'Published', @u, GETDATE(), GETDATE()
FROM ExamClasses ec
WHERE ec.ExamID=@e
  AND NOT EXISTS (SELECT 1 FROM ResultPublications rp WHERE rp.ExamID=@e AND rp.ClassID=ec.ClassID AND ISNULL(rp.SectionID,0)=ISNULL(ec.SectionID,0) AND ISNULL(rp.Status,'Published')='Published')",
                        P("@e", examId), P("@u", userId));

                    NonQuery(cn, tx, "UPDATE Exams SET Status='Published', PublishedBy=@u, PublishedAt=GETDATE(), UpdatedAt=GETDATE() WHERE ExamID=@e", P("@u", userId), P("@e", examId));

                    // Freeze immutable snapshots (upsert one row per student).
                    foreach (StudentResult r in snapshot)
                        UpsertSummary(cn, tx, examId, r, userId, publishing: true);

                    tx.Commit();
                }
            }
        }

        /// <summary>Insert/update one immutable StudentExamSummaries row inside a transaction.</summary>
        private void UpsertSummary(SqlConnection cn, SqlTransaction tx, int examId, StudentResult r, int userId, bool publishing)
        {
            int updated = Convert.ToInt32(Scalar(cn, tx, @"
UPDATE StudentExamSummaries SET
    AcademicYearID=@ay, ClassID=@cl, SectionID=@sec,
    TotalObtained=@obt, TotalMaximum=@max, AveragePercentage=@avg,
    OverallGrade=@grade, Rank=@rank, ResultStatus=@status,
    PublicationStatus=CASE WHEN @pub=1 THEN 'Published' ELSE PublicationStatus END,
    PublishedBy=CASE WHEN @pub=1 THEN @u ELSE PublishedBy END,
    PublishedAt=CASE WHEN @pub=1 THEN GETDATE() ELSE PublishedAt END,
    UpdatedAt=GETDATE()
WHERE ExamID=@e AND StudentID=@st;
SELECT @@ROWCOUNT",
                P("@ay", ExamYear(examId)), P("@cl", r.ClassID), P("@sec", r.SectionID),
                P("@obt", r.TotalObtained), P("@max", (decimal)r.TotalMaximum), P("@avg", r.Average),
                P("@grade", (object)(string.IsNullOrEmpty(r.Grade) ? null : r.Grade) ?? DBNull.Value),
                P("@rank", r.Rank > 0 ? (object)r.Rank : DBNull.Value), P("@status", (object)r.ResultStatus ?? DBNull.Value),
                P("@pub", publishing ? 1 : 0), P("@u", userId), P("@e", examId), P("@st", r.StudentID)));

            if (updated == 0)
                NonQuery(cn, tx, @"
INSERT INTO StudentExamSummaries
    (ExamID, StudentID, AcademicYearID, ClassID, SectionID, TotalObtained, TotalMaximum,
     AveragePercentage, OverallGrade, Rank, ResultStatus, PublicationStatus,
     CalculatedBy, CalculatedAt, PublishedBy, PublishedAt, CreatedAt, UpdatedAt)
VALUES (@e,@st,@ay,@cl,@sec,@obt,@max,@avg,@grade,@rank,@status,
     @pubStatus, @u, GETDATE(), CASE WHEN @pub=1 THEN @u ELSE NULL END, CASE WHEN @pub=1 THEN GETDATE() ELSE NULL END, GETDATE(), GETDATE())",
                    P("@e", examId), P("@st", r.StudentID), P("@ay", ExamYear(examId)), P("@cl", r.ClassID), P("@sec", r.SectionID),
                    P("@obt", r.TotalObtained), P("@max", (decimal)r.TotalMaximum), P("@avg", r.Average),
                    P("@grade", (object)(string.IsNullOrEmpty(r.Grade) ? null : r.Grade) ?? DBNull.Value),
                    P("@rank", r.Rank > 0 ? (object)r.Rank : DBNull.Value), P("@status", (object)r.ResultStatus ?? DBNull.Value),
                    P("@pubStatus", publishing ? "Published" : "Draft"), P("@pub", publishing ? 1 : 0), P("@u", userId));
        }

        private int ExamYear(int examId)
        {
            object o = ExecuteScalar("SELECT AcademicYearID FROM Exams WHERE ExamID=@e", new[] { P("@e", examId) });
            return o == null || o == DBNull.Value ? 0 : Convert.ToInt32(o);
        }

        public void UnpublishResults(int examId, string reason, int userId, string role)
        {
            if (!CanManage(role)) throw new InvalidOperationException("You are not authorized to unpublish results.");
            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    string status = Convert.ToString(Scalar(cn, tx, "SELECT Status FROM Exams WITH (UPDLOCK,HOLDLOCK) WHERE ExamID=@e", P("@e", examId)));
                    if (!status.Equals("Published", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("The examination is not published.");

                    // preserve history: mark publication rows Unpublished (do not delete)
                    NonQuery(cn, tx, "UPDATE ResultPublications SET Status='Unpublished', UnpublishedBy=@u, UnpublishedAt=GETDATE(), UnpublishReason=@r WHERE ExamID=@e AND ISNULL(Status,'Published')='Published'",
                        P("@u", userId), P("@r", (object)reason ?? DBNull.Value), P("@e", examId));
                    // Snapshot rows are preserved (immutable history) but hidden from published views.
                    NonQuery(cn, tx, "UPDATE StudentExamSummaries SET PublicationStatus='Unpublished', UpdatedAt=GETDATE() WHERE ExamID=@e AND PublicationStatus='Published'", P("@e", examId));
                    NonQuery(cn, tx, "UPDATE Exams SET Status='Completed', UpdatedAt=GETDATE() WHERE ExamID=@e", P("@e", examId));
                    tx.Commit();
                }
            }
        }

        public DataRow GetResultsDashboardSummary(int? academicYearId, int? termId)
        {
            const string sql = @"
SELECT
  (SELECT COUNT(*) FROM Exams e WHERE ISNULL(e.Status,'')='Published' AND (@yr IS NULL OR e.AcademicYearID=@yr) AND (@tm IS NULL OR e.TermID=@tm)) AS Published,
  (SELECT COUNT(DISTINCT r.StudentID) FROM ExamResults r JOIN Exams e ON r.ExamID=e.ExamID WHERE (@yr IS NULL OR e.AcademicYearID=@yr) AND (@tm IS NULL OR e.TermID=@tm)) AS Evaluated;";
            DataTable t = ExecuteDataTable(sql, new[] { P("@yr", (object)academicYearId ?? DBNull.Value), P("@tm", (object)termId ?? DBNull.Value) });
            return t.Rows[0];
        }

        /// <summary>Subject breakdown + totals for one student's result (report card / details).</summary>
        public DataTable GetStudentResultBreakdown(int examId, int studentId)
        {
            const string sql = @"
SELECT sub.SubjectName, es.TotalMarks, r.Marks, r.Grade, ISNULL(r.AttendanceStatus,'Present') AS AttendanceStatus, r.Remarks, ISNULL(r.Status,'') AS MarkStatus
FROM ExamSubjects es
JOIN Subjects sub ON es.SubjectID=sub.SubjectID
LEFT JOIN ExamResults r ON r.ExamID=es.ExamID AND r.SubjectID=es.SubjectID AND r.StudentID=@st
WHERE es.ExamID=@e
ORDER BY sub.SubjectName";
            return ExecuteDataTable(sql, new[] { P("@e", examId), P("@st", studentId) });
        }

        public StudentResult GetStudentResult(int examId, int studentId)
        {
            foreach (var r in ComputeResults(examId, null)) if (r.StudentID == studentId) return r;
            return null;
        }

        public string GetStudentAdmissionNo(int studentId)
        {
            object o = ExecuteScalar("SELECT AdmissionNo FROM Students WHERE StudentID=@s", new[] { P("@s", studentId) });
            return o == null || o == DBNull.Value ? "" : Convert.ToString(o);
        }

        // ================================================================
        // EXAM ROOMS
        // ================================================================
        public DataTable GetExamRoomsAll()
        {
            return ExecuteDataTable(@"
SELECT r.ExamRoomID, r.RoomName, r.Capacity, ISNULL(r.Location,'') AS Location, ISNULL(r.Status,'Active') AS Status,
       (SELECT COUNT(*) FROM ExamSchedules sc WHERE sc.ExamRoomID=r.ExamRoomID AND sc.IsActive=1) AS Bookings
FROM ExamRooms r ORDER BY r.RoomName", null);
        }

        public DataRow GetExamRoom(int id)
        {
            DataTable t = ExecuteDataTable("SELECT * FROM ExamRooms WHERE ExamRoomID=@id", new[] { P("@id", id) });
            return t.Rows.Count > 0 ? t.Rows[0] : null;
        }

        public void SaveExamRoom(int id, string name, int capacity, string location, string status)
        {
            name = (name ?? "").Trim();
            if (name.Length == 0) throw new ArgumentException("Room name is required.");
            if (capacity <= 0) throw new ArgumentException("Capacity must be greater than zero.");
            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    if ((int)Scalar(cn, tx, "SELECT COUNT(*) FROM ExamRooms WHERE RoomName=@n AND ExamRoomID<>@id", P("@n", name), P("@id", id)) > 0)
                        throw new InvalidOperationException("A room with this name already exists.");
                    if (id > 0)
                        NonQuery(cn, tx, "UPDATE ExamRooms SET RoomName=@n, Capacity=@c, Location=@l, Status=@st WHERE ExamRoomID=@id",
                            P("@n", name), P("@c", capacity), P("@l", (object)location ?? DBNull.Value), P("@st", status ?? "Active"), P("@id", id));
                    else
                        NonQuery(cn, tx, "INSERT INTO ExamRooms (RoomName, Capacity, Location, Status, CreatedAt) VALUES (@n,@c,@l,@st,GETDATE())",
                            P("@n", name), P("@c", capacity), P("@l", (object)location ?? DBNull.Value), P("@st", status ?? "Active"));
                    tx.Commit();
                }
            }
        }

        public void SetExamRoomStatus(int id, string status)
        {
            ExecuteNonQuery("UPDATE ExamRooms SET Status=@st WHERE ExamRoomID=@id", new[] { P("@st", status ?? "Active"), P("@id", id) });
        }

        // ================================================================
        // GRADING SCALES
        // ================================================================
        public DataTable GetGradingScales(int? academicYearId)
        {
            const string sql = @"
SELECT GradeID, GradeLetter, MinMarks, MaxMarks, GPA, ISNULL(Description,'') AS Description,
       IsPass, ISNULL(Status,'Active') AS Status, AcademicYearID
FROM GradingScale
WHERE (@yr IS NULL OR AcademicYearID=@yr)
ORDER BY MinMarks DESC;";
            return ExecuteDataTable(sql, new[] { P("@yr", (object)academicYearId ?? DBNull.Value) });
        }

        public DataRow GetGradingScale(int id)
        {
            DataTable t = ExecuteDataTable("SELECT * FROM GradingScale WHERE GradeID=@id", new[] { P("@id", id) });
            return t.Rows.Count > 0 ? t.Rows[0] : null;
        }

        public void SaveGradingScale(int id, int academicYearId, string letter, int min, int max, decimal gpa,
            string description, bool isPass, string status)
        {
            letter = (letter ?? "").Trim();
            if (letter.Length == 0) throw new ArgumentException("Grade name is required.");
            if (academicYearId <= 0) throw new ArgumentException("Academic year is required.");
            if (min < 0 || max > 100) throw new ArgumentException("Percentages must be between 0 and 100.");
            if (min > max) throw new ArgumentException("Minimum percentage cannot exceed the maximum.");

            using (SqlConnection cn = CreateConnection())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    // duplicate grade letter within the year
                    if ((int)Scalar(cn, tx, "SELECT COUNT(*) FROM GradingScale WHERE AcademicYearID=@y AND GradeLetter=@l AND GradeID<>@id",
                            P("@y", academicYearId), P("@l", letter), P("@id", id)) > 0)
                        throw new InvalidOperationException("A grade with this name already exists for the year.");

                    // overlapping ranges (min<=existingMax AND max>=existingMin)
                    if ((int)Scalar(cn, tx,
                            "SELECT COUNT(*) FROM GradingScale WITH (UPDLOCK,HOLDLOCK) WHERE AcademicYearID=@y AND GradeID<>@id AND @min <= MaxMarks AND @max >= MinMarks",
                            P("@y", academicYearId), P("@id", id), P("@min", min), P("@max", max)) > 0)
                        throw new InvalidOperationException("This percentage range overlaps another grade.");

                    if (id > 0)
                        NonQuery(cn, tx, "UPDATE GradingScale SET GradeLetter=@l, MinMarks=@min, MaxMarks=@max, GPA=@g, Description=@d, IsPass=@p, Status=@st, AcademicYearID=@y WHERE GradeID=@id",
                            P("@l", letter), P("@min", min), P("@max", max), P("@g", gpa), P("@d", (object)description ?? DBNull.Value), P("@p", isPass), P("@st", status ?? "Active"), P("@y", academicYearId), P("@id", id));
                    else
                        NonQuery(cn, tx, "INSERT INTO GradingScale (GradeLetter, MinMarks, MaxMarks, GPA, Description, IsPass, Status, AcademicYearID) VALUES (@l,@min,@max,@g,@d,@p,@st,@y)",
                            P("@l", letter), P("@min", min), P("@max", max), P("@g", gpa), P("@d", (object)description ?? DBNull.Value), P("@p", isPass), P("@st", status ?? "Active"), P("@y", academicYearId));
                    tx.Commit();
                }
            }
        }

        public void SetGradingScaleStatus(int id, string status)
        {
            ExecuteNonQuery("UPDATE GradingScale SET Status=@st WHERE GradeID=@id",
                new[] { P("@st", status ?? "Active"), P("@id", id) });
        }

        /// <summary>Server-side grade lookup for a percentage using the year's active scale.</summary>
        public string GradeForPercentage(int academicYearId, decimal percentage)
        {
            object o = ExecuteScalar(
                "SELECT TOP 1 GradeLetter FROM GradingScale WHERE AcademicYearID=@y AND ISNULL(Status,'Active')='Active' AND @p BETWEEN MinMarks AND MaxMarks ORDER BY MinMarks DESC",
                new[] { P("@y", academicYearId), P("@p", percentage) });
            return o == null || o == DBNull.Value ? "" : Convert.ToString(o);
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

        public DataTable GetTerms(int? academicYearId)
        {
            return ExecuteDataTable("SELECT TermID, TermName, AcademicYearID FROM Terms WHERE (@y IS NULL OR AcademicYearID=@y) ORDER BY StartDate",
                new[] { P("@y", (object)academicYearId ?? DBNull.Value) });
        }

        public DataTable GetClasses()
        {
            return ExecuteDataTable("SELECT ClassID, ClassName FROM Classes WHERE ISNULL(Status,'Active')='Active' ORDER BY ClassID", null);
        }

        public DataTable GetExamRooms()
        {
            return ExecuteDataTable("SELECT ExamRoomID, RoomName, Capacity, ISNULL(Location,'') AS Location, Status FROM ExamRooms WHERE ISNULL(Status,'Active')='Active' ORDER BY RoomName", null);
        }

        // ================================================================
        // ADO helpers
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
