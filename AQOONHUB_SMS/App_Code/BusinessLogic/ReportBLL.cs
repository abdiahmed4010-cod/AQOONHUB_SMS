using System;
using System.Collections.Generic;
using System.Data;
using AQOONHUB.DataAccess;
using AQOONHUB.Models;
using AQOONHUB.Utilities;

namespace AQOONHUB.BusinessLogic
{
    public class ReportBLL
    {
        private ReportDAL reportDAL;
        private AuditLogger auditLogger;

        public ReportBLL()
        {
            reportDAL = new ReportDAL();
            auditLogger = new AuditLogger();
        }

        #region Validation

        /// <summary>
        /// Validates report data before save
        /// </summary>
        private List<string> ValidateReport(Report report, bool isNew)
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(report.ReportName))
                errors.Add("Report name is required");

            if (string.IsNullOrWhiteSpace(report.ReportType))
                errors.Add("Report type is required");

            if (report.GeneratedBy <= 0)
                errors.Add("Generator user is required");

            if (string.IsNullOrWhiteSpace(report.Status))
                errors.Add("Status is required");

            return errors;
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Gets all reports with optional filters
        /// </summary>
        public List<Report> GetReports(string status = null, string reportType = null)
        {
            return reportDAL.GetAllReports(status, reportType);
        }

        /// <summary>
        /// Gets report by ID
        /// </summary>
        public Report GetReport(int reportId)
        {
            if (reportId <= 0)
                throw new ArgumentException("Invalid report ID");

            return reportDAL.GetReportById(reportId);
        }

        /// <summary>
        /// Creates new report
        /// </summary>
        public Report CreateReport(Report report, int createdBy, string ipAddress)
        {
            // Validate
            var errors = ValidateReport(report, true);
            if (errors.Count > 0)
                throw new ValidationException(string.Join(", ", errors));

            // Set defaults
            report.Status = "Pending";
            report.GeneratedAt = DateTime.Now;
            report.CreatedAt = DateTime.Now;

            // Save to database
            int newId = reportDAL.AddReport(report);
            report.ReportID = newId;

            // Log audit
            auditLogger.LogCreate(createdBy, "Reports", "Report", report.ReportName,
                string.Format("Created {0} report: {1}", report.ReportType, report.ReportName));

            return report;
        }

        /// <summary>
        /// Updates existing report
        /// </summary>
        public bool UpdateReport(Report report, int updatedBy, string ipAddress)
        {
            var errors = ValidateReport(report, false);
            if (errors.Count > 0)
                throw new ValidationException(string.Join(", ", errors));

            // Get old data for audit
            var oldReport = reportDAL.GetReportById(report.ReportID);
            if (oldReport == null)
                throw new Exception("Report not found");

            // Update
            bool result = reportDAL.UpdateReport(report);

            if (result)
            {
                // Log changes
                if (oldReport.Status != report.Status)
                {
                    auditLogger.LogUpdate(updatedBy, "Reports", "Report", report.ReportName,
                        "Status", oldReport.Status, report.Status);
                }
                if (oldReport.FilePath != report.FilePath)
                {
                    auditLogger.LogUpdate(updatedBy, "Reports", "Report", report.ReportName,
                        "FilePath", oldReport.FilePath, report.FilePath);
                }
            }

            return result;
        }

        /// <summary>
        /// Deletes report
        /// </summary>
        public bool DeleteReport(int reportId, int deletedBy, string ipAddress, string reason)
        {
            var report = reportDAL.GetReportById(reportId);
            if (report == null)
                throw new Exception("Report not found");

            bool result = reportDAL.DeleteReport(reportId);

            if (result)
            {
                auditLogger.LogDelete(deletedBy, "Reports", "Report", report.ReportName, reason);
            }

            return result;
        }

        #endregion

        #region Business Rules

        /// <summary>
        /// Marks report as completed
        /// </summary>
        public bool CompleteReport(int reportId, string filePath, int completedBy)
        {
            var report = reportDAL.GetReportById(reportId);
            if (report == null)
                throw new Exception("Report not found");

            if (report.Status == "Completed")
                throw new Exception("Report is already completed");

            report.Status = "Completed";
            report.FilePath = filePath;

            bool result = reportDAL.UpdateReport(report);

            if (result)
            {
                auditLogger.LogAction(completedBy, "COMPLETE", "Reports",
                    string.Format("Completed report: {0}", report.ReportName));
            }

            return result;
        }

        /// <summary>
        /// Marks report as failed
        /// </summary>
        public bool FailReport(int reportId, string reason, int failedBy)
        {
            var report = reportDAL.GetReportById(reportId);
            if (report == null)
                throw new Exception("Report not found");

            if (report.Status == "Completed")
                throw new Exception("Cannot mark a completed report as failed");

            bool result = reportDAL.UpdateReportStatus(reportId, "Failed");

            if (result)
            {
                auditLogger.LogAction(failedBy, "FAIL", "Reports",
                    string.Format("Report failed: {0}. Reason: {1}", report.ReportName, reason));
            }

            return result;
        }

        /// <summary>
        /// Re-generates existing report
        /// </summary>
        public bool RegenerateReport(int reportId, int regeneratedBy)
        {
            var report = reportDAL.GetReportById(reportId);
            if (report == null)
                throw new Exception("Report not found");

            // Reset status to pending
            report.Status = "Pending";
            report.GeneratedAt = DateTime.Now;

            bool result = reportDAL.UpdateReport(report);

            if (result)
            {
                auditLogger.LogAction(regeneratedBy, "REGENERATE", "Reports",
                    string.Format("Re-generated report: {0}", report.ReportName));
            }

            return result;
        }

        #endregion

        #region Reports

        /// <summary>
        /// Gets reports by status
        /// </summary>
        public List<Report> GetReportsByStatus(string status)
        {
            return reportDAL.GetAllReports(status, null);
        }

        /// <summary>
        /// Gets reports by type
        /// </summary>
        public List<Report> GetReportsByType(string reportType)
        {
            return reportDAL.GetAllReports(null, reportType);
        }

        #endregion
    }
}