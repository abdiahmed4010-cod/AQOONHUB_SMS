using System;

namespace AQOONHUB.Models
{
    /// <summary>
    /// Represents a user-role assignment
    /// </summary>
    public class UserRole
    {
        private int _userRoleID;
        private int _userID;
        private int _roleID;
        private int _assignedBy;
        private DateTime _assignedAt;

        public int UserRoleID
        {
            get { return _userRoleID; }
            set { _userRoleID = value; }
        }

        public int UserID
        {
            get { return _userID; }
            set { _userID = value; }
        }

        public int RoleID
        {
            get { return _roleID; }
            set { _roleID = value; }
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