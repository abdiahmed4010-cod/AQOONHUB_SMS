using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using AQOONHUB.DataAccess;
using AQOONHUB.Models;
using AQOONHUB.Utilities;

namespace AQOONHUB.BusinessLogic
{
    /// <summary>
    /// Business logic layer for managing student admissions
    /// </summary>
    public class AdmissionBLL
    {
        private DatabaseHelper db;
        private AuditLogger auditLogger;
        private StudentDAL studentDAL;

        /// <summary>
        /// Initializes a new instance of the AdmissionBLL class
        /// </summary>
        public AdmissionBLL()
        {
            db = new DatabaseHelper();
            auditLogger = new AuditLogger();
            studentDAL = new StudentDAL();
        }

        #region Retrieval Methods

        /// <summary>
        /// Retrieves all admissions with optional filtering
        /// </summary>
        /// <param name="status">Filter by status (optional)</param>
        /// <param name="search">Search term for name or application number (optional)</param>
        /// <returns>List of Admission objects</returns>
        public List<Admission> GetAdmissions(string status, string search)
        {
            try
            {
                List<Admission> admissions = new List<Admission>();

                string query = @"
                    SELECT a.*, c.ClassName as ApplyingForClassName, u.FullName as ReviewedByName
                    FROM Admissions a
                    INNER JOIN Classes c ON a.ApplyingForClassID = c.ClassID
                    LEFT JOIN Users u ON a.ReviewedBy = u.UserID
                    WHERE 1=1";

                List<SqlParameter> parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(status))
                {
                    query += " AND a.Status = @Status";
                    parameters.Add(new SqlParameter("@Status", status));
                }

                if (!string.IsNullOrEmpty(search))
                {
                    query += @" AND (a.FirstName LIKE @Search OR a.LastName LIKE @Search 
                              OR a.ApplicationNo LIKE @Search OR a.GuardianName LIKE @Search)";
                    parameters.Add(new SqlParameter("@Search", "%" + search + "%"));
                }

                query += " ORDER BY a.ApplicationDate DESC";

                DataTable dt = db.ExecuteQuery(query, parameters.ToArray());

                foreach (DataRow row in dt.Rows)
                {
                    admissions.Add(MapToAdmission(row));
                }

                return admissions;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Admissions", string.Format("GetAdmissions failed: {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Retrieves all admissions without filtering
        /// </summary>
        /// <returns>List of all Admission objects</returns>
        public List<Admission> GetAdmissions()
        {
            return GetAdmissions(null, null);
        }

        /// <summary>
        /// Retrieves a single admission by its ID
        /// </summary>
        /// <param name="admissionId">The admission ID</param>
        /// <returns>Admission object or null if not found</returns>
        public Admission GetAdmissionById(int admissionId)
        {
            try
            {
                if (admissionId <= 0)
                {
                    throw new ArgumentException("Admission ID must be greater than zero.", "admissionId");
                }

                string query = @"
                    SELECT a.*, c.ClassName as ApplyingForClassName, u.FullName as ReviewedByName
                    FROM Admissions a
                    INNER JOIN Classes c ON a.ApplyingForClassID = c.ClassID
                    LEFT JOIN Users u ON a.ReviewedBy = u.UserID
                    WHERE a.AdmissionID = @AdmissionID";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@AdmissionID", admissionId)
                };

                DataTable dt = db.ExecuteQuery(query, parameters);

                if (dt.Rows.Count > 0)
                    return MapToAdmission(dt.Rows[0]);

                return null;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Admissions", string.Format("GetAdmissionById failed for ID {0}: {1}", admissionId, ex.Message));
                throw;
            }
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Creates a new admission application
        /// </summary>
        /// <param name="admission">The admission to create</param>
        /// <param name="userId">ID of the user creating the admission</param>
        /// <returns>The ID of the newly created admission</returns>
        public int CreateAdmission(Admission admission, int userId)
        {
            try
            {
                ValidateAdmission(admission);

                admission.ApplicationNo = GenerateApplicationNo();
                admission.ApplicationDate = DateTime.Now;
                admission.Status = "Pending";

                string query = @"
                    INSERT INTO Admissions (ApplicationNo, FirstName, LastName, Gender, 
                        DateOfBirth, ApplyingForClassID, GuardianName, GuardianPhone, 
                        GuardianEmail, ApplicationDate, Status, Notes)
                    VALUES (@ApplicationNo, @FirstName, @LastName, @Gender,
                        @DateOfBirth, @ApplyingForClassID, @GuardianName, @GuardianPhone,
                        @GuardianEmail, @ApplicationDate, @Status, @Notes);
                    SELECT SCOPE_IDENTITY();";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@ApplicationNo", admission.ApplicationNo),
                    new SqlParameter("@FirstName", admission.FirstName),
                    new SqlParameter("@LastName", admission.LastName),
                    new SqlParameter("@Gender", admission.Gender),
                    new SqlParameter("@DateOfBirth", admission.DateOfBirth),
                    new SqlParameter("@ApplyingForClassID", admission.ApplyingForClassID),
                    new SqlParameter("@GuardianName", string.IsNullOrEmpty(admission.GuardianName) ? (object)DBNull.Value : admission.GuardianName),
                    new SqlParameter("@GuardianPhone", string.IsNullOrEmpty(admission.GuardianPhone) ? (object)DBNull.Value : admission.GuardianPhone),
                    new SqlParameter("@GuardianEmail", string.IsNullOrEmpty(admission.GuardianEmail) ? (object)DBNull.Value : admission.GuardianEmail),
                    new SqlParameter("@ApplicationDate", admission.ApplicationDate),
                    new SqlParameter("@Status", admission.Status),
                    new SqlParameter("@Notes", string.IsNullOrEmpty(admission.Notes) ? (object)DBNull.Value : admission.Notes)
                };

