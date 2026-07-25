using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AQOONHUB_SMS.Modules.Parents
{
    public partial class ParentAccount : System.Web.UI.Page
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
                ShowError("You do not have permission to manage login accounts.");
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
                int guardianId;
                int.TryParse(Request.QueryString["id"], out guardianId);

                if (!LoadGuardian(guardianId))
                {
                    pnlBody.Visible = false;
                    pnlNotFound.Visible = true;
                }
            }
        }

        private bool LoadGuardian(int guardianId)
        {
            if (guardianId <= 0) return false;

            DataTable dt = ExecuteQuery("SELECT * FROM Guardians WHERE GuardianID = @Id", new[] { new SqlParameter("@Id", guardianId) });
            if (dt.Rows.Count == 0) return false;

            DataRow row = dt.Rows[0];
            lblGuardianName.Text = row["FullName"].ToString();

            bool hasUserId = dt.Columns.Contains("UserID") && row["UserID"] != DBNull.Value;
            if (!hasUserId)
            {
                lblLinkedStatus.Text = "No login account linked";
                pnlLinkedDetails.Visible = false;
                return true;
            }

            lblLinkedStatus.Text = "Linked";
            int userId = Convert.ToInt32(row["UserID"]);

            // Read the Users row generically (SELECT *) rather than naming specific columns,
            // since the real Users schema was not confirmed for this task — this avoids
            // guessing at column names like Login.aspx.cs's session/auth pattern uses.
            try
            {
                DataTable userDt = ExecuteQuery("SELECT * FROM Users WHERE UserID = @Id", new[] { new SqlParameter("@Id", userId) });
                if (userDt.Rows.Count > 0)
                {
                    DataRow userRow = userDt.Rows[0];
                    string username = FirstNonEmptyColumn(userDt, userRow, "Username", "Email", "UserEmail");
                    lblLinkedUsername.Text = string.IsNullOrEmpty(username) ? ("User #" + userId) : username;

                    string statusCol = FirstNonEmptyColumn(userDt, userRow, "Status", "IsActive", "Active");
                    lblLinkedAccountStatus.Text = string.IsNullOrEmpty(statusCol) ? "Unknown" : statusCol;

                    pnlLinkedDetails.Visible = true;
                }
                else
                {
                    lblLinkedStatus.Text = "Linked (User #" + userId + ", record not found)";
                    pnlLinkedDetails.Visible = false;
                }
            }
            catch (SqlException)
            {
                lblLinkedStatus.Text = "Linked (User #" + userId + ")";
                pnlLinkedDetails.Visible = false;
            }

            return true;
        }

        private string FirstNonEmptyColumn(DataTable dt, DataRow row, params string[] candidateNames)
        {
            foreach (string name in candidateNames)
            {
                if (dt.Columns.Contains(name) && row[name] != DBNull.Value)
                    return row[name].ToString();
            }
            return null;
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
