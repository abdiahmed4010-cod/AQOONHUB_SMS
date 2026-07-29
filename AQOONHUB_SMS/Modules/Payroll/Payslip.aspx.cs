using System;
using System.Data;
using System.Web;

namespace AQOONHUB_SMS.Modules.Payroll
{
    public partial class Payslip : System.Web.UI.Page
    {
        private readonly PayrollRepository _repo = new PayrollRepository();

        private int RecordId
        {
            get { int id; return int.TryParse(Request.QueryString["id"], out id) ? id : 0; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx", true);
                return;
            }
            string role = _repo.NormalizeRole(Convert.ToString(Session["Role"]));
            if (role != "superadmin" && role != "admin" && role != "accountant" && role != "finance")
            {
                Response.Redirect("~/Default.aspx", true);
                return;
            }

            if (!IsPostBack) LoadPayslip();
        }

        private void LoadPayslip()
        {
            DataRow row = RecordId > 0 ? _repo.GetPayrollRecord(RecordId) : null;
            if (row == null)
            {
                pnlBody.Visible = false;
                pnlNotFound.Visible = true;
                return;
            }

            string period = Convert.ToString(row["PeriodName"]);
            litPeriod.Text = Enc(period);
            litPeriod2.Text = Enc(period);
            litEmpId.Text = Enc(row["EmployeeID"]);
            litDept.Text = Enc(row["Department"]);
            litPosition.Text = Enc(row["Position"]);
            litPayDate.Text = row["PaymentDate"] == DBNull.Value ? "—" : Convert.ToDateTime(row["PaymentDate"]).ToString("dd MMM yyyy");

            litMethod.Text = Blank(row["PaymentMethod"]);
            litReference.Text = Blank(row["PaymentReference"]);
            litPaidDate.Text = row["PaidDate"] == DBNull.Value ? "—" : Convert.ToDateTime(row["PaidDate"]).ToString("dd MMM yyyy");
            lblStatus.Text = Enc(row["PaymentStatus"]);

            // Housing & Transport allowances are intentionally excluded.
            litBasic.Text = Money(row["BasicSalary"]);
            litOther.Text = Money(row["OtherAllowance"]);
            litBonus.Text = Money(row["BonusAmount"]);
            litGross.Text = Money(row["GrossSalary"]);
            litTax.Text = Money(row["TaxDeduction"]);
            litOtherDed.Text = Money(row["OtherDeduction"]);
            litDeductions.Text = Money(row["TotalDeductions"]);
            litNet.Text = Money(row["NetSalary"]);
        }

        private string Money(object value)
        {
            return PayrollFormat.Money(value);
        }

        private string Blank(object v)
        {
            string s = v == null || v == DBNull.Value ? "" : v.ToString();
            return string.IsNullOrEmpty(s) ? "—" : HttpUtility.HtmlEncode(s);
        }

        private string Enc(object v)
        {
            return HttpUtility.HtmlEncode(v == null || v == DBNull.Value ? "" : v.ToString());
        }
    }
}
