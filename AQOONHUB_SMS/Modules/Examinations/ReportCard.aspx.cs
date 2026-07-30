using System;
using System.Data;
using System.Globalization;

namespace AQOONHUB_SMS.Modules.Examinations
{
    public partial class ReportCard : System.Web.UI.Page
    {
        private readonly ExaminationsRepository _repo = new ExaminationsRepository();

        private int ExamId { get { int v; return int.TryParse(Request.QueryString["exam"], out v) ? v : 0; } }
        private int StudentId { get { int v; return int.TryParse(Request.QueryString["student"], out v) ? v : 0; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack) LoadCard();
        }

        private int UserId { get { int v; return int.TryParse(Convert.ToString(Session["UserID"]), out v) ? v : 0; } }
        private string Role { get { return Convert.ToString(Session["Role"]); } }

        private bool Authorize()
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return false; }
            string r = _repo.NormalizeRole(Role);
            bool parentLike = r == "parent" || r == "guardian";
            // Management/registrar/teacher, or a parent linked to this student, may view.
            if (!_repo.CanView(Role) && !parentLike) { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return false; }
            // Server-side ownership check: a parent can only open a linked child's card,
            // and only for published results. Prevents QueryString student-id tampering.
            if (StudentId > 0 && !_repo.UserCanViewStudentResult(UserId, Role, StudentId))
            { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return false; }
            if (parentLike && ExamId > 0 && !_repo.ResultsArePublished(ExamId))
            { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return false; }
            return true;
        }

        protected string Pct(object marks, object total)
        {
            if (marks == null || marks == DBNull.Value || total == null || total == DBNull.Value) return "—";
            decimal m = Convert.ToDecimal(marks), t = Convert.ToDecimal(total);
            return t > 0 ? Math.Round(m * 100m / t, 1).ToString("0.#", CultureInfo.InvariantCulture) + "%" : "—";
        }

        private void LoadCard()
        {
            DataRow ex = ExamId > 0 ? _repo.GetExamination(ExamId) : null;
            ExaminationsRepository.StudentResult sr = ex != null && StudentId > 0 ? _repo.GetStudentResult(ExamId, StudentId) : null;
            if (ex == null || sr == null) { pnlDenied.Visible = true; pnlBody.Visible = false; return; }
            pnlBody.Visible = true;

            litExam.Text = Server.HtmlEncode(Convert.ToString(ex["ExamName"]) + " · " + Convert.ToString(ex["ExamType"]));
            litName.Text = Server.HtmlEncode(sr.FullName);
            litCode.Text = Server.HtmlEncode(sr.StudentCode);
            litClass.Text = Server.HtmlEncode(sr.ClassName + " / " + sr.SectionName);
            litYear.Text = Server.HtmlEncode(Convert.ToString(ex["YearName"]));
            litTerm.Text = Server.HtmlEncode(Convert.ToString(ex["TermName"]));

            DataTable br = _repo.GetStudentResultBreakdown(ExamId, StudentId);
            rptSubjects.DataSource = br; rptSubjects.DataBind();
            string adm = _repo.GetStudentAdmissionNo(StudentId);
            litAdm.Text = string.IsNullOrEmpty(adm) ? "—" : Server.HtmlEncode(adm);

            litObtained.Text = sr.TotalObtained.ToString("0.##");
            litMax.Text = sr.TotalMaximum.ToString();
            litAvg.Text = sr.Average.ToString("0.00") + "%";
            litGrade.Text = string.IsNullOrEmpty(sr.Grade) ? "—" : Server.HtmlEncode(sr.Grade);
            litRank.Text = sr.Rank > 0 ? sr.Rank.ToString() : "—";
            litStatus.Text = Server.HtmlEncode(sr.ResultStatus);

            bool published = _repo.ResultsArePublished(ExamId);
            DataRow pub = _repo.GetResultPublication(ExamId);
            litPublished.Text = published && pub != null && pub["PublishedAt"] != DBNull.Value
                ? "Published: " + Convert.ToDateTime(pub["PublishedAt"]).ToString("dd MMM yyyy")
                : "Not yet published (preview)";
        }
    }
}
