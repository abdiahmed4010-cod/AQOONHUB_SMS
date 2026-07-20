using System;
using System.Collections.Generic;
using System.Data;
using AQOONHUB_SMS.App_Code.DataAccess;
using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;

namespace AQOONHUB_SMS.App_Code.BusinessLogic
{
    public class StudentBLL
    {
        private StudentDAL studentDAL;
        private AuditLogger auditLogger;

        public StudentBLL()
        {
            studentDAL = new StudentDAL();
            auditLogger = new AuditLogger();
        }

        #region Validation

        /// <summary>
        /// Validates student data before save
        /// </summary>
        private List<string> ValidateStudent(Student student, bool isNew)
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(student.FirstName))
                errors.Add("First name is required");

            if (string.IsNullOrWhiteSpace(student.LastName))
                errors.Add("Last name is required");

            if (student.DateOfBirth == DateTime.MinValue)
                errors.Add("Date of birth is required");
            else if (student.DateOfBirth > DateTime.Now.AddYears(-3))
                errors.Add("Student must be at least 3 years old");
            else if (student.DateOfBirth < DateTime.Now.AddYears(-25))
                errors.Add("Student age seems invalid (over 25 years)");

            if (string.IsNullOrWhiteSpace(student.Gender))
                errors.Add("Gender is required");

            if (student.GuardianID <= 0)
                errors.Add("Guardian is required");

            if (student.SectionID <= 0)
                errors.Add("Class/Section is required");

            return errors;
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Gets all students with optional filters
        /// </summary>
        public List<Student> GetStudents(string status = null, string search = null)
        {
            return studentDAL.GetAllStudents(status, search);
        }

        /// <summary>
        /// Gets student by ID
        /// </summary>
        public Student GetStudent(int studentId)
        {
            if (studentId <= 0)
                throw new ArgumentException("Invalid student ID");

            return studentDAL.GetStudentById(studentId);
        }

        /// <summary>
        /// Creates new student with auto-generated IDs
        /// </summary>
        public Student CreateStudent(Student student, int createdBy, string ipAddress)
        {
            // Validate
            var errors = ValidateStudent(student, true);
            if (errors.Count > 0)
                throw new ValidationException(string.Join(", ", errors));

            // Generate IDs
            student.StudentCode = studentDAL.GenerateStudentCode();
            student.AdmissionNo = GenerateAdmissionNo();

            // Set defaults
            student.Status = "Active";
            student.EnrollmentDate = DateTime.Now;
            student.AcademicYearID = GetCurrentAcademicYearId();

            // Save to database
            int newId = studentDAL.AddStudent(student);
            student.StudentID = newId;

            // Generate invoice for new student
            GenerateInitialInvoice(student);

            // Log audit
            auditLogger.LogCreate(createdBy, "Students", "Student", student.StudentCode,
                string.Format("Registered {0} in {1}", student.FullName, student.ClassName));

            return student;
        }

        /// <summary>
        /// Updates existing student
        /// </summary>
        public bool UpdateStudent(Student student, int updatedBy, string ipAddress)
        {
            var errors = ValidateStudent(student, false);
            if (errors.Count > 0)
                throw new ValidationException(string.Join(", ", errors));

            // Get old data for audit
            var oldStudent = studentDAL.GetStudentById(student.StudentID);
            if (oldStudent == null)
                throw new Exception("Student not found");

            // Update
            bool result = studentDAL.UpdateStudent(student);

            if (result)
            {
                // Log changes
                if (oldStudent.Status != student.Status)
                {
                    auditLogger.LogUpdate(updatedBy, "Students", "Student", student.StudentCode,
                        "Status", oldStudent.Status, student.Status);
                }
                if (oldStudent.SectionID != student.SectionID)
                {
                    auditLogger.LogUpdate(updatedBy, "Students", "Student", student.StudentCode,
                        "Section", oldStudent.ClassName, student.ClassName);
                }
            }

            return result;
        }

        /// <summary>
        /// Soft deletes student
        /// </summary>
        public bool DeleteStudent(int studentId, int deletedBy, string ipAddress, string reason)
        {
            var student = studentDAL.GetStudentById(studentId);
            if (student == null)
                throw new Exception("Student not found");

            if (student.Status == "Deleted")
                throw new Exception("Student is already deleted");

            bool result = studentDAL.SoftDeleteStudent(studentId);

            if (result)
            {
                auditLogger.LogDelete(deletedBy, "Students", "Student", student.StudentCode, reason);
            }

            return result;
        }

        #endregion

        #region Business Rules

