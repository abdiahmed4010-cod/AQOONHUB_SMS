using System;
using System.Data;

namespace AQOONHUB_SMS.Modules.Reports
{
    public sealed partial class ReportsRepository
    {
        /// <summary>Stage 3 handler dispatch (Examination / Attendance / Finance / Payroll).
        /// Called from GetReportData's default. Every query is hardcoded + parameterized.</summary>
        private DataTable Stage3(string handler, ReportFilter f, bool allowSensitive)
        {
            switch (handler)
            {
                // ---- EXAMINATION ----
                case "exam-list": return ExamList(f);
                case "exam-schedule": return ExamSchedule(f);
                case "exam-rooms": return ExecuteDataTable("SELECT RoomName AS [Room], ISNULL(Location,'') AS [Location], ISNULL(Capacity,0) AS [Capacity], ISNULL(Status,'Active') AS [Status] FROM ExamRooms ORDER BY RoomName", null);
                case "exam-invigilators": return ExamInvigilators(f);
                case "exam-marks-status": return MarksEntryStatus(f);
                case "exam-missing-marks": return MarksByStatus(f, "missing");
                case "exam-submitted-marks": return MarksByStatus(f, "Submitted");
                case "exam-locked-marks": return MarksByStatus(f, "Locked");
                case "exam-student-results": return StudentResults(f);
                case "exam-class-results": return ClassResults(f);
                case "exam-subject-results": return SubjectResults(f);
                case "exam-grade-distribution": return GradeDistribution(f);
                case "exam-pass-fail": return PassFail(f);
                case "exam-top-performers": return Performers(f, "top");
                case "exam-lowest-performers": return Performers(f, "low");
                case "exam-class-ranking": return ClassRanking(f);
                case "exam-subject-ranking": return SubjectRanking(f);
                case "exam-overall-analysis": return OverallAnalysis(f);
                case "exam-published-results": return Summaries(f, "Published");
                case "exam-unpublished-results": return Summaries(f, "!Published");
                case "exam-report-cards": return Summaries(f, "Published");
                case "exam-result-history": return Summaries(f, null);

                default: return Stage3Attendance(handler, f, allowSensitive);
            }
        }

        // ===================== EXAMINATION HANDLERS =====================
        private DataTable ExamList(ReportFilter f)
        {
            const string sql = @"
SELECT e.ExamName AS [Examination], e.ExamType AS [Type], y.YearName AS [Academic Year], ISNULL(t.TermName,'') AS [Term],
       e.StartDate AS [Start], e.EndDate AS [End], ISNULL(e.Status,'Draft') AS [Status],
       CASE WHEN e.Status='Published' THEN 'Published' ELSE 'Unpublished' END AS [Publication]
FROM Exams e LEFT JOIN AcademicYears y ON y.AcademicYearID=e.AcademicYearID LEFT JOIN Terms t ON t.TermID=e.TermID
WHERE (@y IS NULL OR e.AcademicYearID=@y) AND (@tm IS NULL OR e.TermID=@tm) AND (@ex IS NULL OR e.ExamID=@ex)
ORDER BY e.StartDate DESC";
            return ExecuteDataTable(sql, ExamP(f));
        }

        private DataTable ExamSchedule(ReportFilter f)
        {
            const string sql = @"
SELECT es.ExamDate AS [Date], es.StartTime AS [Start], es.EndTime AS [End], c.ClassName AS [Class], sec.SectionName AS [Section],
       sub.SubjectName AS [Subject], ISNULL(r.RoomName,'—') AS [Room], ISNULL(u.FullName,'—') AS [Invigilator], ISNULL(es.Status,'Scheduled') AS [Status]
FROM ExamSchedules es
JOIN Exams e ON e.ExamID=es.ExamID
LEFT JOIN Classes c ON c.ClassID=es.ClassID LEFT JOIN Sections sec ON sec.SectionID=es.SectionID
LEFT JOIN Subjects sub ON sub.SubjectID=es.SubjectID LEFT JOIN ExamRooms r ON r.ExamRoomID=es.ExamRoomID
LEFT JOIN Staff sf ON sf.StaffID=es.InvigilatorStaffID LEFT JOIN Users u ON u.UserID=sf.UserID
WHERE (@ex IS NULL OR es.ExamID=@ex) AND (@y IS NULL OR e.AcademicYearID=@y) AND (@c IS NULL OR es.ClassID=@c) AND (@sec IS NULL OR es.SectionID=@sec)
ORDER BY es.ExamDate, es.StartTime";
            return ExecuteDataTable(sql, ExamP(f));
        }

