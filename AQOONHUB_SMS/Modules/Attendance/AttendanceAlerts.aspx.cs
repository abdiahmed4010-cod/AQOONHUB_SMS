using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Attendance
{
    public partial class AttendanceAlerts : System.Web.UI.Page
    {
        private readonly AttendanceRepository _repo = new AttendanceRepository();
        private string Role { get { return Convert.ToString(Session["Role"]); } }
        private int UserId { get { int v; return int.TryParse(Convert.ToString(Session["UserID"]), out v) ? v : 0; } }
        public bool CanManage { get { return _repo.CanManageAttendance(Role); } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack) { btnGenerate.Visible = CanManage; LoadData(); }
        }

        private bool Authorize()
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return false; }
            if (!_repo.UserCanViewAttendanceAlerts(Role)) { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return false; }
            return true;
        }

        protected string SevStyle(string s)
        {
            switch ((s ?? "").ToLowerInvariant())
            {
                case "critical": return "background:#FEE2E2;color:#DC2626";
                case "warning": return "background:#FEF3C7;color:#B45309";
                default: return "background:#E0F2FE;color:#0369A1";
            }
        }
        protected string StatusStyle(string s)
        {
            switch ((s ?? "").ToLowerInvariant())
            {
                case "new": return "background:#FEE2E2;color:#DC2626";
                case "reviewed": return "background:#FEF3C7;color:#B45309";
                case "resolved": return "background:#DCFCE7;color:#15803D";
                case "dismissed": return "background:#F1F5F9;color:#64748B";
                default: return "background:#F1F5F9;color:#64748B";
            }
        }

        private void LoadData()
        {
            DataRow s = _repo.GetAttendanceAlertSummary();
            litTotal.Text = Convert.ToString(s["Total"]); litNew.Text = Convert.ToString(s["NewCount"]);
            litReviewed.Text = Convert.ToString(s["ReviewedCount"]); litResolved.Text = Convert.ToString(s["ResolvedCount"]);
            litCritical.Text = Convert.ToString(s["CriticalActive"]);
            gv.DataSource = _repo.GetAttendanceAlerts(ddlType.SelectedValue, ddlStatus.SelectedValue, ddlSeverity.SelectedValue);
            gv.DataBind();
        }

        protected void btnFilter_Click(object sender, EventArgs e) { LoadData(); }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            if (!CanManage) { Show(false, "You are not authorized to generate alerts."); return; }
            try
            {
                int year = _repo.GetActiveAcademicYearId();
                int n = _repo.GenerateAttendanceAlerts(year);
                Show(true, "Alerts regenerated. " + n + " new alert(s) created; existing active alerts refreshed.");
                LoadData();
            }
            catch (Exception ex) { Show(false, ex.Message); }
        }

        protected void gv_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!CanManage) { Show(false, "You are not authorized."); return; }
            int id; if (!int.TryParse(Convert.ToString(e.CommandArgument), out id)) return;
            try
            {
                if (e.CommandName == "Review") { _repo.UpdateAttendanceAlertStatus(id, "Reviewed", UserId, Role); Show(true, "Alert marked Reviewed."); LoadData(); }
                else if (e.CommandName == "Dismiss") { _repo.UpdateAttendanceAlertStatus(id, "Dismissed", UserId, Role); Show(true, "Alert dismissed."); LoadData(); }
                else if (e.CommandName == "Resolve")
                {
                    hfResolveId.Value = id.ToString(); litResolveId.Text = id.ToString();
                    txtNotes.Text = ""; pnlResolve.Visible = true;
                }
            }
            catch (Exception ex) { Show(false, ex.Message); }
        }

        protected void btnResolveConfirm_Click(object sender, EventArgs e)
        {
            if (!CanManage) { Show(false, "You are not authorized."); return; }
            int id; if (!int.TryParse(hfResolveId.Value, out id)) { pnlResolve.Visible = false; return; }
            try { _repo.ResolveAttendanceAlert(id, txtNotes.Text.Trim(), UserId, Role); pnlResolve.Visible = false; Show(true, "Alert resolved."); LoadData(); }
            catch (Exception ex) { Show(false, ex.Message); }
        }
        protected void btnResolveCancel_Click(object sender, EventArgs e) { pnlResolve.Visible = false; }

        private void Show(bool ok, string text)
        {
            msg.Visible = true;
            msg.CssClass = "rounded-lg p-3 mb-4 text-sm " + (ok ? "bg-emerald-50 text-emerald-800 border border-emerald-200" : "bg-amber-50 text-amber-800 border border-amber-200");
            msgText.Text = HttpUtility.HtmlEncode(text);
        }
    }
}
