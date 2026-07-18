using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using AQOONHUB.DataAccess;

namespace AQOONHUB.Utilities
{
    /// <summary>
    /// Provides comprehensive audit logging for all system activities
    /// </summary>
    public class AuditLogger
    {
        private DatabaseHelper db;
        private string connectionString;

        public AuditLogger()
        {
            db = new DatabaseHelper();
            connectionString = System.Configuration.ConfigurationManager
                .ConnectionStrings["AQOONHUB_DB"].ConnectionString;
        }

        #region Core Logging Methods

        /// <summary>
        /// Logs a general action to the audit trail
        /// </summary>
        public void LogAction(int? userId, string action, string module, string detail)
        {
            string ipAddress = GetClientIpAddress();
            LogAction(userId, action, module, detail, ipAddress);
        }

        /// <summary>
        /// Logs a general action with specific IP address
        /// </summary>
        public void LogAction(int? userId, string action, string module, string detail, string ipAddress)
        {
            try
            {
                string query = @"
                    INSERT INTO AuditLog (UserID, Action, Module, Detail, IPAddress, ActionTime)
                    VALUES (@UserID, @Action, @Module, @Detail, @IPAddress, GETDATE())";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserID", (object)userId ?? DBNull.Value),
                    new SqlParameter("@Action", action),
                    new SqlParameter("@Module", module),
                    new SqlParameter("@Detail", TruncateString(detail, 4000)),
                    new SqlParameter("@IPAddress", (object)ipAddress ?? DBNull.Value)
                };

