using System;

namespace AQOONHUB.Models
{
    public class User
    {
        private int _userID;
        private string _fullName;
        private string _email;
        private string _passwordHash;
        private string _phone;
        private string _role;
        private bool _isActive;
        private DateTime? _lastLogin;
        private DateTime _createdAt;

        public int UserID
        {
            get { return _userID; }
            set { _userID = value; }
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

        public string PasswordHash
        {
            get { return _passwordHash; }
            set { _passwordHash = value; }
        }

        public string Phone
        {
            get { return _phone; }
            set { _phone = value; }
        }

        public string Role
        {
            get { return _role; }
            set { _role = value; }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }

        public DateTime? LastLogin
        {
            get { return _lastLogin; }
            set { _lastLogin = value; }
        }

        public DateTime CreatedAt
        {
            get { return _createdAt; }
            set { _createdAt = value; }
        }

        public string RoleBadgeClass
        {
            get
            {
                if (_role == "SuperAdmin")
                    return "bg-danger";
                else if (_role == "Admin")
                    return "bg-warning";
                else if (_role == "Teacher")
                    return "bg-success";
                else if (_role == "Accountant")
                    return "bg-info";
                else if (_role == "HR")
                    return "bg-primary";
                else if (_role == "Parent")
                    return "bg-secondary";
                else
                    return "bg-secondary";
            }
        }

        public string Initials
        {
            get
            {
                string[] parts = _fullName.Split(' ');
                string initials = "";
                foreach (string part in parts)
                {
                    if (!string.IsNullOrEmpty(part))
                        initials += part[0];
                }
                return initials.ToUpper();
            }
        }
    }
}