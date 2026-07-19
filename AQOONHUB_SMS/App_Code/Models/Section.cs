using System;

namespace AQOONHUB.Models
{
    /// <summary>
    /// Represents a class section
    /// </summary>
    public class Section
    {
        private int _sectionID;
        private string _sectionName;
        private int _classID;
        private string _className;
        private int _teacherID;
        private string _teacherName;
        private int _capacity;
        private int _currentEnrollment;
        private string _roomNumber;
        private string _status;
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public int SectionID
        {
            get { return _sectionID; }
            set { _sectionID = value; }
        }

        public string SectionName
        {
            get { return _sectionName; }
            set { _sectionName = value; }
        }

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

        public int TeacherID
        {
            get { return _teacherID; }
            set { _teacherID = value; }
        }

        public string TeacherName
        {
            get { return _teacherName; }
            set { _teacherName = value; }
        }

        public int Capacity
        {
            get { return _capacity; }
            set { _capacity = value; }
        }

        public int CurrentEnrollment
        {
            get { return _currentEnrollment; }
            set { _currentEnrollment = value; }
        }

        public string RoomNumber
        {
            get { return _roomNumber; }
            set { _roomNumber = value; }
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
            get { return _className + " - " + _sectionName; }
        }

        public int AvailableSeats
        {
            get { return _capacity - _currentEnrollment; }
        }

        public bool IsFull
        {
            get { return _currentEnrollment >= _capacity; }
        }

        public string StatusBadgeClass
        {
            get
            {
                switch (_status)
                {
                    case "Active": return "bg-success";
                    case "Inactive": return "bg-secondary";
                    case "Full": return "bg-danger";
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