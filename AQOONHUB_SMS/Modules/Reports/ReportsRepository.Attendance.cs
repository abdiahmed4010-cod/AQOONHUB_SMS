using System;
using System.Data;

namespace AQOONHUB_SMS.Modules.Reports
{
    public sealed partial class ReportsRepository
    {
        private DataTable Stage3Attendance(string handler, ReportFilter f, bool allowSensitive)
        {
            switch (handler)
            {
                case "att-by-date": return AttByDate(f);
                case "att-individual": return AttIndividual(f);
                case "att-class": return AttClass(f);        // class/section aggregate
                case "att-section": return AttClass(f);
                case "att-subject": return AttByDate(f);
                case "att-present": return AttStatusList(f, "Present");
                case "att-absent": return AttStatusList(f, "Absent");
                case "att-late": return AttStatusList(f, "Late");
                case "att-excused": return AttStatusList(f, "Excused");
                case "att-low-attendance": return AttLow(f);
                case "att-consecutive": return AlertList(f, "ConsecutiveAbsence");
                case "att-frequent-late": return AlertList(f, "FrequentLate");
                case "att-daily-summary": return AttSummary(f, "day");
                case "att-weekly-summary": return AttSummary(f, "week");
                case "att-monthly-summary": return AttSummary(f, "month");
                case "att-trend": return AttSummary(f, "day");
                case "att-calendar": return AttSummary(f, "day");
                case "att-unsubmitted": return AttUnsubmitted(f);
                case "att-alerts": return AlertList(f, null);
                case "att-import-history": return AttImportHistory();

                default: return Stage3Finance(handler, f, allowSensitive);
            }
        }

        // Settings-aware attendance rate expression (matches the Attendance module policy).
        private void AttSettings(out bool includeLate, out bool excludeExcused, out decimal lowThreshold)
        {
            includeLate = true; excludeExcused = true; lowThreshold = 85m;
            if (ExecuteScalar("SELECT OBJECT_ID('dbo.AttendanceSettings','U')", null) == DBNull.Value) return;
            DataTable t = ExecuteDataTable("SELECT TOP 1 IncludeLateAsAttended, ExcludeExcusedFromRate, LowAttendanceThreshold FROM AttendanceSettings ORDER BY AttendanceSettingsID", null);
            if (t.Rows.Count == 0) return;
            includeLate = Convert.ToBoolean(t.Rows[0]["IncludeLateAsAttended"]);
            excludeExcused = Convert.ToBoolean(t.Rows[0]["ExcludeExcusedFromRate"]);
            lowThreshold = Convert.ToDecimal(t.Rows[0]["LowAttendanceThreshold"]);
        }

        private string OfficialWhere() { return " ss.Status IN ('Submitted','Locked') "; }

        private DataTable AttByDate(ReportFilter f)
        {
            string sql = @"
SELECT ss.AttendanceDate AS [Date], st.FullName AS [Student], st.StudentCode AS [Code], c.ClassName AS [Class], sec.SectionName AS [Section],
       ss.SessionType AS [Type], r.AttendanceStatus AS [Status], r.CheckInTime AS [Check-in], r.LateMinutes AS [Late Min],
       ISNULL(r.Remarks,'') AS [Remarks], ISNULL(u.FullName,'') AS [Marked By], ss.Status AS [Session]
FROM AttendanceSessions ss JOIN AttendanceRecords r ON r.AttendanceSessionID=ss.AttendanceSessionID
JOIN Students st ON st.StudentID=r.StudentID JOIN Classes c ON c.ClassID=ss.ClassID JOIN Sections sec ON sec.SectionID=ss.SectionID
LEFT JOIN Users u ON u.UserID=ss.MarkedBy
WHERE " + OfficialWhere() + @" AND (@y IS NULL OR ss.AcademicYearID=@y) AND (@c IS NULL OR ss.ClassID=@c) AND (@sec IS NULL OR ss.SectionID=@sec)
  AND (@from IS NULL OR ss.AttendanceDate>=@from) AND (@to IS NULL OR ss.AttendanceDate<=@to)
ORDER BY ss.AttendanceDate DESC, st.FullName";
            return ExecuteDataTable(sql, AttP(f));
        }

