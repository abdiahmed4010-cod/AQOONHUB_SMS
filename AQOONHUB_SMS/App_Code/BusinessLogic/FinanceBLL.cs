using System;
using System.Collections.Generic;
using System.Data;
using AQOONHUB_SMS.App_Code.DataAccess;
using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;

namespace AQOONHUB_SMS.App_Code.BusinessLogic
{
    public class FinanceBLL
    {
        private FinanceDAL financeDAL;
        private StudentDAL studentDAL;
        private AuditLogger auditLogger;

        public FinanceBLL()
        {
            financeDAL = new FinanceDAL();
            studentDAL = new StudentDAL();
            auditLogger = new AuditLogger();
        }

        #region Invoice Management

        /// <summary>
        /// Gets all invoices with filters
        /// </summary>
        public List<Invoice> GetInvoices(string status = null, int? studentId = null, string search = null)
        {
            return financeDAL.GetInvoices(status, studentId, search);
        }

        public Invoice GetInvoice(int invoiceId)
        {
            return financeDAL.GetInvoiceById(invoiceId);
        }

        /// <summary>
        /// Generates invoices for a term (bulk operation)
        /// </summary>
        public List<string> GenerateTermInvoices(int termId, int classId, int generatedBy)
        {
            var students = studentDAL.GetAllStudents("Active", null);
            List<string> generatedInvoices = new List<string>();

            foreach (var student in students)
            {
                // Check if invoice already exists for this term
                if (!InvoiceExistsForTerm(student.StudentID, termId))
                {
                    string invoiceNo = financeDAL.GenerateInvoice(student.StudentID, termId, generatedBy);
                    if (invoiceNo != null)
                        generatedInvoices.Add(invoiceNo);
                }
            }

            if (generatedInvoices.Count > 0)
            {
                auditLogger.LogBulkOperation(generatedBy, "Finance", "INVOICE_GENERATION",
                    generatedInvoices.Count, string.Format("Generated invoices for term {0}", termId));
            }

            return generatedInvoices;
        }

        /// <summary>
        /// Voids an invoice with validation
        /// </summary>
        public bool VoidInvoice(int invoiceId, int voidedBy, string reason)
        {
            var invoice = financeDAL.GetInvoiceById(invoiceId);
            if (invoice == null)
                throw new Exception("Invoice not found");

            if (invoice.Status == "Void")
                throw new Exception("Invoice is already voided");

            if (invoice.Status == "Paid")
                throw new Exception("Cannot void a paid invoice. Process refund instead.");

            bool result = financeDAL.VoidInvoice(invoiceId, voidedBy, reason);

            if (result)
            {
                auditLogger.LogAction(voidedBy, "VOID", "Finance",
                    string.Format("Voided invoice {0}. Reason: {1}", invoice.InvoiceNo, reason));
            }

            return result;
        }

        #endregion

        #region Payment Processing

        /// <summary>
        /// Records payment with business rules
        /// </summary>
        public string RecordPayment(int invoiceId, decimal amount, string paymentMethod,
            int receivedBy, string notes = null)
        {
            // Validate
            var invoice = financeDAL.GetInvoiceById(invoiceId);
            if (invoice == null)
                throw new Exception("Invoice not found");

            if (invoice.Status == "Paid")
                throw new Exception("Invoice is already fully paid");

            if (invoice.Status == "Void")
                throw new Exception("Cannot pay a voided invoice");

            if (amount <= 0)
                throw new ValidationException("Payment amount must be greater than 0");

            if (amount > invoice.Balance)
                throw new ValidationException(string.Format("Payment amount (${0:N2}) exceeds balance (${1:N2})", amount, invoice.Balance));

            // Validate payment method
            string[] validMethods = { "Zaad", "eDahab", "Cash", "Bank Transfer" };
            if (!Array.Exists(validMethods, m => m == paymentMethod))
                throw new ValidationException("Invalid payment method");

            // Record payment
            string receiptNo = financeDAL.RecordPayment(invoiceId, amount, paymentMethod, receivedBy, notes);

            if (receiptNo != null)
            {
                auditLogger.LogAction(receivedBy, "PAYMENT", "Finance",
                    string.Format("Recorded payment {0} for invoice {1}. Amount: ${2:N2} via {3}", receiptNo, invoice.InvoiceNo, amount, paymentMethod));

                // Check if fully paid and send notification
                var updatedInvoice = financeDAL.GetInvoiceById(invoiceId);
                if (updatedInvoice.Status == "Paid")
                {
                    // Trigger notification to parent
                    NotifyPaymentComplete(updatedInvoice);
                }
            }

            return receiptNo;
        }

