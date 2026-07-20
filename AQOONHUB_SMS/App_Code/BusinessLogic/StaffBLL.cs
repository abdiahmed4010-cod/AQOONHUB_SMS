using System;
using System.Collections.Generic;
using System.Data;
using AQOONHUB_SMS.App_Code.DataAccess;
using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;

namespace AQOONHUB_SMS.App_Code.BusinessLogic
{
    public class StaffBLL
    {
        private StaffDAL staffDAL;
        private AuditLogger auditLogger;

        public StaffBLL()
        {
            staffDAL = new StaffDAL();
            auditLogger = new AuditLogger();
        }

        #region Validation

        private List<string> ValidateStaff(Staff staff, bool isNew)
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(staff.FullName))
                errors.Add("Full name is required");

            if (string.IsNullOrWhiteSpace(staff.Email))
                errors.Add("Email is required");
            else if (!SecurityHelper.IsValidEmail(staff.Email))
                errors.Add("Invalid email format");

            if (string.IsNullOrWhiteSpace(staff.Department))
                errors.Add("Department is required");

            if (string.IsNullOrWhiteSpace(staff.Position))
                errors.Add("Position is required");

            if (staff.HireDate == DateTime.MinValue)
                errors.Add("Hire date is required");

            if (staff.Salary <= 0)
                errors.Add("Salary must be greater than 0");

            if (isNew && string.IsNullOrWhiteSpace(staff.PasswordHash))
                errors.Add("Password is required for new staff");