                db.ExecuteNonQuery(query, parameters);
            }
            catch (Exception ex)
            {
                // Write to Windows Event Log if database logging fails
                System.Diagnostics.EventLog.WriteEntry("AQOONHUB",
                    string.Format("Audit logging failed: {0}. Original action: {1} - {2}", ex.Message, action, detail),
                    System.Diagnostics.EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// Logs a CREATE action
        /// </summary>
        public void LogCreate(int userId, string module, string entityType, string entityId, string description)
        {
            string detail = string.Format("Created {0} [{1}]: {2}", entityType, entityId, description);
            LogAction(userId, "CREATE", module, detail);
        }

        /// <summary>
        /// Logs an UPDATE action with before/after values
        /// </summary>
        public void LogUpdate(int userId, string module, string entityType, string entityId,
            string fieldName, string oldValue, string newValue)
        {
            string detail = string.Format("Updated {0} [{1}] - Field '{2}': '{3}' → '{4}'", entityType, entityId, fieldName, oldValue, newValue);
            LogAction(userId, "UPDATE", module, detail);
        }

        /// <summary>
        /// Logs a DELETE action
        /// </summary>
        public void LogDelete(int userId, string module, string entityType, string entityId, string description)
        {
            string detail = string.Format("Deleted {0} [{1}]: {2}", entityType, entityId, description);
            LogAction(userId, "DELETE", module, detail);
        }

        /// <summary>
        /// Logs a RESTORE action (soft-delete reversal)
        /// </summary>
        public void LogRestore(int userId, string module, string entityType, string entityId)
        {
            string detail = string.Format("Restored {0} [{1}] from archive", entityType, entityId);
            LogAction(userId, "RESTORE", module, detail);
        }

        /// <summary>
        /// Logs an EXPORT action
        /// </summary>
        public void LogExport(int userId, string module, string exportType, string fileName, int recordCount)
        {
            string detail = string.Format("Exported {0} to '{1}' ({2} records)", exportType, fileName, recordCount);
            LogAction(userId, "EXPORT", module, detail);
        }

        /// <summary>
        /// Logs a LOGIN action
        /// </summary>
        public void LogLogin(int? userId, string username, bool success, string failureReason = null)
        {
            string action = success ? "LOGIN" : "LOGIN_FAILED";
            string detail;
            if (success)
            {
                detail = string.Format("Successful login: {0}", username);
            }
            else
            {
                detail = string.Format("Failed login attempt: {0}. Reason: {1}", username, failureReason ?? "Invalid credentials");
            }

            LogAction(userId, action, "Auth", detail);
        }

        /// <summary>
        /// Logs a LOGOUT action
        /// </summary>
        public void LogLogout(int userId, string username)
        {
            string detail = string.Format("User logged out: {0}", username);
            LogAction(userId, "LOGOUT", "Auth", detail);
        }

        /// <summary>
        /// Logs a PERMISSION CHANGE action
        /// </summary>
        public void LogPermissionChange(int userId, string module, string targetUser,
            string permission, string oldValue, string newValue)
        {
            string detail = string.Format("Changed permission for {0}: {1} '{2}' → '{3}'", targetUser, permission, oldValue, newValue);
            LogAction(userId, "PERMISSION_CHANGE", module, detail);
        }

        /// <summary>
        /// Logs a BULK OPERATION
        /// </summary>
        public void LogBulkOperation(int userId, string module, string operation, int affectedCount, string description)
        {
            string detail = string.Format("Bulk {0}: {1} records affected. {2}", operation, affectedCount, description);
            LogAction(userId, "BULK", module, detail);
        }

        #endregion

        #region Login Activity Logging

        /// <summary>
        /// Records login attempt in LoginActivity table
        /// </summary>
        public void RecordLoginActivity(int? userId, string username, string role, bool success, string failureReason = null)
        {
            try
            {
                string query = @"
                    INSERT INTO LoginActivity (UserID, IPAddress, DeviceInfo, LoginTime, Status, FailureReason)
                    VALUES (@UserID, @IPAddress, @DeviceInfo, GETDATE(), @Status, @FailureReason)";

                string deviceInfo = GetDeviceInfo();
                string ipAddress = GetClientIpAddress();

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserID", (object)userId ?? DBNull.Value),
                    new SqlParameter("@IPAddress", ipAddress),
                    new SqlParameter("@DeviceInfo", deviceInfo),
                    new SqlParameter("@Status", success ? "Success" : "Failed"),
                    new SqlParameter("@FailureReason", (object)failureReason ?? DBNull.Value)
                };

                db.ExecuteNonQuery(query, parameters);

                // Also log to AuditLog
                LogLogin(userId, username, success, failureReason);
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("AQOONHUB",
                    string.Format("Login activity logging failed: {0}", ex.Message),
                    System.Diagnostics.EventLogEntryType.Error);
            }
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// Gets audit log entries with filtering
        /// </summary>
        public DataTable GetAuditLog(string module = null, string action = null,
            DateTime? fromDate = null, DateTime? toDate = null, int? userId = null, int top = 100)
        {
            string query = string.Format(@"
                SELECT TOP ({0})
                    al.AuditID,
                    al.Action,
                    al.Module,
                    al.Detail,
                    al.IPAddress,
                    al.ActionTime,
                    ISNULL(u.FullName, 'System') as UserName,
                    u.Email as UserEmail
                FROM AuditLog al
                LEFT JOIN Users u ON al.UserID = u.UserID
                WHERE 1=1", top);

            var parameters = new System.Collections.Generic.List<SqlParameter>();

            if (!string.IsNullOrEmpty(module))
            {
                query += " AND al.Module = @Module";
                parameters.Add(new SqlParameter("@Module", module));
            }

            if (!string.IsNullOrEmpty(action))
            {
                query += " AND al.Action = @Action";
                parameters.Add(new SqlParameter("@Action", action));
            }

            if (fromDate.HasValue)
            {
                query += " AND al.ActionTime >= @FromDate";
                parameters.Add(new SqlParameter("@FromDate", fromDate.Value));
            }

            if (toDate.HasValue)
            {
                query += " AND al.ActionTime <= @ToDate";
                parameters.Add(new SqlParameter("@ToDate", toDate.Value.AddDays(1)));
            }

            if (userId.HasValue)
            {
                query += " AND al.UserID = @UserID";
                parameters.Add(new SqlParameter("@UserID", userId.Value));
            }

            query += " ORDER BY al.ActionTime DESC";

            return db.ExecuteQuery(query, parameters.ToArray());
        }

        /// <summary>
        /// Gets login activity history
        /// </summary>
        public DataTable GetLoginActivity(int? userId = null, string status = null, int top = 100)
        {
            string query = string.Format(@"
                SELECT TOP ({0})
                    la.LoginID,
                    ISNULL(u.FullName, 'Unknown') as UserName,
                    ISNULL(u.Role, '—') as Role,
                    la.IPAddress,
                    la.DeviceInfo,
                    la.LoginTime,
                    la.Status,
                    la.FailureReason
                FROM LoginActivity la
                LEFT JOIN Users u ON la.UserID = u.UserID
                WHERE 1=1", top);

            var parameters = new System.Collections.Generic.List<SqlParameter>();

            if (userId.HasValue)
            {
                query += " AND la.UserID = @UserID";
                parameters.Add(new SqlParameter("@UserID", userId.Value));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query += " AND la.Status = @Status";
                parameters.Add(new SqlParameter("@Status", status));
            }

            query += " ORDER BY la.LoginTime DESC";

            return db.ExecuteQuery(query, parameters.ToArray());
        }

        /// <summary>
        /// Gets summary statistics for dashboard
        /// </summary>
        public DataTable GetAuditSummary(DateTime? date = null)
        {
            string query = @"
                SELECT 
                    Action,
                    COUNT(*) as Count,
                    MAX(ActionTime) as LastOccurrence
                FROM AuditLog
                WHERE CAST(ActionTime as DATE) = CAST(ISNULL(@Date, GETDATE()) as DATE)
                GROUP BY Action
                ORDER BY Count DESC";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Date", (object)date ?? DBNull.Value)
            };

            return db.ExecuteQuery(query, parameters);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets client IP address
        /// </summary>
        private string GetClientIpAddress()
        {
            try
            {
                HttpContext context = HttpContext.Current;
                if (context == null) return "Unknown";

                string ip = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ip))
                    ip = context.Request.ServerVariables["REMOTE_ADDR"];

                if (string.IsNullOrEmpty(ip))
                    ip = context.Request.UserHostAddress;

                // Handle multiple IPs (proxies)
                if (!string.IsNullOrEmpty(ip) && ip.Contains(","))
                    ip = ip.Split(',')[0].Trim();

                return ip ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Gets device/browser information
        /// </summary>
        private string GetDeviceInfo()
        {
            try
            {
                HttpContext context = HttpContext.Current;
                if (context == null) return "Unknown";

                string browser = context.Request.Browser.Browser;
                string version = context.Request.Browser.Version;
                string platform = context.Request.Browser.Platform;

                return string.Format("{0} {1} — {2}", browser, version, platform);
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Truncates string to maximum length
        /// </summary>
        private string TruncateString(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        #endregion

        #region Maintenance

        /// <summary>
        /// Archives old audit log entries (run as scheduled job)
        /// </summary>
        public bool ArchiveOldEntries(int daysToKeep = 365)
        {
            try
            {
                string query = @"
                    INSERT INTO AuditLogArchive 
                    SELECT * FROM AuditLog 
                    WHERE ActionTime < DATEADD(day, -@DaysToKeep, GETDATE());
                    
                    DELETE FROM AuditLog 
                    WHERE ActionTime < DATEADD(day, -@DaysToKeep, GETDATE());";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@DaysToKeep", daysToKeep)
                };

                db.ExecuteNonQuery(query, parameters);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}