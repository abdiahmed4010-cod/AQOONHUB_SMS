using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Parents
{
    public partial class AssignStudent : System.Web.UI.Page
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

        private int ExecuteNonQuery(SqlConnection conn, SqlTransaction tx, string query, SqlParameter[] parameters = null)
        {
            using (SqlCommand cmd = new SqlCommand(query, conn, tx))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        private int SelectedStudentId
        {
            get { return ViewState["SelStudentId"] == null ? 0 : (int)ViewState["SelStudentId"]; }
            set { ViewState["SelStudentId"] = value; }
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
                ShowError("You do not have permission to assign students. This page is available to Super Admin, Admin, and Registrar roles only.");
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
                LoadGuardians();

                int guardianId;
                if (int.TryParse(Request.QueryString["guardianId"], out guardianId) && guardianId > 0)
                {
                    ListItem item = ddlGuardian.Items.FindByValue(guardianId.ToString());
                    if (item != null) { ddlGuardian.ClearSelection(); item.Selected = true; }
                }
            }
        }

        private void LoadGuardians()
        {
            DataTable dt = ExecuteQuery("SELECT GuardianID, FullName, Phone FROM Guardians WHERE IsActive = 1 ORDER BY FullName");
            ddlGuardian.Items.Clear();
            ddlGuardian.Items.Add(new ListItem("Select Guardian", "0"));
            foreach (DataRow row in dt.Rows)
                ddlGuardian.Items.Add(new ListItem(row["FullName"] + " — " + row["Phone"], row["GuardianID"].ToString()));
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string search = txtSearch.Text.Trim();
            string query = @"
                SELECT s.StudentID, s.StudentCode, s.AdmissionNo,
                       LTRIM(RTRIM(ISNULL(s.FirstName,'') + ' ' + ISNULL(s.LastName,''))) AS FullName,
                       c.ClassName, g.FullName AS CurrentGuardianName
                FROM Students s
                INNER JOIN Sections sec ON s.SectionID = sec.SectionID
                INNER JOIN Classes c ON sec.ClassID = c.ClassID
                INNER JOIN Guardians g ON s.GuardianID = g.GuardianID
                WHERE s.Status <> 'Deleted'
                  AND (s.StudentCode LIKE @Search OR s.AdmissionNo LIKE @Search
                       OR s.FirstName LIKE @Search OR s.LastName LIKE @Search)
                ORDER BY s.FirstName";

            DataTable dt = ExecuteQuery(query, new[] { new SqlParameter("@Search", "%" + search + "%") });
            gvResults.DataSource = dt;
            gvResults.DataBind();
        }

        protected void gvResults_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Select") return;

            int studentId = Convert.ToInt32(e.CommandArgument);
            SelectedStudentId = studentId;

            string query = @"
                SELECT s.StudentID,
                       LTRIM(RTRIM(ISNULL(s.FirstName,'') + ' ' + ISNULL(s.LastName,''))) AS FullName,
                       s.StudentCode, g.FullName AS CurrentGuardianName
                FROM Students s
                INNER JOIN Guardians g ON s.GuardianID = g.GuardianID
                WHERE s.StudentID = @Id";
            DataTable dt = ExecuteQuery(query, new[] { new SqlParameter("@Id", studentId) });
            if (dt.Rows.Count == 0) return;

            DataRow row = dt.Rows[0];
            lblConfirmStudent.Text = row["FullName"] + " (" + row["StudentCode"] + ")";
            lblConfirmCurrentGuardian.Text = row["CurrentGuardianName"].ToString();
            lblConfirmNewGuardian.Text = ddlGuardian.SelectedItem != null ? ddlGuardian.SelectedItem.Text : "—";

            pnlConfirm.Visible = true;
        }

        protected void btnCancelConfirm_Click(object sender, EventArgs e)
        {
            pnlConfirm.Visible = false;
            SelectedStudentId = 0;
        }

        protected void btnConfirmAssign_Click(object sender, EventArgs e)
        {
            if (!CanManageParents()) { ShowError("You do not have permission to assign students."); return; }

            int guardianId;
            if (!int.TryParse(ddlGuardian.SelectedValue, out guardianId) || guardianId <= 0)
            { ShowError("Please select a guardian."); return; }
            if (SelectedStudentId <= 0)
            { ShowError("Please select a student first."); return; }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        object studentExists = ExecuteScalar(conn, tx,
                            "SELECT COUNT(1) FROM Students WITH (UPDLOCK, HOLDLOCK) WHERE StudentID = @Id AND Status <> 'Deleted'",
                            new[] { new SqlParameter("@Id", SelectedStudentId) });
                        if (Convert.ToInt32(studentExists) == 0)
                        { tx.Rollback(); ShowError("Student not found."); return; }

                        object guardianRow = ExecuteScalar(conn, tx,
                            "SELECT IsActive FROM Guardians WITH (UPDLOCK, HOLDLOCK) WHERE GuardianID = @Id",
                            new[] { new SqlParameter("@Id", guardianId) });
                        if (guardianRow == null)
                        { tx.Rollback(); ShowError("Guardian not found."); return; }
                        if (!Convert.ToBoolean(guardianRow))
                        { tx.Rollback(); ShowError("The selected guardian is inactive."); return; }

                        ExecuteNonQuery(conn, tx,
                            "UPDATE Students SET GuardianID = @GuardianID, UpdatedAt = GETDATE() WHERE StudentID = @StudentID",
                            new[] { new SqlParameter("@GuardianID", guardianId), new SqlParameter("@StudentID", SelectedStudentId) });

                        // Audit log, best-effort only — never blocks the assignment.
                        if (TableExists(conn, tx, "AuditLog"))
                        {
                            try
                            {
                                object userIdObj = Session["UserID"];
                                object userId = userIdObj != null ? (object)Convert.ToInt32(userIdObj) : DBNull.Value;
                                ExecuteNonQuery(conn, tx, @"
                                    INSERT INTO AuditLog (Action, EntityName, EntityID, UserID, Description, CreatedAt)
                                    VALUES ('Guardian Assigned', 'Student', @EntityID, @UserID, @Description, GETDATE())",
                                    new[]
                                    {
                                        new SqlParameter("@EntityID", SelectedStudentId),
                                        new SqlParameter("@UserID", userId),
                                        new SqlParameter("@Description", "Guardian reassigned to GuardianID " + guardianId)
                                    });
                            }
                            catch { }
                        }

                        tx.Commit();
                        ShowSuccess("Guardian assigned successfully.");
                        pnlConfirm.Visible = false;
                        SelectedStudentId = 0;
                        gvResults.DataSource = null;
                        gvResults.DataBind();
                    }
                    catch (Exception)
                    {
                        try { tx.Rollback(); } catch { }
                        ShowError("The assignment could not be completed due to a system error. Please try again.");
                    }
                }
            }
        }

        private bool TableExists(SqlConnection conn, SqlTransaction tx, string tableName)
        {
            object result = ExecuteScalar(conn, tx, "SELECT COUNT(1) FROM sys.tables WHERE name = @TableName",
                new[] { new SqlParameter("@TableName", tableName) });
            return Convert.ToInt32(result) > 0;
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
