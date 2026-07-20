using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;


namespace AQOONHUB_SMS.App_Code.DataAccess
{
    public class FinanceDAL
    {
        private DatabaseHelper db;

        public FinanceDAL()
        {
            db = new DatabaseHelper();
        }

        public DataTable GetFeeStructures(int? academicYearId, string category)
        {
            string query = @"
                SELECT fs.*, c.ClassName
                FROM FeeStructures fs
                LEFT JOIN Classes c ON fs.ClassID = c.ClassID
                WHERE fs.IsActive = 1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (academicYearId.HasValue)
            {
                query += " AND fs.AcademicYearID = @AcademicYearID";
                parameters.Add(new SqlParameter("@AcademicYearID", academicYearId.Value));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query += " AND fs.Category = @Category";
                parameters.Add(new SqlParameter("@Category", category));
            }

            query += " ORDER BY fs.Category, fs.Amount";

            return db.ExecuteQuery(query, parameters.ToArray());
        }

        public DataTable GetFeeStructures()
        {
            return GetFeeStructures(null, null);
        }

        public int AddFeeStructure(string feeName, string category, int? classId, decimal amount, string billingTerm, int academicYearId)
        {
            string query = @"
                INSERT INTO FeeStructures (FeeName, Category, ClassID, Amount, BillingTerm, AcademicYearID, IsActive)
                VALUES (@FeeName, @Category, @ClassID, @Amount, @BillingTerm, @AcademicYearID, 1);
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@FeeName", feeName),
                new SqlParameter("@Category", category),
                new SqlParameter("@ClassID", classId.HasValue ? (object)classId.Value : DBNull.Value),
                new SqlParameter("@Amount", amount),
                new SqlParameter("@BillingTerm", billingTerm),
                new SqlParameter("@AcademicYearID", academicYearId)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        public List<Invoice> GetInvoices(string status, int? studentId, string search)
        {
            List<Invoice> invoices = new List<Invoice>();

            string query = @"
                SELECT i.*, s.StudentCode, s.FirstName + ' ' + s.LastName as StudentName,
                       c.ClassName, sec.SectionName,
                       ay.YearName, t.TermName
                FROM Invoices i
                INNER JOIN Students s ON i.StudentID = s.StudentID
                INNER JOIN Sections sec ON s.SectionID = sec.SectionID
                INNER JOIN Classes c ON sec.ClassID = c.ClassID
                INNER JOIN AcademicYears ay ON i.AcademicYearID = ay.AcademicYearID
                INNER JOIN Terms t ON i.TermID = t.TermID
                WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(status))
            {
                query += " AND i.Status = @Status";
                parameters.Add(new SqlParameter("@Status", status));
            }

            if (studentId.HasValue)
            {
                query += " AND i.StudentID = @StudentID";
                parameters.Add(new SqlParameter("@StudentID", studentId.Value));
            }

            if (!string.IsNullOrEmpty(search))
            {
                query += @" AND (i.InvoiceNo LIKE @Search OR s.FirstName LIKE @Search 
                          OR s.LastName LIKE @Search OR s.StudentCode LIKE @Search)";
                parameters.Add(new SqlParameter("@Search", "%" + search + "%"));
            }

            query += " ORDER BY i.GeneratedAt DESC";

            DataTable dt = db.ExecuteQuery(query, parameters.ToArray());

            foreach (DataRow row in dt.Rows)
            {
                invoices.Add(MapToInvoice(row));
            }

            return invoices;
        }

        public List<Invoice> GetInvoices()
        {
            return GetInvoices(null, null, null);
        }

        public Invoice GetInvoiceById(int invoiceId)
        {
            string query = @"
                SELECT i.*, s.StudentCode, s.FirstName + ' ' + s.LastName as StudentName,
                       c.ClassName, sec.SectionName
                FROM Invoices i
                INNER JOIN Students s ON i.StudentID = s.StudentID
                INNER JOIN Sections sec ON s.SectionID = sec.SectionID
                INNER JOIN Classes c ON sec.ClassID = c.ClassID
                WHERE i.InvoiceID = @InvoiceID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@InvoiceID", invoiceId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
                return MapToInvoice(dt.Rows[0]);

            return null;
        }