        private DataTable AttIndividual(ReportFilter f)
        {
            // Historical: joins through AttendanceSessions (not current SectionID).
            string sql = @"
SELECT ss.AttendanceDate AS [Date], c.ClassName AS [Class], sec.SectionName AS [Section], ss.SessionType AS [Type],
       r.AttendanceStatus AS [Status], r.CheckInTime AS [Check-in], r.LateMinutes AS [Late Min]
FROM AttendanceRecords r JOIN AttendanceSessions ss ON ss.AttendanceSessionID=r.AttendanceSessionID
JOIN Classes c ON c.ClassID=ss.ClassID JOIN Sections sec ON sec.SectionID=ss.SectionID
WHERE " + OfficialWhere() + @" AND r.StudentID=@st ORDER BY ss.AttendanceDate DESC";
            return ExecuteDataTable(sql, AttP(f));
        }

        private DataTable AttClass(ReportFilter f)
        {
            bool incLate, excExc; decimal thr; AttSettings(out incLate, out excExc, out thr);
            string attended = incLate ? "(P+L)" : "P";
            string denom = excExc ? "(P+A+L)" : "(P+A+L+E)";
            string sql = @"
;WITH agg AS (
  SELECT r.StudentID,
    SUM(CASE WHEN r.AttendanceStatus='Present' THEN 1 ELSE 0 END) P,
    SUM(CASE WHEN r.AttendanceStatus='Absent'  THEN 1 ELSE 0 END) A,
    SUM(CASE WHEN r.AttendanceStatus='Late'    THEN 1 ELSE 0 END) L,
    SUM(CASE WHEN r.AttendanceStatus='Excused' THEN 1 ELSE 0 END) E, COUNT(*) Total
  FROM AttendanceSessions ss JOIN AttendanceRecords r ON r.AttendanceSessionID=ss.AttendanceSessionID
  WHERE " + OfficialWhere() + @" AND (@y IS NULL OR ss.AcademicYearID=@y) AND (@c IS NULL OR ss.ClassID=@c) AND (@sec IS NULL OR ss.SectionID=@sec)
    AND (@from IS NULL OR ss.AttendanceDate>=@from) AND (@to IS NULL OR ss.AttendanceDate<=@to)
  GROUP BY r.StudentID)
SELECT st.FullName AS [Student], agg.P AS [Present], agg.A AS [Absent], agg.L AS [Late], agg.E AS [Excused], agg.Total AS [Sessions],
  CASE WHEN " + denom + ">0 THEN CAST(" + attended + "*100.0/" + denom + @" AS decimal(6,2)) ELSE 0 END AS [Attendance %],
  CASE WHEN " + denom + ">0 AND " + attended + "*100.0/" + denom + "<@thr THEN 'At Risk' ELSE 'Good' END AS [Risk]" + @"
FROM agg JOIN Students st ON st.StudentID=agg.StudentID ORDER BY st.FullName";
            var ps = new System.Collections.Generic.List<System.Data.SqlClient.SqlParameter>(AttP(f)); ps.Add(P("@thr", thr));
            return ExecuteDataTable(sql, ps.ToArray());
        }

        private DataTable AttStatusList(ReportFilter f, string status)
        {
            string sql = @"
SELECT ss.AttendanceDate AS [Date], st.FullName AS [Student], st.StudentCode AS [Code], c.ClassName AS [Class], sec.SectionName AS [Section], r.CheckInTime AS [Check-in]
FROM AttendanceSessions ss JOIN AttendanceRecords r ON r.AttendanceSessionID=ss.AttendanceSessionID
JOIN Students st ON st.StudentID=r.StudentID JOIN Classes c ON c.ClassID=ss.ClassID JOIN Sections sec ON sec.SectionID=ss.SectionID
WHERE " + OfficialWhere() + @" AND r.AttendanceStatus=@status AND (@y IS NULL OR ss.AcademicYearID=@y) AND (@c IS NULL OR ss.ClassID=@c) AND (@sec IS NULL OR ss.SectionID=@sec)
  AND (@from IS NULL OR ss.AttendanceDate>=@from) AND (@to IS NULL OR ss.AttendanceDate<=@to)
ORDER BY ss.AttendanceDate DESC, st.FullName";
            var ps = new System.Collections.Generic.List<System.Data.SqlClient.SqlParameter>(AttP(f)); ps.Add(P("@status", status));
            return ExecuteDataTable(sql, ps.ToArray());
        }

        private DataTable AttLow(ReportFilter f)
        {
            DataTable all = AttClass(f);
            DataView dv = all.DefaultView; dv.RowFilter = "[Risk]='At Risk'"; return dv.ToTable();
        }

