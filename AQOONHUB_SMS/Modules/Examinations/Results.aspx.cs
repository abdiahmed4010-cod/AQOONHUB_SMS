using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Examinations
{
    public partial class Results : System.Web.UI.Page
    {
        private readonly ExaminationsRepository _repo = new ExaminationsRepository();

        public int CurrentExamId { get { int v; return int.TryParse(ddlExam.SelectedValue, out v) ? v : 0; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack)
            {
                BindYears(); BindTerms(); BindExams(); BindSections();
                LoadResults();
            }
        }

        private bool Authorize()
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return false; }
            // management + teacher/registrar view
            if (!_repo.CanView(Convert.ToString(Session["Role"]))) { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return false; }
            return true;
        }

        private bool CanManage { get { return _repo.CanManage(Convert.ToString(Session["Role"])); } }
        private int UserId { get { int v; return int.TryParse(Convert.ToString(Session["UserID"]), out v) ? v : 0; } }
        private int YearV() { int v; return int.TryParse(ddlYear.SelectedValue, out v) ? v : 0; }
        private int TermV() { int v; return int.TryParse(ddlTerm.SelectedValue, out v) ? v : 0; }
        private int SectionV() { int v; return int.TryParse(ddlSection.SelectedValue, out v) ? v : 0; }

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

        protected void ddlYear_Changed(object sender, EventArgs e) { BindTerms(); BindExams(); BindSections(); LoadResults(); }
        protected void ddlTerm_Changed(object sender, EventArgs e) { BindExams(); BindSections(); LoadResults(); }
        protected void ddlExam_Changed(object sender, EventArgs e) { BindSections(); LoadResults(); }
        protected void btnFilter_Click(object sender, EventArgs e) { LoadResults(); }
        protected void btnCalculate_Click(object sender, EventArgs e) { LoadResults(); Show(true, "Results calculated (preview). Publishing remains a separate action."); }
        protected void btnReset_Click(object sender, EventArgs e) { BindYears(); BindTerms(); BindExams(); BindSections(); ddlStatus.SelectedIndex = 0; LoadResults(); }

        private List<ExaminationsRepository.StudentResult> Current()
        {
            var all = _repo.ComputeResults(CurrentExamId, SectionV() > 0 ? SectionV() : (int?)null);
            string st = ddlStatus.SelectedValue;
            if (!string.IsNullOrEmpty(st)) all = all.FindAll(r => string.Equals(r.ResultStatus, st, StringComparison.OrdinalIgnoreCase));
            return all;
        }

        private void LoadResults()
        {
            btnUnpublish.Visible = false; btnPublish.Enabled = true;
            if (CurrentExamId <= 0)
            {
                gvResults.DataSource = null; gvResults.DataBind();
                litEvaluated.Text = litReady.Text = litIncomplete.Text = "0"; litPassRate.Text = "0%"; litTop.Text = "—"; litPublished.Text = "No";
                return;
            }

            var results = Current();
            gvResults.DataSource = results; gvResults.DataBind();

            int evaluated = results.Count, ready = 0, incomplete = 0, passed = 0, complete = 0;
            decimal top = 0;
            foreach (var r in results)
            {
                if (r.Complete) { ready++; complete++; if (r.Average > top) top = r.Average; }
                if (r.ResultStatus == "Incomplete") incomplete++;
                if (r.ResultStatus == "Passed") passed++;
            }
            litEvaluated.Text = evaluated.ToString();
            litReady.Text = ready.ToString();
            litIncomplete.Text = incomplete.ToString();
            litPassRate.Text = complete > 0 ? Math.Round(passed * 100.0 / complete, 1) + "%" : "0%";
            litTop.Text = top > 0 ? top.ToString("0.00") + "%" : "—";

            bool published = _repo.ResultsArePublished(CurrentExamId);
            litPublished.Text = published ? "Yes" : "No";
            btnPublish.Enabled = CanManage && !published;
            btnUnpublish.Visible = CanManage && published;
        }

        protected void btnPublish_Click(object sender, EventArgs e)
        {
            if (!CanManage) { Show(false, "You are not authorized to publish results."); return; }
            if (CurrentExamId <= 0) { Show(false, "Select an examination."); return; }
            try { _repo.PublishResults(CurrentExamId, UserId); Show(true, "Results published. The examination is now Published and locked."); BindExams(); LoadResults(); }
            catch (Exception ex) { Show(false, ex.Message); LoadResults(); }
        }

        protected void btnUnpublish_Click(object sender, EventArgs e)
        {
            try { _repo.UnpublishResults(CurrentExamId, "Unpublished for correction", UserId, Convert.ToString(Session["Role"])); Show(true, "Results unpublished. The examination returned to Completed."); BindExams(); LoadResults(); }
            catch (Exception ex) { Show(false, ex.Message); LoadResults(); }
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            if (CurrentExamId <= 0) { Show(false, "Select an examination to export."); return; }
            DataRow ex = _repo.GetExamination(CurrentExamId);
            string examName = ex != null ? Convert.ToString(ex["ExamName"]) : "";
            string yearName = ex != null ? Convert.ToString(ex["YearName"]) : "";
            string termName = ex != null ? Convert.ToString(ex["TermName"]) : "";
            bool published = _repo.ResultsArePublished(CurrentExamId);

            StringBuilder b = new StringBuilder();
            string[] headers = { "Student Code", "Student Name", "Academic Year", "Term", "Examination", "Class", "Section", "Total Obtained", "Total Maximum", "Average %", "Overall Grade", "Rank", "Result Status", "Publication Status" };
            foreach (string h in headers) b.Append(Csv(h)).Append(',');
            b.AppendLine();
            foreach (var r in Current())
            {
                b.Append(Csv(r.StudentCode)).Append(',').Append(Csv(r.FullName)).Append(',').Append(Csv(yearName)).Append(',').Append(Csv(termName)).Append(',')
                 .Append(Csv(examName)).Append(',').Append(Csv(r.ClassName)).Append(',').Append(Csv(r.SectionName)).Append(',')
                 .Append(Csv(r.TotalObtained.ToString("0.##"))).Append(',').Append(Csv(r.TotalMaximum.ToString())).Append(',')
                 .Append(Csv(r.Average.ToString("0.00"))).Append(',').Append(Csv(r.Grade)).Append(',').Append(Csv(r.Rank > 0 ? r.Rank.ToString() : "")).Append(',')
                 .Append(Csv(r.ResultStatus)).Append(',').Append(Csv(published ? "Published" : "Not Published")).AppendLine();
            }
            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment; filename=results.csv");
            Response.Write(b.ToString());
            Response.End();
        }

        private static string Csv(string value)
        {
            string s = value ?? string.Empty;
            if (s.Length > 0 && "=+-@\t\r".IndexOf(s[0]) >= 0) s = "'" + s;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }

        private void Show(bool ok, string text)
        {
            msg.Visible = true;
            msg.CssClass = "rounded-lg p-3 mb-4 text-sm " + (ok
                ? "bg-emerald-50 text-emerald-800 border border-emerald-200"
                : "bg-amber-50 text-amber-800 border border-amber-200");
            msgText.Text = HttpUtility.HtmlEncode(text);
        }
    }
}
