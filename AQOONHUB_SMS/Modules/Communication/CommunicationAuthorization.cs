using System;
using System.Web;
using System.Web.UI;

namespace AQOONHUB_SMS.Modules.Communication
{
    public static class CommunicationAuthorization
    {
        public static string Normalize(string role) { return (role ?? "").Trim().Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant(); }
        public static bool CanView(string role) { string r=Normalize(role); return r=="superadmin"||r=="admin"||r=="academic"||r=="registrar"||r=="accountant"||r=="teacher"||r=="parent"||r=="guardian"||r=="security"; }
        public static bool IsAdmin(string role) { string r=Normalize(role); return r=="superadmin"||r=="admin"; }
        public static bool CanManageAnnouncements(string role) { string r=Normalize(role); return IsAdmin(role)||r=="academic"||r=="registrar"||r=="teacher"||r=="security"; }
        public static bool CanUseOutbound(string role) { string r=Normalize(role); return IsAdmin(role)||r=="academic"||r=="registrar"||r=="accountant"||r=="security"; }
        public static bool CanManageTemplates(string role) { string r=Normalize(role); return IsAdmin(role)||r=="academic"||r=="registrar"||r=="accountant"; }
        public static bool CanViewDeliveryLogs(string role) { return CanUseOutbound(role); }
        public static bool CanViewProviderStatus(string role) { return CanUseOutbound(role); }
        public static bool CanTargetSchoolWide(string role) { return IsAdmin(role); }
        public static bool IsParent(string role) { string r=Normalize(role); return r=="parent"||r=="guardian"; }
        public static void Demand(Page page)
        {
            if (CanView(page.Session["Role"] as string)) return;
            page.Response.StatusCode=403; page.Response.TrySkipIisCustomErrors=true;
            throw new HttpException(403,"Communication access denied.");
        }
        public static void Demand(Page page, Func<string,bool> rule)
        {
            Demand(page); if(rule(page.Session["Role"] as string))return;
            page.Response.StatusCode=403; page.Response.TrySkipIisCustomErrors=true;
            throw new HttpException(403,"Communication action denied.");
        }
    }
}
