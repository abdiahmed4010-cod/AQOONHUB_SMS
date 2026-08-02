using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Examinations
{
    public partial class MarksEntry : System.Web.UI.Page
    {
        private readonly ExaminationsRepository _repo = new ExaminationsRepository();

        // used by markup during data binding
        public bool CanEdit { get; private set; }
        public int TotalMarks { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack)
            {
                BindYears(); BindTerms(); BindExams(); BindClasses(); BindSections(); BindSubjects();
                LoadScale();
                LoadStudents();
            }
        }

        private bool Authorize()
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return false; }
            if (!_repo.CanEnterMarks(Convert.ToString(Session["Role"]))) { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return false; }
            return true;
        }

        private string Role { get { return Convert.ToString(Session["Role"]); } }
        private int UserId { get { int v; return int.TryParse(Convert.ToString(Session["UserID"]), out v) ? v : 0; } }
        private int YearV() { int v; return int.TryParse(ddlYear.SelectedValue, out v) ? v : 0; }
        private int TermV() { int v; return int.TryParse(ddlTerm.SelectedValue, out v) ? v : 0; }
        private int ExamV() { int v; return int.TryParse(ddlExam.SelectedValue, out v) ? v : 0; }
        private int ClassV() { int v; return int.TryParse(ddlClass.SelectedValue, out v) ? v : 0; }
        private int SectionV() { int v; return int.TryParse(ddlSection.SelectedValue, out v) ? v : 0; }
        private int SubjectV() { int v; return int.TryParse(ddlSubject.SelectedValue, out v) ? v : 0; }

        // ---- dropdown binding ----
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
            foreach (DataRow r in _repo.GetMarksEntryExams(YearV() > 0 ? YearV() : (int?)null, TermV() > 0 ? TermV() : (int?)null).Rows)
                ddlExam.Items.Add(new ListItem(Convert.ToString(r["ExamName"]), Convert.ToString(r["ExamID"])));
        }
        private void BindClasses()
        {
            ddlClass.Items.Clear(); ddlClass.Items.Add(new ListItem("— Select class —", "0"));
            if (ExamV() > 0) foreach (DataRow r in _repo.GetClassesForExam(ExamV()).Rows) ddlClass.Items.Add(new ListItem(Convert.ToString(r["ClassName"]), Convert.ToString(r["ClassID"])));
        }
        private void BindSections()
        {
            ddlSection.Items.Clear(); ddlSection.Items.Add(new ListItem("— Select section —", "0"));
            if (ClassV() > 0) foreach (DataRow r in _repo.GetSectionsForClass(ClassV()).Rows) ddlSection.Items.Add(new ListItem(Convert.ToString(r["SectionName"]), Convert.ToString(r["SectionID"])));
        }
        private void BindSubjects()
        {
            ddlSubject.Items.Clear(); ddlSubject.Items.Add(new ListItem("— Select subject —", "0"));
            if (ExamV() > 0 && ClassV() > 0)
                foreach (DataRow r in _repo.GetSubjectsForMarks(ExamV(), ClassV()).Rows) ddlSubject.Items.Add(new ListItem(Convert.ToString(r["SubjectName"]), Convert.ToString(r["SubjectID"])));
        }

        protected void ddlYear_Changed(object sender, EventArgs e) { BindTerms(); BindExams(); BindClasses(); BindSections(); BindSubjects(); LoadScale(); LoadStudents(); }
        protected void ddlTerm_Changed(object sender, EventArgs e) { BindExams(); BindClasses(); BindSections(); BindSubjects(); LoadStudents(); }
        protected void ddlExam_Changed(object sender, EventArgs e) { BindClasses(); BindSections(); BindSubjects(); LoadStudents(); }
        protected void ddlClass_Changed(object sender, EventArgs e) { BindSections(); BindSubjects(); LoadStudents(); }
        protected void ddlSection_Changed(object sender, EventArgs e) { LoadStudents(); }
        protected void ddlSubject_Changed(object sender, EventArgs e) { LoadStudents(); }
        protected void btnReset_Click(object sender, EventArgs e)
        {
            BindYears(); BindTerms(); BindExams(); BindClasses(); BindSections(); BindSubjects(); LoadScale(); LoadStudents();
        }

        private void LoadScale()
        {
            gvScale.DataSource = _repo.GetGradingScales(YearV()); gvScale.DataBind();
        }

        private int ExamAcademicYear()
        {
            DataRow ex = _repo.GetExamination(ExamV());
            return ex != null && ex["AcademicYearID"] != DBNull.Value ? Convert.ToInt32(ex["AcademicYearID"]) : YearV();
        }

        private void LoadStudents()
        {
            btnReopen.Visible = false;
            if (ExamV() <= 0 || ClassV() <= 0 || SectionV() <= 0 || SubjectV() <= 0)
            {
                CanEdit = false; TotalMarks = 0;
                gvMarks.DataSource = null; gvMarks.DataBind();
                litTotal.Text = litEntered.Text = litRemaining.Text = "0"; litCompletion.Text = "0%";
                litSumTotal.Text = litSumEntered.Text = litSumSubmitted.Text = "0"; litReady.Text = "No"; litLockNote.Text = "";
                return;
            }

            DataRow esub = _repo.GetExamSubjectRow(ExamV(), ClassV(), SubjectV());
            TotalMarks = esub != null && esub["TotalMarks"] != DBNull.Value ? Convert.ToInt32(esub["TotalMarks"]) : 100;

            bool submitted = _repo.ExamSubjectMarksAreSubmitted(ExamV(), SectionV(), SubjectV());
            bool authorized = _repo.UserCanEnterSubjectMarks(UserId, Role, SectionV(), SubjectV(), ExamAcademicYear());
            CanEdit = authorized && !submitted;

            btnSaveDraft.Enabled = CanEdit; btnSubmit.Enabled = CanEdit;
            btnReopen.Visible = submitted && _repo.CanManage(Role);
            litLockNote.Text = submitted ? "Submitted (locked)" : (authorized ? "Editable" : "Read-only (not your assignment)");

            DataTable dt = _repo.GetStudentsForMarksEntry(ExamV(), SectionV(), SubjectV());
            gvMarks.DataSource = dt; gvMarks.DataBind();

            // preselect attendance per row
            for (int i = 0; i < gvMarks.Rows.Count && i < dt.Rows.Count; i++)
            {
                DropDownList ddl = (DropDownList)gvMarks.Rows[i].FindControl("ddlAtt");
                if (ddl != null)
                {
                    string att = Convert.ToString(dt.Rows[i]["AttendanceStatus"]);
                    if (ddl.Items.FindByValue(att) != null) ddl.SelectedValue = att;
                }
            }

            DataRow sum = _repo.GetMarkEntrySummary(ExamV(), SectionV(), SubjectV());
            long total = Convert.ToInt64(sum["TotalStudents"]), entered = Convert.ToInt64(sum["Entered"]), sub = Convert.ToInt64(sum["Submitted"]);
            litTotal.Text = litSumTotal.Text = total.ToString();
            litEntered.Text = litSumEntered.Text = entered.ToString();
            litRemaining.Text = Math.Max(0, total - entered).ToString();
            litSumSubmitted.Text = sub.ToString();
            litCompletion.Text = total > 0 ? Math.Round(entered * 100.0 / total, 0) + "%" : "0%";
            litReady.Text = (total > 0 && entered >= total && sub == 0) ? "Yes" : "No";
        }

        private List<ExaminationsRepository.MarkRow> ReadGrid()
        {
            var rows = new List<ExaminationsRepository.MarkRow>();
            foreach (GridViewRow gr in gvMarks.Rows)
            {
                int sid = Convert.ToInt32(gvMarks.DataKeys[gr.RowIndex].Value);
                TextBox tScore = (TextBox)gr.FindControl("txtScore");
                TextBox tRem = (TextBox)gr.FindControl("txtRemarks");
                DropDownList dAtt = (DropDownList)gr.FindControl("ddlAtt");
                decimal? score = null;
                string raw = tScore.Text.Trim();
                if (raw.Length > 0)
                {
                    decimal parsed;
                    if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out parsed))
                        throw new InvalidOperationException("Invalid score for a student.");
                    score = parsed;
                }
                rows.Add(new ExaminationsRepository.MarkRow { StudentID = sid, Score = score, Remarks = tRem.Text.Trim(), Attendance = dAtt.SelectedValue });
            }
            return rows;
        }

        protected void btnSaveDraft_Click(object sender, EventArgs e) { Save(false); }
        protected void btnSubmit_Click(object sender, EventArgs e) { Save(true); }

        private int CurrentTotalMarks()
        {
            DataRow esub = _repo.GetExamSubjectRow(ExamV(), ClassV(), SubjectV());
            return esub != null && esub["TotalMarks"] != DBNull.Value ? Convert.ToInt32(esub["TotalMarks"]) : 100;
        }

        private void Save(bool submit)
        {
            if (ExamV() <= 0 || ClassV() <= 0 || SectionV() <= 0 || SubjectV() <= 0) { Show(false, "Select an examination, class, section and subject."); return; }
            try
            {
                var rows = ReadGrid();
                _repo.SaveMarks(ExamV(), ClassV(), SectionV(), SubjectV(), CurrentTotalMarks(), rows, UserId, Role, ExamAcademicYear(), submit);
                Show(true, submit ? "Marks submitted and locked." : "Progress saved as draft.");
                LoadStudents();
            }
            catch (Exception ex) { Show(false, ex.Message); LoadStudents(); }
        }

        protected void btnReopen_Click(object sender, EventArgs e)
        {
            try
            {
                _repo.ReopenMarks(ExamV(), SectionV(), SubjectV(), "Reopened for correction", UserId, Role);
                Show(true, "Marks reopened. They are editable again.");
                LoadStudents();
            }
            catch (Exception ex) { Show(false, ex.Message); }
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            if (ExamV() <= 0 || SectionV() <= 0 || SubjectV() <= 0) { Show(false, "Select an examination, section and subject to export."); return; }
            DataTable dt = _repo.GetStudentsForMarksEntry(ExamV(), SectionV(), SubjectV());
            int total = CurrentTotalMarks();
            StringBuilder b = new StringBuilder("\uFEFF");
            string[] headers = { "Student Code", "Student Name", "Admission No", "Score", "Total Marks", "Grade", "Attendance", "Remarks", "Status" };
            foreach (string h in headers) b.Append(Csv(h)).Append(',');
            b.AppendLine();
            foreach (DataRow r in dt.Rows)
            {
                b.Append(Csv(Convert.ToString(r["StudentCode"]))).Append(',')
                 .Append(Csv(Convert.ToString(r["FullName"]))).Append(',')
                 .Append(Csv(Convert.ToString(r["AdmissionNo"]))).Append(',')
                 .Append(Csv(r["Marks"] == DBNull.Value ? "" : Convert.ToString(r["Marks"]))).Append(',')
                 .Append(Csv(total.ToString())).Append(',')
                 .Append(Csv(r["Grade"] == DBNull.Value ? "" : Convert.ToString(r["Grade"]))).Append(',')
                 .Append(Csv(Convert.ToString(r["AttendanceStatus"]))).Append(',')
                 .Append(Csv(r["Remarks"] == DBNull.Value ? "" : Convert.ToString(r["Remarks"]))).Append(',')
                 .Append(Csv(Convert.ToString(r["MarkStatus"]))).AppendLine();
            }
            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment; filename=marks-sheet.csv");
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