        private DataTable ExamInvigilators(ReportFilter f)
        {
            const string sql = @"
SELECT e.ExamName AS [Exam], sub.SubjectName AS [Subject], es.ExamDate AS [Date], ISNULL(u.FullName,'—') AS [Invigilator], ISNULL(r.RoomName,'—') AS [Room]
FROM ExamSchedules es JOIN Exams e ON e.ExamID=es.ExamID LEFT JOIN Subjects sub ON sub.SubjectID=es.SubjectID
LEFT JOIN Staff sf ON sf.StaffID=es.InvigilatorStaffID LEFT JOIN Users u ON u.UserID=sf.UserID LEFT JOIN ExamRooms r ON r.ExamRoomID=es.ExamRoomID
WHERE es.InvigilatorStaffID IS NOT NULL AND (@ex IS NULL OR es.ExamID=@ex) AND (@y IS NULL OR e.AcademicYearID=@y)
ORDER BY es.ExamDate";
            return ExecuteDataTable(sql, ExamP(f));
        }

        private DataTable MarksEntryStatus(ReportFilter f)
        {
            // Per exam-subject scope: eligible students vs entered/submitted marks (NULL marks are NOT counted as entered).
            const string sql = @"
SELECT c.ClassName AS [Class], sec.SectionName AS [Section], sub.SubjectName AS [Subject],
  (SELECT COUNT(*) FROM Students st WHERE st.SectionID=es.SectionID AND ISNULL(st.Status,'Active')='Active') AS [Eligible],
  (SELECT COUNT(*) FROM ExamResults r WHERE r.ExamID=es.ExamID AND r.SubjectID=es.SubjectID AND r.Marks IS NOT NULL) AS [Entered],
  (SELECT COUNT(*) FROM ExamResults r WHERE r.ExamID=es.ExamID AND r.SubjectID=es.SubjectID AND ISNULL(r.Status,'')='Submitted') AS [Submitted],
  ISNULL(u.FullName,'—') AS [Teacher]
FROM ExamSubjects es
JOIN Classes c ON c.ClassID=es.ClassID LEFT JOIN Sections sec ON sec.SectionID=es.SectionID
JOIN Subjects sub ON sub.SubjectID=es.SubjectID
LEFT JOIN ClassSubjectTeachers cst ON cst.SectionID=es.SectionID AND cst.SubjectID=es.SubjectID
LEFT JOIN Staff sf ON sf.StaffID=cst.StaffID LEFT JOIN Users u ON u.UserID=sf.UserID
WHERE (@ex IS NULL OR es.ExamID=@ex)
ORDER BY c.ClassName, sub.SubjectName";
            return ExecuteDataTable(sql, ExamP(f));
        }

        private DataTable MarksByStatus(ReportFilter f, string mode)
        {
            string where;
            if (mode == "missing") where = "r.Marks IS NULL";
            else if (mode == "Submitted") where = "ISNULL(r.Status,'')='Submitted'";
            else if (mode == "Locked") where = "ISNULL(r.Status,'')='Locked'";
            else where = "1=1";
            string sql = @"
SELECT st.FullName AS [Student], sub.SubjectName AS [Subject], r.Marks AS [Marks], ISNULL(r.Grade,'') AS [Grade], ISNULL(r.Status,'') AS [Status]
FROM ExamResults r JOIN Students st ON st.StudentID=r.StudentID JOIN Subjects sub ON sub.SubjectID=r.SubjectID
WHERE " + where + " AND (@ex IS NULL OR r.ExamID=@ex) ORDER BY st.FullName, sub.SubjectName";
            return ExecuteDataTable(sql, ExamP(f));
        }

        private DataTable StudentResults(ReportFilter f)
        {
            const string sql = @"
SELECT st.FullName AS [Student], sub.SubjectName AS [Subject], ISNULL(esub.TotalMarks,0) AS [Maximum], r.Marks AS [Obtained],
  CASE WHEN ISNULL(esub.TotalMarks,0)>0 AND r.Marks IS NOT NULL THEN CAST(ROUND(r.Marks*100.0/esub.TotalMarks,2) AS decimal(6,2)) ELSE NULL END AS [Percentage],
  ISNULL(r.Grade,'') AS [Grade], ISNULL(r.Status,'') AS [Result Status]
FROM ExamResults r JOIN Students st ON st.StudentID=r.StudentID JOIN Subjects sub ON sub.SubjectID=r.SubjectID
LEFT JOIN ExamSubjects esub ON esub.ExamID=r.ExamID AND esub.SubjectID=r.SubjectID
WHERE (@ex IS NULL OR r.ExamID=@ex) AND (@st IS NULL OR r.StudentID=@st)
ORDER BY st.FullName, sub.SubjectName";
            return ExecuteDataTable(sql, ExamP(f));
        }

