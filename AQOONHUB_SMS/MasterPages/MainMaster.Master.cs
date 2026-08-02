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
            // Canonical session key set by Login is "UserID" (fallback to legacy "UserId").
            object v = Session["UserID"] ?? Session["UserId"];
            if (v != null && int.TryParse(v.ToString(), out int userId))
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
            // Canonical session key set by Login is "Role" (fallback to legacy "UserRole").
            return (Session["Role"] as string) ?? (Session["UserRole"] as string) ?? "guest";
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

        /// <summary>Normalized role from the actual login session key (Session["Role"]).</summary>
        private string NormRole()
        {
            string r = Session["Role"] as string ?? "";
            return r.Trim().Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();
        }

        /// <summary>Parent/guardian users see only their own children's attendance, not management pages.</summary>
        public bool IsParentRole { get { string r = NormRole(); return r == "parent" || r == "guardian"; } }

        /// <summary>Whether the current user may see the centralized Reports menu (management roles only).</summary>
        public bool CanAccessReports
        {
            get { return AQOONHUB_SMS.Modules.Reports.ReportAuthorization.CanAccessReports(Session["Role"] as string); }
        }

        public bool CanAdminSystem { get { return AQOONHUB_SMS.Modules.Administration.SystemAuthorization.IsAdministrator(Session["Role"] as string); } }
        public bool CanViewSystemSecurity { get { return AQOONHUB_SMS.Modules.Administration.SystemAuthorization.CanViewSecurity(Session["Role"] as string); } }

        /// <summary>Role-filtered Reports category links whose pages already exist (label, url, navkey, icon).</summary>
        public System.Collections.Generic.List<System.Tuple<string, string, string, string>> ReportsMenu
        {
            get
            {
                var role = Session["Role"] as string;
                var list = new System.Collections.Generic.List<System.Tuple<string, string, string, string>>();
                foreach (string cat in AQOONHUB_SMS.Modules.Reports.ReportAuthorization.AllCategories)
                {
                    if (cat == AQOONHUB_SMS.Modules.Reports.ReportAuthorization.Overview) continue;
                    if (!AQOONHUB_SMS.Modules.Reports.ReportAuthorization.PageExists(cat)) continue;       // only built pages
                    if (!AQOONHUB_SMS.Modules.Reports.ReportAuthorization.CanViewCategory(role, cat)) continue; // role filter
                    list.Add(System.Tuple.Create(
                        AQOONHUB_SMS.Modules.Reports.ReportAuthorization.Label(cat),
                        AQOONHUB_SMS.Modules.Reports.ReportAuthorization.PageUrl(cat),
                        "reports-" + cat.ToLowerInvariant(),
                        AQOONHUB_SMS.Modules.Reports.ReportAuthorization.Icon(cat)));
                }
                return list;
            }
        }
    }
}
