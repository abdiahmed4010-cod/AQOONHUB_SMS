using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Reports
{
    public partial class ReportViewer : System.Web.UI.Page
    {
        private readonly ReportsRepository _repo = new ReportsRepository();
        private string Role { get { return Convert.ToString(Session["Role"]); } }
        private int? UserId { get { int v; return int.TryParse(Convert.ToString(Session["UserID"]), out v) ? v : (int?)null; } }
        private ReportDefinition _def;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return; }

            _def = ReportCatalog.Resolve(Request.QueryString["report"]);
            if (_def == null) { ShowInvalid("The requested report was not found."); return; }
            if (!ReportAuthorization.CanRunReport(Role, _def)) { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return; }
            if (!_def.Available) { ShowInvalid(_def.Description); return; }

            pnlReport.Visible = true;
            ConfigureHeader();
            ConfigureFilterVisibility();

            if (!IsPostBack)
            {
                if (_def.Orientation == "Portrait") litPageCss.Text = "<style>@media print{@page{size:A4 portrait;margin:12mm;}}</style>";
                else litPageCss.Text = "<style>@media print{@page{size:A4 landscape;margin:10mm;}}</style>";
                BindYears(); BindTerms(); BindClasses(); BindSections(); BindSubjects(); BindTeachers(); BindDepartments(); BindStatusOptions(); BindStudents(); BindExams(); BindPeriods();
                RunReport(logView: true);
            }
        }

        private void ShowInvalid(string msg)
        {
            pnlInvalid.Visible = true; pnlReport.Visible = false;
            litInvalid.Text = ReportUi.Enc(msg);
        }

        private void ConfigureHeader()
        {
            litTitle.Text = ReportUi.Enc(_def.Title);
            litPrintTitle.Text = ReportUi.Enc(_def.Title);
            litCrumb.Text = ReportUi.Enc(_def.Title);
            litDesc.Text = ReportUi.Enc(_def.Description);
            string catUrl = ResolveUrl(ReportAuthorization.PageUrl(_def.Category));
            lnkCategory.Text = ReportUi.Enc(ReportAuthorization.Label(_def.Category));
            lnkCategory.NavigateUrl = catUrl;
            lnkBack.NavigateUrl = catUrl;
        }

        private bool Has(string filter) { return Array.IndexOf(_def.Filters, filter) >= 0; }

        private void ConfigureFilterVisibility()
        {
            fYear.Visible = Has("year"); fTerm.Visible = Has("term"); fClass.Visible = Has("class");
            fSection.Visible = Has("section"); fSubject.Visible = Has("subject"); fTeacher.Visible = Has("teacher");
            fExam.Visible = Has("exam"); fPeriod.Visible = Has("period");
            fStudent.Visible = Has("student"); fDept.Visible = Has("department"); fGender.Visible = Has("gender");
            fStatus.Visible = Has("status"); fFrom.Visible = Has("from"); fTo.Visible = Has("to"); fSearch.Visible = Has("search");
            pnlFilters.Visible = _def.Filters.Length > 0;
        }

        // ---- filter binders ----
        private int? Sel(DropDownList d) { int v; return int.TryParse(d.SelectedValue, out v) && v > 0 ? v : (int?)null; }
        private void BindYears()
        {
            ddlYear.Items.Clear(); ddlYear.Items.Add(new ListItem("All Years", "0"));
            foreach (DataRow r in _repo.GetAcademicYears().Rows) ddlYear.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
        }
        private void BindTerms() { ddlTerm.Items.Clear(); ddlTerm.Items.Add(new ListItem("All Terms", "0")); foreach (DataRow r in _repo.GetTerms(Sel(ddlYear)).Rows) ddlTerm.Items.Add(new ListItem(Convert.ToString(r["TermName"]), Convert.ToString(r["TermID"]))); }
        private void BindClasses() { ddlClass.Items.Clear(); ddlClass.Items.Add(new ListItem("All Classes", "0")); foreach (DataRow r in _repo.GetClasses(Sel(ddlYear)).Rows) ddlClass.Items.Add(new ListItem(Convert.ToString(r["ClassName"]), Convert.ToString(r["ClassID"]))); }
        private void BindSections() { ddlSection.Items.Clear(); ddlSection.Items.Add(new ListItem("All Sections", "0")); foreach (DataRow r in _repo.GetSections(Sel(ddlClass)).Rows) ddlSection.Items.Add(new ListItem(Convert.ToString(r["SectionName"]), Convert.ToString(r["SectionID"]))); }
        private void BindSubjects() { ddlSubject.Items.Clear(); ddlSubject.Items.Add(new ListItem("All Subjects", "0")); foreach (DataRow r in _repo.GetSubjects().Rows) ddlSubject.Items.Add(new ListItem(Convert.ToString(r["SubjectName"]), Convert.ToString(r["SubjectID"]))); }
        private void BindTeachers() { ddlTeacher.Items.Clear(); ddlTeacher.Items.Add(new ListItem("All Teachers", "0")); foreach (DataRow r in _repo.GetTeachers().Rows) ddlTeacher.Items.Add(new ListItem(Convert.ToString(r["Name"]), Convert.ToString(r["StaffID"]))); }
        private void BindDepartments() { ddlDept.Items.Clear(); ddlDept.Items.Add(new ListItem("All Departments", "")); foreach (DataRow r in _repo.GetDepartments().Rows) ddlDept.Items.Add(new ListItem(Convert.ToString(r["Department"]), Convert.ToString(r["Department"]))); }
        private void BindStudents() { ddlStudent.Items.Clear(); ddlStudent.Items.Add(new ListItem("— Select student —", "0")); foreach (DataRow r in _repo.GetStudents(Sel(ddlSection)).Rows) ddlStudent.Items.Add(new ListItem(Convert.ToString(r["Name"]), Convert.ToString(r["StudentID"]))); }
        private void BindExams() { ddlExam.Items.Clear(); ddlExam.Items.Add(new ListItem("All Examinations", "0")); foreach (DataRow r in _repo.GetExamsLookup(Sel(ddlYear)).Rows) ddlExam.Items.Add(new ListItem(Convert.ToString(r["ExamName"]), Convert.ToString(r["ExamID"]))); }
        private void BindPeriods() { ddlPeriod.Items.Clear(); ddlPeriod.Items.Add(new ListItem("All Periods", "0")); foreach (DataRow r in _repo.GetPayrollPeriods().Rows) ddlPeriod.Items.Add(new ListItem(Convert.ToString(r["PeriodName"]), Convert.ToString(r["PayrollPeriodID"]))); }
        private void BindStatusOptions()
        {
            ddlStatus.Items.Clear();
            foreach (var s in new[] { "", "Active", "Inactive", "Withdrawn", "Graduated" })
                ddlStatus.Items.Add(new ListItem(s == "" ? "All Status" : s, s));
        }

        protected void ddlYear_Changed(object sender, EventArgs e) { BindTerms(); BindClasses(); BindSections(); BindStudents(); BindExams(); }
        protected void ddlClass_Changed(object sender, EventArgs e) { BindSections(); BindStudents(); }
        protected void ddlSection_Changed(object sender, EventArgs e) { BindStudents(); }
        protected void btnApply_Click(object sender, EventArgs e) { gv.PageIndex = 0; RunReport(logView: false); }
        protected void btnReset_Click(object sender, EventArgs e) { Response.Redirect(Request.RawUrl.Split('?')[0] + "?report=" + HttpUtility.UrlEncode(_def.Key)); }
        protected void gv_PageIndexChanging(object sender, GridViewPageEventArgs e) { gv.PageIndex = e.NewPageIndex; RunReport(logView: false); }

        private ReportFilter CurrentFilter()
        {
            DateTime d;
            var f = new ReportFilter
            {
                YearID = Sel(ddlYear), TermID = Sel(ddlTerm), ClassID = Sel(ddlClass), SectionID = Sel(ddlSection),
                SubjectID = Sel(ddlSubject), StaffID = Sel(ddlTeacher), StudentID = Sel(ddlStudent),
                ExamID = Sel(ddlExam), PeriodID = Sel(ddlPeriod),
                Gender = ddlGender.SelectedValue, Status = ddlStatus.SelectedValue, Department = ddlDept.SelectedValue,
                Search = (txtSearch.Text ?? "").Trim(),
                From = DateTime.TryParse(txtFrom.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d : (DateTime?)null
            };
            f.To = DateTime.TryParse(txtTo.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d : (DateTime?)null;
            return f;
        }

        private string FilterSummary(ReportFilter f)
        {
            var parts = new List<string>();
            if (fYear.Visible && f.YearID.HasValue) parts.Add("Year=" + ddlYear.SelectedItem.Text);
            if (fExam.Visible && f.ExamID.HasValue) parts.Add("Exam=" + ddlExam.SelectedItem.Text);
            if (fPeriod.Visible && f.PeriodID.HasValue) parts.Add("Period=" + ddlPeriod.SelectedItem.Text);
            if (fClass.Visible && f.ClassID.HasValue) parts.Add("Class=" + ddlClass.SelectedItem.Text);
            if (fSection.Visible && f.SectionID.HasValue) parts.Add("Section=" + ddlSection.SelectedItem.Text);
            if (fGender.Visible && f.SafeGender != null) parts.Add("Gender=" + f.SafeGender);
            if (fStatus.Visible && !string.IsNullOrEmpty(f.Status)) parts.Add("Status=" + f.Status);
            if (fFrom.Visible && f.From.HasValue) parts.Add("From=" + f.From.Value.ToString("yyyy-MM-dd"));
            if (fTo.Visible && f.To.HasValue) parts.Add("To=" + f.To.Value.ToString("yyyy-MM-dd"));
            // NOTE: never include free-text search / medical values in audit summary
            return parts.Count > 0 ? string.Join(", ", parts) : "All records";
        }

        private DataTable _current;
        private void RunReport(bool logView)
        {
            ReportFilter f = CurrentFilter();
            bool allowSensitive = ReportAuthorization.AllowSensitive(Role, _def);
            DataTable data = _repo.GetReportData(_def.Handler, f, allowSensitive);
            if (data == null) data = new DataTable();
            _current = data;

            gv.DataSource = data; gv.DataBind();

            string summary = FilterSummary(f);
            litSummary.Text = "<div class='text-xs text-gray-500'><b>" + ReportUi.Enc(ReportAuthorization.Label(_def.Category)) + "</b> · " +
                data.Rows.Count + " record(s) · " + ReportUi.Enc(summary) + " · Generated " + DateTime.Now.ToString("dd MMM yyyy HH:mm") + "</div>";

            if (logView) _repo.LogAudit(UserId, "Viewed", _def.Key, _def.Title, _def.Category, summary, "Success", ClientIp());
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            if (_def == null || !ReportAuthorization.CanRunReport(Role, _def)) { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return; }
            ReportFilter f = CurrentFilter();
            bool allowSensitive = ReportAuthorization.AllowSensitive(Role, _def);
            DataTable data = _repo.GetReportData(_def.Handler, f, allowSensitive) ?? new DataTable();
            var svc = new ReportExportService(_repo);
            svc.Export(Response, _def, data, UserId, FilterSummary(f), ClientIp());
        }

        private string ClientIp()
        {
            string ip = Request.UserHostAddress;
            return string.IsNullOrEmpty(ip) ? null : ip;
        }
    }
}
