using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;
using AQOONHUB.DataAccess;
using AQOONHUB.Models;
using AQOONHUB.Utilities;

namespace AQOONHUB.BusinessLogic
{
    public class AuthenticationBLL
    {
        private DatabaseHelper db;
        private AuditLogger auditLogger;

        public AuthenticationBLL()
        {
            db = new DatabaseHelper();
            auditLogger = new AuditLogger();
        }

        #region Login

        /// <summary>
        /// Authenticates user credentials and establishes session
        /// </summary>
        public User Login(string email, string password, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                auditLogger.RecordLoginActivity(null, email, null, false, "Empty credentials provided");
                throw new ValidationException("Email and password are required");
            }

            // Check if account is locked
            if (IsAccountLocked(email))
            {
                auditLogger.RecordLoginActivity(null, email, null, false, "Account is locked");
                throw new UnauthorizedAccessException("Account is temporarily locked due to multiple failed login attempts. Please try again later or contact administrator.");
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
                RecordFailedLoginAttempt(email, ipAddress);
                auditLogger.RecordLoginActivity(null, email, null, false, "User not found");
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            DataRow userRow = dt.Rows[0];

            // Check if active
            if (!Convert.ToBoolean(userRow["IsActive"]))
            {
                auditLogger.RecordLoginActivity(
                    Convert.ToInt32(userRow["UserID"]),
                    email,
                    userRow["Role"].ToString(),
                    false,
                    "Account is disabled"
                );
                throw new UnauthorizedAccessException("Account is disabled. Contact administrator.");
            }

            // Verify password
            string storedHash = userRow["PasswordHash"].ToString();
            if (!SecurityHelper.VerifyPassword(password, storedHash))
            {
                RecordFailedLoginAttempt(email, ipAddress);
                auditLogger.RecordLoginActivity(
                    Convert.ToInt32(userRow["UserID"]),
                    email,
                    userRow["Role"].ToString(),
                    false,
                    "Invalid password"
                );
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Clear failed login attempts on successful login
            ClearFailedLoginAttempts(email);

            // Update last login
            UpdateLastLogin(Convert.ToInt32(userRow["UserID"]));

            // Create user object
            User user = MapToUser(userRow);

            // Establish session
            EstablishSession(user);

            // Log successful login
            auditLogger.RecordLoginActivity(
                user.UserID,
                email,
                user.Role,
                true,
                "Login successful"
            );

            return user;
        }

        #endregion

        #region Logout

        /// <summary>
        /// Clears user session and authentication cookies
        /// </summary>
        public void Logout()
        {
            if (HttpContext.Current != null)
            {
                // Clear session
                if (HttpContext.Current.Session != null)
                {
                    HttpContext.Current.Session.Remove("CurrentUser");
                    HttpContext.Current.Session.Abandon();
                }

                // Clear forms authentication
                FormsAuthentication.SignOut();
            }
        }

        #endregion

        #region Validate User

        /// <summary>
        /// Validates user credentials without establishing session
        /// </summary>
        public bool ValidateUser(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            string query = "SELECT PasswordHash, IsActive FROM Users WHERE Email = @Email";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Email", email)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
            {
                return false;
            }

            DataRow row = dt.Rows[0];

            if (!Convert.ToBoolean(row["IsActive"]))
            {
                return false;
            }

            string storedHash = row["PasswordHash"].ToString();
            return SecurityHelper.VerifyPassword(password, storedHash);
        }

        #endregion

        #region Change Password

        /// <summary>
        /// Changes user password with current password verification
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

            // Check new password is not same as old
            string newHash = SecurityHelper.HashPassword(newPassword);
            if (newHash == storedHash)
                throw new ValidationException("New password cannot be the same as current password");

            // Update password
            string updateQuery = @"
                UPDATE Users SET 
                    PasswordHash = @PasswordHash,
                    LastPasswordChange = GETDATE()
                WHERE UserID = @UserID";

            SqlParameter[] updateParams = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@PasswordHash", newHash)
            };

            bool result = db.ExecuteNonQuery(updateQuery, updateParams) > 0;

            if (result)
            {
                auditLogger.LogAction(changedBy, "PASSWORD_CHANGE", "Users",
                    string.Format("Password changed for user ID {0}", userId));
            }

            return result;
        }

        #endregion

        #region Reset Password

        /// <summary>
        /// Resets password by administrator without current password
        /// </summary>
        public bool ResetPassword(int userId, string newPassword, int resetBy)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ValidationException("New password is required");

            // Validate new password
            var validation = SecurityHelper.ValidatePasswordPolicy(newPassword);
            if (!validation.IsValid)
                throw new ValidationException(string.Join(", ", validation.Errors));

