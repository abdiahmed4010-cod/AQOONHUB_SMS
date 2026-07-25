using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Parents
{
    public partial class AddParent : System.Web.UI.Page
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
            if (!CanManageParents())
            {
                ShowError("You do not have permission to add parents. This page is available to Super Admin, Admin, and Registrar roles only.");
                pnlFormBody.Visible = false;
                return false;
            }
            return true;
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            CheckAuthorization();
        }

        private bool ValidateParent(out string errorMessage)
        {
            errorMessage = null;
            if (string.IsNullOrWhiteSpace(txtFullName.Text)) { errorMessage = "Full name is required."; return false; }
            if (string.IsNullOrEmpty(ddlRelationship.SelectedValue)) { errorMessage = "Please select a relationship."; return false; }
            if (string.IsNullOrWhiteSpace(txtPhone.Text)) { errorMessage = "Phone is required."; return false; }
            if (txtPhone.Text.Trim().Length > 30) { errorMessage = "Phone must be 30 characters or fewer."; return false; }
            if (txtAddress.Text.Length > 200) { errorMessage = "Address must be 200 characters or fewer."; return false; }
            return true;
        }

        private bool CheckDuplicateAndWarn()
        {
            string phone = txtPhone.Text.Trim();
            string email = txtEmail.Text.Trim();

            DataTable dt = ExecuteQuery(
                "SELECT FullName FROM Guardians WHERE Phone = @Phone OR (@Email <> '' AND Email = @Email)",
                new[] { new SqlParameter("@Phone", phone), new SqlParameter("@Email", email) });

            if (dt.Rows.Count > 0)
            {
                lblDuplicateWarning.Text = "A guardian with this phone or email may already exist: " + dt.Rows[0]["FullName"] +
                    ". Review Parents.aspx before creating a duplicate, or save again to continue anyway.";
                pnlDuplicateWarning.Visible = true;
                return true;
            }
            pnlDuplicateWarning.Visible = false;
            return false;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int? newId = SaveParent();
            if (newId.HasValue) Response.Redirect("~/Modules/Parents/ParentDetails.aspx?id=" + newId, true);
        }

        protected void btnSaveAndAddAnother_Click(object sender, EventArgs e)
        {
            int? newId = SaveParent();
            if (newId.HasValue)
            {
                ShowSuccess("Guardian saved successfully. Form has been reset for the next entry.");
                ClearForm();
            }
        }

        private int? SaveParent()
        {
            if (!CanManageParents()) { ShowError("You do not have permission to add parents."); return null; }
            if (!Page.IsValid) return null;

            string validationError;
            if (!ValidateParent(out validationError)) { ShowError(validationError); return null; }

            // Duplicate detection warns rather than blocks: the first submit that finds a
            // possible match shows the warning and stops; pressing Save again proceeds,
            // since two legitimate different guardians can share a phone/email pattern.
            if (!pnlDuplicateWarning.Visible && CheckDuplicateAndWarn())
            {
                return null;
            }

            string query = @"
                INSERT INTO Guardians
                    (FullName, Relationship, Phone, AlternatePhone, Email, NationalID, Occupation,
                     EmergencyContact, Address, IsActive, CreatedAt, UpdatedAt)
                OUTPUT INSERTED.GuardianID
                VALUES
                    (@FullName, @Relationship, @Phone, @AlternatePhone, @Email, @NationalID, @Occupation,
                     @EmergencyContact, @Address, @IsActive, SYSDATETIME(), SYSDATETIME())";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Relationship", ddlRelationship.SelectedValue);
                    cmd.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim());
                    string altPhone = txtAlternatePhone.Text.Trim();
                    cmd.Parameters.AddWithValue("@AlternatePhone", string.IsNullOrEmpty(altPhone) ? (object)DBNull.Value : altPhone);
                    string email = txtEmail.Text.Trim();
                    cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                    string nationalId = txtNationalId.Text.Trim();
                    cmd.Parameters.AddWithValue("@NationalID", string.IsNullOrEmpty(nationalId) ? (object)DBNull.Value : nationalId);
                    string occupation = txtOccupation.Text.Trim();
                    cmd.Parameters.AddWithValue("@Occupation", string.IsNullOrEmpty(occupation) ? (object)DBNull.Value : occupation);
                    string emergency = txtEmergencyContact.Text.Trim();
                    cmd.Parameters.AddWithValue("@EmergencyContact", string.IsNullOrEmpty(emergency) ? (object)DBNull.Value : emergency);
                    string address = txtAddress.Text.Trim();
                    cmd.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(address) ? (object)DBNull.Value : address);
                    cmd.Parameters.AddWithValue("@IsActive", ddlStatus.SelectedValue == "1");

                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception)
            {
                ShowError("The guardian could not be saved due to a system error. Please try again.");
                return null;
            }
        }

        protected void btnReset_Click(object sender, EventArgs e) { ClearForm(); }
        protected void btnCancel_Click(object sender, EventArgs e) { Response.Redirect("~/Modules/Parents/Parents.aspx", true); }

        private void ClearForm()
        {
            txtFullName.Text = "";
            ddlRelationship.SelectedIndex = 0;
            txtPhone.Text = "";
            txtAlternatePhone.Text = "";
            txtEmail.Text = "";
            txtNationalId.Text = "";
            txtOccupation.Text = "";
            txtEmergencyContact.Text = "";
            txtAddress.Text = "";
            ddlStatus.SelectedValue = "1";
            pnlError.Visible = false;
            pnlDuplicateWarning.Visible = false;
        }

        private void ShowSuccess(string message)
        {
            lblSuccess.Text = message;
            pnlSuccess.Visible = true;
            pnlError.Visible = false;
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            pnlError.Visible = true;
            pnlSuccess.Visible = false;
        }
    }
}
