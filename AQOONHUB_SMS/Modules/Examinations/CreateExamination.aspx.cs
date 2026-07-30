using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Examinations
{
    public partial class CreateExamination : System.Web.UI.Page
    {
        private readonly ExaminationsRepository _repo = new ExaminationsRepository();

        private int EditId
        {
            get { int id; return int.TryParse(Request.QueryString["id"], out id) ? id : 0; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack)
            {
                BindYears();
                BindTerms();
                BindClasses();
                BindSectionsAndSubjects();
                if (EditId > 0) LoadForEdit();
                RefreshOverview();
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

        // ---- binding ----
        private int YearV() { int v; return int.TryParse(ddlYear.SelectedValue, out v) ? v : 0; }
        private int ClassV() { int v; return int.TryParse(ddlClass.SelectedValue, out v) ? v : 0; }

        private void BindYears()
        {
            ddlYear.Items.Clear();
            foreach (DataRow r in _repo.GetAcademicYears().Rows)
                ddlYear.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
            int active = _repo.GetActiveAcademicYearId();
            if (active > 0 && ddlYear.Items.FindByValue(active.ToString()) != null) ddlYear.SelectedValue = active.ToString();
        }

        private void BindTerms()
        {
            ddlTerm.Items.Clear();
            foreach (DataRow r in _repo.GetTerms(YearV()).Rows)
                ddlTerm.Items.Add(new ListItem(Convert.ToString(r["TermName"]), Convert.ToString(r["TermID"])));
            if (ddlTerm.Items.Count == 0) ddlTerm.Items.Add(new ListItem("— No terms —", "0"));
        }

        private void BindClasses()
        {
            ddlClass.Items.Clear();
            ddlClass.Items.Add(new ListItem("— Select class —", "0"));
            foreach (DataRow r in _repo.GetClasses().Rows)
                ddlClass.Items.Add(new ListItem(Convert.ToString(r["ClassName"]), Convert.ToString(r["ClassID"])));
        }

        private void BindSectionsAndSubjects()
        {
            ddlSection.Items.Clear();
            ddlSection.Items.Add(new ListItem("All Sections", "0"));
            cblSubjects.Items.Clear();
            int cls = ClassV();
            if (cls > 0)
            {
                foreach (DataRow r in _repo.GetSectionsForClass(cls).Rows)
                    ddlSection.Items.Add(new ListItem(Convert.ToString(r["SectionName"]), Convert.ToString(r["SectionID"])));
                foreach (DataRow r in _repo.GetSubjectsForClass(cls, YearV()).Rows)
                    cblSubjects.Items.Add(new ListItem(Convert.ToString(r["SubjectName"]), Convert.ToString(r["SubjectID"])));
            }
            pnlNoSubjects.Visible = cls > 0 && cblSubjects.Items.Count == 0;
            pnlSubjects.Visible = ddlScope.SelectedValue == "Selected";
        }

        protected void ddlYear_Changed(object sender, EventArgs e) { BindTerms(); BindSectionsAndSubjects(); RefreshOverview(); }
        protected void ddlClass_Changed(object sender, EventArgs e) { BindSectionsAndSubjects(); RefreshOverview(); }
        protected void ddlScope_Changed(object sender, EventArgs e) { pnlSubjects.Visible = ddlScope.SelectedValue == "Selected"; RefreshOverview(); }

        private List<int> SelectedSubjectIds()
        {
            var ids = new List<int>();
            if (ddlScope.SelectedValue == "All")
            {
                if (ClassV() > 0)
                    foreach (DataRow r in _repo.GetSubjectsForClass(ClassV(), YearV()).Rows)
                        ids.Add(Convert.ToInt32(r["SubjectID"]));
            }
            else
            {
                foreach (ListItem li in cblSubjects.Items) if (li.Selected) ids.Add(int.Parse(li.Value));
            }
            return ids;
        }

        // ---- overview ----
        private void RefreshOverview()
        {
            ovName.Text = string.IsNullOrWhiteSpace(txtName.Text) ? "—" : Server.HtmlEncode(txtName.Text.Trim());
            ovYear.Text = ddlYear.SelectedIndex >= 0 ? Server.HtmlEncode(ddlYear.SelectedItem.Text) : "—";
            ovTerm.Text = ddlTerm.SelectedIndex >= 0 ? Server.HtmlEncode(ddlTerm.SelectedItem.Text) : "—";
            ovType.Text = Server.HtmlEncode(ddlType.SelectedItem.Text);
            DateTime s, en;
            bool hasS = DateTime.TryParse(txtStart.Text, out s), hasE = DateTime.TryParse(txtEnd.Text, out en);
            ovDuration.Text = hasS && hasE ? s.ToString("dd MMM yyyy") + " – " + en.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) : "—";
            ovClass.Text = ddlClass.SelectedIndex > 0
                ? "<span class='chip'>" + Server.HtmlEncode(ddlClass.SelectedItem.Text) + " / " + Server.HtmlEncode(ddlSection.SelectedItem.Text) + "</span>"
                : "<span class='chip'>—</span>";

            var ids = SelectedSubjectIds();
            if (ids.Count == 0) ovSubjects.Text = "<span class='chip'>—</span>";
            else
            {
                System.Text.StringBuilder b = new System.Text.StringBuilder();
                if (ddlScope.SelectedValue == "All")
                    foreach (DataRow r in _repo.GetSubjectsForClass(ClassV(), YearV()).Rows)
                        b.Append("<span class='chip'>").Append(Server.HtmlEncode(Convert.ToString(r["SubjectName"]))).Append("</span>");
                else
                    foreach (ListItem li in cblSubjects.Items) if (li.Selected) b.Append("<span class='chip'>").Append(Server.HtmlEncode(li.Text)).Append("</span>");
                ovSubjects.Text = b.ToString();
            }
        }

        // ---- edit ----
        private void LoadForEdit()
        {
            DataRow ex = _repo.GetExamination(EditId);
            if (ex == null) { Show(false, "Examination not found."); return; }
            string st = Convert.ToString(ex["Status"]);
            if (!st.Equals("Draft", StringComparison.OrdinalIgnoreCase) && !st.Equals("Active", StringComparison.OrdinalIgnoreCase))
            { Show(false, "Only Draft or Active exams can be edited."); btnDraft.Enabled = btnCreate.Enabled = false; return; }

            litHeading.Text = litCrumb.Text = "Edit Examination";
            hfId.Value = EditId.ToString();
            txtName.Text = Convert.ToString(ex["ExamName"]);
            if (ddlYear.Items.FindByValue(Convert.ToString(ex["AcademicYearID"])) != null) ddlYear.SelectedValue = Convert.ToString(ex["AcademicYearID"]);
            BindTerms();
            if (ddlTerm.Items.FindByValue(Convert.ToString(ex["TermID"])) != null) ddlTerm.SelectedValue = Convert.ToString(ex["TermID"]);
            if (ddlType.Items.FindByValue(Convert.ToString(ex["ExamType"])) != null) ddlType.SelectedValue = Convert.ToString(ex["ExamType"]);
            txtStart.Text = Convert.ToDateTime(ex["StartDate"]).ToString("yyyy-MM-dd");
            txtEnd.Text = Convert.ToDateTime(ex["EndDate"]).ToString("yyyy-MM-dd");
            txtPass.Text = Convert.ToString(ex["PassingMark"]);
            txtTotal.Text = Convert.ToString(ex["TotalMarks"]);
            txtWeight.Text = Convert.ToString(ex["Weight"]);

            DataTable ecs = _repo.GetExamClasses(EditId);
            if (ecs.Rows.Count > 0)
            {
                string cid = Convert.ToString(ecs.Rows[0]["ClassID"]);
                if (ddlClass.Items.FindByValue(cid) != null) ddlClass.SelectedValue = cid;
                BindSectionsAndSubjects();
                string sec = ecs.Rows[0]["SectionID"] == DBNull.Value ? "0" : Convert.ToString(ecs.Rows[0]["SectionID"]);
                if (ddlSection.Items.FindByValue(sec) != null) ddlSection.SelectedValue = sec;
            }
            // preselect subjects
            ddlScope.SelectedValue = "Selected";
            pnlSubjects.Visible = true;
            DataTable subs = _repo.GetExamSubjects(EditId);
            foreach (DataRow r in subs.Rows)
            {
                ListItem li = cblSubjects.Items.FindByValue(Convert.ToString(r["SubjectID"]));
                if (li != null) li.Selected = true;
            }
        }

        // ---- save ----
        private void Save(string status)
        {
            int id = int.Parse(hfId.Value);
            int year, term, cls, section, pass, total, weight;
            int.TryParse(ddlYear.SelectedValue, out year);
            int.TryParse(ddlTerm.SelectedValue, out term);
            int.TryParse(ddlClass.SelectedValue, out cls);
            int.TryParse(ddlSection.SelectedValue, out section);
            int.TryParse(txtPass.Text, out pass);
            int.TryParse(txtTotal.Text, out total);
            int.TryParse(txtWeight.Text, out weight);

            DateTime start, end;
            if (!DateTime.TryParse(txtStart.Text, out start)) { Reopen("Please provide a valid start date."); return; }
            if (!DateTime.TryParse(txtEnd.Text, out end)) { Reopen("Please provide a valid end date."); return; }

            List<int> subjects = SelectedSubjectIds();
            int? userId = null; int uid;
            if (int.TryParse(Convert.ToString(Session["UserID"]), out uid)) userId = uid;

            try
            {
                int newId = _repo.SaveExamination(id, txtName.Text, year, term, ddlType.SelectedValue, start, end,
                    cls, section > 0 ? section : (int?)null, subjects, pass, total, weight, status, userId);
                Response.Redirect("~/Modules/Examinations/ExaminationDetails.aspx?id=" + newId + "&saved=1", true);
            }
            catch (System.Threading.ThreadAbortException) { throw; }
            catch (Exception ex) { Reopen(ex.Message); }
        }

        protected void btnDraft_Click(object sender, EventArgs e) { Save("Draft"); }
        protected void btnCreate_Click(object sender, EventArgs e) { Save("Active"); }

        private void Reopen(string text) { RefreshOverview(); Show(false, text); }

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
