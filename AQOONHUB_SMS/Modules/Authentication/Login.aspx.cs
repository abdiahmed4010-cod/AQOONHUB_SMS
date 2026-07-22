using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using System.Configuration;
using System.Web;
using System.Diagnostics;

namespace AQOONHUB_SMS.Modules.Authentication
{
    public partial class Login : System.Web.UI.Page
    {
        private string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["AQOONHUB_DB"].ConnectionString; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                pnlError.Visible = false;
                lblErrorMessage.Text = string.Empty;

                if (User.Identity.IsAuthenticated && Session["IsAuthenticated"] != null)
                {
                    Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                string returnUrl = Request.QueryString["ReturnUrl"];
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    if (IsLocalUrl(returnUrl))
                    {
                        ViewState["ReturnUrl"] = returnUrl;
                    }
                }
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            pnlError.Visible = false;
            lblErrorMessage.Text = string.Empty;

            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(email))
            {
                ShowError("Please enter your email address.");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Please enter your password.");
                return;
            }

            if (!IsValidEmail(email))
            {
                ShowError("Please enter a valid email address.");
                return;
            }

            UserAuthResult authResult = AuthenticateUser(email, password);

            if (!authResult.IsAuthenticated)
            {
                ShowError(authResult.ErrorMessage);
                return;
            }

            if (!authResult.IsActive)
            {
                ShowError("Account disabled. Please contact the administrator.");
                return;
            }

            bool createPersistentCookie = chkRememberMe.Checked;
            FormsAuthentication.SetAuthCookie(authResult.Email, createPersistentCookie);

            Session["UserID"] = authResult.UserID;
            Session["UserName"] = authResult.FullName;
            Session["UserEmail"] = authResult.Email;
            Session["Role"] = authResult.Role;
            Session["IsAuthenticated"] = true;
            Session["LoginTime"] = DateTime.Now;

            if (createPersistentCookie)
            {
                HttpCookie authCookie = FormsAuthentication.GetAuthCookie(authResult.Email, true);
                authCookie.Expires = DateTime.Now.AddDays(30);
                Response.Cookies.Add(authCookie);
            }

            try
            {
                LogLoginActivity(authResult.UserID, authResult.Email);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Login] Audit logging failed (non-critical): " + ex.ToString());
            }

            string returnUrl = ViewState["ReturnUrl"] as string;
            if (!string.IsNullOrEmpty(returnUrl) && IsLocalUrl(returnUrl))
            {
                Response.Redirect(returnUrl, false);
            }
            else
            {
                Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", false);
            }
            Context.ApplicationInstance.CompleteRequest();
        }

        #region Authentication Logic

        private UserAuthResult AuthenticateUser(string email, string password)
        {
            UserAuthResult result = new UserAuthResult();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            UserID,
                            FullName,
                            Email,
                            PasswordHash,
                            Role,
                            IsActive
                        FROM dbo.Users
                        WHERE Email = @Email";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                result.IsAuthenticated = false;
                                result.ErrorMessage = "Invalid email or password.";
                                return result;
                            }

                            result.UserID = reader["UserID"] != DBNull.Value ? Convert.ToInt32(reader["UserID"]) : 0;
                            result.FullName = reader["FullName"] != DBNull.Value ? reader["FullName"].ToString() : string.Empty;
                            result.Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : string.Empty;
                            string storedPassword = reader["PasswordHash"] != DBNull.Value ? reader["PasswordHash"].ToString() : string.Empty;
                            result.Role = reader["Role"] != DBNull.Value ? reader["Role"].ToString() : string.Empty;
                            result.IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]);

                            bool passwordValid = VerifyPassword(password, storedPassword);

                            if (!passwordValid)
                            {
                                result.IsAuthenticated = false;
                                result.ErrorMessage = "Invalid email or password.";
                                return result;
                            }

                            result.IsAuthenticated = true;
                            result.ErrorMessage = string.Empty;
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Debug.WriteLine("[Login] SQL Exception: " + sqlEx.ToString());
                result.IsAuthenticated = false;
                result.ErrorMessage = "Database error. Please contact support.";
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Login] General Exception: " + ex.ToString());
                result.IsAuthenticated = false;
                result.ErrorMessage = "An error occurred during login. Please try again.";
            }

            return result;
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(password))
                return false;

            if (storedHash.Contains(":"))
            {
                string[] parts = storedHash.Split(':');
                if (parts.Length == 3)
                {
                    return VerifyPBKDF2Password(password, storedHash);
                }
            }

            return password == storedHash;
        }

        private bool VerifyPBKDF2Password(string password, string storedHash)
        {
            try
            {
                string[] parts = storedHash.Split(':');
                if (parts.Length != 3)
                    return false;

                int iterations = int.Parse(parts[0]);
                byte[] salt = Convert.FromBase64String(parts[1]);
                byte[] storedHashBytes = Convert.FromBase64String(parts[2]);

                using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(
                    password, salt, iterations, HashAlgorithmName.SHA256))
                {
                    byte[] computedHash = pbkdf2.GetBytes(storedHashBytes.Length);
                    return ConstantTimeEquals(storedHashBytes, computedHash);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Login] PBKDF2 verification failed: " + ex.ToString());
                return false;
            }
        }

        private bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }

        private void LogLoginActivity(int userId, string email)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    // Query-ga waxaa loo qasacay columns-ka jira: UserID, IPAddress, LoginTime, Status
                    // Haddii aad rabto Email iyo Device, soo kordhi table-ka:
                    // ALTER TABLE LoginActivity ADD Email NVARCHAR(255), Device NVARCHAR(500);
                    string query = @"
                        INSERT INTO LoginActivity (UserID, IPAddress, LoginTime, Status)
                        VALUES (@UserID, @IPAddress, @LoginTime, 'Success')";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@IPAddress", GetClientIPAddress());
                        cmd.Parameters.AddWithValue("@LoginTime", DateTime.Now);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Login] LoginActivity insert failed: " + ex.ToString());
            }
        }

        private string GetClientIPAddress()
        {
            string ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ip))
            {
                ip = Request.ServerVariables["REMOTE_ADDR"];
            }
            if (string.IsNullOrEmpty(ip))
            {
                ip = Request.UserHostAddress;
            }
            return ip ?? "Unknown";
        }

        #endregion

        #region Helper Methods

        private void ShowError(string message)
        {
            lblErrorMessage.Text = message;
            pnlError.Visible = true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsLocalUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            if (url.StartsWith("//"))
                return false;

            if (Uri.TryCreate(url, UriKind.Absolute, out Uri absoluteUri))
            {
                return string.Equals(
                    Request.Url.Host,
                    absoluteUri.Host,
                    StringComparison.OrdinalIgnoreCase);
            }

            return url.StartsWith("/") || url.StartsWith("~");
        }

        #endregion
    }

    public class UserAuthResult
    {
        public bool IsAuthenticated { get; set; }
        public bool IsActive { get; set; }
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string ErrorMessage { get; set; }
    }
}