using System;

namespace AQOONHUB_SMS.App_Code.Models
{
    public class Payment
    {
        private int _paymentID;
        private string _receiptNo;
        private int _invoiceID;
        private string _invoiceNo;
        private int _studentID;
        private string _studentName;
        private decimal _amount;
        private string _paymentMethod;
        private DateTime _paymentDate;
        private int _receivedBy;
        private string _receivedByName;
        private string _notes;
        private DateTime _createdAt;

        public int PaymentID
        {
            get { return _paymentID; }
            set { _paymentID = value; }
        }

        public string ReceiptNo
        {
            get { return _receiptNo; }
            set { _receiptNo = value; }
        }

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

        public decimal Amount
        {
            get { return _amount; }
            set { _amount = value; }
        }

        public string PaymentMethod
        {
            get { return _paymentMethod; }
            set { _paymentMethod = value; }
        }

        public DateTime PaymentDate
        {
            get { return _paymentDate; }
            set { _paymentDate = value; }
        }

        public int ReceivedBy
        {
            get { return _receivedBy; }
            set { _receivedBy = value; }
        }

        public string ReceivedByName
        {
            get { return _receivedByName; }
            set { _receivedByName = value; }
        }

        public string Notes
        {
            get { return _notes; }
            set { _notes = value; }
        }

        public DateTime CreatedAt
        {
            get { return _createdAt; }
            set { _createdAt = value; }
        }

        public string MethodBadgeClass
        {
            get
            {
                if (_paymentMethod == "Zaad")
                    return "bg-success";
                else if (_paymentMethod == "eDahab")
                    return "bg-info";
                else if (_paymentMethod == "Cash")
                    return "bg-warning";
                else if (_paymentMethod == "Bank Transfer")
                    return "bg-primary";
                else
                    return "bg-secondary";
            }
        }
    }
}