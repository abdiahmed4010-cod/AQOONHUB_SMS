using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AQOONHUB_SMS.Modules.Examinations
{
    /// <summary>
    /// Read-only lookups + student examination listing for the Transcript Center.
    /// Reuses the existing examination tables and the ExaminationsRepository grading /
    /// per-exam result logic — it never writes marks, results, students or grades.
    /// </summary>
    public sealed class TranscriptRepository
    {
        private readonly string _cs;

        public TranscriptRepository()
        {
            var s = ConfigurationManager.ConnectionStrings["AQOONHUB_DB"];
            if (s == null || string.IsNullOrWhiteSpace(s.ConnectionString))
                throw new ConfigurationErrorsException("Connection string 'AQOONHUB_DB' was not found.");
            _cs = s.ConnectionString;
        }

        private DataTable Q(string sql, params SqlParameter[] p)
        {
            using (var cn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(sql, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                if (p != null) cmd.Parameters.AddRange(p);
                var t = new DataTable();
                da.Fill(t);
                return t;
            }
        }
        private static SqlParameter P(string n, object v) { return new SqlParameter(n, v ?? DBNull.Value); }

        // ---- Filter lookups ----
        public DataTable AcademicYears()
        {
            return Q("SELECT AcademicYearID, YearName FROM AcademicYears ORDER BY StartDate DESC, AcademicYearID DESC");
        }
        public DataTable Terms(int academicYearId)
        {
            return Q("SELECT TermID, TermName FROM Terms WHERE AcademicYearID=@y ORDER BY StartDate, TermID", P("@y", academicYearId));
        }
        public DataTable Classes()
        {
            return Q("SELECT ClassID, ClassName FROM Classes WHERE ISNULL(Status,'Active')<>'Inactive' ORDER BY ClassName");
        }
        public DataTable Sections(int classId)
        {
            return Q("SELECT SectionID, SectionName FROM Sections WHERE ClassID=@c AND ISNULL(Status,'Active')='Active' ORDER BY SectionName", P("@c", classId));
        }
        /// <summary>Students filtered by section (when given) and/or a free-text search on name/code/admission no.</summary>
        public DataTable Students(int? sectionId, string search)
        {
            string s = (search ?? "").Trim();
            return Q(@"SELECT StudentID, StudentCode, AdmissionNo,
                          LTRIM(RTRIM(ISNULL(FirstName,'')+' '+ISNULL(LastName,''))) AS FullName
                       FROM Students
                       WHERE Status<>'Deleted'
                         AND (@sec IS NULL OR SectionID=@sec)
                         AND (@q='' OR FirstName LIKE '%'+@q+'%' OR LastName LIKE '%'+@q+'%'
                              OR StudentCode LIKE '%'+@q+'%' OR AdmissionNo LIKE '%'+@q+'%')
                       ORDER BY FullName",
                P("@sec", (object)sectionId ?? DBNull.Value), P("@q", s));
        }

        // ---- Student profile ----
        public DataRow StudentProfile(int studentId)
        {
            var t = Q(@"SELECT s.StudentID, s.StudentCode, s.AdmissionNo, s.Gender, s.DateOfBirth,
                           s.EnrollmentDate, s.Status,
                           LTRIM(RTRIM(ISNULL(s.FirstName,'')+' '+ISNULL(s.LastName,''))) AS FullName,
                           c.ClassID, c.ClassName, sec.SectionID, sec.SectionName,
                           s.AcademicYearID, ay.YearName
                        FROM Students s
                        JOIN Sections sec ON sec.SectionID=s.SectionID
                        JOIN Classes c ON c.ClassID=sec.ClassID
                        LEFT JOIN AcademicYears ay ON ay.AcademicYearID=s.AcademicYearID
                        WHERE s.StudentID=@s", P("@s", studentId));
            return t.Rows.Count == 0 ? null : t.Rows[0];
        }

        /// <summary>
        /// Distinct examinations the student actually has recorded results for, optionally
        /// scoped to an academic year and/or term. Ordered chronologically (year, term, exam).
        /// </summary>
        public DataTable StudentExams(int studentId, int? academicYearId, int? termId)
        {
            return Q(@"SELECT DISTINCT e.ExamID, e.ExamName, e.ExamType, e.AcademicYearID, ay.YearName,
                          e.TermID, t.TermName, ay.StartDate AS YearStart, t.StartDate AS TermStart
                       FROM ExamResults r
                       JOIN Exams e ON e.ExamID=r.ExamID
                       JOIN AcademicYears ay ON ay.AcademicYearID=e.AcademicYearID
                       LEFT JOIN Terms t ON t.TermID=e.TermID
                       WHERE r.StudentID=@s
                         AND (@y IS NULL OR e.AcademicYearID=@y)
                         AND (@tm IS NULL OR e.TermID=@tm)
                       ORDER BY ay.StartDate, e.TermID, e.ExamID",
                P("@s", studentId), P("@y", (object)academicYearId ?? DBNull.Value), P("@tm", (object)termId ?? DBNull.Value));
        }

        /// <summary>Per-subject result rows for an exam (reuses ExamSubjects/Subjects/ExamResults,
        /// including the grade already stored at marks entry). Read-only; no calculation duplicated.</summary>
        public DataTable StudentExamBreakdown(int examId, int studentId)
        {
            return Q(@"SELECT sub.SubjectCode, sub.SubjectName, es.TotalMarks, r.Marks, r.Grade,
                          ISNULL(r.AttendanceStatus,'Present') AS AttendanceStatus, r.Remarks
                       FROM ExamSubjects es
                       JOIN Subjects sub ON es.SubjectID=sub.SubjectID
                       LEFT JOIN ExamResults r ON r.ExamID=es.ExamID AND r.SubjectID=es.SubjectID AND r.StudentID=@st
                       WHERE es.ExamID=@ex
                       ORDER BY sub.SubjectName",
                P("@ex", examId), P("@st", studentId));
        }

        // ---- School header info + grading scale ----
        public DataRow SchoolInfo()
        {
            var t = Q("SELECT TOP 1 SchoolName, Address, Phone, Email, LogoPath FROM SchoolSettings ORDER BY SettingID");
            return t.Rows.Count == 0 ? null : t.Rows[0];
        }
        public DataTable GradingScale(int academicYearId)
        {
            return Q("SELECT GradeLetter, MinMarks, MaxMarks FROM GradingScale WHERE AcademicYearID=@y AND ISNULL(Status,'Active')='Active' ORDER BY MinMarks DESC", P("@y", academicYearId));
        }
    }
}
