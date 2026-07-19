using System;
using System.Collections.Generic;
using AQOONHUB_SMS.App_Code.DataAccess;
using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;

namespace AQOONHUB_SMS.App_Code.BusinessLogic
{
    /// <summary>
    /// Business logic layer for term management
    /// </summary>
    public class TermBLL
    {
        private TermDAL termDAL;
        private AuditLogger auditLogger;

        /// <summary>
        /// Initializes a new instance of the TermBLL class
        /// </summary>
        public TermBLL()
        {
            termDAL = new TermDAL();
            auditLogger = new AuditLogger();
        }

        #region Retrieval Operations

        /// <summary>
        /// Gets all terms
        /// </summary>
        public List<Term> GetAllTerms()
        {
            try
            {
                return termDAL.GetAllTerms();
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Term",
                    string.Format("Failed to retrieve all terms: {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Gets terms filtered by academic year
        /// </summary>
        public List<Term> GetTermsByAcademicYear(int academicYearID)
        {
            try
            {
                if (academicYearID <= 0)
                    throw new ValidationException("Invalid academic year ID");

                return termDAL.GetAllTerms(academicYearID);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Term",
                    string.Format("Failed to retrieve terms for academic year ID {1}: {0}", ex.Message, academicYearID));
                throw;
            }
        }

        /// <summary>
        /// Gets a term by ID
        /// </summary>
        public Term GetTermById(int termID)
        {
            try
            {
                if (termID <= 0)
                    throw new ValidationException("Invalid term ID");

                return termDAL.GetTermById(termID);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Term",
                    string.Format("Failed to retrieve term ID {1}: {0}", ex.Message, termID));
                throw;
            }
        }

        /// <summary>
        /// Gets the current active term
        /// </summary>
        public Term GetCurrentTerm()
        {
            try
            {
                return termDAL.GetCurrentTerm();
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Term",
                    string.Format("Failed to retrieve current term: {0}", ex.Message));
                throw;
            }
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// Adds a new term with validation
        /// </summary>
        public int AddTerm(Term term, int createdBy)
        {
            // Validate required fields
            if (term == null)
                throw new ValidationException("Term data is required");

            if (string.IsNullOrWhiteSpace(term.TermName))
                throw new ValidationException("Term name is required");

            if (term.TermName.Length > 50)
                throw new ValidationException("Term name must not exceed 50 characters");

            if (term.AcademicYearID <= 0)
                throw new ValidationException("Academic year is required");

            if (term.StartDate == DateTime.MinValue)
                throw new ValidationException("Start date is required");

            if (term.EndDate == DateTime.MinValue)
                throw new ValidationException("End date is required");

            if (term.StartDate >= term.EndDate)
                throw new ValidationException("Start date must be before end date");

            if (string.IsNullOrWhiteSpace(term.Status))
                throw new ValidationException("Status is required");

            string[] validStatuses = new string[] { "Active", "Closed", "Upcoming" };
            bool statusValid = false;
            for (int i = 0; i < validStatuses.Length; i++)
            {
                if (validStatuses[i] == term.Status)
                {
                    statusValid = true;
                    break;
                }
            }
            if (!statusValid)
                throw new ValidationException("Status must be Active, Closed, or Upcoming");

            // Verify the academic year exists
            AcademicYearDAL academicYearDAL = new AcademicYearDAL();
            var academicYear = academicYearDAL.GetAcademicYearById(term.AcademicYearID);
            if (academicYear == null)
                throw new ValidationException("Specified academic year does not exist");

            // Check for duplicate term name within the same academic year
            List<Term> existingTerms = termDAL.GetAllTerms(term.AcademicYearID);
            for (int i = 0; i < existingTerms.Count; i++)
            {
                if (existingTerms[i].TermName.Trim().ToLower() == term.TermName.Trim().ToLower())
                    throw new ValidationException(string.Format("A term with the name '{0}' already exists for this academic year", term.TermName));
            }

            // Check for date overlap with existing terms in the same academic year
            for (int i = 0; i < existingTerms.Count; i++)
            {
                if (term.StartDate <= existingTerms[i].EndDate &&
                    term.EndDate >= existingTerms[i].StartDate)
                {
                    throw new ValidationException(string.Format("Date range overlaps with existing term '{0}'", existingTerms[i].TermName));
                }
            }

            // Ensure term dates fall within the academic year date range
            if (term.StartDate < academicYear.StartDate || term.EndDate > academicYear.EndDate)
            {
                throw new ValidationException(string.Format("Term dates must fall within the academic year period ({0:yyyy-MM-dd} to {1:yyyy-MM-dd})",
                    academicYear.StartDate, academicYear.EndDate));
            }

            try
            {
                int termID = termDAL.AddTerm(term);

                auditLogger.LogCreate(createdBy, "Term", "Term", termID.ToString(),
                    string.Format("Added term: {0} for academic year {1} ({2:yyyy-MM-dd} to {3:yyyy-MM-dd})",
                        term.TermName, academicYear.YearName, term.StartDate, term.EndDate));

                return termID;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(createdBy, "ERROR", "Term",
                    string.Format("Failed to add term '{1}': {0}", ex.Message, term.TermName));
                throw;
            }
        }

        #endregion

        #region Update Operations

        /// <summary>
        /// Updates an existing term with validation
        /// </summary>
        public bool UpdateTerm(Term term, int updatedBy)
        {
            // Validate required fields
            if (term == null)
                throw new ValidationException("Term data is required");

            if (term.TermID <= 0)
                throw new ValidationException("Invalid term ID");

            if (string.IsNullOrWhiteSpace(term.TermName))
                throw new ValidationException("Term name is required");

            if (term.TermName.Length > 50)
                throw new ValidationException("Term name must not exceed 50 characters");

            if (term.AcademicYearID <= 0)
                throw new ValidationException("Academic year is required");

            if (term.StartDate == DateTime.MinValue)
                throw new ValidationException("Start date is required");

            if (term.EndDate == DateTime.MinValue)
                throw new ValidationException("End date is required");

            if (term.StartDate >= term.EndDate)
                throw new ValidationException("Start date must be before end date");

            if (string.IsNullOrWhiteSpace(term.Status))
                throw new ValidationException("Status is required");

            string[] validStatuses = new string[] { "Active", "Closed", "Upcoming" };
            bool statusValid = false;
            for (int i = 0; i < validStatuses.Length; i++)
            {
                if (validStatuses[i] == term.Status)
                {
                    statusValid = true;
                    break;
                }
            }
            if (!statusValid)
                throw new ValidationException("Status must be Active, Closed, or Upcoming");

            // Verify the term exists
            Term existing = termDAL.GetTermById(term.TermID);
            if (existing == null)
                throw new ValidationException("Term not found");

            // Verify the academic year exists
            AcademicYearDAL academicYearDAL = new AcademicYearDAL();
            var academicYear = academicYearDAL.GetAcademicYearById(term.AcademicYearID);
            if (academicYear == null)
                throw new ValidationException("Specified academic year does not exist");

            // Check for duplicate term name within the same academic year (excluding current record)
            List<Term> allTerms = termDAL.GetAllTerms(term.AcademicYearID);
            for (int i = 0; i < allTerms.Count; i++)
            {
                if (allTerms[i].TermID != term.TermID &&
                    allTerms[i].TermName.Trim().ToLower() == term.TermName.Trim().ToLower())
                {
                    throw new ValidationException(string.Format("A term with the name '{0}' already exists for this academic year", term.TermName));
                }
            }

            // Check for date overlap with other terms in the same academic year (excluding current record)
            for (int i = 0; i < allTerms.Count; i++)
            {
                if (allTerms[i].TermID != term.TermID &&
                    term.StartDate <= allTerms[i].EndDate &&
                    term.EndDate >= allTerms[i].StartDate)
                {
                    throw new ValidationException(string.Format("Date range overlaps with existing term '{0}'", allTerms[i].TermName));
                }
            }

            // Ensure term dates fall within the academic year date range
            if (term.StartDate < academicYear.StartDate || term.EndDate > academicYear.EndDate)
            {
                throw new ValidationException(string.Format("Term dates must fall within the academic year period ({0:yyyy-MM-dd} to {1:yyyy-MM-dd})",
                    academicYear.StartDate, academicYear.EndDate));
            }

            try
            {
                bool result = termDAL.UpdateTerm(term);

                if (result)
                {
                    // Log field changes
                    if (existing.TermName != term.TermName)
                    {
                        auditLogger.LogUpdate(updatedBy, "Term", "Term",
                            term.TermID.ToString(), "TermName", existing.TermName, term.TermName);
                    }
                    if (existing.StartDate != term.StartDate)
                    {
                        auditLogger.LogUpdate(updatedBy, "Term", "Term",
                            term.TermID.ToString(), "StartDate",
                            existing.StartDate.ToString("yyyy-MM-dd"), term.StartDate.ToString("yyyy-MM-dd"));
                    }
                    if (existing.EndDate != term.EndDate)
                    {
                        auditLogger.LogUpdate(updatedBy, "Term", "Term",
                            term.TermID.ToString(), "EndDate",
                            existing.EndDate.ToString("yyyy-MM-dd"), term.EndDate.ToString("yyyy-MM-dd"));
                    }
                    if (existing.Status != term.Status)
                    {
                        auditLogger.LogUpdate(updatedBy, "Term", "Term",
                            term.TermID.ToString(), "Status", existing.Status, term.Status);
                    }
                    if (existing.IsCurrent != term.IsCurrent)
                    {
                        auditLogger.LogUpdate(updatedBy, "Term", "Term",
                            term.TermID.ToString(), "IsCurrent",
                            existing.IsCurrent.ToString(), term.IsCurrent.ToString());
                    }
                    if (existing.AcademicYearID != term.AcademicYearID)
                    {
                        auditLogger.LogUpdate(updatedBy, "Term", "Term",
                            term.TermID.ToString(), "AcademicYearID",
                            existing.AcademicYearID.ToString(), term.AcademicYearID.ToString());
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
                auditLogger.LogAction(updatedBy, "ERROR", "Term",
                    string.Format("Failed to update term ID {1}: {0}", ex.Message, term.TermID));
                throw;
            }
        }

        /// <summary>
        /// Sets the specified term as current, clearing all others
        /// </summary>
        public bool SetCurrentTerm(int termID, int setBy)
        {
            if (termID <= 0)
                throw new ValidationException("Invalid term ID");

            Term targetTerm = termDAL.GetTermById(termID);
            if (targetTerm == null)
                throw new ValidationException("Term not found");

            if (targetTerm.Status != "Active")
                throw new ValidationException("Only active terms can be set as current");

            if (targetTerm.IsCurrent)
                throw new ValidationException("This is already the current term");

            try
            {
                bool cleared = termDAL.ClearCurrentFlag();
                if (!cleared)
                {
                    auditLogger.LogAction(setBy, "WARNING", "Term",
                        string.Format("Failed to clear current flag before setting term ID {0} as current", termID));
                }

                targetTerm.IsCurrent = true;
                bool result = termDAL.UpdateTerm(targetTerm);

                if (result)
                {
                    auditLogger.LogAction(setBy, "TERM_CHANGE", "Term",
                        string.Format("Set term '{0}' as current", targetTerm.TermName));

                    // Trigger term change processes
                    OnTermChange(termID);
                }

                return result;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(setBy, "ERROR", "Term",
                    string.Format("Failed to set term ID {1} as current: {0}", ex.Message, termID));
                throw;
            }
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Soft deletes a term
        /// </summary>
        public bool SoftDeleteTerm(int termID, int deletedBy)
        {
            if (termID <= 0)
                throw new ValidationException("Invalid term ID");

            Term existing = termDAL.GetTermById(termID);
            if (existing == null)
                throw new ValidationException("Term not found");

            if (existing.IsCurrent)
                throw new ValidationException("Cannot delete the current term. Please set another term as current first.");

            if (existing.Status == "Closed")
                throw new ValidationException("Term is already closed");

            try
            {
                bool result = termDAL.SoftDeleteTerm(termID);

                if (result)
                {
                    auditLogger.LogDelete(deletedBy, "Term", "Term",
                        termID.ToString(),
                        string.Format("Soft deleted term: {0}", existing.TermName));
                }

                return result;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(deletedBy, "ERROR", "Term",
                    string.Format("Failed to soft delete term ID {1}: {0}", ex.Message, termID));
                throw;
            }
        }

        #endregion

        #region Business Rules

        /// <summary>
        /// Validates whether a new term can be created
        /// </summary>
        public List<string> ValidateNewTerm(Term term)
        {
            List<string> errors = new List<string>();

            if (term == null)
            {
                errors.Add("Term data is required");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(term.TermName))
                errors.Add("Term name is required");
            else if (term.TermName.Length > 50)
                errors.Add("Term name must not exceed 50 characters");

            if (term.AcademicYearID <= 0)
                errors.Add("Academic year is required");

            if (term.StartDate == DateTime.MinValue)
                errors.Add("Start date is required");

            if (term.EndDate == DateTime.MinValue)
                errors.Add("End date is required");

            if (term.StartDate != DateTime.MinValue && term.EndDate != DateTime.MinValue)
            {
                if (term.StartDate >= term.EndDate)
                    errors.Add("Start date must be before end date");

                if ((term.EndDate - term.StartDate).TotalDays < 7)
                    errors.Add("Term must be at least 7 days long");
            }

            if (string.IsNullOrWhiteSpace(term.Status))
                errors.Add("Status is required");

            return errors;
        }

        /// <summary>
        /// Checks if a term can be deleted
        /// </summary>
        public bool CanDeleteTerm(int termID, out string reason)
        {
            reason = string.Empty;

            if (termID <= 0)
            {
                reason = "Invalid term ID";
                return false;
            }

            Term term = termDAL.GetTermById(termID);
            if (term == null)
            {
                reason = "Term not found";
                return false;
            }

            if (term.IsCurrent)
            {
                reason = "Cannot delete the current term";
                return false;
            }

            if (term.Status == "Closed")
            {
                reason = "Term is already closed";
                return false;
            }

            return true;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Trigger processes when term changes
        /// </summary>
        private void OnTermChange(int termID)
        {
            // Trigger processes when term changes:
            // 1. Generate term invoices
            // 2. Reset attendance counters
            // 3. Update exam schedules
            // 4. Notify stakeholders
        }

        #endregion
    }
}