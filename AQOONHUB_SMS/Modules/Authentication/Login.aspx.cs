using System;

namespace AQOONHUB_SMS.Modules.Authentication
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Clear any previous error messages
                pnlError.Visible = false;
                lblErrorMessage.Text = string.Empty;

                // Check if there's a return URL
                string returnUrl = Request.QueryString["ReturnUrl"];
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    // Store return URL for redirect after successful login
                    // Only store if it's a local URL to prevent open redirect attacks
                    if (IsLocalUrl(returnUrl))
                    {
                        ViewState["ReturnUrl"] = returnUrl;
                    }
                }
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            // Clear previous error messages
            pnlError.Visible = false;
            lblErrorMessage.Text = string.Empty;

            // Get input values
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();

            // Validate email/username
            if (string.IsNullOrEmpty(email))
            {
                ShowError("Please enter your email address.");
                return;
            }

            // Validate password
            if (string.IsNullOrEmpty(password))
            {
                ShowError("Please enter your password.");
                return;
            }

            // Basic email format validation
            if (!IsValidEmail(email))
            {
                ShowError("Please enter a valid email address.");
                return;
            }

            // TODO: Replace with actual authentication logic
            // For now, accept any non-empty credentials as temporary behavior
            // In production, this should validate against SQL Server database

            // Temporary validation: accept any email with @ and password >= 4 chars
            if (password.Length < 4)
            {
                ShowError("Password must be at least 4 characters long.");
                return;
            }

            // Successful login (temporary behavior)
            // TODO: Implement real authentication, session management, and role assignment

            // Store basic session info (temporary)
            Session["UserEmail"] = email;
            Session["IsAuthenticated"] = true;
            Session["LoginTime"] = DateTime.Now;

            // Handle "Remember Me" option
            if (chkRememberMe.Checked)
            {
                // TODO: Implement persistent cookie for "Remember Me"
                // This should create a secure, encrypted persistent cookie
            }

            // Redirect to dashboard or return URL
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

        /// <summary>
        /// Displays an error message to the user.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        private void ShowError(string message)
        {
            lblErrorMessage.Text = message;
            pnlError.Visible = true;
        }

        /// <summary>
        /// Performs basic email format validation.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if the email format is valid; otherwise, false.</returns>
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

        /// <summary>
        /// Validates that a URL is local to the current application.
        /// Prevents open redirect attacks by rejecting external URLs.
        /// Compatible with ASP.NET Web Forms .NET Framework 4.8.
        /// </summary>
        /// <param name="url">The URL to validate.</param>
        /// <returns>True if the URL is local; otherwise, false.</returns>
        private bool IsLocalUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            // Check if it's an absolute URL (http://, https://, //)
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri absoluteUri))
            {
                // Compare with current request host
                return string.Equals(
                    Request.Url.Host,
                    absoluteUri.Host,
                    StringComparison.OrdinalIgnoreCase);
            }

            // Relative URLs: must start with / or ~ and not start with //
            // Reject protocol-relative URLs (//evil.com)
            if (url.StartsWith("//"))
            {
                return false;
            }

            // Allow application-relative paths (~/...) and absolute paths (/...)
            return url.StartsWith("/") || url.StartsWith("~");
        }
    }
}