        public string GenerateInvoice(int studentId, int termId, int generatedBy)
        {
            string query = "sp_GenerateInvoice";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@StudentID", studentId),
                new SqlParameter("@TermID", termId),
                new SqlParameter("@GeneratedBy", generatedBy)
            };

            DataTable dt = db.ExecuteStoredProcedure(query, parameters);

            if (dt.Rows.Count > 0)
                return dt.Rows[0]["GeneratedInvoiceNo"].ToString();

            return null;
        }

        public bool VoidInvoice(int invoiceId, int voidedBy, string reason)
        {
            string query = @"
                UPDATE Invoices SET
                    Status = 'Void',
                    PaidAmount = 0
                WHERE InvoiceID = @InvoiceID;

                INSERT INTO AuditLog (UserID, Action, Module, Detail, IPAddress, ActionTime)
                VALUES (@VoidedBy, 'VOID', 'Invoices', 'Voided invoice ' + CAST(@InvoiceID AS VARCHAR) + ': ' + @Reason, 'SYSTEM', GETDATE());";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@InvoiceID", invoiceId),
                new SqlParameter("@VoidedBy", voidedBy),
                new SqlParameter("@Reason", reason)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        public DataTable GetInvoiceItems(int invoiceId)
        {
            string query = @"
                SELECT ii.*, fs.FeeName, fs.Category
                FROM InvoiceItems ii
                INNER JOIN FeeStructures fs ON ii.FeeStructureID = fs.FeeStructureID
                WHERE ii.InvoiceID = @InvoiceID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@InvoiceID", invoiceId)
            };

            return db.ExecuteQuery(query, parameters);
        }

        public DataTable GetPayments(int? invoiceId, int? studentId, string method)
        {
            string query = @"
                SELECT p.*, i.InvoiceNo, s.FirstName + ' ' + s.LastName as StudentName,
                       u.FullName as ReceivedByName
                FROM Payments p
                INNER JOIN Invoices i ON p.InvoiceID = i.InvoiceID
                INNER JOIN Students s ON i.StudentID = s.StudentID
                INNER JOIN Users u ON p.ReceivedBy = u.UserID
                WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (invoiceId.HasValue)
            {
                query += " AND p.InvoiceID = @InvoiceID";
                parameters.Add(new SqlParameter("@InvoiceID", invoiceId.Value));
            }

            if (studentId.HasValue)
            {
                query += " AND i.StudentID = @StudentID";
                parameters.Add(new SqlParameter("@StudentID", studentId.Value));
            }

            if (!string.IsNullOrEmpty(method))
            {
                query += " AND p.PaymentMethod = @PaymentMethod";
                parameters.Add(new SqlParameter("@PaymentMethod", method));
            }

            query += " ORDER BY p.PaymentDate DESC";

            return db.ExecuteQuery(query, parameters.ToArray());
        }

        public DataTable GetPayments()
        {
            return GetPayments(null, null, null);
        }

        public string RecordPayment(int invoiceId, decimal amount, string paymentMethod, int receivedBy, string notes)
        {
            string query = "sp_RecordPayment";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@InvoiceID", invoiceId),
                new SqlParameter("@Amount", amount),
                new SqlParameter("@PaymentMethod", paymentMethod),
                new SqlParameter("@ReceivedBy", receivedBy),
                new SqlParameter("@Notes", string.IsNullOrEmpty(notes) ? (object)DBNull.Value : notes)
            };

            DataTable dt = db.ExecuteStoredProcedure(query, parameters);

            if (dt.Rows.Count > 0)
                return dt.Rows[0]["GeneratedReceiptNo"].ToString();

            return null;
        }

        public DataTable GetPaymentSummary(int? academicYearId)
        {
            string query = @"
                SELECT 
                    COUNT(*) as TotalPayments,
                    SUM(Amount) as TotalCollected,
                    SUM(CASE WHEN PaymentMethod = 'Zaad' THEN Amount ELSE 0 END) as ZaadTotal,
                    SUM(CASE WHEN PaymentMethod = 'eDahab' THEN Amount ELSE 0 END) as EdahabTotal,
                    SUM(CASE WHEN PaymentMethod = 'Cash' THEN Amount ELSE 0 END) as CashTotal,
                    SUM(CASE WHEN PaymentMethod = 'Bank Transfer' THEN Amount ELSE 0 END) as BankTotal
                FROM Payments p
                INNER JOIN Invoices i ON p.InvoiceID = i.InvoiceID
                WHERE (@AcademicYearID IS NULL OR i.AcademicYearID = @AcademicYearID)";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@AcademicYearID", academicYearId.HasValue ? (object)academicYearId.Value : DBNull.Value)
            };

            return db.ExecuteQuery(query, parameters);
        }

        public DataTable GetOutstandingBalances(string className, int? daysOverdue)
        {
            string query = @"
                SELECT 
                    i.InvoiceID,
                    i.InvoiceNo,
                    s.StudentCode,
                    s.FirstName + ' ' + s.LastName as StudentName,
                    c.ClassName,
                    sec.SectionName,
                    i.TotalAmount,
                    i.PaidAmount,
                    i.TotalAmount - i.PaidAmount as Balance,
                    i.DueDate,
                    DATEDIFF(day, i.DueDate, GETDATE()) as DaysOverdue,
                    CASE 
                        WHEN DATEDIFF(day, i.DueDate, GETDATE()) > 90 THEN 'Critical'
                        WHEN DATEDIFF(day, i.DueDate, GETDATE()) > 30 THEN 'High'
                        WHEN DATEDIFF(day, i.DueDate, GETDATE()) > 0 THEN 'Medium'
                        ELSE 'Current'
                    END as Priority
                FROM Invoices i
                INNER JOIN Students s ON i.StudentID = s.StudentID
                INNER JOIN Sections sec ON s.SectionID = sec.SectionID
                INNER JOIN Classes c ON sec.ClassID = c.ClassID
                WHERE i.Status IN ('Pending', 'Partially Paid', 'Overdue')
                AND i.TotalAmount > i.PaidAmount";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(className))
            {
                query += " AND c.ClassName = @ClassName";
                parameters.Add(new SqlParameter("@ClassName", className));
            }

            if (daysOverdue.HasValue)
            {
                query += " AND DATEDIFF(day, i.DueDate, GETDATE()) >= @DaysOverdue";
                parameters.Add(new SqlParameter("@DaysOverdue", daysOverdue.Value));
            }

            query += " ORDER BY Balance DESC";

            return db.ExecuteQuery(query, parameters.ToArray());
        }

        public DataTable GetOutstandingBalances()
        {
            return GetOutstandingBalances(null, null);
        }

        public DataTable GetFinancialSummary(int academicYearId)
        {
            string query = @"
                SELECT 
                    SUM(TotalAmount) as TotalBilled,
                    SUM(PaidAmount) as TotalCollected,
                    SUM(TotalAmount - PaidAmount) as TotalOutstanding,
                    COUNT(*) as TotalInvoices,
                    SUM(CASE WHEN Status = 'Paid' THEN 1 ELSE 0 END) as PaidCount,
                    SUM(CASE WHEN Status = 'Partially Paid' THEN 1 ELSE 0 END) as PartialCount,
                    SUM(CASE WHEN Status = 'Overdue' THEN 1 ELSE 0 END) as OverdueCount
                FROM Invoices
                WHERE AcademicYearID = @AcademicYearID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@AcademicYearID", academicYearId)
            };

            return db.ExecuteQuery(query, parameters);
        }

        private Invoice MapToInvoice(DataRow row)
        {
            Invoice invoice = new Invoice();
            invoice.InvoiceID = Convert.ToInt32(row["InvoiceID"]);
            invoice.InvoiceNo = row["InvoiceNo"].ToString();
            invoice.StudentID = Convert.ToInt32(row["StudentID"]);
            invoice.StudentName = row["StudentName"].ToString();
            invoice.StudentCode = row["StudentCode"].ToString();
            invoice.ClassName = row["ClassName"].ToString();
            invoice.SectionName = row["SectionName"].ToString();
            invoice.AcademicYearID = Convert.ToInt32(row["AcademicYearID"]);
            invoice.TermID = Convert.ToInt32(row["TermID"]);
            invoice.TotalAmount = Convert.ToDecimal(row["TotalAmount"]);
            invoice.PaidAmount = Convert.ToDecimal(row["PaidAmount"]);
            invoice.DueDate = Convert.ToDateTime(row["DueDate"]);
            invoice.Status = row["Status"].ToString();
            invoice.GeneratedAt = Convert.ToDateTime(row["GeneratedAt"]);
            return invoice;
        }
    }
}