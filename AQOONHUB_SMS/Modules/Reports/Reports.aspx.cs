using System;
using System.Data;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace AQOONHUB_SMS.Modules.Reports
{
    public partial class Reports : System.Web.UI.Page
    {
        private readonly ReportsRepository _repo = new ReportsRepository();
        private string Role { get { return Convert.ToString(Session["Role"]); } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack) LoadDashboard();
        }

        private bool Authorize()
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return false; }
            if (!ReportAuthorization.CanAccessReports(Role)) { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return false; }
            return true;
        }

        protected void btnRefresh_Click(object sender, EventArgs e) { LoadDashboard(); }

        private void LoadDashboard()
        {
            lnkCustom.Visible = ReportAuthorization.CanViewCategory(Role, ReportAuthorization.CustomBuilder);

            DataRow s = _repo.GetOverviewSummary(Role);
            litTotal.Text = Convert.ToString(s["TotalGenerated"]);
            litToday.Text = Convert.ToString(s["GeneratedToday"]);
            litSaved.Text = Convert.ToString(s["SavedCount"]);
            litScheduled.Text = Convert.ToString(s["ScheduledCount"]);
            litExports.Text = Convert.ToString(s["RecentExports"]);
            litCategories.Text = Convert.ToString(s["ActiveCategories"]);

            DataTable monthly = _repo.GetMonthlyGeneration();
            bool hasChart = monthly.Rows.Count > 0;
            pnlChart.Visible = hasChart; pnlChartEmpty.Visible = !hasChart;
            if (hasChart) EmitChart(monthly);

            DataTable mostUsed = _repo.GetMostUsedReports(6);
            rptMostUsed.DataSource = mostUsed; rptMostUsed.DataBind();
            pnlMostUsedEmpty.Visible = mostUsed.Rows.Count == 0;

            gvActivity.DataSource = _repo.GetRecentActivity(8); gvActivity.DataBind();
            gvExports.DataSource = _repo.GetRecentExports(8); gvExports.DataBind();
            gvScheduled.DataSource = _repo.GetScheduledPreview(8); gvScheduled.DataBind();
            gvSources.DataSource = _repo.GetDataSourceStatus(); gvSources.DataBind();

            litCategoryCards.Text = BuildCategoryCards();
        }

        private void EmitChart(DataTable monthly)
        {
            var labels = new System.Collections.Generic.List<string>();
            var data = new System.Collections.Generic.List<int>();
            foreach (DataRow r in monthly.Rows)
            {
                labels.Add(Convert.ToDateTime(r["Bucket"]).ToString("MMM yyyy"));
                data.Add(Convert.ToInt32(r["Cnt"]));
            }
            var ser = new JavaScriptSerializer();
            litChartData.Text = "<script>window.RP={labels:" + ser.Serialize(labels) + ",data:" + ser.Serialize(data) + "};</script>";
        }

        /// <summary>Only categories this role may view; only existing pages are clickable (no dead links).</summary>
        private string BuildCategoryCards()
        {
            StringBuilder b = new StringBuilder();
            foreach (string cat in ReportAuthorization.VisibleCategories(Role))
            {
                if (cat == ReportAuthorization.Overview) continue;
                string label = ReportUi.Enc(ReportAuthorization.Label(cat));
                string icon = ReportAuthorization.Icon(cat);
                bool exists = ReportAuthorization.PageExists(cat);
                if (exists)
                {
                    string url = ResolveUrl(ReportAuthorization.PageUrl(cat));
                    b.Append("<a class='cat' href='").Append(HttpUtility.HtmlEncode(url)).Append("'>")
                     .Append("<span class='ic'><i data-lucide='").Append(icon).Append("' class='w-4 h-4'></i></span>")
                     .Append("<span class='nm'>").Append(label).Append("</span></a>");
                }
                else
                {
                    // available soon - not a dead link (no href), clearly disabled
                    b.Append("<span class='cat disabled' title='Coming soon'>")
                     .Append("<span class='ic'><i data-lucide='").Append(icon).Append("' class='w-4 h-4'></i></span>")
                     .Append("<span class='nm'>").Append(label).Append("</span></span>");
                }
            }
            return b.ToString();
        }
    }
}
