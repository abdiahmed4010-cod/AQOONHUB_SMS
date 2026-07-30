using System;
using System.Data;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Examinations
{
    public partial class Examinations : System.Web.UI.Page
    {
        private readonly ExaminationsRepository _repo = new ExaminationsRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack)
            {
                BindYears();
                BindTerms();
                LoadAll();
            }
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

        protected string StatusStyle(string status)
        {
            switch ((status ?? "").ToLowerInvariant())
            {
                case "published": return "background:#CCFBF1;color:#0F766E";
                case "completed": return "background:#DCFCE7;color:#15803D";
                case "ongoing": return "background:#FEF3C7;color:#B45309";
                case "scheduled": return "background:#DBEAFE;color:#2563EB";
                case "cancelled": return "background:#FEE2E2;color:#DC2626";
                default: return "background:#F1F5F9;color:#64748B"; // Draft
            }
        }

        private int? YearF() { int v; return int.TryParse(ddlYear.SelectedValue, out v) && v > 0 ? v : (int?)null; }
        private int? TermF() { int v; return int.TryParse(ddlTerm.SelectedValue, out v) && v > 0 ? v : (int?)null; }

        private void BindYears()
        {
            ddlYear.Items.Clear();
            ddlYear.Items.Add(new ListItem("All Years", ""));
            foreach (DataRow r in _repo.GetAcademicYears().Rows)
                ddlYear.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
            int active = _repo.GetActiveAcademicYearId();
            if (active > 0 && ddlYear.Items.FindByValue(active.ToString()) != null) ddlYear.SelectedValue = active.ToString();
        }

        private void BindTerms()
        {
            ddlTerm.Items.Clear();
            ddlTerm.Items.Add(new ListItem("All Terms", ""));
            foreach (DataRow r in _repo.GetTerms(YearF()).Rows)
                ddlTerm.Items.Add(new ListItem(Convert.ToString(r["TermName"]), Convert.ToString(r["TermID"])));
        }

        protected void ddlYear_Changed(object sender, EventArgs e) { BindTerms(); LoadAll(); }
        protected void btnFilter_Click(object sender, EventArgs e) { LoadAll(); }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            int active = _repo.GetActiveAcademicYearId();
            if (active > 0 && ddlYear.Items.FindByValue(active.ToString()) != null) ddlYear.SelectedValue = active.ToString();
            BindTerms(); ddlStatus.SelectedIndex = 0;
            LoadAll();
        }

        private void LoadAll()
        {
            DataRow s = _repo.GetSummary(YearF(), TermF());
            litTotal.Text = Convert.ToString(s["TotalExams"]);
            litUpcoming.Text = Convert.ToString(s["UpcomingExams"]);
            litCompleted.Text = Convert.ToString(s["CompletedExams"]);
            litPending.Text = Convert.ToString(s["PendingMarkEntry"]);
            litPublished.Text = Convert.ToString(s["ResultsPublished"]);

            gvExams.DataSource = _repo.GetExaminations(YearF(), TermF(), null, ddlStatus.SelectedValue, "");
            gvExams.DataBind();

            gvRooms.DataSource = _repo.GetExamRooms();
            gvRooms.DataBind();

            DataTable act = _repo.GetExamActivities(6);
            if (act.Rows.Count == 0) pnlNoActivity.Visible = true;
            else { rptActivity.DataSource = act; rptActivity.DataBind(); }
        }
    }
}