            // Check user exists
            string checkQuery = "SELECT UserID, Email FROM Users WHERE UserID = @UserID";
            SqlParameter[] checkParams = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId)
            };

            DataTable dt = db.ExecuteQuery(checkQuery, checkParams);
            if (dt.Rows.Count == 0)
                throw new Exception("User not found");

            string email = dt.Rows[0]["Email"].ToString();

            // Update password
            string newHash = SecurityHelper.HashPassword(newPassword);
            string updateQuery = @"
                UPDATE Users SET 
                    PasswordHash = @PasswordHash,
                    LastPasswordChange = GETDATE(),
                    PasswordResetToken = NULL,
                    TokenExpiry = NULL
                WHERE UserID = @UserID";

            SqlParameter[] updateParams = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@PasswordHash", newHash)
            };

            bool result = db.ExecuteNonQuery(updateQuery, updateParams) > 0;

            if (result)
            {
                auditLogger.LogAction(resetBy, "PASSWORD_RESET", "Users",
                    string.Format("Password reset for user {0} ({1})", userId, email));
            }

            return result;
        }

        #endregion

        #region Hash Password

        /// <summary>
        /// Hashes a plain text password
        /// </summary>
        public string HashPassword(string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
                throw new ArgumentException("Password cannot be empty", "plainPassword");

            return SecurityHelper.HashPassword(plainPassword);
        }

        #endregion

        #region Verify Password

        /// <summary>
        /// Verifies a plain text password against stored hash
        /// </summary>
        public bool VerifyPassword(string plainPassword, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(plainPassword) || string.IsNullOrWhiteSpace(passwordHash))
                return false;

            return SecurityHelper.VerifyPassword(plainPassword, passwordHash);
        }

        #endregion

        #region Check Role

        /// <summary>
        /// Checks if user has specified role
        /// </summary>
        public bool CheckRole(int userId, string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return false;

            string query = "SELECT Role FROM Users WHERE UserID = @UserID AND IsActive = 1";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
                return false;

            string userRole = dt.Rows[0]["Role"].ToString();
            return userRole.Equals(role, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Check Permission

        /// <summary>
        /// Checks if user role has permission for module action
        /// </summary>
        public bool CheckPermission(string role, string module, string action)
        {
            if (string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(module))
                return false;

            // Role-based permission matrix
            Dictionary<string, string[]> permissions = new Dictionary<string, string[]>();
            permissions.Add("SuperAdmin", new string[] { "Students", "Staff", "Finance", "Academics", "Exams", "Attendance", "Reports", "Settings", "Users" });
            permissions.Add("Admin", new string[] { "Students", "Staff", "Finance", "Academics", "Exams", "Attendance", "Reports" });
            permissions.Add("Teacher", new string[] { "Students", "Academics", "Exams", "Attendance" });
            permissions.Add("Accountant", new string[] { "Finance", "Reports" });
            permissions.Add("HR", new string[] { "Staff", "Reports" });
            permissions.Add("Parent", new string[] { "Students" });

            if (!permissions.ContainsKey(role))
                return false;

            string[] allowedModules = permissions[role];
            return Array.Exists(allowedModules, m => m.Equals(module, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region Account Lock

        /// <summary>
        /// Checks if user account is locked due to failed attempts
        /// </summary>
        public bool IsAccountLocked(string email)
        {
            string query = @"
                SELECT UserID, IsLocked, LockoutEnd 
                FROM Users 
                WHERE Email = @Email";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Email", email)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
                return false;

            DataRow row = dt.Rows[0];

            if (row["IsLocked"] != DBNull.Value && Convert.ToBoolean(row["IsLocked"]))
            {
                if (row["LockoutEnd"] != DBNull.Value)
                {
                    DateTime lockoutEnd = Convert.ToDateTime(row["LockoutEnd"]);
                    if (lockoutEnd > DateTime.Now)
                    {
                        return true;
                    }
                    else
                    {
                        // Lockout expired, auto-unlock
                        int userId = Convert.ToInt32(row["UserID"]);
                        UnlockAccount(userId, 0);
                        return false;
                    }
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Unlocks a user account
        /// </summary>
        public bool UnlockAccount(int userId, int unlockedBy)
        {
            string query = @"
                UPDATE Users SET 
                    IsLocked = 0,
                    LockoutEnd = NULL,
                    FailedAttempts = 0
                WHERE UserID = @UserID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId)
            };

            bool result = db.ExecuteNonQuery(query, parameters) > 0;

            if (result && unlockedBy > 0)
            {
                auditLogger.LogAction(unlockedBy, "UNLOCK_ACCOUNT", "Users",
                    string.Format("Unlocked account for user ID {0}", userId));
            }

            return result;
        }

        #endregion

        #region Helper Methods

        private void RecordFailedLoginAttempt(string email, string ipAddress)
        {
            string query = @"
                UPDATE Users SET 
                    FailedAttempts = ISNULL(FailedAttempts, 0) + 1,
                    LastFailedAttempt = GETDATE()
                WHERE Email = @Email;

                SELECT FailedAttempts FROM Users WHERE Email = @Email;";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Email", email)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
            {
                int failedAttempts = Convert.ToInt32(dt.Rows[0]["FailedAttempts"]);

                if (failedAttempts >= 5)
                {
                    // Lock account for 30 minutes
                    string lockQuery = @"
                        UPDATE Users SET 
                            IsLocked = 1,
                            LockoutEnd = DATEADD(minute, 30, GETDATE())
                        WHERE Email = @Email";

                    SqlParameter[] lockParams = new SqlParameter[]
                    {
                        new SqlParameter("@Email", email)
                    };

                    db.ExecuteNonQuery(lockQuery, lockParams);

                    auditLogger.LogAction(0, "ACCOUNT_LOCKED", "Users",
                        string.Format("Account locked for {0} after {1} failed attempts", email, failedAttempts));
                }
            }
        }

        private void ClearFailedLoginAttempts(string email)
        {
            string query = @"
                UPDATE Users SET 
                    FailedAttempts = 0,
                    LastFailedAttempt = NULL
                WHERE Email = @Email";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Email", email)
            };

            db.ExecuteNonQuery(query, parameters);
        }

        private void UpdateLastLogin(int userId)
        {
            string query = "UPDATE Users SET LastLogin = GETDATE() WHERE UserID = @UserID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId)
            };

            db.ExecuteNonQuery(query, parameters);
        }

        private void EstablishSession(User user)
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session["CurrentUser"] = user;

                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                    1,
                    user.Email,
                    DateTime.Now,
                    DateTime.Now.AddMinutes(30),
                    false,
                    string.Format("{0}|{1}", user.UserID, user.Role),
                    FormsAuthentication.FormsCookiePath);

                string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                HttpContext.Current.Response.Cookies.Add(authCookie);
            }
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