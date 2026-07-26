using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Staff
{
    public partial class Staff : System.Web.UI.Page
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

        private string NormalizeRole(string role)
        {
            if (string.IsNullOrEmpty(role)) return string.Empty;
            return role.Trim().Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();
        }

        private static readonly string[] FullAccessRoles = { "superadmin", "admin", "hr" };

        private bool CanManageStaff()
        {
            string normalized = NormalizeRole(Session["Role"] as string);
            foreach (string r in FullAccessRoles) if (normalized == r) return true;
            return false;
        }

        private bool CheckAuthorization()
        {
            string role = Session["Role"] as string;
            if (string.IsNullOrEmpty(role))
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx", true);
                return false;
            }
            return true; // read-only list access for any logged-in role; write actions gated separately
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!CheckAuthorization()) return;

            lnkAddStaff.Visible = CanManageStaff();

            if (!IsPostBack)
            {
                LoadDepartments();
                LoadSummaryCards();
                LoadStaff();
            }
        }

        private void LoadDepartments()
        {
            DataTable dt = ExecuteQuery("SELECT DISTINCT Department FROM Staff WHERE Department IS NOT NULL ORDER BY Department");
            foreach (DataRow row in dt.Rows)
                ddlDepartment.Items.Add(new ListItem(row["Department"].ToString(), row["Department"].ToString()));
        }

        private void LoadSummaryCards()
        {
            string query = @"
                SELECT
                    COUNT(*) AS TotalCount,
                    SUM(CASE WHEN Status = 'Active' THEN 1 ELSE 0 END) AS ActiveCount,
                    SUM(CASE WHEN Status = 'On Leave' THEN 1 ELSE 0 END) AS OnLeaveCount,
                    SUM(CASE WHEN Status IN ('Inactive', 'Retired') THEN 1 ELSE 0 END) AS InactiveCount
                FROM Staff";

            DataTable dt = ExecuteQuery(query);
            if (dt.Rows.Count == 0) return;
            DataRow row = dt.Rows[0];
            lblTotalCount.Text = SafeInt(row["TotalCount"]).ToString();
            lblActiveCount.Text = SafeInt(row["ActiveCount"]).ToString();
            lblOnLeaveCount.Text = SafeInt(row["OnLeaveCount"]).ToString();
            lblInactiveCount.Text = SafeInt(row["InactiveCount"]).ToString();
        }

        private int SafeInt(object val) { return val == DBNull.Value ? 0 : Convert.ToInt32(val); }

        private void LoadStaff()
        {
            string where = " WHERE 1=1";
            List<SqlParameter> parameters = new List<SqlParameter>();

            string search = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(search))
            {
                where += " AND (u.FullName LIKE @Search OR s.EmployeeID LIKE @Search OR u.Email LIKE @Search)";
                parameters.Add(new SqlParameter("@Search", "%" + search + "%"));
            }
            if (!string.IsNullOrEmpty(ddlDepartment.SelectedValue))
            {
                where += " AND s.Department = @Department";
                parameters.Add(new SqlParameter("@Department", ddlDepartment.SelectedValue));
            }
            if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
            {
                where += " AND s.Status = @Status";
                parameters.Add(new SqlParameter("@Status", ddlStatus.SelectedValue));
            }

            string countQuery = "SELECT COUNT(*) FROM Staff s INNER JOIN Users u ON s.UserID = u.UserID" + where;
            int totalCount = Convert.ToInt32(ExecuteScalar(countQuery, CloneParams(parameters)));

            int totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            if (totalPages < 1) totalPages = 1;
            if (CurrentPage > totalPages) CurrentPage = totalPages;
            if (CurrentPage < 1) CurrentPage = 1;
            int offset = (CurrentPage - 1) * PageSize;

            string query = @"
                SELECT s.StaffID, s.EmployeeID, u.FullName, u.Email, u.Phone,
                       s.Department, s.Position, s.HireDate, s.Salary, s.LeaveBalance, s.Status
                FROM Staff s
                INNER JOIN Users u ON s.UserID = u.UserID"
                + where + @"
                ORDER BY u.FullName
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            List<SqlParameter> pageParams = new List<SqlParameter>(CloneParams(parameters));
            pageParams.Add(new SqlParameter("@Offset", offset));
            pageParams.Add(new SqlParameter("@PageSize", PageSize));

            DataTable dt = ExecuteQuery(query, pageParams.ToArray());
            gvStaff.DataSource = dt;
            gvStaff.DataBind();

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

        protected void btnSearch_Click(object sender, EventArgs e) { CurrentPage = 1; LoadStaff(); }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlDepartment.SelectedIndex = 0;
            ddlStatus.SelectedIndex = 0;
            CurrentPage = 1;
            LoadStaff();
        }

        protected void btnPrevPage_Click(object sender, EventArgs e) { if (CurrentPage > 1) CurrentPage--; LoadStaff(); }
        protected void btnNextPage_Click(object sender, EventArgs e) { CurrentPage++; LoadStaff(); }

        protected void gvStaff_RowCommand(object sender, GridViewCommandEventArgs e) { }

        #region Template Helpers

        protected string GetInitials(object fullNameValue)
        {
            string fullName = (fullNameValue == null || fullNameValue == DBNull.Value) ? null : fullNameValue.ToString();
            if (string.IsNullOrWhiteSpace(fullName)) return "ST";
            string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        private static readonly string[] AvatarColors =
        {
            "#2563EB", "#7C3AED", "#0EA5E9", "#22C55E", "#F59E0B", "#EF4444", "#EC4899", "#14B8A6"
        };

        protected string GetAvatarColor(object fullNameValue)
        {
            string name = (fullNameValue == null || fullNameValue == DBNull.Value) ? "" : fullNameValue.ToString();
            int sum = 0;
            foreach (char c in name) sum += c;
            return AvatarColors[Math.Abs(sum) % AvatarColors.Length];
        }

        protected string GetStatusStyle(object statusValue)
        {
            string status = statusValue == null || statusValue == DBNull.Value ? "" : statusValue.ToString();
            switch (status)
            {
                case "Active": return "background:#DCFCE7;color:#15803D";
                case "On Leave": return "background:#FFFBEB;color:#B45309";
                case "Inactive": return "background:#F1F5F9;color:#64748B";
                case "Retired": return "background:#EDE9FE;color:#6D28D9";
                default: return "background:#F1F5F9;color:#64748B";
            }
        }

        #endregion
    }
}
