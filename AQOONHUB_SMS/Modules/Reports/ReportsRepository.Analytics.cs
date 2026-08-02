using System;
using System.Data;

namespace AQOONHUB_SMS.Modules.Reports
{
    public sealed partial class ReportsRepository
    {
        private DataTable Stage4Analytics(string handler, ReportFilter f, bool allowSensitive)
        {
            switch (handler)
            {
                case "analytics-trend": return GetStudentPerformanceTrend(f);
                case "analytics-classes": return GetClassPerformanceComparison(f, false);
                case "analytics-subjects": return GetSubjectPerformanceComparison(f);
                case "analytics-pass-fail": return GetPassFailDistribution(f);
                case "analytics-enrollment": return GetEnrollmentGrowth(f);
                case "analytics-years": return GetAcademicYearComparison(f);
                case "analytics-attendance-exam": return GetAttendanceExamRelationship(f);
                case "analytics-top-classes": return GetClassPerformanceComparison(f, false);
                case "analytics-low-classes": return GetClassPerformanceComparison(f, true);
                case "analytics-at-risk": return GetAtRiskStudents(f);
                default: throw new InvalidOperationException("Unknown report handler.");
            }
        }

        private System.Data.SqlClient.SqlParameter[] AnalyticsP(ReportFilter f)
        {
            return new[] { P("@y", (object)f.YearID ?? DBNull.Value), P("@t", (object)f.TermID ?? DBNull.Value), P("@e", (object)f.ExamID ?? DBNull.Value), P("@c", (object)f.ClassID ?? DBNull.Value), P("@sec", (object)f.SectionID ?? DBNull.Value) };
        }

        public DataTable GetPerformanceSummary(ReportFilter f)
        {
            const string sql = @"SELECT CAST(AVG(CAST(s.AveragePercentage AS decimal(18,4))) AS decimal(6,2)) AS AveragePerformance,
 CAST(100.0*SUM(CASE WHEN s.ResultStatus='Passed' THEN 1 ELSE 0 END)/NULLIF(SUM(CASE WHEN s.ResultStatus IN ('Passed','Failed') THEN 1 ELSE 0 END),0) AS decimal(6,2)) AS PassRate,
 CAST(100.0*SUM(CASE WHEN s.ResultStatus='Failed' THEN 1 ELSE 0 END)/NULLIF(SUM(CASE WHEN s.ResultStatus IN ('Passed','Failed') THEN 1 ELSE 0 END),0) AS decimal(6,2)) AS FailureRate,
 COUNT(DISTINCT s.StudentID) AS TotalStudents, SUM(CASE WHEN s.ResultStatus='Failed' THEN 1 ELSE 0 END) AS AtRiskStudents
FROM StudentExamSummaries s JOIN Exams e ON e.ExamID=s.ExamID
WHERE s.PublicationStatus='Published' AND (@y IS NULL OR s.AcademicYearID=@y) AND (@t IS NULL OR e.TermID=@t) AND (@e IS NULL OR s.ExamID=@e) AND (@c IS NULL OR s.ClassID=@c) AND (@sec IS NULL OR s.SectionID=@sec)";
            return ExecuteDataTable(sql, AnalyticsP(f));
        }

