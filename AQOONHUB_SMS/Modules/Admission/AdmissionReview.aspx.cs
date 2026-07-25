using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Admission
{
    public partial class AdmissionReview : System.Web.UI.Page
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

        private object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteScalar();
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

        private int ExecuteNonQueryDirect(string query, SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        private int AdmissionId
        {
            get { return ViewState["AdmissionId"] == null ? 0 : (int)ViewState["AdmissionId"]; }
            set { ViewState["AdmissionId"] = value; }
        }

        private int ApplyingForClassId
        {
            get { return ViewState["ClassId"] == null ? 0 : (int)ViewState["ClassId"]; }
            set { ViewState["ClassId"] = value; }
        }

        private int CurrentGuardianId
        {
            get { return ViewState["CurGuardianId"] == null ? 0 : (int)ViewState["CurGuardianId"]; }
            set { ViewState["CurGuardianId"] = value; }
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
            return true; // read-only view allowed; write actions gated separately below
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!CheckAuthorization()) return;

            if (!IsPostBack)
            {
                int id;
                int.TryParse(Request.QueryString["id"], out id);
                AdmissionId = id;

                if (!LoadApplication())
                {
                    pnlBody.Visible = false;
                    pnlNotFound.Visible = true;
                    return;
                }
            }

            bool canManage = CanManageAdmissions();
            pnlActionForm.Visible = canManage && pnlActionForm.Visible;
        }

        private bool LoadApplication()
        {
            if (AdmissionId <= 0) return false;

            string query = @"
                SELECT a.*, c.ClassName
                FROM Admissions a
                INNER JOIN Classes c ON a.ApplyingForClassID = c.ClassID
                WHERE a.AdmissionID = @Id";

            DataTable dt = ExecuteQuery(query, new[] { new SqlParameter("@Id", AdmissionId) });
            if (dt.Rows.Count == 0) return false;

            DataRow row = dt.Rows[0];
            string fullName = row["FirstName"] + " " + row["LastName"];
            string status = row["Status"].ToString();
            ApplyingForClassId = Convert.ToInt32(row["ApplyingForClassID"]);

            lblFullName.Text = fullName;
            lblApplicationNo.Text = row["ApplicationNo"].ToString();
            lblGender.Text = row["Gender"].ToString();
            lblDob.Text = Convert.ToDateTime(row["DateOfBirth"]).ToString("MMM dd, yyyy");
            lblClass.Text = row["ClassName"].ToString();
            lblAppDate.Text = Convert.ToDateTime(row["ApplicationDate"]).ToString("MMM dd, yyyy");
            lblGuardianName.Text = row["GuardianName"].ToString();
            lblGuardianPhone.Text = row["GuardianPhone"].ToString();
            lblGuardianEmail.Text = row["GuardianEmail"] == DBNull.Value ? "—" : row["GuardianEmail"].ToString();
            lblNotes.Text = row["Notes"] == DBNull.Value ? "—" : row["Notes"].ToString();
            lblReviewed.Text = row["ReviewedBy"] == DBNull.Value
                ? "Not yet reviewed"
                : ("User #" + row["ReviewedBy"] + " — " + (row["ReviewedAt"] == DBNull.Value ? "—" : Convert.ToDateTime(row["ReviewedAt"]).ToString("MMM dd, yyyy HH:mm")));

            lblStatusBadge.Text = status;
            ApplyStatusStyle(status);

            bool isFinal = status == "Approved" || status == "Enrolled" || status == "Rejected";
            bool hasGuardian = dt.Columns.Contains("GuardianID") && row["GuardianID"] != DBNull.Value;
            CurrentGuardianId = hasGuardian ? Convert.ToInt32(row["GuardianID"]) : 0;

            pnlAlreadyFinalized.Visible = isFinal;
            if (isFinal)
            {
                pnlActionForm.Visible = false;
                pnlNoGuardianWarning.Visible = false;
                lblFinalizedText.Text = (status == "Approved" || status == "Enrolled")
                    ? "This application has already been approved and converted to a student."
                    : "This application has already been rejected.";
            }
            else
            {
                LoadAcademicYears();
                LoadSections(ApplyingForClassId);

                if (!hasGuardian)
                {
                    pnlNoGuardianWarning.Visible = true;
                    pnlActionForm.Visible = true;
                    btnApprove.Enabled = false;
                    LoadLinkableGuardians();
                }
                else
                {
                    pnlNoGuardianWarning.Visible = false;
                    pnlActionForm.Visible = true;
                    btnApprove.Enabled = true;
                }
            }

            return true;
        }

        private void LoadLinkableGuardians()
        {
            DataTable dt = ExecuteQuery("SELECT GuardianID, FullName, Phone FROM Guardians WHERE IsActive = 1 ORDER BY FullName");
            ddlLinkExistingGuardian.Items.Clear();
            ddlLinkExistingGuardian.Items.Add(new ListItem("Select Guardian", "0"));
            foreach (DataRow r in dt.Rows)
                ddlLinkExistingGuardian.Items.Add(new ListItem(r["FullName"] + " — " + r["Phone"], r["GuardianID"].ToString()));
        }

        private void ApplyStatusStyle(string status)
        {
            string bg, color;
            switch (status)
            {
                case "Pending": bg = "#FFFBEB"; color = "#B45309"; break;
                case "Under Review": bg = "#EFF6FF"; color = "#1D4ED8"; break;
                case "Approved": bg = "#DCFCE7"; color = "#15803D"; break;
                case "Enrolled": bg = "#DCFCE7"; color = "#15803D"; break;
                case "Rejected": bg = "#FEE2E2"; color = "#B91C1C"; break;
                default: bg = "#F1F5F9"; color = "#64748B"; break;
            }
            lblStatusBadge.Style["background"] = bg;
            lblStatusBadge.Style["color"] = color;
        }

        private void LoadAcademicYears()
        {
            DataTable dt = ExecuteQuery("SELECT AcademicYearID, YearName, Status FROM AcademicYears ORDER BY StartDate DESC");
            ddlAcademicYear.Items.Clear();
            ddlAcademicYear.Items.Add(new ListItem("Select Academic Year", "0"));
            foreach (DataRow row in dt.Rows)
            {
                string label = row["YearName"] + (row["Status"].ToString() == "Active" ? " (Current)" : "");
                ddlAcademicYear.Items.Add(new ListItem(label, row["AcademicYearID"].ToString()));
            }
            foreach (DataRow row in dt.Rows)
            {
                if (row["Status"].ToString() == "Active")
                {
                    ListItem item = ddlAcademicYear.Items.FindByValue(row["AcademicYearID"].ToString());
                    if (item != null) { ddlAcademicYear.ClearSelection(); item.Selected = true; }
                    break;
                }
            }
        }

        private void LoadSections(int classId)
        {
            ddlSection.Items.Clear();
            ddlSection.Items.Add(new ListItem("Select Section", "0"));
            if (classId <= 0) return;

            DataTable dt = ExecuteQuery(
                "SELECT SectionID, SectionName FROM Sections WHERE ClassID = @ClassID ORDER BY SectionName",
                new[] { new SqlParameter("@ClassID", classId) });
            foreach (DataRow row in dt.Rows)
                ddlSection.Items.Add(new ListItem(row["SectionName"].ToString(), row["SectionID"].ToString()));
        }

        #region Actions

        protected void btnUnderReview_Click(object sender, EventArgs e)
        {
            if (!CanManageAdmissions()) { ShowError("You do not have permission to review applications."); return; }

            ExecuteNonQueryDirect(
                "UPDATE Admissions SET Status = 'Under Review', ReviewedBy = @UserId, ReviewedAt = GETDATE() WHERE AdmissionID = @Id AND Status = 'Pending'",
                new[] { new SqlParameter("@UserId", CurrentUserIdOrNull()), new SqlParameter("@Id", AdmissionId) });

            ShowSuccess("Application marked as Under Review.");
            LoadApplication();
        }

        protected void btnLinkExistingGuardian_Click(object sender, EventArgs e)
        {
            if (!CanManageAdmissions()) { ShowError("You do not have permission to review applications."); return; }

            int guardianId;
            if (!int.TryParse(ddlLinkExistingGuardian.SelectedValue, out guardianId) || guardianId <= 0)
            { ShowError("Please select a guardian to link."); return; }

            object exists = ExecuteScalar(
                "SELECT COUNT(1) FROM Guardians WHERE GuardianID = @Id",
                new[] { new SqlParameter("@Id", guardianId) });
            if (Convert.ToInt32(exists) == 0)
            { ShowError("The selected guardian could not be found."); return; }

            ExecuteNonQueryDirect(
                "UPDATE Admissions SET GuardianID = @GuardianID, UpdatedAt = SYSDATETIME() WHERE AdmissionID = @Id",
                new[] { new SqlParameter("@GuardianID", guardianId), new SqlParameter("@Id", AdmissionId) });

            ShowSuccess("Guardian linked to this application.");
            LoadApplication();
        }

        protected void btnCreateGuardianFromApp_Click(object sender, EventArgs e)
        {
            if (!CanManageAdmissions()) { ShowError("You do not have permission to review applications."); return; }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        DataTable dt;
                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT GuardianName, GuardianPhone, GuardianEmail FROM Admissions WITH (UPDLOCK, HOLDLOCK) WHERE AdmissionID = @Id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Id", AdmissionId);
                            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            {
                                dt = new DataTable();
                                da.Fill(dt);
                            }
                        }
                        if (dt.Rows.Count == 0)
                        { tx.Rollback(); ShowError("Application not found."); return; }

                        DataRow app = dt.Rows[0];
                        string guardianName = app["GuardianName"].ToString();
                        string guardianPhone = app["GuardianPhone"].ToString();
                        object guardianEmail = app["GuardianEmail"] == DBNull.Value ? DBNull.Value : (object)app["GuardianEmail"];

                        // Check for a likely-duplicate existing Guardian — surfaced as part of
                        // the confirmation message rather than blocked, since the registrar
                        // already confirmed via the client-side confirm() dialog before this
                        // request was sent.
                        object dupCount = ExecuteScalar(conn, tx,
                            "SELECT COUNT(1) FROM Guardians WHERE Phone = @Phone OR (@Email IS NOT NULL AND Email = @Email)",
                            new[] { new SqlParameter("@Phone", guardianPhone), new SqlParameter("@Email", guardianEmail) });
                        bool possibleDuplicate = Convert.ToInt32(dupCount) > 0;

                        int newGuardianId;
                        using (SqlCommand cmd = new SqlCommand(@"
                            INSERT INTO Guardians (FullName, Relationship, Phone, Email, IsActive, CreatedAt, UpdatedAt)
                            OUTPUT INSERTED.GuardianID
                            VALUES (@FullName, 'Guardian', @Phone, @Email, 1, SYSDATETIME(), SYSDATETIME())", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@FullName", guardianName);
                            cmd.Parameters.AddWithValue("@Phone", guardianPhone);
                            cmd.Parameters.AddWithValue("@Email", guardianEmail);
                            newGuardianId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        ExecuteNonQuery(conn, tx,
                            "UPDATE Admissions SET GuardianID = @GuardianID, UpdatedAt = SYSDATETIME() WHERE AdmissionID = @Id",
                            new[] { new SqlParameter("@GuardianID", newGuardianId), new SqlParameter("@Id", AdmissionId) });

                        tx.Commit();
                        ShowSuccess(possibleDuplicate
                            ? "Guardian created and linked, but a similar guardian already existed — review Parents.aspx for possible duplicates."
                            : "Guardian created and linked to this application.");
                        LoadApplication();
                    }
                    catch (Exception)
                    {
                        try { tx.Rollback(); } catch { }
                        ShowError("The guardian could not be created due to a system error. Please try again.");
                    }
                }
            }
        }

        protected void btnReject_Click(object sender, EventArgs e)
        {
            if (!CanManageAdmissions()) { ShowError("You do not have permission to reject applications."); return; }

            ExecuteNonQueryDirect(
                "UPDATE Admissions SET Status = 'Rejected', ReviewedBy = @UserId, ReviewedAt = GETDATE() WHERE AdmissionID = @Id",
                new[] { new SqlParameter("@UserId", CurrentUserIdOrNull()), new SqlParameter("@Id", AdmissionId) });

            ShowSuccess("Application rejected.");
            LoadApplication();
        }

        protected void btnApprove_Click(object sender, EventArgs e)
        {
            if (!CanManageAdmissions()) { ShowError("You do not have permission to approve applications."); return; }
            if (!Page.IsValid) return;

            int academicYearId, sectionId;
            if (!int.TryParse(ddlAcademicYear.SelectedValue, out academicYearId) || academicYearId <= 0)
            { ShowError("Please select an academic year."); return; }
            if (!int.TryParse(ddlSection.SelectedValue, out sectionId) || sectionId <= 0)
            { ShowError("Please select a section."); return; }

            object sectionCheck = ExecuteScalar(
                "SELECT COUNT(1) FROM Sections WHERE SectionID = @SectionID AND ClassID = @ClassID",
                new[] { new SqlParameter("@SectionID", sectionId), new SqlParameter("@ClassID", ApplyingForClassId) });
            if (Convert.ToInt32(sectionCheck) == 0)
            { ShowError("The selected section does not belong to the applicant's class."); return; }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        object statusCheck = ExecuteScalar(conn, tx,
                            "SELECT Status FROM Admissions WITH (UPDLOCK, HOLDLOCK) WHERE AdmissionID = @Id",
                            new[] { new SqlParameter("@Id", AdmissionId) });

                        if (statusCheck == null)
                        {
                            tx.Rollback();
                            ShowError("Application not found.");
                            return;
                        }
                        if (statusCheck.ToString() == "Approved" || statusCheck.ToString() == "Enrolled" || statusCheck.ToString() == "Rejected")
                        {
                            tx.Rollback();
                            ShowError("This application has already been finalized.");
                            return;
                        }

                        DataTable dt;
                        using (SqlCommand cmd = new SqlCommand("SELECT * FROM Admissions WHERE AdmissionID = @Id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Id", AdmissionId);
                            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            {
                                dt = new DataTable();
                                da.Fill(dt);
                            }
                        }
                        DataRow app = dt.Rows[0];

                        // Permanent fix: GuardianID must already be validated and present on
                        // the Admissions row (linked at application time, or via "Link Existing
                        // Guardian" / "Create Guardian From Application" on this page) — never
                        // create a throwaway Guardian here at approval time.
                        if (app["GuardianID"] == DBNull.Value)
                        {
                            tx.Rollback();
                            ShowError("This application is not linked to a valid Guardian record. Select or create a Guardian before enrollment.");
                            return;
                        }
                        int guardianId = Convert.ToInt32(app["GuardianID"]);

                        object guardianCheck = ExecuteScalar(conn, tx,
                            "SELECT IsActive FROM Guardians WITH (UPDLOCK, HOLDLOCK) WHERE GuardianID = @Id",
                            new[] { new SqlParameter("@Id", guardianId) });
                        if (guardianCheck == null)
                        {
                            tx.Rollback();
                            ShowError("The linked Guardian record could not be found. Please re-link a valid Guardian.");
                            return;
                        }
                        if (!Convert.ToBoolean(guardianCheck))
                        {
                            tx.Rollback();
                            ShowError("The linked Guardian is inactive. Activate the Guardian or link a different one before enrolling.");
                            return;
                        }

                        string studentCode = GenerateUniqueStudentCode(conn, tx);
                        string admissionNo = GenerateUniqueAdmissionNumber(conn, tx);

                        string insertStudent = @"
                            INSERT INTO Students
                                (StudentCode, AdmissionNo, FirstName, LastName, Gender, DateOfBirth,
                                 GuardianID, SectionID, AcademicYearID, Status, EnrollmentDate, CreatedAt, UpdatedAt)
                            OUTPUT INSERTED.StudentID
                            VALUES
                                (@StudentCode, @AdmissionNo, @FirstName, @LastName, @Gender, @DateOfBirth,
                                 @GuardianID, @SectionID, @AcademicYearID, 'Active', GETDATE(), GETDATE(), GETDATE())";

                        int newStudentId;
                        using (SqlCommand cmd = new SqlCommand(insertStudent, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@StudentCode", studentCode);
                            cmd.Parameters.AddWithValue("@AdmissionNo", admissionNo);
                            cmd.Parameters.AddWithValue("@FirstName", app["FirstName"]);
                            cmd.Parameters.AddWithValue("@LastName", app["LastName"]);
                            cmd.Parameters.AddWithValue("@Gender", app["Gender"]);
                            cmd.Parameters.AddWithValue("@DateOfBirth", app["DateOfBirth"]);
                            cmd.Parameters.AddWithValue("@GuardianID", guardianId);
                            cmd.Parameters.AddWithValue("@SectionID", sectionId);
                            cmd.Parameters.AddWithValue("@AcademicYearID", academicYearId);
                            newStudentId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        ExecuteNonQuery(conn, tx,
                            "UPDATE Admissions SET Status = 'Enrolled', StudentID = @StudentID, EnrolledBy = @UserId, EnrolledAt = GETDATE(), ReviewedBy = @UserId, ReviewedAt = GETDATE() WHERE AdmissionID = @Id",
                            new[]
                            {
                                new SqlParameter("@StudentID", newStudentId),
                                new SqlParameter("@UserId", CurrentUserIdOrNull()),
                                new SqlParameter("@Id", AdmissionId)
                            });

                        tx.Commit();
                        ShowSuccess("Application approved. Student enrolled as " + studentCode + " (Admission No " + admissionNo + ").");
                        LoadApplication();
                    }
                    catch (Exception)
                    {
                        try { tx.Rollback(); } catch { }
                        ShowError("The application could not be approved due to a system error. Please try again.");
                    }
                }
            }
        }

        private object CurrentUserIdOrNull()
        {
            object userIdObj = Session["UserID"];
            return userIdObj != null ? (object)Convert.ToInt32(userIdObj) : DBNull.Value;
        }

        /// <summary>Same StudentCode generation pattern as AddStudent.aspx.cs (AQH-{year}-{0000}).</summary>
        private string GenerateUniqueStudentCode(SqlConnection conn, SqlTransaction tx)
        {
            for (int attempt = 0; attempt < 5; attempt++)
            {
                int year = DateTime.Now.Year;
                string prefix = "AQH-" + year + "-";

                object result;
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT TOP 1 StudentCode FROM Students WITH (UPDLOCK, HOLDLOCK) WHERE StudentCode LIKE @Prefix + '%' ORDER BY StudentCode DESC", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@Prefix", prefix);
                    result = cmd.ExecuteScalar();
                }

                int nextNumber = 1;
                if (result != null && result != DBNull.Value)
                {
                    string last = result.ToString();
                    string numPart = last.Substring(last.LastIndexOf('-') + 1);
                    int lastNum;
                    if (int.TryParse(numPart, out lastNum)) nextNumber = lastNum + 1;
                }
                string candidate = prefix + nextNumber.ToString("D4");

                object exists = ExecuteScalar(conn, tx,
                    "SELECT COUNT(1) FROM Students WITH (UPDLOCK, HOLDLOCK) WHERE StudentCode = @Code",
                    new[] { new SqlParameter("@Code", candidate) });
                if (Convert.ToInt32(exists) == 0) return candidate;
            }
            throw new InvalidOperationException("Could not generate a unique Student Code after several attempts.");
        }

        /// <summary>Same AdmissionNo generation pattern as AddStudent.aspx.cs (ADM-{0000}, numeric-safe).</summary>
        private string GenerateUniqueAdmissionNumber(SqlConnection conn, SqlTransaction tx)
        {
            for (int attempt = 0; attempt < 5; attempt++)
            {
                object result = ExecuteScalar(conn, tx, @"
                    SELECT ISNULL(MAX(TRY_CONVERT(INT, SUBSTRING(AdmissionNo, 5, LEN(AdmissionNo)))), 0) + 1
                    FROM Students WITH (UPDLOCK, HOLDLOCK) WHERE AdmissionNo LIKE 'ADM-%'");

                int nextNumber = (result == null || result == DBNull.Value) ? 1 : Convert.ToInt32(result);
                string candidate = "ADM-" + nextNumber.ToString("D4");

                object exists = ExecuteScalar(conn, tx,
                    "SELECT COUNT(1) FROM Students WITH (UPDLOCK, HOLDLOCK) WHERE AdmissionNo = @No",
                    new[] { new SqlParameter("@No", candidate) });
                if (Convert.ToInt32(exists) == 0) return candidate;
            }
            throw new InvalidOperationException("Could not generate a unique Admission Number after several attempts.");
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
