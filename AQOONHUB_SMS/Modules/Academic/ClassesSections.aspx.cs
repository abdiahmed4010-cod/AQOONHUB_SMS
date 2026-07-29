using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Academic
{
    public partial class ClassesSections : System.Web.UI.Page
    {
        private readonly AcademicsRepository _repo = new AcademicsRepository();

        private int SelectedClassId
        {
            get { object o = ViewState["sel"]; return o == null ? 0 : (int)o; }
            set { ViewState["sel"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack)
            {
                BindYears();
                BindClasses();
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

        // ---- helpers used by markup ----
        protected bool IsSelected(object classId)
        {
            return classId != null && Convert.ToInt32(classId) == SelectedClassId;
        }

        protected string StatusStyle(string status)
        {
            switch ((status ?? "").ToLowerInvariant())
            {
                case "active": return "background:#DCFCE7;color:#15803D";
                case "inactive": return "background:#FEF3C7;color:#B45309";
                case "archived": return "background:#F1F5F9;color:#64748B";
                default: return "background:#F1F5F9;color:#64748B";
            }
        }

        private int? YearFilter()
        {
            int y;
            return int.TryParse(ddlYearFilter.SelectedValue, out y) && y > 0 ? y : (int?)null;
        }

        // ---- binding ----
        private void BindYears()
        {
            DataTable dt = _repo.GetAcademicYearsLookup();
            ddlYearFilter.Items.Clear();
            ddlYearFilter.Items.Add(new ListItem("All Years", ""));
            foreach (DataRow r in dt.Rows)
                ddlYearFilter.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
            int active = _repo.GetActiveAcademicYearId();
            if (active > 0 && ddlYearFilter.Items.FindByValue(active.ToString()) != null)
                ddlYearFilter.SelectedValue = active.ToString();
        }

        private void BindClasses()
        {
            DataTable dt = _repo.GetClasses(YearFilter(), txtSearch.Text.Trim(), ddlStatusFilter.SelectedValue);
            rptClasses.DataSource = dt;
            rptClasses.DataBind();
            pnlNoClasses.Visible = dt.Rows.Count == 0;

            long totSec = 0, totStu = 0, totCap = 0;
            foreach (DataRow r in dt.Rows)
            {
                totSec += Convert.ToInt64(r["SectionCount"]);
                totStu += Convert.ToInt64(r["StudentCount"]);
                totCap += Convert.ToInt64(r["Capacity"]);
            }
            litTotClasses.Text = dt.Rows.Count.ToString();
            litTotSections.Text = totSec.ToString();
            litTotStudents.Text = totStu.ToString();
            litOccupancy.Text = totCap > 0 ? Math.Round(totStu * 100.0 / totCap, 1) + "%" : "0%";

            if (SelectedClassId > 0) BindSections();
        }

        private void BindSections()
        {
            DataRow c = _repo.GetClass(SelectedClassId);
            if (c == null) { pnlSections.Visible = false; return; }
            litSelClass.Text = Server.HtmlEncode(Convert.ToString(c["ClassName"]));
            gvSections.DataSource = _repo.GetSections(SelectedClassId);
            gvSections.DataBind();
            pnlSections.Visible = true;
        }

        // ---- filters / selection ----
        protected void btnFilter_Click(object sender, EventArgs e) { BindClasses(); }

        protected void rptClasses_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Select")
            {
                SelectedClassId = Convert.ToInt32(e.CommandArgument);
                BindClasses();
                BindSections();
            }
        }

        // ---- class drawer ----
        protected void btnAddClass_Click(object sender, EventArgs e)
        {
            hfClassId.Value = "0";
            litClassTitle.Text = "Add Class";
            txtClassName.Text = txtClassCode.Text = txtClassCapacity.Text = "";
            ddlLevel.SelectedIndex = 0; ddlClassStatus.SelectedValue = "Active";
            BindYearDropdown(ddlClassYear, _repo.GetActiveAcademicYearId());
            pnlClassDrawer.Visible = true;
        }

        protected void btnCancelClass_Click(object sender, EventArgs e) { pnlClassDrawer.Visible = false; }

        protected void btnSaveClass_Click(object sender, EventArgs e)
        {
            int id = int.Parse(hfClassId.Value);
            int capacity, year;
            int.TryParse(txtClassCapacity.Text, out capacity);
            int.TryParse(ddlClassYear.SelectedValue, out year);
            if (year <= 0) { ShowAndReopenClass("Please select an academic year."); return; }

            try
            {
                _repo.SaveClass(id, txtClassName.Text, txtClassCode.Text, ddlLevel.SelectedValue, capacity,
                    null, year, ddlClassStatus.SelectedValue);
                Show(true, id > 0 ? "Class updated." : "Class created.");
                pnlClassDrawer.Visible = false;
                BindClasses();
            }
            catch (Exception ex) { ShowAndReopenClass(ex.Message); }
        }

        private void ShowAndReopenClass(string text) { Show(false, text); pnlClassDrawer.Visible = true; }

        // ---- section drawer ----
        protected void btnAddSection_Click(object sender, EventArgs e)
        {
            if (SelectedClassId <= 0) { Show(false, "Select a class first."); return; }
            hfSectionId.Value = "0";
            litSectionTitle.Text = "Add New Section";
            txtSectionName.Text = txtSectionRoom.Text = txtSectionCapacity.Text = "";
            ddlSectionStatus.SelectedValue = "Active";
            BindClassDropdown(ddlSectionClass, SelectedClassId);
            BindTeacherDropdown(ddlSectionTeacher, 0);
            DataRow c = _repo.GetClass(SelectedClassId);
            int y = c != null && c["AcademicYearID"] != DBNull.Value ? Convert.ToInt32(c["AcademicYearID"]) : _repo.GetActiveAcademicYearId();
            BindYearDropdown(ddlSectionYear, y);
            pnlSectionDrawer.Visible = true;
        }

        protected void btnCancelSection_Click(object sender, EventArgs e) { pnlSectionDrawer.Visible = false; }

        protected void gvSections_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id) || id <= 0) return;

            if (e.CommandName == "EditSection")
            {
                DataRow r = _repo.GetSection(id);
                if (r == null) return;
                hfSectionId.Value = id.ToString();
                litSectionTitle.Text = "Edit Section";
                BindClassDropdown(ddlSectionClass, Convert.ToInt32(r["ClassID"]));
                txtSectionName.Text = Convert.ToString(r["SectionName"]);
                BindTeacherDropdown(ddlSectionTeacher, r["StaffID"] == DBNull.Value ? 0 : Convert.ToInt32(r["StaffID"]));
                txtSectionRoom.Text = Convert.ToString(r["RoomNumber"]);
                txtSectionCapacity.Text = Convert.ToString(r["Capacity"]);
                int y = r["AcademicYearID"] == DBNull.Value ? _repo.GetActiveAcademicYearId() : Convert.ToInt32(r["AcademicYearID"]);
                BindYearDropdown(ddlSectionYear, y);
                ddlSectionStatus.SelectedValue = r["Status"] == DBNull.Value ? "Active" : Convert.ToString(r["Status"]);
                pnlSectionDrawer.Visible = true;
            }
            else if (e.CommandName == "ArchiveSection")
            {
                try { _repo.SetSectionStatus(id, "Archived"); Show(true, "Section archived."); BindSections(); }
                catch (Exception ex) { Show(false, ex.Message); }
            }
        }

        protected void btnSaveSection_Click(object sender, EventArgs e)
        {
            int id = int.Parse(hfSectionId.Value);
            int classId, capacity, year, teacher;
            int.TryParse(ddlSectionClass.SelectedValue, out classId);
            int.TryParse(txtSectionCapacity.Text, out capacity);
            int.TryParse(ddlSectionYear.SelectedValue, out year);
            int.TryParse(ddlSectionTeacher.SelectedValue, out teacher);
            if (classId <= 0) { ShowAndReopenSection("Please select a class."); return; }
            if (year <= 0) { ShowAndReopenSection("Please select an academic year."); return; }

            // Section year must match class year (server-side, do not trust hidden values)
            DataRow cls = _repo.GetClass(classId);
            if (cls != null && cls["AcademicYearID"] != DBNull.Value && Convert.ToInt32(cls["AcademicYearID"]) != year)
            { ShowAndReopenSection("The section's academic year must match the class's academic year."); return; }

            try
            {
                _repo.SaveSection(id, classId, txtSectionName.Text, teacher > 0 ? teacher : (int?)null,
                    txtSectionRoom.Text, capacity, year, ddlSectionStatus.SelectedValue);
                Show(true, id > 0 ? "Section updated." : "Section created.");
                pnlSectionDrawer.Visible = false;
                SelectedClassId = classId;
                BindClasses();
                BindSections();
            }
            catch (Exception ex) { ShowAndReopenSection(ex.Message); }
        }

        private void ShowAndReopenSection(string text) { Show(false, text); pnlSectionDrawer.Visible = true; }

        // ---- dropdown helpers ----
        private void BindYearDropdown(DropDownList ddl, int selected)
        {
            ddl.Items.Clear();
            foreach (DataRow r in _repo.GetAcademicYearsLookup().Rows)
                ddl.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
            if (selected > 0 && ddl.Items.FindByValue(selected.ToString()) != null)
                ddl.SelectedValue = selected.ToString();
        }

        private void BindClassDropdown(DropDownList ddl, int selected)
        {
            ddl.Items.Clear();
            foreach (DataRow r in _repo.GetClassesLookup().Rows)
                ddl.Items.Add(new ListItem(Convert.ToString(r["ClassName"]), Convert.ToString(r["ClassID"])));
            if (selected > 0 && ddl.Items.FindByValue(selected.ToString()) != null)
                ddl.SelectedValue = selected.ToString();
        }

        private void BindTeacherDropdown(DropDownList ddl, int selected)
        {
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem("— No class teacher —", "0"));
            foreach (DataRow r in _repo.GetActiveStaff().Rows)
                ddl.Items.Add(new ListItem(Convert.ToString(r["FullName"]), Convert.ToString(r["StaffID"])));
            if (selected > 0 && ddl.Items.FindByValue(selected.ToString()) != null)
                ddl.SelectedValue = selected.ToString();
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
