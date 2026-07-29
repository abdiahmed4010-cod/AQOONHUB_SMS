using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AQOONHUB_SMS.Modules.Finance
{
    public partial class Payments : System.Web.UI.Page
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

        private object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

        private const int PageSize = 10;
        private int CurrentPage
        {
            get { return ViewState["CurrentPage"] == null ? 1 : (int)ViewState["CurrentPage"]; }
            set { ViewState["CurrentPage"] = value; }
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
                LoadSummaryCards();
                LoadPayments();
            }
        }

        private void LoadSummaryCards()
        {
            string query = @"
                SELECT
                    ISNULL(SUM(CASE WHEN CAST(PaymentDate AS DATE) = CAST(GETDATE() AS DATE) THEN Amount ELSE 0 END), 0) AS Today,
                    ISNULL(SUM(CASE WHEN YEAR(PaymentDate) = YEAR(GETDATE()) AND MONTH(PaymentDate) = MONTH(GETDATE()) THEN Amount ELSE 0 END), 0) AS ThisMonth,
                    ISNULL(SUM(Amount), 0) AS AllTime,
                    COUNT(*) AS TotalCount
                FROM Payments";

            DataTable dt = ExecuteQuery(query);
            if (dt.Rows.Count == 0) return;
            DataRow row = dt.Rows[0];
            lblCollectedToday.Text = "$" + Convert.ToDecimal(row["Today"]).ToString("N2");
            lblCollectedMonth.Text = "$" + Convert.ToDecimal(row["ThisMonth"]).ToString("N2");
            lblCollectedAllTime.Text = "$" + Convert.ToDecimal(row["AllTime"]).ToString("N2");
            lblTotalCount.Text = row["TotalCount"].ToString();
        }

        private void LoadPayments()
        {
            string where = " WHERE 1=1";
            List<SqlParameter> parameters = new List<SqlParameter>();

            string search = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(search))
            {
                where += @" AND (p.ReceiptNo LIKE @Search OR i.InvoiceNo LIKE @Search OR s.FirstName LIKE @Search OR s.LastName LIKE @Search)";
                parameters.Add(new SqlParameter("@Search", "%" + search + "%"));
            }
            if (!string.IsNullOrEmpty(ddlMethod.SelectedValue))
            {
                where += " AND p.PaymentMethod = @Method";
                parameters.Add(new SqlParameter("@Method", ddlMethod.SelectedValue));
            }
            DateTime fromDate, toDate;
            if (DateTime.TryParse(txtFromDate.Text, out fromDate))
            {
                where += " AND p.PaymentDate >= @FromDate";
                parameters.Add(new SqlParameter("@FromDate", fromDate));
            }
            if (DateTime.TryParse(txtToDate.Text, out toDate))
            {
                where += " AND p.PaymentDate <= @ToDate";
                parameters.Add(new SqlParameter("@ToDate", toDate));
            }

            string countQuery = @"
                SELECT COUNT(*) FROM Payments p
                INNER JOIN Invoices i ON p.InvoiceID = i.InvoiceID
                INNER JOIN Students s ON i.StudentID = s.StudentID" + where;
            int totalCount = Convert.ToInt32(ExecuteScalar(countQuery, CloneParams(parameters)));

            int totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            if (totalPages < 1) totalPages = 1;
            if (CurrentPage > totalPages) CurrentPage = totalPages;
            if (CurrentPage < 1) CurrentPage = 1;
            int offset = (CurrentPage - 1) * PageSize;

            string query = @"
                SELECT p.PaymentID, p.ReceiptNo, p.Amount, p.PaymentMethod, p.PaymentDate, p.Notes,
                       i.InvoiceID, i.InvoiceNo,
                       LTRIM(RTRIM(ISNULL(s.FirstName,'') + ' ' + ISNULL(s.LastName,''))) AS StudentName
                FROM Payments p
                INNER JOIN Invoices i ON p.InvoiceID = i.InvoiceID
                INNER JOIN Students s ON i.StudentID = s.StudentID"
                + where + @"
                ORDER BY p.PaymentDate DESC, p.PaymentID DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            List<SqlParameter> pageParams = new List<SqlParameter>(CloneParams(parameters));
            pageParams.Add(new SqlParameter("@Offset", offset));
            pageParams.Add(new SqlParameter("@PageSize", PageSize));

            DataTable dt = ExecuteQuery(query, pageParams.ToArray());
            gvPayments.DataSource = dt;
            gvPayments.DataBind();

            int shownFrom = totalCount == 0 ? 0 : offset + 1;
            int shownTo = Math.Min(offset + PageSize, totalCount);
            lblResultsSummary.Text = string.Format("Showing {0}–{1} of {2}", shownFrom, shownTo, totalCount);
            lblPageIndicator.Text = string.Format("Page {0} of {1}", CurrentPage, totalPages);
            btnPrevPage.Enabled = CurrentPage > 1;
            btnNextPage.Enabled = CurrentPage < totalPages;
        }

        private SqlParameter[] CloneParams(List<SqlParameter> source)
        {
            List<SqlParameter> clones = new List<SqlParameter>();
            foreach (SqlParameter p in source) clones.Add((SqlParameter)((ICloneable)p).Clone());
            return clones.ToArray();
        }

        protected void btnSearch_Click(object sender, EventArgs e) { CurrentPage = 1; LoadPayments(); }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlMethod.SelectedIndex = 0;
            txtFromDate.Text = "";
            txtToDate.Text = "";
            CurrentPage = 1;
            LoadPayments();
        }

        protected void btnPrevPage_Click(object sender, EventArgs e) { if (CurrentPage > 1) CurrentPage--; LoadPayments(); }
        protected void btnNextPage_Click(object sender, EventArgs e) { CurrentPage++; LoadPayments(); }

        #region Template Helpers

        protected string GetInitials(object nameValue)
        {
            string name = (nameValue == null || nameValue == DBNull.Value) ? null : nameValue.ToString();
            if (string.IsNullOrWhiteSpace(name)) return "ST";
            string[] parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        private static readonly string[] AvatarColors =
        {
            "#2563EB", "#7C3AED", "#0EA5E9", "#22C55E", "#F59E0B", "#EF4444", "#EC4899", "#14B8A6"
        };

        protected string GetAvatarColor(object nameValue)
        {
            string name = (nameValue == null || nameValue == DBNull.Value) ? "" : nameValue.ToString();
            int sum = 0;
            foreach (char c in name) sum += c;
            return AvatarColors[Math.Abs(sum) % AvatarColors.Length];
        }

        #endregion
    }
}
