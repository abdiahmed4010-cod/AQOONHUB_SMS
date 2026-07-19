using System;
using System.Collections.Generic;
using AQOONHUB_SMS.App_Code.DataAccess;
using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;

namespace AQOONHUB_SMS.App_Code.BusinessLogic
{
    /// <summary>
    /// Business logic layer for class management
    /// </summary>
    public class ClassBLL
    {
        private ClassDAL classDAL;
        private AuditLogger auditLogger;

        /// <summary>
        /// Initializes a new instance of the ClassBLL class
        /// </summary>
        public ClassBLL()
        {
            classDAL = new ClassDAL();
            auditLogger = new AuditLogger();
        }

        #region Retrieval Operations

        /// <summary>
        /// Gets all classes
        /// </summary>
        public List<Class> GetAllClasses()
        {
            try
            {
                return classDAL.GetAllClasses();
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Class",
                    string.Format("Failed to retrieve all classes: {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Gets a class by ID
        /// </summary>
        public Class GetClassById(int classID)
        {
            try
            {
                if (classID <= 0)
                    throw new ValidationException("Invalid class ID");

                return classDAL.GetClassById(classID);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Class",
                    string.Format("Failed to retrieve class ID {1}: {0}", ex.Message, classID));
                throw;
            }
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// Adds a new class with validation
        /// </summary>
        public int AddClass(Class classObj, int createdBy)
        {
            // Validate required fields
            if (classObj == null)
                throw new ValidationException("Class data is required");

            if (string.IsNullOrWhiteSpace(classObj.ClassName))
                throw new ValidationException("Class name is required");

            if (classObj.ClassName.Length > 50)
                throw new ValidationException("Class name must not exceed 50 characters");

            if (classObj.Capacity <= 0 || classObj.Capacity > 100)
                throw new ValidationException("Capacity must be between 1 and 100");

            if (string.IsNullOrWhiteSpace(classObj.Status))
                throw new ValidationException("Status is required");

            string[] validStatuses = new string[] { "Active", "Inactive" };
            bool statusValid = false;
            for (int i = 0; i < validStatuses.Length; i++)
            {
                if (validStatuses[i] == classObj.Status)
                {
                    statusValid = true;
                    break;
                }
            }
            if (!statusValid)
                throw new ValidationException("Status must be Active or Inactive");

            // Check for duplicate class name
            List<Class> existingClasses = classDAL.GetAllClasses();
            for (int i = 0; i < existingClasses.Count; i++)
            {
                if (existingClasses[i].ClassName.Trim().ToLower() == classObj.ClassName.Trim().ToLower())
                    throw new ValidationException(string.Format("A class with the name '{0}' already exists", classObj.ClassName));
            }

            try
            {
                int classID = classDAL.AddClass(classObj);

                auditLogger.LogCreate(createdBy, "Class", "Class", classID.ToString(),
                    string.Format("Added class: {0} with capacity {1}", classObj.ClassName, classObj.Capacity));

                return classID;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(createdBy, "ERROR", "Class",
                    string.Format("Failed to add class '{1}': {0}", ex.Message, classObj.ClassName));
                throw;
            }
        }

        #endregion

        #region Update Operations

        /// <summary>
        /// Updates an existing class with validation
        /// </summary>
        public bool UpdateClass(Class classObj, int updatedBy)
        {
            // Validate required fields
            if (classObj == null)
                throw new ValidationException("Class data is required");

            if (classObj.ClassID <= 0)
                throw new ValidationException("Invalid class ID");

            if (string.IsNullOrWhiteSpace(classObj.ClassName))
                throw new ValidationException("Class name is required");

            if (classObj.ClassName.Length > 50)
                throw new ValidationException("Class name must not exceed 50 characters");

            if (classObj.Capacity <= 0 || classObj.Capacity > 100)
                throw new ValidationException("Capacity must be between 1 and 100");

            if (string.IsNullOrWhiteSpace(classObj.Status))
                throw new ValidationException("Status is required");

            string[] validStatuses = new string[] { "Active", "Inactive" };
            bool statusValid = false;
            for (int i = 0; i < validStatuses.Length; i++)
            {
                if (validStatuses[i] == classObj.Status)
                {
                    statusValid = true;
                    break;
                }
            }
            if (!statusValid)
                throw new ValidationException("Status must be Active or Inactive");

            // Verify the class exists
            Class existing = classDAL.GetClassById(classObj.ClassID);
            if (existing == null)
                throw new ValidationException("Class not found");

            // Check for duplicate class name (excluding current record)
            List<Class> allClasses = classDAL.GetAllClasses();
            for (int i = 0; i < allClasses.Count; i++)
            {
                if (allClasses[i].ClassID != classObj.ClassID &&
                    allClasses[i].ClassName.Trim().ToLower() == classObj.ClassName.Trim().ToLower())
                {
                    throw new ValidationException(string.Format("A class with the name '{0}' already exists", classObj.ClassName));
                }
            }

            try
            {
                bool result = classDAL.UpdateClass(classObj);

                if (result)
                {
                    // Log field changes
                    if (existing.ClassName != classObj.ClassName)
                    {
                        auditLogger.LogUpdate(updatedBy, "Class", "Class",
                            classObj.ClassID.ToString(), "ClassName", existing.ClassName, classObj.ClassName);
                    }
                    if (existing.Capacity != classObj.Capacity)
                    {
                        auditLogger.LogUpdate(updatedBy, "Class", "Class",
                            classObj.ClassID.ToString(), "Capacity",
                            existing.Capacity.ToString(), classObj.Capacity.ToString());
                    }
                    if (existing.RoomNumber != classObj.RoomNumber)
                    {
                        auditLogger.LogUpdate(updatedBy, "Class", "Class",
                            classObj.ClassID.ToString(), "RoomNumber",
                            existing.RoomNumber ?? string.Empty, classObj.RoomNumber ?? string.Empty);
                    }
                    if (existing.Status != classObj.Status)
                    {
                        auditLogger.LogUpdate(updatedBy, "Class", "Class",
                            classObj.ClassID.ToString(), "Status", existing.Status, classObj.Status);
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
                auditLogger.LogAction(updatedBy, "ERROR", "Class",
                    string.Format("Failed to update class ID {1}: {0}", ex.Message, classObj.ClassID));
                throw;
            }
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Soft deletes a class
        /// </summary>
        public bool SoftDeleteClass(int classID, int deletedBy)
        {
            if (classID <= 0)
                throw new ValidationException("Invalid class ID");

            Class existing = classDAL.GetClassById(classID);
            if (existing == null)
                throw new ValidationException("Class not found");

            if (existing.Status == "Inactive")
                throw new ValidationException("Class is already inactive");

            try
            {
                bool result = classDAL.SoftDeleteClass(classID);

                if (result)
                {
                    auditLogger.LogDelete(deletedBy, "Class", "Class",
                        classID.ToString(),
                        string.Format("Soft deleted class: {0}", existing.ClassName));
                }

                return result;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(deletedBy, "ERROR", "Class",
                    string.Format("Failed to soft delete class ID {1}: {0}", ex.Message, classID));
                throw;
            }
        }

        #endregion

        #region Business Rules

        /// <summary>
        /// Validates whether a new class can be created
        /// </summary>
        public List<string> ValidateNewClass(Class classObj)
        {
            List<string> errors = new List<string>();

            if (classObj == null)
            {
                errors.Add("Class data is required");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(classObj.ClassName))
                errors.Add("Class name is required");
            else if (classObj.ClassName.Length > 50)
                errors.Add("Class name must not exceed 50 characters");

            if (classObj.Capacity <= 0 || classObj.Capacity > 100)
                errors.Add("Capacity must be between 1 and 100");

            if (string.IsNullOrWhiteSpace(classObj.Status))
                errors.Add("Status is required");

            return errors;
        }

        /// <summary>
        /// Checks if a class can be deleted
        /// </summary>
        public bool CanDeleteClass(int classID, out string reason)
        {
            reason = string.Empty;

            if (classID <= 0)
            {
                reason = "Invalid class ID";
                return false;
            }

            Class classObj = classDAL.GetClassById(classID);
            if (classObj == null)
            {
                reason = "Class not found";
                return false;
            }

            if (classObj.Status == "Inactive")
            {
                reason = "Class is already inactive";
                return false;
            }

            return true;
        }

        #endregion
    }
}