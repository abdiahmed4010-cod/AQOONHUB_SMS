using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Academic
{
    public partial class AcademicYears : System.Web.UI.Page
    {
        private readonly AcademicsRepository _repo = new AcademicsRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack) BindGrid();
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

        private void BindGrid()
        {
            gv.DataSource = _repo.GetAcademicYears(txtSearch.Text.Trim(), ddlFilterStatus.SelectedValue);
            gv.DataBind();
        }

        protected string StatusStyle(string status)
        {
            switch ((status ?? "").ToLowerInvariant())
            {
                case "active": return "background:#DCFCE7;color:#15803D";
                case "completed": return "background:#E0E7FF;color:#4338CA";
                case "cancelled": return "background:#FEE2E2;color:#DC2626";
                default: return "background:#F1F5F9;color:#64748B"; // Draft/Closed
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e) { BindGrid(); }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            hfId.Value = "0";
            litFormTitle.Text = "Add Academic Year";
            txtName.Text = txtDesc.Text = "";
            txtStart.Text = txtEnd.Text = "";
            ddlStatus.SelectedValue = "Draft";
            pnlForm.Visible = true;
        }

        protected void btnCancel_Click(object sender, EventArgs e) { pnlForm.Visible = false; }

        protected void gv_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id) || id <= 0) return;

            if (e.CommandName == "EditRow")
            {
                DataRow r = _repo.GetAcademicYear(id);
                if (r == null) return;
                hfId.Value = id.ToString();
                litFormTitle.Text = "Edit Academic Year";
                txtName.Text = Convert.ToString(r["YearName"]);
                txtStart.Text = Convert.ToDateTime(r["StartDate"]).ToString("yyyy-MM-dd");
                txtEnd.Text = Convert.ToDateTime(r["EndDate"]).ToString("yyyy-MM-dd");
                ddlStatus.SelectedValue = Convert.ToString(r["Status"]);
                pnlForm.Visible = true;
            }
            else if (e.CommandName == "Activate")
            {
                try
                {
                    _repo.SetAcademicYearStatus(id, "Active");
                    Show(true, "Academic year set as Active. Any previously active year is now Completed.");
                    BindGrid();
                }
                catch (Exception ex) { Show(false, ex.Message); }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int id = int.Parse(hfId.Value);
            DateTime start, end;
            if (!DateTime.TryParse(txtStart.Text, out start)) { Show(false, "Please provide a valid start date."); pnlForm.Visible = true; return; }
            if (!DateTime.TryParse(txtEnd.Text, out end)) { Show(false, "Please provide a valid end date."); pnlForm.Visible = true; return; }

            try
            {
                _repo.SaveAcademicYear(id, txtName.Text, start, end, ddlStatus.SelectedValue);
                Show(true, id > 0 ? "Academic year updated." : "Academic year created.");
                pnlForm.Visible = false;
                BindGrid();
            }
            catch (Exception ex)
            {
                Show(false, ex.Message);
                pnlForm.Visible = true;
            }
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
