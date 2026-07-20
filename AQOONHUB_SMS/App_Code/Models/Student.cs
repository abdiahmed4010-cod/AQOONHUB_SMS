using System;

namespace AQOONHUB_SMS.App_Code.Models
{
    public class Student
    {
        private int _studentID;
        private string _studentCode;
        private string _admissionNo;
        private string _firstName;
        private string _lastName;
        private string _gender;
        private DateTime _dateOfBirth;
        private int _guardianID;
        private string _guardianName;
        private int _sectionID;
        private string _className;
        private string _sectionName;
        private int _academicYearID;
        private string _status;
        private string _photoPath;
        private string _medicalNotes;
        private string _address;
        private DateTime _enrollmentDate;
        private DateTime _createdAt;
        private DateTime _updatedAt;
        private decimal _billedAmount;
        private decimal _paidAmount;
        private int _attendancePercentage;
        private decimal _gpa;

        public int StudentID
        {
            get { return _studentID; }
            set { _studentID = value; }
        }

        public string StudentCode
        {
            get { return _studentCode; }
            set { _studentCode = value; }
        }

        public string AdmissionNo
        {
            get { return _admissionNo; }
            set { _admissionNo = value; }
        }

        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }

        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; }
        }

        public string FullName
        {
            get { return _firstName + " " + _lastName; }
        }

        public string Gender
        {
            get { return _gender; }
            set { _gender = value; }
        }

        public DateTime DateOfBirth
        {
            get { return _dateOfBirth; }
            set { _dateOfBirth = value; }
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

        public int SectionID
        {
            get { return _sectionID; }
            set { _sectionID = value; }
        }

        public string ClassName
        {
            get { return _className; }
            set { _className = value; }
        }

        public string SectionName
        {
            get { return _sectionName; }
            set { _sectionName = value; }
        }

        public int AcademicYearID
        {
            get { return _academicYearID; }
            set { _academicYearID = value; }
        }

        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public string PhotoPath
        {
            get { return _photoPath; }
            set { _photoPath = value; }
        }

        public string MedicalNotes
        {
            get { return _medicalNotes; }
            set { _medicalNotes = value; }
        }

        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        public DateTime EnrollmentDate
        {
            get { return _enrollmentDate; }
            set { _enrollmentDate = value; }
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

        public decimal BilledAmount
        {
            get { return _billedAmount; }
            set { _billedAmount = value; }
        }

        public decimal PaidAmount
        {
            get { return _paidAmount; }
            set { _paidAmount = value; }
        }

        public decimal Balance
        {
            get { return _billedAmount - _paidAmount; }
        }

        public int AttendancePercentage
        {
            get { return _attendancePercentage; }
            set { _attendancePercentage = value; }
        }

        public decimal GPA
        {
            get { return _gpa; }
            set { _gpa = value; }
        }

        public int Age
        {
            get
            {
                int age = DateTime.Now.Year - _dateOfBirth.Year;
                if (DateTime.Now.DayOfYear < _dateOfBirth.DayOfYear)
                {
                    age--;
                }
                return age;
            }
        }

        public string StatusBadgeClass
        {
            get
            {
                if (_status == "Active")
                    return "bg-success";
                else if (_status == "Suspended")
                    return "bg-warning";
                else if (_status == "Transferred")
                    return "bg-info";
                else if (_status == "Graduated")
                    return "bg-primary";
                else
                    return "bg-secondary";
            }
        }
    }
}