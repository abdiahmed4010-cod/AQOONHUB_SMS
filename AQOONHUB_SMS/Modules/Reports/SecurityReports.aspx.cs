using System;
using System.Data;

namespace AQOONHUB_SMS.Modules.Reports
{
    public partial class SecurityReports : ReportCategoryBase
    {
        protected override string CategoryKey { get { return ReportAuthorization.Security; } }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthorizeCategory()) return;
            litCards.Text = ReportUi.RenderReportCards(CategoryKey, Role, u => ResolveUrl(u));
            if (!IsPostBack) BindSummary();
        }
        private void BindSummary()
        {
            try
            {
                DataRow r = Repo.GetSecuritySummary();
                litUsers.Text = Convert.ToString(r["TotalUsers"]); litActive.Text = Convert.ToString(r["ActiveUsers"]);
                litInactive.Text = Convert.ToString(r["InactiveUsers"]); litFailed.Text = Convert.ToString(r["FailedLogins"]);
            }
            catch { pnlError.Visible = true; litError.Text = "Security summary is temporarily unavailable. Report cards remain available according to your permissions."; }
        }
    }
}
