using System;
using System.Data;

namespace AQOONHUB_SMS.Modules.Reports
{
    public sealed partial class ReportsRepository
    {
        // Historical payroll uses the SNAPSHOT columns stored on PayrollRecords per period -
        // never recomputed from the employee's current salary structure.
        private DataTable Stage3Payroll(string handler, ReportFilter f, bool allowSensitive)
        {
            switch (handler)
            {
                case "pay-salary-structure": return ExecuteDataTable("SELECT ISNULL(u.FullName,sf.EmployeeID) AS [Employee], ss.BasicSalary AS [Basic], ss.HousingAllowance AS [Housing], ss.TransportAllowance AS [Transport], ss.OtherAllowance AS [Other Allow], ss.TaxDeduction AS [Tax], ss.OtherDeduction AS [Other Deduct], ss.EffectiveFrom AS [Effective], ISNULL(ss.Status,'') AS [Status] FROM StaffSalaryStructures ss JOIN Staff sf ON sf.StaffID=ss.StaffID LEFT JOIN Users u ON u.UserID=sf.UserID ORDER BY [Employee]", null);
                case "pay-monthly": return Payroll(f, null);
                case "pay-by-department": return ExecuteDataTable("SELECT ISNULL(sf.Department,'Unassigned') AS [Department], COUNT(*) AS [Employees], SUM(pr.GrossSalary) AS [Gross], SUM(pr.NetSalary) AS [Net] FROM PayrollRecords pr JOIN Staff sf ON sf.StaffID=pr.StaffID WHERE (@p IS NULL OR pr.PayrollPeriodID=@p) GROUP BY sf.Department ORDER BY [Net] DESC", PeriodP(f));
                case "pay-by-employee": return Payroll(f, null);
                case "pay-basic": return PayCol(f, "BasicSalary", "Basic Salary");
                case "pay-allowances": return PayCol(f, "TotalAllowances", "Allowances");
                case "pay-deductions": return PayCol(f, "TotalDeductions", "Deductions");
                case "pay-net": return PayCol(f, "NetSalary", "Net Salary");
                case "pay-paid": return Payroll(f, "Paid");
                case "pay-unpaid": return Payroll(f, "!Paid");
                case "pay-history": return ExecuteDataTable("SELECT ISNULL(u.FullName,sf.EmployeeID) AS [Employee], pp.PeriodName AS [Period], pr.NetSalary AS [Net], pr.PaymentStatus AS [Status], pr.PaidDate AS [Paid Date], ISNULL(pr.PaymentMethod,'') AS [Method] FROM PayrollRecords pr JOIN Staff sf ON sf.StaffID=pr.StaffID LEFT JOIN Users u ON u.UserID=sf.UserID LEFT JOIN PayrollPeriods pp ON pp.PayrollPeriodID=pr.PayrollPeriodID ORDER BY pr.PaidDate DESC", null);
                case "pay-payslips": return Payroll(f, null);
                case "pay-run": return ExecuteDataTable("SELECT pp.PeriodName AS [Pay Run], pp.StartDate AS [Start], pp.EndDate AS [End], pp.Status AS [Status], (SELECT COUNT(*) FROM PayrollRecords r WHERE r.PayrollPeriodID=pp.PayrollPeriodID) AS [Employees], (SELECT ISNULL(SUM(r.GrossSalary),0) FROM PayrollRecords r WHERE r.PayrollPeriodID=pp.PayrollPeriodID) AS [Gross], (SELECT ISNULL(SUM(r.TotalDeductions),0) FROM PayrollRecords r WHERE r.PayrollPeriodID=pp.PayrollPeriodID) AS [Deductions], (SELECT ISNULL(SUM(r.NetSalary),0) FROM PayrollRecords r WHERE r.PayrollPeriodID=pp.PayrollPeriodID) AS [Net], ISNULL(u.FullName,'') AS [Processed By] FROM PayrollPeriods pp LEFT JOIN Users u ON u.UserID=pp.CreatedBy ORDER BY pp.StartDate DESC", null);
                case "pay-cancelled": return Payroll(f, "Cancelled");
                case "pay-summary": return PayrollSummary(f);
                case "pay-annual-cost": return ExecuteDataTable("SELECT YEAR(pp.StartDate) AS [Year], COUNT(*) AS [Records], SUM(pr.NetSalary) AS [Total Net] FROM PayrollRecords pr JOIN PayrollPeriods pp ON pp.PayrollPeriodID=pr.PayrollPeriodID GROUP BY YEAR(pp.StartDate) ORDER BY [Year] DESC", null);
                case "pay-salary-change": return ExecuteDataTable("SELECT ISNULL(u.FullName,sf.EmployeeID) AS [Employee], ss.BasicSalary AS [Basic], ss.EffectiveFrom AS [Effective From], ss.EffectiveTo AS [Effective To], ISNULL(ss.Status,'') AS [Status] FROM StaffSalaryStructures ss JOIN Staff sf ON sf.StaffID=ss.StaffID LEFT JOIN Users u ON u.UserID=sf.UserID ORDER BY sf.StaffID, ss.EffectiveFrom", null);

                default: return Stage4(handler, f, allowSensitive);
            }
        }

