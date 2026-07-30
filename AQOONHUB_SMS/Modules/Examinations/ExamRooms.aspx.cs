using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Examinations
{
    public partial class ExamRooms : System.Web.UI.Page
    {
        private readonly ExaminationsRepository _repo = new ExaminationsRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack) BindAll();
        }

        private bool Authorize()
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return false; }
            if (!_repo.CanManage(Convert.ToString(Session["Role"]))) { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return false; }
            return true;
        }

        private void BindAll()
        {
            gvRooms.DataSource = _repo.GetExamRoomsAll(); gvRooms.DataBind();
            gvInvig.DataSource = _repo.GetActiveInvigilators(); gvInvig.DataBind();
        }

        protected void btnClear_Click(object sender, EventArgs e) { ResetForm(); }

        private void ResetForm()
        {
            hfId.Value = "0"; litFormTitle.Text = "Add Room";
            txtName.Text = txtLocation.Text = ""; txtCapacity.Text = "40"; ddlStatus.SelectedValue = "Active";
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int id = int.Parse(hfId.Value);
            int cap; int.TryParse(txtCapacity.Text, out cap);
            try
            {
                _repo.SaveExamRoom(id, txtName.Text, cap, txtLocation.Text.Trim(), ddlStatus.SelectedValue);
                Show(true, id > 0 ? "Room updated." : "Room created.");
                ResetForm(); BindAll();
            }
            catch (Exception ex) { Show(false, ex.Message); }
        }

        protected void gvRooms_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id) || id <= 0) return;
            if (e.CommandName == "EditRow")
            {
                DataRow r = _repo.GetExamRoom(id);
                if (r == null) return;
                hfId.Value = id.ToString(); litFormTitle.Text = "Edit Room";
                txtName.Text = Convert.ToString(r["RoomName"]);
                txtCapacity.Text = Convert.ToString(r["Capacity"]);
                txtLocation.Text = r["Location"] == DBNull.Value ? "" : Convert.ToString(r["Location"]);
                ddlStatus.SelectedValue = r["Status"] == DBNull.Value ? "Active" : Convert.ToString(r["Status"]);
            }
            else if (e.CommandName == "ToggleRow")
            {
                DataRow r = _repo.GetExamRoom(id);
                if (r == null) return;
                bool active = string.Equals(Convert.ToString(r["Status"]), "Active", StringComparison.OrdinalIgnoreCase);
                _repo.SetExamRoomStatus(id, active ? "Inactive" : "Active");
                Show(true, active ? "Room deactivated." : "Room activated.");
                BindAll();
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
