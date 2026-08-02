using System;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Reports
{
    /// <summary>Base for the report-category card pages: server-side authorization + card rendering
    /// through the shared ReportUi/ReportCatalog/ReportAuthorization layer.</summary>
    public abstract class ReportCategoryBase : System.Web.UI.Page
    {
        protected readonly ReportsRepository Repo = new ReportsRepository();
        protected string Role { get { return Convert.ToString(Session["Role"]); } }
        protected abstract string CategoryKey { get; }

        protected bool AuthorizeCategory()
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return false; }
            if (!ReportAuthorization.CanViewCategory(Role, CategoryKey)) { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return false; }
            return true;
        }

        protected void RenderCards(Literal cards, Literal dataSource)
        {
            cards.Text = ReportUi.RenderReportCards(CategoryKey, Role, u => ResolveUrl(u));
            if (dataSource != null && CategoryKey == ReportAuthorization.Library)
                dataSource.Text = "<div class='rounded-lg p-3 mb-4 text-sm bg-amber-50 text-amber-800 border border-amber-200'>Library data source is not present in this build. Report cards are shown as unavailable until a Library module provides book/borrowing data.</div>";
        }
    }
}
