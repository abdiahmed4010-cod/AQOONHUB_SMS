using System;
using System.Data;

namespace AQOONHUB_SMS.Modules.Reports
{
    public sealed partial class ReportsRepository
    {
        // Balance is computed from real invoice amounts (decimal), never from display text.
        private DataTable Stage3Finance(string handler, ReportFilter f, bool allowSensitive)
        {
            switch (handler)
            {
                case "fin-fee-categories": return ExecuteDataTable("SELECT CategoryName AS [Category], ISNULL(CategoryCode,'') AS [Code], ISNULL(Description,'') AS [Description], CASE WHEN ISNULL(IsActive,1)=1 THEN 'Active' ELSE 'Inactive' END AS [Status] FROM FeeCategories ORDER BY CategoryName", null);
                case "fin-fee-structure": return ExecuteDataTable("SELECT fs.FeeName AS [Fee], ISNULL(fs.Category,'') AS [Category], ISNULL(c.ClassName,'') AS [Class], fs.Amount AS [Amount], ISNULL(fs.BillingTerm,'') AS [Billing], CASE WHEN ISNULL(fs.IsActive,1)=1 THEN 'Active' ELSE 'Inactive' END AS [Status] FROM FeeStructures fs LEFT JOIN Classes c ON c.ClassID=fs.ClassID WHERE (@y IS NULL OR fs.AcademicYearID=@y) ORDER BY c.ClassName, fs.FeeName", YP(f));
                case "fin-student-statement": return StudentStatement(f);
                case "fin-class-fee": return ExecuteDataTable("SELECT c.ClassName AS [Class], ISNULL(sec.SectionName,'') AS [Section], fc.CategoryName AS [Category], cfs.Amount AS [Amount], ISNULL(cfs.BillingTerm,'') AS [Billing] FROM ClassFeeStructures cfs JOIN Classes c ON c.ClassID=cfs.ClassID LEFT JOIN Sections sec ON sec.SectionID=cfs.SectionID LEFT JOIN FeeCategories fc ON fc.FeeCategoryID=cfs.FeeCategoryID WHERE (@y IS NULL OR cfs.AcademicYearID=@y) AND (@c IS NULL OR cfs.ClassID=@c) ORDER BY c.ClassName", YCP(f));
                case "fin-section-fee": return ExecuteDataTable("SELECT c.ClassName AS [Class], sec.SectionName AS [Section], fc.CategoryName AS [Category], cfs.Amount AS [Amount] FROM ClassFeeStructures cfs JOIN Classes c ON c.ClassID=cfs.ClassID JOIN Sections sec ON sec.SectionID=cfs.SectionID LEFT JOIN FeeCategories fc ON fc.FeeCategoryID=cfs.FeeCategoryID WHERE cfs.SectionID IS NOT NULL AND (@y IS NULL OR cfs.AcademicYearID=@y) ORDER BY c.ClassName, sec.SectionName", YP(f));
                case "fin-collected": return ExecuteDataTable("SELECT p.PaymentDate AS [Date], p.ReceiptNumber AS [Receipt], st.FullName AS [Student], p.AmountPaid AS [Amount], p.PaymentMethod AS [Method], ISNULL(u.FullName,'') AS [Cashier] FROM FeePayments p JOIN Students st ON st.StudentID=p.StudentID LEFT JOIN Users u ON u.UserID=p.ReceivedBy WHERE (@from IS NULL OR p.PaymentDate>=@from) AND (@to IS NULL OR p.PaymentDate<=@to) ORDER BY p.PaymentDate DESC", DateParams(f));
                case "fin-outstanding": return Invoices(f, "outstanding");
                case "fin-overdue": return Invoices(f, "overdue");
                case "fin-partial": return Invoices(f, "partial");
                case "fin-fully-paid": return Invoices(f, "paid");
                case "fin-unpaid": return Invoices(f, "unpaid");
                case "fin-payment-history": return ExecuteDataTable("SELECT p.PaymentDate AS [Date], p.ReceiptNumber AS [Receipt], st.FullName AS [Student], p.AmountPaid AS [Amount], p.PaymentMethod AS [Method], ISNULL(p.ReferenceNumber,'') AS [Reference] FROM FeePayments p JOIN Students st ON st.StudentID=p.StudentID WHERE (@st IS NULL OR p.StudentID=@st) ORDER BY p.PaymentDate DESC", new[] { P("@st", (object)f.StudentID ?? DBNull.Value) });
                case "fin-daily-collection": return Collection(f, "day");
                case "fin-weekly-collection": return Collection(f, "week");
                case "fin-monthly-collection": return Collection(f, "month");
                case "fin-year-collection": return ExecuteDataTable("SELECT ISNULL(y.YearName,'—') AS [Academic Year], COUNT(*) AS [Payments], SUM(p.AmountPaid) AS [Collected] FROM FeePayments p LEFT JOIN FeeInvoices i ON i.InvoiceID=p.InvoiceID LEFT JOIN AcademicYears y ON y.AcademicYearID=i.AcademicYearID GROUP BY y.YearName, y.StartDate ORDER BY y.StartDate DESC", null);
                case "fin-payment-methods": return ExecuteDataTable("SELECT ISNULL(PaymentMethod,'Unknown') AS [Method], COUNT(*) AS [Payments], SUM(AmountPaid) AS [Total] FROM FeePayments GROUP BY PaymentMethod ORDER BY [Total] DESC", null);
                case "fin-cashier": return ExecuteDataTable("SELECT ISNULL(u.FullName,'—') AS [Cashier], COUNT(*) AS [Payments], SUM(p.AmountPaid) AS [Collected] FROM FeePayments p LEFT JOIN Users u ON u.UserID=p.ReceivedBy GROUP BY u.FullName ORDER BY [Collected] DESC", null);
                case "fin-income-summary":
                case "fin-summary": return FinanceSummary();
                case "fin-discounts": return ExecuteDataTable("SELECT i.InvoiceNumber AS [Invoice], st.FullName AS [Student], i.DiscountAmount AS [Discount], i.TotalAmount AS [Total] FROM FeeInvoices i JOIN Students st ON st.StudentID=i.StudentID WHERE ISNULL(i.DiscountAmount,0)>0 ORDER BY i.DiscountAmount DESC", null);

                default: return Stage3Payroll(handler, f, allowSensitive);
            }
        }

