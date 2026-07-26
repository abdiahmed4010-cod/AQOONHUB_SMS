using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AQOONHUB_SMS.Modules.Staff
{
    public partial class StaffDetails : System.Web.UI.Page
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
                    pnlBody.Visible = false;
                    pnlNotFound.Visible = true;
                    return;
                }
            }

            bool canManage = CanManageStaff();
            lnkEdit.Visible = canManage;
            btnToggleLeave.Visible = canManage;
            btnDeactivate.Visible = canManage;
        }

        private bool LoadStaff()
        {
            if (StaffId <= 0) return false;

            string query = @"
                SELECT s.StaffID, s.EmployeeID, s.Department, s.Position, s.HireDate, s.Salary, s.LeaveBalance, s.Status,
                       u.FullName, u.Email, u.Phone, u.Role
                FROM Staff s
                INNER JOIN Users u ON s.UserID = u.UserID
                WHERE s.StaffID = @Id";

            DataTable dt = ExecuteQuery(query, new[] { new SqlParameter("@Id", StaffId) });
            if (dt.Rows.Count == 0) return false;

            DataRow row = dt.Rows[0];
            string fullName = row["FullName"].ToString();
            string status = row["Status"].ToString();

            lblFullName.Text = fullName;
            lblInitials.Text = GetInitials(fullName);
            lblEmployeeId.Text = row["EmployeeID"].ToString();
            lblPosition.Text = row["Position"].ToString();
            lblDepartment.Text = row["Department"].ToString();

            lblStatusBadge.Text = status;
            ApplyStatusStyle(status);

            lblDetailName.Text = fullName;
            lblDetailEmail.Text = row["Email"].ToString();
            lblDetailPhone.Text = row["Phone"].ToString();
            lblDetailRole.Text = row["Role"].ToString();

            lblDetailEmployeeId.Text = row["EmployeeID"].ToString();
            lblDetailDepartment.Text = row["Department"].ToString();
            lblDetailPosition.Text = row["Position"].ToString();
            lblDetailHireDate.Text = Convert.ToDateTime(row["HireDate"]).ToString("MMM dd, yyyy");
            lblDetailSalary.Text = Convert.ToDecimal(row["Salary"]).ToString("N2");
            lblDetailLeaveBalance.Text = row["LeaveBalance"] + " days";

            lblToggleLeaveText.Text = status == "On Leave" ? "Return from Leave" : "Mark On Leave";
            btnDeactivate.Visible = status != "Inactive";
            lnkEdit.NavigateUrl = ResolveUrl("~/Modules/Staff/EditStaff.aspx?id=" + StaffId);

            return true;
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "ST";
            string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        private void ApplyStatusStyle(string status)
        {
            string bg, color;
            switch (status)
            {
                case "Active": bg = "#DCFCE7"; color = "#15803D"; break;
                case "On Leave": bg = "#FFFBEB"; color = "#B45309"; break;
                case "Inactive": bg = "#F1F5F9"; color = "#64748B"; break;
                case "Retired": bg = "#EDE9FE"; color = "#6D28D9"; break;
                default: bg = "#F1F5F9"; color = "#64748B"; break;
            }
            lblStatusBadge.Style["background"] = bg;
            lblStatusBadge.Style["color"] = color;
        }

        private static readonly string[] AllowedStatuses = { "Active", "On Leave", "Inactive", "Retired" };

        private void UpdateStatus(string newStatus)
        {
            if (!CanManageStaff()) { ShowError("You do not have permission to change this staff member's status."); return; }

            bool ok = false;
            foreach (string s in AllowedStatuses) if (s == newStatus) { ok = true; break; }
            if (!ok) return;

            ExecuteNonQuery("UPDATE Staff SET Status = @Status WHERE StaffID = @Id",
                new[] { new SqlParameter("@Status", newStatus), new SqlParameter("@Id", StaffId) });

            ShowSuccess("Status updated to " + newStatus + ".");
            LoadStaff();
        }

        protected void btnToggleLeave_Click(object sender, EventArgs e)
        {
            UpdateStatus(lblToggleLeaveText.Text == "Return from Leave" ? "Active" : "On Leave");
        }

        protected void btnDeactivate_Click(object sender, EventArgs e)
        {
            UpdateStatus("Inactive");
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
