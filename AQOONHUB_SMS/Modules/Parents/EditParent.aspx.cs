using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AQOONHUB_SMS.Modules.Parents
{
    public partial class EditParent : System.Web.UI.Page
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

        private int GuardianId
        {
            get { return ViewState["GuardianId"] == null ? 0 : (int)ViewState["GuardianId"]; }
            set { ViewState["GuardianId"] = value; }
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
                ShowError("You do not have permission to edit parents. This page is available to Super Admin, Admin, and Registrar roles only.");
                pnlFormBody.Visible = false;
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
                GuardianId = id;

                if (!LoadGuardian())
                {
                    pnlFormBody.Visible = false;
                    pnlNotFound.Visible = true;
                }
            }
        }

        private bool LoadGuardian()
        {
            if (GuardianId <= 0) return false;

            DataTable dt = ExecuteQuery("SELECT * FROM Guardians WHERE GuardianID = @Id", new[] { new SqlParameter("@Id", GuardianId) });
            if (dt.Rows.Count == 0) return false;

            DataRow row = dt.Rows[0];
            txtFullName.Text = row["FullName"].ToString();
            ddlRelationship.SelectedValue = row["Relationship"].ToString();
            txtPhone.Text = row["Phone"].ToString();
            txtAlternatePhone.Text = ColOrEmpty(dt, "AlternatePhone", row);
            txtEmail.Text = ColOrEmpty(dt, "Email", row);
            txtNationalId.Text = ColOrEmpty(dt, "NationalID", row);
            txtOccupation.Text = ColOrEmpty(dt, "Occupation", row);
            txtEmergencyContact.Text = ColOrEmpty(dt, "EmergencyContact", row);
            txtAddress.Text = ColOrEmpty(dt, "Address", row);
            ddlStatus.SelectedValue = Convert.ToBoolean(row["IsActive"]) ? "1" : "0";

            return true;
        }

        private string ColOrEmpty(DataTable dt, string colName, DataRow row)
        {
            if (!dt.Columns.Contains(colName)) return "";
            return row[colName] == DBNull.Value ? "" : row[colName].ToString();
        }

        private bool ValidateParent(out string errorMessage)
        {
            errorMessage = null;
            if (string.IsNullOrWhiteSpace(txtFullName.Text)) { errorMessage = "Full name is required."; return false; }
            if (string.IsNullOrEmpty(ddlRelationship.SelectedValue)) { errorMessage = "Please select a relationship."; return false; }
            if (string.IsNullOrWhiteSpace(txtPhone.Text)) { errorMessage = "Phone is required."; return false; }
            if (txtAddress.Text.Length > 200) { errorMessage = "Address must be 200 characters or fewer."; return false; }
            return true;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!CanManageParents()) { ShowError("You do not have permission to edit parents."); return; }
            if (!Page.IsValid) return;

            string validationError;
            if (!ValidateParent(out validationError)) { ShowError(validationError); return; }

            // Duplicate check excludes this guardian's own current row.
            DataTable dupCheck = ExecuteQuery(
                "SELECT FullName FROM Guardians WHERE (Phone = @Phone OR (@Email <> '' AND Email = @Email)) AND GuardianID <> @Id",
                new[] { new SqlParameter("@Phone", txtPhone.Text.Trim()), new SqlParameter("@Email", txtEmail.Text.Trim()), new SqlParameter("@Id", GuardianId) });
            if (dupCheck.Rows.Count > 0)
            {
                ShowError("Another guardian already uses this phone or email: " + dupCheck.Rows[0]["FullName"] + ".");
                return;
            }

            string query = @"
                UPDATE Guardians SET
                    FullName = @FullName, Relationship = @Relationship, Phone = @Phone,
                    AlternatePhone = @AlternatePhone, Email = @Email, NationalID = @NationalID,
                    Occupation = @Occupation, EmergencyContact = @EmergencyContact, Address = @Address,
                    IsActive = @IsActive, UpdatedAt = SYSDATETIME()
                WHERE GuardianID = @Id";

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
                    cmd.Parameters.AddWithValue("@Id", GuardianId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                Response.Redirect("~/Modules/Parents/ParentDetails.aspx?id=" + GuardianId, true);
            }
            catch (Exception)
            {
                ShowError("The guardian could not be updated due to a system error. Please try again.");
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/Parents/ParentDetails.aspx?id=" + GuardianId, true);
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