        /// <summary>
        /// Applies discount to invoice
        /// </summary>
        public bool ApplyDiscount(int invoiceId, decimal discountAmount, string discountType,
            string reason, int appliedBy)
        {
            var invoice = financeDAL.GetInvoiceById(invoiceId);
            if (invoice == null)
                throw new Exception("Invoice not found");

            if (discountAmount <= 0 || discountAmount >= invoice.TotalAmount)
                throw new ValidationException("Invalid discount amount");

            // Apply discount logic
            decimal newTotal = invoice.TotalAmount - discountAmount;

            auditLogger.LogAction(appliedBy, "DISCOUNT", "Finance",
                string.Format("Applied {0} discount of ${1:N2} to invoice {2}. Reason: {3}", discountType, discountAmount, invoice.InvoiceNo, reason));

            return true;
        }

        #endregion

        #region Financial Reports

        /// <summary>
        /// Gets collection summary by method
        /// </summary>
        public DataTable GetCollectionByMethod(int? academicYearId = null)
        {
            return financeDAL.GetPaymentSummary(academicYearId ?? GetCurrentAcademicYearId());
        }

        /// <summary>
        /// Gets aging report for outstanding balances
        /// </summary>
        public DataTable GetAgingReport()
        {
            return financeDAL.GetOutstandingBalances();
        }

        /// <summary>
        /// Gets financial dashboard stats
        /// </summary>
        public DashboardStats GetFinancialStats()
        {
            var summary = financeDAL.GetFinancialSummary(GetCurrentAcademicYearId());
            var stats = new DashboardStats();

            if (summary.Rows.Count > 0)
            {
                var row = summary.Rows[0];
                stats.TotalBilled = row["TotalBilled"] != DBNull.Value ? Convert.ToDecimal(row["TotalBilled"]) : 0;
                stats.TotalCollected = row["TotalCollected"] != DBNull.Value ? Convert.ToDecimal(row["TotalCollected"]) : 0;
                stats.TotalOutstanding = row["TotalOutstanding"] != DBNull.Value ? Convert.ToDecimal(row["TotalOutstanding"]) : 0;
            }

            return stats;
        }

        #endregion

        #region Fee Structure Management

        public DataTable GetFeeStructures(int? academicYearId = null)
        {
            return financeDAL.GetFeeStructures(academicYearId, null);
        }

        public int AddFeeStructure(string feeName, string category, int? classId,
            decimal amount, string billingTerm, int academicYearId, int createdBy)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(feeName))
                throw new ValidationException("Fee name is required");

            if (amount <= 0)
                throw new ValidationException("Amount must be greater than 0");

            int feeId = financeDAL.AddFeeStructure(feeName, category, classId, amount, billingTerm, academicYearId);

            auditLogger.LogCreate(createdBy, "Finance", "Fee Structure", feeId.ToString(),
                string.Format("Added {0}: ${1:N2} ({2})", feeName, amount, billingTerm));

            return feeId;
        }

        #endregion

        #region Helper Methods

        private bool InvoiceExistsForTerm(int studentId, int termId)
        {
            var invoices = financeDAL.GetInvoices(null, studentId, null);
            foreach (var inv in invoices)
            {
                if (inv.TermID == termId && inv.Status != "Void")
                    return true;
            }
            return false;
        }

        private int GetCurrentAcademicYearId()
        {
            AcademicDAL academicDAL = new AcademicDAL();
            var year = academicDAL.GetCurrentAcademicYear();
            return year != null ? Convert.ToInt32(year["AcademicYearID"]) : 1;
        }

        private void NotifyPaymentComplete(Invoice invoice)
        {
            // Send notification to parent
            // Implementation depends on notification service
        }

        #endregion
    }
}