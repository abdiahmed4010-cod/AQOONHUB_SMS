using System;
using System.Collections.Generic;
using AQOONHUB_SMS.App_Code.DataAccess;
using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;

namespace AQOONHUB_SMS.App_Code.BusinessLogic
{
    /// <summary>
    /// Business logic layer for academic year management
    /// </summary>
    public class AcademicYearBLL
    {
        private AcademicYearDAL academicYearDAL;
        private AuditLogger auditLogger;

        /// <summary>
        /// Initializes a new instance of the AcademicYearBLL class
        /// </summary>
        public AcademicYearBLL()
        {
            academicYearDAL = new AcademicYearDAL();
            auditLogger = new AuditLogger();
        }

        #region Retrieval Operations

        /// <summary>
        /// Gets all academic years
        /// </summary>
        public List<AcademicYear> GetAllAcademicYears()
        {
            try
            {
                return academicYearDAL.GetAllAcademicYears();
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "AcademicYear",
                    string.Format("Failed to retrieve all academic years: {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Gets academic years filtered by status
        /// </summary>
        public List<AcademicYear> GetAcademicYearsByStatus(string status)
        {
            try
            {
                return academicYearDAL.GetAllAcademicYears(status);
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "AcademicYear",
                    string.Format("Failed to retrieve academic years by status '{1}': {0}", ex.Message, status));
                throw;
            }
        }

        /// <summary>
        /// Gets an academic year by ID
        /// </summary>
        public AcademicYear GetAcademicYearById(int academicYearID)
        {
            try
            {
                if (academicYearID <= 0)
                    throw new ValidationException("Invalid academic year ID");

                return academicYearDAL.GetAcademicYearById(academicYearID);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "AcademicYear",
                    string.Format("Failed to retrieve academic year ID {1}: {0}", ex.Message, academicYearID));
                throw;
            }
        }

        /// <summary>
        /// Gets the current active academic year
        /// </summary>
        public AcademicYear GetCurrentAcademicYear()
        {
            try
            {
                return academicYearDAL.GetCurrentAcademicYear();
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "AcademicYear",
                    string.Format("Failed to retrieve current academic year: {0}", ex.Message));
                throw;
            }
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// Adds a new academic year with validation
        /// </summary>
        public int AddAcademicYear(AcademicYear academicYear, int createdBy)
        {
            // Validate required fields
            if (academicYear == null)
                throw new ValidationException("Academic year data is required");

            if (string.IsNullOrWhiteSpace(academicYear.YearName))
                throw new ValidationException("Year name is required");

            if (academicYear.YearName.Length > 50)
                throw new ValidationException("Year name must not exceed 50 characters");

            if (academicYear.StartDate == DateTime.MinValue)
                throw new ValidationException("Start date is required");

            if (academicYear.EndDate == DateTime.MinValue)
                throw new ValidationException("End date is required");

            if (academicYear.StartDate >= academicYear.EndDate)
                throw new ValidationException("Start date must be before end date");

            if (string.IsNullOrWhiteSpace(academicYear.Status))
                throw new ValidationException("Status is required");

            string[] validStatuses = new string[] { "Active", "Closed", "Upcoming" };
            bool statusValid = false;
            for (int i = 0; i < validStatuses.Length; i++)
            {
                if (validStatuses[i] == academicYear.Status)
                {
                    statusValid = true;
                    break;
                }
            }
            if (!statusValid)
                throw new ValidationException("Status must be Active, Closed, or Upcoming");

            // Check for duplicate year name
            List<AcademicYear> existingYears = academicYearDAL.GetAllAcademicYears();
            for (int i = 0; i < existingYears.Count; i++)
            {
                if (existingYears[i].YearName.Trim().ToLower() == academicYear.YearName.Trim().ToLower())
                    throw new ValidationException(string.Format("An academic year with the name '{0}' already exists", academicYear.YearName));
            }

            // Check for date overlap with existing active years
            for (int i = 0; i < existingYears.Count; i++)
            {
                if (existingYears[i].Status == "Active" &&
                    academicYear.StartDate <= existingYears[i].EndDate &&
                    academicYear.EndDate >= existingYears[i].StartDate)
                {
                    throw new ValidationException(string.Format("Date range overlaps with existing academic year '{0}'", existingYears[i].YearName));
                }
            }

            try
            {
                int academicYearID = academicYearDAL.AddAcademicYear(academicYear);

                auditLogger.LogCreate(createdBy, "AcademicYear", "AcademicYear", academicYearID.ToString(),
                    string.Format("Added academic year: {0} ({1:yyyy-MM-dd} to {2:yyyy-MM-dd})",
                        academicYear.YearName, academicYear.StartDate, academicYear.EndDate));

                return academicYearID;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(createdBy, "ERROR", "AcademicYear",
                    string.Format("Failed to add academic year '{1}': {0}", ex.Message, academicYear.YearName));
                throw;
            }
        }

        #endregion

        #region Update Operations

        /// <summary>
        /// Updates an existing academic year with validation
        /// </summary>
        public bool UpdateAcademicYear(AcademicYear academicYear, int updatedBy)
        {
            // Validate required fields
            if (academicYear == null)
                throw new ValidationException("Academic year data is required");

            if (academicYear.AcademicYearID <= 0)
                throw new ValidationException("Invalid academic year ID");

            if (string.IsNullOrWhiteSpace(academicYear.YearName))
                throw new ValidationException("Year name is required");

            if (academicYear.YearName.Length > 50)
                throw new ValidationException("Year name must not exceed 50 characters");

            if (academicYear.StartDate == DateTime.MinValue)
                throw new ValidationException("Start date is required");

            if (academicYear.EndDate == DateTime.MinValue)
                throw new ValidationException("End date is required");

            if (academicYear.StartDate >= academicYear.EndDate)
                throw new ValidationException("Start date must be before end date");

            if (string.IsNullOrWhiteSpace(academicYear.Status))
                throw new ValidationException("Status is required");

            string[] validStatuses = new string[] { "Active", "Closed", "Upcoming" };
            bool statusValid = false;
            for (int i = 0; i < validStatuses.Length; i++)
            {
                if (validStatuses[i] == academicYear.Status)
                {
                    statusValid = true;
                    break;
                }
            }
            if (!statusValid)
                throw new ValidationException("Status must be Active, Closed, or Upcoming");

            // Verify the academic year exists
            AcademicYear existing = academicYearDAL.GetAcademicYearById(academicYear.AcademicYearID);
            if (existing == null)
                throw new ValidationException("Academic year not found");

            // Check for duplicate year name (excluding current record)
            List<AcademicYear> allYears = academicYearDAL.GetAllAcademicYears();
            for (int i = 0; i < allYears.Count; i++)
            {
                if (allYears[i].AcademicYearID != academicYear.AcademicYearID &&
                    allYears[i].YearName.Trim().ToLower() == academicYear.YearName.Trim().ToLower())
                {
                    throw new ValidationException(string.Format("An academic year with the name '{0}' already exists", academicYear.YearName));
                }
            }

            // Check for date overlap with other active years (excluding current record)
            for (int i = 0; i < allYears.Count; i++)
            {
                if (allYears[i].AcademicYearID != academicYear.AcademicYearID &&
                    allYears[i].Status == "Active" &&
                    academicYear.StartDate <= allYears[i].EndDate &&
                    academicYear.EndDate >= allYears[i].StartDate)
                {
                    throw new ValidationException(string.Format("Date range overlaps with existing academic year '{0}'", allYears[i].YearName));
                }
            }

            try
            {
                bool result = academicYearDAL.UpdateAcademicYear(academicYear);

                if (result)
                {
                    // Log field changes
                    if (existing.YearName != academicYear.YearName)
                    {
                        auditLogger.LogUpdate(updatedBy, "AcademicYear", "AcademicYear",
                            academicYear.AcademicYearID.ToString(), "YearName", existing.YearName, academicYear.YearName);
                    }
                    if (existing.StartDate != academicYear.StartDate)
                    {
                        auditLogger.LogUpdate(updatedBy, "AcademicYear", "AcademicYear",
                            academicYear.AcademicYearID.ToString(), "StartDate",
                            existing.StartDate.ToString("yyyy-MM-dd"), academicYear.StartDate.ToString("yyyy-MM-dd"));
                    }
                    if (existing.EndDate != academicYear.EndDate)
                    {
                        auditLogger.LogUpdate(updatedBy, "AcademicYear", "AcademicYear",
                            academicYear.AcademicYearID.ToString(), "EndDate",
                            existing.EndDate.ToString("yyyy-MM-dd"), academicYear.EndDate.ToString("yyyy-MM-dd"));
                    }
                    if (existing.Status != academicYear.Status)
                    {
                        auditLogger.LogUpdate(updatedBy, "AcademicYear", "AcademicYear",
                            academicYear.AcademicYearID.ToString(), "Status", existing.Status, academicYear.Status);
                    }
                    if (existing.IsCurrent != academicYear.IsCurrent)
                    {
                        auditLogger.LogUpdate(updatedBy, "AcademicYear", "AcademicYear",
                            academicYear.AcademicYearID.ToString(), "IsCurrent",
                            existing.IsCurrent.ToString(), academicYear.IsCurrent.ToString());
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
                auditLogger.LogAction(updatedBy, "ERROR", "AcademicYear",
                    string.Format("Failed to update academic year ID {1}: {0}", ex.Message, academicYear.AcademicYearID));
                throw;
            }
        }

        /// <summary>
        /// Sets the specified academic year as current, clearing all others
        /// </summary>
        public bool SetCurrentAcademicYear(int academicYearID, int setBy)
        {
            if (academicYearID <= 0)
                throw new ValidationException("Invalid academic year ID");

            AcademicYear targetYear = academicYearDAL.GetAcademicYearById(academicYearID);
            if (targetYear == null)
                throw new ValidationException("Academic year not found");

            if (targetYear.Status != "Active")
                throw new ValidationException("Only active academic years can be set as current");

            if (targetYear.IsCurrent)
                throw new ValidationException("This is already the current academic year");

            try
            {
                bool cleared = academicYearDAL.ClearCurrentFlag();
                if (!cleared)
                {
                    auditLogger.LogAction(setBy, "WARNING", "AcademicYear",
                        string.Format("Failed to clear current flag before setting year ID {0} as current", academicYearID));
                }

                targetYear.IsCurrent = true;
                bool result = academicYearDAL.UpdateAcademicYear(targetYear);

                if (result)
                {
                    auditLogger.LogAction(setBy, "YEAR_CHANGE", "AcademicYear",
                        string.Format("Set academic year '{0}' as current", targetYear.YearName));
                }

                return result;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(setBy, "ERROR", "AcademicYear",
                    string.Format("Failed to set academic year ID {1} as current: {0}", ex.Message, academicYearID));
                throw;
            }
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Soft deletes an academic year
        /// </summary>
        public bool SoftDeleteAcademicYear(int academicYearID, int deletedBy)
        {
            if (academicYearID <= 0)
                throw new ValidationException("Invalid academic year ID");

            AcademicYear existing = academicYearDAL.GetAcademicYearById(academicYearID);
            if (existing == null)
                throw new ValidationException("Academic year not found");

            if (existing.IsCurrent)
                throw new ValidationException("Cannot delete the current academic year. Please set another year as current first.");

            if (existing.Status == "Closed")
                throw new ValidationException("Academic year is already closed");

            try
            {
                bool result = academicYearDAL.SoftDeleteAcademicYear(academicYearID);

                if (result)
                {
                    auditLogger.LogDelete(deletedBy, "AcademicYear", "AcademicYear",
                        academicYearID.ToString(),
                        string.Format("Soft deleted academic year: {0}", existing.YearName));
                }

                return result;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(deletedBy, "ERROR", "AcademicYear",
                    string.Format("Failed to soft delete academic year ID {1}: {0}", ex.Message, academicYearID));
                throw;
            }
        }

        #endregion

        #region Business Rules

        /// <summary>
        /// Validates whether a new academic year can be created
        /// </summary>
        public List<string> ValidateNewAcademicYear(AcademicYear academicYear)
        {
            List<string> errors = new List<string>();

            if (academicYear == null)
            {
                errors.Add("Academic year data is required");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(academicYear.YearName))
                errors.Add("Year name is required");
            else if (academicYear.YearName.Length > 50)
                errors.Add("Year name must not exceed 50 characters");

            if (academicYear.StartDate == DateTime.MinValue)
                errors.Add("Start date is required");

            if (academicYear.EndDate == DateTime.MinValue)
                errors.Add("End date is required");

            if (academicYear.StartDate != DateTime.MinValue && academicYear.EndDate != DateTime.MinValue)
            {
                if (academicYear.StartDate >= academicYear.EndDate)
                    errors.Add("Start date must be before end date");

                if ((academicYear.EndDate - academicYear.StartDate).TotalDays < 30)
                    errors.Add("Academic year must be at least 30 days long");
            }

            if (string.IsNullOrWhiteSpace(academicYear.Status))
                errors.Add("Status is required");

            return errors;
        }

        /// <summary>
        /// Checks if an academic year can be deleted
        /// </summary>
        public bool CanDeleteAcademicYear(int academicYearID, out string reason)
        {
            reason = string.Empty;

            if (academicYearID <= 0)
            {
                reason = "Invalid academic year ID";
                return false;
            }

            AcademicYear year = academicYearDAL.GetAcademicYearById(academicYearID);
            if (year == null)
            {
                reason = "Academic year not found";
                return false;
            }

            if (year.IsCurrent)
            {
                reason = "Cannot delete the current academic year";
                return false;
            }

            if (year.Status == "Closed")
            {
                reason = "Academic year is already closed";
                return false;
            }

            return true;
        }

        #endregion
    }
}