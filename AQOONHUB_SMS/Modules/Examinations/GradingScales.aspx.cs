using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Examinations
{
    public partial class GradingScales : System.Web.UI.Page
    {
        private readonly ExaminationsRepository _repo = new ExaminationsRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack)
            {
                BindYears();
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

        private int YearF() { int v; return int.TryParse(ddlYearFilter.SelectedValue, out v) ? v : 0; }

        private void BindYears()
        {
            ddlYearFilter.Items.Clear();
            ddlYearForm.Items.Clear();
            foreach (DataRow r in _repo.GetAcademicYears().Rows)
            {
                ddlYearFilter.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
                ddlYearForm.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
            }
            int active = _repo.GetActiveAcademicYearId();
            if (active > 0 && ddlYearFilter.Items.FindByValue(active.ToString()) != null) ddlYearFilter.SelectedValue = active.ToString();
        }

        private void BindGrid()
        {
            gv.DataSource = _repo.GetGradingScales(YearF() > 0 ? YearF() : (int?)null);
            gv.DataBind();
        }

        protected void ddlYearFilter_Changed(object sender, EventArgs e) { BindGrid(); }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            hfId.Value = "0";
            litFormTitle.Text = "Add Grade";
            txtLetter.Text = txtDesc.Text = ""; txtMin.Text = txtMax.Text = ""; txtGpa.Text = "0";
            ddlPass.SelectedValue = "1"; ddlStatus.SelectedValue = "Active";
            if (YearF() > 0 && ddlYearForm.Items.FindByValue(YearF().ToString()) != null) ddlYearForm.SelectedValue = YearF().ToString();
            pnlForm.Visible = true;
        }

        protected void btnCancel_Click(object sender, EventArgs e) { pnlForm.Visible = false; }

        protected void gv_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (e.CommandName != "EditRow" || !int.TryParse(Convert.ToString(e.CommandArgument), out id) || id <= 0) return;
            DataRow r = _repo.GetGradingScale(id);
            if (r == null) return;
            hfId.Value = id.ToString();
            litFormTitle.Text = "Edit Grade";
            txtLetter.Text = Convert.ToString(r["GradeLetter"]);
            txtMin.Text = Convert.ToString(r["MinMarks"]);
            txtMax.Text = Convert.ToString(r["MaxMarks"]);
            txtGpa.Text = Convert.ToString(r["GPA"]);
            txtDesc.Text = r["Description"] == DBNull.Value ? "" : Convert.ToString(r["Description"]);
            ddlPass.SelectedValue = Convert.ToBoolean(r["IsPass"]) ? "1" : "0";
            ddlStatus.SelectedValue = r["Status"] == DBNull.Value ? "Active" : Convert.ToString(r["Status"]);
            if (ddlYearForm.Items.FindByValue(Convert.ToString(r["AcademicYearID"])) != null) ddlYearForm.SelectedValue = Convert.ToString(r["AcademicYearID"]);
            pnlForm.Visible = true;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int id = int.Parse(hfId.Value);
            int min, max, year; decimal gpa;
            int.TryParse(txtMin.Text, out min);
            int.TryParse(txtMax.Text, out max);
            int.TryParse(ddlYearForm.SelectedValue, out year);
            decimal.TryParse(txtGpa.Text, out gpa);
            try
            {
                _repo.SaveGradingScale(id, year, txtLetter.Text, min, max, gpa, txtDesc.Text.Trim(),
                    ddlPass.SelectedValue == "1", ddlStatus.SelectedValue);
                Show(true, id > 0 ? "Grade updated." : "Grade created.");
                pnlForm.Visible = false;
                BindGrid();
            }
            catch (Exception ex) { Show(false, ex.Message); pnlForm.Visible = true; }
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
