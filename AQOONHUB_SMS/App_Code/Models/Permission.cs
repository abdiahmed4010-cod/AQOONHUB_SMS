using System;

namespace AQOONHUB_SMS.App_Code.Models
{
    /// <summary>
    /// Represents a system permission
    /// </summary>
    public class Permission
    {
        private int _permissionID;
        private string _permissionName;
        private string _module;
        private string _description;
        private bool _isActive;
        private DateTime _createdAt;

        public int PermissionID
        {
            get { return _permissionID; }
            set { _permissionID = value; }
        }

        public string PermissionName
        {
            get { return _permissionName; }
            set { _permissionName = value; }
        }

        public string Module
        {
            get { return _module; }
            set { _module = value; }
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

        public string DisplayName
        {
            get { return _module + " - " + _permissionName; }
        }
    }
}