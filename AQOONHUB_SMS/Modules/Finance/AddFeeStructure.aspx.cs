using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Finance
{
    public partial class AddFeeStructure : System.Web.UI.Page
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
                LoadCategories();
                LoadAcademicYears();
                LoadClasses();
            }
        }

        private void LoadCategories()
        {
            ddlCategory.Items.Clear();
            ddlCategory.Items.Add(new ListItem("Select Category", ""));
            // Categories are sourced from the Fee Categories the user creates.
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
            foreach (DataRow row in dt.Rows)
            {
                if (row["Status"].ToString() == "Active")
                {
                    ListItem item = ddlAcademicYear.Items.FindByValue(row["AcademicYearID"].ToString());
                    if (item != null) { ddlAcademicYear.ClearSelection(); item.Selected = true; }
                    break;
                }
            }
        }

        private void LoadClasses()
        {
            DataTable dt = ExecuteQuery("SELECT ClassID, ClassName FROM Classes ORDER BY ClassName");
            foreach (DataRow row in dt.Rows)
                ddlClass.Items.Add(new ListItem(row["ClassName"].ToString(), row["ClassID"].ToString()));
        }

        private bool ValidateFee(out string errorMessage)
        {
            errorMessage = null;
            if (string.IsNullOrEmpty(ddlCategory.SelectedValue)) { errorMessage = "Please select a category."; return false; }
            if (ddlClass.SelectedValue == "0") { errorMessage = "Please select a class."; return false; }
            if (ddlAcademicYear.SelectedValue == "0") { errorMessage = "Please select an academic year."; return false; }
            if (string.IsNullOrEmpty(ddlBillingTerm.SelectedValue)) { errorMessage = "Please select a billing term."; return false; }

            decimal amount;
            if (!decimal.TryParse(txtAmount.Text, out amount) || amount <= 0)
            { errorMessage = "Please provide a valid amount."; return false; }

            if (ddlDiscountType.SelectedValue != "No Discount")
            {
                decimal disc;
                if (!decimal.TryParse(txtDiscount.Text, out disc) || disc < 0)
                { errorMessage = "Please provide a valid discount amount."; return false; }
                if (ddlDiscountType.SelectedValue == "Percentage" && disc > 100)
                { errorMessage = "Percentage discount cannot exceed 100%."; return false; }
                if (ddlDiscountType.SelectedValue == "Fixed Amount" && disc > amount)
                { errorMessage = "Fixed discount cannot exceed the amount."; return false; }
            }

            return true;
        }

        protected void ddlDiscountType_Changed(object sender, EventArgs e)
        {
            bool hasDiscount = ddlDiscountType.SelectedValue != "No Discount";
            txtDiscount.Enabled = hasDiscount;
            if (!hasDiscount) txtDiscount.Text = "0";
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!CanManageFinance()) { ShowError("You do not have permission to add fee structures."); return; }
            if (!Page.IsValid) return;

            string validationError;
            if (!ValidateFee(out validationError)) { ShowError(validationError); return; }

            // Saved into ClassFeeStructures so the new Fee Management module
            // (Create Invoice / dashboard) picks the structure up by FeeCategoryID.
            try
            {
                repo.SaveFeeStructure(
                    int.Parse(ddlAcademicYear.SelectedValue),
                    int.Parse(ddlClass.SelectedValue),
                    null, // section (all sections)
                    int.Parse(ddlCategory.SelectedValue),
                    ddlBillingTerm.SelectedValue,
                    decimal.Parse(txtAmount.Text),
                    ddlDiscountType.SelectedValue,
                    ParseDiscount(),
                    null,
                    ddlIsActive.SelectedValue == "1");
                Response.Redirect("~/Modules/Finance/FeeStructures.aspx", true);
            }
            catch (System.Threading.ThreadAbortException) { throw; }
            catch (SqlException sqlEx) when (sqlEx.Number == 50001)
            {
                ShowError("An active fee structure already exists for this class, section and category.");
            }
            catch (Exception)
            {
                ShowError("The fee structure could not be saved due to a system error. Please try again.");
            }
        }

        private decimal ParseDiscount()
        {
            if (ddlDiscountType.SelectedValue == "No Discount") return 0m;
            decimal d;
            return decimal.TryParse(txtDiscount.Text, out d) && d > 0 ? d : 0m;
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
