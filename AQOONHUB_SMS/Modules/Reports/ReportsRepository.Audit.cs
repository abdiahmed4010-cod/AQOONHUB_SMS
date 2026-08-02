using System.Data;
namespace AQOONHUB_SMS.Modules.Reports
{ public sealed partial class ReportsRepository { public DataTable GetReportAudit(int uid,string role){if(!ReportAuthorization.IsStage5Admin(role))return new DataTable();return ExecuteDataTable(@"SELECT a.ReportAuditLogID,COALESCE(u.FullName,'System') [User],a.Action,a.ReportName,a.ReportKey,a.Category,a.CreatedAt,a.FilterSummary,a.ResultStatus,a.IpAddress FROM ReportAuditLogs a LEFT JOIN Users u ON u.UserID=a.UserID ORDER BY a.CreatedAt DESC",null);} } }
