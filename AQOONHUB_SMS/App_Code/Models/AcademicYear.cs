using System;

namespace AQOONHUB_SMS.App_Code.Models
{
    /// <summary>
    /// Represents an academic year
    /// </summary>
    public class AcademicYear
    {
        private int _academicYearID;
        private string _yearName;
        private DateTime _startDate;
        private DateTime _endDate;
        private bool _isCurrent;
        private string _status;
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public int AcademicYearID
        {
            get { return _academicYearID; }
            set { _academicYearID = value; }
        }

        public string YearName
        {
            get { return _yearName; }
            set { _yearName = value; }
        }

        public DateTime StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        }

        public DateTime EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
        }

        public bool IsCurrent
        {
            get { return _isCurrent; }
            set { _isCurrent = value; }
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
            get { return _yearName + " (" + _startDate.ToString("yyyy") + " - " + _endDate.ToString("yyyy") + ")"; }
        }

        public string StatusBadgeClass
        {
            get
            {
                switch (_status)
                {
                    case "Active": return "bg-success";
                    case "Closed": return "bg-secondary";
                    case "Upcoming": return "bg-info";
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