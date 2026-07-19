using System;

namespace AQOONHUB.Models
{
    /// <summary>
    /// Represents the assignment of a teacher to a subject within a class section
    /// </summary>
    public class ClassSubjectTeacher
    {
        private int _classSubjectTeacherID;
        private int _sectionID;
        private string _sectionName;
        private int _subjectID;
        private string _subjectName;
        private int _teacherID;
        private string _teacherName;
        private int _academicYearID;
        private string _academicYearName;
        private int _termID;
        private string _termName;
        private string _status;
        private DateTime _assignedAt;
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public int ClassSubjectTeacherID
        {
            get { return _classSubjectTeacherID; }
            set { _classSubjectTeacherID = value; }
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

        public int AcademicYearID
        {
            get { return _academicYearID; }
            set { _academicYearID = value; }
        }

        public string AcademicYearName
        {
            get { return _academicYearName; }
            set { _academicYearName = value; }
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

        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public DateTime AssignedAt
        {
            get { return _assignedAt; }
            set { _assignedAt = value; }
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
            get { return _sectionName + " - " + _subjectName + " (" + _teacherName + ")"; }
        }

        public string StatusBadgeClass
        {
            get
            {
                switch (_status)
                {
                    case "Active": return "bg-success";
                    case "Inactive": return "bg-secondary";
                    case "Transferred": return "bg-info";
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