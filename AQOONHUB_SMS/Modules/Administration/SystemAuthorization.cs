using System;
using System.Web;

namespace AQOONHUB_SMS.Modules.Administration
{
    public static class SystemAuthorization
    {
        public static string Normalize(string role) { return (role ?? "").Trim().Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant(); }
        public static bool IsAdministrator(string role) { string r = Normalize(role); return r == "superadmin" || r == "admin"; }
        public static bool CanViewSecurity(string role) { string r = Normalize(role); return IsAdministrator(role) || r == "security"; }
        public static void DemandAdmin(System.Web.UI.Page page)
        {
            if (IsAdministrator(page.Session["Role"] as string)) return;
            page.Response.StatusCode = 403;
            page.Response.Redirect("~/Modules/Dashboard/Dashboard.aspx?denied=system", false);
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
        public static void DemandSecurity(System.Web.UI.Page page)
        {
            if (CanViewSecurity(page.Session["Role"] as string)) return;
            page.Response.StatusCode = 403;
            page.Response.Redirect("~/Modules/Dashboard/Dashboard.aspx?denied=system", false);
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
    }
}
