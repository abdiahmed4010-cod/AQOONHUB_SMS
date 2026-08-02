using System;
using System.Data;

namespace AQOONHUB_SMS.Modules.Reports
{
    public sealed partial class ReportsRepository
    {
        private DataTable Stage4(string handler, ReportFilter f, bool allowSensitive)
        {
            switch (handler)
            {
                case "security-users": return GetUserAccountsReport(f);
                case "security-users-role": return GetUsersByRoleReport(f);
                case "security-users-active": return GetUserStatusReport(true);
                case "security-users-inactive": return GetUserStatusReport(false);
                case "security-login-history": return GetLoginHistoryReport(f, false);
                case "security-failed-logins": return GetLoginHistoryReport(f, true);
                case "security-role-permissions": return GetRolePermissionReport();
                case "security-audit-log": return GetAuditLogReport(f);
                case "security-user-activity": return GetUserActivityReport(f);
                case "security-sensitive-actions": return GetSensitiveActionReport(f);
                case "security-unavailable": return null;
                default: return Stage4Analytics(handler, f, allowSensitive);
            }
        }

        public DataRow GetSecuritySummary()
        {
            const string sql = @"SELECT
 (SELECT COUNT(*) FROM Users) AS TotalUsers,
 (SELECT COUNT(*) FROM Users WHERE IsActive=1) AS ActiveUsers,
 (SELECT COUNT(*) FROM Users WHERE IsActive=0) AS InactiveUsers,
 (SELECT COUNT(*) FROM LoginActivity WHERE Status='Failed') AS FailedLogins";
            return ExecuteDataTable(sql, null).Rows[0];
        }

        private DataTable GetUserAccountsReport(ReportFilter f)
        {
            const string sql = @"SELECT UserID AS [User ID], FullName AS [Name], Email, ISNULL(Phone,'') AS [Phone],
 Role, CASE WHEN IsActive=1 THEN 'Active' ELSE 'Inactive' END AS [Status], CreatedAt AS [Created], LastLogin AS [Last Login]
FROM Users WHERE (@role IS NULL OR Role=@role) AND (@q='' OR FullName LIKE '%'+@q+'%' OR Email LIKE '%'+@q+'%') ORDER BY FullName";
            return ExecuteDataTable(sql, new[] { P("@role", (object)NullIfEmpty(f.Role) ?? DBNull.Value), P("@q", f.Search ?? "") });
        }

        private DataTable GetUsersByRoleReport(ReportFilter f)
        {
            return ExecuteDataTable("SELECT ISNULL(Role,'Unassigned') AS [Role], COUNT(*) AS [Users], SUM(CASE WHEN IsActive=1 THEN 1 ELSE 0 END) AS [Active], SUM(CASE WHEN IsActive=0 THEN 1 ELSE 0 END) AS [Inactive] FROM Users GROUP BY Role ORDER BY [Users] DESC, [Role]", null);
        }

        private DataTable GetUserStatusReport(bool active)
        {
            return ExecuteDataTable("SELECT UserID AS [User ID], FullName AS [Name], Email, Role, CreatedAt AS [Created], LastLogin AS [Last Login] FROM Users WHERE IsActive=@active ORDER BY FullName", new[] { P("@active", active) });
        }

        private DataTable GetLoginHistoryReport(ReportFilter f, bool failedOnly)
        {
            string sql = @"SELECT la.LoginID AS [ID], ISNULL(u.FullName,'Unknown user') AS [User], ISNULL(u.Role,'') AS [Role],
 la.LoginTime AS [When], la.Status, ISNULL(la.FailureReason,'') AS [Failure Reason], ISNULL(la.IPAddress,'') AS [IP Address], ISNULL(la.DeviceInfo,'') AS [Device]
FROM LoginActivity la LEFT JOIN Users u ON u.UserID=la.UserID
WHERE (@failed=0 OR la.Status='Failed') AND (@from IS NULL OR la.LoginTime>=@from) AND (@to IS NULL OR la.LoginTime<DATEADD(day,1,@to)) ORDER BY la.LoginTime DESC";
            return ExecuteDataTable(sql, new[] { P("@failed", failedOnly), P("@from", (object)f.From ?? DBNull.Value), P("@to", (object)f.To ?? DBNull.Value) });
        }

        private DataTable GetRolePermissionReport()
        {
            return ExecuteDataTable(@"SELECT r.RoleName AS [Role], p.Module, p.PermissionName AS [Permission], ISNULL(rp.AssignedAt,r.CreatedAt) AS [Assigned]
FROM RolePermissions rp JOIN Roles r ON r.RoleID=rp.RoleID JOIN Permissions p ON p.PermissionID=rp.PermissionID
ORDER BY r.RoleName, p.Module, p.PermissionName", null);
        }

        private DataTable GetAuditLogReport(ReportFilter f)
        {
            return ExecuteDataTable(@"SELECT al.AuditID AS [ID], ISNULL(u.FullName,'System') AS [User], al.Action, al.Module,
 al.Detail, al.ActionTime AS [When], ISNULL(al.IPAddress,'') AS [IP Address]
FROM AuditLog al LEFT JOIN Users u ON u.UserID=al.UserID
WHERE (@from IS NULL OR al.ActionTime>=@from) AND (@to IS NULL OR al.ActionTime<DATEADD(day,1,@to)) ORDER BY al.ActionTime DESC",
                new[] { P("@from", (object)f.From ?? DBNull.Value), P("@to", (object)f.To ?? DBNull.Value) });
        }

        private DataTable GetUserActivityReport(ReportFilter f)
        {
            return GetLoginHistoryReport(f, false);
        }

        private DataTable GetSensitiveActionReport(ReportFilter f)
        {
            return ExecuteDataTable(@"SELECT al.AuditID AS [ID], ISNULL(u.FullName,'System') AS [User], al.Action, al.Module, al.Detail, al.ActionTime AS [When]
FROM AuditLog al LEFT JOIN Users u ON u.UserID=al.UserID
WHERE (UPPER(al.Action) IN ('DELETE','SUSPEND','REACTIVATE','PERMISSION','ROLE') OR al.Module IN ('Security','Users','Roles'))
AND (@from IS NULL OR al.ActionTime>=@from) AND (@to IS NULL OR al.ActionTime<DATEADD(day,1,@to)) ORDER BY al.ActionTime DESC",
                new[] { P("@from", (object)f.From ?? DBNull.Value), P("@to", (object)f.To ?? DBNull.Value) });
        }
    }
}
