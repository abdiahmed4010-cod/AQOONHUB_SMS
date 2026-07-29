using System;
using System.Data;
using System.Text;

namespace AQOONHUB_SMS.Modules.Finance
{
    public partial class FeeReports : System.Web.UI.Page
    {
        readonly FeeRepository repo = new FeeRepository();

        DataTable Data() { return repo.GetInvoices("", status.SelectedValue); }

        protected void Page_Load(object s, EventArgs e)
        {
            if (Session["UserID"] == null) Response.Redirect("~/Modules/Authentication/Login.aspx");
            if (!IsPostBack)
            {
                LoadSummary();
                Bind();
                if (Request.QueryString["export"] == "csv") Export();
            }
        }

        void LoadSummary()
        {
            try
            {
                DataRow row = repo.GetDashboardSummary().Rows[0];
                litTotal.Text = Convert.ToInt32(row["TotalInvoices"]).ToString("N0");
                litCollected.Text = "$" + Convert.ToDecimal(row["CollectedThisMonth"]).ToString("N2");
                litOutstanding.Text = "$" + Convert.ToDecimal(row["Outstanding"]).ToString("N2");
                litSuccess.Text = Convert.ToDecimal(row["SuccessRate"]).ToString("N2") + "%";
            }
            catch { /* summary is best-effort */ }
        }

        void Bind() { grid.DataSource = Data(); grid.DataBind(); }

        protected void view_Click(object s, EventArgs e) { LoadSummary(); Bind(); }

        protected void csv_Click(object s, EventArgs e) { Export(); }

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

        void Export()
        {
            DataTable d = Data();
            StringBuilder b = new StringBuilder();
            foreach (DataColumn c in d.Columns) b.Append('"').Append(c.ColumnName.Replace("\"", "\"\"")).Append("\",");
            b.AppendLine();
            foreach (DataRow r in d.Rows)
            {
                foreach (object v in r.ItemArray) b.Append('"').Append(Convert.ToString(v).Replace("\"", "\"\"")).Append("\",");
                b.AppendLine();
            }
            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment; filename=fee-report.csv");
            Response.Write(b.ToString());
            Response.End();
        }
    }
}
