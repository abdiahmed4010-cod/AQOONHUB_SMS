using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Students
{
    public partial class Students : System.Web.UI.Page
    {
        // ------------------------------------------------------------------
        // Local ADO.NET access. This page does NOT depend on DatabaseHelper —
        // DatabaseHelper.cs is left completely untouched.
        // ------------------------------------------------------------------
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["AQOONHUB_DB"].ConnectionString;

        private DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        private object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

        private int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        private const int DefaultPageSize = 10;

        private int CurrentPage
        {
            get { return ViewState["CurrentPage"] == null ? 1 : (int)ViewState["CurrentPage"]; }
            set { ViewState["CurrentPage"] = value; }
        }

        private int PageSize
        {
            get { return ViewState["PageSize"] == null ? DefaultPageSize : (int)ViewState["PageSize"]; }
            set { ViewState["PageSize"] = value; }
        }

        private string SortExpression
        {
            get { return ViewState["SortExpression"] == null ? "CreatedAt" : (string)ViewState["SortExpression"]; }
            set { ViewState["SortExpression"] = value; }
        }

        private string SortDirection
        {
            get { return ViewState["SortDirection"] == null ? "DESC" : (string)ViewState["SortDirection"]; }
            set { ViewState["SortDirection"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!CheckAccess())
                return;

            if (!IsPostBack)
            {
                LoadClasses();
                LoadSections(0);
                LoadSummaryCards();
                LoadStudents();
            }
        }

        #region Authorization

        private bool CheckAccess()
        {
            string role = Session["Role"] as string;

            if (string.IsNullOrEmpty(role))
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx", true);
                return false;
            }

            if (role.Equals("Accountant", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Modules/Authentication/NotAuthorized.aspx", true);
                return false;
            }

            bool readOnly = role.Equals("Teacher", StringComparison.OrdinalIgnoreCase);
            if (readOnly)
            {
                lnkAddStudent.Visible = false;
            }

            return true;
        }

        #endregion

        #region Dropdown Loading

        private void LoadClasses()
        {
            string query = "SELECT ClassID, ClassName FROM Classes ORDER BY ClassName";
            DataTable dt = ExecuteQuery(query);

            ddlClass.Items.Clear();
            ddlClass.Items.Add(new ListItem("All Classes", "0"));
            foreach (DataRow row in dt.Rows)
            {
                ddlClass.Items.Add(new ListItem(row["ClassName"].ToString(), row["ClassID"].ToString()));
            }
        }

        private void LoadSections(int classId)
        {
            ddlSection.Items.Clear();
            ddlSection.Items.Add(new ListItem("All Sections", "0"));

            string query = "SELECT SectionID, SectionName FROM Sections WHERE (@ClassID = 0 OR ClassID = @ClassID) ORDER BY SectionName";
            SqlParameter[] parameters = { new SqlParameter("@ClassID", classId) };
            DataTable dt = ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                ddlSection.Items.Add(new ListItem(row["SectionName"].ToString(), row["SectionID"].ToString()));
            }
        }

        protected void ddlClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            int classId = 0;
            int.TryParse(ddlClass.SelectedValue, out classId);
            LoadSections(classId);
            CurrentPage = 1;
            LoadStudents();
        }

        #endregion

        #region Summary Cards

        private void LoadSummaryCards()
        {
            string query = @"
                SELECT
                    COUNT(*) AS TotalCount,
                    SUM(CASE WHEN Status = 'Active' THEN 1 ELSE 0 END) AS ActiveCount,
                    SUM(CASE WHEN Status = 'Inactive' THEN 1 ELSE 0 END) AS InactiveCount,
                    SUM(CASE WHEN Status = 'Graduated' THEN 1 ELSE 0 END) AS GraduatedCount,
                    SUM(CASE WHEN Status = 'Transferred' THEN 1 ELSE 0 END) AS TransferredCount
                FROM Students
                WHERE Status <> 'Deleted'";

            DataTable dt = ExecuteQuery(query);
            if (dt.Rows.Count == 0) return;

            DataRow row = dt.Rows[0];
            lblTotalStudents.Text = SafeInt(row["TotalCount"]).ToString();
            lblActiveStudents.Text = SafeInt(row["ActiveCount"]).ToString();
            lblInactiveStudents.Text = SafeInt(row["InactiveCount"]).ToString();
            lblGraduatedStudents.Text = SafeInt(row["GraduatedCount"]).ToString();
            lblTransferredStudents.Text = SafeInt(row["TransferredCount"]).ToString();
        }

        private int SafeInt(object val)
        {
            return val == DBNull.Value ? 0 : Convert.ToInt32(val);
        }

        #endregion

        #region Query Building + Load

        private void BuildStudentFilter(out string whereClause, out List<SqlParameter> parameters)
        {
            parameters = new List<SqlParameter>();
            string where = " WHERE s.Status <> 'Deleted'";

            string search = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(search))
            {
                where += @" AND (
                    s.FirstName LIKE @Search OR
                    s.LastName LIKE @Search OR
                    (s.FirstName + ' ' + s.LastName) LIKE @Search OR
                    s.StudentCode LIKE @Search OR
                    s.AdmissionNo LIKE @Search OR
                    g.FullName LIKE @Search OR
                    g.Phone LIKE @Search
                )";
                parameters.Add(new SqlParameter("@Search", "%" + search + "%"));
            }

            int classId;
            if (int.TryParse(ddlClass.SelectedValue, out classId) && classId > 0)
            {
                where += " AND c.ClassID = @ClassID";
                parameters.Add(new SqlParameter("@ClassID", classId));
            }

            int sectionId;
            if (int.TryParse(ddlSection.SelectedValue, out sectionId) && sectionId > 0)
            {
                where += " AND sec.SectionID = @SectionID";
                parameters.Add(new SqlParameter("@SectionID", sectionId));
            }

            if (!string.IsNullOrEmpty(ddlGender.SelectedValue))
            {
                where += " AND s.Gender = @Gender";
                parameters.Add(new SqlParameter("@Gender", ddlGender.SelectedValue));
            }

            if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
            {
                where += " AND s.Status = @Status";
                parameters.Add(new SqlParameter("@Status", ddlStatus.SelectedValue));
            }

            whereClause = where;
        }

        private static readonly Dictionary<string, string> SortColumnMap = new Dictionary<string, string>
        {
            { "StudentCode", "s.StudentCode" },
            { "AdmissionNo", "s.AdmissionNo" },
            { "FirstName", "s.FirstName" },
            { "Gender", "s.Gender" },
            { "DateOfBirth", "s.DateOfBirth" },
            { "ClassName", "c.ClassName" },
            { "SectionName", "sec.SectionName" },
            { "GuardianName", "g.FullName" },
            { "EnrollmentDate", "s.EnrollmentDate" },
            { "Status", "s.Status" },
            { "CreatedAt", "s.CreatedAt" }
        };

        private SqlParameter[] CloneParameters(IEnumerable<SqlParameter> source)
        {
            List<SqlParameter> clones = new List<SqlParameter>();
            foreach (SqlParameter p in source)
            {
                clones.Add((SqlParameter)((ICloneable)p).Clone());
            }
            return clones.ToArray();
        }

        private void LoadStudents()
        {
            string whereClause;
            List<SqlParameter> filterParams;
            BuildStudentFilter(out whereClause, out filterParams);

            string countQuery = @"
                SELECT COUNT(*) 
                FROM Students s
                INNER JOIN Guardians g ON s.GuardianID = g.GuardianID
                INNER JOIN Sections sec ON s.SectionID = sec.SectionID
                INNER JOIN Classes c ON sec.ClassID = c.ClassID
                LEFT JOIN AcademicYears ay ON s.AcademicYearID = ay.AcademicYearID"
                + whereClause;

            int totalCount = Convert.ToInt32(ExecuteScalar(countQuery, CloneParameters(filterParams)));

            int totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            if (totalPages < 1) totalPages = 1;
            if (CurrentPage > totalPages) CurrentPage = totalPages;
            if (CurrentPage < 1) CurrentPage = 1;

            string sortCol;
            if (!SortColumnMap.TryGetValue(SortExpression, out sortCol))
                sortCol = "s.CreatedAt";

            int offset = (CurrentPage - 1) * PageSize;

            string pageQuery = @"
                SELECT
                    s.StudentID, s.StudentCode, s.AdmissionNo,
                    s.FirstName, s.LastName,
                    LTRIM(RTRIM(ISNULL(s.FirstName, '') + ' ' + ISNULL(s.LastName, ''))) AS FullName,
                    s.Gender, s.DateOfBirth, s.Status, s.PhotoPath, s.EnrollmentDate,
                    g.FullName AS GuardianName, g.Phone AS GuardianPhone,
                    sec.SectionName, c.ClassName,
                    ay.YearName AS AcademicYearName
                FROM Students s
                INNER JOIN Guardians g ON s.GuardianID = g.GuardianID
                INNER JOIN Sections sec ON s.SectionID = sec.SectionID
                INNER JOIN Classes c ON sec.ClassID = c.ClassID
                LEFT JOIN AcademicYears ay ON s.AcademicYearID = ay.AcademicYearID"
                + whereClause + @"
                ORDER BY " + sortCol + " " + SortDirection + @"
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            List<SqlParameter> pageParams = new List<SqlParameter>(CloneParameters(filterParams));
            pageParams.Add(new SqlParameter("@Offset", offset));
            pageParams.Add(new SqlParameter("@PageSize", PageSize));

            DataTable dt = ExecuteQuery(pageQuery, pageParams.ToArray());

            gvStudents.DataSource = dt;
            gvStudents.DataBind();

            int shownFrom = totalCount == 0 ? 0 : offset + 1;
            int shownTo = Math.Min(offset + PageSize, totalCount);
            lblResultsSummary.Text = string.Format("Showing {0}–{1} of {2}", shownFrom, shownTo, totalCount);
            lblPageIndicator.Text = string.Format("Page {0} of {1}", CurrentPage, totalPages);

            btnPrevPage.Enabled = CurrentPage > 1;
            btnNextPage.Enabled = CurrentPage < totalPages;
        }

        #endregion

        #region Search / Filter Events

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            CurrentPage = 1;
            LoadStudents();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            ddlClass.SelectedIndex = 0;
            LoadSections(0);
            ddlGender.SelectedIndex = 0;
            ddlStatus.SelectedIndex = 0;
            CurrentPage = 1;
            SortExpression = "CreatedAt";
            SortDirection = "DESC";
            LoadStudents();
        }

        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            int size;
            if (int.TryParse(ddlPageSize.SelectedValue, out size))
                PageSize = size;
            CurrentPage = 1;
            LoadStudents();
        }

        protected void btnPrevPage_Click(object sender, EventArgs e)
        {
            if (CurrentPage > 1) CurrentPage--;
            LoadStudents();
        }

        protected void btnNextPage_Click(object sender, EventArgs e)
        {
            CurrentPage++;
            LoadStudents();
        }

        #endregion

        #region Sorting

        protected void gvStudents_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (SortExpression == e.SortExpression)
            {
                SortDirection = SortDirection == "ASC" ? "DESC" : "ASC";
            }
            else
            {
                SortExpression = e.SortExpression;
                SortDirection = "ASC";
            }
            CurrentPage = 1;
            LoadStudents();
        }

        #endregion

        #region Row Actions (status changes)

        protected void gvStudents_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string role = Session["Role"] as string;
            bool readOnly = role != null && role.Equals("Teacher", StringComparison.OrdinalIgnoreCase);
            if (readOnly)
            {
                return;
            }

            try
            {
                if (e.CommandName == "SoftDelete")
                {
                    string[] parts = e.CommandArgument.ToString().Split('|');
                    int studentId = Convert.ToInt32(parts[0]);
                    SoftDeleteStudent(studentId);
                }

                LoadSummaryCards();
                LoadStudents();
            }
            catch (Exception)
            {
                LoadStudents();
            }
        }

        /// <summary>
        /// Soft-deletes a student (Status = 'Deleted'). Restore/permanent-delete is
        /// a Super Admin action, handled elsewhere (Audit Log / Trash), not here.
        /// </summary>
        private void SoftDeleteStudent(int studentId)
        {
            string query = "UPDATE Students SET Status = 'Deleted', UpdatedAt = GETDATE() WHERE StudentID = @StudentID";
            SqlParameter[] parameters = { new SqlParameter("@StudentID", studentId) };
            ExecuteNonQuery(query, parameters);
        }

        #endregion

        #region Template Helpers (called from markup)

        /// <summary>
        /// Returns up to two uppercase initials from a full name.
        /// Never throws on null/DBNull/empty input; falls back to "ST".
        /// </summary>
        protected string GetInitials(object fullNameValue)
        {
            string fullName = (fullNameValue == null || fullNameValue == DBNull.Value)
                ? null
                : fullNameValue.ToString();

            if (string.IsNullOrWhiteSpace(fullName))
                return "ST";

            string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return "ST";

            if (parts.Length == 1)
                return parts[0].Substring(0, 1).ToUpperInvariant();

            string first = parts[0].Length > 0 ? parts[0].Substring(0, 1) : string.Empty;
            string last = parts[parts.Length - 1].Length > 0 ? parts[parts.Length - 1].Substring(0, 1) : string.Empty;
            string initials = (first + last).ToUpperInvariant();

            return string.IsNullOrEmpty(initials) ? "ST" : initials;
        }

        /// <summary>
        /// Overload supporting a first-name/last-name call pattern, in case the
        /// markup is later changed to bind FirstName/LastName separately.
        /// </summary>
        protected string GetInitials(object firstNameValue, object lastNameValue)
        {
            string first = (firstNameValue == null || firstNameValue == DBNull.Value) ? "" : firstNameValue.ToString();
            string last = (lastNameValue == null || lastNameValue == DBNull.Value) ? "" : lastNameValue.ToString();
            string combined = (first + " " + last).Trim();
            return GetInitials((object)combined);
        }

        /// <summary>
        /// Returns an inline CSS style string (background/color) matching the
        /// BADGE_STYLES palette used in the shared prototype design.
        /// </summary>
        protected string GetStatusBadgeStyle(object statusValue)
        {
            string status = (statusValue == null || statusValue == DBNull.Value) ? "" : statusValue.ToString();

            switch (status)
            {
                case "Active": return "background:#DCFCE7;color:#15803D";
                case "Inactive": return "background:#F1F5F9;color:#64748B";
                case "Graduated": return "background:#EDE9FE;color:#6D28D9";
                case "Transferred": return "background:#E0F2FE;color:#0369A1";
                default: return "background:#F1F5F9;color:#64748B";
            }
        }

        private static readonly string[] AvatarColors =
        {
            "#2563EB", "#7C3AED", "#0EA5E9", "#22C55E", "#F59E0B", "#EF4444", "#EC4899", "#14B8A6"
        };

        /// <summary>
        /// Deterministically picks an avatar color from a name, matching the
        /// prototype's avColor() character-sum hash so the same name always
        /// gets the same color.
        /// </summary>
        protected string GetAvatarColor(object fullNameValue)
        {
            string name = (fullNameValue == null || fullNameValue == DBNull.Value) ? "" : fullNameValue.ToString();
            int sum = 0;
            foreach (char c in name) sum += c;
            return AvatarColors[Math.Abs(sum) % AvatarColors.Length];
        }

        #endregion
    }
}