        /// <summary>
        /// Promotes student to next class
        /// </summary>
        public bool PromoteStudent(int studentId, int newSectionId, int promotedBy)
        {
            var student = studentDAL.GetStudentById(studentId);
            if (student == null)
                throw new Exception("Student not found");

            if (student.Status != "Active")
                throw new Exception("Only active students can be promoted");

            // Check if student passed
            var results = GetStudentResults(studentId);
            bool passed = CheckIfPassed(results);

            if (!passed)
                throw new Exception("Student has not met promotion requirements");

            // Perform promotion
            AcademicDAL academicDAL = new AcademicDAL();
            bool result = academicDAL.PromoteStudent(studentId, newSectionId, GetCurrentAcademicYearId());

            if (result)
            {
                auditLogger.LogAction(promotedBy, "PROMOTE", "Students",
                    string.Format("Promoted {0} ({1}) to new class", student.FullName, student.StudentCode));
            }

            return result;
        }

        /// <summary>
        /// Transfers student to another school
        /// </summary>
        public bool TransferStudent(int studentId, string transferTo, int transferredBy, string reason)
        {
            var student = studentDAL.GetStudentById(studentId);
            if (student == null)
                throw new Exception("Student not found");

            // Update status
            student.Status = "Transferred";
            studentDAL.UpdateStudent(student);

            // Log
            auditLogger.LogAction(transferredBy, "TRANSFER", "Students",
                string.Format("Transferred {0} to {1}. Reason: {2}", student.FullName, transferTo, reason));

            return true;
        }

        /// <summary>
        /// Suspends student
        /// </summary>
        public bool SuspendStudent(int studentId, int suspendedBy, string reason, DateTime? until = null)
        {
            var student = studentDAL.GetStudentById(studentId);
            if (student == null)
                throw new Exception("Student not found");

            student.Status = "Suspended";
            studentDAL.UpdateStudent(student);

            auditLogger.LogAction(suspendedBy, "SUSPEND", "Students",
                string.Format("Suspended {0}. Reason: {1}. Until: {2}", student.FullName, reason, until != null ? until.Value.ToString("yyyy-MM-dd") : "Indefinite"));

            return true;
        }

        /// <summary>
        /// Reactivates suspended student
        /// </summary>
        public bool ReactivateStudent(int studentId, int reactivatedBy)
        {
            var student = studentDAL.GetStudentById(studentId);
            if (student == null)
                throw new Exception("Student not found");

            if (student.Status != "Suspended")
                throw new Exception("Only suspended students can be reactivated");

            student.Status = "Active";
            bool result = studentDAL.UpdateStudent(student);

            if (result)
            {
                auditLogger.LogAction(reactivatedBy, "REACTIVATE", "Students",
                    string.Format("Reactivated {0}", student.FullName));
            }

            return result;
        }

        #endregion

        #region Reports

        /// <summary>
        /// Gets students with outstanding fees
        /// </summary>
        public List<Student> GetStudentsWithOutstandingFees(decimal minBalance = 0)
        {
            var students = studentDAL.GetAllStudents("Active", null);
            return students.FindAll(s => s.Balance > minBalance);
        }

        /// <summary>
        /// Gets students with low attendance
        /// </summary>
        public List<Student> GetStudentsWithLowAttendance(int threshold = 75)
        {
            AttendanceDAL attendanceDAL = new AttendanceDAL();
            var students = studentDAL.GetAllStudents("Active", null);

            List<Student> lowAttendance = new List<Student>();
            foreach (var student in students)
            {
                decimal percentage = attendanceDAL.GetStudentAttendancePercentage(student.StudentID);
                if (percentage < threshold)
                {
                    student.AttendancePercentage = (int)percentage;
                    lowAttendance.Add(student);
                }
            }

            return lowAttendance;
        }

        /// <summary>
        /// Gets enrollment statistics
        /// </summary>
        public DataTable GetEnrollmentStats()
        {
            return studentDAL.GetEnrollmentByClass();
        }

        #endregion

        #region Helper Methods

        private string GenerateAdmissionNo()
        {
            return "ADM-" + DateTime.Now.Year.ToString().Substring(2) + new Random().Next(1000, 9999);
        }

        private int GetCurrentAcademicYearId()
        {
            AcademicDAL academicDAL = new AcademicDAL();
            var year = academicDAL.GetCurrentAcademicYear();
            return year != null ? Convert.ToInt32(year["AcademicYearID"]) : 1;
        }

        private void GenerateInitialInvoice(Student student)
        {
            FinanceDAL financeDAL = new FinanceDAL();
            // Logic to generate initial invoice based on class fee structure
        }

        private DataTable GetStudentResults(int studentId)
        {
            // Get exam results
            return new DataTable(); // Placeholder
        }

        private bool CheckIfPassed(DataTable results)
        {
            // Check if student passed required subjects
            return true; // Placeholder
        }

        #endregion
    }

    /// <summary>
    /// Custom exception for validation errors
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
}