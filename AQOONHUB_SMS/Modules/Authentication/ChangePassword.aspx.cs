using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Web.Security;

namespace AQOONHUB_SMS.Modules.Authentication
{
    public partial class ChangePassword : System.Web.UI.Page
    {
        private string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["AQOONHUB_DB"].ConnectionString; }
        }

        /// <summary>Authenticated user id from the SESSION only — never from the query string.</summary>
        private int CurrentUserId
        {
            get { int v; return int.TryParse(Convert.ToString(Session["UserID"]), out v) ? v : 0; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (CurrentUserId <= 0)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx", true);
                return;
            }
            if (!IsPostBack) { pnlError.Visible = false; }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int userId = CurrentUserId;
            if (userId <= 0) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return; }

            string current = txtCurrent.Text;
            string @new = txtNew.Text;
            string confirm = txtConfirm.Text;

            string validationError;
            if (!Validate(current, @new, confirm, out validationError)) { ShowError(validationError); return; }

            // One transaction: lock user row, verify current, write new hash + clear the flag.
            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        try
                        {
                            string storedHash;
                            using (var cmd = new SqlCommand("SELECT PasswordHash FROM dbo.Users WITH (UPDLOCK, HOLDLOCK) WHERE UserID=@id", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@id", userId);
                                object o = cmd.ExecuteScalar();
                                if (o == null || o == DBNull.Value) { tx.Rollback(); ShowError("Your account could not be verified. Please sign in again."); return; }
                                storedHash = Convert.ToString(o);
                            }

                            if (!Parents.ParentAccountService.VerifyPassword(current, storedHash))
                            { tx.Rollback(); ShowError("Your current password is incorrect."); return; }

                            // Reject reusing the same password (compare against the stored hash).
                            if (Parents.ParentAccountService.VerifyPassword(@new, storedHash))
                            { tx.Rollback(); ShowError("Your new password must be different from your current password."); return; }

                            string newHash = Parents.ParentAccountService.HashPassword(@new);
                            using (var cmd = new SqlCommand(
                                "UPDATE dbo.Users SET PasswordHash=@h, MustChangePassword=0, UpdatedAt=GETDATE() WHERE UserID=@id", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@h", newHash);
                                cmd.Parameters.AddWithValue("@id", userId);
                                cmd.ExecuteNonQuery();
                            }

                            using (var cmd = new SqlCommand(
                                "INSERT dbo.AuditLog(UserID,Action,Module,Detail,IPAddress,ActionTime) VALUES(@id,'PASSWORD_CHANGED','Auth','User changed their password',@ip,GETDATE())", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@id", userId);
                                cmd.Parameters.AddWithValue("@ip", (object)(Request.UserHostAddress ?? "Unavailable"));
                                cmd.ExecuteNonQuery();
                            }

                            tx.Commit();
                        }
                        catch
                        {
                            try { tx.Rollback(); } catch { }
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ChangePassword] " + ex);
                ShowError("Your password could not be updated due to a system error. Please try again.");
                return;
            }

            // Clear sensitive form values + flag; then continue to Dashboard.
            txtCurrent.Text = txtNew.Text = txtConfirm.Text = string.Empty;
            Session["MustChangePassword"] = false;
            Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true);
        }

        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();
            Response.Redirect("~/Modules/Authentication/Login.aspx", true);
        }

        private bool Validate(string current, string @new, string confirm, out string error)
        {
            error = null;
            if (string.IsNullOrEmpty(current)) { error = "Please enter your current password."; return false; }
            if (string.IsNullOrEmpty(@new)) { error = "Please enter a new password."; return false; }
            if (string.IsNullOrEmpty(confirm)) { error = "Please confirm your new password."; return false; }
            if (@new != confirm) { error = "The new password and confirmation do not match."; return false; }
            if (@new == current) { error = "Your new password must be different from your current password."; return false; }
            if (@new.Length < 8) { error = "Your new password must be at least 8 characters."; return false; }

            bool hasLetter = false, hasDigit = false;
            foreach (char c in @new) { if (char.IsLetter(c)) hasLetter = true; else if (char.IsDigit(c)) hasDigit = true; }
            if (!hasLetter || !hasDigit) { error = "Your new password must contain both letters and numbers."; return false; }

            string lower = @new.ToLowerInvariant();
            string[] weak = { "password", "12345678", "123456789", "qwerty", "parent123", "aqoonhub", "letmein", "welcome1" };
            foreach (string w in weak) { if (lower == w) { error = "That password is too common. Please choose a stronger password."; return false; } }

            return true;
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            pnlError.Visible = true;
        }
    }
}
