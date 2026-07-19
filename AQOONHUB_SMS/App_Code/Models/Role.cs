using System;

namespace AQOONHUB.Models
{
    /// <summary>
    /// Represents a user role in the system
    /// </summary>
    public class Role
    {
        private int _roleID;
        private string _roleName;
        private string _description;
        private bool _isActive;
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public int RoleID
        {
            get { return _roleID; }
            set { _roleID = value; }
        }

        public string RoleName
        {
            get { return _roleName; }
            set { _roleName = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }

        public DateTime CreatedAt
        {
            get { return _createdAt; }
            set { _createdAt = value; }
        }

        public DateTime UpdatedAt
        {
            get { return _updatedAt; }
            set { _updatedAt = value; }
        }

        public string StatusBadgeClass
        {
            get
            {
                if (_isActive)
                    return "bg-success";
                else
                    return "bg-secondary";
            }
        }

        public string StatusText
        {
            get
            {
                if (_isActive)
                    return "Active";
                else
                    return "Inactive";
            }
        }
    }
}