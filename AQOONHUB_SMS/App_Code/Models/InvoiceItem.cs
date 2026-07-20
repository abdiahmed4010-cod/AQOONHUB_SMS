using System;

namespace AQOONHUB_SMS.App_Code.Models
{
    /// <summary>
    /// Represents an item within an invoice
    /// </summary>
    public class InvoiceItem
    {
        private int _invoiceItemID;
        private int _invoiceID;
        private int _feeStructureID;
        private string _itemName;
        private int _quantity;
        private decimal _unitPrice;
        private decimal _totalAmount;

        public int InvoiceItemID
        {
            get { return _invoiceItemID; }
            set { _invoiceItemID = value; }
        }

        public int InvoiceID
        {
            get { return _invoiceID; }
            set { _invoiceID = value; }
        }

        public int FeeStructureID
        {
            get { return _feeStructureID; }
            set { _feeStructureID = value; }
        }

        public string ItemName
        {
            get { return _itemName; }
            set { _itemName = value; }
        }

        public int Quantity
        {
            get { return _quantity; }
            set { _quantity = value; }
        }

        public decimal UnitPrice
        {
            get { return _unitPrice; }
            set { _unitPrice = value; }
        }

        public decimal TotalAmount
        {
            get { return _totalAmount; }
            set { _totalAmount = value; }
        }

        public decimal CalculatedTotal
        {
            get { return _quantity * _unitPrice; }
        }
    }
}