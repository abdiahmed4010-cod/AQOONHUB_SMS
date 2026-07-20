using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;


namespace AQOONHUB_SMS.App_Code.DataAccess
{
    /// <summary>
    /// Data access layer for permission management
    /// </summary>
    public class PermissionDAL
    {
        private DatabaseHelper db;

        /// <summary>
        /// Initializes a new instance of the PermissionDAL class
        /// </summary>
        public PermissionDAL()
        {
            db = new DatabaseHelper();
        }

        #region Retrieval Operations

        /// <summary>
        /// Gets all permissions
        /// </summary>
        public List<Permission> GetAllPermissions()
        {
            List<Permission> permissions = new List<Permission>();

            string query = "SELECT * FROM Permissions ORDER BY Module, PermissionName";

            DataTable dt = db.ExecuteQuery(query);

            foreach (DataRow row in dt.Rows)
            {
                permissions.Add(MapToPermission(row));
            }

            return permissions;
        }

        /// <summary>
        /// Gets permission by ID
        /// </summary>
        public Permission GetPermissionById(int permissionId)
        {
            string query = "SELECT * FROM Permissions WHERE PermissionID = @PermissionID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PermissionID", permissionId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
                return MapToPermission(dt.Rows[0]);

            return null;
        }

        /// <summary>
        /// Gets permission by name
        /// </summary>
        public Permission GetPermissionByName(string permissionName)
        {
            string query = "SELECT * FROM Permissions WHERE PermissionName = @PermissionName";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PermissionName", permissionName)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
                return MapToPermission(dt.Rows[0]);

            return null;
        }

        /// <summary>
        /// Gets permissions by module
        /// </summary>
        public List<Permission> GetPermissionsByModule(string module)
        {
            List<Permission> permissions = new List<Permission>();

            string query = "SELECT * FROM Permissions WHERE Module = @Module ORDER BY PermissionName";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Module", module)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                permissions.Add(MapToPermission(row));
            }

            return permissions;
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// Adds a new permission
        /// </summary>
        public int AddPermission(Permission permission)
        {
            string query = @"
                INSERT INTO Permissions (PermissionName, Module, Description, IsActive, CreatedAt)
                VALUES (@PermissionName, @Module, @Description, @IsActive, GETDATE());
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PermissionName", permission.PermissionName),
                new SqlParameter("@Module", permission.Module),
                new SqlParameter("@Description", string.IsNullOrEmpty(permission.Description) ? (object)DBNull.Value : permission.Description),
                new SqlParameter("@IsActive", permission.IsActive)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        #endregion

        #region Update Operations

        /// <summary>
        /// Updates an existing permission
        /// </summary>
        public bool UpdatePermission(Permission permission)
        {
            string query = @"
                UPDATE Permissions SET
                    PermissionName = @PermissionName,
                    Module = @Module,
                    Description = @Description,
                    IsActive = @IsActive
                WHERE PermissionID = @PermissionID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PermissionID", permission.PermissionID),
                new SqlParameter("@PermissionName", permission.PermissionName),
                new SqlParameter("@Module", permission.Module),
                new SqlParameter("@Description", string.IsNullOrEmpty(permission.Description) ? (object)DBNull.Value : permission.Description),
                new SqlParameter("@IsActive", permission.IsActive)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Deletes a permission
        /// </summary>
        public bool DeletePermission(int permissionId)
        {
            string query = "DELETE FROM Permissions WHERE PermissionID = @PermissionID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PermissionID", permissionId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        #endregion

        #region Mapping

        /// <summary>
        /// Maps a DataRow to a Permission object
        /// </summary>
        private Permission MapToPermission(DataRow row)
        {
            Permission permission = new Permission();
            permission.PermissionID = Convert.ToInt32(row["PermissionID"]);
            permission.PermissionName = row["PermissionName"].ToString();
            permission.Module = row["Module"].ToString();
            permission.Description = row["Description"] == DBNull.Value ? null : row["Description"].ToString();
            permission.IsActive = Convert.ToBoolean(row["IsActive"]);
            permission.CreatedAt = Convert.ToDateTime(row["CreatedAt"]);
            return permission;
        }

        #endregion
    }
}