        private DataTable Payroll(ReportFilter f, string statusMode)
        {
            string st = "";
            if (statusMode == "Paid") st = " AND pr.PaymentStatus='Paid'";
            else if (statusMode == "!Paid") st = " AND ISNULL(pr.PaymentStatus,'')<>'Paid'";
            else if (statusMode == "Cancelled") st = " AND pr.PaymentStatus='Cancelled'";
            string sql = @"
SELECT ISNULL(u.FullName,sf.EmployeeID) AS [Employee], ISNULL(sf.Department,'') AS [Department], pr.BasicSalary AS [Basic],
       pr.TotalAllowances AS [Allowances], pr.TotalDeductions AS [Deductions], pr.GrossSalary AS [Gross], pr.NetSalary AS [Net],
       pr.PaymentStatus AS [Status], pr.PaidDate AS [Paid Date]
FROM PayrollRecords pr JOIN Staff sf ON sf.StaffID=pr.StaffID LEFT JOIN Users u ON u.UserID=sf.UserID
WHERE (@p IS NULL OR pr.PayrollPeriodID=@p)" + st + @"
  AND (@dept IS NULL OR sf.Department=@dept) AND (@staff IS NULL OR pr.StaffID=@staff)
ORDER BY [Employee]";
            return ExecuteDataTable(sql, new[] { P("@p", (object)f.PeriodID ?? DBNull.Value), P("@dept", (object)NullIfEmpty(f.Department) ?? DBNull.Value), P("@staff", (object)f.StaffID ?? DBNull.Value) });
        }

        private DataTable PayCol(ReportFilter f, string col, string label)
        {
            string sql = "SELECT ISNULL(u.FullName,sf.EmployeeID) AS [Employee], ISNULL(sf.Department,'') AS [Department], pr." + col + " AS [" + label + @"], pr.PaymentStatus AS [Status]
FROM PayrollRecords pr JOIN Staff sf ON sf.StaffID=pr.StaffID LEFT JOIN Users u ON u.UserID=sf.UserID
WHERE (@p IS NULL OR pr.PayrollPeriodID=@p) ORDER BY [Employee]";
            return ExecuteDataTable(sql, PeriodP(f));
        }

        private DataTable PayrollSummary(ReportFilter f)
        {
            const string sql = @"
SELECT 'Total Employees' AS [Metric], CAST(COUNT(*) AS decimal(18,2)) AS [Value] FROM PayrollRecords pr WHERE (@p IS NULL OR pr.PayrollPeriodID=@p)
UNION ALL SELECT 'Total Basic', CAST(ISNULL(SUM(BasicSalary),0) AS decimal(18,2)) FROM PayrollRecords WHERE (@p IS NULL OR PayrollPeriodID=@p)
UNION ALL SELECT 'Total Allowances', CAST(ISNULL(SUM(TotalAllowances),0) AS decimal(18,2)) FROM PayrollRecords WHERE (@p IS NULL OR PayrollPeriodID=@p)
UNION ALL SELECT 'Total Deductions', CAST(ISNULL(SUM(TotalDeductions),0) AS decimal(18,2)) FROM PayrollRecords WHERE (@p IS NULL OR PayrollPeriodID=@p)
UNION ALL SELECT 'Total Gross', CAST(ISNULL(SUM(GrossSalary),0) AS decimal(18,2)) FROM PayrollRecords WHERE (@p IS NULL OR PayrollPeriodID=@p)
UNION ALL SELECT 'Total Net', CAST(ISNULL(SUM(NetSalary),0) AS decimal(18,2)) FROM PayrollRecords WHERE (@p IS NULL OR PayrollPeriodID=@p)
UNION ALL SELECT 'Paid', CAST(SUM(CASE WHEN PaymentStatus='Paid' THEN 1 ELSE 0 END) AS decimal(18,2)) FROM PayrollRecords WHERE (@p IS NULL OR PayrollPeriodID=@p)
UNION ALL SELECT 'Unpaid', CAST(SUM(CASE WHEN ISNULL(PaymentStatus,'')<>'Paid' THEN 1 ELSE 0 END) AS decimal(18,2)) FROM PayrollRecords WHERE (@p IS NULL OR PayrollPeriodID=@p)";
            return ExecuteDataTable(sql, PeriodP(f));
        }

        private System.Data.SqlClient.SqlParameter[] PeriodP(ReportFilter f) { return new[] { P("@p", (object)f.PeriodID ?? DBNull.Value) }; }
    }
}
