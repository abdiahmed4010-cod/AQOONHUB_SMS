using System;

namespace AQOONHUB_SMS.App_Code.Models
{
    public class AttendanceRecord
    {
        private int _attendanceID;
        private int _studentID;
        private string _studentName;
        private string _studentCode;
        private int _sectionID;
        private int? _subjectID;
        private string _subjectName;
        private DateTime _attendanceDate;
        private string _period;
        private string _status;
        private int _markedBy;
        private string _markedByName;
        private string _remarks;

        public int AttendanceID
        {
            get { return _attendanceID; }
            set { _attendanceID = value; }
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

        public string StudentCode
        {
            get { return _studentCode; }
            set { _studentCode = value; }
        }

        public int SectionID
        {
            get { return _sectionID; }
            set { _sectionID = value; }
        }

        public int? SubjectID
        {
            get { return _subjectID; }
            set { _subjectID = value; }
        }

        public string SubjectName
        {
            get { return _subjectName; }
            set { _subjectName = value; }
        }

        public DateTime AttendanceDate
        {
            get { return _attendanceDate; }
            set { _attendanceDate = value; }
        }

        public string Period
        {
            get { return _period; }
            set { _period = value; }
        }

        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public int MarkedBy
        {
            get { return _markedBy; }
            set { _markedBy = value; }
        }

        public string MarkedByName
        {
            get { return _markedByName; }
            set { _markedByName = value; }
        }

        public string Remarks
        {
            get { return _remarks; }
            set { _remarks = value; }
        }

        public string StatusBadgeClass
        {
            get
            {
                if (_status == "Present")
                    return "bg-success";
                else if (_status == "Absent")
                    return "bg-danger";
                else if (_status == "Late")
                    return "bg-warning";
                else if (_status == "Excused")
                    return "bg-info";
                else if (_status == "Holiday")
                    return "bg-dark";
                else
                    return "bg-secondary";
            }
        }
    }
}