        public DataTable GetStudentPerformanceTrend(ReportFilter f)
        {
            return ExecuteDataTable(@"SELECT e.ExamName AS [Label], CAST(AVG(s.AveragePercentage) AS decimal(6,2)) AS [Value]
FROM StudentExamSummaries s JOIN Exams e ON e.ExamID=s.ExamID WHERE s.PublicationStatus='Published'
AND (@y IS NULL OR s.AcademicYearID=@y) AND (@t IS NULL OR e.TermID=@t) AND (@e IS NULL OR s.ExamID=@e) AND (@c IS NULL OR s.ClassID=@c) AND (@sec IS NULL OR s.SectionID=@sec)
GROUP BY e.ExamID,e.ExamName,e.StartDate ORDER BY e.StartDate,e.ExamID", AnalyticsP(f));
        }

        public DataTable GetClassPerformanceComparison(ReportFilter f, bool ascending)
        {
            string sql = @"SELECT c.ClassName AS [Label], CAST(AVG(s.AveragePercentage) AS decimal(6,2)) AS [Value], COUNT(*) AS [Students]
FROM StudentExamSummaries s JOIN Exams e ON e.ExamID=s.ExamID JOIN Classes c ON c.ClassID=s.ClassID
WHERE s.PublicationStatus='Published' AND (@y IS NULL OR s.AcademicYearID=@y) AND (@t IS NULL OR e.TermID=@t) AND (@e IS NULL OR s.ExamID=@e) AND (@c IS NULL OR s.ClassID=@c) AND (@sec IS NULL OR s.SectionID=@sec)
GROUP BY c.ClassID,c.ClassName ORDER BY [Value] " + (ascending ? "ASC" : "DESC");
            return ExecuteDataTable(sql, AnalyticsP(f));
        }

        public DataTable GetSubjectPerformanceComparison(ReportFilter f)
        {
            return ExecuteDataTable(@"SELECT sub.SubjectName AS [Label], CAST(AVG(100.0*r.Marks/NULLIF(r.TotalMarks,0)) AS decimal(6,2)) AS [Value], COUNT(*) AS [Results]
FROM ExamResults r JOIN StudentExamSummaries ss ON ss.ExamID=r.ExamID AND ss.StudentID=r.StudentID JOIN Exams e ON e.ExamID=r.ExamID JOIN Subjects sub ON sub.SubjectID=r.SubjectID
WHERE ss.PublicationStatus='Published' AND r.Status='Submitted' AND r.Marks IS NOT NULL AND (@y IS NULL OR ss.AcademicYearID=@y) AND (@t IS NULL OR e.TermID=@t) AND (@e IS NULL OR r.ExamID=@e) AND (@c IS NULL OR ss.ClassID=@c) AND (@sec IS NULL OR ss.SectionID=@sec)
GROUP BY sub.SubjectID,sub.SubjectName ORDER BY [Value] DESC", AnalyticsP(f));
        }

        public DataTable GetPassFailDistribution(ReportFilter f)
        {
            return ExecuteDataTable(@"SELECT ResultStatus AS [Label], COUNT(*) AS [Value] FROM StudentExamSummaries s JOIN Exams e ON e.ExamID=s.ExamID
WHERE s.PublicationStatus='Published' AND s.ResultStatus IN ('Passed','Failed') AND (@y IS NULL OR s.AcademicYearID=@y) AND (@t IS NULL OR e.TermID=@t) AND (@e IS NULL OR s.ExamID=@e) AND (@c IS NULL OR s.ClassID=@c) AND (@sec IS NULL OR s.SectionID=@sec) GROUP BY ResultStatus", AnalyticsP(f));
        }

        public DataTable GetEnrollmentGrowth(ReportFilter f)
        {
            return ExecuteDataTable("SELECT CONVERT(char(7),EnrollmentDate,120) AS [Label], COUNT(*) AS [Value] FROM Students WHERE EnrollmentDate IS NOT NULL AND (@y IS NULL OR AcademicYearID=@y) GROUP BY CONVERT(char(7),EnrollmentDate,120) ORDER BY [Label]", new[] { P("@y", (object)f.YearID ?? DBNull.Value) });
        }

        public DataTable GetAcademicYearComparison(ReportFilter f)
        {
            return ExecuteDataTable(@"SELECT y.YearName AS [Label], CAST(AVG(s.AveragePercentage) AS decimal(6,2)) AS [Value] FROM StudentExamSummaries s JOIN AcademicYears y ON y.AcademicYearID=s.AcademicYearID WHERE s.PublicationStatus='Published' GROUP BY y.YearName,y.StartDate ORDER BY y.StartDate", null);
        }

        public DataTable GetAttendanceExamRelationship(ReportFilter f)
        {
            return ExecuteDataTable(@"WITH a AS (SELECT ar.StudentID, ses.AcademicYearID, ses.TermID, CAST(100.0*SUM(CASE WHEN ar.AttendanceStatus IN ('Present','Late') THEN 1 ELSE 0 END)/NULLIF(COUNT(*),0) AS decimal(6,2)) AttendanceRate
FROM AttendanceRecords ar JOIN AttendanceSessions ses ON ses.AttendanceSessionID=ar.AttendanceSessionID WHERE ses.Status IN ('Submitted','Locked') GROUP BY ar.StudentID,ses.AcademicYearID,ses.TermID)
SELECT st.FullName AS [Student], a.AttendanceRate AS [Attendance], CAST(AVG(s.AveragePercentage) AS decimal(6,2)) AS [Performance]
FROM a JOIN StudentExamSummaries s ON s.StudentID=a.StudentID AND s.AcademicYearID=a.AcademicYearID JOIN Exams e ON e.ExamID=s.ExamID AND (a.TermID IS NULL OR e.TermID=a.TermID) JOIN Students st ON st.StudentID=s.StudentID
WHERE s.PublicationStatus='Published' AND (@y IS NULL OR s.AcademicYearID=@y) AND (@t IS NULL OR e.TermID=@t) AND (@e IS NULL OR s.ExamID=@e) AND (@c IS NULL OR s.ClassID=@c) AND (@sec IS NULL OR s.SectionID=@sec)
GROUP BY st.FullName,a.AttendanceRate ORDER BY st.FullName", AnalyticsP(f));
        }

        public DataTable GetAtRiskStudents(ReportFilter f)
        {
            return ExecuteDataTable(@"WITH a AS (SELECT ar.StudentID, ses.AcademicYearID, CAST(100.0*SUM(CASE WHEN ar.AttendanceStatus IN ('Present','Late') THEN 1 ELSE 0 END)/NULLIF(COUNT(*),0) AS decimal(6,2)) AttendanceRate FROM AttendanceRecords ar JOIN AttendanceSessions ses ON ses.AttendanceSessionID=ar.AttendanceSessionID WHERE ses.Status IN ('Submitted','Locked') GROUP BY ar.StudentID,ses.AcademicYearID)
SELECT st.FullName AS [Student], c.ClassName AS [Historical Class], sec.SectionName AS [Historical Section], s.AveragePercentage AS [Exam Average], a.AttendanceRate AS [Attendance Rate], CASE WHEN s.ResultStatus='Failed' THEN 1 ELSE 0 END AS [Failed Subjects],
CASE WHEN s.ResultStatus='Failed' AND a.AttendanceRate<cfg.LowAttendanceThreshold THEN 'Low performance; Low attendance' WHEN s.ResultStatus='Failed' THEN 'Low performance' ELSE 'Low attendance' END AS [Risk Indicators]
FROM StudentExamSummaries s JOIN Students st ON st.StudentID=s.StudentID LEFT JOIN Classes c ON c.ClassID=s.ClassID LEFT JOIN Sections sec ON sec.SectionID=s.SectionID LEFT JOIN a ON a.StudentID=s.StudentID AND a.AcademicYearID=s.AcademicYearID CROSS JOIN (SELECT TOP 1 LowAttendanceThreshold FROM AttendanceSettings ORDER BY AttendanceSettingsID) cfg JOIN Exams e ON e.ExamID=s.ExamID
WHERE s.PublicationStatus='Published' AND (s.ResultStatus='Failed' OR a.AttendanceRate<cfg.LowAttendanceThreshold) AND (@y IS NULL OR s.AcademicYearID=@y) AND (@t IS NULL OR e.TermID=@t) AND (@e IS NULL OR s.ExamID=@e) AND (@c IS NULL OR s.ClassID=@c) AND (@sec IS NULL OR s.SectionID=@sec) ORDER BY s.AveragePercentage", AnalyticsP(f));
        }
    }
}
