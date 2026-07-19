using System;

namespace AQOONHUB.Models
{
    /// <summary>
    /// Represents a school subject
    /// </summary>
    public class Subject
    {
        private int _subjectID;
        private string _subjectName;
        private string _subjectCode;
        private string _description;
        private int _creditHours;
        private string _category;
        private string _status;
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public int SubjectID
        {
            get { return _subjectID; }
            set { _subjectID = value; }
        }

        public string SubjectName
        {
            get { return _subjectName; }
            set { _subjectName = value; }
        }

        public string SubjectCode
        {
            get { return _subjectCode; }
            set { _subjectCode = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public int CreditHours
        {
            get { return _creditHours; }
            set { _creditHours = value; }
        }

        public string Category
        {
            get { return _category; }
            set { _category = value; }
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
            get { return _subjectName + " (" + _subjectCode + ")"; }
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