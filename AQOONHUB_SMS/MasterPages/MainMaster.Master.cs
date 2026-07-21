using System;

namespace AQOONHUB_SMS.MasterPages
{
    public partial class MainMaster : System.Web.UI.MasterPage
    {
        protected void Page_Init(object sender, EventArgs e)
        {
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var contentTitle = Page.Title;
                if (!string.IsNullOrEmpty(contentTitle) && contentTitle != "AQOONHUB — School Management System")
                {
                    Page.Title = contentTitle + " — AQOONHUB";
                }
            }
            ApplyUserTheme();
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
        }

        private void ApplyUserTheme()
        {
            string theme = Request.Cookies["aqh_theme"]?.Value;
            if (string.IsNullOrEmpty(theme))
            {
                theme = Session["UserTheme"] as string;
            }
            if (string.IsNullOrEmpty(theme))
            {
                theme = "light";
            }
        }

        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            try
            {
                LogAuditAction("LOGOUT", "Auth", "User signed out");

                Session.Clear();
                Session.Abandon();

                if (Request.Cookies[".ASPXAUTH"] != null)
                {
                    var authCookie = new System.Web.HttpCookie(".ASPXAUTH")
                    {
                        Expires = DateTime.Now.AddDays(-1),
                        Value = string.Empty
                    };
                    Response.Cookies.Add(authCookie);
                }

                Response.Redirect("~/Modules/Authentication/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (System.Threading.ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");
                Response.Redirect("~/Modules/Authentication/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        public void ShowToast(string message, string type = "success", string title = "")
        {
            string encodedMessage = System.Web.HttpUtility.JavaScriptStringEncode(message);
            string encodedType = System.Web.HttpUtility.JavaScriptStringEncode(type);
            string encodedTitle = string.IsNullOrEmpty(title) ? "null" : ("'" + System.Web.HttpUtility.JavaScriptStringEncode(title) + "'");

            string script = string.Format(
                "if(window.showToast){{ window.showToast('{0}','{1}',{2}); }}",
                encodedMessage,
                encodedType,
                encodedTitle);

            System.Web.UI.ScriptManager.RegisterStartupScript(this, GetType(), "toast_" + Guid.NewGuid().ToString("N"), script, true);
        }

        public void ShowAlert(string message, string type = "info")
        {
            string encodedMessage = System.Web.HttpUtility.JavaScriptStringEncode(message);
            string script = string.Format("alert('{0}');", encodedMessage);
            System.Web.UI.ScriptManager.RegisterStartupScript(this, GetType(), "alert_" + Guid.NewGuid().ToString("N"), script, true);
        }

        private void LogAuditAction(string action, string module, string detail)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[AUDIT] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | {action} | {module} | {detail}");
            }
            catch
            {
            }
        }

        private int GetCurrentUserId()
        {
            if (Session["UserId"] != null && int.TryParse(Session["UserId"].ToString(), out int userId))
            {
                return userId;
            }
            return 0;
        }

        public string GetCurrentUserName()
        {
            return Session["UserName"] as string ?? "Guest";
        }

        public string GetCurrentUserRole()
        {
            return Session["UserRole"] as string ?? "guest";
        }

        public bool IsInRole(string role)
        {
            var userRole = GetCurrentUserRole();
            return string.Equals(userRole, role, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsInAnyRole(params string[] roles)
        {
            var userRole = GetCurrentUserRole();
            foreach (var role in roles)
            {
                if (string.Equals(userRole, role, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}