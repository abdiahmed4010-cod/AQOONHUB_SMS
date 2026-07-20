using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;


namespace AQOONHUB_SMS.App_Code.DataAccess
{
    /// <summary>
    /// Data access layer for user management
    /// </summary>
    public class UserDAL
    {
        private DatabaseHelper db;

        /// <summary>
        /// Initializes a new instance of the UserDAL class
        /// </summary>
        public UserDAL()
        {
            db = new DatabaseHelper();
        }

        #region Retrieval Operations

        /// <summary>
        /// Gets all users
        /// </summary>
        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();

            string query = "SELECT * FROM Users ORDER BY FullName";

            DataTable dt = db.ExecuteQuery(query);

            foreach (DataRow row in dt.Rows)
            {
                users.Add(MapToUser(row));
            }

            return users;
        }

        /// <summary>
        /// Gets user by ID
        /// </summary>
        public User GetUserById(int userId)
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

        /// <summary>
        /// Gets user by email
        /// </summary>
        public User GetUserByEmail(string email)
        {
            string query = "SELECT * FROM Users WHERE Email = @Email";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Email", email)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
                return MapToUser(dt.Rows[0]);

            return null;
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// Adds a new user
        /// </summary>
        public int AddUser(User user)
        {
            string query = @"
                INSERT INTO Users (FullName, Email, PasswordHash, Phone, Role, IsActive, LastLogin, CreatedAt, UpdatedAt)
                VALUES (@FullName, @Email, @PasswordHash, @Phone, @Role, @IsActive, @LastLogin, GETDATE(), GETDATE());
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@FullName", user.FullName),
                new SqlParameter("@Email", user.Email),
                new SqlParameter("@PasswordHash", user.PasswordHash),
                new SqlParameter("@Phone", string.IsNullOrEmpty(user.Phone) ? (object)DBNull.Value : user.Phone),
                new SqlParameter("@Role", user.Role),
                new SqlParameter("@IsActive", user.IsActive),
                new SqlParameter("@LastLogin", DBNull.Value)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        #endregion

        #region Update Operations

        /// <summary>
        /// Updates an existing user
        /// </summary>
        public bool UpdateUser(User user)
        {
            string query = @"
                UPDATE Users SET
                    FullName = @FullName,
                    Email = @Email,
                    PasswordHash = @PasswordHash,
                    Phone = @Phone,
                    Role = @Role,
                    IsActive = @IsActive,
                    UpdatedAt = GETDATE()
                WHERE UserID = @UserID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", user.UserID),
                new SqlParameter("@FullName", user.FullName),
                new SqlParameter("@Email", user.Email),
                new SqlParameter("@PasswordHash", user.PasswordHash),
                new SqlParameter("@Phone", string.IsNullOrEmpty(user.Phone) ? (object)DBNull.Value : user.Phone),
                new SqlParameter("@Role", user.Role),
                new SqlParameter("@IsActive", user.IsActive)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Updates user last login timestamp
        /// </summary>
        public bool UpdateLastLogin(int userId)
        {
            string query = @"
                UPDATE Users SET
                    LastLogin = GETDATE(),
                    UpdatedAt = GETDATE()
                WHERE UserID = @UserID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Deletes a user
        /// </summary>
        public bool DeleteUser(int userId)
        {
            string query = "DELETE FROM Users WHERE UserID = @UserID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        #endregion

        #region Mapping

        /// <summary>
        /// Maps a DataRow to a User object
        /// </summary>
        private User MapToUser(DataRow row)
        {
            User user = new User();
            user.UserID = Convert.ToInt32(row["UserID"]);
            user.FullName = row["FullName"].ToString();
            user.Email = row["Email"].ToString();
            user.PasswordHash = row["PasswordHash"].ToString();
            user.Phone = row["Phone"] == DBNull.Value ? null : row["Phone"].ToString();
            user.Role = row["Role"].ToString();
            user.IsActive = Convert.ToBoolean(row["IsActive"]);

            if (row["LastLogin"] != DBNull.Value)
                user.LastLogin = Convert.ToDateTime(row["LastLogin"]);

            user.CreatedAt = Convert.ToDateTime(row["CreatedAt"]);
            return user;
        }

        #endregion
    }
}