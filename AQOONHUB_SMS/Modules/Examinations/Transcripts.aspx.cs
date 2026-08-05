using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Examinations
{
    public partial class Transcripts : System.Web.UI.Page
    {
        private readonly TranscriptRepository _repo = new TranscriptRepository();
        private readonly ExaminationsRepository _exam = new ExaminationsRepository();

        private string Role { get { return Convert.ToString(Session["Role"]); } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack)
            {
                BindYears();
                BindTerms();
                BindClasses();
                BindSections();
                ddlStudent.Items.Add(new ListItem("— Select student —", "0"));
                ApplyTypeOptionalLabels();
            }
        }

        // ---- Authorization: same permission set the Examinations module already uses ----
        private bool Authorize()
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return false; }
            if (!_exam.CanView(Role)) { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx?denied=transcripts", true); return false; }
            return true;
        }

        // ---- Helpers ----
        private static int Sel(DropDownList d) { int v; return int.TryParse(d.SelectedValue, out v) && v > 0 ? v : 0; }
        private static string Enc(object o) { return System.Web.HttpUtility.HtmlEncode(Convert.ToString(o)); }
        private bool IsFull { get { return ddlType.SelectedValue == "Full"; } }

        private void ApplyTypeOptionalLabels()
        {
            bool full = IsFull;
            lblYearOpt.Text = full ? "(optional)" : "";
            lblTermOpt.Text = full ? "(optional)" : "";
            wrapTerm.Style["opacity"] = full ? "0.6" : "1";
        }

        private void BindYears()
        {
            ddlYear.Items.Clear();
            ddlYear.Items.Add(new ListItem(IsFull ? "All Years" : "— Select year —", "0"));
            foreach (DataRow r in _repo.AcademicYears().Rows)
                ddlYear.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
        }
        private void BindTerms()
        {
            ddlTerm.Items.Clear();
            ddlTerm.Items.Add(new ListItem(IsFull ? "All Terms" : "— Select term —", "0"));
            int y = Sel(ddlYear);
            if (y > 0)
                foreach (DataRow r in _repo.Terms(y).Rows)
                    ddlTerm.Items.Add(new ListItem(Convert.ToString(r["TermName"]), Convert.ToString(r["TermID"])));
        }
        private void BindClasses()
        {
            ddlClass.Items.Clear();
            ddlClass.Items.Add(new ListItem("— All classes —", "0"));
            foreach (DataRow r in _repo.Classes().Rows)
                ddlClass.Items.Add(new ListItem(Convert.ToString(r["ClassName"]), Convert.ToString(r["ClassID"])));
        }
        private void BindSections()
        {
            ddlSection.Items.Clear();
            ddlSection.Items.Add(new ListItem("— All sections —", "0"));
            int c = Sel(ddlClass);
            if (c > 0)
                foreach (DataRow r in _repo.Sections(c).Rows)
                    ddlSection.Items.Add(new ListItem(Convert.ToString(r["SectionName"]), Convert.ToString(r["SectionID"])));
        }
        private void BindStudents(string search)
        {
            ddlStudent.Items.Clear();
            ddlStudent.Items.Add(new ListItem("— Select student —", "0"));
            int sec = Sel(ddlSection);
            DataTable t = _repo.Students(sec > 0 ? sec : (int?)null, search);
            foreach (DataRow r in t.Rows)
            {
                string label = Convert.ToString(r["FullName"]) + " (" + Convert.ToString(r["StudentCode"]) + ")";
                ddlStudent.Items.Add(new ListItem(label, Convert.ToString(r["StudentID"])));
            }
        }

        protected void ddlType_Changed(object sender, EventArgs e) { BindYears(); BindTerms(); ApplyTypeOptionalLabels(); }
        protected void ddlYear_Changed(object sender, EventArgs e) { BindTerms(); }
        protected void ddlClass_Changed(object sender, EventArgs e) { BindSections(); BindStudents(txtSearch.Text); }
        protected void ddlSection_Changed(object sender, EventArgs e) { BindStudents(txtSearch.Text); }
        protected void btnFind_Click(object sender, EventArgs e) { BindStudents(txtSearch.Text); }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ddlType.SelectedIndex = 0; ddlYear.SelectedIndex = 0; ddlClass.SelectedIndex = 0;
            txtSearch.Text = "";
            BindYears(); BindTerms(); BindSections(); ddlStudent.Items.Clear();
            ddlStudent.Items.Add(new ListItem("— Select student —", "0"));
            ApplyTypeOptionalLabels();
            litDoc.Text = ""; pnlMsg.Visible = false;
        }

        private void Msg(string m) { lblMsg.Text = Enc(m); pnlMsg.Visible = true; litDoc.Text = ""; }

        protected void btnLoad_Click(object sender, EventArgs e)
        {
            pnlMsg.Visible = false;
            int studentId = Sel(ddlStudent);
            if (studentId <= 0) { Msg("Please select a student (use the filters or the search box, then choose a student)."); return; }

            DataRow prof = _repo.StudentProfile(studentId);
            if (prof == null) { Msg("Student not found. Please choose a valid student."); return; }

            if (IsFull) { BuildFull(studentId, prof); return; }

            int yearId = Sel(ddlYear), termId = Sel(ddlTerm);
            if (yearId <= 0) { Msg("For a Term Transcript, please select an academic year."); return; }
            if (termId <= 0) { Msg("For a Term Transcript, please select a term / semester."); return; }
            BuildTerm(studentId, yearId, termId, prof);
        }

        // ============================================================
        //  Rendering
        // ============================================================
        private const string SignSvg = "<svg viewBox='0 0 80 30' fill='none' xmlns='http://www.w3.org/2000/svg'><path d='M2 22 C 12 4, 20 4, 26 16 S 40 30, 48 12 62 4, 78 18' stroke='currentColor' stroke-width='2' stroke-linecap='round'/></svg>";

        private string Header(DataRow school, string title, string refNo, bool official, string pageLabel)
        {
            string name = school != null && school["SchoolName"] != DBNull.Value ? Convert.ToString(school["SchoolName"]) : "AQOONHUB Academy";
            string addr = school != null && school["Address"] != DBNull.Value ? Convert.ToString(school["Address"]) : "";
            string phone = school != null && school["Phone"] != DBNull.Value ? Convert.ToString(school["Phone"]) : "";
            string email = school != null && school["Email"] != DBNull.Value ? Convert.ToString(school["Email"]) : "";
            string logo = ResolveUrl("~/Assets/images/logo.png");

            var sb = new StringBuilder();
            sb.Append("<div class='tr-top'>");
            sb.Append("<img src='").Append(logo).Append("' alt='School logo' />");
            sb.Append("<div class='tr-brand'><div class='nm'>").Append(Enc(name)).Append("</div>");
            sb.Append("<div class='tag'>Smart School, Bright Future</div>");
            sb.Append("<div class='tr-contacts' style='margin-top:8px'>");
            if (!string.IsNullOrEmpty(addr)) sb.Append("<div>").Append(Ico("map-pin")).Append(Enc(addr)).Append("</div>");
            if (!string.IsNullOrEmpty(phone)) sb.Append("<div>").Append(Ico("phone")).Append(Enc(phone)).Append("</div>");
            if (!string.IsNullOrEmpty(email)) sb.Append("<div>").Append(Ico("mail")).Append(Enc(email)).Append("</div>");
            sb.Append("</div></div>");
            sb.Append("<div class='tr-refbox'>");
            sb.Append("<div class='rl'>Reference No<br/><span class='tr-pill'>").Append(Enc(refNo)).Append("</span></div>");
            sb.Append("<div class='gen'>Generated On<br/><b>").Append(DateTime.Now.ToString("dd MMM yyyy")).Append("</b></div>");
            sb.Append("<div class='gen'>Status: <span class='tr-badge ").Append(official ? "official" : "draft").Append("'>")
              .Append(official ? "OFFICIAL" : "DRAFT").Append("</span></div>");
            sb.Append("</div></div>");
            if (!string.IsNullOrEmpty(pageLabel)) sb.Append("<div class='tr-pageno'>").Append(Enc(pageLabel)).Append("</div>");
            sb.Append("<div class='tr-doctitle'>").Append(Enc(title)).Append("</div>");
            return sb.ToString();
        }

        private static string Ico(string n) { return "<svg data-lucide='" + n + "' aria-hidden='true'></svg>"; }
        private string Bar(string icon, string text) { return "<div class='tr-bar'>" + Ico(icon) + "<span>" + Enc(text) + "</span></div>"; }

        private string ResultsTable(DataTable br)
        {
            var sb = new StringBuilder();
            sb.Append("<table class='tr-table'><thead><tr>")
              .Append("<th class='num'>No.</th><th>Subject Code</th><th>Subject Name</th>")
              .Append("<th class='num'>Total Mark</th><th class='num'>Grade</th><th>Remarks</th></tr></thead><tbody>");
            int n = 0;
            foreach (DataRow r in br.Rows)
            {
                n++;
                string mark = r["Marks"] == DBNull.Value ? "—"
                    : Convert.ToDecimal(r["Marks"]).ToString("0.##") + (r["TotalMarks"] == DBNull.Value ? "" : " / " + Convert.ToDecimal(r["TotalMarks"]).ToString("0"));
                string grade = r["Grade"] == DBNull.Value ? "" : Convert.ToString(r["Grade"]);
                string rem = r["Remarks"] == DBNull.Value || string.IsNullOrEmpty(Convert.ToString(r["Remarks"])) ? "" : Convert.ToString(r["Remarks"]);
                sb.Append("<tr><td class='num'>").Append(n).Append("</td>")
                  .Append("<td>").Append(Enc(r["SubjectCode"])).Append("</td>")
                  .Append("<td>").Append(Enc(r["SubjectName"])).Append("</td>")
                  .Append("<td class='num'>").Append(Enc(mark)).Append("</td>")
                  .Append("<td class='num'>").Append(string.IsNullOrEmpty(grade) ? "—" : "<span class='tr-gr'>" + Enc(grade) + "</span>").Append("</td>")
                  .Append("<td>").Append(Enc(rem)).Append("</td></tr>");
            }
            sb.Append("</tbody></table>");
            return sb.ToString();
        }

        private string SummaryBoxes(int subjects, decimal obtained, decimal max, decimal avg, string grade, bool pass)
        {
            var sb = new StringBuilder("<div class='tr-summary'>");
            sb.Append(Box("Total Subjects", subjects.ToString(), ""));
            sb.Append(Box("Total Marks", obtained.ToString("0.##") + " / " + max.ToString("0"), ""));
            sb.Append(Box("Average", avg.ToString("0.0", CultureInfo.InvariantCulture) + "%", ""));
            sb.Append(Box("Overall Grade", string.IsNullOrEmpty(grade) ? "—" : grade, ""));
            sb.Append(Box("Result", pass ? "PASS" : "FAIL", pass ? "ok" : "bad"));
            sb.Append("</div>");
            return sb.ToString();
        }
        private string Box(string lbl, string val, string valClass) { return "<div class='box'><div class='lbl'>" + Enc(lbl) + "</div><div class='val " + valClass + "'>" + Enc(val) + "</div></div>"; }

        /// <summary>Position row — only rendered when a class rank was actually calculated for this term.</summary>
        private string PositionRow(int rank)
        {
            if (rank <= 0) return "";
            return "<div class='tr-subrow'><div class='b'><div class='k'>Class Position</div><div class='v'>" + rank + "</div></div></div>";
        }

        private string StudentInfoBlock(DataRow p, bool full, string academicYear, string term)
        {
            var sb = new StringBuilder(Bar(full ? "user" : "user", full ? "Student Profile" : "Student Information"));
            sb.Append("<div class='tr-info'>");
            sb.Append(Info("Full Name", p["FullName"]));
            sb.Append(Info("Student Code", p["StudentCode"]));
            sb.Append(Info("Admission No.", p["AdmissionNo"]));
            sb.Append(Info("Gender", p["Gender"]));
            sb.Append(Info("Date of Birth", p["DateOfBirth"] == DBNull.Value ? "—" : Convert.ToDateTime(p["DateOfBirth"]).ToString("dd MMM yyyy")));
            sb.Append(Info(full ? "Admission Date" : "Enrollment Date", p["EnrollmentDate"] == DBNull.Value ? "—" : Convert.ToDateTime(p["EnrollmentDate"]).ToString("dd MMM yyyy")));
            sb.Append(Info(full ? "Current Class" : "Class", p["ClassName"]));
            if (!full) sb.Append(Info("Section", p["SectionName"]));
            if (!full) { sb.Append(Info("Academic Year", academicYear)); sb.Append(Info("Term", term)); }
            sb.Append(Info("Student Status", p["Status"]));
            sb.Append("</div>");
            return sb.ToString();
        }
        private string Info(string k, object v) { return "<div><span class='k'>" + Enc(k) + ":</span> <span class='v'>" + Enc(v) + "</span></div>"; }

        private string ScaleClass(string g)
        {
            string s = (g ?? "").Trim().ToUpperInvariant();
            if (s.StartsWith("A")) return "sc-a";
            if (s.StartsWith("B")) return "sc-b";
            if (s.StartsWith("C")) return "sc-c";
            if (s.StartsWith("D")) return "sc-d";
            return "sc-f";
        }
        private string GradingScaleBlock(int yearId)
        {
            DataTable g = _repo.GradingScale(yearId);
            if (g.Rows.Count == 0) return "";
            var sb = new StringBuilder(Bar("list-checks", "Grading Scale"));
            sb.Append("<div class='tr-scale'>");
            foreach (DataRow r in g.Rows)
                sb.Append("<div class='chip ").Append(ScaleClass(Convert.ToString(r["GradeLetter"]))).Append("'>")
                  .Append("<div class='g'>").Append(Enc(r["GradeLetter"])).Append(" ").Append(Enc(r["MinMarks"])).Append("–").Append(Enc(r["MaxMarks"])).Append("</div></div>");
            sb.Append("</div>");
            return sb.ToString();
        }

        private string SignatureBlock(bool termLayout)
        {
            string sig = "<div class='sig'>" + SignSvg + "</div>";
            var sb = new StringBuilder("<div class='tr-sign ").Append(termLayout ? "" : "three").Append("'>");
            if (termLayout)
                sb.Append("<div>").Append(sig).Append("<div class='line'></div><div class='nm'>—</div><div class='role'>Class Teacher</div></div>");
            sb.Append("<div>").Append(sig).Append("<div class='line'></div><div class='nm'>—</div><div class='role'>Registrar</div></div>");
            sb.Append("<div>").Append(sig).Append("<div class='line'></div><div class='nm'>—</div><div class='role'>Principal</div></div>");
            sb.Append("<div><div class='tr-stamp'>School<br/>Stamp</div><div class='role' style='margin-top:6px'>Official Seal</div></div>");
            sb.Append("</div>");
            return sb.ToString();
        }

        private string FooterBar()
        {
            return "<div class='tr-footer'>" + Ico("shield-check") +
                   "<span>This transcript is valid only when signed and stamped by the authorized school administration.</span></div>";
        }

        /// <summary>Aggregate one exam's result. Returns null if the student has no computable result.</summary>
        private ExaminationsRepository.StudentResult Res(int examId, int studentId)
        {
            try { return _exam.GetStudentResult(examId, studentId); } catch { return null; }
        }

        // ---------------- TERM ----------------
        private void BuildTerm(int studentId, int yearId, int termId, DataRow prof)
        {
            DataTable exams = _repo.StudentExams(studentId, yearId, termId);
            if (exams.Rows.Count == 0) { Msg("No completed examination results were found for the selected student and academic period."); return; }

            DataRow school = _repo.SchoolInfo();
            string academicYear = "", term = "";
            decimal obtained = 0, max = 0; int subjects = 0; bool allPass = true, allPublished = true; int singleRank = 0;
            var body = new StringBuilder();

            foreach (DataRow ex in exams.Rows)
            {
                int examId = Convert.ToInt32(ex["ExamID"]);
                academicYear = Convert.ToString(ex["YearName"]);
                term = Convert.ToString(ex["TermName"]);
                var sr = Res(examId, studentId);
                DataTable br = _repo.StudentExamBreakdown(examId, studentId);
                if (exams.Rows.Count > 1)
                    body.Append("<div class='tr-term-title'>").Append(Enc(ex["ExamName"])).Append(" · ").Append(Enc(ex["ExamType"])).Append("</div>");
                body.Append(ResultsTable(br));
                if (sr != null)
                {
                    obtained += sr.TotalObtained; max += sr.TotalMaximum;
                    if (!string.Equals(sr.ResultStatus, "Pass", StringComparison.OrdinalIgnoreCase)) allPass = false;
                    if (exams.Rows.Count == 1) singleRank = sr.Rank;
                }
                subjects += br.Rows.Count;
                if (!_exam.ResultsArePublished(examId)) allPublished = false;
            }

            decimal avg = max > 0 ? obtained * 100m / max : 0;
            string grade = _exam.GetGradeForPercentage(yearId, avg);
            string refNo = "TR-" + DateTime.Now.ToString("yyyy") + "-" + (termId * 100 + studentId).ToString("00000");

            var doc = new StringBuilder("<div class='tr-doc theme-blue tr-page'>");
            doc.Append(Header(school, "OFFICIAL TERM TRANSCRIPT", refNo, allPublished, ""));
            doc.Append(StudentInfoBlock(prof, false, academicYear, term));
            doc.Append(Bar("clipboard-list", "Academic Results — " + term));
            doc.Append(body);
            doc.Append(Bar("bar-chart-3", "Term Summary"));
            doc.Append(SummaryBoxes(subjects, obtained, max, avg, grade, allPass));
            doc.Append(PositionRow(singleRank));
            doc.Append(GradingScaleBlock(yearId));
            doc.Append(SignatureBlock(true));
            doc.Append(FooterBar());
            doc.Append("</div>");
            litDoc.Text = doc.ToString();
        }

        // ---------------- FULL ----------------
        private void BuildFull(int studentId, DataRow prof)
        {
            DataTable exams = _repo.StudentExams(studentId, null, null);
            if (exams.Rows.Count == 0) { Msg("No completed examination results were found for the selected student and academic period."); return; }

            DataRow school = _repo.SchoolInfo();
            string refNo = "FT-" + DateTime.Now.ToString("yyyy") + "-" + studentId.ToString("00000");
            string verify = "AQH-FT-" + DateTime.Now.ToString("yyyy") + "-" + studentId.ToString("00000");

            // Group exams by year -> term (preserve chronological order from the query).
            var yearOrder = new List<int>(); var yearName = new Dictionary<int, string>();
            var byYearTerm = new Dictionary<int, Dictionary<int, List<DataRow>>>();
            var termName = new Dictionary<int, string>();
            bool allPublished = true;
            foreach (DataRow ex in exams.Rows)
            {
                int y = Convert.ToInt32(ex["AcademicYearID"]);
                int tm = ex["TermID"] == DBNull.Value ? 0 : Convert.ToInt32(ex["TermID"]);
                if (!byYearTerm.ContainsKey(y)) { byYearTerm[y] = new Dictionary<int, List<DataRow>>(); yearOrder.Add(y); yearName[y] = Convert.ToString(ex["YearName"]); }
                if (!byYearTerm[y].ContainsKey(tm)) byYearTerm[y][tm] = new List<DataRow>();
                byYearTerm[y][tm].Add(ex);
                termName[tm] = ex["TermName"] == DBNull.Value ? "General" : Convert.ToString(ex["TermName"]);
                if (!_exam.ResultsArePublished(Convert.ToInt32(ex["ExamID"]))) allPublished = false;
            }

            decimal cumObtained = 0, cumMax = 0; int cumSubjects = 0, termsCompleted = 0; bool cumPass = true;

            var doc = new StringBuilder("<div class='tr-doc theme-purple tr-page'>");
            doc.Append(Header(school, "OFFICIAL ACADEMIC TRANSCRIPT", refNo, allPublished, "Academic Transcript"));
            doc.Append(StudentInfoBlock(prof, true, "", ""));
            doc.Append(Bar("book-open", "Academic Records"));

            foreach (int y in yearOrder)
            {
                doc.Append("<div class='tr-year-title'>Academic Year ").Append(Enc(yearName[y])).Append("</div>");
                doc.Append("<div class='tr-yeargrid'>");
                decimal yObtained = 0, yMax = 0; int ySubjects = 0; bool yPass = true;
                foreach (var termKv in byYearTerm[y])
                {
                    int tm = termKv.Key;
                    termsCompleted++;
                    var cell = new StringBuilder("<div><div class='tr-term-title'>").Append(Enc(termName.ContainsKey(tm) ? termName[tm] : "Term")).Append("</div>");
                    decimal tObtained = 0, tMax = 0; int tSubjects = 0; bool tPass = true;
                    foreach (DataRow ex in termKv.Value)
                    {
                        int examId = Convert.ToInt32(ex["ExamID"]);
                        var sr = Res(examId, studentId);
                        DataTable br = _repo.StudentExamBreakdown(examId, studentId);
                        if (termKv.Value.Count > 1)
                            cell.Append("<div style='font-size:.72rem;font-weight:600;color:#475569;margin:4px 0 2px'>").Append(Enc(ex["ExamName"])).Append("</div>");
                        cell.Append(ResultsTable(br));
                        if (sr != null) { tObtained += sr.TotalObtained; tMax += sr.TotalMaximum; if (!string.Equals(sr.ResultStatus, "Pass", StringComparison.OrdinalIgnoreCase)) tPass = false; }
                        tSubjects += br.Rows.Count;
                    }
                    cell.Append("</div>");
                    doc.Append(cell.ToString());
                    yObtained += tObtained; yMax += tMax; ySubjects += tSubjects; if (!tPass) yPass = false;
                }
                doc.Append("</div>");
                decimal yAvg = yMax > 0 ? yObtained * 100m / yMax : 0;
                doc.Append("<div class='tr-yearsum'>")
                   .Append("<span>Year Summary — <b>").Append(Enc(yearName[y])).Append("</b></span>")
                   .Append("<span>Subjects: <b>").Append(ySubjects).Append("</b></span>")
                   .Append("<span>Annual Average: <b>").Append(yAvg.ToString("0.0", CultureInfo.InvariantCulture)).Append("%</b></span>")
                   .Append("<span>Overall Grade: <b>").Append(Enc(_exam.GetGradeForPercentage(y, yAvg))).Append("</b></span>")
                   .Append("<span>Result: <b>").Append(yPass ? "PASS" : "FAIL").Append("</b></span></div>");
                cumObtained += yObtained; cumMax += yMax; cumSubjects += ySubjects; if (!yPass) cumPass = false;
            }

            decimal cumAvg = cumMax > 0 ? cumObtained * 100m / cumMax : 0;
            int lastYear = yearOrder.Count > 0 ? yearOrder[yearOrder.Count - 1] : 0;
            string cumGrade = lastYear > 0 ? _exam.GetGradeForPercentage(lastYear, cumAvg) : "";

            doc.Append(Bar("award", "Cumulative Academic Summary"));
            doc.Append("<div class='tr-cumbox'>")
               .Append(Cum("Academic Years Completed", yearOrder.Count.ToString()))
               .Append(Cum("Total Terms Completed", termsCompleted.ToString()))
               .Append(Cum("Total Subjects Completed", cumSubjects.ToString()))
               .Append(Cum("Cumulative Average", cumAvg.ToString("0.0", CultureInfo.InvariantCulture) + "%"))
               .Append(Cum("Final Overall Grade", string.IsNullOrEmpty(cumGrade) ? "—" : cumGrade))
               .Append(Cum("Academic Standing", cumPass ? "GOOD STANDING" : "REVIEW REQUIRED"))
               .Append("</div>");
            doc.Append(GradingScaleBlock(lastYear));
            doc.Append(Bar("shield-check", "Certification"));
            doc.Append("<div class='tr-cert'>This is to certify that the academic record shown above is a true record of the student&rsquo;s performance at this school.</div>");
            doc.Append(SignatureBlock(false));
            doc.Append("<div class='tr-verify'>Verification Code: <b>").Append(Enc(verify)).Append("</b></div>");
            doc.Append(FooterBar());
            doc.Append("</div>");
            litDoc.Text = doc.ToString();
        }
        private string Cum(string k, string v) { return "<div><span class='k'>" + Enc(k) + ":</span> <span class='v'>" + Enc(v) + "</span></div>"; }
    }
}
