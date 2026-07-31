using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Attendance
{
    public partial class StudentAttendanceReport : System.Web.UI.Page
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
                BindYears(); BindTerms(); BindClasses(); BindSections(); BindStudents();
                int qsStudent; if (int.TryParse(Request.QueryString["student"], out qsStudent) && qsStudent > 0) PreselectStudent(qsStudent);
            }
        }

        private bool Authorize()
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return false; }
            if (!_repo.CanViewAttendance(Role)) { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return false; }
            return true;
        }

        private int YearV() { int v; return int.TryParse(ddlYear.SelectedValue, out v) ? v : 0; }
        private int TermV() { int v; return int.TryParse(ddlTerm.SelectedValue, out v) ? v : 0; }
        private int ClassV() { int v; return int.TryParse(ddlClass.SelectedValue, out v) ? v : 0; }
        private int SectionV() { int v; return int.TryParse(ddlSection.SelectedValue, out v) ? v : 0; }
        private int StudentV() { int v; return int.TryParse(ddlStudent.SelectedValue, out v) ? v : 0; }
        private DateTime FromV() { DateTime d; return DateTime.TryParse(txtFrom.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d.Date : DateTime.Today.AddMonths(-6); }
        private DateTime ToV() { DateTime d; return DateTime.TryParse(txtTo.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d.Date : DateTime.Today; }

        protected string StatusStyle(string s) { return AttendanceUi.StatusStyle(s); }
        protected string FormatTime(object t) { return AttendanceUi.FormatTime(t); }

        private void BindYears()
        {
            ddlYear.Items.Clear();
            foreach (DataRow r in _repo.GetAcademicYears().Rows) ddlYear.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
            int a = _repo.GetActiveAcademicYearId(); if (a > 0 && ddlYear.Items.FindByValue(a.ToString()) != null) ddlYear.SelectedValue = a.ToString();
        }
        private void BindTerms() { ddlTerm.Items.Clear(); ddlTerm.Items.Add(new ListItem("All Terms", "0")); foreach (DataRow r in _repo.GetTerms(YearV()).Rows) ddlTerm.Items.Add(new ListItem(Convert.ToString(r["TermName"]), Convert.ToString(r["TermID"]))); }
        private void BindClasses() { ddlClass.Items.Clear(); ddlClass.Items.Add(new ListItem("— Select class —", "0")); foreach (DataRow r in _repo.GetClasses(YearV()).Rows) ddlClass.Items.Add(new ListItem(Convert.ToString(r["ClassName"]), Convert.ToString(r["ClassID"]))); }
        private void BindSections() { ddlSection.Items.Clear(); ddlSection.Items.Add(new ListItem("— Select section —", "0")); if (ClassV() > 0) foreach (DataRow r in _repo.GetSectionsForClass(ClassV()).Rows) ddlSection.Items.Add(new ListItem(Convert.ToString(r["SectionName"]), Convert.ToString(r["SectionID"]))); }
        private void BindStudents()
        {
            ddlStudent.Items.Clear(); ddlStudent.Items.Add(new ListItem("— Select student —", "0"));
            if (SectionV() > 0)
                foreach (DataRow r in _repo.GetEligibleStudents(YearV(), SectionV()).Rows)
                    ddlStudent.Items.Add(new ListItem(Convert.ToString(r["FullName"]) + " (" + Convert.ToString(r["StudentCode"]) + ")", Convert.ToString(r["StudentID"])));
        }

        private void PreselectStudent(int studentId)
        {
            // If a student id was passed, locate its section so the dropdowns populate and select it.
            DataRow row = null;
            DataTable t = _repo.GetStudentBasic(studentId);
            if (t.Rows.Count > 0) row = t.Rows[0];
            if (row == null) return;
            int cls = Convert.ToInt32(row["ClassID"]); int sec = Convert.ToInt32(row["SectionID"]);
            if (ddlClass.Items.FindByValue(cls.ToString()) != null) { ddlClass.SelectedValue = cls.ToString(); BindSections(); }
            if (ddlSection.Items.FindByValue(sec.ToString()) != null) { ddlSection.SelectedValue = sec.ToString(); BindStudents(); }
            if (ddlStudent.Items.FindByValue(studentId.ToString()) != null) ddlStudent.SelectedValue = studentId.ToString();
        }

        protected void ddlYear_Changed(object sender, EventArgs e) { BindTerms(); BindClasses(); BindSections(); BindStudents(); }
        protected void ddlClass_Changed(object sender, EventArgs e) { BindSections(); BindStudents(); }
        protected void ddlSection_Changed(object sender, EventArgs e) { BindStudents(); }
        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtFrom.Text = new DateTime(DateTime.Today.Year, 1, 1).ToString("yyyy-MM-dd"); txtTo.Text = DateTime.Today.ToString("yyyy-MM-dd");
            ddlSessionType.SelectedIndex = 0; BindYears(); BindTerms(); BindClasses(); BindSections(); BindStudents();
            gv.DataSource = null; gv.DataBind(); ResetCards();
        }

        private bool ValidateAndAuthorize(out string err)
        {
            err = null;
            if (StudentV() <= 0) { err = "Select a student."; return false; }
            if (ToV() < FromV()) { err = "The 'To' date cannot be before the 'From' date."; return false; }
            // Teacher may view only students within an assigned scope; managers/registrar unrestricted.
            if (!_repo.CanManageAttendance(Role) && _repo.NormalizeRole(Role) == "teacher")
            {
                if (SectionV() <= 0) { err = "Select the section you are assigned to."; return false; }
                if (!_repo.UserCanViewAttendanceScope(UserId, Role, ClassV(), SectionV(), null, YearV()))
                { err = "You are not authorized to view attendance for this student's class/section."; return false; }
            }
            return true;
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            string err;
            if (!ValidateAndAuthorize(out err)) { Show(false, err); gv.DataSource = null; gv.DataBind(); ResetCards(); return; }
            LoadData();
        }

        private void LoadData()
        {
            int? term = TermV() > 0 ? TermV() : (int?)null;
            string type = ddlSessionType.SelectedValue;
            DataTable rows = _repo.GetStudentAttendanceReport(StudentV(), YearV(), term, FromV(), ToV(), type, null);
            gv.DataSource = rows; gv.DataBind();
            DataRow s = _repo.GetStudentAttendanceSummary(StudentV(), YearV(), term, FromV(), ToV(), type, null);
            litTotal.Text = Convert.ToString(s["TotalSessions"]); litP.Text = Convert.ToString(s["Present"]);
            litA.Text = Convert.ToString(s["Absent"]); litL.Text = Convert.ToString(s["Late"]); litE.Text = Convert.ToString(s["Excused"]);
            litPct.Text = Convert.ToDecimal(s["Percentage"]).ToString("0.0") + "%";
            litHeader.Text = "<div class='text-sm font-semibold'>" + HttpUtility.HtmlEncode(ddlStudent.SelectedItem.Text) + "</div><div class='text-xs text-gray-500'>" +
                HttpUtility.HtmlEncode(FromV().ToString("dd MMM yyyy") + " – " + ToV().ToString("dd MMM yyyy")) + " · Generated " + DateTime.Now.ToString("dd MMM yyyy HH:mm") + "</div>";
        }

        private void ResetCards() { litTotal.Text = litP.Text = litA.Text = litL.Text = litE.Text = "0"; litPct.Text = "0%"; litHeader.Text = ""; }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            string err;
            if (!ValidateAndAuthorize(out err)) { Show(false, err); return; }
            int? term = TermV() > 0 ? TermV() : (int?)null;
            DataTable rows = _repo.GetStudentAttendanceReport(StudentV(), YearV(), term, FromV(), ToV(), ddlSessionType.SelectedValue, null);

            StringBuilder b = new StringBuilder();
            string[] headers = { "Date", "Academic Year", "Class", "Section", "Subject", "Session Type", "Status", "Check-in Time", "Late Minutes", "Remarks", "Marked By" };
            for (int i = 0; i < headers.Length; i++) { b.Append(AttendanceUi.Csv(headers[i])); if (i < headers.Length - 1) b.Append(','); }
            b.AppendLine();
            foreach (DataRow r in rows.Rows)
            {
                string[] cells =
                {
                    Convert.ToDateTime(r["AttendanceDate"]).ToString("yyyy-MM-dd"), Convert.ToString(r["YearName"]), Convert.ToString(r["ClassName"]),
                    Convert.ToString(r["SectionName"]), Convert.ToString(r["SubjectName"]), Convert.ToString(r["SessionType"]), Convert.ToString(r["AttendanceStatus"]),
                    AttendanceUi.FormatTimeCsv(r["CheckInTime"]), r["LateMinutes"] == DBNull.Value ? "" : Convert.ToString(r["LateMinutes"]),
                    Convert.ToString(r["Remarks"]), Convert.ToString(r["MarkedByName"])
                };
                for (int i = 0; i < cells.Length; i++) { b.Append(AttendanceUi.Csv(cells[i])); if (i < cells.Length - 1) b.Append(','); }
                b.AppendLine();
            }
            string code = ddlStudent.SelectedItem.Text;
            AttendanceUi.WriteCsv(Response, "student-attendance-" + AttendanceUi.Slug(code) + ".csv", b.ToString());
        }

        private void Show(bool ok, string text)
        {
            msg.Visible = true;
            msg.CssClass = "rounded-lg p-3 mb-4 text-sm no-print " + (ok ? "bg-emerald-50 text-emerald-800 border border-emerald-200" : "bg-amber-50 text-amber-800 border border-amber-200");
            msgText.Text = HttpUtility.HtmlEncode(text);
        }
    }
}
