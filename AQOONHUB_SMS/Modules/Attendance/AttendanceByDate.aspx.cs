using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Attendance
{
    public partial class AttendanceByDate : System.Web.UI.Page
    {
        private readonly AttendanceRepository _repo = new AttendanceRepository();

        private string Role { get { return Convert.ToString(Session["Role"]); } }
        private int UserId { get { int v; return int.TryParse(Convert.ToString(Session["UserID"]), out v) ? v : 0; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack)
            {
                txtDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                BindYears(); BindTerms(); BindClasses(); BindSections(); BindSubjects();
                if (TryPrefillFromQueryString()) LoadIfAuthorized();
            }
        }

        /// <summary>Optional prefill from calendar day-clicks (server still revalidates scope + permission).</summary>
        private bool TryPrefillFromQueryString()
        {
            int y, cls, sec; DateTime d;
            if (!int.TryParse(Request.QueryString["y"], out y) || y <= 0) return false;
            if (!int.TryParse(Request.QueryString["class"], out cls) || cls <= 0) return false;
            if (!int.TryParse(Request.QueryString["section"], out sec) || sec <= 0) return false;
            if (!DateTime.TryParse(Request.QueryString["date"], CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) return false;
            if (ddlYear.Items.FindByValue(y.ToString()) != null) { ddlYear.SelectedValue = y.ToString(); BindTerms(); BindClasses(); BindSections(); BindSubjects(); }
            string type = Request.QueryString["type"]; if (!string.IsNullOrEmpty(type) && ddlSessionType.Items.FindByValue(type) != null) { ddlSessionType.SelectedValue = type; BindSubjects(); }
            if (ddlClass.Items.FindByValue(cls.ToString()) != null) { ddlClass.SelectedValue = cls.ToString(); BindSections(); BindSubjects(); }
            if (ddlSection.Items.FindByValue(sec.ToString()) != null) ddlSection.SelectedValue = sec.ToString();
            txtDate.Text = d.ToString("yyyy-MM-dd");
            return true;
        }

        private void LoadIfAuthorized()
        {
            string err;
            if (ValidateAndAuthorize(out err)) LoadData(); else Show(false, err);
        }

        private bool Authorize()
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return false; }
            if (!_repo.CanViewAttendance(Role)) { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return false; }
            return true;
        }

        // scope accessors
        private int YearV() { int v; return int.TryParse(ddlYear.SelectedValue, out v) ? v : 0; }
        private int TermV() { int v; return int.TryParse(ddlTerm.SelectedValue, out v) ? v : 0; }
        private int ClassV() { int v; return int.TryParse(ddlClass.SelectedValue, out v) ? v : 0; }
        private int SectionV() { int v; return int.TryParse(ddlSection.SelectedValue, out v) ? v : 0; }
        private int SubjectV() { int v; return int.TryParse(ddlSubject.SelectedValue, out v) ? v : 0; }
        private string SessionTypeV { get { return ddlSessionType.SelectedValue; } }
        private bool IsSubject { get { return string.Equals(SessionTypeV, "Subject", StringComparison.OrdinalIgnoreCase); } }
        private DateTime DateV() { DateTime d; return DateTime.TryParse(txtDate.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d.Date : DateTime.Today; }

        protected string StatusStyle(string s) { return AttendanceUi.StatusStyle(s); }
        protected string SessionStyle(string s) { return AttendanceUi.SessionStyle(s); }
        protected string FormatTime(object t) { return AttendanceUi.FormatTime(t); }

        private void BindYears()
        {
            ddlYear.Items.Clear();
            foreach (DataRow r in _repo.GetAcademicYears().Rows) ddlYear.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
            int a = _repo.GetActiveAcademicYearId(); if (a > 0 && ddlYear.Items.FindByValue(a.ToString()) != null) ddlYear.SelectedValue = a.ToString();
        }
        private void BindTerms()
        {
            ddlTerm.Items.Clear(); ddlTerm.Items.Add(new ListItem("All Terms", "0"));
            foreach (DataRow r in _repo.GetTerms(YearV()).Rows) ddlTerm.Items.Add(new ListItem(Convert.ToString(r["TermName"]), Convert.ToString(r["TermID"])));
        }
        private void BindClasses()
        {
            ddlClass.Items.Clear(); ddlClass.Items.Add(new ListItem("— Select class —", "0"));
            foreach (DataRow r in _repo.GetClasses(YearV()).Rows) ddlClass.Items.Add(new ListItem(Convert.ToString(r["ClassName"]), Convert.ToString(r["ClassID"])));
        }
        private void BindSections()
        {
            ddlSection.Items.Clear(); ddlSection.Items.Add(new ListItem("— Select section —", "0"));
            if (ClassV() > 0) foreach (DataRow r in _repo.GetSectionsForClass(ClassV()).Rows) ddlSection.Items.Add(new ListItem(Convert.ToString(r["SectionName"]), Convert.ToString(r["SectionID"])));
        }
        private void BindSubjects()
        {
            ddlSubject.Items.Clear(); ddlSubject.Items.Add(new ListItem("— Select subject —", "0"));
            ddlSubject.Enabled = IsSubject;
            if (IsSubject && ClassV() > 0) foreach (DataRow r in _repo.GetSubjectsForClass(ClassV(), YearV()).Rows) ddlSubject.Items.Add(new ListItem(Convert.ToString(r["SubjectName"]), Convert.ToString(r["SubjectID"])));
        }

        protected void ddlYear_Changed(object sender, EventArgs e) { BindTerms(); BindClasses(); BindSections(); BindSubjects(); }
        protected void ddlClass_Changed(object sender, EventArgs e) { BindSections(); BindSubjects(); }
        protected void ddlSessionType_Changed(object sender, EventArgs e) { BindSubjects(); }
        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtDate.Text = DateTime.Today.ToString("yyyy-MM-dd"); ddlSessionType.SelectedValue = "Daily"; ddlStatus.SelectedIndex = 0;
            BindYears(); BindTerms(); BindClasses(); BindSections(); BindSubjects();
            gv.DataSource = null; gv.DataBind(); ResetCards();
        }

        private bool ValidateAndAuthorize(out string err)
        {
            err = null;
            if (ClassV() <= 0 || SectionV() <= 0) { err = "Select a class and section."; return false; }
            var sc = new AttendanceRepository.AttendanceScope
            {
                AcademicYearID = YearV(), TermID = TermV() > 0 ? TermV() : (int?)null, AttendanceDate = DateV(),
                ClassID = ClassV(), SectionID = SectionV(), SubjectID = IsSubject && SubjectV() > 0 ? SubjectV() : (int?)null, SessionType = SessionTypeV
            };
            err = _repo.ValidateAttendanceScope(sc);
            if (err != null) return false;
            if (!_repo.UserCanViewAttendanceScope(UserId, Role, ClassV(), SectionV(), IsSubject && SubjectV() > 0 ? SubjectV() : (int?)null, YearV()))
            { err = "You are not authorized to view attendance for this class/section."; return false; }
            return true;
        }

        private bool ManagerViewingDraft { get { return _repo.CanManageAttendance(Role) && string.Equals(ddlStatus.SelectedValue, "Draft", StringComparison.OrdinalIgnoreCase); } }

        protected void btnView_Click(object sender, EventArgs e)
        {
            string err;
            if (!ValidateAndAuthorize(out err)) { Show(false, err); gv.DataSource = null; gv.DataBind(); ResetCards(); return; }
            LoadData();
        }

        private void LoadData()
        {
            int? subj = IsSubject && SubjectV() > 0 ? SubjectV() : (int?)null;
            DataTable rows = _repo.GetAttendanceByDate(YearV(), DateV(), ClassV(), SectionV(), subj, SessionTypeV, ddlStatus.SelectedValue, ManagerViewingDraft);
            gv.DataSource = rows; gv.DataBind();

            DataRow s = _repo.GetAttendanceByDateSummary(YearV(), DateV(), ClassV(), SectionV(), subj, SessionTypeV, ddlStatus.SelectedValue, ManagerViewingDraft);
            litTotal.Text = Convert.ToString(s["TotalStudents"]);
            litP.Text = Convert.ToString(s["Present"]); litA.Text = Convert.ToString(s["Absent"]);
            litL.Text = Convert.ToString(s["Late"]); litE.Text = Convert.ToString(s["Excused"]);
            litRate.Text = Convert.ToDecimal(s["Rate"]).ToString("0.0") + "%";

            litFilterSummary.Text = "<div class='text-xs text-gray-500'>" + HttpUtility.HtmlEncode(
                ddlClass.SelectedItem.Text + " / " + ddlSection.SelectedItem.Text + " · " + DateV().ToString("dd MMM yyyy") +
                " · " + SessionTypeV + (subj.HasValue ? " · " + ddlSubject.SelectedItem.Text : "")) +
                " · Generated " + DateTime.Now.ToString("dd MMM yyyy HH:mm") + "</div>";

            if (_repo.CanManageAttendance(Role) || _repo.CanMarkAttendance(Role))
            {
                lnkOpenMark.Visible = true;
                lnkOpenMark.NavigateUrl = "~/Modules/Attendance/MarkAttendance.aspx";
            }
        }

        private void ResetCards()
        {
            litTotal.Text = litP.Text = litA.Text = litL.Text = litE.Text = "0"; litRate.Text = "0%"; litFilterSummary.Text = "";
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            string err;
            if (!ValidateAndAuthorize(out err)) { Show(false, err); return; }
            int? subj = IsSubject && SubjectV() > 0 ? SubjectV() : (int?)null;
            DataTable rows = _repo.GetAttendanceByDate(YearV(), DateV(), ClassV(), SectionV(), subj, SessionTypeV, ddlStatus.SelectedValue, ManagerViewingDraft);

            StringBuilder b = new StringBuilder();
            string[] headers = { "Student Code", "Admission No", "Student Name", "Status", "Check-in Time", "Late Minutes", "Remarks", "Marked By", "Session Status" };
            for (int i = 0; i < headers.Length; i++) { b.Append(AttendanceUi.Csv(headers[i])); if (i < headers.Length - 1) b.Append(','); }
            b.AppendLine();
            foreach (DataRow r in rows.Rows)
            {
                string[] cells =
                {
                    Convert.ToString(r["StudentCode"]), Convert.ToString(r["AdmissionNo"]), Convert.ToString(r["FullName"]),
                    Convert.ToString(r["AttendanceStatus"]), AttendanceUi.FormatTimeCsv(r["CheckInTime"]),
                    r["LateMinutes"] == DBNull.Value ? "" : Convert.ToString(r["LateMinutes"]),
                    Convert.ToString(r["Remarks"]), Convert.ToString(r["MarkedByName"]), Convert.ToString(r["SessionStatus"])
                };
                for (int i = 0; i < cells.Length; i++) { b.Append(AttendanceUi.Csv(cells[i])); if (i < cells.Length - 1) b.Append(','); }
                b.AppendLine();
            }
            AttendanceUi.WriteCsv(Response, "attendance-by-date-" + DateV().ToString("yyyy-MM-dd") + ".csv", b.ToString());
        }

        private void Show(bool ok, string text)
        {
            msg.Visible = true;
            msg.CssClass = "rounded-lg p-3 mb-4 text-sm no-print " + (ok ? "bg-emerald-50 text-emerald-800 border border-emerald-200" : "bg-amber-50 text-amber-800 border border-amber-200");
            msgText.Text = HttpUtility.HtmlEncode(text);
        }
    }
}
