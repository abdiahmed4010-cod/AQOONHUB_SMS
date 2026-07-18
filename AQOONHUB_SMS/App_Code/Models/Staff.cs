using System;

namespace AQOONHUB.Models
{
    public class Staff
    {
        private int _staffID;
        private int _userID;
        private string _employeeID;
        private string _fullName;
        private string _email;
        private string _phone;
        private string _department;
        private string _position;
        private DateTime _hireDate;
        private decimal _salary;
        private int _leaveBalance;
        private string _status;
        private bool _userActive;
        private string _passwordHash;

        public int StaffID
        {
            get { return _staffID; }
            set { _staffID = value; }
        }

        public int UserID
        {
            get { return _userID; }
            set { _userID = value; }
        }

        public string EmployeeID
        {
            get { return _employeeID; }
            set { _employeeID = value; }
        }

        public string FullName
        {
            get { return _fullName; }
            set { _fullName = value; }
        }

        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        public string Phone
        {
            get { return _phone; }
            set { _phone = value; }
        }

        public string Department
        {
            get { return _department; }
            set { _department = value; }
        }

        public string Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public string Role
        {
            get
            {
                if (_position != null && _position.Contains("Teacher"))
                    return "Teacher";
                else if (_department == "Finance")
                    return "Accountant";
                else if (_department == "Administration")
                    return "HR";
                else
                    return "Staff";
            }
        }

        public DateTime HireDate
        {
            get { return _hireDate; }
            set { _hireDate = value; }
        }

        public decimal Salary
        {
            get { return _salary; }
            set { _salary = value; }
        }

        public int LeaveBalance
        {
            get { return _leaveBalance; }
            set { _leaveBalance = value; }
        }

        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public bool UserActive
        {
            get { return _userActive; }
            set { _userActive = value; }
        }

        public string PasswordHash
        {
            get { return _passwordHash; }
            set { _passwordHash = value; }
        }

        public int YearsOfService
        {
            get { return DateTime.Now.Year - _hireDate.Year; }
        }

        public decimal MonthlyDeduction
        {
            get { return _salary * 0.05m; }
        }

        public decimal NetSalary
        {
            get { return _salary - MonthlyDeduction; }
        }

        public string StatusBadgeClass
        {
            get
            {
                if (_status == "Active")
                    return "bg-success";
                else if (_status == "On Leave")
                    return "bg-warning";
                else if (_status == "Inactive")
                    return "bg-secondary";
                else if (_status == "Retired")
                    return "bg-dark";
                else
                    return "bg-secondary";
            }
        }
    }
}