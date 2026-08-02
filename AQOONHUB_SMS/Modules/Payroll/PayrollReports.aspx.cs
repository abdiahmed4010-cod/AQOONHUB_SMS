using System;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Payroll
{
    public partial class PayrollReports : System.Web.UI.Page
    {
        private readonly PayrollRepository _repo = new PayrollRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthorizeFinance()) return;
            if (!IsPostBack)
            {
                BindPeriods();
                BindDepartments();
                LoadReport();
            }
        }

        private bool AuthorizeFinance()
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx", true);
                return false;
            }
            string role = _repo.NormalizeRole(Convert.ToString(Session["Role"]));
            if (role != "superadmin" && role != "admin" && role != "accountant" && role != "finance")
            {
                Response.Redirect("~/Default.aspx", true);
                return false;
            }
            return true;
        }

        private void BindPeriods()
        {
            DataTable dt = _repo.GetPayrollPeriods();
            ddlPeriod.Items.Clear();
            ddlPeriod.Items.Add(new ListItem("All Periods", ""));
            foreach (DataRow row in dt.Rows)
                ddlPeriod.Items.Add(new ListItem(Convert.ToString(row["PeriodName"]), Convert.ToString(row["PayrollPeriodID"])));
        }

        private void BindDepartments()
        {
            DataTable staff = _repo.GetEligibleStaff();
            System.Collections.Generic.SortedSet<string> depts =
                new System.Collections.Generic.SortedSet<string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (DataRow row in staff.Rows)
            {
                string d = Convert.ToString(row["Department"]);
                if (!string.IsNullOrWhiteSpace(d)) depts.Add(d.Trim());
            }
            ddlDept.Items.Clear();
            ddlDept.Items.Add(new ListItem("All Departments", ""));
            foreach (string d in depts) ddlDept.Items.Add(new ListItem(d, d));
        }

        private int? PeriodFilter()
        {
            int pid;
            return int.TryParse(ddlPeriod.SelectedValue, out pid) && pid > 0 ? pid : (int?)null;
        }

        private void LoadReport()
        {
            DataTable summary = _repo.GetPayrollReport(PeriodFilter(), ddlDept.SelectedValue, ddlStatus.SelectedValue);
            DataRow r = summary.Rows[0];

            litEmployees.Text = Convert.ToInt64(r["RecordCount"]).ToString("N0");
            litBasic.Text = Money(r["TotalBasic"]);
            litOtherAllow.Text = Money(r["TotalOtherAllowance"]);
            litBonus.Text = Money(r["TotalBonus"]);
            litGross.Text = Money(r["TotalGross"]);
            litNet.Text = Money(r["TotalNet"]);
            litTax.Text = Money(r["TotalTax"]);
            litOtherDed.Text = Money(r["TotalOtherDeduction"]);
            litDeductions.Text = Money(r["TotalDeductions"]);
            litPaid.Text = Money(r["PaidAmount"]);
            litPending.Text = Money(r["PendingAmount"]);
            litFailed.Text = Convert.ToInt64(r["FailedCount"]).ToString("N0");

            gvDept.DataSource = _repo.GetPayrollByDepartment(PeriodFilter(), ddlStatus.SelectedValue);
            gvDept.DataBind();
        }

        protected void btnView_Click(object sender, EventArgs e) { LoadReport(); }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            DataTable dt = _repo.GetPayrollByDepartment(PeriodFilter(), ddlStatus.SelectedValue);
            StringBuilder b = new StringBuilder("\uFEFF");
            foreach (DataColumn c in dt.Columns) b.Append(CsvCell(c.ColumnName)).Append(',');
            b.AppendLine();
            foreach (DataRow row in dt.Rows)
            {
                foreach (object v in row.ItemArray) b.Append(CsvCell(Convert.ToString(v))).Append(',');
                b.AppendLine();
            }
            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment; filename=payroll-report.csv");
            Response.Write(b.ToString());
            Response.End();
        }

        // Escapes a CSV cell and neutralizes formula/CSV-injection payloads
        // (values that begin with =, +, -, @, tab or CR) by prefixing an apostrophe.
        private static string CsvCell(string value)
        {
            string s = value ?? string.Empty;
            if (s.Length > 0 && "=+-@\t\r".IndexOf(s[0]) >= 0)
                s = "'" + s;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }

        private string Money(object value)
        {
            return PayrollFormat.Money(value);
        }
    }
}
