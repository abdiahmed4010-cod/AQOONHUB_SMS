using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Attendance
{
    public partial class ClassAttendanceReport : System.Web.UI.Page
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
                BindYears(); BindTerms(); BindClasses(); BindSections(); BindSubjects();
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
        private int SubjectV() { int v; return int.TryParse(ddlSubject.SelectedValue, out v) ? v : 0; }
        private string SessionTypeV { get { return ddlSessionType.SelectedValue; } }
        private bool IsSubject { get { return string.Equals(SessionTypeV, "Subject", StringComparison.OrdinalIgnoreCase); } }
        private DateTime FromV() { DateTime d; return DateTime.TryParse(txtFrom.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d.Date : DateTime.Today.AddMonths(-6); }
        private DateTime ToV() { DateTime d; return DateTime.TryParse(txtTo.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d.Date : DateTime.Today; }

        protected string RiskStyle(string s) { return AttendanceUi.RiskStyle(s); }

        private void BindYears()
        {
            ddlYear.Items.Clear();
            foreach (DataRow r in _repo.GetAcademicYears().Rows) ddlYear.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
            int a = _repo.GetActiveAcademicYearId(); if (a > 0 && ddlYear.Items.FindByValue(a.ToString()) != null) ddlYear.SelectedValue = a.ToString();
        }
        private void BindTerms() { ddlTerm.Items.Clear(); ddlTerm.Items.Add(new ListItem("All Terms", "0")); foreach (DataRow r in _repo.GetTerms(YearV()).Rows) ddlTerm.Items.Add(new ListItem(Convert.ToString(r["TermName"]), Convert.ToString(r["TermID"]))); }
        private void BindClasses() { ddlClass.Items.Clear(); ddlClass.Items.Add(new ListItem("— Select class —", "0")); foreach (DataRow r in _repo.GetClasses(YearV()).Rows) ddlClass.Items.Add(new ListItem(Convert.ToString(r["ClassName"]), Convert.ToString(r["ClassID"]))); }
        private void BindSections() { ddlSection.Items.Clear(); ddlSection.Items.Add(new ListItem("— Select section —", "0")); if (ClassV() > 0) foreach (DataRow r in _repo.GetSectionsForClass(ClassV()).Rows) ddlSection.Items.Add(new ListItem(Convert.ToString(r["SectionName"]), Convert.ToString(r["SectionID"]))); }
        private void BindSubjects()
        {
            ddlSubject.Items.Clear(); ddlSubject.Items.Add(new ListItem("— All subjects —", "0")); ddlSubject.Enabled = IsSubject;
            if (IsSubject && ClassV() > 0) foreach (DataRow r in _repo.GetSubjectsForClass(ClassV(), YearV()).Rows) ddlSubject.Items.Add(new ListItem(Convert.ToString(r["SubjectName"]), Convert.ToString(r["SubjectID"])));
        }

        protected void ddlYear_Changed(object sender, EventArgs e) { BindTerms(); BindClasses(); BindSections(); BindSubjects(); }
        protected void ddlClass_Changed(object sender, EventArgs e) { BindSections(); BindSubjects(); }
        protected void ddlSessionType_Changed(object sender, EventArgs e) { BindSubjects(); }
        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtFrom.Text = new DateTime(DateTime.Today.Year, 1, 1).ToString("yyyy-MM-dd"); txtTo.Text = DateTime.Today.ToString("yyyy-MM-dd");
            ddlSessionType.SelectedIndex = 0; BindYears(); BindTerms(); BindClasses(); BindSections(); BindSubjects();
            gv.DataSource = null; gv.DataBind(); ResetCards();
        }

        private bool ValidateAndAuthorize(out string err)
        {
            err = null;
            if (ClassV() <= 0 || SectionV() <= 0) { err = "Select a class and section."; return false; }
            if (ToV() < FromV()) { err = "The 'To' date cannot be before the 'From' date."; return false; }
            if (!_repo.UserCanViewAttendanceScope(UserId, Role, ClassV(), SectionV(), IsSubject && SubjectV() > 0 ? SubjectV() : (int?)null, YearV()))
            { err = "You are not authorized to view attendance for this class/section."; return false; }
            return true;
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            string err;
            if (!ValidateAndAuthorize(out err)) { Show(false, err); gv.DataSource = null; gv.DataBind(); ResetCards(); return; }
            LoadData();
        }

        private DataTable CurrentRows()
        {
            int? term = TermV() > 0 ? TermV() : (int?)null;
            int? subj = IsSubject && SubjectV() > 0 ? SubjectV() : (int?)null;
            return _repo.GetClassAttendanceReport(YearV(), term, ClassV(), SectionV(), FromV(), ToV(), SessionTypeV, subj);
        }

        private void LoadData()
        {
            gv.DataSource = CurrentRows(); gv.DataBind();
            int? term = TermV() > 0 ? TermV() : (int?)null;
            int? subj = IsSubject && SubjectV() > 0 ? SubjectV() : (int?)null;
            DataRow s = _repo.GetClassAttendanceSummary(YearV(), term, ClassV(), SectionV(), FromV(), ToV(), SessionTypeV, subj);
            litSessions.Text = Convert.ToString(s["Sessions"]);
            litAvg.Text = Convert.ToDecimal(s["AverageRate"]).ToString("0.0") + "%";
            litP.Text = Convert.ToString(s["Present"]); litA.Text = Convert.ToString(s["Absent"]);
            litL.Text = Convert.ToString(s["Late"]); litE.Text = Convert.ToString(s["Excused"]);
            litHeader.Text = "<div class='text-sm font-semibold'>" + HttpUtility.HtmlEncode(ddlClass.SelectedItem.Text + " / " + ddlSection.SelectedItem.Text) + "</div><div class='text-xs text-gray-500'>" +
                HttpUtility.HtmlEncode(FromV().ToString("dd MMM yyyy") + " – " + ToV().ToString("dd MMM yyyy")) + " · Generated " + DateTime.Now.ToString("dd MMM yyyy HH:mm") + "</div>";
        }

        private void ResetCards() { litSessions.Text = litP.Text = litA.Text = litL.Text = litE.Text = "0"; litAvg.Text = "0%"; litHeader.Text = ""; }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            string err;
            if (!ValidateAndAuthorize(out err)) { Show(false, err); return; }
            DataTable rows = CurrentRows();
            StringBuilder b = new StringBuilder();
            string[] headers = { "Student Code", "Student Name", "Total Sessions", "Present", "Absent", "Late", "Excused", "Attendance %", "Risk Status" };
            for (int i = 0; i < headers.Length; i++) { b.Append(AttendanceUi.Csv(headers[i])); if (i < headers.Length - 1) b.Append(','); }
            b.AppendLine();
            foreach (DataRow r in rows.Rows)
            {
                string[] cells =
                {
                    Convert.ToString(r["StudentCode"]), Convert.ToString(r["FullName"]), Convert.ToString(r["TotalSessions"]),
                    Convert.ToString(r["Present"]), Convert.ToString(r["Absent"]), Convert.ToString(r["Late"]), Convert.ToString(r["Excused"]),
                    Convert.ToDecimal(r["Percentage"]).ToString("0.0"), Convert.ToString(r["Risk"])
                };
                for (int i = 0; i < cells.Length; i++) { b.Append(AttendanceUi.Csv(cells[i])); if (i < cells.Length - 1) b.Append(','); }
                b.AppendLine();
            }
            AttendanceUi.WriteCsv(Response, "class-attendance-" + AttendanceUi.Slug(ddlSection.SelectedItem.Text) + ".csv", b.ToString());
        }

        private void Show(bool ok, string text)
        {
            msg.Visible = true;
            msg.CssClass = "rounded-lg p-3 mb-4 text-sm no-print " + (ok ? "bg-emerald-50 text-emerald-800 border border-emerald-200" : "bg-amber-50 text-amber-800 border border-amber-200");
            msgText.Text = HttpUtility.HtmlEncode(text);
        }
    }
}
