using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;


namespace AQOONHUB_SMS.App_Code.DataAccess
{
    /// <summary>
    /// Data access layer for role-permission assignment management
    /// </summary>
    public class RolePermissionDAL
    {
        private DatabaseHelper db;

        /// <summary>
        /// Initializes a new instance of the RolePermissionDAL class
        /// </summary>
        public RolePermissionDAL()
        {
            db = new DatabaseHelper();
        }

        #region Retrieval Operations

        /// <summary>
        /// Gets all role-permission assignments
        /// </summary>
        public List<RolePermission> GetAllRolePermissions()
        {
            List<RolePermission> rolePermissions = new List<RolePermission>();

            string query = "SELECT * FROM RolePermissions ORDER BY AssignedAt DESC";

            DataTable dt = db.ExecuteQuery(query);

            foreach (DataRow row in dt.Rows)
            {
                rolePermissions.Add(MapToRolePermission(row));
            }

            return rolePermissions;
        }

        /// <summary>
        /// Gets role-permission assignment by ID
        /// </summary>
        public RolePermission GetRolePermissionById(int rolePermissionId)
        {
            string query = "SELECT * FROM RolePermissions WHERE RolePermissionID = @RolePermissionID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@RolePermissionID", rolePermissionId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
                return MapToRolePermission(dt.Rows[0]);

            return null;
        }

        /// <summary>
        /// Gets role-permission assignments by role ID
        /// </summary>
        public List<RolePermission> GetRolePermissionsByRoleId(int roleId)
        {
            List<RolePermission> rolePermissions = new List<RolePermission>();

            string query = "SELECT * FROM RolePermissions WHERE RoleID = @RoleID ORDER BY AssignedAt DESC";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@RoleID", roleId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                rolePermissions.Add(MapToRolePermission(row));
            }

            return rolePermissions;
        }

        /// <summary>
        /// Gets role-permission assignments by permission ID
        /// </summary>
        public List<RolePermission> GetRolePermissionsByPermissionId(int permissionId)
        {
            List<RolePermission> rolePermissions = new List<RolePermission>();

            string query = "SELECT * FROM RolePermissions WHERE PermissionID = @PermissionID ORDER BY AssignedAt DESC";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PermissionID", permissionId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                rolePermissions.Add(MapToRolePermission(row));
            }

            return rolePermissions;
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// Adds a new role-permission assignment
        /// </summary>
        public int AddRolePermission(RolePermission rolePermission)
        {
            string query = @"
                INSERT INTO RolePermissions (RoleID, PermissionID, AssignedAt)
                VALUES (@RoleID, @PermissionID, GETDATE());
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@RoleID", rolePermission.RoleID),
                new SqlParameter("@PermissionID", rolePermission.PermissionID)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        #endregion

        #region Update Operations

        /// <summary>
        /// Updates an existing role-permission assignment
        /// </summary>
        public bool UpdateRolePermission(RolePermission rolePermission)
        {
            string query = @"
                UPDATE RolePermissions SET
                    RoleID = @RoleID,
                    PermissionID = @PermissionID,
                    AssignedAt = GETDATE()
                WHERE RolePermissionID = @RolePermissionID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@RolePermissionID", rolePermission.RolePermissionID),
                new SqlParameter("@RoleID", rolePermission.RoleID),
                new SqlParameter("@PermissionID", rolePermission.PermissionID)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Deletes a role-permission assignment
        /// </summary>
        public bool DeleteRolePermission(int rolePermissionId)
        {
            string query = "DELETE FROM RolePermissions WHERE RolePermissionID = @RolePermissionID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@RolePermissionID", rolePermissionId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Deletes all role-permission assignments for a role
        /// </summary>
        public bool DeleteRolePermissionsByRoleId(int roleId)
        {
            string query = "DELETE FROM RolePermissions WHERE RoleID = @RoleID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@RoleID", roleId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Deletes all role-permission assignments for a permission
        /// </summary>
        public bool DeleteRolePermissionsByPermissionId(int permissionId)
        {
            string query = "DELETE FROM RolePermissions WHERE PermissionID = @PermissionID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PermissionID", permissionId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        #endregion

        #region Mapping

        /// <summary>
        /// Maps a DataRow to a RolePermission object
        /// </summary>
        private RolePermission MapToRolePermission(DataRow row)
        {
            RolePermission rolePermission = new RolePermission();
            rolePermission.RolePermissionID = Convert.ToInt32(row["RolePermissionID"]);
            rolePermission.RoleID = Convert.ToInt32(row["RoleID"]);
            rolePermission.PermissionID = Convert.ToInt32(row["PermissionID"]);
            rolePermission.AssignedAt = Convert.ToDateTime(row["AssignedAt"]);
            return rolePermission;
        }

        #endregion
    }
}