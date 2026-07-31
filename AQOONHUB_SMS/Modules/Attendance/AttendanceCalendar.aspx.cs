using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Attendance
{
    public partial class AttendanceCalendar : System.Web.UI.Page
    {
        private readonly AttendanceRepository _repo = new AttendanceRepository();

        private string Role { get { return Convert.ToString(Session["Role"]); } }
        private int UserId { get { int v; return int.TryParse(Convert.ToString(Session["UserID"]), out v) ? v : 0; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack)
            {
                txtMonth.Text = DateTime.Today.ToString("yyyy-MM");
                BindYears(); BindClasses(); BindSections(); BindStudents();
            }
        }

        private bool Authorize()
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return false; }
            if (!_repo.CanViewAttendance(Role)) { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return false; }
            return true;
        }

        private int YearV() { int v; return int.TryParse(ddlYear.SelectedValue, out v) ? v : 0; }
        private int ClassV() { int v; return int.TryParse(ddlClass.SelectedValue, out v) ? v : 0; }
        private int SectionV() { int v; return int.TryParse(ddlSection.SelectedValue, out v) ? v : 0; }
        private int StudentV() { int v; return int.TryParse(ddlStudent.SelectedValue, out v) ? v : 0; }
        private string SessionTypeV { get { return ddlSessionType.SelectedValue; } }
        private void GetMonth(out int year, out int month)
        {
            DateTime d;
            if (DateTime.TryParse((txtMonth.Text ?? "") + "-01", CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) { year = d.Year; month = d.Month; }
            else { year = DateTime.Today.Year; month = DateTime.Today.Month; }
        }

        private void BindYears()
        {
            ddlYear.Items.Clear();
            foreach (DataRow r in _repo.GetAcademicYears().Rows) ddlYear.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
            int a = _repo.GetActiveAcademicYearId(); if (a > 0 && ddlYear.Items.FindByValue(a.ToString()) != null) ddlYear.SelectedValue = a.ToString();
        }
        private void BindClasses() { ddlClass.Items.Clear(); ddlClass.Items.Add(new ListItem("— Select class —", "0")); foreach (DataRow r in _repo.GetClasses(YearV()).Rows) ddlClass.Items.Add(new ListItem(Convert.ToString(r["ClassName"]), Convert.ToString(r["ClassID"]))); }
        private void BindSections() { ddlSection.Items.Clear(); ddlSection.Items.Add(new ListItem("— Select section —", "0")); if (ClassV() > 0) foreach (DataRow r in _repo.GetSectionsForClass(ClassV()).Rows) ddlSection.Items.Add(new ListItem(Convert.ToString(r["SectionName"]), Convert.ToString(r["SectionID"]))); }
        private void BindStudents()
        {
            ddlStudent.Items.Clear(); ddlStudent.Items.Add(new ListItem("— Whole section —", "0"));
            if (SectionV() > 0) foreach (DataRow r in _repo.GetEligibleStudents(YearV(), SectionV()).Rows)
                ddlStudent.Items.Add(new ListItem(Convert.ToString(r["FullName"]) + " (" + Convert.ToString(r["StudentCode"]) + ")", Convert.ToString(r["StudentID"])));
        }

        protected void ddlYear_Changed(object sender, EventArgs e) { BindClasses(); BindSections(); BindStudents(); litCalendar.Text = ""; }
        protected void ddlClass_Changed(object sender, EventArgs e) { BindSections(); BindStudents(); litCalendar.Text = ""; }
        protected void ddlSection_Changed(object sender, EventArgs e) { BindStudents(); litCalendar.Text = ""; }
        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtMonth.Text = DateTime.Today.ToString("yyyy-MM"); ddlSessionType.SelectedIndex = 0;
            BindYears(); BindClasses(); BindSections(); BindStudents(); litCalendar.Text = "";
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            if (ClassV() <= 0 || SectionV() <= 0) { Show(false, "Select a class and section."); litCalendar.Text = ""; return; }
            if (!_repo.UserCanViewAttendanceScope(UserId, Role, ClassV(), SectionV(), null, YearV()))
            { Show(false, "You are not authorized to view this class/section."); litCalendar.Text = ""; return; }
            RenderCalendar();
        }

        private void RenderCalendar()
        {
            int year, month; GetMonth(out year, out month);
            bool studentMode = StudentV() > 0;
            DataTable data = _repo.GetAttendanceCalendar(YearV(), year, month, ClassV(), SectionV(), studentMode ? StudentV() : (int?)null, SessionTypeV);

            // index by day
            var byDay = new System.Collections.Generic.Dictionary<int, DataRow>();
            foreach (DataRow r in data.Rows) byDay[Convert.ToDateTime(r["AttendanceDate"]).Day] = r;

            decimal threshold = _repo.LowAttendanceThreshold();
            DateTime first = new DateTime(year, month, 1);
            int lead = ((int)first.DayOfWeek + 6) % 7; // Monday-first offset
            int days = DateTime.DaysInMonth(year, month);

            StringBuilder b = new StringBuilder();
            b.Append("<div class='cal'>");
            for (int i = 0; i < lead; i++) b.Append("<div class='cell empty'></div>");
            for (int day = 1; day <= days; day++)
            {
                DataRow r = byDay.ContainsKey(day) ? byDay[day] : null;
                string color, label;
                DayStatus(r, studentMode, threshold, out color, out label);
                string dateStr = new DateTime(year, month, day).ToString("yyyy-MM-dd");
                string href;
                if (studentMode)
                    href = ResolveUrl("~/Modules/Attendance/StudentAttendanceReport.aspx?student=" + StudentV());
                else
                    href = ResolveUrl("~/Modules/Attendance/AttendanceByDate.aspx?y=" + YearV() + "&class=" + ClassV() + "&section=" + SectionV() + "&date=" + dateStr + "&type=" + HttpUtility.UrlEncode(string.IsNullOrEmpty(SessionTypeV) ? "Daily" : SessionTypeV));

                b.Append("<div class='cell'>");
                if (r != null)
                    b.Append("<a href='").Append(HttpUtility.HtmlEncode(href)).Append("'>");
                b.Append("<div class='dn'>").Append(day).Append("</div>");
                b.Append("<div style='margin-top:.35rem'><span class='dot' style='background:").Append(color).Append("'></span>").Append(HttpUtility.HtmlEncode(label)).Append("</div>");
                if (r != null) b.Append("</a>");
                b.Append("</div>");
            }
            b.Append("</div>");
            litCalendar.Text = b.ToString();
        }

        /// <summary>Documented daily-status rule.
        /// Student mode: Late>Absent>Excused>Present priority.
        /// Class mode: No Session when empty; Red when rate&lt;threshold; Orange when late ratio&gt;=20%;
        /// Green when all present; otherwise Mixed.</summary>
        private void DayStatus(DataRow r, bool studentMode, decimal threshold, out string color, out string label)
        {
            if (r == null) { color = "#e5e7eb"; label = "No Session"; return; }
            int p = ToInt(r["P"]), a = ToInt(r["A"]), l = ToInt(r["L"]), e = ToInt(r["E"]), total = ToInt(r["Total"]);
            if (total == 0) { color = "#e5e7eb"; label = "No Session"; return; }

            if (studentMode)
            {
                if (l > 0) { color = "#D97706"; label = "Late"; }
                else if (a > 0) { color = "#DC2626"; label = "Absent"; }
                else if (e > 0) { color = "#7C3AED"; label = "Excused"; }
                else { color = "#16A34A"; label = "Present"; }
                return;
            }

            decimal rate = _repo.CalculateAttendanceRate(p, a, l, e);
            decimal lateRatio = total > 0 ? (decimal)l / total : 0m;
            if (rate < threshold) { color = "#DC2626"; label = rate.ToString("0") + "%"; }
            else if (lateRatio >= 0.2m) { color = "#D97706"; label = rate.ToString("0") + "%"; }
            else if (a == 0 && l == 0 && e == 0) { color = "#16A34A"; label = "100%"; }
            else { color = "#3B82F6"; label = rate.ToString("0") + "%"; }
        }

        private static int ToInt(object o) { return o == null || o == DBNull.Value ? 0 : Convert.ToInt32(o); }

        private void Show(bool ok, string text)
        {
            msg.Visible = true;
            msg.CssClass = "rounded-lg p-3 mb-4 text-sm " + (ok ? "bg-emerald-50 text-emerald-800 border border-emerald-200" : "bg-amber-50 text-amber-800 border border-amber-200");
            msgText.Text = HttpUtility.HtmlEncode(text);
        }
    }
}