        // Class Results use the immutable snapshot (StudentExamSummaries) - historical ClassID/SectionID/Grade/Rank preserved.
        private DataTable ClassResults(ReportFilter f)
        {
            const string sql = @"
SELECT st.FullName AS [Student], c.ClassName AS [Class], sec.SectionName AS [Section],
       s.TotalObtained AS [Total], s.TotalMaximum AS [Maximum], s.AveragePercentage AS [Average %],
       ISNULL(s.OverallGrade,'') AS [Grade], ISNULL(s.Rank,0) AS [Rank], ISNULL(s.ResultStatus,'') AS [Result], ISNULL(s.PublicationStatus,'') AS [Publication]
FROM StudentExamSummaries s JOIN Students st ON st.StudentID=s.StudentID
LEFT JOIN Classes c ON c.ClassID=s.ClassID LEFT JOIN Sections sec ON sec.SectionID=s.SectionID
WHERE (@ex IS NULL OR s.ExamID=@ex) AND (@c IS NULL OR s.ClassID=@c) AND (@sec IS NULL OR s.SectionID=@sec)
ORDER BY ISNULL(s.Rank,999), s.AveragePercentage DESC";
            return ExecuteDataTable(sql, ExamP(f));
        }

        private DataTable SubjectResults(ReportFilter f)
        {
            const string sql = @"
SELECT sub.SubjectName AS [Subject], COUNT(r.Marks) AS [Students], CAST(ISNULL(AVG(r.Marks),0) AS decimal(6,2)) AS [Average],
  ISNULL(MAX(r.Marks),0) AS [Highest], ISNULL(MIN(r.Marks),0) AS [Lowest],
  SUM(CASE WHEN r.Marks>=esub.PassingMark THEN 1 ELSE 0 END) AS [Pass], SUM(CASE WHEN r.Marks<esub.PassingMark THEN 1 ELSE 0 END) AS [Fail],
  CASE WHEN COUNT(r.Marks)>0 THEN CAST(SUM(CASE WHEN r.Marks>=esub.PassingMark THEN 1 ELSE 0 END)*100.0/COUNT(r.Marks) AS decimal(6,2)) ELSE 0 END AS [Pass %]
FROM ExamResults r JOIN Subjects sub ON sub.SubjectID=r.SubjectID
JOIN ExamSubjects esub ON esub.ExamID=r.ExamID AND esub.SubjectID=r.SubjectID
WHERE r.Marks IS NOT NULL AND (@ex IS NULL OR r.ExamID=@ex)
GROUP BY sub.SubjectName ORDER BY sub.SubjectName";
            return ExecuteDataTable(sql, ExamP(f));
        }

        // Grade distribution from the published SNAPSHOT grades (never re-graded from a current scale).
        private DataTable GradeDistribution(ReportFilter f)
        {
            const string sql = @"
SELECT ISNULL(s.OverallGrade,'(none)') AS [Grade], COUNT(*) AS [Students],
  CAST(COUNT(*)*100.0 / NULLIF((SELECT COUNT(*) FROM StudentExamSummaries s2 WHERE (@ex IS NULL OR s2.ExamID=@ex)),0) AS decimal(6,2)) AS [Percent of Class]
FROM StudentExamSummaries s WHERE (@ex IS NULL OR s.ExamID=@ex)
GROUP BY s.OverallGrade ORDER BY s.OverallGrade";
            return ExecuteDataTable(sql, ExamP(f));
        }

        private DataTable PassFail(ReportFilter f)
        {
            const string sql = @"
SELECT ISNULL(s.ResultStatus,'(none)') AS [Result], COUNT(*) AS [Students]
FROM StudentExamSummaries s WHERE (@ex IS NULL OR s.ExamID=@ex) GROUP BY s.ResultStatus ORDER BY [Students] DESC";
            return ExecuteDataTable(sql, ExamP(f));
        }

        private DataTable Performers(ReportFilter f, string mode)
        {
            string order = mode == "top" ? "s.AveragePercentage DESC" : "s.AveragePercentage ASC";
            string sql = @"
SELECT TOP 20 ISNULL(s.Rank,0) AS [Rank], st.FullName AS [Student], s.TotalObtained AS [Total], s.AveragePercentage AS [Average %],
       ISNULL(s.OverallGrade,'') AS [Grade], c.ClassName AS [Class], sec.SectionName AS [Section]
FROM StudentExamSummaries s JOIN Students st ON st.StudentID=s.StudentID
LEFT JOIN Classes c ON c.ClassID=s.ClassID LEFT JOIN Sections sec ON sec.SectionID=s.SectionID
WHERE (@ex IS NULL OR s.ExamID=@ex) AND ISNULL(s.ResultStatus,'') IN ('Passed','Failed')
ORDER BY " + order;
            return ExecuteDataTable(sql, ExamP(f));
        }

