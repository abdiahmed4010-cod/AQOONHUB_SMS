using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AQOONHUB_SMS.Modules.Reports
{
    /// <summary>
    /// Single canonical data-access layer for the Reports module. Stage 1 provides the
    /// connection foundation, shared lookups and the real Overview-dashboard queries.
    /// Category report methods are added in later stages (partial-class files may be used).
    /// Direct ADO.NET, fully parameterised. No SQL, table or column names ever come from
    /// user input - report keys resolve through ReportCatalog.
    /// </summary>
    public sealed partial class ReportsRepository
    {
        private readonly string _connectionString;

        public ReportsRepository()
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["AQOONHUB_DB"];
            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
                throw new ConfigurationErrorsException("Connection string 'AQOONHUB_DB' was not found.");
            _connectionString = settings.ConnectionString;
        }

        // ================================================================
        // OVERVIEW DASHBOARD (real data only; 0/empty states when no history)
        // ================================================================
        public DataRow GetOverviewSummary(string role)
        {
            const string sql = @"
SELECT
  (SELECT COUNT(*) FROM ReportExports) AS TotalGenerated,
  (SELECT COUNT(*) FROM ReportExports WHERE CAST(GeneratedAt AS date)=CAST(GETDATE() AS date)) AS GeneratedToday,
  (SELECT COUNT(*) FROM SavedReports WHERE ISNULL(IsActive,1)=1) AS SavedCount,
  (SELECT COUNT(*) FROM ScheduledReports) AS ScheduledCount,
  (SELECT COUNT(*) FROM ReportExports WHERE GeneratedAt >= DATEADD(DAY,-30,GETDATE())) AS RecentExports;";
            DataRow r = ExecuteDataTable(sql, null).Rows[0];

            DataTable outT = new DataTable();
            foreach (string c in new[] { "TotalGenerated", "GeneratedToday", "SavedCount", "ScheduledCount", "RecentExports", "ActiveCategories" })
                outT.Columns.Add(c, typeof(int));
            outT.Rows.Add(N(r["TotalGenerated"]), N(r["GeneratedToday"]), N(r["SavedCount"]), N(r["ScheduledCount"]), N(r["RecentExports"]),
                ReportAuthorization.VisibleCategories(role).Count);
            return outT.Rows[0];
        }

        /// <summary>Report generations per month for the last 12 months (real export rows).</summary>
        public DataTable GetMonthlyGeneration()
        {
            const string sql = @"
SELECT DATEFROMPARTS(YEAR(GeneratedAt),MONTH(GeneratedAt),1) AS Bucket, COUNT(*) AS Cnt
FROM ReportExports
WHERE GeneratedAt >= DATEADD(MONTH,-11, DATEFROMPARTS(YEAR(GETDATE()),MONTH(GETDATE()),1))
GROUP BY DATEFROMPARTS(YEAR(GeneratedAt),MONTH(GeneratedAt),1)
ORDER BY Bucket;";
            return ExecuteDataTable(sql, null);
        }

        public DataTable GetMostUsedReports(int top)
        {
            string sql = @"
SELECT TOP (" + Clamp(top, 1, 20) + @") ReportName, COUNT(*) AS Uses
FROM ReportAuditLogs WHERE Action IN ('Viewed','Generated','Exported') AND ReportName IS NOT NULL
GROUP BY ReportName ORDER BY COUNT(*) DESC;";
            return ExecuteDataTable(sql, null);
        }

        public DataTable GetRecentActivity(int top)
        {
            string sql = @"
SELECT TOP (" + Clamp(top, 1, 20) + @") a.Action, a.ReportName, a.Category, a.ResultStatus, a.CreatedAt,
       COALESCE(u.FullName,'—') AS UserName
FROM ReportAuditLogs a LEFT JOIN Users u ON u.UserID=a.UserID
ORDER BY a.ReportAuditLogID DESC;";
            return ExecuteDataTable(sql, null);
        }

        public DataTable GetRecentExports(int top)
        {
            string sql = @"
SELECT TOP (" + Clamp(top, 1, 20) + @") e.ReportName, e.Category, e.ExportFormat, e.Status, e.GeneratedAt,
       COALESCE(u.FullName,'—') AS GeneratedByName
FROM ReportExports e LEFT JOIN Users u ON u.UserID=e.GeneratedBy
ORDER BY e.ReportExportID DESC;";
            return ExecuteDataTable(sql, null);
        }

        public DataTable GetScheduledPreview(int top)
        {
            string sql = @"
SELECT TOP (" + Clamp(top, 1, 20) + @") s.Frequency, s.ExportFormat, s.Status, s.NextRunAt,
       ISNULL(sr.ReportName,'(unsaved)') AS ReportName
FROM ScheduledReports s LEFT JOIN SavedReports sr ON sr.SavedReportID=s.SavedReportID
ORDER BY s.ScheduledReportID DESC;";
            return ExecuteDataTable(sql, null);
        }

        /// <summary>Availability + row count of the report data sources the module reads from.</summary>
        public DataTable GetDataSourceStatus()
        {
            // (label, table) pairs — Transport/Hostel intentionally excluded.
            var sources = new[]
            {
                new[]{"Students","Students"}, new[]{"Academics","Classes"}, new[]{"Examinations","Exams"},
                new[]{"Attendance","AttendanceSessions"}, new[]{"Finance","FeePayments"}, new[]{"Payroll","PayrollRecords"},
                new[]{"Staff","Staff"}, new[]{"Guardians","Guardians"}, new[]{"Users","Users"}, new[]{"Library","Books"}
            };
            DataTable outT = new DataTable();
            outT.Columns.Add("Source", typeof(string)); outT.Columns.Add("Available", typeof(bool)); outT.Columns.Add("Rows", typeof(long));
            foreach (var s in sources)
            {
                long rows = 0; bool available = false;
                object exists = ExecuteScalar("SELECT OBJECT_ID(@t,'U')", new[] { P("@t", "dbo." + s[1]) });
                if (exists != null && exists != DBNull.Value)
                {
                    available = true;
                    // table name comes from our own fixed whitelist above, never from user input
                    object c = ExecuteScalar("SELECT COUNT_BIG(*) FROM [" + s[1] + "]", null);
                    rows = c == null || c == DBNull.Value ? 0 : Convert.ToInt64(c);
                }
                outT.Rows.Add(s[0], available, rows);
            }
            return outT;
        }

        /// <summary>Append-only report audit entry.</summary>
        public void LogAudit(int? userId, string action, string reportKey, string reportName, string category, string filterSummary, string resultStatus, string ip)
        {
            const string sql = @"
INSERT INTO ReportAuditLogs (UserID, Action, ReportKey, ReportName, Category, FilterSummary, ResultStatus, IpAddress, CreatedAt)
VALUES (@u,@a,@k,@n,@c,@f,@r,@ip,GETDATE())";
            ExecuteNonQuery(sql, new[]
            {
                P("@u", (object)userId ?? DBNull.Value), P("@a", action ?? ""), P("@k", (object)reportKey ?? DBNull.Value),
                P("@n", (object)reportName ?? DBNull.Value), P("@c", (object)category ?? DBNull.Value),
                P("@f", (object)filterSummary ?? DBNull.Value), P("@r", resultStatus ?? "Success"), P("@ip", (object)ip ?? DBNull.Value)
            });
        }

        /// <summary>Record export metadata (never the file content itself).</summary>
        public void RecordExport(string reportKey, string reportName, string category, string format, string filterSummary, string fileName, long fileSize, int? userId)
        {
            const string sql = @"
INSERT INTO ReportExports (ReportKey, ReportName, Category, ExportFormat, FilterSummary, FileName, FileSize, Status, GeneratedBy, GeneratedAt)
VALUES (@k,@n,@c,@fmt,@f,@fn,@sz,'Generated',@u,GETDATE())";
            ExecuteNonQuery(sql, new[]
            {
                P("@k", reportKey ?? ""), P("@n", reportName ?? ""), P("@c", category ?? ""), P("@fmt", format ?? "CSV"),
                P("@f", (object)filterSummary ?? DBNull.Value), P("@fn", (object)fileName ?? DBNull.Value),
                P("@sz", fileSize), P("@u", (object)userId ?? DBNull.Value)
            });
        }

        // ================================================================
        // SHARED LOOKUPS
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

        // ================================================================
        // HELPERS
        // ================================================================
        private static int N(object o) { return o == null || o == DBNull.Value ? 0 : Convert.ToInt32(o); }
        private static int Clamp(int v, int lo, int hi) { return v < lo ? lo : (v > hi ? hi : v); }

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
    }
}
