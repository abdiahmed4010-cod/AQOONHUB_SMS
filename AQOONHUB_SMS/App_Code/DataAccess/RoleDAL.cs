using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace AQOONHUB_SMS.App_Code.DataAccess
{
    /// <summary>
    /// Data access layer for role management
    /// </summary>
    public class RoleDAL
    {
        private DatabaseHelper db;

        public RoleDAL()
        {
            db = new DatabaseHelper();
        }

        #region Retrieval Operations

        /// <summary>
        /// Gets all roles
        /// </summary>
        public List<Role> GetAllRoles()
        {
            string query = "SELECT * FROM Roles ORDER BY RoleName";
            DataTable dt = db.ExecuteQuery(query);

            List<Role> roles = new List<Role>();
            foreach (DataRow row in dt.Rows)
            {
                roles.Add(MapToRole(row));
            }
            return roles;
        }

        /// <summary>
        /// Gets a role by ID
        /// </summary>
        public Role GetRoleById(int roleId)
        {
            string query = "SELECT * FROM Roles WHERE RoleID = @RoleID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@RoleID", roleId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);
            return dt.Rows.Count > 0 ? MapToRole(dt.Rows[0]) : null;
        }

        /// <summary>
        /// Gets a role by name
        /// </summary>
        public Role GetRoleByName(string roleName)
        {
            string query = "SELECT * FROM Roles WHERE RoleName = @RoleName";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@RoleName", roleName)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);
            return dt.Rows.Count > 0 ? MapToRole(dt.Rows[0]) : null;
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// Adds a new role
        /// </summary>
        public int AddRole(Role role)
        {
            string query = @"
                INSERT INTO Roles (RoleName, Description, IsActive, CreatedAt, UpdatedAt)
                VALUES (@RoleName, @Description, @IsActive, GETDATE(), GETDATE());
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@RoleName", role.RoleName),
                new SqlParameter("@Description", (object)role.Description ?? DBNull.Value),
                new SqlParameter("@IsActive", role.IsActive)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        #endregion

        #region Update Operations

        /// <summary>
        /// Updates an existing role
        /// </summary>
        public bool UpdateRole(Role role)
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
                new SqlParameter("@RoleID", role.RoleID),
                new SqlParameter("@RoleName", role.RoleName),
                new SqlParameter("@Description", (object)role.Description ?? DBNull.Value),
                new SqlParameter("@IsActive", role.IsActive)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Deletes a role
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

        #region Mapping

        /// <summary>
        /// Maps a DataRow to a Role object
        /// </summary>
        private Role MapToRole(DataRow row)
        {
            return new Role
            {
                RoleID = Convert.ToInt32(row["RoleID"]),
                RoleName = row["RoleName"].ToString(),
                Description = row["Description"] != DBNull.Value ? row["Description"].ToString() : null,
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                UpdatedAt = Convert.ToDateTime(row["UpdatedAt"])
            };
        }

        #endregion
    }
}