using System;
using System.Data;
using System.Globalization;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Attendance
{
    public partial class Attendance : System.Web.UI.Page
    {
        private readonly AttendanceRepository _repo = new AttendanceRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack)
            {
                txtDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                BindYears(); BindClasses(); BindSections();
                LoadDashboard();
            }
        }

        private bool Authorize()
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return false; }
            if (!_repo.CanViewAttendance(Convert.ToString(Session["Role"]))) { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return false; }
            return true;
        }

        private int YearV() { int v; return int.TryParse(ddlYear.SelectedValue, out v) ? v : 0; }
        private int ClassV() { int v; return int.TryParse(ddlClass.SelectedValue, out v) ? v : 0; }
        private int SectionV() { int v; return int.TryParse(ddlSection.SelectedValue, out v) ? v : 0; }
        private DateTime DateV()
        {
            DateTime d;
            return DateTime.TryParse(txtDate.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d.Date : DateTime.Today;
        }

        protected string StatusStyle(string s)
        {
            switch ((s ?? "").ToLowerInvariant())
            {
                case "present": return "background:#DCFCE7;color:#15803D";
                case "absent": return "background:#FEE2E2;color:#DC2626";
                case "late": return "background:#FEF3C7;color:#B45309";
                case "excused": return "background:#EDE9FE;color:#6D28D9";
                default: return "background:#F1F5F9;color:#64748B";
            }
        }

        protected string FormatTime(object t)
        {
            if (t == null || t == DBNull.Value) return "—";
            TimeSpan ts = (TimeSpan)t;
            return new DateTime(ts.Ticks).ToString("hh:mm tt", CultureInfo.InvariantCulture);
        }

        private void BindYears()
        {
            ddlYear.Items.Clear();
            foreach (DataRow r in _repo.GetAcademicYears().Rows)
                ddlYear.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
            int a = _repo.GetActiveAcademicYearId();
            if (a > 0 && ddlYear.Items.FindByValue(a.ToString()) != null) ddlYear.SelectedValue = a.ToString();
        }

        private void BindClasses()
        {
            ddlClass.Items.Clear();
            ddlClass.Items.Add(new ListItem("All Classes", "0"));
            foreach (DataRow r in _repo.GetClasses(YearV()).Rows)
                ddlClass.Items.Add(new ListItem(Convert.ToString(r["ClassName"]), Convert.ToString(r["ClassID"])));
        }

        private void BindSections()
        {
            ddlSection.Items.Clear();
            ddlSection.Items.Add(new ListItem("All Sections", "0"));
            if (ClassV() > 0)
                foreach (DataRow r in _repo.GetSectionsForClass(ClassV()).Rows)
                    ddlSection.Items.Add(new ListItem(Convert.ToString(r["SectionName"]), Convert.ToString(r["SectionID"])));
        }

        protected void ddlYear_Changed(object sender, EventArgs e) { BindClasses(); BindSections(); LoadDashboard(); }
        protected void ddlClass_Changed(object sender, EventArgs e) { BindSections(); LoadDashboard(); }
        protected void btnFilter_Click(object sender, EventArgs e) { LoadDashboard(); }
        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
            BindYears(); BindClasses(); BindSections(); LoadDashboard();
        }

        private void LoadDashboard()
        {
            int year = YearV();
            DateTime date = DateV();
            int? cls = ClassV() > 0 ? ClassV() : (int?)null;
            int? sec = SectionV() > 0 ? SectionV() : (int?)null;

            DataRow s = _repo.GetAttendanceDashboardSummary(year, date);
            litTotal.Text = Convert.ToString(s["TotalStudents"]);
            litPresent.Text = Convert.ToString(s["PresentToday"]);
            litAbsent.Text = Convert.ToString(s["AbsentToday"]);
            litLate.Text = Convert.ToString(s["LateToday"]);
            litRate.Text = Convert.ToDecimal(s["AttendanceRate"]).ToString("0.0") + "%";

            litBP.Text = Convert.ToString(s["PresentToday"]);
            litBA.Text = Convert.ToString(s["AbsentToday"]);
            litBL.Text = Convert.ToString(s["LateToday"]);
            litBE.Text = Convert.ToString(s["ExcusedToday"]);

            gvToday.DataSource = _repo.GetTodayAttendance(year, date, cls, sec, null);
            gvToday.DataBind();

            gvByClass.DataSource = _repo.GetAttendanceByClass(year, date);
            gvByClass.DataBind();

            DataTable act = _repo.GetRecentAttendanceActivity(year, 8);
            rptActivity.DataSource = act; rptActivity.DataBind();
            pnlNoActivity.Visible = act.Rows.Count == 0;
        }
    }
}
