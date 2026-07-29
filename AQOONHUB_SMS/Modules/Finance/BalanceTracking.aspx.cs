using System;
using System.Data;

namespace AQOONHUB_SMS.Modules.Finance
{
    public partial class BalanceTracking : System.Web.UI.Page
    {
        protected void Page_Load(object s, EventArgs e)
        {
            if (Session["UserID"] == null) Response.Redirect("~/Modules/Authentication/Login.aspx");
            if (!IsPostBack)
            {
                DataTable d = new FeeRepository().GetInvoices("", "");
                d.DefaultView.RowFilter = "Balance > 0";
                grid.DataSource = d.DefaultView;
                grid.DataBind();
            }
        }

        protected string DaysOverdue(object due)
        {
            if (due == null || due == DBNull.Value) return "";
            DateTime d = Convert.ToDateTime(due).Date;
            int days = (DateTime.Today - d).Days;
            return days > 0 ? days + " day" + (days == 1 ? "" : "s") : "";
        }

        protected string StatusStyle(object statusValue)
        {
            switch (Convert.ToString(statusValue))
            {
                case "Paid": return "background:#DCFCE7;color:#15803D";
                case "Partial": return "background:#FEF3C7;color:#B45309";
                case "Unpaid": return "background:#E2E8F0;color:#475569";
                case "Overdue": return "background:#FEE2E2;color:#DC2626";
                default: return "background:#F1F5F9;color:#64748B";
            }
        }
    }
}
