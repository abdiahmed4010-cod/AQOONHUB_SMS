using System;

namespace AQOONHUB.Models
{
    /// <summary>
    /// Represents a class timetable entry
    /// </summary>
    public class Timetable
    {
        private int _timetableID;
        private int _sectionID;
        private string _sectionName;
        private int _subjectID;
        private string _subjectName;
        private int _teacherID;
        private string _teacherName;
        private string _dayOfWeek;
        private TimeSpan _startTime;
        private TimeSpan _endTime;
        private string _roomNumber;
        private int _academicYearID;
        private int _termID;
        private string _status;
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public int TimetableID
        {
            get { return _timetableID; }
            set { _timetableID = value; }
        }

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

        public string DayOfWeek
        {
            get { return _dayOfWeek; }
            set { _dayOfWeek = value; }
        }

        public TimeSpan StartTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }

        public TimeSpan EndTime
        {
            get { return _endTime; }
            set { _endTime = value; }
        }

        public string RoomNumber
        {
            get { return _roomNumber; }
            set { _roomNumber = value; }
        }

        public int AcademicYearID
        {
            get { return _academicYearID; }
            set { _academicYearID = value; }
        }

        public int TermID
        {
            get { return _termID; }
            set { _termID = value; }
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

        public string DisplayTime
        {
            get
            {
                DateTime start = DateTime.Today.Add(_startTime);
                DateTime end = DateTime.Today.Add(_endTime);
                return start.ToString("hh:mm tt") + " - " + end.ToString("hh:mm tt");
            }
        }

        public int DurationMinutes
        {
            get
            {
                return (int)_endTime.Subtract(_startTime).TotalMinutes;
            }
        }

        public string StatusBadgeClass
        {
            get
            {
                switch (_status)
                {
                    case "Active": return "bg-success";
                    case "Cancelled": return "bg-danger";
                    case "Rescheduled": return "bg-info";
                    default: return "bg-secondary";
                }
            }
        }

        public bool IsActive
        {
            get { return _status == "Active"; }
        }
    }
}