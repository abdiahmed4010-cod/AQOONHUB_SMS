using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;


namespace AQOONHUB_SMS.App_Code.DataAccess
{
    public class ReportDAL
    {
        private DatabaseHelper db;

        public ReportDAL()
        {
            db = new DatabaseHelper();
        }

        public List<Report> GetAllReports(string status, string reportType)
        {
            List<Report> reports = new List<Report>();

            string query = @"
                SELECT r.*, u.FullName as GeneratedByName
                FROM Reports r
                INNER JOIN Users u ON r.GeneratedBy = u.UserID
                WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(status))
            {
                query += " AND r.Status = @Status";
                parameters.Add(new SqlParameter("@Status", status));
            }

            if (!string.IsNullOrEmpty(reportType))
            {
                query += " AND r.ReportType = @ReportType";
                parameters.Add(new SqlParameter("@ReportType", reportType));
            }

            query += " ORDER BY r.CreatedAt DESC";

            DataTable dt = db.ExecuteQuery(query, parameters.ToArray());

            foreach (DataRow row in dt.Rows)
            {
                reports.Add(MapToReport(row));
            }

            return reports;
        }

        public List<Report> GetAllReports()
        {
            return GetAllReports(null, null);
        }

        public Report GetReportById(int reportId)
        {
            string query = @"
                SELECT r.*, u.FullName as GeneratedByName
                FROM Reports r
                INNER JOIN Users u ON r.GeneratedBy = u.UserID
                WHERE r.ReportID = @ReportID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ReportID", reportId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
                return MapToReport(dt.Rows[0]);

            return null;
        }

        public int AddReport(Report report)
        {
            string query = @"
                INSERT INTO Reports (ReportName, ReportType, Description, Parameters,
                    FilePath, Status, GeneratedBy, GeneratedAt, CreatedAt)
                VALUES (@ReportName, @ReportType, @Description, @Parameters,
                    @FilePath, @Status, @GeneratedBy, @GeneratedAt, @CreatedAt);
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ReportName", report.ReportName),
                new SqlParameter("@ReportType", report.ReportType),
                new SqlParameter("@Description", string.IsNullOrEmpty(report.Description) ? (object)DBNull.Value : report.Description),
                new SqlParameter("@Parameters", string.IsNullOrEmpty(report.Parameters) ? (object)DBNull.Value : report.Parameters),
                new SqlParameter("@FilePath", string.IsNullOrEmpty(report.FilePath) ? (object)DBNull.Value : report.FilePath),
                new SqlParameter("@Status", report.Status),
                new SqlParameter("@GeneratedBy", report.GeneratedBy),
                new SqlParameter("@GeneratedAt", report.GeneratedAt),
                new SqlParameter("@CreatedAt", report.CreatedAt)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        public bool UpdateReport(Report report)
        {
            string query = @"
                UPDATE Reports SET
                    ReportName = @ReportName,
                    ReportType = @ReportType,
                    Description = @Description,
                    Parameters = @Parameters,
                    FilePath = @FilePath,
                    Status = @Status,
                    GeneratedBy = @GeneratedBy,
                    GeneratedAt = @GeneratedAt
                WHERE ReportID = @ReportID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ReportID", report.ReportID),
                new SqlParameter("@ReportName", report.ReportName),
                new SqlParameter("@ReportType", report.ReportType),
                new SqlParameter("@Description", string.IsNullOrEmpty(report.Description) ? (object)DBNull.Value : report.Description),
                new SqlParameter("@Parameters", string.IsNullOrEmpty(report.Parameters) ? (object)DBNull.Value : report.Parameters),
                new SqlParameter("@FilePath", string.IsNullOrEmpty(report.FilePath) ? (object)DBNull.Value : report.FilePath),
                new SqlParameter("@Status", report.Status),
                new SqlParameter("@GeneratedBy", report.GeneratedBy),
                new SqlParameter("@GeneratedAt", report.GeneratedAt)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool UpdateReportStatus(int reportId, string status)
        {
            string query = @"
                UPDATE Reports SET
                    Status = @Status
                WHERE ReportID = @ReportID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ReportID", reportId),
                new SqlParameter("@Status", status)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool DeleteReport(int reportId)
        {
            string query = "DELETE FROM Reports WHERE ReportID = @ReportID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ReportID", reportId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        private Report MapToReport(DataRow row)
        {
            Report report = new Report();
            report.ReportID = Convert.ToInt32(row["ReportID"]);
            report.ReportName = row["ReportName"].ToString();
            report.ReportType = row["ReportType"].ToString();
            report.Description = row["Description"] == DBNull.Value ? null : row["Description"].ToString();
            report.Parameters = row["Parameters"] == DBNull.Value ? null : row["Parameters"].ToString();
            report.FilePath = row["FilePath"] == DBNull.Value ? null : row["FilePath"].ToString();
            report.Status = row["Status"].ToString();
            report.GeneratedBy = Convert.ToInt32(row["GeneratedBy"]);
            report.GeneratedByName = row["GeneratedByName"].ToString();
            report.GeneratedAt = Convert.ToDateTime(row["GeneratedAt"]);
            report.CreatedAt = Convert.ToDateTime(row["CreatedAt"]);
            return report;
        }
    }
}