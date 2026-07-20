using System;

namespace AQOONHUB_SMS.App_Code.Models
{
    public class ExamResult
    {
        private int _resultID;
        private int _examID;
        private int _studentID;
        private string _studentName;
        private int _subjectID;
        private string _subjectName;
        private decimal _marks;
        private string _remarks;
        private int _enteredBy;

        public int ResultID
        {
            get { return _resultID; }
            set { _resultID = value; }
        }

        public int ExamID
        {
            get { return _examID; }
            set { _examID = value; }
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

        public decimal Marks
        {
            get { return _marks; }
            set { _marks = value; }
        }

        public string Grade
        {
            get
            {
                if (_marks >= 90)
                    return "A";
                else if (_marks >= 80)
                    return "B";
                else if (_marks >= 70)
                    return "C";
                else if (_marks >= 60)
                    return "D";
                else
                    return "F";
            }
        }

        public decimal GPA
        {
            get
            {
                if (_marks >= 90)
                    return 4.0m;
                else if (_marks >= 80)
                    return 3.0m;
                else if (_marks >= 70)
                    return 2.0m;
                else if (_marks >= 60)
                    return 1.0m;
                else
                    return 0.0m;
            }
        }

        public string Remarks
        {
            get { return _remarks; }
            set { _remarks = value; }
        }

        public int EnteredBy
        {
            get { return _enteredBy; }
            set { _enteredBy = value; }
        }

        public string GradeBadgeClass
        {
            get
            {
                if (Grade == "A")
                    return "bg-success";
                else if (Grade == "B")
                    return "bg-primary";
                else if (Grade == "C")
                    return "bg-info";
                else if (Grade == "D")
                    return "bg-warning";
                else if (Grade == "F")
                    return "bg-danger";
                else
                    return "bg-secondary";
            }
        }
    }
}