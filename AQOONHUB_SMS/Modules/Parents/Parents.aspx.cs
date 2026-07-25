using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Parents
{
    public partial class Parents : System.Web.UI.Page
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

        private int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        private const int DefaultPageSize = 10;
        private int CurrentPage
        {
            get { return ViewState["CurrentPage"] == null ? 1 : (int)ViewState["CurrentPage"]; }
            set { ViewState["CurrentPage"] = value; }
        }
        private int PageSize
        {
            get { return ViewState["PageSize"] == null ? DefaultPageSize : (int)ViewState["PageSize"]; }
            set { ViewState["PageSize"] = value; }
        }

        #region Authorization

        private string NormalizeRole(string role)
        {
            if (string.IsNullOrEmpty(role)) return string.Empty;
            return role.Trim().Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();
        }

        private static readonly string[] FullAccessRoles = { "superadmin", "admin", "registrar" };

        private bool CanManageParents()
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
            // Read-only list access for Teacher/Accountant per spec; write actions gated separately.
            return true;
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!CheckAuthorization()) return;

            lnkAddParent.Visible = CanManageParents();

            if (!IsPostBack)
            {
                LoadRelationships();
                LoadSummaryCards();
                LoadParents();
            }
        }

        private void LoadRelationships()
        {
            DataTable dt = ExecuteQuery("SELECT DISTINCT Relationship FROM Guardians WHERE Relationship IS NOT NULL ORDER BY Relationship");
            foreach (DataRow row in dt.Rows)
                ddlRelationship.Items.Add(new ListItem(row["Relationship"].ToString(), row["Relationship"].ToString()));
        }

        private void LoadSummaryCards()
        {
            string query = @"
                SELECT
                    COUNT(*) AS TotalCount,
                    SUM(CASE WHEN g.IsActive = 1 THEN 1 ELSE 0 END) AS ActiveCount,
                    SUM(CASE WHEN g.IsActive = 0 THEN 1 ELSE 0 END) AS InactiveCount,
                    SUM(CASE WHEN linked.GuardianID IS NOT NULL THEN 1 ELSE 0 END) AS WithStudentsCount,
                    SUM(CASE WHEN linked.GuardianID IS NULL THEN 1 ELSE 0 END) AS WithoutStudentsCount
                FROM Guardians g
                LEFT JOIN (SELECT DISTINCT GuardianID FROM Students) linked ON linked.GuardianID = g.GuardianID";

            DataTable dt = ExecuteQuery(query);
            if (dt.Rows.Count == 0) return;
            DataRow row = dt.Rows[0];
            lblTotalCount.Text = SafeInt(row["TotalCount"]).ToString();
            lblActiveCount.Text = SafeInt(row["ActiveCount"]).ToString();
            lblInactiveCount.Text = SafeInt(row["InactiveCount"]).ToString();
            lblWithStudentsCount.Text = SafeInt(row["WithStudentsCount"]).ToString();
            lblWithoutStudentsCount.Text = SafeInt(row["WithoutStudentsCount"]).ToString();
        }

        private int SafeInt(object val) { return val == DBNull.Value ? 0 : Convert.ToInt32(val); }

        private void LoadParents()
        {
            string where = " WHERE 1=1";
            List<SqlParameter> parameters = new List<SqlParameter>();

            string search = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(search))
            {
                where += " AND (g.FullName LIKE @Search OR g.Phone LIKE @Search OR g.Email LIKE @Search)";
                parameters.Add(new SqlParameter("@Search", "%" + search + "%"));
            }
            if (!string.IsNullOrEmpty(ddlRelationship.SelectedValue))
            {
                where += " AND g.Relationship = @Relationship";
                parameters.Add(new SqlParameter("@Relationship", ddlRelationship.SelectedValue));
            }
            if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
            {
                where += " AND g.IsActive = @IsActive";
                parameters.Add(new SqlParameter("@IsActive", ddlStatus.SelectedValue == "1"));
            }
            if (ddlHasStudents.SelectedValue == "1")
            {
                where += " AND EXISTS (SELECT 1 FROM Students s WHERE s.GuardianID = g.GuardianID)";
            }
            else if (ddlHasStudents.SelectedValue == "0")
            {
                where += " AND NOT EXISTS (SELECT 1 FROM Students s WHERE s.GuardianID = g.GuardianID)";
            }

            string countQuery = "SELECT COUNT(*) FROM Guardians g" + where;
            int totalCount = Convert.ToInt32(ExecuteScalar(countQuery, CloneParams(parameters)));

            int totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            if (totalPages < 1) totalPages = 1;
            if (CurrentPage > totalPages) CurrentPage = totalPages;
            if (CurrentPage < 1) CurrentPage = 1;
            int offset = (CurrentPage - 1) * PageSize;

            string query = @"
                SELECT g.GuardianID, g.FullName, g.Relationship, g.Phone, g.Email, g.Occupation, g.IsActive,
                       (SELECT COUNT(*) FROM Students s WHERE s.GuardianID = g.GuardianID) AS StudentCount
                FROM Guardians g"
                + where + @"
                ORDER BY g.FullName
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            List<SqlParameter> pageParams = new List<SqlParameter>(CloneParams(parameters));
            pageParams.Add(new SqlParameter("@Offset", offset));
            pageParams.Add(new SqlParameter("@PageSize", PageSize));

            DataTable dt = ExecuteQuery(query, pageParams.ToArray());
            gvParents.DataSource = dt;
            gvParents.DataBind();

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

        protected void btnSearch_Click(object sender, EventArgs e) { CurrentPage = 1; LoadParents(); }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlRelationship.SelectedIndex = 0;
            ddlStatus.SelectedIndex = 0;
            ddlHasStudents.SelectedIndex = 0;
            CurrentPage = 1;
            LoadParents();
        }

        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            int size;
            if (int.TryParse(ddlPageSize.SelectedValue, out size)) PageSize = size;
            CurrentPage = 1;
            LoadParents();
        }

        protected void btnPrevPage_Click(object sender, EventArgs e) { if (CurrentPage > 1) CurrentPage--; LoadParents(); }
        protected void btnNextPage_Click(object sender, EventArgs e) { CurrentPage++; LoadParents(); }

        protected void gvParents_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!CanManageParents()) return;

            if (e.CommandName == "ToggleActive")
            {
                string[] parts = e.CommandArgument.ToString().Split('|');
                int guardianId = Convert.ToInt32(parts[0]);
                bool currentActive = parts[1] == "True";

                ExecuteNonQuery(
                    "UPDATE Guardians SET IsActive = @IsActive, UpdatedAt = SYSDATETIME() WHERE GuardianID = @Id",
                    new[] { new SqlParameter("@IsActive", !currentActive), new SqlParameter("@Id", guardianId) });

                LoadSummaryCards();
                LoadParents();
            }
        }

        #region Template Helpers

        protected string GetInitials(object fullNameValue)
        {
            string fullName = (fullNameValue == null || fullNameValue == DBNull.Value) ? null : fullNameValue.ToString();
            if (string.IsNullOrWhiteSpace(fullName)) return "GD";
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

        #endregion
    }
}