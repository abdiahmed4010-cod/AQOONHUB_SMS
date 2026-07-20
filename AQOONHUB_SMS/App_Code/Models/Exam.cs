using System;

namespace AQOONHUB_SMS.App_Code.Models
{
    public class Exam
    {
        private int _examID;
        private string _examName;
        private string _examType;
        private int _termID;
        private string _termName;
        private int _academicYearID;
        private string _yearName;
        private DateTime _startDate;
        private DateTime _endDate;
        private string _status;
        private int _createdBy;
        private DateTime _createdAt;

        public int ExamID
        {
            get { return _examID; }
            set { _examID = value; }
        }

        public string ExamName
        {
            get { return _examName; }
            set { _examName = value; }
        }

        public string ExamType
        {
            get { return _examType; }
            set { _examType = value; }
        }

        public int TermID
        {
            get { return _termID; }
            set { _termID = value; }
        }

        public string TermName
        {
            get { return _termName; }
            set { _termName = value; }
        }

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

        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public int CreatedBy
        {
            get { return _createdBy; }
            set { _createdBy = value; }
        }

        public DateTime CreatedAt
        {
            get { return _createdAt; }
            set { _createdAt = value; }
        }

        public string StatusBadgeClass
        {
            get
            {
                if (_status == "Published")
                    return "bg-success";
                else if (_status == "Draft")
                    return "bg-secondary";
                else if (_status == "Closed")
                    return "bg-dark";
                else
                    return "bg-secondary";
            }
        }

        public bool IsUpcoming
        {
            get { return _startDate > DateTime.Now; }
        }

        public bool IsActive
        {
            get { return _startDate <= DateTime.Now && _endDate >= DateTime.Now; }
        }
    }
}