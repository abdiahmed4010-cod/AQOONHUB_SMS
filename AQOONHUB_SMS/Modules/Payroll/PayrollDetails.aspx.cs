using System;
using System.Data;
using System.Web;

namespace AQOONHUB_SMS.Modules.Payroll
{
    public partial class PayrollDetails : System.Web.UI.Page
    {
        private readonly PayrollRepository _repo = new PayrollRepository();

        private int RecordId
        {
            get
            {
                int id;
                return int.TryParse(Request.QueryString["id"], out id) ? id : 0;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthorizeFinance()) return;
            if (!IsPostBack) LoadRecord();
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

        private void LoadRecord()
        {
            if (RecordId <= 0) { ShowNotFound(); return; }

            DataRow row = _repo.GetPayrollRecord(RecordId);
            if (row == null) { ShowNotFound(); return; }

            pnlBody.Visible = true;
            pnlNotFound.Visible = false;

            string empId = Convert.ToString(row["EmployeeID"]);
            string period = Convert.ToString(row["PeriodName"]);
            string status = Convert.ToString(row["PaymentStatus"]);

            litEmployee.Text = Enc(empId);
            litPeriod.Text = Enc(period);
            litEmpId.Text = Enc(empId);
            litDept.Text = Enc(row["Department"]);
            litPosition.Text = Enc(row["Position"]);
            litPeriod2.Text = Enc(period);
            litPayDate.Text = row["PaymentDate"] == DBNull.Value ? "—" : Convert.ToDateTime(row["PaymentDate"]).ToString("dd MMM yyyy");

            // Earnings / deductions — Housing & Transport allowances are intentionally excluded.
            litBasic.Text = Money(row["BasicSalary"]);
            litOther.Text = Money(row["OtherAllowance"]);
            litBonus.Text = Money(row["BonusAmount"]);
            litGross.Text = Money(row["GrossSalary"]);
            litTax.Text = Money(row["TaxDeduction"]);
            litOtherDed.Text = Money(row["OtherDeduction"]);
            litDeductions.Text = Money(row["TotalDeductions"]);
            litNet.Text = Money(row["NetSalary"]);

            // Payment info
            litStatus.Text = Enc(status);
            litMethod.Text = row["PaymentMethod"] == DBNull.Value || string.IsNullOrEmpty(Convert.ToString(row["PaymentMethod"])) ? "—" : Enc(row["PaymentMethod"]);
            litReference.Text = row["PaymentReference"] == DBNull.Value || string.IsNullOrEmpty(Convert.ToString(row["PaymentReference"])) ? "—" : Enc(row["PaymentReference"]);
            litPaidDate.Text = row["PaidDate"] == DBNull.Value ? "—" : Convert.ToDateTime(row["PaidDate"]).ToString("dd MMM yyyy");

            lblStatusBadge.Text = HttpUtility.HtmlEncode(status);
            lblStatusBadge.Attributes["style"] = StatusStyle(status);

            // Payslip link is available once the record is Paid.
            lnkPayslip.Visible = status.Equals("Paid", StringComparison.OrdinalIgnoreCase);
            lnkPayslip.NavigateUrl = "Payslip.aspx?id=" + RecordId;

            // Only Pending or Failed records can be paid.
            bool canPay = status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                          status.Equals("Failed", StringComparison.OrdinalIgnoreCase);
            pnlPay.Visible = canPay;
            if (canPay && !IsPostBack)
            {
                txtPaidDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                string method = Convert.ToString(row["PaymentMethod"]);
                if (!string.IsNullOrEmpty(method) && ddlPayMethod.Items.FindByValue(method) != null)
                    ddlPayMethod.SelectedValue = method;
            }
        }

        protected void btnMarkPaid_Click(object sender, EventArgs e)
        {
            if (RecordId <= 0) return;

            string method = ddlPayMethod.SelectedValue;
            if (string.IsNullOrWhiteSpace(method)) { ShowError("Please select a payment method."); return; }

            DateTime paidDate;
            if (!DateTime.TryParse(txtPaidDate.Text, out paidDate)) { ShowError("Please provide a valid paid date."); return; }

            try
            {
                int? userId = null; int uid;
                if (int.TryParse(Convert.ToString(Session["UserID"]), out uid)) userId = uid;

                _repo.MarkPayrollPaid(RecordId, method, txtReference.Text.Trim(), paidDate, userId);
                ShowSuccess("Payment recorded. The record is now marked Paid.");
                LoadRecord();
            }
            catch (Exception ex)
            {
                ShowError(FriendlyError(ex.Message));
            }
        }

        protected void btnMarkFailed_Click(object sender, EventArgs e)
        {
            if (RecordId <= 0) return;
            try
            {
                int? userId = null; int uid;
                if (int.TryParse(Convert.ToString(Session["UserID"]), out uid)) userId = uid;

                _repo.SetPayrollPaymentFailed(RecordId, userId);

                string note = txtFailNote.Text.Trim();
                if (!string.IsNullOrEmpty(note))
                    _repo.UpdatePayrollNotes(RecordId, note, userId);

                ShowSuccess("The payment has been marked as Failed.");
                LoadRecord();
            }
            catch (Exception ex)
            {
                ShowError(FriendlyError(ex.Message));
            }
        }

        #region Helpers

        private string Money(object value)
        {
            return PayrollFormat.Money(value);
        }

        private string StatusStyle(string status)
        {
            switch (status)
            {
                case "Paid": return "background:#DCFCE7;color:#15803D";
                case "Pending": return "background:#FEF3C7;color:#B45309";
                case "Failed": return "background:#FEE2E2;color:#DC2626";
                case "Cancelled": return "background:#F1F5F9;color:#64748B";
                default: return "background:#F1F5F9;color:#64748B";
            }
        }

        private string FriendlyError(string raw)
        {
            if (!string.IsNullOrEmpty(raw) &&
                (raw.IndexOf("Processing", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 raw.IndexOf("Pending", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 raw.IndexOf("Failed", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 raw.IndexOf("required", StringComparison.OrdinalIgnoreCase) >= 0))
                return HttpUtility.HtmlDecode(raw); // repository messages are already safe, user-facing text
            return "The action could not be completed due to a system error. Please try again.";
        }

        private void ShowNotFound()
        {
            pnlBody.Visible = false;
            pnlNotFound.Visible = true;
        }

        private string Enc(object v)
        {
            return HttpUtility.HtmlEncode(v == null || v == DBNull.Value ? "" : v.ToString());
        }

        private void ShowSuccess(string message)
        {
            msg.Visible = true;
            msg.CssClass = "p-3 mb-4 rounded-lg text-sm bg-emerald-50 text-emerald-800 border border-emerald-200";
            msgText.Text = HttpUtility.HtmlEncode(message);
        }

        private void ShowError(string message)
        {
            msg.Visible = true;
            msg.CssClass = "p-3 mb-4 rounded-lg text-sm bg-amber-50 text-amber-800 border border-amber-200";
            msgText.Text = HttpUtility.HtmlEncode(message);
        }

        #endregion
    }
}
