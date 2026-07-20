using System;

namespace AQOONHUB_SMS.App_Code.Models
{
    public class Invoice
    {
        private int _invoiceID;
        private string _invoiceNo;
        private int _studentID;
        private string _studentName;
        private string _studentCode;
        private string _className;
        private string _sectionName;
        private int _academicYearID;
        private int _termID;
        private decimal _totalAmount;
        private decimal _paidAmount;
        private DateTime _dueDate;
        private string _status;
        private DateTime _generatedAt;
        private int _generatedBy;

        public int InvoiceID
        {
            get { return _invoiceID; }
            set { _invoiceID = value; }
        }

        public string InvoiceNo
        {
            get { return _invoiceNo; }
            set { _invoiceNo = value; }
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

        public int TermID
        {
            get { return _termID; }
            set { _termID = value; }
        }

        public decimal TotalAmount
        {
            get { return _totalAmount; }
            set { _totalAmount = value; }
        }

        public decimal PaidAmount
        {
            get { return _paidAmount; }
            set { _paidAmount = value; }
        }

        public decimal Balance
        {
            get { return _totalAmount - _paidAmount; }
        }

        public DateTime DueDate
        {
            get { return _dueDate; }
            set { _dueDate = value; }
        }

        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public DateTime GeneratedAt
        {
            get { return _generatedAt; }
            set { _generatedAt = value; }
        }

        public int GeneratedBy
        {
            get { return _generatedBy; }
            set { _generatedBy = value; }
        }

        public bool IsOverdue
        {
            get { return _dueDate < DateTime.Now && Balance > 0; }
        }

        public int DaysOverdue
        {
            get
            {
                if (IsOverdue)
                    return (DateTime.Now - _dueDate).Days;
                else
                    return 0;
            }
        }

        public string StatusBadgeClass
        {
            get
            {
                if (_status == "Paid")
                    return "bg-success";
                else if (_status == "Partially Paid")
                    return "bg-warning";
                else if (_status == "Pending")
                    return "bg-info";
                else if (_status == "Overdue")
                    return "bg-danger";
                else if (_status == "Void")
                    return "bg-dark";
                else
                    return "bg-secondary";
            }
        }

        public string PriorityLevel
        {
            get
            {
                if (DaysOverdue > 90)
                    return "Critical";
                else if (DaysOverdue > 30)
                    return "High";
                else if (DaysOverdue > 0)
                    return "Medium";
                else
                    return "Current";
            }
        }
    }
}