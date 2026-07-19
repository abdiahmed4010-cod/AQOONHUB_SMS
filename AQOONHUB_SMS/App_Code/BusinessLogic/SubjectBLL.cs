using System;
using System.Collections.Generic;
using AQOONHUB_SMS.App_Code.DataAccess;
using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;

namespace AQOONHUB_SMS.App_Code.BusinessLogic
{
    /// <summary>
    /// Business logic layer for subject management
    /// </summary>
    public class SubjectBLL
    {
        private SubjectDAL subjectDAL;
        private AuditLogger auditLogger;

        /// <summary>
        /// Initializes a new instance of the SubjectBLL class
        /// </summary>
        public SubjectBLL()
        {
            subjectDAL = new SubjectDAL();
            auditLogger = new AuditLogger();
        }

        #region Retrieval Operations

        /// <summary>
        /// Gets all subjects
        /// </summary>
        public List<Subject> GetAllSubjects()
        {
            try
            {
                return subjectDAL.GetAllSubjects();
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Subject",
                    string.Format("Failed to retrieve all subjects: {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Gets subjects filtered by active status
        /// </summary>
        public List<Subject> GetSubjectsByStatus(bool activeOnly)
        {
            try
            {
                return subjectDAL.GetAllSubjects(activeOnly);
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Subject",
                    string.Format("Failed to retrieve subjects (activeOnly={1}): {0}", ex.Message, activeOnly));
                throw;
            }
        }

        /// <summary>
        /// Gets a subject by ID
        /// </summary>
        public Subject GetSubjectById(int subjectID)
        {
            try
            {
                if (subjectID <= 0)
                    throw new ValidationException("Invalid subject ID");

                return subjectDAL.GetSubjectById(subjectID);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Subject",
                    string.Format("Failed to retrieve subject ID {1}: {0}", ex.Message, subjectID));
                throw;
            }
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// Adds a new subject with validation
        /// </summary>
        public int AddSubject(Subject subject, int createdBy)
        {
            // Validate required fields
            if (subject == null)
                throw new ValidationException("Subject data is required");

            if (string.IsNullOrWhiteSpace(subject.SubjectName))
                throw new ValidationException("Subject name is required");

            if (subject.SubjectName.Length > 100)
                throw new ValidationException("Subject name must not exceed 100 characters");

            if (string.IsNullOrWhiteSpace(subject.SubjectCode))
                throw new ValidationException("Subject code is required");

            if (subject.SubjectCode.Length > 10)
                throw new ValidationException("Subject code must be 10 characters or less");

            if (string.IsNullOrWhiteSpace(subject.Status))
                throw new ValidationException("Status is required");

            string[] validStatuses = new string[] { "Active", "Inactive" };
            bool statusValid = false;
            for (int i = 0; i < validStatuses.Length; i++)
            {
                if (validStatuses[i] == subject.Status)
                {
                    statusValid = true;
                    break;
                }
            }
            if (!statusValid)
                throw new ValidationException("Status must be Active or Inactive");

            // Check for duplicate subject code
            List<Subject> existingSubjects = subjectDAL.GetAllSubjects();
            for (int i = 0; i < existingSubjects.Count; i++)
            {
                if (existingSubjects[i].SubjectCode.Trim().ToUpper() == subject.SubjectCode.Trim().ToUpper())
                    throw new ValidationException(string.Format("A subject with the code '{0}' already exists", subject.SubjectCode));
            }

            // Check for duplicate subject name
            for (int i = 0; i < existingSubjects.Count; i++)
            {
                if (existingSubjects[i].SubjectName.Trim().ToLower() == subject.SubjectName.Trim().ToLower())
                    throw new ValidationException(string.Format("A subject with the name '{0}' already exists", subject.SubjectName));
            }

            try
            {
                int subjectID = subjectDAL.AddSubject(subject);

                auditLogger.LogCreate(createdBy, "Subject", "Subject", subject.SubjectCode,
                    string.Format("Added subject: {0} ({1})", subject.SubjectName, subject.SubjectCode));

                return subjectID;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(createdBy, "ERROR", "Subject",
                    string.Format("Failed to add subject '{1}': {0}", ex.Message, subject.SubjectName));
                throw;
            }
        }

        #endregion

        #region Update Operations

        /// <summary>
        /// Updates an existing subject with validation
        /// </summary>
        public bool UpdateSubject(Subject subject, int updatedBy)
        {
            // Validate required fields
            if (subject == null)
                throw new ValidationException("Subject data is required");

            if (subject.SubjectID <= 0)
                throw new ValidationException("Invalid subject ID");

            if (string.IsNullOrWhiteSpace(subject.SubjectName))
                throw new ValidationException("Subject name is required");

            if (subject.SubjectName.Length > 100)
                throw new ValidationException("Subject name must not exceed 100 characters");

            if (string.IsNullOrWhiteSpace(subject.SubjectCode))
                throw new ValidationException("Subject code is required");

            if (subject.SubjectCode.Length > 10)
                throw new ValidationException("Subject code must be 10 characters or less");

            if (string.IsNullOrWhiteSpace(subject.Status))
                throw new ValidationException("Status is required");

            string[] validStatuses = new string[] { "Active", "Inactive" };
            bool statusValid = false;
            for (int i = 0; i < validStatuses.Length; i++)
            {
                if (validStatuses[i] == subject.Status)
                {
                    statusValid = true;
                    break;
                }
            }
            if (!statusValid)
                throw new ValidationException("Status must be Active or Inactive");

            // Verify the subject exists
            Subject existing = subjectDAL.GetSubjectById(subject.SubjectID);
            if (existing == null)
                throw new ValidationException("Subject not found");

            // Check for duplicate subject code (excluding current record)
            List<Subject> allSubjects = subjectDAL.GetAllSubjects();
            for (int i = 0; i < allSubjects.Count; i++)
            {
                if (allSubjects[i].SubjectID != subject.SubjectID &&
                    allSubjects[i].SubjectCode.Trim().ToUpper() == subject.SubjectCode.Trim().ToUpper())
                {
                    throw new ValidationException(string.Format("A subject with the code '{0}' already exists", subject.SubjectCode));
                }
            }

            // Check for duplicate subject name (excluding current record)
            for (int i = 0; i < allSubjects.Count; i++)
            {
                if (allSubjects[i].SubjectID != subject.SubjectID &&
                    allSubjects[i].SubjectName.Trim().ToLower() == subject.SubjectName.Trim().ToLower())
                {
                    throw new ValidationException(string.Format("A subject with the name '{0}' already exists", subject.SubjectName));
                }
            }

            try
            {
                bool result = subjectDAL.UpdateSubject(subject);

                if (result)
                {
                    // Log field changes
                    if (existing.SubjectName != subject.SubjectName)
                    {
                        auditLogger.LogUpdate(updatedBy, "Subject", "Subject",
                            subject.SubjectID.ToString(), "SubjectName", existing.SubjectName, subject.SubjectName);
                    }
                    if (existing.SubjectCode != subject.SubjectCode)
                    {
                        auditLogger.LogUpdate(updatedBy, "Subject", "Subject",
                            subject.SubjectID.ToString(), "SubjectCode", existing.SubjectCode, subject.SubjectCode);
                    }
                    if (existing.Description != subject.Description)
                    {
                        auditLogger.LogUpdate(updatedBy, "Subject", "Subject",
                            subject.SubjectID.ToString(), "Description",
                            existing.Description ?? string.Empty, subject.Description ?? string.Empty);
                    }
                    if (existing.Status != subject.Status)
                    {
                        auditLogger.LogUpdate(updatedBy, "Subject", "Subject",
                            subject.SubjectID.ToString(), "Status", existing.Status, subject.Status);
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
                auditLogger.LogAction(updatedBy, "ERROR", "Subject",
                    string.Format("Failed to update subject ID {1}: {0}", ex.Message, subject.SubjectID));
                throw;
            }
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Soft deletes a subject
        /// </summary>
        public bool SoftDeleteSubject(int subjectID, int deletedBy)
        {
            if (subjectID <= 0)
                throw new ValidationException("Invalid subject ID");

            Subject existing = subjectDAL.GetSubjectById(subjectID);
            if (existing == null)
                throw new ValidationException("Subject not found");

            if (existing.Status == "Inactive")
                throw new ValidationException("Subject is already inactive");

            try
            {
                bool result = subjectDAL.SoftDeleteSubject(subjectID);

                if (result)
                {
                    auditLogger.LogDelete(deletedBy, "Subject", "Subject",
                        subjectID.ToString(),
                        string.Format("Soft deleted subject: {0} ({1})", existing.SubjectName, existing.SubjectCode));
                }

                return result;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(deletedBy, "ERROR", "Subject",
                    string.Format("Failed to soft delete subject ID {1}: {0}", ex.Message, subjectID));
                throw;
            }
        }

        #endregion

        #region Business Rules

        /// <summary>
        /// Validates whether a new subject can be created
        /// </summary>
        public List<string> ValidateNewSubject(Subject subject)
        {
            List<string> errors = new List<string>();

            if (subject == null)
            {
                errors.Add("Subject data is required");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(subject.SubjectName))
                errors.Add("Subject name is required");
            else if (subject.SubjectName.Length > 100)
                errors.Add("Subject name must not exceed 100 characters");

            if (string.IsNullOrWhiteSpace(subject.SubjectCode))
                errors.Add("Subject code is required");
            else if (subject.SubjectCode.Length > 10)
                errors.Add("Subject code must be 10 characters or less");

            if (string.IsNullOrWhiteSpace(subject.Status))
                errors.Add("Status is required");

            return errors;
        }

        /// <summary>
        /// Checks if a subject can be deleted
        /// </summary>
        public bool CanDeleteSubject(int subjectID, out string reason)
        {
            reason = string.Empty;

            if (subjectID <= 0)
            {
                reason = "Invalid subject ID";
                return false;
            }

            Subject subject = subjectDAL.GetSubjectById(subjectID);
            if (subject == null)
            {
                reason = "Subject not found";
                return false;
            }

            if (subject.Status == "Inactive")
            {
                reason = "Subject is already inactive";
                return false;
            }

            return true;
        }

        #endregion
    }
}