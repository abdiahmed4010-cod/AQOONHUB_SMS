using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AQOONHUB_SMS.Modules.Parents
{
    public partial class ParentDetails : System.Web.UI.Page
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
                    pnlBody.Visible = false;
                    pnlNotFound.Visible = true;
                    return;
                }
                LoadLinkedStudents();
                LoadLinkedAdmissions();
            }

            bool canManage = CanManageParents();
            lnkEdit.Visible = canManage;
            lnkAssignStudent.Visible = canManage;
            lnkManageLogin.Visible = canManage;
            btnToggleActive.Visible = canManage;
        }

        private bool LoadGuardian()
        {
            if (GuardianId <= 0) return false;

            string query = "SELECT * FROM Guardians WHERE GuardianID = @Id";
            DataTable dt = ExecuteQuery(query, new[] { new SqlParameter("@Id", GuardianId) });
            if (dt.Rows.Count == 0) return false;

            DataRow row = dt.Rows[0];
            string fullName = row["FullName"].ToString();
            bool isActive = Convert.ToBoolean(row["IsActive"]);

            lblFullName.Text = fullName;
            lblInitials.Text = GetInitials(fullName);
            lblRelationship.Text = row["Relationship"].ToString();
            lblPhone.Text = row["Phone"].ToString();

            lblStatusBadge.Text = isActive ? "Active" : "Inactive";
            lblStatusBadge.Style["background"] = isActive ? "#DCFCE7" : "#F1F5F9";
            lblStatusBadge.Style["color"] = isActive ? "#15803D" : "#64748B";
            lblToggleActiveText.Text = isActive ? "Deactivate" : "Activate";

            lblDetailPhone.Text = row["Phone"].ToString();
            lblDetailAltPhone.Text = ColOrDash(dt, "AlternatePhone", row);
            lblDetailEmail.Text = ColOrDash(dt, "Email", row);
            lblDetailAddress.Text = ColOrDash(dt, "Address", row);
            lblDetailEmergency.Text = ColOrDash(dt, "EmergencyContact", row);
            lblDetailOccupation.Text = ColOrDash(dt, "Occupation", row);
            lblDetailNationalId.Text = ColOrDash(dt, "NationalID", row);

            bool hasUserId = dt.Columns.Contains("UserID") && row["UserID"] != DBNull.Value;
            lblDetailLoginAccount.Text = hasUserId ? "Linked (User #" + row["UserID"] + ")" : "No login account linked";

            lblDetailCreated.Text = dt.Columns.Contains("CreatedAt") && row["CreatedAt"] != DBNull.Value
                ? Convert.ToDateTime(row["CreatedAt"]).ToString("MMM dd, yyyy") : "—";
            lblDetailUpdated.Text = dt.Columns.Contains("UpdatedAt") && row["UpdatedAt"] != DBNull.Value
                ? Convert.ToDateTime(row["UpdatedAt"]).ToString("MMM dd, yyyy") : "—";

            lnkEdit.NavigateUrl = ResolveUrl("~/Modules/Parents/EditParent.aspx?id=" + GuardianId);
            lnkAssignStudent.NavigateUrl = ResolveUrl("~/Modules/Parents/AssignStudent.aspx?guardianId=" + GuardianId);
            lnkManageLogin.NavigateUrl = ResolveUrl("~/Modules/Parents/ParentAccount.aspx?id=" + GuardianId);

            return true;
        }

        private string ColOrDash(DataTable dt, string colName, DataRow row)
        {
            if (!dt.Columns.Contains(colName)) return "—";
            return row[colName] == DBNull.Value ? "—" : row[colName].ToString();
        }

        private void LoadLinkedStudents()
        {
            string query = @"
                SELECT s.StudentID, s.StudentCode, s.AdmissionNo,
                       LTRIM(RTRIM(ISNULL(s.FirstName,'') + ' ' + ISNULL(s.LastName,''))) AS FullName,
                       s.Status, sec.SectionName, c.ClassName
                FROM Students s
                INNER JOIN Sections sec ON s.SectionID = sec.SectionID
                INNER JOIN Classes c ON sec.ClassID = c.ClassID
                WHERE s.GuardianID = @GuardianID AND s.Status <> 'Deleted'
                ORDER BY s.FirstName";
            DataTable dt = ExecuteQuery(query, new[] { new SqlParameter("@GuardianID", GuardianId) });
            gvStudents.DataSource = dt;
            gvStudents.DataBind();
        }

        private void LoadLinkedAdmissions()
        {
            string query = @"
                SELECT a.AdmissionID, a.ApplicationNo,
                       LTRIM(RTRIM(ISNULL(a.FirstName,'') + ' ' + ISNULL(a.LastName,''))) AS FullName,
                       a.Status, c.ClassName
                FROM Admissions a
                INNER JOIN Classes c ON a.ApplyingForClassID = c.ClassID
                WHERE a.GuardianID = @GuardianID
                ORDER BY a.ApplicationDate DESC";

            DataTable dt;
            try { dt = ExecuteQuery(query, new[] { new SqlParameter("@GuardianID", GuardianId) }); }
            catch (SqlException) { dt = new DataTable(); } // Admissions.GuardianID may not exist yet if migration hasn't run

            gvAdmissions.DataSource = dt;
            gvAdmissions.DataBind();
        }

        protected void btnToggleActive_Click(object sender, EventArgs e)
        {
            if (!CanManageParents()) { ShowError("You do not have permission to change this guardian's status."); return; }

            object activeStudentCount = ExecuteScalarHelper(
                "SELECT COUNT(1) FROM Students WHERE GuardianID = @Id AND Status = 'Active'",
                new[] { new SqlParameter("@Id", GuardianId) });

            bool isCurrentlyActive = lblToggleActiveText.Text == "Deactivate";
            if (isCurrentlyActive && Convert.ToInt32(activeStudentCount) > 0)
            {
                ShowError("This guardian is linked to " + activeStudentCount + " active student(s). Deactivating is allowed, but the guardian will remain linked — reassign students first if that's not intended.");
            }

            ExecuteNonQuery(
                "UPDATE Guardians SET IsActive = @IsActive, UpdatedAt = SYSDATETIME() WHERE GuardianID = @Id",
                new[] { new SqlParameter("@IsActive", !isCurrentlyActive), new SqlParameter("@Id", GuardianId) });

            LoadGuardian();
            LoadLinkedStudents();
            LoadLinkedAdmissions();
        }

        private object ExecuteScalarHelper(string query, SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

        #region Template Helpers

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "GD";
            string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        protected string GetStudentStatusStyle(object statusValue)
        {
            string status = statusValue == null || statusValue == DBNull.Value ? "" : statusValue.ToString();
            switch (status)
            {
                case "Active": return "background:#DCFCE7;color:#15803D";
                case "Inactive": return "background:#F1F5F9;color:#64748B";
                case "Graduated": return "background:#EDE9FE;color:#6D28D9";
                case "Transferred": return "background:#E0F2FE;color:#0369A1";
                default: return "background:#F1F5F9;color:#64748B";
            }
        }

        protected string GetAdmissionStatusStyle(object statusValue)
        {
            string status = statusValue == null || statusValue == DBNull.Value ? "" : statusValue.ToString();
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