        private DataTable AttSummary(ReportFilter f, string grain)
        {
            string bucket = grain == "month" ? "DATEFROMPARTS(YEAR(ss.AttendanceDate),MONTH(ss.AttendanceDate),1)"
                : grain == "week" ? "DATEADD(DAY, -(DATEPART(WEEKDAY, ss.AttendanceDate)-1), CAST(ss.AttendanceDate AS date))"
                : "CAST(ss.AttendanceDate AS date)";
            string sql = @"
SELECT " + bucket + @" AS [Period],
  SUM(CASE WHEN r.AttendanceStatus='Present' THEN 1 ELSE 0 END) AS [Present],
  SUM(CASE WHEN r.AttendanceStatus='Absent'  THEN 1 ELSE 0 END) AS [Absent],
  SUM(CASE WHEN r.AttendanceStatus='Late'    THEN 1 ELSE 0 END) AS [Late],
  SUM(CASE WHEN r.AttendanceStatus='Excused' THEN 1 ELSE 0 END) AS [Excused], COUNT(*) AS [Total]
FROM AttendanceSessions ss JOIN AttendanceRecords r ON r.AttendanceSessionID=ss.AttendanceSessionID
WHERE " + OfficialWhere() + @" AND (@y IS NULL OR ss.AcademicYearID=@y) AND (@c IS NULL OR ss.ClassID=@c) AND (@sec IS NULL OR ss.SectionID=@sec)
  AND (@from IS NULL OR ss.AttendanceDate>=@from) AND (@to IS NULL OR ss.AttendanceDate<=@to)
GROUP BY " + bucket + " ORDER BY [Period]";
            return ExecuteDataTable(sql, AttP(f));
        }

        private DataTable AttUnsubmitted(ReportFilter f)
        {
            // Specifically reads DRAFT (operational) sessions.
            string sql = @"
SELECT ss.AttendanceDate AS [Date], c.ClassName AS [Class], sec.SectionName AS [Section], ss.SessionType AS [Type], ss.Status AS [Status], ss.CreatedAt AS [Created]
FROM AttendanceSessions ss JOIN Classes c ON c.ClassID=ss.ClassID JOIN Sections sec ON sec.SectionID=ss.SectionID
WHERE ss.Status='Draft' AND (@y IS NULL OR ss.AcademicYearID=@y) AND (@c IS NULL OR ss.ClassID=@c) AND (@sec IS NULL OR ss.SectionID=@sec)
ORDER BY ss.AttendanceDate DESC";
            return ExecuteDataTable(sql, AttP(f));
        }

        private DataTable AlertList(ReportFilter f, string type)
        {
            if (ExecuteScalar("SELECT OBJECT_ID('dbo.AttendanceAlerts','U')", null) == DBNull.Value) return null;
            // No internal ResolutionNotes exposed.
            string sql = @"
SELECT a.AlertType AS [Type], ISNULL(st.FullName,'') AS [Student], a.Title AS [Title], a.Description AS [Description],
       a.Severity AS [Severity], a.Status AS [Status], a.LastDetectedAt AS [Detected]
FROM AttendanceAlerts a LEFT JOIN Students st ON st.StudentID=a.StudentID
WHERE (@type='' OR a.AlertType=@type) ORDER BY a.LastDetectedAt DESC";
            return ExecuteDataTable(sql, new[] { P("@type", type ?? "") });
        }

        private DataTable AttImportHistory()
        {
            if (ExecuteScalar("SELECT OBJECT_ID('dbo.AttendanceImportBatches','U')", null) == DBNull.Value) return null;
            // No file content / no file path exposed.
            string sql = @"
SELECT b.OriginalFileName AS [File], c.ClassName AS [Class], sec.SectionName AS [Section], b.SessionType AS [Type],
       b.TotalRows AS [Total], b.ValidRows AS [Valid], b.ErrorRows AS [Errors], b.ImportedSessions AS [Sessions], b.ImportedRecords AS [Records],
       ISNULL(u.FullName,'') AS [Imported By], b.ImportedAt AS [Imported], b.ImportStatus AS [Status]
FROM AttendanceImportBatches b LEFT JOIN Classes c ON c.ClassID=b.ClassID LEFT JOIN Sections sec ON sec.SectionID=b.SectionID
LEFT JOIN Users u ON u.UserID=b.ImportedBy ORDER BY b.AttendanceImportBatchID DESC";
            return ExecuteDataTable(sql, null);
        }

        private System.Data.SqlClient.SqlParameter[] AttP(ReportFilter f)
        {
            return new[]
            {
                P("@y",(object)f.YearID??DBNull.Value), P("@c",(object)f.ClassID??DBNull.Value), P("@sec",(object)f.SectionID??DBNull.Value),
                P("@st",(object)f.StudentID??DBNull.Value), P("@from",(object)f.From??DBNull.Value), P("@to",(object)f.To??DBNull.Value)
            };
        }
    }
}
