using System;
using System.Collections.Generic;
using AQOONHUB_SMS.App_Code.DataAccess;
using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;

namespace AQOONHUB_SMS.App_Code.BusinessLogic
{
    /// <summary>
    /// Business logic layer for role-permission assignment management
    /// </summary>
    public class RolePermissionBLL
    {
        private RolePermissionDAL rolePermissionDAL;
        private RoleDAL roleDAL;
        private PermissionDAL permissionDAL;
        private AuditLogger auditLogger;

        /// <summary>
        /// Initializes a new instance of the RolePermissionBLL class
        /// </summary>
        public RolePermissionBLL()
        {
            rolePermissionDAL = new RolePermissionDAL();
            roleDAL = new RoleDAL();
            permissionDAL = new PermissionDAL();
            auditLogger = new AuditLogger();
        }

        #region Retrieval Operations

        /// <summary>
        /// Gets all role-permission assignments
        /// </summary>
        public List<RolePermission> GetAllRolePermissions()
        {
            try
            {
                return rolePermissionDAL.GetAllRolePermissions();
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "RolePermission",
                    string.Format("Failed to retrieve all role-permission assignments: {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Gets a role-permission assignment by ID
        /// </summary>
        public RolePermission GetRolePermissionById(int rolePermissionID)
        {
            try
            {
                if (rolePermissionID <= 0)
                    throw new ValidationException("Invalid role-permission ID");

                return rolePermissionDAL.GetRolePermissionById(rolePermissionID);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "RolePermission",
                    string.Format("Failed to retrieve role-permission ID {1}: {0}", ex.Message, rolePermissionID));
                throw;
            }
        }

        /// <summary>
        /// Gets permissions assigned to a role
        /// </summary>
        public List<RolePermission> GetRolePermissionsByRoleId(int roleID)
        {
            try
            {
                if (roleID <= 0)
                    throw new ValidationException("Invalid role ID");

                return rolePermissionDAL.GetRolePermissionsByRoleId(roleID);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "RolePermission",
                    string.Format("Failed to retrieve permissions for role ID {1}: {0}", ex.Message, roleID));
                throw;
            }
        }

        /// <summary>
        /// Gets roles assigned to a permission
        /// </summary>
        public List<RolePermission> GetRolePermissionsByPermissionId(int permissionID)
        {
            try
            {
                if (permissionID <= 0)
                    throw new ValidationException("Invalid permission ID");

                return rolePermissionDAL.GetRolePermissionsByPermissionId(permissionID);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "RolePermission",
                    string.Format("Failed to retrieve roles for permission ID {1}: {0}", ex.Message, permissionID));
                throw;
            }
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// Assigns a permission to a role
        /// </summary>
        public int AssignPermissionToRole(RolePermission rolePermission, int createdBy)
        {
            if (rolePermission == null)
                throw new ValidationException("Role-permission data is required");

            if (rolePermission.RoleID <= 0)
                throw new ValidationException("Role is required");

            if (rolePermission.PermissionID <= 0)
                throw new ValidationException("Permission is required");

            // Verify the role exists
            Role role = roleDAL.GetRoleById(rolePermission.RoleID);
            if (role == null)
                throw new ValidationException("Specified role does not exist");

            // Verify the permission exists
            Permission permission = permissionDAL.GetPermissionById(rolePermission.PermissionID);
            if (permission == null)
                throw new ValidationException("Specified permission does not exist");

            // Check for duplicate assignment
            List<RolePermission> existingAssignments = rolePermissionDAL.GetRolePermissionsByRoleId(rolePermission.RoleID);
            for (int i = 0; i < existingAssignments.Count; i++)
            {
                if (existingAssignments[i].PermissionID == rolePermission.PermissionID)
                {
                    throw new ValidationException("Role already has this permission assigned");
                }
            }

            try
            {
                int rolePermissionID = rolePermissionDAL.AddRolePermission(rolePermission);

                auditLogger.LogCreate(createdBy, "RolePermission", "Role Permission Assignment", rolePermissionID.ToString(),
                    string.Format("Assigned permission '{0}' to role '{1}'", permission.PermissionName, role.RoleName));

                return rolePermissionID;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(createdBy, "ERROR", "RolePermission",
                    string.Format("Failed to assign permission ID {1} to role ID {2}: {0}", ex.Message, rolePermission.PermissionID, rolePermission.RoleID));
                throw;
            }
        }

        #endregion

        #region Update Operations

        /// <summary>
        /// Updates a role-permission assignment
        /// </summary>
        public bool UpdateRolePermission(RolePermission rolePermission, int updatedBy)
        {
            if (rolePermission == null)
                throw new ValidationException("Role-permission data is required");

            if (rolePermission.RolePermissionID <= 0)
                throw new ValidationException("Invalid role-permission ID");

            if (rolePermission.RoleID <= 0)
                throw new ValidationException("Role is required");

            if (rolePermission.PermissionID <= 0)
                throw new ValidationException("Permission is required");

            // Verify the assignment exists
            RolePermission existing = rolePermissionDAL.GetRolePermissionById(rolePermission.RolePermissionID);
            if (existing == null)
                throw new ValidationException("Role-permission assignment not found");

            // Verify the role exists
            Role role = roleDAL.GetRoleById(rolePermission.RoleID);
            if (role == null)
                throw new ValidationException("Specified role does not exist");

            // Verify the permission exists
            Permission permission = permissionDAL.GetPermissionById(rolePermission.PermissionID);
            if (permission == null)
                throw new ValidationException("Specified permission does not exist");

            // Check for duplicate assignment (excluding current)
            if (existing.RoleID != rolePermission.RoleID || existing.PermissionID != rolePermission.PermissionID)
            {
                List<RolePermission> existingAssignments = rolePermissionDAL.GetRolePermissionsByRoleId(rolePermission.RoleID);
                for (int i = 0; i < existingAssignments.Count; i++)
                {
                    if (existingAssignments[i].RolePermissionID != rolePermission.RolePermissionID &&
                        existingAssignments[i].PermissionID == rolePermission.PermissionID)
                    {
                        throw new ValidationException("Role already has this permission assigned");
                    }
                }
            }

            try
            {
                bool result = rolePermissionDAL.UpdateRolePermission(rolePermission);

                if (result)
                {
                    if (existing.PermissionID != rolePermission.PermissionID)
                    {
                        auditLogger.LogUpdate(updatedBy, "RolePermission", "Role Permission Assignment",
                            rolePermission.RolePermissionID.ToString(), "PermissionID",
                            existing.PermissionID.ToString(), rolePermission.PermissionID.ToString());
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
                auditLogger.LogAction(updatedBy, "ERROR", "RolePermission",
                    string.Format("Failed to update role-permission ID {1}: {0}", ex.Message, rolePermission.RolePermissionID));
                throw;
            }
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Removes a permission from a role
        /// </summary>
        public bool RemovePermissionFromRole(int rolePermissionID, int deletedBy)
        {
            if (rolePermissionID <= 0)
                throw new ValidationException("Invalid role-permission ID");

            RolePermission existing = rolePermissionDAL.GetRolePermissionById(rolePermissionID);
            if (existing == null)
                throw new ValidationException("Role-permission assignment not found");

            try
            {
                bool result = rolePermissionDAL.DeleteRolePermission(rolePermissionID);

                if (result)
                {
                    auditLogger.LogDelete(deletedBy, "RolePermission", "Role Permission Assignment",
                        rolePermissionID.ToString(),
                        string.Format("Removed permission ID {0} from role ID {1}", existing.PermissionID, existing.RoleID));
                }

                return result;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(deletedBy, "ERROR", "RolePermission",
                    string.Format("Failed to delete role-permission ID {1}: {0}", ex.Message, rolePermissionID));
                throw;
            }
        }

        #endregion
    }
}