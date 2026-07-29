using System;
using System.Data;
using System.Web;

namespace AQOONHUB_SMS.Modules.Finance
{
    public partial class FeeManagement : System.Web.UI.Page
    {
        private readonly FeeRepository _repo = new FeeRepository();
        protected void Page_Load(object sender, EventArgs e)
        {
            AuthorizeFinance();
            if (!IsPostBack) LoadData();
        }
        private void AuthorizeFinance()
        {
            if (Session["UserID"] == null) Response.Redirect("~/Modules/Authentication/Login.aspx");
            string role = Convert.ToString(Session["Role"]).Replace(" ", "").ToLowerInvariant();
            if (role != "superadmin" && role != "admin" && role != "accountant" && role != "finance")
                Response.Redirect("~/Default.aspx");
        }
        private void LoadData()
        {
            try
            {
                DataTable summary = _repo.GetDashboardSummary();
                DataRow row = summary.Rows[0];
                litTotalInvoices.Text = Convert.ToInt32(row["TotalInvoices"]).ToString("N0");
                litCollected.Text = Money(row["CollectedThisMonth"]);
                litOutstanding.Text = Money(row["Outstanding"]);
                litOverdue.Text = Convert.ToInt32(row["Overdue"]).ToString("N0");
                litSuccess.Text = Convert.ToDecimal(row["SuccessRate"]).ToString("N2") + "%";
                rptInvoices.DataSource = _repo.GetInvoices(txtSearch.Text.Trim(), ddlStatus.SelectedValue);
                rptInvoices.DataBind();
                rptPayments.DataSource = _repo.GetRecentPayments();
                rptPayments.DataBind();
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                litError.Text = HttpUtility.HtmlEncode("Fee Management is not initialized. Run Database/Scripts/FeeManagementModule.sql. " + ex.Message);
            }
        }
        protected void btnFilter_Click(object sender, EventArgs e) { LoadData(); }
        protected string Money(object value) { return "$" + Convert.ToDecimal(value).ToString("N2"); }
        protected string Date(object value) { return Convert.ToDateTime(value).ToString("dd MMM yyyy"); }
    }
}