            return errors;
        }

        #endregion

        #region CRUD Operations

        public List<Staff> GetStaff(string department = null, string status = null, string search = null)
        {
            return staffDAL.GetAllStaff(department, status, search);
        }

        public Staff GetStaff(int staffId)
        {
            if (staffId <= 0)
                throw new ArgumentException("Invalid staff ID");

            return staffDAL.GetStaffById(staffId);
        }

        public Staff GetStaffByUserId(int userId)
        {
            return staffDAL.GetStaffByUserId(userId);
        }

        /// <summary>
        /// Creates new staff with user account
        /// </summary>
        public Staff CreateStaff(Staff staff, int createdBy, string ipAddress)
        {
            var errors = ValidateStaff(staff, true);
            if (errors.Count > 0)
                throw new ValidationException(string.Join(", ", errors));

            // Generate employee ID
            staff.EmployeeID = staffDAL.GenerateEmployeeId();

            // Hash password if provided
            if (!string.IsNullOrEmpty(staff.PasswordHash))
            {
                staff.PasswordHash = SecurityHelper.HashPassword(staff.PasswordHash);
            }

            // Set defaults
            staff.Status = "Active";
            staff.LeaveBalance = 18; // Default annual leave

            // Save
            int newId = staffDAL.AddStaff(staff);
            staff.StaffID = newId;

            // Log
            auditLogger.LogCreate(createdBy, "Staff", "Staff Member", staff.EmployeeID,
                string.Format("Added {0} as {1} in {2}", staff.FullName, staff.Position, staff.Department));

            return staff;
        }

        public bool UpdateStaff(Staff staff, int updatedBy, string ipAddress)
        {
            var errors = ValidateStaff(staff, false);
            if (errors.Count > 0)
                throw new ValidationException(string.Join(", ", errors));

            var oldStaff = staffDAL.GetStaffById(staff.StaffID);
            if (oldStaff == null)
                throw new Exception("Staff not found");

            bool result = staffDAL.UpdateStaff(staff);

            if (result)
            {
                // Log significant changes
                if (oldStaff.Salary != staff.Salary)
                {
                    auditLogger.LogUpdate(updatedBy, "Staff", "Staff", staff.EmployeeID,
                        "Salary", oldStaff.Salary.ToString("C"), staff.Salary.ToString("C"));
                }
                if (oldStaff.Status != staff.Status)
                {
                    auditLogger.LogUpdate(updatedBy, "Staff", "Staff", staff.EmployeeID,
                        "Status", oldStaff.Status, staff.Status);
                }
            }

            return result;
        }

        public bool DeleteStaff(int staffId, int deletedBy, string reason)
        {
            var staff = staffDAL.GetStaffById(staffId);
            if (staff == null)
                throw new Exception("Staff not found");

            // Check if staff has active classes
            if (HasActiveClasses(staffId))
                throw new Exception("Cannot delete staff with active class assignments");

            bool result = staffDAL.SoftDeleteStaff(staffId);

            if (result)
            {
                auditLogger.LogDelete(deletedBy, "Staff", "Staff Member", staff.EmployeeID, reason);
            }

            return result;
        }

        #endregion

        #region Leave Management

        /// <summary>
        /// Requests leave with validation
        /// </summary>
        public int RequestLeave(int staffId, string leaveType, DateTime startDate, DateTime endDate,
            string reason, int requestedBy)
        {
            // Validate dates
            if (startDate > endDate)
                throw new ValidationException("Start date cannot be after end date");

            if (startDate < DateTime.Now.Date)
                throw new ValidationException("Cannot request leave for past dates");

            int days = (endDate - startDate).Days + 1;

            // Check balance
            var staff = staffDAL.GetStaffById(staffId);
            if (staff == null)
                throw new Exception("Staff not found");

            if (leaveType == "Annual" && days > staff.LeaveBalance)
                throw new ValidationException(string.Format("Insufficient leave balance. Available: {0} days, Requested: {1} days", staff.LeaveBalance, days));

            // Create request
            int leaveId = staffDAL.RequestLeave(staffId, leaveType, startDate, endDate, reason);

            auditLogger.LogAction(requestedBy, "LEAVE_REQUEST", "HR",
                string.Format("{0} requested {1} leave: {2:yyyy-MM-dd} to {3:yyyy-MM-dd} ({4} days)", staff.FullName, leaveType, startDate, endDate, days));

            return leaveId;
        }

        /// <summary>
        /// Approves or rejects leave request
        /// </summary>
        public bool ProcessLeaveRequest(int leaveId, int approvedBy, bool approve, string notes)
        {
            var leaveRequests = staffDAL.GetLeaveRequests();
            DataRow leaveRow = null;

            foreach (DataRow row in leaveRequests.Rows)
            {
                if (Convert.ToInt32(row["LeaveID"]) == leaveId)
                {
                    leaveRow = row;
                    break;
                }
            }

            if (leaveRow == null)
                throw new Exception("Leave request not found");

            string staffName = leaveRow["StaffName"].ToString();
            string leaveType = leaveRow["LeaveType"].ToString();

            bool result = staffDAL.ApproveLeave(leaveId, approvedBy, approve, notes);

            if (result)
            {
                string action = approve ? "APPROVED" : "REJECTED";
                auditLogger.LogAction(approvedBy, string.Format("LEAVE_{0}", action), "HR",
                    string.Format("{0} {1} leave for {2}. Notes: {3}", action, leaveType, staffName, notes));
            }

            return result;
        }

        #endregion

        #region Payroll

        /// <summary>
        /// Gets payroll with calculations
        /// </summary>
        public DataTable GetPayroll(int? month = null, int? year = null)
        {
            return staffDAL.GetPayroll(null, month, year);
        }

        /// <summary>
        /// Processes payroll for a period
        /// </summary>
        public bool ProcessPayroll(int month, int year, int processedBy)
        {
            var payroll = staffDAL.GetPayroll(null, month, year);

            // Business rules for payroll processing
            foreach (DataRow row in payroll.Rows)
            {
                int staffId = Convert.ToInt32(row["StaffID"]);
                decimal gross = Convert.ToDecimal(row["GrossSalary"]);

                // Calculate deductions
                decimal tax = gross * 0.05m; // 5% tax
                decimal net = gross - tax;

                // Save payroll record
                // ... payroll record logic
            }

            auditLogger.LogAction(processedBy, "PAYROLL_RUN", "HR",
                string.Format("Processed payroll for {0}/{1}. {2} staff members.", month, year, payroll.Rows.Count));

            return true;
        }

        #endregion

        #region Helper Methods

        private bool HasActiveClasses(int staffId)
        {
            AcademicDAL academicDAL = new AcademicDAL();
            var assignments = academicDAL.GetClassSubjectTeachers(null, GetCurrentAcademicYearId());

            foreach (DataRow row in assignments.Rows)
            {
                if (Convert.ToInt32(row["StaffID"]) == staffId)
                    return true;
            }

            return false;
        }

        private int GetCurrentAcademicYearId()
        {
            AcademicDAL academicDAL = new AcademicDAL();
            var year = academicDAL.GetCurrentAcademicYear();
            return year != null ? Convert.ToInt32(year["AcademicYearID"]) : 1;
        }

        #endregion
    }
}