using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Attendance
{
    public partial class ParentAttendance : System.Web.UI.Page
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
                BindChildren();
            }
        }

        private bool Authorize()
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return false; }
            // Parent-only page.
            string r = _repo.NormalizeRole(Role);
            if (r != "parent" && r != "guardian") { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return false; }
            return true;
        }

        protected string StatusStyle(string s) { return AttendanceUi.StatusStyle(s); }
        protected string FormatTime(object t) { return AttendanceUi.FormatTime(t); }

        private int ChildV() { int v; return int.TryParse(ddlChild.SelectedValue, out v) ? v : 0; }
        private void GetMonth(out int y, out int m)
        {
            DateTime d;
            if (DateTime.TryParse((txtMonth.Text ?? "") + "-01", CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) { y = d.Year; m = d.Month; }
            else { y = DateTime.Today.Year; m = DateTime.Today.Month; }
        }

        private void BindChildren()
        {
            DataTable kids = _repo.GetParentLinkedStudents(UserId);
            if (kids.Rows.Count == 0) { pnlNoChildren.Visible = true; pnlBody.Visible = false; return; }
            pnlBody.Visible = true;
            ddlChild.Items.Clear();
            foreach (DataRow r in kids.Rows)
                ddlChild.Items.Add(new ListItem(Convert.ToString(r["FullName"]) + " (" + Convert.ToString(r["StudentCode"]) + ")", Convert.ToString(r["StudentID"])));
            LoadChild();
        }

        protected void ddlChild_Changed(object sender, EventArgs e) { LoadChild(); }
        protected void txtMonth_Changed(object sender, EventArgs e) { LoadChild(); }

        private void LoadChild()
        {
            int studentId = ChildV();
            // Server-side ownership re-check before ANY data retrieval (defeats QueryString/postback tampering).
            if (studentId <= 0 || !_repo.UserOwnsStudent(UserId, studentId))
            {
                pnlBody.Visible = false;
                Show(false, "That student is not linked to your account.");
                return;
            }

            DataTable kids = _repo.GetParentLinkedStudents(UserId);
            foreach (DataRow r in kids.Rows)
                if (Convert.ToInt32(r["StudentID"]) == studentId)
                    litChildInfo.Text = HttpUtility.HtmlEncode(Convert.ToString(r["ClassName"]) + " / " + Convert.ToString(r["SectionName"]) + " · " + Convert.ToString(r["YearName"]));

            DataRow s = _repo.GetParentAttendanceSummary(studentId);
            litPct.Text = Convert.ToDecimal(s["Percentage"]).ToString("0.0") + "%";
            litTotal.Text = Convert.ToString(s["TotalSessions"]); litP.Text = Convert.ToString(s["Present"]);
            litA.Text = Convert.ToString(s["Absent"]); litL.Text = Convert.ToString(s["Late"]); litE.Text = Convert.ToString(s["Excused"]);

            gvRecent.DataSource = _repo.GetParentRecentAttendance(studentId, 15); gvRecent.DataBind();

            DataTable alerts = _repo.GetParentVisibleAlerts(studentId);
            rptAlerts.DataSource = alerts; rptAlerts.DataBind();
            pnlAlerts.Visible = alerts.Rows.Count > 0;

            RenderCalendar(studentId);
        }

        private void RenderCalendar(int studentId)
        {
            int y, m; GetMonth(out y, out m);
            DataTable data = _repo.GetParentAttendanceCalendar(_repo.GetActiveAcademicYearId(), y, m, studentId);
            var byDay = new System.Collections.Generic.Dictionary<int, DataRow>();
            foreach (DataRow r in data.Rows) byDay[Convert.ToDateTime(r["AttendanceDate"]).Day] = r;

            DateTime first = new DateTime(y, m, 1);
            int lead = ((int)first.DayOfWeek + 6) % 7;
            int days = DateTime.DaysInMonth(y, m);
            StringBuilder b = new StringBuilder("<div class='cal'>");
            for (int i = 0; i < lead; i++) b.Append("<div class='cell empty'></div>");
            for (int day = 1; day <= days; day++)
            {
                DataRow r = byDay.ContainsKey(day) ? byDay[day] : null;
                string color = "#e5e7eb", label = "";
                if (r != null)
                {
                    int p = ToInt(r["P"]), a = ToInt(r["A"]), l = ToInt(r["L"]), e = ToInt(r["E"]);
                    if (l > 0) { color = "#D97706"; label = "Late"; }
                    else if (a > 0) { color = "#DC2626"; label = "Absent"; }
                    else if (e > 0) { color = "#7C3AED"; label = "Excused"; }
                    else if (p > 0) { color = "#16A34A"; label = "Present"; }
                }
                b.Append("<div class='cell'><div style='font-weight:700'>").Append(day).Append("</div>");
                b.Append("<div><span class='dot' style='background:").Append(color).Append("'></span> ").Append(HttpUtility.HtmlEncode(label)).Append("</div></div>");
            }
            b.Append("</div>");
            litCalendar.Text = b.ToString();
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
