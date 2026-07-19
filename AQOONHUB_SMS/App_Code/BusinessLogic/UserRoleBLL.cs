using System;
using System.Collections.Generic;
using AQOONHUB_SMS.App_Code.DataAccess;
using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;

namespace AQOONHUB_SMS.App_Code.BusinessLogic
{
    /// <summary>
    /// Business logic layer for user-role assignment management
    /// </summary>
    public class UserRoleBLL
    {
        private UserRoleDAL userRoleDAL;
        private UserDAL userDAL;
        private RoleDAL roleDAL;
        private AuditLogger auditLogger;

        /// <summary>
        /// Initializes a new instance of the UserRoleBLL class
        /// </summary>
        public UserRoleBLL()
        {
            userRoleDAL = new UserRoleDAL();
            userDAL = new UserDAL();
            roleDAL = new RoleDAL();
            auditLogger = new AuditLogger();
        }

        #region Retrieval Operations

        /// <summary>
        /// Gets all user-role assignments
        /// </summary>
        public List<UserRole> GetAllUserRoles()
        {
            try
            {
                return userRoleDAL.GetAllUserRoles();
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "UserRole",
                    string.Format("Failed to retrieve all user-role assignments: {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Gets a user-role assignment by ID
        /// </summary>
        public UserRole GetUserRoleById(int userRoleID)
        {
            try
            {
                if (userRoleID <= 0)
                    throw new ValidationException("Invalid user-role ID");

                return userRoleDAL.GetUserRoleById(userRoleID);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "UserRole",
                    string.Format("Failed to retrieve user-role ID {1}: {0}", ex.Message, userRoleID));
                throw;
            }
        }

        /// <summary>
        /// Gets roles assigned to a user
        /// </summary>
        public List<UserRole> GetUserRolesByUserId(int userID)
        {
            try
            {
                if (userID <= 0)
                    throw new ValidationException("Invalid user ID");

                return userRoleDAL.GetUserRolesByUserId(userID);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "UserRole",
                    string.Format("Failed to retrieve roles for user ID {1}: {0}", ex.Message, userID));
                throw;
            }
        }

        /// <summary>
        /// Gets users assigned to a role
        /// </summary>
        public List<UserRole> GetUserRolesByRoleId(int roleID)
        {
            try
            {
                if (roleID <= 0)
                    throw new ValidationException("Invalid role ID");

                return userRoleDAL.GetUserRolesByRoleId(roleID);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "UserRole",
                    string.Format("Failed to retrieve users for role ID {1}: {0}", ex.Message, roleID));
                throw;
            }
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// Assigns a role to a user
        /// </summary>
        public int AssignRoleToUser(UserRole userRole, int createdBy)
        {
            if (userRole == null)
                throw new ValidationException("User-role data is required");

            if (userRole.UserID <= 0)
                throw new ValidationException("User is required");

            if (userRole.RoleID <= 0)
                throw new ValidationException("Role is required");

            // Verify the user exists
            User user = userDAL.GetUserById(userRole.UserID);
            if (user == null)
                throw new ValidationException("Specified user does not exist");

            // Verify the role exists
            Role role = roleDAL.GetRoleById(userRole.RoleID);
            if (role == null)
                throw new ValidationException("Specified role does not exist");

            // Check for duplicate assignment
            List<UserRole> existingAssignments = userRoleDAL.GetUserRolesByUserId(userRole.UserID);
            for (int i = 0; i < existingAssignments.Count; i++)
            {
                if (existingAssignments[i].RoleID == userRole.RoleID)
                {
                    throw new ValidationException("User already has this role assigned");
                }
            }

            try
            {
                int userRoleID = userRoleDAL.AddUserRole(userRole);

                auditLogger.LogCreate(createdBy, "UserRole", "User Role Assignment", userRoleID.ToString(),
                    string.Format("Assigned role '{0}' to user ID {1}", role.RoleName, userRole.UserID));

                return userRoleID;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(createdBy, "ERROR", "UserRole",
                    string.Format("Failed to assign role ID {1} to user ID {2}: {0}", ex.Message, userRole.RoleID, userRole.UserID));
                throw;
            }
        }

        #endregion

        #region Update Operations

        /// <summary>
        /// Updates a user-role assignment
        /// </summary>
        public bool UpdateUserRole(UserRole userRole, int updatedBy)
        {
            if (userRole == null)
                throw new ValidationException("User-role data is required");

            if (userRole.UserRoleID <= 0)
                throw new ValidationException("Invalid user-role ID");

            if (userRole.UserID <= 0)
                throw new ValidationException("User is required");

            if (userRole.RoleID <= 0)
                throw new ValidationException("Role is required");

            // Verify the assignment exists
            UserRole existing = userRoleDAL.GetUserRoleById(userRole.UserRoleID);
            if (existing == null)
                throw new ValidationException("User-role assignment not found");

            // Verify the user exists
            User user = userDAL.GetUserById(userRole.UserID);
            if (user == null)
                throw new ValidationException("Specified user does not exist");

            // Verify the role exists
            Role role = roleDAL.GetRoleById(userRole.RoleID);
            if (role == null)
                throw new ValidationException("Specified role does not exist");

            // Check for duplicate assignment (excluding current)
            if (existing.RoleID != userRole.RoleID || existing.UserID != userRole.UserID)
            {
                List<UserRole> existingAssignments = userRoleDAL.GetUserRolesByUserId(userRole.UserID);
                for (int i = 0; i < existingAssignments.Count; i++)
                {
                    if (existingAssignments[i].UserRoleID != userRole.UserRoleID &&
                        existingAssignments[i].RoleID == userRole.RoleID)
                    {
                        throw new ValidationException("User already has this role assigned");
                    }
                }
            }

            try
            {
                bool result = userRoleDAL.UpdateUserRole(userRole);

                if (result)
                {
                    if (existing.RoleID != userRole.RoleID)
                    {
                        auditLogger.LogUpdate(updatedBy, "UserRole", "User Role Assignment",
                            userRole.UserRoleID.ToString(), "RoleID",
                            existing.RoleID.ToString(), userRole.RoleID.ToString());
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
                auditLogger.LogAction(updatedBy, "ERROR", "UserRole",
                    string.Format("Failed to update user-role ID {1}: {0}", ex.Message, userRole.UserRoleID));
                throw;
            }
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Removes a role from a user
        /// </summary>
        public bool RemoveRoleFromUser(int userRoleID, int deletedBy)
        {
            if (userRoleID <= 0)
                throw new ValidationException("Invalid user-role ID");

            UserRole existing = userRoleDAL.GetUserRoleById(userRoleID);
            if (existing == null)
                throw new ValidationException("User-role assignment not found");

            try
            {
                bool result = userRoleDAL.DeleteUserRole(userRoleID);

                if (result)
                {
                    auditLogger.LogDelete(deletedBy, "UserRole", "User Role Assignment",
                        userRoleID.ToString(),
                        string.Format("Removed role assignment for user ID {0}", existing.UserID));
                }

                return result;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(deletedBy, "ERROR", "UserRole",
                    string.Format("Failed to delete user-role ID {1}: {0}", ex.Message, userRoleID));
                throw;
            }
        }

        #endregion
    }
}