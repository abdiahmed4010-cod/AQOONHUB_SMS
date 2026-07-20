using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;


namespace AQOONHUB_SMS.App_Code.DataAccess
{
    public class StaffDAL
    {
        private DatabaseHelper db;

        public StaffDAL()
        {
            db = new DatabaseHelper();
        }

        public List<Staff> GetAllStaff(string department, string status, string search)
        {
            List<Staff> staffList = new List<Staff>();

            string query = @"
                SELECT s.*, u.FullName, u.Email, u.Phone, u.IsActive as UserActive
                FROM Staff s
                INNER JOIN Users u ON s.UserID = u.UserID
                WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(department))
            {
                query += " AND s.Department = @Department";
                parameters.Add(new SqlParameter("@Department", department));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query += " AND s.Status = @Status";
                parameters.Add(new SqlParameter("@Status", status));
            }

            if (!string.IsNullOrEmpty(search))
            {
                query += @" AND (u.FullName LIKE @Search OR s.EmployeeID LIKE @Search 
                          OR s.Position LIKE @Search OR u.Email LIKE @Search)";
                parameters.Add(new SqlParameter("@Search", "%" + search + "%"));
            }

            query += " ORDER BY s.HireDate DESC";

            DataTable dt = db.ExecuteQuery(query, parameters.ToArray());

            foreach (DataRow row in dt.Rows)
            {
                staffList.Add(MapToStaff(row));
            }

