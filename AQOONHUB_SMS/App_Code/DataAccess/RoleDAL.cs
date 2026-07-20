using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;


namespace AQOONHUB_SMS.App_Code.DataAccess
{
    public class RoleDAL
    {
        private DatabaseHelper db;

        public RoleDAL()
        {
            db = new DatabaseHelper();
        }

        #region Roles

        /// <summary>
        /// Gets all roles
        /// </summary>
        public DataTable GetAllRoles()
        {
            string query = "SELECT * FROM Roles ORDER BY RoleName";
            return db.ExecuteQuery(query);
        }

        /// <summary>
        /// Gets role by ID
        /// </summary>
        public DataRow GetRoleById(int roleId)
        {
            string query = "SELECT * FROM Roles WHERE RoleID = @RoleID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@RoleID", roleId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>
        /// Gets role by name
        /// </summary>
        public DataRow GetRoleByName(string roleName)
        {
            string query = "SELECT * FROM Roles WHERE RoleName = @RoleName";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@RoleName", roleName)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>
        /// Adds role
        /// </summary>
        public int AddRole(string roleName, string description, bool isActive)
        {
            string query = @"
                INSERT INTO Roles (RoleName, Description, IsActive, CreatedAt, UpdatedAt)
                VALUES (@RoleName, @Description, @IsActive, GETDATE(), GETDATE());
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@RoleName", roleName),
                new SqlParameter("@Description", (object)description ?? DBNull.Value),
                new SqlParameter("@IsActive", isActive)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        /// <summary>
        /// Updates role
        /// </summary>
        public bool UpdateRole(int roleId, string roleName, string description, bool isActive)
        {
            string query = @"
                UPDATE Roles SET
                    RoleName = @RoleName,
                    Description = @Description,
                    IsActive = @IsActive,
                    UpdatedAt = GETDATE()
                WHERE RoleID = @RoleID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@RoleID", roleId),
                new SqlParameter("@RoleName", roleName),
                new SqlParameter("@Description", (object)description ?? DBNull.Value),
                new SqlParameter("@IsActive", isActive)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Deletes role
        /// </summary>
        public bool DeleteRole(int roleId)
        {
            string query = "DELETE FROM Roles WHERE RoleID = @RoleID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@RoleID", roleId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        #endregion
    }
}