                int newId = Convert.ToInt32(db.ExecuteScalar(query, parameters));

                auditLogger.LogCreate(userId, "Admissions", "Admission", newId.ToString(),
                    string.Format("Application {0} for {1} {2}", admission.ApplicationNo, admission.FirstName, admission.LastName));

                return newId;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(userId, "ERROR", "Admissions", string.Format("CreateAdmission failed: {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Updates an existing admission
        /// </summary>
        /// <param name="admission">The admission with updated values</param>
        /// <param name="userId">ID of the user performing the update</param>
        /// <returns>True if update succeeded</returns>
        public bool UpdateAdmission(Admission admission, int userId)
        {
            try
            {
                if (admission == null)
                {
                    throw new ArgumentNullException("admission");
                }

                if (admission.AdmissionID <= 0)
                {
                    throw new ArgumentException("Admission ID must be greater than zero.", "admission");
                }

                Admission existing = GetAdmissionById(admission.AdmissionID);
                if (existing == null)
                {
                    throw new InvalidOperationException(string.Format("Admission with ID {0} not found.", admission.AdmissionID));
                }

                if (existing.Status == "Approved" || existing.Status == "Rejected")
                {
                    throw new InvalidOperationException("Cannot update an admission that has already been approved or rejected.");
                }

                ValidateAdmission(admission);

                string query = @"
                    UPDATE Admissions SET
                        FirstName = @FirstName,
                        LastName = @LastName,
                        Gender = @Gender,
                        DateOfBirth = @DateOfBirth,
                        ApplyingForClassID = @ApplyingForClassID,
                        GuardianName = @GuardianName,
                        GuardianPhone = @GuardianPhone,
                        GuardianEmail = @GuardianEmail,
                        Notes = @Notes
                    WHERE AdmissionID = @AdmissionID";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@AdmissionID", admission.AdmissionID),
                    new SqlParameter("@FirstName", admission.FirstName),
                    new SqlParameter("@LastName", admission.LastName),
                    new SqlParameter("@Gender", admission.Gender),
                    new SqlParameter("@DateOfBirth", admission.DateOfBirth),
                    new SqlParameter("@ApplyingForClassID", admission.ApplyingForClassID),
                    new SqlParameter("@GuardianName", string.IsNullOrEmpty(admission.GuardianName) ? (object)DBNull.Value : admission.GuardianName),
                    new SqlParameter("@GuardianPhone", string.IsNullOrEmpty(admission.GuardianPhone) ? (object)DBNull.Value : admission.GuardianPhone),
                    new SqlParameter("@GuardianEmail", string.IsNullOrEmpty(admission.GuardianEmail) ? (object)DBNull.Value : admission.GuardianEmail),
                    new SqlParameter("@Notes", string.IsNullOrEmpty(admission.Notes) ? (object)DBNull.Value : admission.Notes)
                };

                bool result = db.ExecuteNonQuery(query, parameters) > 0;

                if (result)
                {
                    auditLogger.LogUpdate(userId, "Admissions", "Admission", admission.AdmissionID.ToString(),
                        "General", string.Format("{0} {1}", existing.FirstName, existing.LastName),
                        string.Format("{0} {1}", admission.FirstName, admission.LastName));
                }

                return result;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(userId, "ERROR", "Admissions", string.Format("UpdateAdmission failed for ID {0}: {1}", admission.AdmissionID, ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Deletes an admission application
        /// </summary>
        /// <param name="admissionId">The admission ID to delete</param>
        /// <param name="userId">ID of the user performing the deletion</param>
        /// <returns>True if deletion succeeded</returns>
        public bool DeleteAdmission(int admissionId, int userId)
        {
            try
            {
                if (admissionId <= 0)
                {
                    throw new ArgumentException("Admission ID must be greater than zero.", "admissionId");
                }

                Admission existing = GetAdmissionById(admissionId);
                if (existing == null)
                {
                    throw new InvalidOperationException(string.Format("Admission with ID {0} not found.", admissionId));
                }

                if (existing.Status == "Approved")
                {
                    throw new InvalidOperationException("Cannot delete an admission that has been approved.");
                }

                string query = "DELETE FROM Admissions WHERE AdmissionID = @AdmissionID";
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@AdmissionID", admissionId)
                };

                bool result = db.ExecuteNonQuery(query, parameters) > 0;

                if (result)
                {
                    auditLogger.LogDelete(userId, "Admissions", "Admission", admissionId.ToString(),
                        string.Format("Application {0} for {1} {2}", existing.ApplicationNo, existing.FirstName, existing.LastName));
                }

                return result;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(userId, "ERROR", "Admissions", string.Format("DeleteAdmission failed for ID {0}: {1}", admissionId, ex.Message));
                throw;
            }
        }

        #endregion

        #region Approval Workflow

        /// <summary>
        /// Approves an admission application
        /// </summary>
        /// <param name="admissionId">The admission ID to approve</param>
        /// <param name="userId">ID of the user approving the admission</param>
        /// <returns>True if approval succeeded</returns>
        public bool ApproveAdmission(int admissionId, int userId)
        {
            try
            {
                if (admissionId <= 0)
                {
                    throw new ArgumentException("Admission ID must be greater than zero.", "admissionId");
                }

                Admission admission = GetAdmissionById(admissionId);
                if (admission == null)
                {
                    throw new InvalidOperationException(string.Format("Admission with ID {0} not found.", admissionId));
                }

                if (admission.Status == "Approved")
                {
                    throw new InvalidOperationException("This admission has already been approved.");
                }

                if (admission.Status == "Rejected")
                {
                    throw new InvalidOperationException("Cannot approve an admission that has been rejected.");
                }

                string query = @"
                    UPDATE Admissions SET
                        Status = 'Approved',
                        ReviewedBy = @ReviewedBy,
                        ReviewedAt = GETDATE()
                    WHERE AdmissionID = @AdmissionID";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@AdmissionID", admissionId),
                    new SqlParameter("@ReviewedBy", userId)
                };

                bool result = db.ExecuteNonQuery(query, parameters) > 0;

                if (result)
                {
                    auditLogger.LogUpdate(userId, "Admissions", "Admission", admissionId.ToString(),
                        "Status", admission.Status, "Approved");
                }

                return result;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(userId, "ERROR", "Admissions", string.Format("ApproveAdmission failed for ID {0}: {1}", admissionId, ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Rejects an admission application
        /// </summary>
        /// <param name="admissionId">The admission ID to reject</param>
        /// <param name="userId">ID of the user rejecting the admission</param>
        /// <param name="rejectionReason">Reason for rejection</param>
        /// <returns>True if rejection succeeded</returns>
        public bool RejectAdmission(int admissionId, int userId, string rejectionReason)
        {
            try
            {
                if (admissionId <= 0)
                {
                    throw new ArgumentException("Admission ID must be greater than zero.", "admissionId");
                }

                Admission admission = GetAdmissionById(admissionId);
                if (admission == null)
                {
                    throw new InvalidOperationException(string.Format("Admission with ID {0} not found.", admissionId));
                }

                if (admission.Status == "Rejected")
                {
                    throw new InvalidOperationException("This admission has already been rejected.");
                }

                if (admission.Status == "Approved")
                {
                    throw new InvalidOperationException("Cannot reject an admission that has been approved.");
                }

                string notes = admission.Notes;
                if (!string.IsNullOrEmpty(rejectionReason))
                {
                    notes = string.IsNullOrEmpty(notes)
                        ? string.Format("Rejection reason: {0}", rejectionReason)
                        : string.Format("{0} | Rejection reason: {1}", notes, rejectionReason);
                }

                string query = @"
                    UPDATE Admissions SET
                        Status = 'Rejected',
                        ReviewedBy = @ReviewedBy,
                        ReviewedAt = GETDATE(),
                        Notes = @Notes
                    WHERE AdmissionID = @AdmissionID";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@AdmissionID", admissionId),
                    new SqlParameter("@ReviewedBy", userId),
                    new SqlParameter("@Notes", string.IsNullOrEmpty(notes) ? (object)DBNull.Value : notes)
                };

                bool result = db.ExecuteNonQuery(query, parameters) > 0;

                if (result)
                {
                    auditLogger.LogUpdate(userId, "Admissions", "Admission", admissionId.ToString(),
                        "Status", admission.Status, "Rejected");
                }

                return result;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(userId, "ERROR", "Admissions", string.Format("RejectAdmission failed for ID {0}: {1}", admissionId, ex.Message));
                throw;
            }
        }

        #endregion

        #region Conversion

        /// <summary>
        /// Converts an approved admission into a registered student
        /// </summary>
        /// <param name="admissionId">The admission ID to convert</param>
        /// <param name="userId">ID of the user performing the conversion</param>
        /// <param name="sectionId">The section ID to assign the student to</param>
        /// <param name="academicYearId">The academic year ID</param>
        /// <param name="guardianId">The guardian ID (if already exists in system)</param>
        /// <returns>The ID of the newly created student</returns>
        public int ConvertAdmissionToStudent(int admissionId, int userId, int sectionId, int academicYearId, int? guardianId)
        {
            try
            {
                if (admissionId <= 0)
                {
                    throw new ArgumentException("Admission ID must be greater than zero.", "admissionId");
                }

                if (sectionId <= 0)
                {
                    throw new ArgumentException("Section ID must be greater than zero.", "sectionId");
                }

                if (academicYearId <= 0)
                {
                    throw new ArgumentException("Academic Year ID must be greater than zero.", "academicYearId");
                }

                Admission admission = GetAdmissionById(admissionId);
                if (admission == null)
                {
                    throw new InvalidOperationException(string.Format("Admission with ID {0} not found.", admissionId));
                }

                if (admission.Status != "Approved")
                {
                    throw new InvalidOperationException("Only approved admissions can be converted to students.");
                }

                // Check if already converted
                string checkQuery = "SELECT COUNT(*) FROM Students WHERE AdmissionNo = @AdmissionNo";
                SqlParameter[] checkParams = new SqlParameter[]
                {
                    new SqlParameter("@AdmissionNo", admission.ApplicationNo)
                };
                int existingCount = Convert.ToInt32(db.ExecuteScalar(checkQuery, checkParams));

                if (existingCount > 0)
                {
                    throw new InvalidOperationException("This admission has already been converted to a student.");
                }

                // Create student from admission data
                Student student = new Student();
                student.StudentCode = studentDAL.GenerateStudentCode();
                student.AdmissionNo = admission.ApplicationNo;
                student.FirstName = admission.FirstName;
                student.LastName = admission.LastName;
                student.Gender = admission.Gender;
                student.DateOfBirth = admission.DateOfBirth;
                student.GuardianID = guardianId.HasValue ? guardianId.Value : 0;
                student.SectionID = sectionId;
                student.AcademicYearID = academicYearId;
                student.Status = "Active";
                student.MedicalNotes = null;
                student.Address = null;

                int studentId = studentDAL.AddStudent(student);

                // Update admission to mark as converted
                string updateQuery = @"
                    UPDATE Admissions SET
                        Status = 'Converted',
                        Notes = ISNULL(Notes, '') + ' | Converted to Student ID: ' + CAST(@StudentID AS VARCHAR)
                    WHERE AdmissionID = @AdmissionID";

                SqlParameter[] updateParams = new SqlParameter[]
                {
                    new SqlParameter("@AdmissionID", admissionId),
                    new SqlParameter("@StudentID", studentId)
                };

                db.ExecuteNonQuery(updateQuery, updateParams);

                auditLogger.LogCreate(userId, "Students", "Student", studentId.ToString(),
                    string.Format("Converted from Admission {0} - {1} {2}", admission.ApplicationNo, admission.FirstName, admission.LastName));

                return studentId;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(userId, "ERROR", "Admissions", string.Format("ConvertAdmissionToStudent failed for ID {0}: {1}", admissionId, ex.Message));
                throw;
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates admission data according to business rules
        /// </summary>
        /// <param name="admission">The admission to validate</param>
        public void ValidateAdmission(Admission admission)
        {
            if (admission == null)
            {
                throw new ArgumentNullException("admission");
            }

            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(admission.FirstName))
            {
                errors.Add("First name is required.");
            }
            else if (admission.FirstName.Length > 100)
            {
                errors.Add("First name cannot exceed 100 characters.");
            }

            if (string.IsNullOrWhiteSpace(admission.LastName))
            {
                errors.Add("Last name is required.");
            }
            else if (admission.LastName.Length > 100)
            {
                errors.Add("Last name cannot exceed 100 characters.");
            }

            if (string.IsNullOrWhiteSpace(admission.Gender))
            {
                errors.Add("Gender is required.");
            }
            else if (admission.Gender != "Male" && admission.Gender != "Female")
            {
                errors.Add("Gender must be either 'Male' or 'Female'.");
            }

            if (admission.DateOfBirth == DateTime.MinValue)
            {
                errors.Add("Date of birth is required.");
            }
            else
            {
                int age = DateTime.Now.Year - admission.DateOfBirth.Year;
                if (DateTime.Now.DayOfYear < admission.DateOfBirth.DayOfYear) age--;

                if (age < 3)
                {
                    errors.Add("Student must be at least 3 years old.");
                }
                if (age > 25)
                {
                    errors.Add("Student age cannot exceed 25 years.");
                }
            }

            if (admission.ApplyingForClassID <= 0)
            {
                errors.Add("Applying for class is required.");
            }

            if (string.IsNullOrWhiteSpace(admission.GuardianName))
            {
                errors.Add("Guardian name is required.");
            }

            if (!string.IsNullOrEmpty(admission.GuardianPhone))
            {
                if (!SecurityHelper.IsValidPhone(admission.GuardianPhone))
                {
                    errors.Add("Guardian phone number is not in a valid format.");
                }
            }

            if (!string.IsNullOrEmpty(admission.GuardianEmail))
            {
                if (!SecurityHelper.IsValidEmail(admission.GuardianEmail))
                {
                    errors.Add("Guardian email address is not valid.");
                }
            }

            if (!string.IsNullOrEmpty(admission.Notes) && admission.Notes.Length > 2000)
            {
                errors.Add("Notes cannot exceed 2000 characters.");
            }

            if (errors.Count > 0)
            {
                throw new ArgumentException(string.Join(" ", errors));
            }
        }

        #endregion

        #region Search

        /// <summary>
        /// Searches admissions by multiple criteria
        /// </summary>
        /// <param name="firstName">First name filter (optional)</param>
        /// <param name="lastName">Last name filter (optional)</param>
        /// <param name="status">Status filter (optional)</param>
        /// <param name="classId">Class ID filter (optional)</param>
        /// <param name="fromDate">Application date from (optional)</param>
        /// <param name="toDate">Application date to (optional)</param>
        /// <returns>List of matching Admission objects</returns>
        public List<Admission> SearchAdmissions(string firstName, string lastName, string status,
            int? classId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                List<Admission> admissions = new List<Admission>();

                string query = @"
                    SELECT a.*, c.ClassName as ApplyingForClassName, u.FullName as ReviewedByName
                    FROM Admissions a
                    INNER JOIN Classes c ON a.ApplyingForClassID = c.ClassID
                    LEFT JOIN Users u ON a.ReviewedBy = u.UserID
                    WHERE 1=1";

                List<SqlParameter> parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(firstName))
                {
                    query += " AND a.FirstName LIKE @FirstName";
                    parameters.Add(new SqlParameter("@FirstName", "%" + firstName + "%"));
                }

                if (!string.IsNullOrEmpty(lastName))
                {
                    query += " AND a.LastName LIKE @LastName";
                    parameters.Add(new SqlParameter("@LastName", "%" + lastName + "%"));
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query += " AND a.Status = @Status";
                    parameters.Add(new SqlParameter("@Status", status));
                }

                if (classId.HasValue)
                {
                    query += " AND a.ApplyingForClassID = @ClassID";
                    parameters.Add(new SqlParameter("@ClassID", classId.Value));
                }

                if (fromDate.HasValue)
                {
                    query += " AND a.ApplicationDate >= @FromDate";
                    parameters.Add(new SqlParameter("@FromDate", fromDate.Value));
                }

                if (toDate.HasValue)
                {
                    query += " AND a.ApplicationDate <= @ToDate";
                    parameters.Add(new SqlParameter("@ToDate", toDate.Value.AddDays(1)));
                }

                query += " ORDER BY a.ApplicationDate DESC";

                DataTable dt = db.ExecuteQuery(query, parameters.ToArray());

                foreach (DataRow row in dt.Rows)
                {
                    admissions.Add(MapToAdmission(row));
                }

                return admissions;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Admissions", string.Format("SearchAdmissions failed: {0}", ex.Message));
                throw;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Generates a unique application number
        /// </summary>
        /// <returns>Unique application number string</returns>
        private string GenerateApplicationNo()
        {
            string query = @"
                SELECT TOP 1 ApplicationNo 
                FROM Admissions 
                WHERE ApplicationNo LIKE 'APP-' + CAST(YEAR(GETDATE()) AS VARCHAR) + '-%'
                ORDER BY ApplicationNo DESC";

            DataTable dt = db.ExecuteQuery(query);

            int nextNumber = 1;
            if (dt.Rows.Count > 0)
            {
                string lastNo = dt.Rows[0]["ApplicationNo"].ToString();
                string lastNum = lastNo.Substring(lastNo.LastIndexOf('-') + 1);
                nextNumber = int.Parse(lastNum) + 1;
            }

            return "APP-" + DateTime.Now.Year.ToString() + "-" + nextNumber.ToString("D4");
        }

        /// <summary>
        /// Maps a DataRow to an Admission object
        /// </summary>
        /// <param name="row">The DataRow to map</param>
        /// <returns>Populated Admission object</returns>
        private Admission MapToAdmission(DataRow row)
        {
            Admission admission = new Admission();
            admission.AdmissionID = Convert.ToInt32(row["AdmissionID"]);
            admission.ApplicationNo = row["ApplicationNo"].ToString();
            admission.FirstName = row["FirstName"].ToString();
            admission.LastName = row["LastName"].ToString();
            admission.Gender = row["Gender"].ToString();
            admission.DateOfBirth = Convert.ToDateTime(row["DateOfBirth"]);
            admission.ApplyingForClassID = Convert.ToInt32(row["ApplyingForClassID"]);
            admission.ApplyingForClassName = row["ApplyingForClassName"].ToString();
            admission.GuardianName = row["GuardianName"] == DBNull.Value ? null : row["GuardianName"].ToString();
            admission.GuardianPhone = row["GuardianPhone"] == DBNull.Value ? null : row["GuardianPhone"].ToString();
            admission.GuardianEmail = row["GuardianEmail"] == DBNull.Value ? null : row["GuardianEmail"].ToString();
            admission.ApplicationDate = Convert.ToDateTime(row["ApplicationDate"]);
            admission.Status = row["Status"].ToString();
            admission.ReviewedBy = row["ReviewedBy"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ReviewedBy"]);
            admission.ReviewedByName = row["ReviewedByName"] == DBNull.Value ? null : row["ReviewedByName"].ToString();
            admission.ReviewedAt = row["ReviewedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ReviewedAt"]);
            admission.Notes = row["Notes"] == DBNull.Value ? null : row["Notes"].ToString();
            return admission;
        }

        #endregion
    }
}