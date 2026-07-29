using System;
using System.Data;
using System.Text;

namespace AQOONHUB_SMS.Modules.Academic
{
    public partial class Academics : System.Web.UI.Page
    {
        private readonly AcademicsRepository _repo = new AcademicsRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack) LoadOverview();
        }

        private bool Authorize()
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx", true);
                return false;
            }
            if (!_repo.CanView(Convert.ToString(Session["Role"])))
            {
                Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true);
                return false;
            }
            return true;
        }

        private void LoadOverview()
        {
            DataRow s = _repo.GetAcademicsSummary();
            litActiveYear.Text = s["ActiveYear"] == DBNull.Value ? "—" : Server.HtmlEncode(Convert.ToString(s["ActiveYear"]));
            litClasses.Text = Convert.ToString(s["TotalClasses"]);
            litSections.Text = Convert.ToString(s["TotalSections"]);
            litSubjects.Text = Convert.ToString(s["TotalSubjects"]);
            litTeachers.Text = Convert.ToString(s["ActiveTeachers"]);

            BuildDistribution();

            DataTable ev = _repo.GetUpcomingEvents();
            if (ev.Rows.Count == 0) { pnlNoEvents.Visible = true; }
            else { rptEvents.DataSource = ev; rptEvents.DataBind(); }
        }

        private void BuildDistribution()
        {
            DataTable dt = _repo.GetStudentDistributionByClass(_repo.GetActiveAcademicYearId());
            long max = 1;
            foreach (DataRow r in dt.Rows)
            {
                long c = Convert.ToInt64(r["StudentCount"]);
                if (c > max) max = c;
            }
            bool any = false;
            StringBuilder b = new StringBuilder();
            foreach (DataRow r in dt.Rows)
            {
                long c = Convert.ToInt64(r["StudentCount"]);
                if (c > 0) any = true;
                int pct = (int)Math.Round(c * 100.0 / max);
                b.Append("<div class=\"bar-row\">")
                 .Append("<span class=\"text-xs text-gray-600 truncate\">").Append(Server.HtmlEncode(Convert.ToString(r["ClassName"]))).Append("</span>")
                 .Append("<div class=\"bar-track\"><div class=\"bar-fill\" style=\"width:").Append(pct).Append("%\"></div></div>")
                 .Append("<span class=\"text-xs font-semibold text-right\">").Append(c).Append("</span></div>");
            }
            litDistribution.Text = b.ToString();
            pnlNoDist.Visible = !any && dt.Rows.Count == 0;
        }
    }
}
