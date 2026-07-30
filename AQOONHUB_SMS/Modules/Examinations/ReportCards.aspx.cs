using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Examinations
{
    public partial class ReportCards : System.Web.UI.Page
    {
        private readonly ExaminationsRepository _repo = new ExaminationsRepository();

        public int CurrentExamId { get { int v; return int.TryParse(ddlExam.SelectedValue, out v) ? v : 0; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack) { BindYears(); BindTerms(); BindExams(); BindSections(); LoadCards(); }
        }

        private bool Authorize()
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return false; }
            if (!_repo.CanView(Convert.ToString(Session["Role"]))) { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return false; }
            return true;
        }

        private int YearV() { int v; return int.TryParse(ddlYear.SelectedValue, out v) ? v : 0; }
        private int TermV() { int v; return int.TryParse(ddlTerm.SelectedValue, out v) ? v : 0; }
        private int SectionV() { int v; return int.TryParse(ddlSection.SelectedValue, out v) ? v : 0; }

        protected string PublicationLabel() { return _published ? "Published" : "Not Published"; }
        protected string PubStyle() { return _published ? "background:#DCFCE7;color:#15803D" : "background:#F1F5F9;color:#64748B"; }
        private bool _published;

        protected string StatusStyle(string s)
        {
            switch ((s ?? "").ToLowerInvariant())
            {
                case "passed": return "background:#DCFCE7;color:#15803D";
                case "failed": return "background:#FEE2E2;color:#DC2626";
                case "incomplete": return "background:#FEF3C7;color:#B45309";
                case "withheld": return "background:#F1F5F9;color:#64748B";
                case "absent": return "background:#FEE2E2;color:#B91C1C";
                default: return "background:#F1F5F9;color:#64748B";
            }
        }

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
        private void BindExams()
        {
            ddlExam.Items.Clear(); ddlExam.Items.Add(new ListItem("— Select exam —", "0"));
            foreach (DataRow r in _repo.GetExaminations(YearV() > 0 ? YearV() : (int?)null, TermV() > 0 ? TermV() : (int?)null, null, "", "").Rows)
                ddlExam.Items.Add(new ListItem(Convert.ToString(r["ExamName"]), Convert.ToString(r["ExamID"])));
        }
        private void BindSections()
        {
            ddlSection.Items.Clear(); ddlSection.Items.Add(new ListItem("All Sections", "0"));
            if (CurrentExamId > 0)
                foreach (DataRow r in _repo.GetClassesForExam(CurrentExamId).Rows)
                    foreach (DataRow s in _repo.GetSectionsForClass(Convert.ToInt32(r["ClassID"])).Rows)
                        ddlSection.Items.Add(new ListItem(Convert.ToString(r["ClassName"]) + " / " + Convert.ToString(s["SectionName"]), Convert.ToString(s["SectionID"])));
        }

        protected void ddlYear_Changed(object sender, EventArgs e) { BindTerms(); BindExams(); BindSections(); LoadCards(); }
        protected void ddlTerm_Changed(object sender, EventArgs e) { BindExams(); BindSections(); LoadCards(); }
        protected void ddlExam_Changed(object sender, EventArgs e) { BindSections(); LoadCards(); }
        protected void btnFilter_Click(object sender, EventArgs e) { LoadCards(); }
        protected void btnReset_Click(object sender, EventArgs e) { BindYears(); BindTerms(); BindExams(); BindSections(); txtSearch.Text = ""; LoadCards(); }

        private void LoadCards()
        {
            if (CurrentExamId <= 0) { gvCards.DataSource = null; gvCards.DataBind(); _published = false; return; }
            _published = _repo.ResultsArePublished(CurrentExamId);
            var all = _repo.ComputeResults(CurrentExamId, SectionV() > 0 ? SectionV() : (int?)null);
            string q = (txtSearch.Text ?? "").Trim();
            if (q.Length > 0)
                all = all.FindAll(r => (r.FullName ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                    || (r.StudentCode ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
            gvCards.DataSource = all; gvCards.DataBind();
        }
    }
}
