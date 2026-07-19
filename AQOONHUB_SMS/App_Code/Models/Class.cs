using System;

namespace AQOONHUB.Models
{
    /// <summary>
    /// Represents a school class level
    /// </summary>
    public class Class
    {
        private int _classID;
        private string _className;
        private string _classCode;
        private int _gradeLevel;
        private string _description;
        private string _status;
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public int ClassID
        {
            get { return _classID; }
            set { _classID = value; }
        }

        public string ClassName
        {
            get { return _className; }
            set { _className = value; }
        }

        public string ClassCode
        {
            get { return _classCode; }
            set { _classCode = value; }
        }

        public int GradeLevel
        {
            get { return _gradeLevel; }
            set { _gradeLevel = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
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

        public string DisplayName
        {
            get { return _className + " (" + _classCode + ")"; }
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