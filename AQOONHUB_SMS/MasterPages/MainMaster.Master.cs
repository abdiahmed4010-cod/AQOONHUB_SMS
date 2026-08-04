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
            // Forced password change gate: a user flagged MustChangePassword may not browse any
            // master-based module. Only the standalone Change Password page (no master) and Login
            // are reachable, so there is no redirect loop.
            if (Session["MustChangePassword"] is bool && (bool)Session["MustChangePassword"])
            {
                Response.Redirect("~/Modules/Authentication/ChangePassword.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

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

        public string GetCurrentUserEmail()
        {
            return Session["UserEmail"] as string ?? string.Empty;
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
        public bool CanAccessCommunication { get { return AQOONHUB_SMS.Modules.Communication.CommunicationAuthorization.CanView(Session["Role"] as string); } }
        public bool CanUseOutboundCommunication { get { return AQOONHUB_SMS.Modules.Communication.CommunicationAuthorization.CanUseOutbound(Session["Role"] as string); } }

        // ============================================================
        // Sidebar navigation — role-group visibility (view filtering only;
        // each target page still enforces its own server-side authorization).
        // ============================================================
        private string RoleN { get { return NormRole(); } }

        /// <summary>Student Management group: student/guardian/admission administration.</summary>
        public bool CanAccessStudentMgmt { get { string r = RoleN; return r == "superadmin" || r == "admin" || r == "registrar"; } }

        /// <summary>Teachers &amp; Staff directory (HR) — hidden from teachers.</summary>
        public bool CanAccessStaff { get { string r = RoleN; return r == "superadmin" || r == "admin" || r == "academic" || r == "registrar"; } }

        /// <summary>Academic Management group (academics, classes, attendance, examinations).</summary>
        public bool CanAccessAcademicMgmt { get { string r = RoleN; return r == "superadmin" || r == "admin" || r == "academic" || r == "registrar" || r == "teacher" || r == "examofficer"; } }

        /// <summary>Finance Management group (fees + payroll).</summary>
        public bool CanAccessFinanceMgmt { get { string r = RoleN; return r == "superadmin" || r == "admin" || r == "accountant"; } }

        /// <summary>Server-computed active navigation key (longest route-prefix match, query string ignored).</summary>
        private string _activeKey;
        public string ActiveKey { get { if (_activeKey == null) _activeKey = ComputeActiveKey(); return _activeKey; } }

        private string ComputeActiveKey()
        {
            string p = (Request.AppRelativeCurrentExecutionFilePath ?? "").ToLowerInvariant(); // e.g. ~/modules/students/students.aspx
            var map = new[]
            {
                new[] {"~/modules/academic/classessections","classes"},
                new[] {"~/modules/academic/","academics"},
                new[] {"~/modules/attendance/parentattendance","myattendance"},
                new[] {"~/modules/attendance/","attendance"},
                new[] {"~/modules/students/","students"},
                new[] {"~/modules/parents/","guardians"},
                new[] {"~/modules/admission/","admissions"},
                new[] {"~/modules/staff/","staff"},
                new[] {"~/modules/examinations/","examinations"},
                new[] {"~/modules/finance/pendingfinancesetup","finance-setup"},
                new[] {"~/modules/finance/","finance"},
                new[] {"~/modules/payroll/","payroll"},
                new[] {"~/modules/reports/","reports"},
                new[] {"~/modules/communication/overview","comm-overview"},
                new[] {"~/modules/communication/announcements","comm-announcements"},
                new[] {"~/modules/communication/messages","comm-messages"},
                new[] {"~/modules/communication/smsemail","comm-sms"},
                new[] {"~/modules/communication/deliverylogs","comm-logs"},
                new[] {"~/modules/administration/users","users"},
                new[] {"~/modules/administration/auditlog","auditlog"},
                new[] {"~/modules/administration/loginactivity","loginactivity"},
                new[] {"~/modules/settings/","settings"},
                new[] {"~/modules/dashboard/","dashboard"},
            };
            string best = "dashboard"; int bestLen = -1;
            foreach (var m in map)
            {
                if (p.StartsWith(m[0], StringComparison.Ordinal) && m[0].Length > bestLen) { best = m[1]; bestLen = m[0].Length; }
            }
            return best;
        }

        /// <summary>Group id owning a child key (used to auto-expand the active parent).</summary>
        public string GroupOf(string key)
        {
            switch (key)
            {
                case "students": case "guardians": case "admissions": return "student";
                case "staff": case "academics": case "classes": case "attendance": case "myattendance": case "examinations": return "academic";
                case "finance": case "finance-setup": case "payroll": return "finance";
                case "reports": return "reports";
                case "comm-overview": case "comm-announcements": case "comm-messages": case "comm-sms": case "comm-logs": return "comm";
                case "users": case "auditlog": case "loginactivity": case "settings": return "system";
                default: return "";
            }
        }

        /// <summary>True when the given dropdown group contains the active route.</summary>
        public bool IsGroupOpen(string groupId) { return GroupOf(ActiveKey) == groupId; }

        /// <summary>CSS class for a child link (adds 'active' on the current route).</summary>
        public string ChildClass(string key) { return key == ActiveKey ? "nav-item active" : "nav-item"; }

        /// <summary>Renders aria-current="page" only on the active child.</summary>
        public string AriaCurrent(string key) { return key == ActiveKey ? "page" : null; }

        /// <summary>Safe initials from the authenticated user's name (no hard-coded values).</summary>
        public string GetUserInitials()
        {
            string name = (GetCurrentUserName() ?? "").Trim();
            if (string.IsNullOrEmpty(name) || name == "Guest") return "U";
            var parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string ini = "";
            for (int i = 0; i < parts.Length && ini.Length < 2; i++)
            {
                if (!string.IsNullOrEmpty(parts[i])) ini += char.ToUpperInvariant(parts[i][0]);
            }
            return string.IsNullOrEmpty(ini) ? "U" : ini;
        }

        /// <summary>Friendly display label for the authenticated user's role.</summary>
        public string GetRoleDisplay()
        {
            switch (RoleN)
            {
                case "superadmin": return "Super Admin";
                case "admin": return "Administrator";
                case "academic": return "Academic";
                case "registrar": return "Registrar";
                case "accountant": return "Accountant";
                case "teacher": return "Teacher";
                case "parent": case "guardian": return "Parent";
                case "security": return "Security";
                case "examofficer": return "Exam Officer";
                default: return "User";
            }
        }

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
