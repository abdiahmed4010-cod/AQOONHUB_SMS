using System;

namespace AQOONHUB.Models
{
    /// <summary>
    /// Represents a grading scale for academic evaluation
    /// </summary>
    public class GradingScale
    {
        private int _gradingScaleID;
        private string _gradeName;
        private decimal _minScore;
        private decimal _maxScore;
        private decimal _gradePoint;
        private string _description;
        private string _status;
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public int GradingScaleID
        {
            get { return _gradingScaleID; }
            set { _gradingScaleID = value; }
        }

        public string GradeName
        {
            get { return _gradeName; }
            set { _gradeName = value; }
        }

        public decimal MinScore
        {
            get { return _minScore; }
            set { _minScore = value; }
        }

        public decimal MaxScore
        {
            get { return _maxScore; }
            set { _maxScore = value; }
        }

        public decimal GradePoint
        {
            get { return _gradePoint; }
            set { _gradePoint = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
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

        public string ScoreRange
        {
            get { return _minScore.ToString("N1") + "% - " + _maxScore.ToString("N1") + "%"; }
        }

        public string DisplayName
        {
            get { return _gradeName + " (" + _gradePoint.ToString("N1") + " GPA)"; }
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