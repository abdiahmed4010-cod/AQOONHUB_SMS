using System;

namespace AQOONHUB_SMS.App_Code.Models
{
    /// <summary>
    /// Represents a school event or announcement
    /// </summary>
    public class Event
    {
        private int _eventID;
        private string _title;
        private string _description;
        private DateTime _eventDate;
        private DateTime _startTime;
        private DateTime _endTime;
        private string _location;
        private string _eventType;
        private string _targetAudience;
        private int _createdBy;
        private string _createdByName;
        private string _status;
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public int EventID
        {
            get { return _eventID; }
            set { _eventID = value; }
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public DateTime EventDate
        {
            get { return _eventDate; }
            set { _eventDate = value; }
        }

        public DateTime StartTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }

        public DateTime EndTime
        {
            get { return _endTime; }
            set { _endTime = value; }
        }

        public string Location
        {
            get { return _location; }
            set { _location = value; }
        }

        public string EventType
        {
            get { return _eventType; }
            set { _eventType = value; }
        }

        public string TargetAudience
        {
            get { return _targetAudience; }
            set { _targetAudience = value; }
        }

        public int CreatedBy
        {
            get { return _createdBy; }
            set { _createdBy = value; }
        }

        public string CreatedByName
        {
            get { return _createdByName; }
            set { _createdByName = value; }
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

        public string DisplayDate
        {
            get { return _eventDate.ToString("dddd, MMMM dd, yyyy"); }
        }

        public string DisplayTime
        {
            get
            {
                return _startTime.ToString("hh:mm tt") + " - " + _endTime.ToString("hh:mm tt");
            }
        }

        public bool IsUpcoming
        {
            get { return _eventDate.Date >= DateTime.Now.Date; }
        }

        public bool IsPast
        {
            get { return _eventDate.Date < DateTime.Now.Date; }
        }

        public string StatusBadgeClass
        {
            get
            {
                switch (_status)
                {
                    case "Active": return "bg-success";
                    case "Cancelled": return "bg-danger";
                    case "Postponed": return "bg-warning";
                    case "Completed": return "bg-secondary";
                    default: return "bg-info";
                }
            }
        }
    }
}