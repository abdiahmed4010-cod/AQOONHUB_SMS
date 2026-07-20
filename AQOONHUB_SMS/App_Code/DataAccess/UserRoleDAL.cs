using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;


namespace AQOONHUB_SMS.App_Code.DataAccess
{
    /// <summary>
    /// Data access layer for user-role assignment management
    /// </summary>
    public class UserRoleDAL
    {
        private DatabaseHelper db;

        /// <summary>
        /// Initializes a new instance of the UserRoleDAL class
        /// </summary>
        public UserRoleDAL()
        {
            db = new DatabaseHelper();
        }

        #region Retrieval Operations

        /// <summary>
        /// Gets all user-role assignments
        /// </summary>
        public List<UserRole> GetAllUserRoles()
        {
            List<UserRole> userRoles = new List<UserRole>();

            string query = "SELECT * FROM UserRoles ORDER BY AssignedAt DESC";

            DataTable dt = db.ExecuteQuery(query);

            foreach (DataRow row in dt.Rows)
            {
                userRoles.Add(MapToUserRole(row));
            }

            return userRoles;
        }

        /// <summary>
        /// Gets user-role assignment by ID
        /// </summary>
        public UserRole GetUserRoleById(int userRoleId)
        {
            string query = "SELECT * FROM UserRoles WHERE UserRoleID = @UserRoleID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserRoleID", userRoleId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
                return MapToUserRole(dt.Rows[0]);

            return null;
        }

        /// <summary>
        /// Gets user-role assignments by user ID
        /// </summary>
        public List<UserRole> GetUserRolesByUserId(int userId)
        {
            List<UserRole> userRoles = new List<UserRole>();

            string query = "SELECT * FROM UserRoles WHERE UserID = @UserID ORDER BY AssignedAt DESC";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                userRoles.Add(MapToUserRole(row));
            }

            return userRoles;
        }

        /// <summary>
        /// Gets user-role assignments by role ID
        /// </summary>
        public List<UserRole> GetUserRolesByRoleId(int roleId)
        {
            List<UserRole> userRoles = new List<UserRole>();

            string query = "SELECT * FROM UserRoles WHERE RoleID = @RoleID ORDER BY AssignedAt DESC";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@RoleID", roleId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                userRoles.Add(MapToUserRole(row));
            }

            return userRoles;
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// Adds a new user-role assignment
        /// </summary>
        public int AddUserRole(UserRole userRole)
        {
            string query = @"
                INSERT INTO UserRoles (UserID, RoleID, AssignedBy, AssignedAt)
                VALUES (@UserID, @RoleID, @AssignedBy, GETDATE());
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userRole.UserID),
                new SqlParameter("@RoleID", userRole.RoleID),
                new SqlParameter("@AssignedBy", userRole.AssignedBy)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        #endregion

        #region Update Operations

        /// <summary>
        /// Updates an existing user-role assignment
        /// </summary>
        public bool UpdateUserRole(UserRole userRole)
        {
            string query = @"
                UPDATE UserRoles SET
                    UserID = @UserID,
                    RoleID = @RoleID,
                    AssignedBy = @AssignedBy,
                    AssignedAt = GETDATE()
                WHERE UserRoleID = @UserRoleID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserRoleID", userRole.UserRoleID),
                new SqlParameter("@UserID", userRole.UserID),
                new SqlParameter("@RoleID", userRole.RoleID),
                new SqlParameter("@AssignedBy", userRole.AssignedBy)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Deletes a user-role assignment
        /// </summary>
        public bool DeleteUserRole(int userRoleId)
        {
            string query = "DELETE FROM UserRoles WHERE UserRoleID = @UserRoleID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserRoleID", userRoleId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Deletes all user-role assignments for a user
        /// </summary>
        public bool DeleteUserRolesByUserId(int userId)
        {
            string query = "DELETE FROM UserRoles WHERE UserID = @UserID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Deletes all user-role assignments for a role
        /// </summary>
        public bool DeleteUserRolesByRoleId(int roleId)
        {
            string query = "DELETE FROM UserRoles WHERE RoleID = @RoleID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@RoleID", roleId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        #endregion

        #region Mapping

        /// <summary>
        /// Maps a DataRow to a UserRole object
        /// </summary>
        private UserRole MapToUserRole(DataRow row)
        {
            UserRole userRole = new UserRole();
            userRole.UserRoleID = Convert.ToInt32(row["UserRoleID"]);
            userRole.UserID = Convert.ToInt32(row["UserID"]);
            userRole.RoleID = Convert.ToInt32(row["RoleID"]);
            userRole.AssignedBy = Convert.ToInt32(row["AssignedBy"]);
            userRole.AssignedAt = Convert.ToDateTime(row["AssignedAt"]);
            return userRole;
        }

        #endregion
    }
}