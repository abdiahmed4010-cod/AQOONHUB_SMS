using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Finance
{
    public partial class EditFeeStructure : System.Web.UI.Page
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

        private int FeeId
        {
            get { return ViewState["FeeId"] == null ? 0 : (int)ViewState["FeeId"]; }
            set { ViewState["FeeId"] = value; }
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
            if (!CanManageFinance())
            {
                ShowError("You do not have permission to manage fee structures.");
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
                int id;
                int.TryParse(Request.QueryString["id"], out id);
                FeeId = id;

                LoadCategories();
                LoadAcademicYears();
                LoadClasses();
                LoadFee();
            }
        }

        private void LoadCategories()
        {
            ddlCategory.Items.Clear();
            ddlCategory.Items.Add(new ListItem("Select Category", ""));
            DataTable dt = ExecuteQuery("SELECT FeeCategoryID, CategoryName FROM FeeCategories WHERE IsActive = 1 ORDER BY CategoryName");
            foreach (DataRow row in dt.Rows)
                ddlCategory.Items.Add(new ListItem(row["CategoryName"].ToString(), row["FeeCategoryID"].ToString()));
        }

        private void LoadAcademicYears()
        {
            DataTable dt = ExecuteQuery("SELECT AcademicYearID, YearName, Status FROM AcademicYears ORDER BY StartDate DESC");
            ddlAcademicYear.Items.Clear();
            ddlAcademicYear.Items.Add(new ListItem("Select Academic Year", "0"));
            foreach (DataRow row in dt.Rows)
            {
                string label = row["YearName"] + (row["Status"].ToString() == "Active" ? " (Current)" : "");
                ddlAcademicYear.Items.Add(new ListItem(label, row["AcademicYearID"].ToString()));
            }
        }

        private void LoadClasses()
        {
            DataTable dt = ExecuteQuery("SELECT ClassID, ClassName FROM Classes ORDER BY ClassName");
            foreach (DataRow row in dt.Rows)
                ddlClass.Items.Add(new ListItem(row["ClassName"].ToString(), row["ClassID"].ToString()));
        }

        private void LoadFee()
        {
            DataTable dt = ExecuteQuery("SELECT * FROM ClassFeeStructures WHERE ClassFeeStructureID = @Id", new[] { new SqlParameter("@Id", FeeId) });
            if (dt.Rows.Count == 0)
            {
                ShowError("Fee structure not found.");
                return;
            }
            DataRow row = dt.Rows[0];
            txtFeeName.Text = row["Description"] == DBNull.Value ? "" : row["Description"].ToString();
            SelectIfPresent(ddlCategory, row["FeeCategoryID"].ToString());
            ddlAcademicYear.SelectedValue = row["AcademicYearID"].ToString();
            SelectIfPresent(ddlClass, row["ClassID"].ToString());
            txtAmount.Text = Convert.ToDecimal(row["Amount"]).ToString("0.00");
            ddlBillingTerm.SelectedValue = row["BillingTerm"].ToString();
            ddlIsActive.SelectedValue = Convert.ToBoolean(row["IsActive"]) ? "1" : "0";
        }

        private void SelectIfPresent(DropDownList ddl, string value)
        {
            ListItem item = ddl.Items.FindByValue(value);
            if (item != null) { ddl.ClearSelection(); item.Selected = true; }
        }

        private bool ValidateFee(out string errorMessage)
        {
            errorMessage = null;
            if (string.IsNullOrWhiteSpace(txtFeeName.Text)) { errorMessage = "Fee name is required."; return false; }
            if (string.IsNullOrEmpty(ddlCategory.SelectedValue)) { errorMessage = "Please select a category."; return false; }
            if (ddlClass.SelectedValue == "0") { errorMessage = "Please select a class."; return false; }
            if (ddlAcademicYear.SelectedValue == "0") { errorMessage = "Please select an academic year."; return false; }
            if (string.IsNullOrEmpty(ddlBillingTerm.SelectedValue)) { errorMessage = "Please select a billing term."; return false; }

            decimal amount;
            if (!decimal.TryParse(txtAmount.Text, out amount) || amount <= 0)
            { errorMessage = "Please provide a valid amount."; return false; }

            return true;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!CanManageFinance()) { ShowError("You do not have permission to edit fee structures."); return; }
            if (!Page.IsValid) return;

            string validationError;
            if (!ValidateFee(out validationError)) { ShowError(validationError); return; }

            string query = @"
                UPDATE ClassFeeStructures SET
                    FeeCategoryID = @Category, ClassID = @ClassID, Amount = @Amount,
                    BillingTerm = @BillingTerm, AcademicYearID = @AcademicYearID,
                    [Description] = @Description, IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME()
                WHERE ClassFeeStructureID = @Id";

            try
            {
                ExecuteNonQuery(query, new[]
                {
                    new SqlParameter("@Category", int.Parse(ddlCategory.SelectedValue)),
                    new SqlParameter("@ClassID", int.Parse(ddlClass.SelectedValue)),
                    new SqlParameter("@Amount", decimal.Parse(txtAmount.Text)),
                    new SqlParameter("@BillingTerm", ddlBillingTerm.SelectedValue),
                    new SqlParameter("@AcademicYearID", int.Parse(ddlAcademicYear.SelectedValue)),
                    new SqlParameter("@Description", string.IsNullOrWhiteSpace(txtFeeName.Text) ? (object)DBNull.Value : txtFeeName.Text.Trim()),
                    new SqlParameter("@IsActive", ddlIsActive.SelectedValue == "1"),
                    new SqlParameter("@Id", FeeId)
                });
                Response.Redirect("~/Modules/Finance/FeeStructures.aspx", true);
            }
            catch (System.Threading.ThreadAbortException) { throw; }
            catch (Exception)
            {
                ShowError("The fee structure could not be updated due to a system error. Please try again.");
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/Finance/FeeStructures.aspx", true);
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            pnlError.Visible = true;
        }
    }
}