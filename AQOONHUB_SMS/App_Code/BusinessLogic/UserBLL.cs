using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using AQOONHUB.DataAccess;
using AQOONHUB.Models;
using AQOONHUB.Utilities;

namespace AQOONHUB.BusinessLogic
{
    public class UserBLL
    {
        private DatabaseHelper db;
        private AuditLogger auditLogger;

        public UserBLL()
        {
            db = new DatabaseHelper();
            auditLogger = new AuditLogger();
        }

        #region Authentication

        /// <summary>
        /// Authenticates user and creates session
        /// </summary>
        public User Authenticate(string email, string password, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                auditLogger.RecordLoginActivity(null, email, null, false, "Empty credentials");
                throw new ValidationException("Email and password are required");
            }

            // Get user by email
            string query = "SELECT * FROM Users WHERE Email = @Email";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Email", email)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
            {
                auditLogger.RecordLoginActivity(null, email, null, false, "User not found");
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            DataRow userRow = dt.Rows[0];

            // Check if active
            if (!Convert.ToBoolean(userRow["IsActive"]))
            {
                auditLogger.RecordLoginActivity(null, email, null, false, "Account disabled");
                throw new UnauthorizedAccessException("Account is disabled. Contact administrator.");
            }

            // Verify password
            string storedHash = userRow["PasswordHash"].ToString();
            if (!SecurityHelper.VerifyPassword(password, storedHash))
            {
                auditLogger.RecordLoginActivity(
                    Convert.ToInt32(userRow["UserID"]),
                    email,
                    userRow["Role"].ToString(),
                    false,
                    "Invalid password"
                );
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Update last login
            UpdateLastLogin(Convert.ToInt32(userRow["UserID"]));

            // Create user object
            User user = MapToUser(userRow);

            // Log successful login
            auditLogger.RecordLoginActivity(
                user.UserID,
                email,
                user.Role,
                true
            );

            return user;
        }

        /// <summary>
        /// Changes user password with validation
        /// </summary>
        public bool ChangePassword(int userId, string currentPassword, string newPassword, int changedBy)
        {
            // Get user
            string query = "SELECT * FROM Users WHERE UserID = @UserID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);
            if (dt.Rows.Count == 0)
                throw new Exception("User not found");

            // Verify current password
            string storedHash = dt.Rows[0]["PasswordHash"].ToString();
            if (!SecurityHelper.VerifyPassword(currentPassword, storedHash))
                throw new ValidationException("Current password is incorrect");

            // Validate new password
            var validation = SecurityHelper.ValidatePasswordPolicy(newPassword);
            if (!validation.IsValid)
                throw new ValidationException(string.Join(", ", validation.Errors));

            // Update password
            string newHash = SecurityHelper.HashPassword(newPassword);
            string updateQuery = "UPDATE Users SET PasswordHash = @PasswordHash WHERE UserID = @UserID";

            SqlParameter[] updateParams = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@PasswordHash", newHash)
            };

            bool result = db.ExecuteNonQuery(updateQuery, updateParams) > 0;

            if (result)
            {
                auditLogger.LogAction(changedBy, "PASSWORD_CHANGE", "Users",
                    "Password changed for user ID " + userId);
            }

            return result;
        }

        /// <summary>
        /// Initiates password reset
        /// </summary>
        public string RequestPasswordReset(string email)
        {
            string query = "SELECT UserID, FullName FROM Users WHERE Email = @Email AND IsActive = 1";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Email", email)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);
            if (dt.Rows.Count == 0)
                return null; // Don't reveal if email exists

            int userId = Convert.ToInt32(dt.Rows[0]["UserID"]);
            string token = SecurityHelper.GenerateResetToken();

            // Save token with expiration (24 hours)
            string saveQuery = @"
                UPDATE Users SET 
                    PasswordResetToken = @Token,
                    TokenExpiry = DATEADD(hour, 24, GETDATE())
                WHERE UserID = @UserID";

            SqlParameter[] saveParams = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@Token", token)
            };

            db.ExecuteNonQuery(saveQuery, saveParams);

            // Send email with reset link
            // EmailService.SendPasswordReset(email, token, dt.Rows[0]["FullName"].ToString());

            return token;
        }

        /// <summary>
        /// Resets password with token
        /// </summary>
        public bool ResetPassword(string token, string newPassword)
        {
            string query = "SELECT UserID FROM Users WHERE PasswordResetToken = @Token AND TokenExpiry > GETDATE()";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Token", token)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);
            if (dt.Rows.Count == 0)
                throw new ValidationException("Invalid or expired reset token");

            int userId = Convert.ToInt32(dt.Rows[0]["UserID"]);
            string newHash = SecurityHelper.HashPassword(newPassword);

            string updateQuery = @"
                UPDATE Users SET 
                    PasswordHash = @PasswordHash,
                    PasswordResetToken = NULL,
                    TokenExpiry = NULL
                WHERE UserID = @UserID";

            SqlParameter[] updateParams = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@PasswordHash", newHash)
            };

