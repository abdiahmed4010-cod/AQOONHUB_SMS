using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Academic
{
    public partial class TeacherAssignments : System.Web.UI.Page
    {
        private readonly AcademicsRepository _repo = new AcademicsRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack)
            {
                BindFilterDropdowns();
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

        protected string Initials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            string[] parts = name.Trim().Split(' ');
            string s = parts[0].Substring(0, 1);
            if (parts.Length > 1) s += parts[parts.Length - 1].Substring(0, 1);
            return s.ToUpperInvariant();
        }

        // ---- filters ----
        private void BindFilterDropdowns()
        {
            FillYears(ddlFYear, true);
            FillList(ddlFClass, _repo.GetClassesLookup(), "ClassName", "ClassID", "All Classes");
            FillList(ddlFSubject, _repo.GetSubjectsLookup(), "SubjectName", "SubjectID", "All Subjects");
            FillList(ddlFTeacher, _repo.GetActiveStaff(), "FullName", "StaffID", "All Teachers");
        }

        private int? Nz(DropDownList ddl)
        {
            int v;
            return int.TryParse(ddl.SelectedValue, out v) && v > 0 ? v : (int?)null;
        }

        private void BindGrid()
        {
            gv.DataSource = _repo.GetTeacherAssignments(Nz(ddlFYear), Nz(ddlFClass), null, Nz(ddlFSubject), Nz(ddlFTeacher), txtSearch.Text.Trim());
            gv.DataBind();
        }

        protected void btnFilter_Click(object sender, EventArgs e) { BindGrid(); }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ddlFYear.SelectedIndex = 0; ddlFClass.SelectedIndex = 0; ddlFSubject.SelectedIndex = 0; ddlFTeacher.SelectedIndex = 0;
            txtSearch.Text = "";
            BindGrid();
        }

        // ---- drawer ----
        protected void btnAssign_Click(object sender, EventArgs e)
        {
            hfId.Value = "0";
            litTitle.Text = "Assign Teacher";
            FillYears(ddlYear, false);
            int y = _repo.GetActiveAcademicYearId();
            if (y > 0 && ddlYear.Items.FindByValue(y.ToString()) != null) ddlYear.SelectedValue = y.ToString();
            FillList(ddlClass, _repo.GetClassesLookup(), "ClassName", "ClassID", "— Select class —");
            ddlSection.Items.Clear(); ddlSection.Items.Add(new ListItem("— Select section —", "0"));
            ddlSubject.Items.Clear(); ddlSubject.Items.Add(new ListItem("— Select subject —", "0"));
            FillList(ddlTeacher, _repo.GetActiveStaff(), "FullName", "StaffID", "— Select teacher —");
            txtPeriods.Text = "4"; ddlStatus.SelectedValue = "1";
            pnlDrawer.Visible = true;
        }

        protected void btnCancel_Click(object sender, EventArgs e) { pnlDrawer.Visible = false; }

        protected void ddlYear_Changed(object sender, EventArgs e)
        {
            ReloadClassDependents();
            pnlDrawer.Visible = true;
        }

        protected void ddlClass_Changed(object sender, EventArgs e)
        {
            ReloadClassDependents();
            pnlDrawer.Visible = true;
        }

        private void ReloadClassDependents()
        {
            int classId, year;
            int.TryParse(ddlClass.SelectedValue, out classId);
            int.TryParse(ddlYear.SelectedValue, out year);

            ddlSection.Items.Clear(); ddlSection.Items.Add(new ListItem("— Select section —", "0"));
            ddlSubject.Items.Clear(); ddlSubject.Items.Add(new ListItem("— Select subject —", "0"));
            if (classId > 0)
            {
                foreach (DataRow r in _repo.GetSectionsLookup(classId).Rows)
                    ddlSection.Items.Add(new ListItem(Convert.ToString(r["SectionName"]), Convert.ToString(r["SectionID"])));
                if (year > 0)
                    foreach (DataRow r in _repo.GetSubjectsForClass(classId, year).Rows)
                        ddlSubject.Items.Add(new ListItem(Convert.ToString(r["SubjectName"]), Convert.ToString(r["SubjectID"])));
            }
        }

        protected void gv_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id) || id <= 0) return;

            if (e.CommandName == "EditRow")
            {
                DataRow r = _repo.GetTeacherAssignment(id);
                if (r == null) return;
                hfId.Value = id.ToString();
                litTitle.Text = "Edit Assignment";
                FillYears(ddlYear, false);
                int year = Convert.ToInt32(r["AcademicYearID"]);
                if (ddlYear.Items.FindByValue(year.ToString()) != null) ddlYear.SelectedValue = year.ToString();
                FillList(ddlClass, _repo.GetClassesLookup(), "ClassName", "ClassID", "— Select class —");
                int classId = Convert.ToInt32(r["ClassID"]);
                if (ddlClass.Items.FindByValue(classId.ToString()) != null) ddlClass.SelectedValue = classId.ToString();
                ReloadClassDependents();
                if (ddlSection.Items.FindByValue(Convert.ToString(r["SectionID"])) != null) ddlSection.SelectedValue = Convert.ToString(r["SectionID"]);
                if (ddlSubject.Items.FindByValue(Convert.ToString(r["SubjectID"])) != null) ddlSubject.SelectedValue = Convert.ToString(r["SubjectID"]);
                FillList(ddlTeacher, _repo.GetActiveStaff(), "FullName", "StaffID", "— Select teacher —");
                if (r["StaffID"] != DBNull.Value && ddlTeacher.Items.FindByValue(Convert.ToString(r["StaffID"])) != null) ddlTeacher.SelectedValue = Convert.ToString(r["StaffID"]);
                txtPeriods.Text = Convert.ToString(r["WeeklyPeriods"]);
                ddlStatus.SelectedValue = Convert.ToBoolean(r["IsActive"]) ? "1" : "0";
                pnlDrawer.Visible = true;
            }
            else if (e.CommandName == "RemoveRow")
            {
                try { _repo.RemoveTeacherAssignment(id); Show(true, "Teacher removed. The class-subject link is preserved."); BindGrid(); }
                catch (Exception ex) { Show(false, ex.Message); }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int section, subject, teacher, year, periods;
            int.TryParse(ddlSection.SelectedValue, out section);
            int.TryParse(ddlSubject.SelectedValue, out subject);
            int.TryParse(ddlTeacher.SelectedValue, out teacher);
            int.TryParse(ddlYear.SelectedValue, out year);
            int.TryParse(txtPeriods.Text, out periods);

            if (year <= 0) { Reopen("Please select an academic year."); return; }
            if (section <= 0) { Reopen("Please select a section."); return; }
            if (subject <= 0) { Reopen("Please select a subject."); return; }
            if (teacher <= 0) { Reopen("Please select a teacher."); return; }

            try
            {
                _repo.SaveTeacherAssignment(section, subject, teacher, year, periods, ddlStatus.SelectedValue == "1");
                Show(true, "Assignment saved.");
                pnlDrawer.Visible = false;
                BindGrid();
            }
            catch (Exception ex) { Reopen(ex.Message); }
        }

        private void Reopen(string text) { Show(false, text); pnlDrawer.Visible = true; }

        // ---- helpers ----
        private void FillYears(DropDownList ddl, bool allOption)
        {
            ddl.Items.Clear();
            if (allOption) ddl.Items.Add(new ListItem("All Years", ""));
            foreach (DataRow r in _repo.GetAcademicYearsLookup().Rows)
                ddl.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
        }

        private void FillList(DropDownList ddl, DataTable dt, string text, string val, string first)
        {
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem(first, first.StartsWith("All") ? "" : "0"));
            foreach (DataRow r in dt.Rows)
                ddl.Items.Add(new ListItem(Convert.ToString(r[text]), Convert.ToString(r[val])));
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
