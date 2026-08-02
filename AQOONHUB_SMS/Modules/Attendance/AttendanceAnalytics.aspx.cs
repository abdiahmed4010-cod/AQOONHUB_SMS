using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Attendance
{
    public partial class AttendanceAnalytics : System.Web.UI.Page
    {
        private readonly AttendanceRepository _repo = new AttendanceRepository();
        private string Role { get { return Convert.ToString(Session["Role"]); } }
        private int UserId { get { int v; return int.TryParse(Convert.ToString(Session["UserID"]), out v) ? v : 0; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack)
            {
                txtFrom.Text = new DateTime(DateTime.Today.Year, 1, 1).ToString("yyyy-MM-dd");
                txtTo.Text = DateTime.Today.ToString("yyyy-MM-dd");
                BindYears(); BindTerms(); BindClasses(); BindSections();
                LoadData();
            }
        }

        private bool Authorize()
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return false; }
            if (!_repo.UserCanViewAttendanceAnalytics(Role)) { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return false; }
            return true;
        }

        private int YearV() { int v; return int.TryParse(ddlYear.SelectedValue, out v) ? v : 0; }
        private int TermV() { int v; return int.TryParse(ddlTerm.SelectedValue, out v) ? v : 0; }
        private int ClassV() { int v; return int.TryParse(ddlClass.SelectedValue, out v) ? v : 0; }
        private int SectionV() { int v; return int.TryParse(ddlSection.SelectedValue, out v) ? v : 0; }
        private string TypeV { get { return ddlSessionType.SelectedValue; } }
        private DateTime FromV() { DateTime d; return DateTime.TryParse(txtFrom.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d.Date : DateTime.Today.AddMonths(-6); }
        private DateTime ToV() { DateTime d; return DateTime.TryParse(txtTo.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d.Date : DateTime.Today; }

        private void BindYears()
        {
            ddlYear.Items.Clear();
            foreach (DataRow r in _repo.GetAcademicYears().Rows) ddlYear.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
            int a = _repo.GetActiveAcademicYearId(); if (a > 0 && ddlYear.Items.FindByValue(a.ToString()) != null) ddlYear.SelectedValue = a.ToString();
        }
        private void BindTerms() { ddlTerm.Items.Clear(); ddlTerm.Items.Add(new ListItem("All Terms", "0")); foreach (DataRow r in _repo.GetTerms(YearV()).Rows) ddlTerm.Items.Add(new ListItem(Convert.ToString(r["TermName"]), Convert.ToString(r["TermID"]))); }
        private void BindClasses() { ddlClass.Items.Clear(); ddlClass.Items.Add(new ListItem("All Classes", "0")); foreach (DataRow r in _repo.GetClasses(YearV()).Rows) ddlClass.Items.Add(new ListItem(Convert.ToString(r["ClassName"]), Convert.ToString(r["ClassID"]))); }
        private void BindSections() { ddlSection.Items.Clear(); ddlSection.Items.Add(new ListItem("All Sections", "0")); if (ClassV() > 0) foreach (DataRow r in _repo.GetSectionsForClass(ClassV()).Rows) ddlSection.Items.Add(new ListItem(Convert.ToString(r["SectionName"]), Convert.ToString(r["SectionID"]))); }

        protected void ddlYear_Changed(object sender, EventArgs e) { BindTerms(); BindClasses(); BindSections(); }
        protected void ddlClass_Changed(object sender, EventArgs e) { BindSections(); }
        protected void btnView_Click(object sender, EventArgs e) { LoadData(); }
        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtFrom.Text = new DateTime(DateTime.Today.Year, 1, 1).ToString("yyyy-MM-dd"); txtTo.Text = DateTime.Today.ToString("yyyy-MM-dd");
            ddlSessionType.SelectedIndex = 0; BindYears(); BindTerms(); BindClasses(); BindSections(); LoadData();
        }

        private void LoadData()
        {
            int y = YearV(); int? tm = TermV() > 0 ? TermV() : (int?)null;
            int? c = ClassV() > 0 ? ClassV() : (int?)null; int? sec = SectionV() > 0 ? SectionV() : (int?)null;
            DateTime from = FromV(), to = ToV();

            // Teacher scope: restrict to an assigned section (must pick one they own).
            if (!_repo.CanManageAttendance(Role) && _repo.NormalizeRole(Role) == "teacher")
            {
                if (!(SectionV() > 0 && _repo.UserCanViewAttendanceScope(UserId, Role, ClassV(), SectionV(), null, y)))
                {
                    pnlData.Visible = false; pnlEmpty.Visible = true; ResetCards();
                    Show(false, "Teachers must select an assigned class and section to view analytics.");
                    return;
                }
            }

            DataRow s = _repo.GetAttendanceAnalyticsSummary(y, tm, c, sec, TypeV, null, from, to);
            litRate.Text = Convert.ToDecimal(s["Rate"]).ToString("0.0") + "%";
            litSessions.Text = Convert.ToString(s["Sessions"]); litStudents.Text = Convert.ToString(s["Students"]);
            litP.Text = Convert.ToString(s["Present"]); litA.Text = Convert.ToString(s["Absent"]);
            litL.Text = Convert.ToString(s["Late"]); litE.Text = Convert.ToString(s["Excused"]); litRisk.Text = Convert.ToString(s["AtRisk"]);

            bool hasData = Convert.ToInt32(s["Sessions"]) > 0;
            pnlData.Visible = hasData; pnlEmpty.Visible = !hasData;
            if (!hasData) { litChartData.Text = ""; return; }

            DataTable breakdown = _repo.GetAttendanceStatusBreakdown(y, tm, c, sec, TypeV, null, from, to);
            DataTable trend = _repo.GetWeeklyAttendanceTrend(y, tm, c, sec, TypeV, null, from, to);
            DataTable month = _repo.GetMonthlyAttendanceTrend(y, tm, c, sec, TypeV, null, from, to);
            DataTable byClass = _repo.GetAttendanceByClassAnalytics(y, tm, c, sec, TypeV, null, from, to);

            gvTop.DataSource = _repo.GetTopAttendanceStudents(y, tm, c, sec, TypeV, null, from, to, 8); gvTop.DataBind();
            gvAbsent.DataSource = _repo.GetMostAbsentStudents(y, tm, c, sec, TypeV, null, from, to, 8); gvAbsent.DataBind();
            gvLate.DataSource = _repo.GetFrequentLateStudents(y, tm, c, sec, TypeV, null, from, to, 8); gvLate.DataBind();
            gvRisk.DataSource = _repo.GetAtRiskStudents(y, tm, c, sec, TypeV, null, from, to); gvRisk.DataBind();

            EmitChartData(breakdown, trend, month, byClass);
        }

        private void EmitChartData(DataTable breakdown, DataTable trend, DataTable month, DataTable byClass)
        {
            var breakMap = new System.Collections.Generic.Dictionary<string, int>();
            foreach (DataRow r in breakdown.Rows) breakMap[Convert.ToString(r["AttendanceStatus"])] = Convert.ToInt32(r["Cnt"]);
            string[] order = { "Present", "Absent", "Late", "Excused" };
            var breakData = new System.Collections.Generic.List<int>();
            foreach (string k in order) breakData.Add(breakMap.ContainsKey(k) ? breakMap[k] : 0);

            var data = new
            {
                breakLabels = order,
                breakData = breakData,
                trendLabels = Col(trend, "Label"),
                trendData = ColD(trend, "Rate"),
                monthLabels = Col(month, "Label"),
                monthData = ColD(month, "Rate"),
                classLabels = Col(byClass, "ClassName"),
                classData = ColD(byClass, "Rate")
            };
            var ser = new JavaScriptSerializer();
            litChartData.Text = "<script>window.AN=" + ser.Serialize(data) + ";</script>";
        }

        private static System.Collections.Generic.List<string> Col(DataTable t, string col)
        { var l = new System.Collections.Generic.List<string>(); foreach (DataRow r in t.Rows) l.Add(Convert.ToString(r[col])); return l; }
        private static System.Collections.Generic.List<decimal> ColD(DataTable t, string col)
        { var l = new System.Collections.Generic.List<decimal>(); foreach (DataRow r in t.Rows) l.Add(Convert.ToDecimal(r[col])); return l; }

        private void ResetCards()
        {
            litRate.Text = "0%"; litSessions.Text = litStudents.Text = litP.Text = litA.Text = litL.Text = litE.Text = litRisk.Text = "0"; litChartData.Text = "";
        }

        private void Show(bool ok, string text)
        {
            msg.Visible = true;
            msg.CssClass = "rounded-lg p-3 mb-4 text-sm " + (ok ? "bg-emerald-50 text-emerald-800 border border-emerald-200" : "bg-amber-50 text-amber-800 border border-amber-200");
            msgText.Text = HttpUtility.HtmlEncode(text);
        }
    }
}
