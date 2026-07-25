using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AQOONHUB_SMS.Modules.Admission
{
    public partial class Admissions : System.Web.UI.Page
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

        private static readonly string[] AllowedNormalizedRoles = { "superadmin", "admin", "registrar" };

        private bool CanManageAdmissions()
        {
            string normalized = NormalizeRole(Session["Role"] as string);
            foreach (string allowed in AllowedNormalizedRoles)
                if (normalized == allowed) return true;
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
            return true; // read-only list access for any logged-in role
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!CheckAuthorization()) return;

            lnkAddAdmission.Visible = CanManageAdmissions();

            if (!IsPostBack)
            {
                LoadSummaryCards();
                LoadAdmissions();
            }
        }

        private void LoadSummaryCards()
        {
            string query = @"
                SELECT
                    SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END) AS PendingCount,
                    SUM(CASE WHEN Status = 'Under Review' THEN 1 ELSE 0 END) AS UnderReviewCount,
                    SUM(CASE WHEN Status IN ('Approved', 'Enrolled') THEN 1 ELSE 0 END) AS ApprovedCount,
                    SUM(CASE WHEN Status = 'Rejected' THEN 1 ELSE 0 END) AS RejectedCount
                FROM Admissions";

            DataTable dt = ExecuteQuery(query);
            if (dt.Rows.Count == 0) return;
            DataRow row = dt.Rows[0];
            lblPendingCount.Text = SafeInt(row["PendingCount"]).ToString();
            lblUnderReviewCount.Text = SafeInt(row["UnderReviewCount"]).ToString();
            lblApprovedCount.Text = SafeInt(row["ApprovedCount"]).ToString();
            lblRejectedCount.Text = SafeInt(row["RejectedCount"]).ToString();
        }

        private int SafeInt(object val) { return val == DBNull.Value ? 0 : Convert.ToInt32(val); }

        private void LoadAdmissions()
        {
            string where = " WHERE 1=1";
            List<SqlParameter> parameters = new List<SqlParameter>();

            string search = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(search))
            {
                where += @" AND (a.FirstName LIKE @Search OR a.LastName LIKE @Search OR a.ApplicationNo LIKE @Search)";
                parameters.Add(new SqlParameter("@Search", "%" + search + "%"));
            }
            if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
            {
                where += " AND a.Status = @Status";
                parameters.Add(new SqlParameter("@Status", ddlStatus.SelectedValue));
            }

            string query = @"
                SELECT
                    a.AdmissionID, a.ApplicationNo,
                    LTRIM(RTRIM(ISNULL(a.FirstName,'') + ' ' + ISNULL(a.LastName,''))) AS FullName,
                    a.Gender, a.DateOfBirth, a.Status, a.ApplicationDate,
                    a.GuardianName, a.GuardianPhone,
                    c.ClassName
                FROM Admissions a
                INNER JOIN Classes c ON a.ApplyingForClassID = c.ClassID"
                + where + @"
                ORDER BY a.ApplicationDate DESC";

            DataTable dt = ExecuteQuery(query, parameters.ToArray());
            gvAdmissions.DataSource = dt;
            gvAdmissions.DataBind();
        }

        protected void btnSearch_Click(object sender, EventArgs e) { LoadAdmissions(); }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlStatus.SelectedIndex = 0;
            LoadAdmissions();
        }

        #region Template Helpers

        protected string GetInitials(object fullNameValue)
        {
            string fullName = (fullNameValue == null || fullNameValue == DBNull.Value) ? null : fullNameValue.ToString();
            if (string.IsNullOrWhiteSpace(fullName)) return "AP";
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

        protected string GetStatusStyle(object statusValue)
        {
            string status = (statusValue == null || statusValue == DBNull.Value) ? "" : statusValue.ToString();
            switch (status)
            {
                case "Pending": return "background:#FFFBEB;color:#B45309";
                case "Under Review": return "background:#EFF6FF;color:#1D4ED8";
                case "Approved": return "background:#DCFCE7;color:#15803D";
                case "Enrolled": return "background:#DCFCE7;color:#15803D";
                case "Rejected": return "background:#FEE2E2;color:#B91C1C";
                default: return "background:#F1F5F9;color:#64748B";
            }
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
