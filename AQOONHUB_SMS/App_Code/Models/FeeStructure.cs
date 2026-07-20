using System;

namespace AQOONHUB_SMS.App_Code.Models
{
    /// <summary>
    /// Represents a fee structure for a class within an academic year
    /// </summary>
    public class FeeStructure
    {
        private int _feeStructureID;
        private int _academicYearID;
        private int _classID;
        private string _feeName;
        private decimal _amount;
        private string _description;
        private DateTime _dueDate;
        private bool _isActive;
        private DateTime _createdAt;

        public int FeeStructureID
        {
            get { return _feeStructureID; }
            set { _feeStructureID = value; }
        }

        public int AcademicYearID
        {
            get { return _academicYearID; }
            set { _academicYearID = value; }
        }

        public int ClassID
        {
            get { return _classID; }
            set { _classID = value; }
        }

        public string FeeName
        {
            get { return _feeName; }
            set { _feeName = value; }
        }

        public decimal Amount
        {
            get { return _amount; }
            set { _amount = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public DateTime DueDate
        {
            get { return _dueDate; }
            set { _dueDate = value; }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
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
                if (_isActive)
                    return "bg-success";
                else
                    return "bg-secondary";
            }
        }

        public string StatusText
        {
            get
            {
                if (_isActive)
                    return "Active";
                else
                    return "Inactive";
            }
        }

        public string DisplayName
        {
            get { return _feeName + " - " + _amount.ToString("N2"); }
        }
    }
}