        private DataTable ClassRanking(ReportFilter f)
        {
            const string sql = @"
SELECT ISNULL(s.Rank,0) AS [Rank], st.FullName AS [Student], c.ClassName AS [Class], sec.SectionName AS [Section],
       s.AveragePercentage AS [Average %], ISNULL(s.OverallGrade,'') AS [Grade]
FROM StudentExamSummaries s JOIN Students st ON st.StudentID=s.StudentID
LEFT JOIN Classes c ON c.ClassID=s.ClassID LEFT JOIN Sections sec ON sec.SectionID=s.SectionID
WHERE (@ex IS NULL OR s.ExamID=@ex) AND s.Rank IS NOT NULL AND (@sec IS NULL OR s.SectionID=@sec)
ORDER BY s.SectionID, s.Rank";
            return ExecuteDataTable(sql, ExamP(f));
        }

        private DataTable SubjectRanking(ReportFilter f)
        {
            const string sql = @"
SELECT sub.SubjectName AS [Subject], CAST(ISNULL(AVG(r.Marks),0) AS decimal(6,2)) AS [Average Mark], COUNT(r.Marks) AS [Students]
FROM ExamResults r JOIN Subjects sub ON sub.SubjectID=r.SubjectID
WHERE r.Marks IS NOT NULL AND (@ex IS NULL OR r.ExamID=@ex)
GROUP BY sub.SubjectName ORDER BY [Average Mark] DESC";
            return ExecuteDataTable(sql, ExamP(f));
        }

        private DataTable OverallAnalysis(ReportFilter f)
        {
            const string sql = @"
SELECT e.ExamName AS [Examination],
  (SELECT COUNT(*) FROM StudentExamSummaries s WHERE s.ExamID=e.ExamID) AS [Students],
  (SELECT CAST(ISNULL(AVG(s.AveragePercentage),0) AS decimal(6,2)) FROM StudentExamSummaries s WHERE s.ExamID=e.ExamID) AS [Class Average %],
  (SELECT COUNT(*) FROM StudentExamSummaries s WHERE s.ExamID=e.ExamID AND s.ResultStatus='Passed') AS [Passed],
  (SELECT COUNT(*) FROM StudentExamSummaries s WHERE s.ExamID=e.ExamID AND s.ResultStatus='Failed') AS [Failed]
FROM Exams e WHERE (@ex IS NULL OR e.ExamID=@ex) AND (@y IS NULL OR e.AcademicYearID=@y)
ORDER BY e.StartDate DESC";
            return ExecuteDataTable(sql, ExamP(f));
        }

        private DataTable Summaries(ReportFilter f, string pubMode)
        {
            string pub = "";
            if (pubMode == "Published") pub = " AND s.PublicationStatus='Published'";
            else if (pubMode == "!Published") pub = " AND ISNULL(s.PublicationStatus,'')<>'Published'";
            string sql = @"
SELECT e.ExamName AS [Examination], st.FullName AS [Student], c.ClassName AS [Class], sec.SectionName AS [Section],
       s.TotalObtained AS [Total], s.AveragePercentage AS [Average %], ISNULL(s.OverallGrade,'') AS [Grade], ISNULL(s.Rank,0) AS [Rank],
       ISNULL(s.ResultStatus,'') AS [Result], ISNULL(s.PublicationStatus,'') AS [Publication], s.PublishedAt AS [Published]
FROM StudentExamSummaries s JOIN Students st ON st.StudentID=s.StudentID JOIN Exams e ON e.ExamID=s.ExamID
LEFT JOIN Classes c ON c.ClassID=s.ClassID LEFT JOIN Sections sec ON sec.SectionID=s.SectionID
WHERE (@ex IS NULL OR s.ExamID=@ex)" + pub + @"
ORDER BY e.ExamName, ISNULL(s.Rank,999)";
            return ExecuteDataTable(sql, ExamP(f));
        }

        private System.Data.SqlClient.SqlParameter[] ExamP(ReportFilter f)
        {
            return new[]
            {
                P("@y",(object)f.YearID??DBNull.Value), P("@tm",(object)f.TermID??DBNull.Value), P("@ex",(object)f.ExamID??DBNull.Value),
                P("@c",(object)f.ClassID??DBNull.Value), P("@sec",(object)f.SectionID??DBNull.Value), P("@st",(object)f.StudentID??DBNull.Value)
            };
        }
    }
}
