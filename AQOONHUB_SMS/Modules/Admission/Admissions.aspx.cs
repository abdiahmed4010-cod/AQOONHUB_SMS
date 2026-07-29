using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Admission
{
    public partial class Admissions : System.Web.UI.Page
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["AQOONHUB_DB"].ConnectionString;

        // Exposed to markup for conditional rendering of Edit/Delete actions.
        public bool CanManage { get; private set; }

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

            CanManage = CanManageAdmissions();
            lnkAddAdmission.Visible = CanManage;

            if (!IsPostBack)
            {
                LoadClassFilter();
                LoadSummaryCards();
                LoadAdmissions();
                LoadRecentAdmissions();
            }
        }

        #region Summary / KPIs

        private void LoadSummaryCards()
        {
            string query = @"
                SELECT
                    COUNT(*) AS TotalCount,
                    SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END) AS PendingCount,
                    SUM(CASE WHEN Status = 'Under Review' THEN 1 ELSE 0 END) AS UnderReviewCount,
                    SUM(CASE WHEN Status IN ('Approved', 'Enrolled') THEN 1 ELSE 0 END) AS ApprovedCount,
                    SUM(CASE WHEN Status = 'Rejected' THEN 1 ELSE 0 END) AS RejectedCount,
                    SUM(CASE WHEN Status = 'Expired' THEN 1 ELSE 0 END) AS ExpiredCount,
                    SUM(CASE WHEN ApplicationDate >= @MonthStart AND ApplicationDate < @MonthEnd THEN 1 ELSE 0 END) AS NewThisMonth
                FROM Admissions";

            DateTime monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            SqlParameter[] p =
            {
                new SqlParameter("@MonthStart", monthStart),
                new SqlParameter("@MonthEnd", monthStart.AddMonths(1))
            };

            DataTable dt = ExecuteQuery(query, p);
            if (dt.Rows.Count == 0) return;
            DataRow row = dt.Rows[0];
            lblTotalCount.Text = SafeInt(row["TotalCount"]).ToString();
            lblNewCount.Text = SafeInt(row["NewThisMonth"]).ToString();
            lblPendingCount.Text = SafeInt(row["PendingCount"]).ToString();
            lblUnderReviewCount.Text = SafeInt(row["UnderReviewCount"]).ToString();
            lblApprovedCount.Text = SafeInt(row["ApprovedCount"]).ToString();
            lblRejectedCount.Text = SafeInt(row["RejectedCount"]).ToString();
            lblExpiredCount.Text = SafeInt(row["ExpiredCount"]).ToString();
        }

        private int SafeInt(object val) { return val == DBNull.Value ? 0 : Convert.ToInt32(val); }

        #endregion

        #region Dropdowns

        private void LoadClassFilter()
        {
            DataTable dt = ExecuteQuery("SELECT ClassID, ClassName FROM Classes ORDER BY ClassName");
            ddlClassFilter.Items.Clear();
            ddlClassFilter.Items.Add(new ListItem("All Classes", ""));
            foreach (DataRow row in dt.Rows)
                ddlClassFilter.Items.Add(new ListItem(row["ClassName"].ToString(), row["ClassID"].ToString()));
        }

        #endregion

        #region List

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
            int classFilterId;
            if (!string.IsNullOrEmpty(ddlClassFilter.SelectedValue) && int.TryParse(ddlClassFilter.SelectedValue, out classFilterId) && classFilterId > 0)
            {
                where += " AND a.ApplyingForClassID = @ClassFilter";
                parameters.Add(new SqlParameter("@ClassFilter", classFilterId));
            }
            DateTime fromDate;
            if (DateTime.TryParse(txtFromDate.Text, out fromDate))
            {
                where += " AND a.ApplicationDate >= @FromDate";
                parameters.Add(new SqlParameter("@FromDate", fromDate.Date));
            }
            DateTime toDate;
            if (DateTime.TryParse(txtToDate.Text, out toDate))
            {
                where += " AND a.ApplicationDate < @ToDate";
                parameters.Add(new SqlParameter("@ToDate", toDate.Date.AddDays(1)));
            }

            string query = @"
                SELECT
                    a.AdmissionID, a.ApplicationNo,
                    LTRIM(RTRIM(ISNULL(a.FirstName,'') + ' ' + ISNULL(a.LastName,''))) AS FullName,
                    a.Gender, a.DateOfBirth, a.Status, a.ApplicationDate,
                    a.GuardianName, a.GuardianPhone, a.StudentID, a.Shift,
                    c.ClassName
                FROM Admissions a
                INNER JOIN Classes c ON a.ApplyingForClassID = c.ClassID"
                + where + @"
                ORDER BY a.ApplicationDate DESC, a.AdmissionID DESC";

            DataTable dt = ExecuteQuery(query, parameters.ToArray());
            gvAdmissions.DataSource = dt;
            gvAdmissions.DataBind();

            int total = dt.Rows.Count;
            if (total == 0)
            {
                lblResultInfo.Text = "No entries";
            }
            else
            {
                int pageSize = gvAdmissions.PageSize;
                int start = gvAdmissions.PageIndex * pageSize + 1;
                int end = Math.Min(start + pageSize - 1, total);
                lblResultInfo.Text = string.Format("Showing {0} to {1} of {2} entries", start, end, total);
            }
        }

        private void LoadRecentAdmissions()
        {
            string query = @"
                SELECT TOP 5
                    a.ApplicationNo,
                    LTRIM(RTRIM(ISNULL(a.FirstName,'') + ' ' + ISNULL(a.LastName,''))) AS FullName,
                    c.ClassName, a.EnrolledAt, a.ApplicationDate
                FROM Admissions a
                INNER JOIN Classes c ON a.ApplyingForClassID = c.ClassID
                WHERE a.Status IN ('Approved', 'Enrolled')
                ORDER BY ISNULL(a.EnrolledAt, a.ApplicationDate) DESC, a.AdmissionID DESC";

            DataTable dt = ExecuteQuery(query);
            if (dt.Rows.Count == 0)
            {
                rptRecent.Visible = false;
                pnlNoRecent.Visible = true;
            }
            else
            {
                rptRecent.Visible = true;
                pnlNoRecent.Visible = false;
                rptRecent.DataSource = dt;
                rptRecent.DataBind();
            }
        }

        protected void gvAdmissions_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAdmissions.PageIndex = e.NewPageIndex;
            LoadAdmissions();
        }

        protected void gvAdmissions_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteRow")
            {
                if (!CanManage) { ShowError("You do not have permission to delete applications."); return; }

                int admissionId;
                if (!int.TryParse(Convert.ToString(e.CommandArgument), out admissionId)) return;

                // Block deletion once an application has been enrolled into a student record.
                DataTable check = ExecuteQuery(
                    "SELECT StudentID FROM Admissions WHERE AdmissionID = @Id",
                    new[] { new SqlParameter("@Id", admissionId) });

                if (check.Rows.Count == 0)
                {
                    ShowError("The application could not be found.");
                }
                else if (check.Rows[0]["StudentID"] != DBNull.Value)
                {
                    ShowError("This application has already been enrolled and cannot be deleted.");
                }
                else
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM Admissions WHERE AdmissionID = @Id AND StudentID IS NULL", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", admissionId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    ShowSuccess("Application deleted successfully.");
                }

                LoadSummaryCards();
                LoadAdmissions();
                LoadRecentAdmissions();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            gvAdmissions.PageIndex = 0;
            LoadAdmissions();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlStatus.SelectedIndex = 0;
            ddlClassFilter.SelectedIndex = 0;
            txtFromDate.Text = "";
            txtToDate.Text = "";
            gvAdmissions.PageIndex = 0;
            LoadAdmissions();
        }

        #endregion

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

        protected string GetShiftStyle(object shiftValue)
        {
            string shift = (shiftValue == null || shiftValue == DBNull.Value) ? "" : shiftValue.ToString();
            switch (shift)
            {
                case "Morning": return "background:#FFFBEB;color:#B45309";
                case "Afternoon": return "background:#EFF6FF;color:#1D4ED8";
                default: return "background:#F1F5F9;color:#64748B";
            }
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
                case "Expired": return "background:#F1F5F9;color:#64748B";
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
