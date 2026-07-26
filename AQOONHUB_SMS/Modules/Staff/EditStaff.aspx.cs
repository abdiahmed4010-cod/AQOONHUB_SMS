using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Staff
{
    public partial class EditStaff : System.Web.UI.Page
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

        private int StaffId
        {
            get { return ViewState["StaffId"] == null ? 0 : (int)ViewState["StaffId"]; }
            set { ViewState["StaffId"] = value; }
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
                ShowError("You do not have permission to edit staff. This page is available to Super Admin, Admin, and HR roles only.");
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
                StaffId = id;

                if (!LoadStaff())
                {
                    pnlFormBody.Visible = false;
                    pnlNotFound.Visible = true;
                }
            }
        }

        private bool LoadStaff()
        {
            if (StaffId <= 0) return false;

            string query = @"
                SELECT s.*, u.FullName, u.Email
                FROM Staff s
                INNER JOIN Users u ON s.UserID = u.UserID
                WHERE s.StaffID = @Id";

            DataTable dt = ExecuteQuery(query, new[] { new SqlParameter("@Id", StaffId) });
            if (dt.Rows.Count == 0) return false;

            DataRow row = dt.Rows[0];
            lblEmployeeId.Text = row["EmployeeID"].ToString();
            lblUserAccount.Text = row["FullName"] + " (" + row["Email"] + ")";
            txtDepartment.Text = row["Department"].ToString();
            txtPosition.Text = row["Position"].ToString();
            txtHireDate.Text = Convert.ToDateTime(row["HireDate"]).ToString("yyyy-MM-dd");
            txtSalary.Text = Convert.ToDecimal(row["Salary"]).ToString("0.00");
            txtLeaveBalance.Text = row["LeaveBalance"].ToString();
            ddlStatus.SelectedValue = row["Status"].ToString();

            return true;
        }

        protected void cvHireDate_ServerValidate(object source, ServerValidateEventArgs args)
        {
            DateTime hireDate;
            if (!DateTime.TryParse(txtHireDate.Text, out hireDate)) { args.IsValid = false; return; }
            args.IsValid = hireDate.Date <= DateTime.Now.Date;
        }

        private bool ValidateStaff(out string errorMessage)
        {
            errorMessage = null;
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

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!CanManageStaff()) { ShowError("You do not have permission to edit staff."); return; }
            if (!Page.IsValid) return;

            string validationError;
            if (!ValidateStaff(out validationError)) { ShowError(validationError); return; }

            string query = @"
                UPDATE Staff SET
                    Department = @Department, Position = @Position, HireDate = @HireDate,
                    Salary = @Salary, LeaveBalance = @LeaveBalance, Status = @Status
                WHERE StaffID = @Id";

            try
            {
                ExecuteNonQuery(query, new[]
                {
                    new SqlParameter("@Department", txtDepartment.Text.Trim()),
                    new SqlParameter("@Position", txtPosition.Text.Trim()),
                    new SqlParameter("@HireDate", DateTime.Parse(txtHireDate.Text)),
                    new SqlParameter("@Salary", decimal.Parse(txtSalary.Text)),
                    new SqlParameter("@LeaveBalance", int.Parse(txtLeaveBalance.Text)),
                    new SqlParameter("@Status", ddlStatus.SelectedValue),
                    new SqlParameter("@Id", StaffId)
                });
                Response.Redirect("~/Modules/Staff/StaffDetails.aspx?id=" + StaffId, true);
            }
            catch (Exception)
            {
                ShowError("The staff member could not be updated due to a system error. Please try again.");
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/Staff/StaffDetails.aspx?id=" + StaffId, true);
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
