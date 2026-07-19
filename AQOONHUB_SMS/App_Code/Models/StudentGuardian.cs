using System;

namespace AQOONHUB.Models
{
    /// <summary>
    /// Represents the relationship between a student and their guardian
    /// </summary>
    public class StudentGuardian
    {
        private int _studentGuardianID;
        private int _studentID;
        private string _studentName;
        private int _guardianID;
        private string _guardianName;
        private string _relationship;
        private bool _isPrimary;
        private bool _isEmergencyContact;
        private string _status;
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public int StudentGuardianID
        {
            get { return _studentGuardianID; }
            set { _studentGuardianID = value; }
        }

        public int StudentID
        {
            get { return _studentID; }
            set { _studentID = value; }
        }

        public string StudentName
        {
            get { return _studentName; }
            set { _studentName = value; }
        }

        public int GuardianID
        {
            get { return _guardianID; }
            set { _guardianID = value; }
        }

        public string GuardianName
        {
            get { return _guardianName; }
            set { _guardianName = value; }
        }

        public string Relationship
        {
            get { return _relationship; }
            set { _relationship = value; }
        }

        public bool IsPrimary
        {
            get { return _isPrimary; }
            set { _isPrimary = value; }
        }

        public bool IsEmergencyContact
        {
            get { return _isEmergencyContact; }
            set { _isEmergencyContact = value; }
        }

        public string Status
        {
            get { return _status; }
            set { _status = value; }
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

        public string RelationshipBadgeClass
        {
            get
            {
                switch (_relationship)
                {
                    case "Father": return "bg-primary";
                    case "Mother": return "bg-info";
                    case "Guardian": return "bg-success";
                    case "Sibling": return "bg-warning";
                    case "Other": return "bg-secondary";
                    default: return "bg-secondary";
                }
            }
        }

        public string PrimaryBadgeClass
        {
            get
            {
                if (_isPrimary)
                    return "bg-success";
                else
                    return "bg-secondary";
            }
        }

        public string PrimaryStatusText
        {
            get
            {
                if (_isPrimary)
                    return "Primary";
                else
                    return "Secondary";
            }
        }

        public string StatusBadgeClass
        {
            get
            {
                switch (_status)
                {
                    case "Active": return "bg-success";
                    case "Inactive": return "bg-secondary";
                    default: return "bg-warning";
                }
            }
        }

        public bool IsActive
        {
            get { return _status == "Active"; }
        }
    }
}