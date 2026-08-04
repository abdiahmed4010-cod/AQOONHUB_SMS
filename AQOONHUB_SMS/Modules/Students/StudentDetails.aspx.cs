using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Students
{
    public partial class StudentDetails : System.Web.UI.Page
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

        private int StudentId
        {
            get { return ViewState["StudentId"] == null ? 0 : (int)ViewState["StudentId"]; }
            set { ViewState["StudentId"] = value; }
        }

        private int GuardianId
        {
            get { return ViewState["GuardianId"] == null ? 0 : (int)ViewState["GuardianId"]; }
            set { ViewState["GuardianId"] = value; }
        }

        #region Authorization (same normalized-role pattern as Students.aspx / AddStudent.aspx)

        private string NormalizeRole(string role)
        {
            if (string.IsNullOrEmpty(role)) return string.Empty;
            return role.Trim().Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();
        }

        private static readonly string[] AllowedNormalizedRoles = { "superadmin", "admin", "registrar" };

        private bool CanManageStudent()
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
            // Read-only view is allowed for any logged-in role (matches Students.aspx list
            // permissions); action buttons are hidden below for roles that can't manage students.
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
                StudentId = id;
                LoadStudent();
            }

            bool canManage = CanManageStudent();
            lnkEdit.Visible = canManage;
            btnToggleActive.Visible = canManage;
            btnGraduate.Visible = canManage;
            btnDelete.Visible = canManage;
            // lnkTransfer stays visible for everyone — StudentTransfer.aspx itself shows
            // read-only history to Teacher/Accountant and hides the write actions there.
        }

        private void LoadStudent()
        {
            if (StudentId <= 0)
            {
                pnlBody.Visible = false;
                pnlNotFound.Visible = true;
                return;
            }

            string query = @"
                SELECT
                    s.StudentID, s.StudentCode, s.AdmissionNo, s.FirstName, s.LastName,
                    LTRIM(RTRIM(ISNULL(s.FirstName,'') + ' ' + ISNULL(s.LastName,''))) AS FullName,
                    s.Gender, s.DateOfBirth, s.Status, s.PhotoPath, s.MedicalNotes, s.Address, s.EnrollmentDate,
                    s.GuardianID,
                    g.FullName AS GuardianName, g.Phone AS GuardianPhone,
                    sec.SectionName, c.ClassName, ay.YearName AS AcademicYearName
                FROM Students s
                INNER JOIN Guardians g ON s.GuardianID = g.GuardianID
                INNER JOIN Sections sec ON s.SectionID = sec.SectionID
                INNER JOIN Classes c ON sec.ClassID = c.ClassID
                LEFT JOIN AcademicYears ay ON s.AcademicYearID = ay.AcademicYearID
                WHERE s.StudentID = @StudentID AND s.Status <> 'Deleted'";

            DataTable dt = ExecuteQuery(query, new[] { new SqlParameter("@StudentID", StudentId) });

            if (dt.Rows.Count == 0)
            {
                pnlBody.Visible = false;
                pnlNotFound.Visible = true;
                return;
            }

            pnlBody.Visible = true;
            pnlNotFound.Visible = false;
            BindRow(dt.Rows[0]);
        }

        private void BindRow(DataRow row)
        {
            string fullName = row["FullName"].ToString();
            string status = row["Status"].ToString();
            string photoPath = row["PhotoPath"] == DBNull.Value ? null : row["PhotoPath"].ToString();
            DateTime dob = Convert.ToDateTime(row["DateOfBirth"]);
            int age = DateTime.Now.Year - dob.Year;
            if (DateTime.Now.DayOfYear < dob.DayOfYear) age--;

            lblFullName.Text = fullName;
            lblStudentCode.Text = row["StudentCode"].ToString();
            lblAdmissionNo.Text = row["AdmissionNo"].ToString();
            lblClassSection.Text = row["ClassName"] + " - " + row["SectionName"];

            lblGenderStat.Text = row["Gender"].ToString();
            lblAgeStat.Text = age.ToString();
            lblStatusStat.Text = status;

            lblStatusBadge.Text = status;
            lblStatusBadge.Style["background"] = GetStatusBg(status);
            lblStatusBadge.Style["color"] = GetStatusColor(status);

            if (!string.IsNullOrEmpty(photoPath))
            {
                imgPhoto.ImageUrl = ResolveUrl("~/" + photoPath);
                imgPhoto.Visible = true;
                pnlPhotoFallback.Visible = false;
            }
            else
            {
                imgPhoto.Visible = false;
                pnlPhotoFallback.Visible = true;
                pnlPhotoFallback.Style["background"] = "#7C3AED";
                lblInitials.Text = GetInitials(fullName);
            }

            lblDetailName.Text = fullName;
            lblDetailGender.Text = row["Gender"].ToString();
            lblDetailDob.Text = dob.ToString("MMM dd, yyyy");
            lblDetailAddress.Text = row["Address"] == DBNull.Value ? "—" : row["Address"].ToString();
            lblDetailMedical.Text = row["MedicalNotes"] == DBNull.Value ? "None recorded" : row["MedicalNotes"].ToString();

            lblDetailStudentCode.Text = row["StudentCode"].ToString();
            lblDetailAdmissionNo.Text = row["AdmissionNo"].ToString();
            lblDetailClassSection.Text = row["ClassName"] + " - " + row["SectionName"];
            lblDetailAcademicYear.Text = row["AcademicYearName"] == DBNull.Value ? "—" : row["AcademicYearName"].ToString();
            lblDetailGuardian.Text = row["GuardianName"] + " (" + row["GuardianPhone"] + ")";
            lblDetailEnrolled.Text = Convert.ToDateTime(row["EnrollmentDate"]).ToString("MMM dd, yyyy");

            GuardianId = row["GuardianID"] == DBNull.Value ? 0 : Convert.ToInt32(row["GuardianID"]);
            RenderParentAccount();

            lblToggleActiveText.Text = status == "Active" ? "Deactivate" : "Activate";
            lnkEdit.NavigateUrl = ResolveUrl("~/Modules/Students/EditStudent.aspx?id=" + StudentId);
            lnkTransfer.NavigateUrl = ResolveUrl("~/Modules/Students/StudentTransfer.aspx?id=" + StudentId);
            lblTransferLinkText.Text = status == "Transferred" ? "Return to School" : "Transfer Student";

            // Disable graduate/deactivate actions once a student is already in a
            // terminal state, to avoid contradictory status changes. Transfer/Return
            // stays enabled always — that page itself decides which mode to show.
            bool isTerminal = status == "Graduated" || status == "Transferred";
            btnGraduate.Enabled = !isTerminal;

            LoadLatestTransferSummary();
        }

        /// <summary>
        /// Shows a brief summary of the most recent transfer record, if the
        /// StudentTransfers table exists and has any rows for this student.
        /// Silently shows nothing if the table isn't present yet.
        /// </summary>
        private void LoadLatestTransferSummary()
        {
            string query = @"
                SELECT TOP 1 TransferType, DestinationSchool, TransferDate, TransferStatus, ReturnedDate
                FROM StudentTransfers
                WHERE StudentID = @StudentID
                ORDER BY CreatedAt DESC";

            DataTable dt;
            try
            {
                dt = ExecuteQuery(query, new[] { new SqlParameter("@StudentID", StudentId) });
            }
            catch (SqlException)
            {
                pnlTransferSummary.Visible = false;
                return;
            }

            if (dt.Rows.Count == 0)
            {
                pnlTransferSummary.Visible = false;
                return;
            }

            DataRow row = dt.Rows[0];
            string status = row["TransferStatus"].ToString();
            string dest = row["DestinationSchool"] == DBNull.Value ? "—" : row["DestinationSchool"].ToString();
            DateTime transferDate = Convert.ToDateTime(row["TransferDate"]);

            if (status == "Active")
            {
                lblTransferSummaryText.Text = "Currently transferred to " + dest + " (since " + transferDate.ToString("MMM dd, yyyy") + ").";
            }
            else if (status == "Returned")
            {
                DateTime returned = row["ReturnedDate"] == DBNull.Value ? transferDate : Convert.ToDateTime(row["ReturnedDate"]);
                lblTransferSummaryText.Text = "Previously transferred to " + dest + "; returned on " + returned.ToString("MMM dd, yyyy") + ".";
            }
            else
            {
                lblTransferSummaryText.Text = "Last transfer to " + dest + " was cancelled.";
            }

            pnlTransferSummary.Visible = true;
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "ST";
            string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        private string GetStatusBg(string status)
        {
            switch (status)
            {
                case "Active": return "#DCFCE7";
                case "Inactive": return "#F1F5F9";
                case "Graduated": return "#EDE9FE";
                case "Transferred": return "#E0F2FE";
                default: return "#F1F5F9";
            }
        }

        private string GetStatusColor(string status)
        {
            switch (status)
            {
                case "Active": return "#15803D";
                case "Inactive": return "#64748B";
                case "Graduated": return "#6D28D9";
                case "Transferred": return "#0369A1";
                default: return "#64748B";
            }
        }

        #region Status Actions

        private static readonly string[] AllowedStatuses = { "Active", "Inactive", "Graduated", "Transferred" };

        private void UpdateStatus(string newStatus)
        {
            if (!CanManageStudent())
            {
                ShowError("You do not have permission to change this student's status.");
                return;
            }

            bool ok = false;
            foreach (string s in AllowedStatuses) if (s == newStatus) { ok = true; break; }
            if (!ok) return;

            ExecuteNonQuery(
                "UPDATE Students SET Status = @Status, UpdatedAt = GETDATE() WHERE StudentID = @StudentID",
                new[] { new SqlParameter("@Status", newStatus), new SqlParameter("@StudentID", StudentId) });

            ShowSuccess("Status updated to " + newStatus + ".");
            LoadStudent();
        }

        protected void btnToggleActive_Click(object sender, EventArgs e)
        {
            UpdateStatus(lblToggleActiveText.Text == "Deactivate" ? "Inactive" : "Active");
        }

        protected void btnGraduate_Click(object sender, EventArgs e)
        {
            UpdateStatus("Graduated");
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (!CanManageStudent())
            {
                ShowError("You do not have permission to delete this student.");
                return;
            }

            ExecuteNonQuery(
                "UPDATE Students SET Status = 'Deleted', UpdatedAt = GETDATE() WHERE StudentID = @StudentID",
                new[] { new SqlParameter("@StudentID", StudentId) });

            Response.Redirect("~/Modules/Students/Students.aspx", true);
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

        #region Parent Account Provisioning

        /// <summary>Renders guardian-account status and shows the create button only when the
        /// current user may manage students AND the guardian has no account with a valid email.</summary>
        private void RenderParentAccount()
        {
            var st = AQOONHUB_SMS.Modules.Parents.ParentAccountService.GetStatus(GuardianId);
            if (st == null) return;

            lblPAName.Text = Server.HtmlEncode(st.Name ?? "—");
            lblPAEmail.Text = Server.HtmlEncode(string.IsNullOrEmpty(st.Email) ? "— (no email on file)" : st.Email);
            lblPAPhone.Text = Server.HtmlEncode(string.IsNullOrEmpty(st.Phone) ? "—" : st.Phone);
            lblPALinkedEmail.Text = string.IsNullOrEmpty(st.LinkedUserEmail) ? "—" : Server.HtmlEncode(st.LinkedUserEmail);

            lblPABadge.Text = st.AccountStatus;
            switch (st.AccountStatus)
            {
                case "Linked Account": lblPABadge.Style["background"] = "#DCFCE7"; lblPABadge.Style["color"] = "#15803D"; break;
                case "Inactive Account": lblPABadge.Style["background"] = "#FEF3C7"; lblPABadge.Style["color"] = "#B45309"; break;
                default: lblPABadge.Style["background"] = "#F1F5F9"; lblPABadge.Style["color"] = "#64748B"; break;
            }

            // Server-side gate: only managers, only when provisioning is genuinely possible.
            btnCreateParentAccount.Visible = CanManageStudent() && st.CanProvision;
        }

        protected void btnCreateParentAccount_Click(object sender, EventArgs e)
        {
            // Authoritative server-side authorization (not just button hiding).
            if (!CanManageStudent())
            {
                Response.StatusCode = 403;
                Response.Redirect("~/Modules/Dashboard/Dashboard.aspx?denied=students", true);
                return;
            }

            int actor;
            int.TryParse(Convert.ToString(Session["UserID"]), out actor);

            string tempPassword, message;
            var outcome = AQOONHUB_SMS.Modules.Parents.ParentAccountService.Provision(
                GuardianId, actor > 0 ? actor : (int?)null, Request.UserHostAddress, out tempPassword, out message);

            if (outcome == AQOONHUB_SMS.Modules.Parents.ParentAccountService.Outcome.CreatedNew
                || outcome == AQOONHUB_SMS.Modules.Parents.ParentAccountService.Outcome.LinkedExisting)
            {
                pnlPAError.Visible = false;
                pnlPASuccess.Visible = true;
                lblPASuccessMsg.Text = (outcome == AQOONHUB_SMS.Modules.Parents.ParentAccountService.Outcome.CreatedNew)
                    ? "Parent account created successfully. Copy this temporary password now. It will not be shown again."
                    : Server.HtmlEncode(message);
                if (outcome == AQOONHUB_SMS.Modules.Parents.ParentAccountService.Outcome.CreatedNew && !string.IsNullOrEmpty(tempPassword))
                {
                    pnlTempWrap.Visible = true;
                    lblPATempPassword.Text = Server.HtmlEncode(tempPassword);
                }
                LoadStudent();          // refresh status badge (now Linked); button hides
            }
            else
            {
                pnlPASuccess.Visible = false;
                pnlPAError.Visible = true;
                lblPAError.Text = Server.HtmlEncode(message ?? "The parent account could not be created.");
                LoadStudent();
            }
        }

        #endregion
    }
}
