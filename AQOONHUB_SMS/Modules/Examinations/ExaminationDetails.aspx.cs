using System;
using System.Data;
using System.Globalization;
using System.Web;

namespace AQOONHUB_SMS.Modules.Examinations
{
    public partial class ExaminationDetails : System.Web.UI.Page
    {
        private readonly ExaminationsRepository _repo = new ExaminationsRepository();

        private int ExamId
        {
            get { int id; return int.TryParse(Request.QueryString["id"], out id) ? id : 0; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack)
            {
                if (Request.QueryString["saved"] == "1") Show(true, "Examination saved.");
                LoadDetails();
            }
        }

        private bool Authorize()
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx", true);
                return false;
            }
            if (!_repo.CanManage(Convert.ToString(Session["Role"])))
            {
                Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true);
                return false;
            }
            return true;
        }

        private void LoadDetails()
        {
            DataRow ex = ExamId > 0 ? _repo.GetExamination(ExamId) : null;
            if (ex == null) { pnlNotFound.Visible = true; pnlBody.Visible = false; return; }
            pnlBody.Visible = true;

            string status = Convert.ToString(ex["Status"]);
            litName.Text = Server.HtmlEncode(Convert.ToString(ex["ExamName"]));
            lblStatus.Text = HttpUtility.HtmlEncode(status);
            lblStatus.Attributes["style"] = StatusStyle(status);
            litType.Text = Server.HtmlEncode(Convert.ToString(ex["ExamType"]));
            litYear.Text = Server.HtmlEncode(Convert.ToString(ex["YearName"]));
            litTerm.Text = Server.HtmlEncode(Convert.ToString(ex["TermName"]));
            litDuration.Text = Convert.ToDateTime(ex["StartDate"]).ToString("dd MMM yyyy") + " – " + Convert.ToDateTime(ex["EndDate"]).ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
            litTotal.Text = Convert.ToString(ex["TotalMarks"]);
            litPass.Text = Convert.ToString(ex["PassingMark"]);
            litWeight.Text = Convert.ToString(ex["Weight"]);
            litCreatedBy.Text = ex["CreatedByName"] == DBNull.Value ? "—" : Server.HtmlEncode(Convert.ToString(ex["CreatedByName"]));

            gvSubjects.DataSource = _repo.GetExamSubjects(ExamId);
            gvSubjects.DataBind();

            System.Text.StringBuilder b = new System.Text.StringBuilder();
            foreach (DataRow r in _repo.GetExamClasses(ExamId).Rows)
            {
                string sec = r["SectionName"] == DBNull.Value ? "All Sections" : Convert.ToString(r["SectionName"]);
                b.Append("<span class='chip'>").Append(Server.HtmlEncode(Convert.ToString(r["ClassName"]))).Append(" / ").Append(Server.HtmlEncode(sec)).Append("</span>");
            }
            litScope.Text = b.Length == 0 ? "<span class='chip'>—</span>" : b.ToString();

            // Stage-2 action visibility
            bool isDraft = status.Equals("Draft", StringComparison.OrdinalIgnoreCase);
            bool isActive = status.Equals("Active", StringComparison.OrdinalIgnoreCase);
            bool hasDeps = _repo.ExaminationHasDependencies(ExamId);

            lnkEdit.NavigateUrl = ResolveUrl("~/Modules/Examinations/CreateExamination.aspx?id=" + ExamId);
            lnkEdit.Visible = (isDraft || isActive) && !hasDeps;
            lnkSchedule.NavigateUrl = ResolveUrl("~/Modules/Examinations/ExamSchedule.aspx?exam=" + ExamId);
            lnkSchedule.Visible = isActive;
            bool isScheduled = status.Equals("Scheduled", StringComparison.OrdinalIgnoreCase) || status.Equals("Ongoing", StringComparison.OrdinalIgnoreCase);
            lnkMarks.NavigateUrl = ResolveUrl("~/Modules/Examinations/MarksEntry.aspx");
            lnkMarks.Visible = isScheduled;
            lnkResults.NavigateUrl = ResolveUrl("~/Modules/Examinations/Results.aspx");
            lnkResults.Visible = isScheduled || status.Equals("Completed", StringComparison.OrdinalIgnoreCase) || status.Equals("Published", StringComparison.OrdinalIgnoreCase);
            btnActivate.Visible = isDraft;
            btnCancel.Visible = (isDraft || isActive);
            btnDelete.Visible = isDraft && !hasDeps;

            // Real schedule summary
            DataRow sum = _repo.GetExamScheduleSummary(ExamId);
            long totalSubj = Convert.ToInt64(sum["TotalSubjects"]);
            long scheduled = Convert.ToInt64(sum["ScheduledSubjects"]);
            litSchedule.Text = scheduled + " of " + totalSubj + " subject(s) scheduled";
            DataRow mp = _repo.GetExamMarksProgress(ExamId);
            long subjMarks = Convert.ToInt64(mp["SubjectsWithMarks"]), subjSub = Convert.ToInt64(mp["SubjectsSubmitted"]);
            litMarks.Text = subjMarks == 0 ? "Not Started" : (subjSub + " submitted, " + subjMarks + " with marks of " + totalSubj);
            litPublication.Text = status.Equals("Scheduled", StringComparison.OrdinalIgnoreCase) ? "Schedule Published" :
                                  status.Equals("Published", StringComparison.OrdinalIgnoreCase) ? "Results Published" : "Not Published";
        }

        private string StatusStyle(string status)
        {
            switch ((status ?? "").ToLowerInvariant())
            {
                case "published": return "background:#CCFBF1;color:#0F766E";
                case "completed": return "background:#DCFCE7;color:#15803D";
                case "ongoing": return "background:#FEF3C7;color:#B45309";
                case "scheduled": return "background:#DBEAFE;color:#2563EB";
                case "active": return "background:#DCFCE7;color:#15803D";
                case "cancelled": return "background:#FEE2E2;color:#DC2626";
                default: return "background:#F1F5F9;color:#64748B";
            }
        }

        protected void btnActivate_Click(object sender, EventArgs e)
        {
            try { _repo.SetExaminationStatus(ExamId, "Active"); Show(true, "Examination activated."); LoadDetails(); }
            catch (Exception ex) { Show(false, ex.Message); LoadDetails(); }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            try { _repo.SetExaminationStatus(ExamId, "Cancelled"); Show(true, "Examination cancelled."); LoadDetails(); }
            catch (Exception ex) { Show(false, ex.Message); LoadDetails(); }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            try { _repo.DeleteDraftExamination(ExamId); Response.Redirect("~/Modules/Examinations/Examinations.aspx", true); }
            catch (System.Threading.ThreadAbortException) { throw; }
            catch (Exception ex) { Show(false, ex.Message); LoadDetails(); }
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