            return db.ExecuteNonQuery(updateQuery, updateParams) > 0;
        }

        #endregion

        #region User Management

        public DataTable GetAllUsers()
        {
            string query = @"
                SELECT u.*, 
                    CASE WHEN s.StaffID IS NOT NULL THEN 'Staff' 
                         WHEN g.GuardianID IS NOT NULL THEN 'Parent' 
                         ELSE 'System' END as UserType
                FROM Users u
                LEFT JOIN Staff s ON u.UserID = s.UserID
                LEFT JOIN Guardians g ON u.UserID = g.UserID
                ORDER BY u.CreatedAt DESC";

            return db.ExecuteQuery(query);
        }

        public User GetUser(int userId)
        {
            string query = "SELECT * FROM Users WHERE UserID = @UserID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);
            if (dt.Rows.Count > 0)
                return MapToUser(dt.Rows[0]);

            return null;
        }

        public int CreateUser(User user, int createdBy)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(user.FullName))
                throw new ValidationException("Full name is required");

            if (string.IsNullOrWhiteSpace(user.Email))
                throw new ValidationException("Email is required");

            if (!SecurityHelper.IsValidEmail(user.Email))
                throw new ValidationException("Invalid email format");

            // Check if email exists
            string checkQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
            SqlParameter[] checkParams = new SqlParameter[]
            {
                new SqlParameter("@Email", user.Email)
            };

            int existing = Convert.ToInt32(db.ExecuteScalar(checkQuery, checkParams));
            if (existing > 0)
                throw new ValidationException("Email already registered");

            // Hash password
            string passwordHash = SecurityHelper.HashPassword(user.PasswordHash);

            string query = @"
                INSERT INTO Users (FullName, Email, PasswordHash, Phone, Role, IsActive, CreatedAt)
                VALUES (@FullName, @Email, @PasswordHash, @Phone, @Role, 1, GETDATE());
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@FullName", user.FullName),
                new SqlParameter("@Email", user.Email),
                new SqlParameter("@PasswordHash", passwordHash),
                new SqlParameter("@Phone", (object)user.Phone ?? DBNull.Value),
                new SqlParameter("@Role", user.Role)
            };

            int newId = Convert.ToInt32(db.ExecuteScalar(query, parameters));

            auditLogger.LogCreate(createdBy, "Users", "User", newId.ToString(),
                string.Format("Created user account for {0} ({1})", user.FullName, user.Role));

            return newId;
        }

        public bool UpdateUser(User user, int updatedBy)
        {
            string query = @"
                UPDATE Users SET
                    FullName = @FullName,
                    Email = @Email,
                    Phone = @Phone,
                    Role = @Role,
                    IsActive = @IsActive
                WHERE UserID = @UserID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", user.UserID),
                new SqlParameter("@FullName", user.FullName),
                new SqlParameter("@Email", user.Email),
                new SqlParameter("@Phone", (object)user.Phone ?? DBNull.Value),
                new SqlParameter("@Role", user.Role),
                new SqlParameter("@IsActive", user.IsActive)
            };

            bool result = db.ExecuteNonQuery(query, parameters) > 0;

            if (result)
            {
                auditLogger.LogUpdate(updatedBy, "Users", "User", user.UserID.ToString(),
                    "Profile", "Previous", string.Format("Name: {0}, Role: {1}", user.FullName, user.Role));
            }

            return result;
        }

        public bool DisableUser(int userId, int disabledBy, string reason)
        {
            string query = "UPDATE Users SET IsActive = 0 WHERE UserID = @UserID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId)
            };

            bool result = db.ExecuteNonQuery(query, parameters) > 0;

            if (result)
            {
                auditLogger.LogAction(disabledBy, "DISABLE", "Users",
                    string.Format("Disabled user ID {0}. Reason: {1}", userId, reason));
            }

            return result;
        }

        #endregion

        #region Role & Permissions

        /// <summary>
        /// Checks if user has permission for an action
        /// </summary>
        public bool HasPermission(string role, string module, string action)
        {
            // Role-based permission matrix
            var permissions = new Dictionary<string, string[]>
            {
                { "SuperAdmin", new[] { "Students", "Staff", "Finance", "Academics", "Exams", "Attendance", "Reports", "Settings", "Users" } },
                { "Admin", new[] { "Students", "Staff", "Finance", "Academics", "Exams", "Attendance", "Reports" } },
                { "Teacher", new[] { "Students", "Academics", "Exams", "Attendance" } },
                { "Accountant", new[] { "Finance", "Reports" } },
                { "HR", new[] { "Staff", "Reports" } },
                { "Parent", new[] { "Students" } }
            };

            if (!permissions.ContainsKey(role))
                return false;

            return Array.Exists(permissions[role], m => m == module);
        }

        /// <summary>
        /// Gets menu items based on role
        /// </summary>
        public DataTable GetMenuForRole(string role)
        {
            // Return menu structure based on role
            string query = @"
                SELECT * FROM MenuItems 
                WHERE @Role IN (SELECT value FROM STRING_SPLIT(AllowedRoles, ','))
                AND IsActive = 1
                ORDER BY SortOrder";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Role", role)
            };

            return db.ExecuteQuery(query, parameters);
        }

        #endregion

        #region Helper Methods

        private void UpdateLastLogin(int userId)
        {
            string query = "UPDATE Users SET LastLogin = GETDATE() WHERE UserID = @UserID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId)
            };

            db.ExecuteNonQuery(query, parameters);
        }

        private User MapToUser(DataRow row)
        {
            return new User
            {
                UserID = Convert.ToInt32(row["UserID"]),
                FullName = row["FullName"].ToString(),
                Email = row["Email"].ToString(),
                Phone = row["Phone"] != DBNull.Value ? row["Phone"].ToString() : null,
                Role = row["Role"].ToString(),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                LastLogin = row["LastLogin"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["LastLogin"]) : null,
                CreatedAt = Convert.ToDateTime(row["CreatedAt"])
            };
        }

        #endregion
    }
}