using System;
using System.Collections.Generic;
using AQOONHUB_SMS.App_Code.DataAccess;
using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;

namespace AQOONHUB_SMS.App_Code.BusinessLogic
{
    /// <summary>
    /// Business logic layer for permission management
    /// </summary>
    public class PermissionBLL
    {
        private PermissionDAL permissionDAL;
        private AuditLogger auditLogger;

        /// <summary>
        /// Initializes a new instance of the PermissionBLL class
        /// </summary>
        public PermissionBLL()
        {
            permissionDAL = new PermissionDAL();
            auditLogger = new AuditLogger();
        }

        #region Retrieval Operations

        /// <summary>
        /// Gets all permissions
        /// </summary>
        public List<Permission> GetAllPermissions()
        {
            try
            {
                return permissionDAL.GetAllPermissions();
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Permission",
                    string.Format("Failed to retrieve all permissions: {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Gets a permission by ID
        /// </summary>
        public Permission GetPermissionById(int permissionID)
        {
            try
            {
                if (permissionID <= 0)
                    throw new ValidationException("Invalid permission ID");

                return permissionDAL.GetPermissionById(permissionID);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Permission",
                    string.Format("Failed to retrieve permission ID {1}: {0}", ex.Message, permissionID));
                throw;
            }
        }

        /// <summary>
        /// Gets a permission by name
        /// </summary>
        public Permission GetPermissionByName(string permissionName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(permissionName))
                    throw new ValidationException("Permission name is required");

                return permissionDAL.GetPermissionByName(permissionName);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Permission",
                    string.Format("Failed to retrieve permission by name '{1}': {0}", ex.Message, permissionName));
                throw;
            }
        }

        /// <summary>
        /// Gets permissions by module
        /// </summary>
        public List<Permission> GetPermissionsByModule(string moduleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(moduleName))
                    throw new ValidationException("Module name is required");

                return permissionDAL.GetPermissionsByModule(moduleName);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Permission",
                    string.Format("Failed to retrieve permissions for module '{1}': {0}", ex.Message, moduleName));
                throw;
            }
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// Adds a new permission
        /// </summary>
        public int AddPermission(Permission permission, int createdBy)
        {
            if (permission == null)
                throw new ValidationException("Permission data is required");

            if (string.IsNullOrWhiteSpace(permission.PermissionName))
                throw new ValidationException("Permission name is required");

            if (permission.PermissionName.Length > 100)
                throw new ValidationException("Permission name cannot exceed 100 characters");

            if (string.IsNullOrWhiteSpace(permission.Module))
                throw new ValidationException("Module name is required");

            // Check for duplicate permission name
            Permission existing = permissionDAL.GetPermissionByName(permission.PermissionName);
            if (existing != null)
                throw new ValidationException("Permission name already exists");

            try
            {
                int permissionID = permissionDAL.AddPermission(permission);

                auditLogger.LogCreate(createdBy, "Permission", "Permission", permissionID.ToString(),
                    string.Format("Added permission '{0}' in module '{1}'", permission.PermissionName, permission.Module));

                return permissionID;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(createdBy, "ERROR", "Permission",
                    string.Format("Failed to add permission '{1}': {0}", ex.Message, permission.PermissionName));
                throw;
            }
        }

        #endregion

        #region Update Operations

        /// <summary>
        /// Updates an existing permission
        /// </summary>
        public bool UpdatePermission(Permission permission, int updatedBy)
        {
            if (permission == null)
                throw new ValidationException("Permission data is required");

            if (permission.PermissionID <= 0)
                throw new ValidationException("Invalid permission ID");

            if (string.IsNullOrWhiteSpace(permission.PermissionName))
                throw new ValidationException("Permission name is required");

            if (permission.PermissionName.Length > 100)
                throw new ValidationException("Permission name cannot exceed 100 characters");

            if (string.IsNullOrWhiteSpace(permission.Module))
                throw new ValidationException("Module name is required");

            // Verify the permission exists
            Permission existing = permissionDAL.GetPermissionById(permission.PermissionID);
            if (existing == null)
                throw new ValidationException("Permission not found");

            // Check for duplicate name (excluding current permission)
            if (!string.Equals(existing.PermissionName, permission.PermissionName, StringComparison.OrdinalIgnoreCase))
            {
                Permission duplicate = permissionDAL.GetPermissionByName(permission.PermissionName);
                if (duplicate != null && duplicate.PermissionID != permission.PermissionID)
                    throw new ValidationException("Permission name already exists");
            }

            try
            {
                bool result = permissionDAL.UpdatePermission(permission);

                if (result)
                {
                    if (!string.Equals(existing.PermissionName, permission.PermissionName, StringComparison.OrdinalIgnoreCase))
                    {
                        auditLogger.LogUpdate(updatedBy, "Permission", "Permission",
                            permission.PermissionID.ToString(), "PermissionName",
                            existing.PermissionName, permission.PermissionName);
                    }
                    if (!string.Equals(existing.Module, permission.Module, StringComparison.OrdinalIgnoreCase))
                    {
                        auditLogger.LogUpdate(updatedBy, "Permission", "Permission",
                            permission.PermissionID.ToString(), "Module",
                            existing.Module, permission.Module);
                    }
                }

                return result;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(updatedBy, "ERROR", "Permission",
                    string.Format("Failed to update permission ID {1}: {0}", ex.Message, permission.PermissionID));
                throw;
            }
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Deletes a permission
        /// </summary>
        public bool DeletePermission(int permissionID, int deletedBy)
        {
            if (permissionID <= 0)
                throw new ValidationException("Invalid permission ID");

            Permission existing = permissionDAL.GetPermissionById(permissionID);
            if (existing == null)
                throw new ValidationException("Permission not found");

            try
            {
                bool result = permissionDAL.DeletePermission(permissionID);

                if (result)
                {
                    auditLogger.LogDelete(deletedBy, "Permission", "Permission",
                        permissionID.ToString(),
                        string.Format("Deleted permission '{0}'", existing.PermissionName));
                }

                return result;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(deletedBy, "ERROR", "Permission",
                    string.Format("Failed to delete permission ID {1}: {0}", ex.Message, permissionID));
                throw;
            }
        }

        #endregion
    }
}