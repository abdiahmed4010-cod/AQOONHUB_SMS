using System;
using System.Data;
using System.Globalization;
using System.Web;

namespace AQOONHUB_SMS.Modules.Attendance
{
    public partial class AttendanceSettings : System.Web.UI.Page
    {
        private readonly AttendanceRepository _repo = new AttendanceRepository();

        private string Role { get { return Convert.ToString(Session["Role"]); } }
        private int UserId { get { int v; return int.TryParse(Convert.ToString(Session["UserID"]), out v) ? v : 0; } }
        private bool CanEdit { get { return _repo.CanEditSettings(Role); } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack)
            {
                pnlReadOnly.Visible = !CanEdit;
                btnSave.Visible = CanEdit;
                LoadSettings();
            }
        }

        private bool Authorize()
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return false; }
            // Managers, teachers and registrar may view; only managers can save (enforced in btnSave_Click).
            if (!_repo.CanViewAttendance(Role)) { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return false; }
            return true;
        }

        private static string T(object time)
        {
            if (time == null || time == DBNull.Value) return "07:00";
            TimeSpan ts = (TimeSpan)time;
            return ts.ToString(@"hh\:mm");
        }

        private void LoadSettings()
        {
            DataRow s = _repo.GetAttendanceSettings();
            chkAllowTeachers.Checked = Convert.ToBoolean(s["AllowTeachersToMark"]);
            chkAllowEdit.Checked = Convert.ToBoolean(s["AllowEditAfterSubmission"]);
            txtEditWindow.Text = Convert.ToString(s["EditWindowHours"]);
            txtStart.Text = T(s["AttendanceStartTime"]);
            txtEnd.Text = T(s["AttendanceEndTime"]);
            txtLateAfter.Text = Convert.ToString(s["LateAfterMinutes"]);
            chkExcusedRemarks.Checked = Convert.ToBoolean(s["ExcusedRequiresRemarks"]);
            chkFuture.Checked = Convert.ToBoolean(s["AllowFutureDate"]);
            chkIncludeLate.Checked = Convert.ToBoolean(s["IncludeLateAsAttended"]);
            chkExcludeExcused.Checked = Convert.ToBoolean(s["ExcludeExcusedFromRate"]);
            txtConsecutive.Text = Convert.ToString(s["ConsecutiveAbsenceAlert"]);
            txtLowThreshold.Text = Convert.ToDecimal(s["LowAttendanceThreshold"]).ToString("0.##", CultureInfo.InvariantCulture);

            bool ro = !CanEdit;
            chkAllowTeachers.Enabled = chkAllowEdit.Enabled = chkExcusedRemarks.Enabled = chkFuture.Enabled =
                chkIncludeLate.Enabled = chkExcludeExcused.Enabled = chkParent.Enabled = chkEmail.Enabled = chkSms.Enabled = !ro;
            txtEditWindow.Enabled = txtStart.Enabled = txtEnd.Enabled = txtLateAfter.Enabled =
                txtConsecutive.Enabled = txtLowThreshold.Enabled = !ro;

            chkParent.Checked = Convert.ToBoolean(s["EnableParentNotifications"]);
            chkEmail.Checked = Convert.ToBoolean(s["EnableEmailNotifications"]);
            chkSms.Checked = Convert.ToBoolean(s["EnableSMSNotifications"]);
        }

        private static TimeSpan ParseTime(string v, TimeSpan fallback)
        {
            TimeSpan ts;
            return TimeSpan.TryParse(v, CultureInfo.InvariantCulture, out ts) ? ts : fallback;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!CanEdit) { Show(false, "You are not authorized to change attendance settings."); return; }
            try
            {
                int editWindow, lateAfter, consecutive;
                decimal lowThreshold;
                if (!int.TryParse(txtEditWindow.Text.Trim(), out editWindow)) throw new ArgumentException("Edit window must be a whole number.");
                if (!int.TryParse(txtLateAfter.Text.Trim(), out lateAfter)) throw new ArgumentException("Late-after minutes must be a whole number.");
                if (!int.TryParse(txtConsecutive.Text.Trim(), out consecutive)) throw new ArgumentException("Consecutive absence threshold must be a whole number.");
                if (!decimal.TryParse(txtLowThreshold.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out lowThreshold))
                    throw new ArgumentException("Low attendance threshold must be a number.");

                TimeSpan start = ParseTime(txtStart.Text.Trim(), new TimeSpan(7, 0, 0));
                TimeSpan end = ParseTime(txtEnd.Text.Trim(), new TimeSpan(10, 0, 0));

                _repo.SaveAttendanceSettings(
                    chkAllowTeachers.Checked, chkAllowEdit.Checked, editWindow,
                    start, end, lateAfter,
                    chkExcusedRemarks.Checked, chkIncludeLate.Checked, chkExcludeExcused.Checked,
                    chkFuture.Checked, chkParent.Checked, chkEmail.Checked, chkSms.Checked,
                    consecutive, lowThreshold, UserId);

                Show(true, "Attendance settings saved.");
            }
            catch (Exception ex) { Show(false, ex.Message); }
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
