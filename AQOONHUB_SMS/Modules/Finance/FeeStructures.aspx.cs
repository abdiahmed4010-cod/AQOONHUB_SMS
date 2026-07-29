using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Finance
{
    public partial class FeeStructures : System.Web.UI.Page
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["AQOONHUB_DB"].ConnectionString;

        private readonly FeeRepository repo = new FeeRepository();

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

        #region Authorization

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

            lnkAddFee.Visible = CanManageFinance();

            if (!IsPostBack)
            {
                LoadAcademicYears();
                LoadClasses();
                LoadFees();
            }
        }

        private void LoadAcademicYears()
        {
            DataTable dt = ExecuteQuery("SELECT AcademicYearID, YearName FROM AcademicYears ORDER BY StartDate DESC");
            foreach (DataRow row in dt.Rows)
                ddlAcademicYear.Items.Add(new ListItem(row["YearName"].ToString(), row["AcademicYearID"].ToString()));
        }

        private void LoadClasses()
        {
            DataTable dt = ExecuteQuery("SELECT ClassID, ClassName FROM Classes ORDER BY ClassName");
            foreach (DataRow row in dt.Rows)
                ddlClass.Items.Add(new ListItem(row["ClassName"].ToString(), row["ClassID"].ToString()));
        }

        private void LoadFees()
        {
            // Fee structures now live in ClassFeeStructures (the schema the new Fee
            // Management module reads for Create Invoice). Filters are applied by name.
            DataTable dt = repo.GetFeeStructures();
            System.Collections.Generic.List<string> filters = new System.Collections.Generic.List<string>();

            string search = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(search))
            {
                string safe = search.Replace("'", "''");
                filters.Add("(CategoryName LIKE '%" + safe + "%' OR ClassName LIKE '%" + safe + "%' OR ISNULL(Description,'') LIKE '%" + safe + "%')");
            }
            if (ddlAcademicYear.SelectedValue != "0" && ddlAcademicYear.SelectedItem != null)
                filters.Add("YearName = '" + ddlAcademicYear.SelectedItem.Text.Replace("'", "''") + "'");
            if (ddlClass.SelectedValue != "0" && ddlClass.SelectedItem != null)
                filters.Add("ClassName = '" + ddlClass.SelectedItem.Text.Replace("'", "''") + "'");
            if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                filters.Add("StatusText = '" + (ddlStatus.SelectedValue == "1" ? "Active" : "Inactive") + "'");

            if (filters.Count > 0)
            {
                DataView dv = dt.DefaultView;
                dv.RowFilter = string.Join(" AND ", filters);
                dt = dv.ToTable();
            }

            gvFees.DataSource = dt;
            gvFees.DataBind();
        }

        protected void btnSearch_Click(object sender, EventArgs e) { LoadFees(); }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlAcademicYear.SelectedIndex = 0;
            ddlClass.SelectedIndex = 0;
            ddlStatus.SelectedIndex = 0;
            LoadFees();
        }

        protected void gvFees_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!CanManageFinance()) return;
            if (e.CommandName != "ToggleActive") return;

            string[] parts = e.CommandArgument.ToString().Split('|');
            int feeId = Convert.ToInt32(parts[0]);
            bool currentActive = parts[1] == "True";

            ExecuteNonQuery("UPDATE ClassFeeStructures SET IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME() WHERE ClassFeeStructureID = @Id",
                new[] { new SqlParameter("@IsActive", !currentActive), new SqlParameter("@Id", feeId) });

            LoadFees();
        }
    }
}
