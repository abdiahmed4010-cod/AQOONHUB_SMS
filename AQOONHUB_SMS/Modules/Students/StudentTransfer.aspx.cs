using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Students
{
    public partial class StudentTransfer : System.Web.UI.Page
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

        private int StudentId
        {
            get { return ViewState["StudentId"] == null ? 0 : (int)ViewState["StudentId"]; }
            set { ViewState["StudentId"] = value; }
        }

        private string CurrentStatus
        {
            get { return ViewState["CurrentStatus"] as string; }
            set { ViewState["CurrentStatus"] = value; }
        }

        private int CurrentAcademicYearId
        {
            get { return ViewState["CurAY"] == null ? 0 : (int)ViewState["CurAY"]; }
            set { ViewState["CurAY"] = value; }
        }

        private int CurrentSectionId
        {
            get { return ViewState["CurSec"] == null ? 0 : (int)ViewState["CurSec"]; }
            set { ViewState["CurSec"] = value; }
        }

        #region Authorization

        private string NormalizeRole(string role)
        {
            if (string.IsNullOrEmpty(role)) return string.Empty;
            return role.Trim().Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();
        }

        private static readonly string[] FullAccessRoles = { "superadmin", "admin", "registrar" };

        private bool CanManageTransfers()
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
            // Everyone with a session gets read-only history access (Teacher/Accountant
            // per the spec); write actions are separately gated by CanManageTransfers().
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

                if (!LoadStudent())
                {
                    pnlBody.Visible = false;
                    pnlNotFound.Visible = true;
                    return;
                }

                LoadCurrentTransfer();
                LoadTransferHistory();

                bool canManage = CanManageTransfers();
                pnlNoPermission.Visible = !canManage;
                btnTransfer.Visible = canManage;
                btnReturn.Visible = canManage;
            }
        }

        #region Load Student / Current Transfer / History

        private bool LoadStudent()
        {
            if (StudentId <= 0) return false;

            string query = @"
                SELECT
                    s.StudentID, s.StudentCode, s.AdmissionNo,
                    LTRIM(RTRIM(ISNULL(s.FirstName,'') + ' ' + ISNULL(s.LastName,''))) AS FullName,
                    s.Gender, s.Status, s.PhotoPath, s.AcademicYearID, s.SectionID,
                    g.FullName AS GuardianName, g.Phone AS GuardianPhone,
                    sec.SectionName, c.ClassName
                FROM Students s
                LEFT JOIN Guardians g ON s.GuardianID = g.GuardianID
                INNER JOIN Sections sec ON s.SectionID = sec.SectionID
                INNER JOIN Classes c ON sec.ClassID = c.ClassID
                WHERE s.StudentID = @StudentID AND s.Status <> 'Deleted'";

            DataTable dt = ExecuteQuery(query, new[] { new SqlParameter("@StudentID", StudentId) });
            if (dt.Rows.Count == 0) return false;

            DataRow row = dt.Rows[0];
            string fullName = row["FullName"].ToString();
            string status = row["Status"].ToString();

            CurrentStatus = status;
            CurrentAcademicYearId = row["AcademicYearID"] == DBNull.Value ? 0 : Convert.ToInt32(row["AcademicYearID"]);
            CurrentSectionId = Convert.ToInt32(row["SectionID"]);

            lblFullName.Text = fullName;
            lblStudentCode.Text = row["StudentCode"].ToString();
            lblAdmissionNo.Text = row["AdmissionNo"].ToString();
            lblGender.Text = row["Gender"].ToString();
            lblClassSection.Text = row["ClassName"] + " - " + row["SectionName"];
            lblGuardian.Text = row["GuardianName"] == DBNull.Value ? "Not assigned" : row["GuardianName"] + " (" + row["GuardianPhone"] + ")";

            lblStatusBadge.Text = status;
            ApplyStatusBadgeStyle(status);

            string photoPath = row["PhotoPath"] == DBNull.Value ? null : row["PhotoPath"].ToString();
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
                lblInitials.Text = GetInitials(fullName);
            }

            bool isTransferred = status == "Transferred";
            lblPageTitle.Text = isTransferred ? "Return to School" : "Transfer Student";

            if (isTransferred)
            {
                LoadReturnDropdowns();
            }

            return true;
        }

        private void ApplyStatusBadgeStyle(string status)
        {
            string bg, color;
            switch (status)
            {
                case "Active": bg = "#DCFCE7"; color = "#15803D"; break;
                case "Inactive": bg = "#F1F5F9"; color = "#64748B"; break;
                case "Graduated": bg = "#EDE9FE"; color = "#6D28D9"; break;
                case "Transferred": bg = "#E0F2FE"; color = "#0369A1"; break;
                default: bg = "#F1F5F9"; color = "#64748B"; break;
            }
            lblStatusBadge.Style["background"] = bg;
            lblStatusBadge.Style["color"] = color;
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "ST";
            string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        private void LoadCurrentTransfer()
        {
            if (CurrentStatus != "Transferred")
            {
                pnlActiveTransferInfo.Visible = false;
                pnlReturnForm.Visible = false;
                pnlTransferForm.Visible = true;
                return;
            }

            string query = @"
                SELECT TOP 1 *
                FROM StudentTransfers
                WHERE StudentID = @StudentID AND TransferStatus = 'Active' AND TransferType = 'External Transfer'
                ORDER BY CreatedAt DESC";

            DataTable dt;
            try
            {
                dt = ExecuteQuery(query, new[] { new SqlParameter("@StudentID", StudentId) });
            }
            catch (SqlException)
            {
                dt = new DataTable();
            }

            if (dt.Rows.Count == 0)
            {
                // Status says Transferred but no active transfer row found (or the table
                // doesn't exist yet) — still show the return form so the student isn't
                // stuck, but skip the "current transfer" detail block.
                pnlActiveTransferInfo.Visible = false;
                pnlTransferForm.Visible = false;
                pnlReturnForm.Visible = true;
                return;
            }

            DataRow row = dt.Rows[0];
            lblCurDestSchool.Text = row["DestinationSchool"] == DBNull.Value ? "—" : row["DestinationSchool"].ToString();
            lblCurDestLocation.Text = row["DestinationLocation"] == DBNull.Value ? "—" : row["DestinationLocation"].ToString();
            lblCurTransferDate.Text = Convert.ToDateTime(row["TransferDate"]).ToString("MMM dd, yyyy");
            lblCurReason.Text = row["TransferReason"].ToString();
            lblCurCertNo.Text = row["TransferCertificateNo"] == DBNull.Value ? "—" : row["TransferCertificateNo"].ToString();
            lblCurNotes.Text = row["TransferNotes"] == DBNull.Value ? "—" : row["TransferNotes"].ToString();
            lblCurProcessedBy.Text = row["TransferredBy"] == DBNull.Value ? "—" : ("User #" + row["TransferredBy"]);

            pnlActiveTransferInfo.Visible = true;
            pnlTransferForm.Visible = false;
            pnlReturnForm.Visible = true;
        }

        private void LoadTransferHistory()
        {
            string query = @"
                SELECT StudentTransferID, TransferType, DestinationSchool, TransferDate, TransferReason,
                       TransferCertificateNo, TransferStatus, ReturnedDate, CreatedAt
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
                // StudentTransfers table not present yet (script not run) — show empty
                // history instead of a raw SQL error.
                dt = new DataTable();
            }

            gvHistory.DataSource = dt;
            gvHistory.DataBind();
        }

        protected string GetHistoryStatusStyle(object statusValue)
        {
            string status = statusValue == null || statusValue == DBNull.Value ? "" : statusValue.ToString();
            switch (status)
            {
                case "Active": return "background:#FEE2E2;color:#B91C1C";
                case "Returned": return "background:#DCFCE7;color:#15803D";
                case "Cancelled": return "background:#F1F5F9;color:#64748B";
                default: return "background:#F1F5F9;color:#64748B";
            }
        }

        #endregion

        #region Dropdown Loading (Return placement)

        private void LoadReturnDropdowns()
        {
            DataTable years = ExecuteQuery("SELECT AcademicYearID, YearName, Status FROM AcademicYears ORDER BY StartDate DESC");
            ddlReturnAcademicYear.Items.Clear();
            ddlReturnAcademicYear.Items.Add(new ListItem("Select Academic Year", "0"));
            foreach (DataRow row in years.Rows)
            {
                string label = row["YearName"] + (row["Status"].ToString() == "Active" ? " (Current)" : "");
                ddlReturnAcademicYear.Items.Add(new ListItem(label, row["AcademicYearID"].ToString()));
            }

            DataTable classes = ExecuteQuery("SELECT ClassID, ClassName FROM Classes ORDER BY ClassName");
            ddlReturnClass.Items.Clear();
            ddlReturnClass.Items.Add(new ListItem("Select Class", "0"));
            foreach (DataRow row in classes.Rows)
                ddlReturnClass.Items.Add(new ListItem(row["ClassName"].ToString(), row["ClassID"].ToString()));

            LoadReturnSections(0);
        }

        private bool IsAdminLevel() { string r = NormalizeRole(Session["Role"] as string); return r == "superadmin" || r == "admin"; }

        /// <summary>Destination sections filtered by Class + selected Shift + Active. Admins also
        /// see NULL-shift sections labelled "Shift Not Assigned"; the save still blocks them.</summary>
        private void LoadReturnSections(int classId)
        {
            ddlReturnSection.Items.Clear();
            ddlReturnSection.Items.Add(new ListItem("Select Section", "0"));
            if (classId <= 0) return;

            string shift = ddlReturnShift.SelectedValue;
            if (shift == "Morning" || shift == "Afternoon")
            {
                DataTable dt = ExecuteQuery(
                    "SELECT SectionID, SectionName FROM Sections WHERE ClassID=@ClassID AND Status='Active' AND Shift=@Shift ORDER BY SectionName",
                    new[] { new SqlParameter("@ClassID", classId), new SqlParameter("@Shift", shift) });
                foreach (DataRow row in dt.Rows)
                    ddlReturnSection.Items.Add(new ListItem(row["SectionName"].ToString(), row["SectionID"].ToString()));
            }

            if (IsAdminLevel())
            {
                DataTable un = ExecuteQuery(
                    "SELECT SectionID, SectionName FROM Sections WHERE ClassID=@ClassID AND Status='Active' AND Shift IS NULL ORDER BY SectionName",
                    new[] { new SqlParameter("@ClassID", classId) });
                foreach (DataRow row in un.Rows)
                {
                    var li = new ListItem(row["SectionName"] + " — Shift Not Assigned", row["SectionID"].ToString());
                    li.Attributes["data-unassigned"] = "1";
                    ddlReturnSection.Items.Add(li);
                }
            }
        }

        protected void ddlReturnClass_SelectedIndexChanged(object sender, EventArgs e) { ReloadReturnSections(); }
        protected void ddlReturnShift_Changed(object sender, EventArgs e) { ReloadReturnSections(); }

        private void ReloadReturnSections()
        {
            int classId;
            int.TryParse(ddlReturnClass.SelectedValue, out classId);
            LoadReturnSections(classId);
        }

        #endregion

        #region Helpers

        /// <summary>Checks whether a table exists, so optional integrations (StudentEnrollments,
        /// AuditLog) are only touched when actually present — never guessed.</summary>
        private bool TableExists(SqlConnection conn, SqlTransaction tx, string tableName)
        {
            object result = ExecuteScalar(conn, tx,
                "SELECT COUNT(1) FROM sys.tables WHERE name = @TableName",
                new[] { new SqlParameter("@TableName", tableName) });
            return Convert.ToInt32(result) > 0;
        }

        #endregion

        #region Transfer Validation + Action

        protected void cvTransferDate_ServerValidate(object source, ServerValidateEventArgs args)
        {
            DateTime d;
            if (!DateTime.TryParse(txtTransferDate.Text, out d)) { args.IsValid = false; return; }
            args.IsValid = d.Date <= DateTime.Now.Date.AddDays(7); // small tolerance, not "far" in the future
        }

        protected void btnTransfer_Click(object sender, EventArgs e)
        {
            if (!CanManageTransfers())
            {
                ShowError("You do not have permission to transfer students.");
                return;
            }
            if (!Page.IsValid) return;

            if (CurrentStatus == "Transferred")
            {
                ShowError("This student already has an active transfer.");
                return;
            }

            DateTime transferDate;
            if (!DateTime.TryParse(txtTransferDate.Text, out transferDate))
            {
                ShowError("Please provide a valid transfer date.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        object statusCheck = ExecuteScalar(conn, tx,
                            "SELECT Status FROM Students WITH (UPDLOCK, HOLDLOCK) WHERE StudentID = @StudentID",
                            new[] { new SqlParameter("@StudentID", StudentId) });

                        if (statusCheck == null)
                        {
                            tx.Rollback();
                            ShowError("Student not found.");
                            return;
                        }
                        if (statusCheck.ToString() == "Transferred")
                        {
                            tx.Rollback();
                            ShowError("This student already has an active transfer.");
                            return;
                        }

                        object activeExists = ExecuteScalar(conn, tx,
                            @"SELECT COUNT(1) FROM StudentTransfers WITH (UPDLOCK, HOLDLOCK)
                              WHERE StudentID = @StudentID AND TransferStatus = 'Active' AND TransferType = 'External Transfer'",
                            new[] { new SqlParameter("@StudentID", StudentId) });
                        if (Convert.ToInt32(activeExists) > 0)
                        {
                            tx.Rollback();
                            ShowError("This student already has an active transfer.");
                            return;
                        }

                        int? currentUserId = null;
                        object userIdObj = Session["UserID"];
                        if (userIdObj != null) currentUserId = Convert.ToInt32(userIdObj);

                        string insertQuery = @"
                            INSERT INTO StudentTransfers
                                (StudentID, TransferType, FromAcademicYearID, FromSectionID,
                                 DestinationSchool, DestinationLocation, DestinationContactPerson, DestinationPhone,
                                 TransferDate, TransferReason, TransferCertificateNo, TransferNotes,
                                 TransferStatus, TransferredBy, CreatedAt, UpdatedAt)
                            VALUES
                                (@StudentID, @TransferType, @FromAcademicYearID, @FromSectionID,
                                 @DestinationSchool, @DestinationLocation, @DestinationContactPerson, @DestinationPhone,
                                 @TransferDate, @TransferReason, @TransferCertificateNo, @TransferNotes,
                                 'Active', @TransferredBy, SYSDATETIME(), SYSDATETIME())";

                        ExecuteNonQuery(conn, tx, insertQuery, new[]
                        {
                            new SqlParameter("@StudentID", StudentId),
                            new SqlParameter("@TransferType", ddlTransferType.SelectedValue),
                            new SqlParameter("@FromAcademicYearID", CurrentAcademicYearId > 0 ? (object)CurrentAcademicYearId : DBNull.Value),
                            new SqlParameter("@FromSectionID", CurrentSectionId),
                            new SqlParameter("@DestinationSchool", txtDestSchool.Text.Trim()),
                            new SqlParameter("@DestinationLocation", txtDestLocation.Text.Trim()),
                            new SqlParameter("@DestinationContactPerson", string.IsNullOrWhiteSpace(txtDestContact.Text) ? (object)DBNull.Value : txtDestContact.Text.Trim()),
                            new SqlParameter("@DestinationPhone", string.IsNullOrWhiteSpace(txtDestPhone.Text) ? (object)DBNull.Value : txtDestPhone.Text.Trim()),
                            new SqlParameter("@TransferDate", transferDate),
                            new SqlParameter("@TransferReason", txtTransferReason.Text.Trim()),
                            new SqlParameter("@TransferCertificateNo", string.IsNullOrWhiteSpace(txtCertNo.Text) ? (object)DBNull.Value : txtCertNo.Text.Trim()),
                            new SqlParameter("@TransferNotes", string.IsNullOrWhiteSpace(txtTransferNotes.Text) ? (object)DBNull.Value : txtTransferNotes.Text.Trim()),
                            new SqlParameter("@TransferredBy", (object)currentUserId ?? DBNull.Value)
                        });

                        ExecuteNonQuery(conn, tx,
                            "UPDATE Students SET Status = 'Transferred', UpdatedAt = GETDATE() WHERE StudentID = @StudentID",
                            new[] { new SqlParameter("@StudentID", StudentId) });

                        // Close active enrollment only if StudentEnrollments exists.
                        if (TableExists(conn, tx, "StudentEnrollments"))
                        {
                            ExecuteNonQuery(conn, tx, @"
                                UPDATE StudentEnrollments SET Status = 'Closed', CompletionDate = @CompletionDate
                                WHERE StudentID = @StudentID AND Status = 'Active'",
                                new[]
                                {
                                    new SqlParameter("@CompletionDate", transferDate),
                                    new SqlParameter("@StudentID", StudentId)
                                });
                        }

                        // Audit log only if the table exists — best-effort, never blocks the transfer.
                        if (TableExists(conn, tx, "AuditLog"))
                        {
                            try
                            {
                                ExecuteNonQuery(conn, tx, @"
                                    INSERT INTO AuditLog (Action, EntityName, EntityID, UserID, Description, CreatedAt)
                                    VALUES ('Student Transferred', 'Student', @EntityID, @UserID, @Description, GETDATE())",
                                    new[]
                                    {
                                        new SqlParameter("@EntityID", StudentId),
                                        new SqlParameter("@UserID", (object)currentUserId ?? DBNull.Value),
                                        new SqlParameter("@Description", "Transferred to " + txtDestSchool.Text.Trim())
                                    });
                            }
                            catch { /* audit is best-effort */ }
                        }

                        tx.Commit();
                        Response.Redirect("~/Modules/Students/StudentTransfer.aspx?id=" + StudentId, true);
                    }
                    catch (Exception)
                    {
                        try { tx.Rollback(); } catch { }
                        ShowError("The transfer could not be completed due to a system error. Please try again.");
                    }
                }
            }
        }

        #endregion

        #region Return Action

        protected void btnReturn_Click(object sender, EventArgs e)
        {
            if (!CanManageTransfers())
            {
                ShowError("You do not have permission to return students to school.");
                return;
            }
            if (!Page.IsValid) return;

            DateTime returnDate;
            if (!DateTime.TryParse(txtReturnDate.Text, out returnDate))
            {
                ShowError("Please provide a valid return date.");
                return;
            }

            int academicYearId, classId, sectionId;
            if (!int.TryParse(ddlReturnAcademicYear.SelectedValue, out academicYearId) || academicYearId <= 0)
            { ShowError("Please select an academic year."); return; }
            if (!int.TryParse(ddlReturnClass.SelectedValue, out classId) || classId <= 0)
            { ShowError("Please select a class."); return; }
            if (!int.TryParse(ddlReturnSection.SelectedValue, out sectionId) || sectionId <= 0)
            { ShowError("Please select a section."); return; }

            object sectionCheck = ExecuteScalar(
                "SELECT COUNT(1) FROM Sections WHERE SectionID = @SectionID AND ClassID = @ClassID AND Status = 'Active'",
                new[] { new SqlParameter("@SectionID", sectionId), new SqlParameter("@ClassID", classId) });
            if (Convert.ToInt32(sectionCheck) == 0)
            { ShowError("The selected section does not belong to the selected class, or is not active."); return; }

            // Shift-aware validation (server-side authoritative): the destination section must
            // carry an assigned shift that matches the selected return shift. NULL is never valid.
            string returnShift = ddlReturnShift.SelectedValue;
            if (returnShift != "Morning" && returnShift != "Afternoon")
            { ShowError("Select a valid shift."); return; }
            object destShiftObj = ExecuteScalar("SELECT Shift FROM Sections WHERE SectionID = @s",
                new[] { new SqlParameter("@s", sectionId) });
            string destShift = (destShiftObj == null || destShiftObj == DBNull.Value) ? null : Convert.ToString(destShiftObj);
            if (string.IsNullOrEmpty(destShift))
            { ShowError("The selected section has no assigned shift. Assign the section's shift first (Classes & Sections)."); return; }
            if (!string.Equals(destShift, returnShift, StringComparison.OrdinalIgnoreCase))
            { ShowError("The selected section belongs to the " + destShift + " shift. Choose a matching section or shift."); return; }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        object statusCheck = ExecuteScalar(conn, tx,
                            "SELECT Status FROM Students WITH (UPDLOCK, HOLDLOCK) WHERE StudentID = @StudentID",
                            new[] { new SqlParameter("@StudentID", StudentId) });

                        if (statusCheck == null)
                        {
                            tx.Rollback();
                            ShowError("Student not found.");
                            return;
                        }
                        if (statusCheck.ToString() != "Transferred")
                        {
                            tx.Rollback();
                            ShowError("This student does not currently have an active transfer to return from.");
                            return;
                        }

                        object activeTransferIdObj = ExecuteScalar(conn, tx, @"
                            SELECT TOP 1 StudentTransferID FROM StudentTransfers WITH (UPDLOCK, HOLDLOCK)
                            WHERE StudentID = @StudentID AND TransferStatus = 'Active' AND TransferType = 'External Transfer'
                            ORDER BY CreatedAt DESC",
                            new[] { new SqlParameter("@StudentID", StudentId) });

                        if (activeTransferIdObj == null)
                        {
                            tx.Rollback();
                            ShowError("No active transfer record was found for this student.");
                            return;
                        }

                        int activeTransferId = Convert.ToInt32(activeTransferIdObj);
                        int? currentUserId = null;
                        object userIdObj = Session["UserID"];
                        if (userIdObj != null) currentUserId = Convert.ToInt32(userIdObj);

                        ExecuteNonQuery(conn, tx, @"
                            UPDATE StudentTransfers SET
                                TransferStatus = 'Returned',
                                ReturnedDate = @ReturnedDate,
                                ReturnAcademicYearID = @ReturnAcademicYearID,
                                ReturnSectionID = @ReturnSectionID,
                                ReturnReason = @ReturnReason,
                                ReturnNotes = @ReturnNotes,
                                ReturnedBy = @ReturnedBy,
                                UpdatedAt = SYSDATETIME()
                            WHERE StudentTransferID = @StudentTransferID",
                            new[]
                            {
                                new SqlParameter("@ReturnedDate", returnDate),
                                new SqlParameter("@ReturnAcademicYearID", academicYearId),
                                new SqlParameter("@ReturnSectionID", sectionId),
                                new SqlParameter("@ReturnReason", txtReturnReason.Text.Trim()),
                                new SqlParameter("@ReturnNotes", string.IsNullOrWhiteSpace(txtReturnNotes.Text) ? (object)DBNull.Value : txtReturnNotes.Text.Trim()),
                                new SqlParameter("@ReturnedBy", (object)currentUserId ?? DBNull.Value),
                                new SqlParameter("@StudentTransferID", activeTransferId)
                            });

                        ExecuteNonQuery(conn, tx, @"
                            UPDATE Students SET
                                Status = 'Active',
                                AcademicYearID = @AcademicYearID,
                                SectionID = @SectionID,
                                UpdatedAt = GETDATE()
                            WHERE StudentID = @StudentID",
                            new[]
                            {
                                new SqlParameter("@AcademicYearID", academicYearId),
                                new SqlParameter("@SectionID", sectionId),
                                new SqlParameter("@StudentID", StudentId)
                            });

                        if (TableExists(conn, tx, "StudentEnrollments"))
                        {
                            ExecuteNonQuery(conn, tx, @"
                                INSERT INTO StudentEnrollments (StudentID, AcademicYearID, SectionID, EnrollmentDate, Status, CreatedAt)
                                VALUES (@StudentID, @AcademicYearID, @SectionID, @EnrollmentDate, 'Active', GETDATE())",
                                new[]
                                {
                                    new SqlParameter("@StudentID", StudentId),
                                    new SqlParameter("@AcademicYearID", academicYearId),
                                    new SqlParameter("@SectionID", sectionId),
                                    new SqlParameter("@EnrollmentDate", returnDate)
                                });
                        }

                        if (TableExists(conn, tx, "AuditLog"))
                        {
                            try
                            {
                                ExecuteNonQuery(conn, tx, @"
                                    INSERT INTO AuditLog (Action, EntityName, EntityID, UserID, Description, CreatedAt)
                                    VALUES ('Student Returned', 'Student', @EntityID, @UserID, @Description, GETDATE())",
                                    new[]
                                    {
                                        new SqlParameter("@EntityID", StudentId),
                                        new SqlParameter("@UserID", (object)currentUserId ?? DBNull.Value),
                                        new SqlParameter("@Description", "Returned to school, status set to Active")
                                    });
                            }
                            catch { }
                        }

                        tx.Commit();
                        Response.Redirect("~/Modules/Students/StudentTransfer.aspx?id=" + StudentId, true);
                    }
                    catch (Exception)
                    {
                        try { tx.Rollback(); } catch { }
                        ShowError("The return could not be completed due to a system error. Please try again.");
                    }
                }
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
