using System;

namespace AQOONHUB_SMS.App_Code.Models
{
    /// <summary>
    /// Represents a role-permission assignment
    /// </summary>
    public class RolePermission
    {
        private int _rolePermissionID;
        private int _roleID;
        private int _permissionID;
        private int _assignedBy;
        private DateTime _assignedAt;

        public int RolePermissionID
        {
            get { return _rolePermissionID; }
            set { _rolePermissionID = value; }
        }

        public int RoleID
        {
            get { return _roleID; }
            set { _roleID = value; }
        }

        public int PermissionID
        {
            get { return _permissionID; }
            set { _permissionID = value; }
        }

        public int AssignedBy
        {
            get { return _assignedBy; }
            set { _assignedBy = value; }
        }

        public DateTime AssignedAt
        {
            get { return _assignedAt; }
            set { _assignedAt = value; }
        }
    }
}