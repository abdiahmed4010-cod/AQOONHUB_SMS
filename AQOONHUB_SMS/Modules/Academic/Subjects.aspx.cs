using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Academic
{
    public partial class Subjects : System.Web.UI.Page
    {
        private readonly AcademicsRepository _repo = new AcademicsRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack)
            {
                BindClassFilter();
                BindGrid();
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

        private void BindClassFilter()
        {
            ddlClassFilter.Items.Clear();
            ddlClassFilter.Items.Add(new ListItem("All Classes", ""));
            foreach (DataRow r in _repo.GetClassesLookup().Rows)
                ddlClassFilter.Items.Add(new ListItem(Convert.ToString(r["ClassName"]), Convert.ToString(r["ClassID"])));
        }

        private int? ClassFilter()
        {
            int c;
            return int.TryParse(ddlClassFilter.SelectedValue, out c) && c > 0 ? c : (int?)null;
        }

        private void BindGrid()
        {
            gv.DataSource = _repo.GetSubjects(txtSearch.Text.Trim(), ddlTypeFilter.SelectedValue, ddlStatusFilter.SelectedValue, ClassFilter());
            gv.DataBind();
        }

        protected void btnFilter_Click(object sender, EventArgs e) { BindGrid(); }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlTypeFilter.SelectedIndex = 0; ddlClassFilter.SelectedIndex = 0; ddlStatusFilter.SelectedIndex = 0;
            BindGrid();
        }

        // ---- subject drawer ----
        protected void btnAdd_Click(object sender, EventArgs e)
        {
            hfId.Value = "0";
            litTitle.Text = "Add Subject";
            txtCode.Text = txtName.Text = txtDesc.Text = "";
            txtMax.Text = "100"; txtPass.Text = "50";
            ddlType.SelectedValue = "Core"; ddlActive.SelectedValue = "1";
            pnlDrawer.Visible = true;
        }

        protected void btnCancel_Click(object sender, EventArgs e) { pnlDrawer.Visible = false; }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int id = int.Parse(hfId.Value);
            int max, pass;
            int.TryParse(txtMax.Text, out max);
            int.TryParse(txtPass.Text, out pass);
            try
            {
                _repo.SaveSubject(id, txtCode.Text, txtName.Text, ddlType.SelectedValue, max, pass,
                    ddlActive.SelectedValue == "1", txtDesc.Text.Trim());
                Show(true, id > 0 ? "Subject updated." : "Subject created.");
                pnlDrawer.Visible = false;
                BindGrid();
            }
            catch (Exception ex) { Show(false, ex.Message); pnlDrawer.Visible = true; }
        }

        protected void gv_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id) || id <= 0) return;

            if (e.CommandName == "EditRow")
            {
                DataRow r = _repo.GetSubject(id);
                if (r == null) return;
                hfId.Value = id.ToString();
                litTitle.Text = "Edit Subject";
                txtCode.Text = Convert.ToString(r["SubjectCode"]);
                txtName.Text = Convert.ToString(r["SubjectName"]);
                ddlType.SelectedValue = Convert.ToString(r["SubjectType"]);
                txtMax.Text = Convert.ToString(r["MaxMarks"]);
                txtPass.Text = Convert.ToString(r["PassMarks"]);
                ddlActive.SelectedValue = Convert.ToBoolean(r["IsActive"]) ? "1" : "0";
                txtDesc.Text = r["Description"] == DBNull.Value ? "" : Convert.ToString(r["Description"]);
                pnlDrawer.Visible = true;
            }
            else if (e.CommandName == "ToggleActive")
            {
                DataRow r = _repo.GetSubject(id);
                if (r == null) return;
                bool active = Convert.ToBoolean(r["IsActive"]);
                if (active && _repo.SubjectHasReferences(id))
                {
                    // deactivate rather than delete when referenced — this is the intended safe action
                }
                _repo.SetSubjectActive(id, !active);
                Show(true, active ? "Subject deactivated." : "Subject activated.");
                BindGrid();
            }
            else if (e.CommandName == "AssignRow")
            {
                DataRow r = _repo.GetSubject(id);
                if (r == null) return;
                hfAssignSubject.Value = id.ToString();
                litAssignSubject.Text = Server.HtmlEncode(Convert.ToString(r["SubjectName"]));
                BindAssignDropdowns();
                txtAssignPeriods.Text = "4";
                pnlAssign.Visible = true;
            }
        }

        // ---- assign-to-class drawer ----
        private void BindAssignDropdowns()
        {
            ddlAssignClass.Items.Clear();
            foreach (DataRow r in _repo.GetClassesLookup().Rows)
                ddlAssignClass.Items.Add(new ListItem(Convert.ToString(r["ClassName"]), Convert.ToString(r["ClassID"])));

            ddlAssignYear.Items.Clear();
            foreach (DataRow r in _repo.GetAcademicYearsLookup().Rows)
                ddlAssignYear.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
            int active = _repo.GetActiveAcademicYearId();
            if (active > 0 && ddlAssignYear.Items.FindByValue(active.ToString()) != null)
                ddlAssignYear.SelectedValue = active.ToString();
        }

        protected void btnCancelAssign_Click(object sender, EventArgs e) { pnlAssign.Visible = false; }

        protected void btnSaveAssign_Click(object sender, EventArgs e)
        {
            int subjectId = int.Parse(hfAssignSubject.Value);
            int classId, year, periods;
            int.TryParse(ddlAssignClass.SelectedValue, out classId);
            int.TryParse(ddlAssignYear.SelectedValue, out year);
            int.TryParse(txtAssignPeriods.Text, out periods);
            try
            {
                _repo.AssignSubjectToClass(subjectId, classId, year, periods);
                Show(true, "Subject assigned to the class. Set the teacher under Teacher Assignments.");
                pnlAssign.Visible = false;
                BindGrid();
            }
            catch (Exception ex) { Show(false, ex.Message); pnlAssign.Visible = true; }
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
