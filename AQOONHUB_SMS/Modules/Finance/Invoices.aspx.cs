using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Finance
{
    public partial class Invoices : System.Web.UI.Page
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

        #region Authorization

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

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!CheckAuthorization()) return;

            if (!IsPostBack)
            {
                LoadAcademicYears();
                LoadTerms();
                LoadSummaryCards();
                LoadInvoices();
            }
        }

        private void LoadAcademicYears()
        {
            DataTable dt = ExecuteQuery("SELECT AcademicYearID, YearName FROM AcademicYears ORDER BY StartDate DESC");
            foreach (DataRow row in dt.Rows)
                ddlAcademicYear.Items.Add(new ListItem(row["YearName"].ToString(), row["AcademicYearID"].ToString()));
        }

        private void LoadTerms()
        {
            DataTable dt = ExecuteQuery("SELECT TermID, TermName FROM Terms ORDER BY StartDate DESC");
            foreach (DataRow row in dt.Rows)
                ddlTerm.Items.Add(new ListItem(row["TermName"].ToString(), row["TermID"].ToString()));
        }

        private void LoadSummaryCards()
        {
            string query = @"
                SELECT
                    ISNULL(SUM(TotalAmount), 0) AS TotalInvoiced,
                    ISNULL(SUM(PaidAmount), 0) AS Collected,
                    ISNULL(SUM(TotalAmount - PaidAmount), 0) AS Outstanding,
                    SUM(CASE WHEN DueDate < CAST(GETDATE() AS DATE) AND PaidAmount < TotalAmount THEN 1 ELSE 0 END) AS OverdueCount
                FROM Invoices";

            DataTable dt = ExecuteQuery(query);
            if (dt.Rows.Count == 0) return;
            DataRow row = dt.Rows[0];
            lblTotalInvoiced.Text = "$" + Convert.ToDecimal(row["TotalInvoiced"]).ToString("N2");
            lblCollected.Text = "$" + Convert.ToDecimal(row["Collected"]).ToString("N2");
            lblOutstanding.Text = "$" + Convert.ToDecimal(row["Outstanding"]).ToString("N2");
            lblOverdueCount.Text = row["OverdueCount"] == DBNull.Value ? "0" : row["OverdueCount"].ToString();
        }

        private void LoadInvoices()
        {
            string where = " WHERE 1=1";
            List<SqlParameter> parameters = new List<SqlParameter>();

            string search = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(search))
            {
                where += @" AND (i.InvoiceNo LIKE @Search OR s.FirstName LIKE @Search OR s.LastName LIKE @Search)";
                parameters.Add(new SqlParameter("@Search", "%" + search + "%"));
            }
            if (ddlAcademicYear.SelectedValue != "0")
            {
                where += " AND i.AcademicYearID = @AcademicYearID";
                parameters.Add(new SqlParameter("@AcademicYearID", int.Parse(ddlAcademicYear.SelectedValue)));
            }
            if (ddlTerm.SelectedValue != "0")
            {
                where += " AND i.TermID = @TermID";
                parameters.Add(new SqlParameter("@TermID", int.Parse(ddlTerm.SelectedValue)));
            }

            string statusFilter = "";
            if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
            {
                if (ddlStatus.SelectedValue == "Overdue")
                    statusFilter = " AND i.DueDate < CAST(GETDATE() AS DATE) AND i.PaidAmount < i.TotalAmount";
                else
                    statusFilter = " AND ISNULL(i.Status, 'Unpaid') = @Status AND NOT (i.DueDate < CAST(GETDATE() AS DATE) AND i.PaidAmount < i.TotalAmount)";
            }
            where += statusFilter;
            if (!string.IsNullOrEmpty(ddlStatus.SelectedValue) && ddlStatus.SelectedValue != "Overdue")
                parameters.Add(new SqlParameter("@Status", ddlStatus.SelectedValue));

            string countQuery = @"
                SELECT COUNT(*) FROM Invoices i
                INNER JOIN Students s ON i.StudentID = s.StudentID" + where;
            int totalCount = Convert.ToInt32(ExecuteScalar(countQuery, CloneParams(parameters)));

            int totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            if (totalPages < 1) totalPages = 1;
            if (CurrentPage > totalPages) CurrentPage = totalPages;
            if (CurrentPage < 1) CurrentPage = 1;
            int offset = (CurrentPage - 1) * PageSize;

            string query = @"
                SELECT
                    i.InvoiceID, i.InvoiceNo, i.TotalAmount, i.PaidAmount, i.DueDate,
                    LTRIM(RTRIM(ISNULL(s.FirstName,'') + ' ' + ISNULL(s.LastName,''))) AS StudentName,
                    s.StudentCode, t.TermName,
                    CASE
                        WHEN i.DueDate < CAST(GETDATE() AS DATE) AND i.PaidAmount < i.TotalAmount THEN 'Overdue'
                        ELSE ISNULL(i.Status, 'Unpaid')
                    END AS DisplayStatus
                FROM Invoices i
                INNER JOIN Students s ON i.StudentID = s.StudentID
                INNER JOIN Terms t ON i.TermID = t.TermID"
                + where + @"
                ORDER BY i.GeneratedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            List<SqlParameter> pageParams = new List<SqlParameter>(CloneParams(parameters));
            pageParams.Add(new SqlParameter("@Offset", offset));
            pageParams.Add(new SqlParameter("@PageSize", PageSize));

            DataTable dt = ExecuteQuery(query, pageParams.ToArray());
            gvInvoices.DataSource = dt;
            gvInvoices.DataBind();

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

        protected void btnSearch_Click(object sender, EventArgs e) { CurrentPage = 1; LoadInvoices(); }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlAcademicYear.SelectedIndex = 0;
            ddlTerm.SelectedIndex = 0;
            ddlStatus.SelectedIndex = 0;
            CurrentPage = 1;
            LoadInvoices();
        }

        protected void btnPrevPage_Click(object sender, EventArgs e) { if (CurrentPage > 1) CurrentPage--; LoadInvoices(); }
        protected void btnNextPage_Click(object sender, EventArgs e) { CurrentPage++; LoadInvoices(); }

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

        protected string GetStatusStyle(object statusValue)
        {
            string status = statusValue == null || statusValue == DBNull.Value ? "" : statusValue.ToString();
            switch (status)
            {
                case "Paid": return "background:#DCFCE7;color:#15803D";
                case "Partially Paid": return "background:#EFF6FF;color:#1D4ED8";
                case "Unpaid": return "background:#F1F5F9;color:#64748B";
                case "Overdue": return "background:#FEE2E2;color:#B91C1C";
                default: return "background:#F1F5F9;color:#64748B";
            }
        }

        #endregion
    }
}
