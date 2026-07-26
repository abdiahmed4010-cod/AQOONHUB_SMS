using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Staff
{
    public partial class AddStaff : System.Web.UI.Page
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

        private object ExecuteScalar(SqlConnection conn, SqlTransaction tx, string query, SqlParameter[] parameters = null)
        {
            using (SqlCommand cmd = new SqlCommand(query, conn, tx))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteScalar();
            }
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
            if (!CanManageStaff())
            {
                ShowError("You do not have permission to add staff. This page is available to Super Admin, Admin, and HR roles only.");
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
                LoadEligibleUsers();
                GenerateAndDisplayEmployeeId();
                txtHireDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            }
        }

        /// <summary>
        /// Only Users not already linked to a Staff record are offered — a UserID can
        /// back at most one Staff row. Any role can be staffed (Teacher/Accountant/HR/
        /// Admin) except Parent, since parents are managed via the Parents module.
        /// </summary>
        private void LoadEligibleUsers()
        {
            string query = @"
                SELECT u.UserID, u.FullName, u.Email, u.Role
                FROM Users u
                WHERE u.Role <> 'Parent'
                  AND NOT EXISTS (SELECT 1 FROM Staff s WHERE s.UserID = u.UserID)
                ORDER BY u.FullName";

            DataTable dt = ExecuteQuery(query);
            ddlUser.Items.Clear();
            ddlUser.Items.Add(new ListItem("Select User Account", "0"));
            foreach (DataRow row in dt.Rows)
            {
                string label = string.Format("{0} — {1} ({2})", row["FullName"], row["Email"], row["Role"]);
                ddlUser.Items.Add(new ListItem(label, row["UserID"].ToString()));
            }
        }

        #region Employee ID Generation (EMP-{0000}, same numeric-safe pattern as AdmissionNo)

        private string GenerateEmployeeId()
        {
            string query = @"
                SELECT ISNULL(
                    MAX(TRY_CONVERT(INT, SUBSTRING(EmployeeID, 5, LEN(EmployeeID)))),
                    0
                ) + 1 AS NextNumber
                FROM Staff
                WHERE EmployeeID LIKE 'EMP-%'";

            object result = ExecuteQuery(query).Rows[0]["NextNumber"];
            int nextNumber = (result == null || result == DBNull.Value) ? 1 : Convert.ToInt32(result);
            return "EMP-" + nextNumber.ToString("D4");
        }

        private string GenerateUniqueEmployeeId(SqlConnection conn, SqlTransaction tx)
        {
            for (int attempt = 0; attempt < 5; attempt++)
            {
                object result = ExecuteScalar(conn, tx, @"
                    SELECT ISNULL(MAX(TRY_CONVERT(INT, SUBSTRING(EmployeeID, 5, LEN(EmployeeID)))), 0) + 1
                    FROM Staff WITH (UPDLOCK, HOLDLOCK) WHERE EmployeeID LIKE 'EMP-%'");

                int nextNumber = (result == null || result == DBNull.Value) ? 1 : Convert.ToInt32(result);
                string candidate = "EMP-" + nextNumber.ToString("D4");

                object exists = ExecuteScalar(conn, tx,
                    "SELECT COUNT(1) FROM Staff WITH (UPDLOCK, HOLDLOCK) WHERE EmployeeID = @Code",
                    new[] { new SqlParameter("@Code", candidate) });
                if (Convert.ToInt32(exists) == 0) return candidate;
            }
            throw new InvalidOperationException("Could not generate a unique Employee ID after several attempts.");
        }

        private void GenerateAndDisplayEmployeeId()
        {
            string code = GenerateEmployeeId();
            hdnEmployeeId.Value = code;
            lblEmployeeId.Text = code;
        }

        #endregion

        #region Validation

        protected void cvHireDate_ServerValidate(object source, ServerValidateEventArgs args)
        {
            DateTime hireDate;
            if (!DateTime.TryParse(txtHireDate.Text, out hireDate)) { args.IsValid = false; return; }
            args.IsValid = hireDate.Date <= DateTime.Now.Date;
        }

        private bool ValidateStaff(out string errorMessage)
        {
            errorMessage = null;

            int userId;
            if (!int.TryParse(ddlUser.SelectedValue, out userId) || userId <= 0)
            { errorMessage = "Please select a user account."; return false; }

            if (string.IsNullOrWhiteSpace(txtDepartment.Text)) { errorMessage = "Department is required."; return false; }
            if (string.IsNullOrWhiteSpace(txtPosition.Text)) { errorMessage = "Position is required."; return false; }

            DateTime hireDate;
            if (!DateTime.TryParse(txtHireDate.Text, out hireDate) || hireDate.Date > DateTime.Now.Date)
            { errorMessage = "Please provide a valid hire date."; return false; }

            decimal salary;
            if (!decimal.TryParse(txtSalary.Text, out salary) || salary <= 0)
            { errorMessage = "Please provide a valid salary."; return false; }

            int leaveBalance;
            if (!int.TryParse(txtLeaveBalance.Text, out leaveBalance) || leaveBalance < 0)
            { errorMessage = "Please provide a valid leave balance."; return false; }

            return true;
        }

        #endregion

        #region Save

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!CanManageStaff()) { ShowError("You do not have permission to add staff."); return; }
            if (!Page.IsValid) return;

            string validationError;
            if (!ValidateStaff(out validationError)) { ShowError(validationError); return; }

            int userId = int.Parse(ddlUser.SelectedValue);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        object alreadyStaffed = ExecuteScalar(conn, tx,
                            "SELECT COUNT(1) FROM Staff WITH (UPDLOCK, HOLDLOCK) WHERE UserID = @UserID",
                            new[] { new SqlParameter("@UserID", userId) });
                        if (Convert.ToInt32(alreadyStaffed) > 0)
                        {
                            tx.Rollback();
                            ShowError("This user account is already linked to a staff record.");
                            return;
                        }

                        string employeeId = GenerateUniqueEmployeeId(conn, tx);

                        string insertQuery = @"
                            INSERT INTO Staff (UserID, EmployeeID, Department, Position, HireDate, Salary, LeaveBalance, Status)
                            VALUES (@UserID, @EmployeeID, @Department, @Position, @HireDate, @Salary, @LeaveBalance, @Status)";

                        using (SqlCommand cmd = new SqlCommand(insertQuery, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@UserID", userId);
                            cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                            cmd.Parameters.AddWithValue("@Department", txtDepartment.Text.Trim());
                            cmd.Parameters.AddWithValue("@Position", txtPosition.Text.Trim());
                            cmd.Parameters.AddWithValue("@HireDate", DateTime.Parse(txtHireDate.Text));
                            cmd.Parameters.AddWithValue("@Salary", decimal.Parse(txtSalary.Text));
                            cmd.Parameters.AddWithValue("@LeaveBalance", int.Parse(txtLeaveBalance.Text));
                            cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        Response.Redirect("~/Modules/Staff/Staff.aspx", true);
                    }
                    catch (Exception)
                    {
                        try { tx.Rollback(); } catch { }
                        ShowError("The staff member could not be saved due to a system error. Please try again.");
                    }
                }
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ddlUser.SelectedIndex = 0;
            txtDepartment.Text = "";
            txtPosition.Text = "";
            txtHireDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtSalary.Text = "";
            txtLeaveBalance.Text = "18";
            ddlStatus.SelectedValue = "Active";
            GenerateAndDisplayEmployeeId();
            LoadEligibleUsers();
            pnlError.Visible = false;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/Staff/Staff.aspx", true);
        }

        #endregion

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
