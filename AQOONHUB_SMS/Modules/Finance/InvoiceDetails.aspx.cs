using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AQOONHUB_SMS.Modules.Finance
{
    public partial class InvoiceDetails : System.Web.UI.Page
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["AQOONHUB_DB"].ConnectionString;

        private DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        private int InvoiceId
        {
            get { return ViewState["InvoiceId"] == null ? 0 : (int)ViewState["InvoiceId"]; }
            set { ViewState["InvoiceId"] = value; }
        }

        private bool CheckAuthorization()
        {
            string role = Session["Role"] as string;
            if (string.IsNullOrEmpty(role))
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx", true);
                return false;
            }
            return true;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!CheckAuthorization()) return;

            if (!IsPostBack)
            {
                int id;
                int.TryParse(Request.QueryString["id"], out id);
                InvoiceId = id;

                if (!LoadInvoice())
                {
                    pnlBody.Visible = false;
                    pnlNotFound.Visible = true;
                    return;
                }
                LoadItems();
                LoadPayments();
            }
        }

        private bool LoadInvoice()
        {
            if (InvoiceId <= 0) return false;

            string query = @"
                SELECT i.InvoiceID, i.InvoiceNo, i.TotalAmount, i.PaidAmount, i.DueDate,
                       LTRIM(RTRIM(ISNULL(s.FirstName,'') + ' ' + ISNULL(s.LastName,''))) AS StudentName,
                       s.StudentCode, ay.YearName, t.TermName,
                       CASE
                           WHEN i.DueDate < CAST(GETDATE() AS DATE) AND i.PaidAmount < i.TotalAmount THEN 'Overdue'
                           ELSE ISNULL(i.Status, 'Unpaid')
                       END AS DisplayStatus
                FROM Invoices i
                INNER JOIN Students s ON i.StudentID = s.StudentID
                INNER JOIN AcademicYears ay ON i.AcademicYearID = ay.AcademicYearID
                INNER JOIN Terms t ON i.TermID = t.TermID
                WHERE i.InvoiceID = @Id";

            DataTable dt = ExecuteQuery(query, new[] { new SqlParameter("@Id", InvoiceId) });
            if (dt.Rows.Count == 0) return false;

            DataRow row = dt.Rows[0];
            decimal total = Convert.ToDecimal(row["TotalAmount"]);
            decimal paid = Convert.ToDecimal(row["PaidAmount"]);
            decimal balance = total - paid;
            string status = row["DisplayStatus"].ToString();

            lblInvoiceNo.Text = row["InvoiceNo"].ToString();
            lblStudentInfo.Text = row["StudentName"] + " (" + row["StudentCode"] + ")";
            lblAcademicYear.Text = row["YearName"].ToString();
            lblTerm.Text = row["TermName"].ToString();
            lblDueDate.Text = Convert.ToDateTime(row["DueDate"]).ToString("MMM dd, yyyy");
            lblTotalAmount.Text = "$" + total.ToString("N2");
            lblPaidAmount.Text = "$" + paid.ToString("N2");
            lblBalance.Text = "$" + balance.ToString("N2");

            lblStatusBadge.Text = status;
            ApplyStatusStyle(status);

            lnkRecordPayment.NavigateUrl = ResolveUrl("~/Modules/Finance/AddPayment.aspx?invoiceId=" + InvoiceId);
            lnkRecordPayment.Visible = status != "Paid" && CanManageFinance();

            return true;
        }

        private string NormalizeRole(string role)
        {
            if (string.IsNullOrEmpty(role)) return string.Empty;
            return role.Trim().Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();
        }

        private static readonly string[] FullAccessRoles = { "superadmin", "admin", "accountant" };

        private bool CanManageFinance()
        {
            string normalized = NormalizeRole(Session["Role"] as string);
            foreach (string r in FullAccessRoles) if (normalized == r) return true;
            return false;
        }

        private void ApplyStatusStyle(string status)
        {
            string bg, color;
            switch (status)
            {
                case "Paid": bg = "#DCFCE7"; color = "#15803D"; break;
                case "Partially Paid": bg = "#EFF6FF"; color = "#1D4ED8"; break;
                case "Unpaid": bg = "#F1F5F9"; color = "#64748B"; break;
                case "Overdue": bg = "#FEE2E2"; color = "#B91C1C"; break;
                default: bg = "#F1F5F9"; color = "#64748B"; break;
            }
            lblStatusBadge.Style["background"] = bg;
            lblStatusBadge.Style["color"] = color;
        }

        private void LoadItems()
        {
            DataTable dt = ExecuteQuery(
                "SELECT Description, Amount FROM InvoiceItems WHERE InvoiceID = @Id ORDER BY InvoiceItemID",
                new[] { new SqlParameter("@Id", InvoiceId) });
            gvItems.DataSource = dt;
            gvItems.DataBind();
        }

        private void LoadPayments()
        {
            DataTable dt = ExecuteQuery(
                "SELECT ReceiptNo, Amount, PaymentMethod, PaymentDate, Notes FROM Payments WHERE InvoiceID = @Id ORDER BY PaymentDate DESC",
                new[] { new SqlParameter("@Id", InvoiceId) });
            gvPayments.DataSource = dt;
            gvPayments.DataBind();
        }
    }
}