            return staffList;
        }

        public List<Staff> GetAllStaff()
        {
            return GetAllStaff(null, null, null);
        }

        public Staff GetStaffById(int staffId)
        {
            string query = @"
                SELECT s.*, u.FullName, u.Email, u.Phone, u.IsActive as UserActive
                FROM Staff s
                INNER JOIN Users u ON s.UserID = u.UserID
                WHERE s.StaffID = @StaffID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@StaffID", staffId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
                return MapToStaff(dt.Rows[0]);

            return null;
        }

        public Staff GetStaffByUserId(int userId)
        {
            string query = @"
                SELECT s.*, u.FullName, u.Email, u.Phone, u.IsActive as UserActive
                FROM Staff s
                INNER JOIN Users u ON s.UserID = u.UserID
                WHERE s.UserID = @UserID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
                return MapToStaff(dt.Rows[0]);

            return null;
        }

        public int AddStaff(Staff staff)
        {
            string userQuery = @"
                INSERT INTO Users (FullName, Email, PasswordHash, Phone, Role, IsActive)
                VALUES (@FullName, @Email, @PasswordHash, @Phone, @Role, 1);
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] userParams = new SqlParameter[]
            {
                new SqlParameter("@FullName", staff.FullName),
                new SqlParameter("@Email", staff.Email),
                new SqlParameter("@PasswordHash", staff.PasswordHash),
                new SqlParameter("@Phone", staff.Phone),
                new SqlParameter("@Role", staff.Role)
            };

            int userId = Convert.ToInt32(db.ExecuteScalar(userQuery, userParams));

            string staffQuery = @"
                INSERT INTO Staff (UserID, EmployeeID, Department, Position, HireDate, Salary, LeaveBalance, Status)
                VALUES (@UserID, @EmployeeID, @Department, @Position, @HireDate, @Salary, @LeaveBalance, @Status);
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] staffParams = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@EmployeeID", staff.EmployeeID),
                new SqlParameter("@Department", staff.Department),
                new SqlParameter("@Position", staff.Position),
                new SqlParameter("@HireDate", staff.HireDate),
                new SqlParameter("@Salary", staff.Salary),
                new SqlParameter("@LeaveBalance", staff.LeaveBalance),
                new SqlParameter("@Status", staff.Status)
            };

            return Convert.ToInt32(db.ExecuteScalar(staffQuery, staffParams));
        }

        public bool UpdateStaff(Staff staff)
        {
            string userQuery = @"
                UPDATE Users SET
                    FullName = @FullName,
                    Email = @Email,
                    Phone = @Phone,
                    IsActive = @IsActive
                WHERE UserID = @UserID";

            SqlParameter[] userParams = new SqlParameter[]
            {
                new SqlParameter("@UserID", staff.UserID),
                new SqlParameter("@FullName", staff.FullName),
                new SqlParameter("@Email", staff.Email),
                new SqlParameter("@Phone", staff.Phone),
                new SqlParameter("@IsActive", staff.UserActive)
            };

            db.ExecuteNonQuery(userQuery, userParams);

            string staffQuery = @"
                UPDATE Staff SET
                    Department = @Department,
                    Position = @Position,
                    HireDate = @HireDate,
                    Salary = @Salary,
                    LeaveBalance = @LeaveBalance,
                    Status = @Status
                WHERE StaffID = @StaffID";

            SqlParameter[] staffParams = new SqlParameter[]
            {
                new SqlParameter("@StaffID", staff.StaffID),
                new SqlParameter("@Department", staff.Department),
                new SqlParameter("@Position", staff.Position),
                new SqlParameter("@HireDate", staff.HireDate),
                new SqlParameter("@Salary", staff.Salary),
                new SqlParameter("@LeaveBalance", staff.LeaveBalance),
                new SqlParameter("@Status", staff.Status)
            };

            return db.ExecuteNonQuery(staffQuery, staffParams) > 0;
        }

        public bool SoftDeleteStaff(int staffId)
        {
            string query = @"
                UPDATE Staff SET Status = 'Inactive' WHERE StaffID = @StaffID;
                UPDATE Users SET IsActive = 0 WHERE UserID = (SELECT UserID FROM Staff WHERE StaffID = @StaffID);";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@StaffID", staffId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        public DataTable GetLeaveRequests(string status, int? staffId)
        {
            string query = @"
                SELECT lr.*, s.EmployeeID, u.FullName as StaffName
                FROM LeaveRequests lr
                INNER JOIN Staff s ON lr.StaffID = s.StaffID
                INNER JOIN Users u ON s.UserID = u.UserID
                WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(status))
            {
                query += " AND lr.Status = @Status";
                parameters.Add(new SqlParameter("@Status", status));
            }

            if (staffId.HasValue)
            {
                query += " AND lr.StaffID = @StaffID";
                parameters.Add(new SqlParameter("@StaffID", staffId.Value));
            }

            query += " ORDER BY lr.CreatedAt DESC";

            return db.ExecuteQuery(query, parameters.ToArray());
        }

        public DataTable GetLeaveRequests()
        {
            return GetLeaveRequests(null, null);
        }

        public int RequestLeave(int staffId, string leaveType, DateTime startDate, DateTime endDate, string reason)
        {
            int days = (endDate - startDate).Days + 1;

            string query = @"
                INSERT INTO LeaveRequests (StaffID, LeaveType, StartDate, EndDate, Days, Reason, Status, CreatedAt)
                VALUES (@StaffID, @LeaveType, @StartDate, @EndDate, @Days, @Reason, 'Pending', GETDATE());
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@StaffID", staffId),
                new SqlParameter("@LeaveType", leaveType),
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate),
                new SqlParameter("@Days", days),
                new SqlParameter("@Reason", string.IsNullOrEmpty(reason) ? (object)DBNull.Value : reason)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        public bool ApproveLeave(int leaveId, int approvedBy, bool approve, string notes)
        {
            string query = @"
                UPDATE LeaveRequests SET
                    Status = @Status,
                    ApprovedBy = @ApprovedBy,
                    ApprovedAt = GETDATE()
                WHERE LeaveID = @LeaveID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@LeaveID", leaveId),
                new SqlParameter("@Status", approve ? "Approved" : "Rejected"),
                new SqlParameter("@ApprovedBy", approvedBy),
                new SqlParameter("@Notes", string.IsNullOrEmpty(notes) ? (object)DBNull.Value : notes)
            };

            bool result = db.ExecuteNonQuery(query, parameters) > 0;

            if (approve && result)
            {
                string deductQuery = @"
                    UPDATE Staff SET LeaveBalance = LeaveBalance - 
                        (SELECT Days FROM LeaveRequests WHERE LeaveID = @LeaveID)
                    WHERE StaffID = (SELECT StaffID FROM LeaveRequests WHERE LeaveID = @LeaveID);";

                SqlParameter[] deductParams = new SqlParameter[]
                {
                    new SqlParameter("@LeaveID", leaveId)
                };

                db.ExecuteNonQuery(deductQuery, deductParams);
            }

            return result;
        }

        public DataTable GetPayroll(int? staffId, int? month, int? year)
        {
            string query = @"
                SELECT s.StaffID, s.EmployeeID, s.Salary as GrossSalary,
                    s.Salary * 0.05 as Deductions,
                    s.Salary * 0.95 as NetSalary,
                    s.Status,
                    u.FullName, u.Email, s.Department
                FROM Staff s
                INNER JOIN Users u ON s.UserID = u.UserID
                WHERE s.Status = 'Active'";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (staffId.HasValue)
            {
                query += " AND s.StaffID = @StaffID";
                parameters.Add(new SqlParameter("@StaffID", staffId.Value));
            }

            query += " ORDER BY s.Department, u.FullName";

            return db.ExecuteQuery(query, parameters.ToArray());
        }

        public string GenerateEmployeeId()
        {
            string query = @"
                SELECT TOP 1 EmployeeID 
                FROM Staff 
                ORDER BY EmployeeID DESC";

            DataTable dt = db.ExecuteQuery(query);

            if (dt.Rows.Count > 0)
            {
                string lastId = dt.Rows[0]["EmployeeID"].ToString();
                int lastNum = int.Parse(lastId.Replace("EMP-", ""));
                return "EMP-" + (lastNum + 1).ToString("D4");
            }

            return "EMP-0001";
        }

        private Staff MapToStaff(DataRow row)
        {
            Staff staff = new Staff();
            staff.StaffID = Convert.ToInt32(row["StaffID"]);
            staff.UserID = Convert.ToInt32(row["UserID"]);
            staff.EmployeeID = row["EmployeeID"].ToString();
            staff.FullName = row["FullName"].ToString();
            staff.Email = row["Email"].ToString();
            staff.Phone = row["Phone"] == DBNull.Value ? null : row["Phone"].ToString();
            staff.Department = row["Department"].ToString();
            staff.Position = row["Position"].ToString();
            staff.HireDate = Convert.ToDateTime(row["HireDate"]);
            staff.Salary = Convert.ToDecimal(row["Salary"]);
            staff.LeaveBalance = Convert.ToInt32(row["LeaveBalance"]);
            staff.Status = row["Status"].ToString();
            staff.UserActive = Convert.ToBoolean(row["UserActive"]);
            return staff;
        }
    }
}