        private DataTable StudentStatement(ReportFilter f)
        {
            if (!f.StudentID.HasValue) return EmptyNote("Select a student to view their fee statement.");
            const string sql = @"
SELECT i.InvoiceNumber AS [Invoice], st.FullName AS [Student], st.StudentCode AS [Code], ISNULL(y.YearName,'') AS [Academic Year],
       i.InvoiceDate AS [Date], i.DueDate AS [Due], i.TotalAmount AS [Total], ISNULL(i.DiscountAmount,0) AS [Discount],
       ISNULL(i.PaidAmount,0) AS [Paid], (i.TotalAmount - ISNULL(i.PaidAmount,0)) AS [Balance], i.Status AS [Status]
FROM FeeInvoices i JOIN Students st ON st.StudentID=i.StudentID LEFT JOIN AcademicYears y ON y.AcademicYearID=i.AcademicYearID
WHERE i.StudentID=@st ORDER BY i.InvoiceDate DESC";
            return ExecuteDataTable(sql, new[] { P("@st", f.StudentID.Value) });
        }

        private DataTable Invoices(ReportFilter f, string mode)
        {
            string w;
            switch (mode)
            {
                case "outstanding": w = "(i.TotalAmount - ISNULL(i.PaidAmount,0)) > 0"; break;
                case "overdue": w = "(i.TotalAmount - ISNULL(i.PaidAmount,0)) > 0 AND i.DueDate < CAST(GETDATE() AS date)"; break;
                case "partial": w = "ISNULL(i.PaidAmount,0) > 0 AND ISNULL(i.PaidAmount,0) < i.TotalAmount"; break;
                case "paid": w = "ISNULL(i.PaidAmount,0) >= i.TotalAmount AND i.TotalAmount > 0"; break;
                case "unpaid": w = "ISNULL(i.PaidAmount,0) = 0"; break;
                default: w = "1=1"; break;
            }
            string sql = @"
SELECT st.FullName AS [Student], st.StudentCode AS [Code], i.InvoiceNumber AS [Invoice], i.TotalAmount AS [Total], ISNULL(i.PaidAmount,0) AS [Paid],
       (i.TotalAmount - ISNULL(i.PaidAmount,0)) AS [Balance], i.DueDate AS [Due], i.Status AS [Status]
FROM FeeInvoices i JOIN Students st ON st.StudentID=i.StudentID
WHERE " + w + @" AND (@y IS NULL OR i.AcademicYearID=@y) ORDER BY [Balance] DESC";
            return ExecuteDataTable(sql, YP(f));
        }

        private DataTable Collection(ReportFilter f, string grain)
        {
            string bucket = grain == "month" ? "DATEFROMPARTS(YEAR(PaymentDate),MONTH(PaymentDate),1)"
                : grain == "week" ? "DATEADD(DAY, -(DATEPART(WEEKDAY, PaymentDate)-1), CAST(PaymentDate AS date))"
                : "CAST(PaymentDate AS date)";
            string sql = "SELECT " + bucket + @" AS [Period], COUNT(*) AS [Payments], SUM(AmountPaid) AS [Collected]
FROM FeePayments WHERE (@from IS NULL OR PaymentDate>=@from) AND (@to IS NULL OR PaymentDate<=@to)
GROUP BY " + bucket + " ORDER BY [Period] DESC";
            return ExecuteDataTable(sql, DateParams(f));
        }

        private DataTable FinanceSummary()
        {
            const string sql = @"
SELECT 'Total Invoiced' AS [Metric], CAST(ISNULL(SUM(TotalAmount),0) AS decimal(18,2)) AS [Amount] FROM FeeInvoices
UNION ALL SELECT 'Total Collected', CAST(ISNULL(SUM(PaidAmount),0) AS decimal(18,2)) FROM FeeInvoices
UNION ALL SELECT 'Total Discount', CAST(ISNULL(SUM(DiscountAmount),0) AS decimal(18,2)) FROM FeeInvoices
UNION ALL SELECT 'Total Outstanding', CAST(ISNULL(SUM(TotalAmount - ISNULL(PaidAmount,0)),0) AS decimal(18,2)) FROM FeeInvoices
UNION ALL SELECT 'Payments Received', (SELECT COUNT(*) FROM FeePayments)";
            return ExecuteDataTable(sql, null);
        }

        private static DataTable EmptyNote(string note) { DataTable t = new DataTable(); t.Columns.Add("Notice"); t.Rows.Add(note); return t; }
    }
}
