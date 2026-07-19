using System;
using System.Collections.Generic;
using AQOONHUB_SMS.App_Code.DataAccess;
using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;

namespace AQOONHUB_SMS.App_Code.BusinessLogic
{
    /// <summary>
    /// Business logic layer for section management
    /// </summary>
    public class SectionBLL
    {
        private SectionDAL sectionDAL;
        private ClassDAL classDAL;
        private AuditLogger auditLogger;

        /// <summary>
        /// Initializes a new instance of the SectionBLL class
        /// </summary>
        public SectionBLL()
        {
            sectionDAL = new SectionDAL();
            classDAL = new ClassDAL();
            auditLogger = new AuditLogger();
        }

        #region Retrieval Operations

        /// <summary>
        /// Gets all sections
        /// </summary>
        public List<Section> GetAllSections()
        {
            try
            {
                return sectionDAL.GetAllSections();
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Section",
                    string.Format("Failed to retrieve all sections: {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Gets sections filtered by class
        /// </summary>
        public List<Section> GetSectionsByClass(int classID)
        {
            try
            {
                if (classID <= 0)
                    throw new ValidationException("Invalid class ID");

                return sectionDAL.GetSectionsByClass(classID);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Section",
                    string.Format("Failed to retrieve sections for class ID {1}: {0}", ex.Message, classID));
                throw;
            }
        }

        /// <summary>
        /// Gets a section by ID
        /// </summary>
        public Section GetSectionById(int sectionID)
        {
            try
            {
                if (sectionID <= 0)
                    throw new ValidationException("Invalid section ID");

                return sectionDAL.GetSectionById(sectionID);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Section",
                    string.Format("Failed to retrieve section ID {1}: {0}", ex.Message, sectionID));
                throw;
            }
        }

        /// <summary>
        /// Gets the enrollment count for a section
        /// </summary>
        public int GetSectionEnrollment(int sectionID)
        {
            try
            {
                if (sectionID <= 0)
                    throw new ValidationException("Invalid section ID");

                return sectionDAL.GetSectionEnrollment(sectionID);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Section",
                    string.Format("Failed to retrieve enrollment for section ID {1}: {0}", ex.Message, sectionID));
                throw;
            }
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// Adds a new section with validation
        /// </summary>
        public int AddSection(Section section, int createdBy)
        {
            // Validate required fields
            if (section == null)
                throw new ValidationException("Section data is required");

            if (section.ClassID <= 0)
                throw new ValidationException("Class is required");

            if (string.IsNullOrWhiteSpace(section.SectionName))
                throw new ValidationException("Section name is required");

            if (section.SectionName.Length > 1)
                throw new ValidationException("Section name must be a single letter (A, B, C, etc.)");

            if (section.Capacity <= 0 || section.Capacity > 100)
                throw new ValidationException("Capacity must be between 1 and 100");

            if (string.IsNullOrWhiteSpace(section.Status))
                throw new ValidationException("Status is required");

            string[] validStatuses = new string[] { "Active", "Inactive" };
            bool statusValid = false;
            for (int i = 0; i < validStatuses.Length; i++)
            {
                if (validStatuses[i] == section.Status)
                {
                    statusValid = true;
                    break;
                }
            }
            if (!statusValid)
                throw new ValidationException("Status must be Active or Inactive");

            // Verify the class exists
            Class classObj = classDAL.GetClassById(section.ClassID);
            if (classObj == null)
                throw new ValidationException("Specified class does not exist");

            // Check if section already exists for this class
            List<Section> existingSections = sectionDAL.GetSectionsByClass(section.ClassID);
            for (int i = 0; i < existingSections.Count; i++)
            {
                if (existingSections[i].SectionName.Trim().ToUpper() == section.SectionName.Trim().ToUpper())
                    throw new ValidationException(string.Format("Section '{0}' already exists for this class", section.SectionName));
            }

            // Ensure section capacity does not exceed class capacity
            if (section.Capacity > classObj.Capacity)
            {
                throw new ValidationException(string.Format("Section capacity ({0}) cannot exceed class capacity ({1})",
                    section.Capacity, classObj.Capacity));
            }

            try
            {
                int sectionID = sectionDAL.AddSection(section);

                auditLogger.LogCreate(createdBy, "Section", "Section", sectionID.ToString(),
                    string.Format("Added Section {0} to class {1} with capacity {2}",
                        section.SectionName, classObj.ClassName, section.Capacity));

                return sectionID;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(createdBy, "ERROR", "Section",
                    string.Format("Failed to add section '{1}' to class ID {2}: {0}", ex.Message, section.SectionName, section.ClassID));
                throw;
            }
        }

        #endregion

        #region Update Operations

        /// <summary>
        /// Updates an existing section with validation
        /// </summary>
        public bool UpdateSection(Section section, int updatedBy)
        {
            // Validate required fields
            if (section == null)
                throw new ValidationException("Section data is required");

            if (section.SectionID <= 0)
                throw new ValidationException("Invalid section ID");

            if (section.ClassID <= 0)
                throw new ValidationException("Class is required");

            if (string.IsNullOrWhiteSpace(section.SectionName))
                throw new ValidationException("Section name is required");

            if (section.SectionName.Length > 1)
                throw new ValidationException("Section name must be a single letter (A, B, C, etc.)");

            if (section.Capacity <= 0 || section.Capacity > 100)
                throw new ValidationException("Capacity must be between 1 and 100");

            if (string.IsNullOrWhiteSpace(section.Status))
                throw new ValidationException("Status is required");

            string[] validStatuses = new string[] { "Active", "Inactive" };
            bool statusValid = false;
            for (int i = 0; i < validStatuses.Length; i++)
            {
                if (validStatuses[i] == section.Status)
                {
                    statusValid = true;
                    break;
                }
            }
            if (!statusValid)
                throw new ValidationException("Status must be Active or Inactive");

            // Verify the section exists
            Section existing = sectionDAL.GetSectionById(section.SectionID);
            if (existing == null)
                throw new ValidationException("Section not found");

            // Verify the class exists
            Class classObj = classDAL.GetClassById(section.ClassID);
            if (classObj == null)
                throw new ValidationException("Specified class does not exist");

            // Check for duplicate section name within the same class (excluding current record)
            List<Section> allSections = sectionDAL.GetSectionsByClass(section.ClassID);
            for (int i = 0; i < allSections.Count; i++)
            {
                if (allSections[i].SectionID != section.SectionID &&
                    allSections[i].SectionName.Trim().ToUpper() == section.SectionName.Trim().ToUpper())
                {
                    throw new ValidationException(string.Format("Section '{0}' already exists for this class", section.SectionName));
                }
            }

            // Ensure section capacity does not exceed class capacity
            if (section.Capacity > classObj.Capacity)
            {
                throw new ValidationException(string.Format("Section capacity ({0}) cannot exceed class capacity ({1})",
                    section.Capacity, classObj.Capacity));
            }

            // Ensure new capacity is not less than current enrollment
            int currentEnrollment = sectionDAL.GetSectionEnrollment(section.SectionID);
            if (section.Capacity < currentEnrollment)
            {
                throw new ValidationException(string.Format("Cannot reduce capacity below current enrollment ({0})", currentEnrollment));
            }

            try
            {
                bool result = sectionDAL.UpdateSection(section);

                if (result)
                {
                    // Log field changes
                    if (existing.SectionName != section.SectionName)
                    {
                        auditLogger.LogUpdate(updatedBy, "Section", "Section",
                            section.SectionID.ToString(), "SectionName", existing.SectionName, section.SectionName);
                    }
                    if (existing.Capacity != section.Capacity)
                    {
                        auditLogger.LogUpdate(updatedBy, "Section", "Section",
                            section.SectionID.ToString(), "Capacity",
                            existing.Capacity.ToString(), section.Capacity.ToString());
                    }
                    if (existing.ClassID != section.ClassID)
                    {
                        auditLogger.LogUpdate(updatedBy, "Section", "Section",
                            section.SectionID.ToString(), "ClassID",
                            existing.ClassID.ToString(), section.ClassID.ToString());
                    }
                    if (existing.Status != section.Status)
                    {
                        auditLogger.LogUpdate(updatedBy, "Section", "Section",
                            section.SectionID.ToString(), "Status", existing.Status, section.Status);
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
                auditLogger.LogAction(updatedBy, "ERROR", "Section",
                    string.Format("Failed to update section ID {1}: {0}", ex.Message, section.SectionID));
                throw;
            }
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Soft deletes a section
        /// </summary>
        public bool SoftDeleteSection(int sectionID, int deletedBy)
        {
            if (sectionID <= 0)
                throw new ValidationException("Invalid section ID");

            Section existing = sectionDAL.GetSectionById(sectionID);
            if (existing == null)
                throw new ValidationException("Section not found");

            if (existing.Status == "Inactive")
                throw new ValidationException("Section is already inactive");

            // Check if section has enrolled students
            int enrollment = sectionDAL.GetSectionEnrollment(sectionID);
            if (enrollment > 0)
                throw new ValidationException(string.Format("Cannot delete section with {0} enrolled students", enrollment));

            try
            {
                bool result = sectionDAL.SoftDeleteSection(sectionID);

                if (result)
                {
                    auditLogger.LogDelete(deletedBy, "Section", "Section",
                        sectionID.ToString(),
                        string.Format("Soft deleted section: {0}", existing.SectionName));
                }

                return result;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(deletedBy, "ERROR", "Section",
                    string.Format("Failed to soft delete section ID {1}: {0}", ex.Message, sectionID));
                throw;
            }
        }

        #endregion

        #region Business Rules

        /// <summary>
        /// Validates whether a new section can be created
        /// </summary>
        public List<string> ValidateNewSection(Section section)
        {
            List<string> errors = new List<string>();

            if (section == null)
            {
                errors.Add("Section data is required");
                return errors;
            }

            if (section.ClassID <= 0)
                errors.Add("Class is required");

            if (string.IsNullOrWhiteSpace(section.SectionName))
                errors.Add("Section name is required");
            else if (section.SectionName.Length > 1)
                errors.Add("Section name must be a single letter");

            if (section.Capacity <= 0 || section.Capacity > 100)
                errors.Add("Capacity must be between 1 and 100");

            if (string.IsNullOrWhiteSpace(section.Status))
                errors.Add("Status is required");

            return errors;
        }

        /// <summary>
        /// Checks if a section can be deleted
        /// </summary>
        public bool CanDeleteSection(int sectionID, out string reason)
        {
            reason = string.Empty;

            if (sectionID <= 0)
            {
                reason = "Invalid section ID";
                return false;
            }

            Section section = sectionDAL.GetSectionById(sectionID);
            if (section == null)
            {
                reason = "Section not found";
                return false;
            }

            if (section.Status == "Inactive")
            {
                reason = "Section is already inactive";
                return false;
            }

            int enrollment = sectionDAL.GetSectionEnrollment(sectionID);
            if (enrollment > 0)
            {
                reason = string.Format("Section has {0} enrolled students", enrollment);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Finds the section with the lowest enrollment for a given class
        /// </summary>
        public int FindSectionWithLowestEnrollment(int classID)
        {
            if (classID <= 0)
                throw new ValidationException("Invalid class ID");

            List<Section> sections = sectionDAL.GetSectionsByClass(classID);
            int bestSectionID = 0;
            int lowestEnrollment = int.MaxValue;

            for (int i = 0; i < sections.Count; i++)
            {
                int sectionID = sections[i].SectionID;
                int enrollment = sectionDAL.GetSectionEnrollment(sectionID);

                if (enrollment < lowestEnrollment)
                {
                    lowestEnrollment = enrollment;
                    bestSectionID = sectionID;
                }
            }

            return bestSectionID;
        }

        #endregion
    }
}