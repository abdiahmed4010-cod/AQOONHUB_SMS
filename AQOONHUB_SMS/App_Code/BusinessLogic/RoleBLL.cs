using System;
using System.Collections.Generic;
using AQOONHUB_SMS.App_Code.DataAccess;
using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;

namespace AQOONHUB_SMS.App_Code.BusinessLogic
{
    /// <summary>
    /// Business logic layer for role management
    /// </summary>
    public class RoleBLL
    {
        private RoleDAL roleDAL;
        private AuditLogger auditLogger;

        /// <summary>
        /// Initializes a new instance of the RoleBLL class
        /// </summary>
        public RoleBLL()
        {
            roleDAL = new RoleDAL();
            auditLogger = new AuditLogger();
        }

        #region Retrieval Operations

        /// <summary>
        /// Gets all roles
        /// </summary>
        public List<Role> GetAllRoles()
        {
            try
            {
                return roleDAL.GetAllRoles();
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Role",
                    string.Format("Failed to retrieve all roles: {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Gets a role by ID
        /// </summary>
        public Role GetRoleById(int roleID)
        {
            try
            {
                if (roleID <= 0)
                    throw new ValidationException("Invalid role ID");

                return roleDAL.GetRoleById(roleID);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Role",
                    string.Format("Failed to retrieve role ID {1}: {0}", ex.Message, roleID));
                throw;
            }
        }

        /// <summary>
        /// Gets a role by name
        /// </summary>
        public Role GetRoleByName(string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleName))
                    throw new ValidationException("Role name is required");

                return roleDAL.GetRoleByName(roleName);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Role",
                    string.Format("Failed to retrieve role by name '{1}': {0}", ex.Message, roleName));
                throw;
            }
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// Adds a new role
        /// </summary>
        public int AddRole(Role role, int createdBy)
        {
            if (role == null)
                throw new ValidationException("Role data is required");

            if (string.IsNullOrWhiteSpace(role.RoleName))
                throw new ValidationException("Role name is required");

            if (role.RoleName.Length > 50)
                throw new ValidationException("Role name cannot exceed 50 characters");

            // Check for duplicate role name
            Role existing = roleDAL.GetRoleByName(role.RoleName);
            if (existing != null)
                throw new ValidationException("Role name already exists");

            try
            {
                int roleID = roleDAL.AddRole(role);

                auditLogger.LogCreate(createdBy, "Role", "Role", roleID.ToString(),
                    string.Format("Added role '{0}'", role.RoleName));

                return roleID;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(createdBy, "ERROR", "Role",
                    string.Format("Failed to add role '{1}': {0}", ex.Message, role.RoleName));
                throw;
            }
        }

        #endregion

        #region Update Operations

        /// <summary>
        /// Updates an existing role
        /// </summary>
        public bool UpdateRole(Role role, int updatedBy)
        {
            if (role == null)
                throw new ValidationException("Role data is required");

            if (role.RoleID <= 0)
                throw new ValidationException("Invalid role ID");

            if (string.IsNullOrWhiteSpace(role.RoleName))
                throw new ValidationException("Role name is required");

            if (role.RoleName.Length > 50)
                throw new ValidationException("Role name cannot exceed 50 characters");

            // Verify the role exists
            Role existing = roleDAL.GetRoleById(role.RoleID);
            if (existing == null)
                throw new ValidationException("Role not found");

            // Check for duplicate name (excluding current role)
            if (!string.Equals(existing.RoleName, role.RoleName, StringComparison.OrdinalIgnoreCase))
            {
                Role duplicate = roleDAL.GetRoleByName(role.RoleName);
                if (duplicate != null && duplicate.RoleID != role.RoleID)
                    throw new ValidationException("Role name already exists");
            }

            try
            {
                bool result = roleDAL.UpdateRole(role);

                if (result)
                {
                    if (!string.Equals(existing.RoleName, role.RoleName, StringComparison.OrdinalIgnoreCase))
                    {
                        auditLogger.LogUpdate(updatedBy, "Role", "Role",
                            role.RoleID.ToString(), "RoleName",
                            existing.RoleName, role.RoleName);
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
                auditLogger.LogAction(updatedBy, "ERROR", "Role",
                    string.Format("Failed to update role ID {1}: {0}", ex.Message, role.RoleID));
                throw;
            }
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Deletes a role
        /// </summary>
        public bool DeleteRole(int roleID, int deletedBy)
        {
            if (roleID <= 0)
                throw new ValidationException("Invalid role ID");

            Role existing = roleDAL.GetRoleById(roleID);
            if (existing == null)
                throw new ValidationException("Role not found");

            try
            {
                bool result = roleDAL.DeleteRole(roleID);

                if (result)
                {
                    auditLogger.LogDelete(deletedBy, "Role", "Role",
                        roleID.ToString(),
                        string.Format("Deleted role '{0}'", existing.RoleName));
                }

                return result;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(deletedBy, "ERROR", "Role",
                    string.Format("Failed to delete role ID {1}: {0}", ex.Message, roleID));
                throw;
            }
        }

        #endregion
    }
}