using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Students
{
    public partial class EditStudent : System.Web.UI.Page
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

        private string CurrentPhotoPath
        {
            get { return ViewState["CurrentPhotoPath"] as string; }
            set { ViewState["CurrentPhotoPath"] = value; }
        }

        // ---- Placement baseline (captured when the form loads) — the authoritative "original"
        //      used for change detection and concurrency comparison. Never trusted from the client. ----
        private int OrigYearId { get { return ViewState["OrigYear"] == null ? 0 : (int)ViewState["OrigYear"]; } set { ViewState["OrigYear"] = value; } }
        private int OrigSectionId { get { return ViewState["OrigSection"] == null ? 0 : (int)ViewState["OrigSection"]; } set { ViewState["OrigSection"] = value; } }
        private int OrigClassId { get { return ViewState["OrigClass"] == null ? 0 : (int)ViewState["OrigClass"]; } set { ViewState["OrigClass"] = value; } }
        private string OrigShift { get { return ViewState["OrigShift"] as string ?? ""; } set { ViewState["OrigShift"] = value ?? ""; } }
        private DateTime OrigEnrollmentDate { get { return ViewState["OrigEnroll"] == null ? DateTime.MinValue : (DateTime)ViewState["OrigEnroll"]; } set { ViewState["OrigEnroll"] = value; } }
        private string PendingPhotoPath { get { return ViewState["PendingPhoto"] as string; } set { ViewState["PendingPhoto"] = value; } }
        private string ConfirmToken { get { return ViewState["ConfirmToken"] as string; } set { ViewState["ConfirmToken"] = value; } }

        #region Authorization

        private string NormalizeRole(string role)
        {
            if (string.IsNullOrEmpty(role)) return string.Empty;
            return role.Trim().Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();
        }

        private static readonly string[] AllowedNormalizedRoles = { "superadmin", "admin", "registrar" };

        private bool CanEditStudent()
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
            if (!CanEditStudent())
            {
                ShowError("You do not have permission to edit students. This page is available to Super Admin, Admin, and Registrar roles only.");
                pnlFormBody.Visible = false;
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
                StudentId = id;

                LoadAcademicYears();
                LoadClasses();

                if (!LoadStudent())
                {
                    pnlFormBody.Visible = false;
                    pnlNotFound.Visible = true;
                }
            }
        }

        #region Dropdown Loading (same pattern as AddStudent.aspx.cs)

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
        }

        private void LoadClasses()
        {
            DataTable dt = ExecuteQuery("SELECT ClassID, ClassName FROM Classes ORDER BY ClassName");
            ddlClass.Items.Clear();
            ddlClass.Items.Add(new ListItem("Select Class", "0"));
            foreach (DataRow row in dt.Rows)
                ddlClass.Items.Add(new ListItem(row["ClassName"].ToString(), row["ClassID"].ToString()));
        }

        /// <summary>Only superadmin/admin may see (but not save to) unassigned-shift sections.</summary>
        private bool IsAdminLevel() { string r = NormalizeRole(Session["Role"] as string); return r == "superadmin" || r == "admin"; }

        /// <summary>
        /// Binds Sections filtered by Class + Shift + Active. Admins additionally see NULL-shift
        /// sections labelled "Shift Not Assigned". When <paramref name="keepSelected"/> is set, the
        /// student's current Section is kept selectable even if it no longer matches the filter, so
        /// editing demographics never forces a silent move (a real move is blocked server-side).
        /// </summary>
        private void LoadSections(int classId, string shift, int selectSectionId = 0, bool keepSelected = false)
        {
            ddlSection.Items.Clear();
            ddlSection.Items.Add(new ListItem("Select Section", "0"));
            if (classId <= 0) return;

            if (shift == "Morning" || shift == "Afternoon")
            {
                DataTable dt = ExecuteQuery("SELECT SectionID, SectionName FROM Sections WHERE ClassID=@ClassID AND Status='Active' AND Shift=@Shift ORDER BY SectionName",
                    new[] { new SqlParameter("@ClassID", classId), new SqlParameter("@Shift", shift) });
                foreach (DataRow row in dt.Rows)
                    ddlSection.Items.Add(new ListItem(row["SectionName"].ToString(), row["SectionID"].ToString()));
            }

            if (IsAdminLevel())
            {
                DataTable un = ExecuteQuery("SELECT SectionID, SectionName FROM Sections WHERE ClassID=@ClassID AND Status='Active' AND Shift IS NULL ORDER BY SectionName",
                    new[] { new SqlParameter("@ClassID", classId) });
                foreach (DataRow row in un.Rows)
                {
                    var li = new ListItem(row["SectionName"] + " — Shift Not Assigned", row["SectionID"].ToString());
                    li.Attributes["data-unassigned"] = "1";
                    ddlSection.Items.Add(li);
                }
            }

            // Keep the current section selectable on initial load even if it no longer matches.
            if (keepSelected && selectSectionId > 0 && ddlSection.Items.FindByValue(selectSectionId.ToString()) == null)
            {
                DataTable cur = ExecuteQuery("SELECT SectionName FROM Sections WHERE SectionID=@s AND ClassID=@c",
                    new[] { new SqlParameter("@s", selectSectionId), new SqlParameter("@c", classId) });
                if (cur.Rows.Count > 0)
                    ddlSection.Items.Add(new ListItem(cur.Rows[0]["SectionName"] + " — Current", selectSectionId.ToString()));
            }

            if (selectSectionId > 0)
            {
                ListItem item = ddlSection.Items.FindByValue(selectSectionId.ToString());
                if (item != null) { ddlSection.ClearSelection(); item.Selected = true; }
            }
        }

        protected void ddlClass_SelectedIndexChanged(object sender, EventArgs e) { ReloadSections(); }
        protected void ddlShift_Changed(object sender, EventArgs e) { ReloadSections(); }

        private void ReloadSections()
        {
            int classId;
            int.TryParse(ddlClass.SelectedValue, out classId);
            LoadSections(classId, ddlShift.SelectedValue);
        }

        private void LoadGuardians(int selectGuardianId)
        {
            string query = "SELECT GuardianID, FullName, Relationship, Phone FROM Guardians WHERE IsActive = 1 ORDER BY FullName";
            DataTable dt = ExecuteQuery(query);

            if (dt.Rows.Count == 0)
            {
                pnlGuardianField.Visible = false;
                pnlNoGuardians.Visible = true;
                return;
            }

            pnlGuardianField.Visible = true;
            pnlNoGuardians.Visible = false;

            ddlGuardian.Items.Clear();
            ddlGuardian.Items.Add(new ListItem("Select Guardian (optional)", "0"));
            foreach (DataRow row in dt.Rows)
            {
                string label = string.Format("{0} — {1} — {2}", row["FullName"], row["Relationship"], row["Phone"]);
                ddlGuardian.Items.Add(new ListItem(label, row["GuardianID"].ToString()));
            }

            if (selectGuardianId > 0)
            {
                ListItem item = ddlGuardian.Items.FindByValue(selectGuardianId.ToString());
                if (item != null) { ddlGuardian.ClearSelection(); item.Selected = true; }
            }
        }

        #endregion

        #region Load Student

        private bool LoadStudent()
        {
            if (StudentId <= 0) return false;

            string query = @"
                SELECT s.*, sec.ClassID
                FROM Students s
                INNER JOIN Sections sec ON s.SectionID = sec.SectionID
                WHERE s.StudentID = @StudentID AND s.Status <> 'Deleted'";
            DataTable dt = ExecuteQuery(query, new[] { new SqlParameter("@StudentID", StudentId) });
            if (dt.Rows.Count == 0) return false;

            DataRow row = dt.Rows[0];

            lblStudentCode.Text = row["StudentCode"].ToString();
            lblAdmissionNo.Text = row["AdmissionNo"].ToString();
            txtFirstName.Text = row["FirstName"].ToString();
            txtLastName.Text = row["LastName"].ToString();
            ddlGender.SelectedValue = row["Gender"].ToString();
            ddlStatus.SelectedValue = row["Status"].ToString();
            string shiftVal = row.Table.Columns.Contains("Shift") && row["Shift"] != DBNull.Value ? row["Shift"].ToString() : "";
            ListItem shiftItem = ddlShift.Items.FindByValue(shiftVal);
            if (shiftItem != null) { ddlShift.ClearSelection(); shiftItem.Selected = true; }
            txtDateOfBirth.Text = Convert.ToDateTime(row["DateOfBirth"]).ToString("yyyy-MM-dd");
            txtEnrollmentDate.Text = Convert.ToDateTime(row["EnrollmentDate"]).ToString("yyyy-MM-dd");
            txtAddress.Text = row["Address"] == DBNull.Value ? "" : row["Address"].ToString();
            txtMedicalNotes.Text = row["MedicalNotes"] == DBNull.Value ? "" : row["MedicalNotes"].ToString();

            int academicYearId = row["AcademicYearID"] == DBNull.Value ? 0 : Convert.ToInt32(row["AcademicYearID"]);
            ListItem yearItem = ddlAcademicYear.Items.FindByValue(academicYearId.ToString());
            if (yearItem != null) { ddlAcademicYear.ClearSelection(); yearItem.Selected = true; }

            int classId = Convert.ToInt32(row["ClassID"]);
            ListItem classItem = ddlClass.Items.FindByValue(classId.ToString());
            if (classItem != null) { ddlClass.ClearSelection(); classItem.Selected = true; }
            int curSectionId = Convert.ToInt32(row["SectionID"]);
            LoadSections(classId, shiftVal, curSectionId, keepSelected: true);

            // Capture the authoritative placement baseline for change-detection + concurrency.
            OrigYearId = academicYearId;
            OrigClassId = classId;
            OrigSectionId = curSectionId;
            OrigShift = shiftVal ?? "";
            OrigEnrollmentDate = Convert.ToDateTime(row["EnrollmentDate"]).Date;

            // Warn when the student's current section has no assigned shift (mixed/unassigned):
            // do not auto-change Student.Shift; a new conflicting placement is blocked on save.
            object curSecShift = ExecuteScalar("SELECT Shift FROM Sections WHERE SectionID=@s", new[] { new SqlParameter("@s", curSectionId) });
            pnlShiftWarn.Visible = (curSecShift == null || curSecShift == DBNull.Value);

            int guardianId = row["GuardianID"] == DBNull.Value ? 0 : Convert.ToInt32(row["GuardianID"]);
            LoadGuardians(guardianId);

            CurrentPhotoPath = row["PhotoPath"] == DBNull.Value ? null : row["PhotoPath"].ToString();
            if (!string.IsNullOrEmpty(CurrentPhotoPath))
            {
                imgCurrentPhoto.ImageUrl = ResolveUrl("~/" + CurrentPhotoPath);
                imgCurrentPhoto.Visible = true;
                pnlCurrentPhotoFallback.Visible = false;
            }
            else
            {
                imgCurrentPhoto.Visible = false;
                pnlCurrentPhotoFallback.Visible = true;
            }

            return true;
        }

        #endregion

        #region Validation

        protected void cvDateOfBirth_ServerValidate(object source, ServerValidateEventArgs args)
        {
            DateTime dob;
            if (!DateTime.TryParse(txtDateOfBirth.Text, out dob)) { args.IsValid = false; return; }
            if (dob.Date > DateTime.Now.Date) { args.IsValid = false; return; }
            int age = DateTime.Now.Year - dob.Year;
            if (DateTime.Now.DayOfYear < dob.DayOfYear) age--;
            args.IsValid = age >= 3 && age <= 25;
        }

        protected void cvPhoto_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (!fuPhoto.HasFile) { args.IsValid = true; return; }
            if (fuPhoto.PostedFile.ContentLength > 2 * 1024 * 1024) { args.IsValid = false; return; }

            string ext = Path.GetExtension(fuPhoto.FileName).ToLowerInvariant();
            string[] allowedExt = { ".jpg", ".jpeg", ".png", ".webp" };
            bool extOk = false;
            foreach (string a in allowedExt) if (ext == a) { extOk = true; break; }
            if (!extOk) { args.IsValid = false; return; }

            string mime = fuPhoto.PostedFile.ContentType;
            string[] allowedMime = { "image/jpeg", "image/png", "image/webp" };
            bool mimeOk = false;
            foreach (string m in allowedMime) if (string.Equals(mime, m, StringComparison.OrdinalIgnoreCase)) { mimeOk = true; break; }
            args.IsValid = mimeOk;
        }

        private bool ValidateStudent(out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
            { errorMessage = "First and last name are required."; return false; }

            DateTime dob, enrollDate;
            if (!DateTime.TryParse(txtDateOfBirth.Text, out dob) || dob.Date > DateTime.Now.Date)
            { errorMessage = "Please provide a valid date of birth."; return false; }

            if (!DateTime.TryParse(txtEnrollmentDate.Text, out enrollDate))
            { errorMessage = "Please provide a valid enrollment date."; return false; }

            int academicYearId, classId, sectionId;
            if (!int.TryParse(ddlAcademicYear.SelectedValue, out academicYearId) || academicYearId <= 0)
            { errorMessage = "Please select an academic year."; return false; }

            if (!int.TryParse(ddlClass.SelectedValue, out classId) || classId <= 0)
            { errorMessage = "Please select a class."; return false; }

            if (!int.TryParse(ddlSection.SelectedValue, out sectionId) || sectionId <= 0)
            { errorMessage = "Please select a section."; return false; }

            object sectionClassCheck = ExecuteScalar(
                "SELECT COUNT(1) FROM Sections WHERE SectionID = @SectionID AND ClassID = @ClassID",
                new[] { new SqlParameter("@SectionID", sectionId), new SqlParameter("@ClassID", classId) });
            if (Convert.ToInt32(sectionClassCheck) == 0)
            { errorMessage = "The selected section does not belong to the selected class."; return false; }

            int guardianId;
            if (int.TryParse(ddlGuardian.SelectedValue, out guardianId) && guardianId > 0)
            {
                object guardianCheck = ExecuteScalar("SELECT COUNT(1) FROM Guardians WHERE GuardianID = @Id",
                    new[] { new SqlParameter("@Id", guardianId) });
                if (Convert.ToInt32(guardianCheck) == 0)
                { errorMessage = "The selected guardian is invalid."; return false; }
            }

            if (txtAddress.Text.Length > 200)
            { errorMessage = "Address must be 200 characters or fewer."; return false; }

            if (txtMedicalNotes.Text.Length > 500)
            { errorMessage = "Medical notes must be 500 characters or fewer."; return false; }

            return true;
        }

        #endregion

        #region Photo

        private string SaveUploadedPhoto()
        {
            if (!fuPhoto.HasFile) return null;
            string ext = Path.GetExtension(fuPhoto.FileName).ToLowerInvariant();
            string safeFileName = Guid.NewGuid().ToString("N") + ext;

            string physicalFolder = Server.MapPath("~/assets/uploads/students/");
            if (!Directory.Exists(physicalFolder))
                Directory.CreateDirectory(physicalFolder);

            string physicalPath = Path.Combine(physicalFolder, safeFileName);
            fuPhoto.SaveAs(physicalPath);
            return "assets/uploads/students/" + safeFileName;
        }

        #endregion

        #region Save

        private int? SelectedGuardianId()
        {
            int g;
            return int.TryParse(ddlGuardian.SelectedValue, out g) && g > 0 ? g : (int?)null;
        }

        private bool PlacementChanged()
        {
            int newSection, newYear, newClass;
            int.TryParse(ddlSection.SelectedValue, out newSection);
            int.TryParse(ddlAcademicYear.SelectedValue, out newYear);
            int.TryParse(ddlClass.SelectedValue, out newClass);
            string newShift = ddlShift.SelectedValue ?? "";
            return newSection != OrigSectionId || newYear != OrigYearId || newClass != OrigClassId
                || !string.Equals(newShift, OrigShift ?? "", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Step 1: validate; save demographics immediately, or open the placement
        /// confirmation modal when Academic Year / Class / Shift / Section changed.</summary>
        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!CanEditStudent()) { ShowError("You do not have permission to edit students."); return; }
            if (!Page.IsValid) return;

            string validationError;
            if (!ValidateStudent(out validationError)) { ShowError(validationError); return; }

            string newPhotoPath = CurrentPhotoPath;
            try
            {
                string uploaded = SaveUploadedPhoto();
                if (!string.IsNullOrEmpty(uploaded)) newPhotoPath = uploaded;
            }
            catch (Exception) { ShowError("The photo could not be saved. Please try a different file."); return; }

            if (!PlacementChanged())
            {
                // Demographic-only save: never touches placement, never writes history.
                string demoError;
                if (!SaveDemographicsOnly(newPhotoPath, SelectedGuardianId(), out demoError)) { ShowError(demoError); return; }
                CurrentPhotoPath = newPhotoPath;
                Response.Redirect("~/Modules/Students/StudentDetails.aspx?id=" + StudentId, true);
                return;
            }

            // Placement changed → do NOT save yet; open the confirmation modal.
            PendingPhotoPath = newPhotoPath;
            ConfirmToken = Guid.NewGuid().ToString("N");
            BuildPlacementSummary();
            if (string.IsNullOrEmpty(txtEffectiveDate.Text)) txtEffectiveDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
            pnlPlacementConfirm.Visible = true;
        }

        private void BuildPlacementSummary()
        {
            hfConfirmToken.Value = ConfirmToken ?? "";
            lblPcCode.Text = Server.HtmlEncode(lblStudentCode.Text);
            lblPcName.Text = Server.HtmlEncode((txtFirstName.Text.Trim() + " " + txtLastName.Text.Trim()).Trim());

            lblPcCurYear.Text = Server.HtmlEncode(YearName(OrigYearId));
            lblPcCurClass.Text = Server.HtmlEncode(ClassName(OrigClassId));
            lblPcCurShift.Text = Server.HtmlEncode(string.IsNullOrEmpty(OrigShift) ? "—" : OrigShift);
            lblPcCurSection.Text = Server.HtmlEncode(SectionName(OrigSectionId));

            int newYear, newClass, newSection;
            int.TryParse(ddlAcademicYear.SelectedValue, out newYear);
            int.TryParse(ddlClass.SelectedValue, out newClass);
            int.TryParse(ddlSection.SelectedValue, out newSection);
            lblPcNewYear.Text = Server.HtmlEncode(YearName(newYear));
            lblPcNewClass.Text = Server.HtmlEncode(ClassName(newClass));
            lblPcNewShift.Text = Server.HtmlEncode(string.IsNullOrEmpty(ddlShift.SelectedValue) ? "—" : ddlShift.SelectedValue);
            lblPcNewSection.Text = Server.HtmlEncode(SectionName(newSection));
        }

        private string YearName(int id) { object o = ExecuteScalar("SELECT YearName FROM AcademicYears WHERE AcademicYearID=@id", new[] { new SqlParameter("@id", id) }); return o == null || o == DBNull.Value ? "—" : Convert.ToString(o); }
        private string ClassName(int id) { object o = ExecuteScalar("SELECT ClassName FROM Classes WHERE ClassID=@id", new[] { new SqlParameter("@id", id) }); return o == null || o == DBNull.Value ? "—" : Convert.ToString(o); }
        private string SectionName(int id) { object o = ExecuteScalar("SELECT SectionName FROM Sections WHERE SectionID=@id", new[] { new SqlParameter("@id", id) }); return o == null || o == DBNull.Value ? "—" : Convert.ToString(o); }

        /// <summary>Step 2: user confirmed the placement change.</summary>
        protected void btnConfirmPlacement_Click(object sender, EventArgs e)
        {
            if (!CanEditStudent()) { Response.StatusCode = 403; Response.Redirect("~/Modules/Dashboard/Dashboard.aspx?denied=students", true); return; }

            // Double-submit / replay guard: a valid confirmation token must exist; consume it.
            if (string.IsNullOrEmpty(ConfirmToken) || !string.Equals(ConfirmToken, hfConfirmToken.Value, StringComparison.Ordinal))
            {
                pnlPlacementConfirm.Visible = false;
                ShowError("This confirmation is no longer valid. Please review the changes and try again.");
                return;
            }

            // Reason (required, trimmed, safe length, no HTML stored).
            string reason = (ddlReason.SelectedValue == "Other")
                ? (txtReasonOther.Text ?? "").Trim()
                : (ddlReason.SelectedValue ?? "").Trim();
            if (string.IsNullOrWhiteSpace(reason)) { BuildPlacementSummary(); pnlPlacementConfirm.Visible = true; ShowConfirmError("A placement change reason is required."); return; }
            if (reason.Length > 300) reason = reason.Substring(0, 300);

            // Effective date (required, valid, not before enrollment).
            DateTime effective;
            if (!DateTime.TryParse(txtEffectiveDate.Text, out effective)) { BuildPlacementSummary(); pnlPlacementConfirm.Visible = true; ShowConfirmError("Please provide a valid effective date."); return; }
            if (effective.Date < OrigEnrollmentDate.Date) { BuildPlacementSummary(); pnlPlacementConfirm.Visible = true; ShowConfirmError("The effective date cannot be before the student's enrollment date (" + OrigEnrollmentDate.ToString("yyyy-MM-dd") + ")."); return; }

            int newSectionId = int.Parse(ddlSection.SelectedValue);
            int newYearId = int.Parse(ddlAcademicYear.SelectedValue);
            string newShift = string.IsNullOrEmpty(ddlShift.SelectedValue) ? null : ddlShift.SelectedValue;
            string photo = PendingPhotoPath ?? CurrentPhotoPath;

            string saveError;
            if (!SaveStudentWithPlacementHistory(photo, SelectedGuardianId(), newSectionId, newYearId, newShift, reason, effective, out saveError))
            {
                BuildPlacementSummary(); pnlPlacementConfirm.Visible = true;
                ShowConfirmError(saveError);
                return;
            }

            ConfirmToken = null;              // consume token → replay/double-click writes nothing more
            CurrentPhotoPath = photo;
            Response.Redirect("~/Modules/Students/StudentDetails.aspx?id=" + StudentId, true);
        }

        protected void btnCancelPlacement_Click(object sender, EventArgs e)
        {
            // No database changes. Discard a photo that was uploaded only for this pending change.
            if (!string.IsNullOrEmpty(PendingPhotoPath) && PendingPhotoPath != CurrentPhotoPath)
                DeletePhotoIfExists(PendingPhotoPath);
            PendingPhotoPath = null;
            ConfirmToken = null;
            pnlPlacementConfirm.Visible = false;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/Students/StudentDetails.aspx?id=" + StudentId, true);
        }

        /// <summary>
        /// Saves the student in ONE transaction. When Section or Academic Year changes,
        /// a StudentPromotions history row is written (old + new placement preserved) and
        /// the destination Section's capacity and Shift are enforced BEFORE the Students
        /// row is mutated. Any failure rolls back the whole operation, so placement history
        /// can never be silently destroyed and old Attendance/Exam/Finance rows are untouched.
        /// </summary>
        private bool SaveStudentWithPlacementHistory(string photoPath, int? guardianId, int newSectionId, int newYearId, string newShift, string reason, DateTime effectiveDate, out string error)
        {
            error = null;
            int actorId;
            int.TryParse(Convert.ToString(Session["UserID"]), out actorId);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        int curSection, curYear;
                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT SectionID, AcademicYearID FROM Students WITH (UPDLOCK, HOLDLOCK) WHERE StudentID=@id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", StudentId);
                            using (SqlDataReader r = cmd.ExecuteReader())
                            {
                                if (!r.Read()) { tx.Rollback(); error = "Student not found."; return false; }
                                curSection = Convert.ToInt32(r["SectionID"]);
                                curYear = Convert.ToInt32(r["AcademicYearID"]);
                            }
                        }

                        // Concurrency: the live placement must still equal the baseline the user confirmed
                        // against. If another user moved the student meanwhile, stop — never overwrite silently.
                        if (curSection != OrigSectionId || curYear != OrigYearId)
                        {
                            tx.Rollback();
                            error = "The student's placement was changed by another user. Reload the latest record before continuing.";
                            return false;
                        }

                        bool sectionChanged = newSectionId != curSection;
                        bool placementChanged = sectionChanged || newYearId != curYear;

                        // Shift compatibility. Checked against the destination section's shift.
                        // A NULL-shift (unassigned) section is only rejected when the student is
                        // being MOVED into it — leaving an unchanged NULL-shift placement is allowed
                        // so demographic-only edits are never blocked.
                        {
                            object secShiftObj;
                            using (SqlCommand cmd = new SqlCommand("SELECT Shift FROM Sections WHERE SectionID=@s", conn, tx))
                            { cmd.Parameters.AddWithValue("@s", newSectionId); secShiftObj = cmd.ExecuteScalar(); }
                            string secShift = (secShiftObj == null || secShiftObj == DBNull.Value) ? null : Convert.ToString(secShiftObj);

                            if (sectionChanged && string.IsNullOrEmpty(secShift))
                            {
                                tx.Rollback();
                                error = "The selected section has no assigned shift. Assign the section's shift first (Classes & Sections) before moving the student there.";
                                return false;
                            }
                            if (!string.IsNullOrEmpty(newShift) && !string.IsNullOrEmpty(secShift) && !string.Equals(secShift, newShift, StringComparison.OrdinalIgnoreCase))
                            {
                                tx.Rollback();
                                error = "The selected section belongs to the " + secShift + " shift. Choose a matching section or shift.";
                                return false;
                            }
                        }

                        // Capacity: only when moving INTO a different section (Stage 5),
                        // under UPDLOCK to prevent two edits racing into the last seat.
                        if (sectionChanged)
                        {
                            int capacity;
                            using (SqlCommand cmd = new SqlCommand(
                                "SELECT ISNULL(Capacity,0) FROM Sections WITH (UPDLOCK, HOLDLOCK) WHERE SectionID=@s", conn, tx))
                            { cmd.Parameters.AddWithValue("@s", newSectionId); object c = cmd.ExecuteScalar(); capacity = (c == null || c == DBNull.Value) ? 0 : Convert.ToInt32(c); }

                            int active;
                            using (SqlCommand cmd = new SqlCommand(
                                "SELECT COUNT(*) FROM Students WHERE SectionID=@s AND Status='Active' AND StudentID<>@id", conn, tx))
                            { cmd.Parameters.AddWithValue("@s", newSectionId); cmd.Parameters.AddWithValue("@id", StudentId); active = Convert.ToInt32(cmd.ExecuteScalar()); }

                            if (capacity > 0 && active >= capacity)
                            {
                                tx.Rollback();
                                error = "The selected section is full (" + active + "/" + capacity + " active students). Choose another section.";
                                return false;
                            }
                        }

                        // Placement history is written BEFORE the current placement is changed.
                        if (placementChanged)
                        {
                            using (SqlCommand cmd = new SqlCommand(@"
                                INSERT INTO StudentPromotions
                                    (StudentID, FromAcademicYearID, ToAcademicYearID, FromSectionID, ToSectionID, Status, ActionDate, PromotedBy, Notes, CreatedAt)
                                VALUES
                                    (@sid, @fromYear, @toYear, @fromSec, @toSec, 'Completed', @actionDate, @by, @notes, GETDATE())", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@sid", StudentId);
                                cmd.Parameters.AddWithValue("@fromYear", curYear);
                                cmd.Parameters.AddWithValue("@toYear", newYearId);
                                cmd.Parameters.AddWithValue("@fromSec", curSection);
                                cmd.Parameters.AddWithValue("@toSec", newSectionId);
                                cmd.Parameters.AddWithValue("@actionDate", effectiveDate.Date);
                                cmd.Parameters.AddWithValue("@by", actorId > 0 ? (object)actorId : DBNull.Value);
                                cmd.Parameters.AddWithValue("@notes", string.IsNullOrWhiteSpace(reason) ? (object)DBNull.Value : reason.Trim());
                                cmd.ExecuteNonQuery();
                            }
                        }

                        using (SqlCommand cmd = new SqlCommand(@"
                            UPDATE Students SET
                                FirstName=@FirstName, LastName=@LastName, Gender=@Gender, Status=@Status,
                                DateOfBirth=@DateOfBirth, EnrollmentDate=@EnrollmentDate,
                                AcademicYearID=@AcademicYearID, SectionID=@SectionID, GuardianID=@GuardianID,
                                Address=@Address, MedicalNotes=@MedicalNotes, PhotoPath=@PhotoPath,
                                Shift=@Shift, UpdatedAt=GETDATE()
                            WHERE StudentID=@StudentID", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@FirstName", txtFirstName.Text.Trim());
                            cmd.Parameters.AddWithValue("@LastName", txtLastName.Text.Trim());
                            cmd.Parameters.AddWithValue("@Gender", ddlGender.SelectedValue);
                            cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);
                            cmd.Parameters.AddWithValue("@DateOfBirth", DateTime.Parse(txtDateOfBirth.Text));
                            cmd.Parameters.AddWithValue("@EnrollmentDate", DateTime.Parse(txtEnrollmentDate.Text));
                            cmd.Parameters.AddWithValue("@AcademicYearID", newYearId);
                            cmd.Parameters.AddWithValue("@SectionID", newSectionId);
                            cmd.Parameters.AddWithValue("@GuardianID", (object)guardianId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(txtAddress.Text.Trim()) ? (object)DBNull.Value : txtAddress.Text.Trim());
                            cmd.Parameters.AddWithValue("@MedicalNotes", string.IsNullOrEmpty(txtMedicalNotes.Text.Trim()) ? (object)DBNull.Value : txtMedicalNotes.Text.Trim());
                            cmd.Parameters.AddWithValue("@PhotoPath", (object)photoPath ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Shift", string.IsNullOrEmpty(newShift) ? (object)DBNull.Value : newShift);
                            cmd.Parameters.AddWithValue("@StudentID", StudentId);
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return true;
                    }
                    catch (SqlException sx) when (sx.Number == 2601 || sx.Number == 2627)
                    {
                        // Defensive: no unique/business-key index constrains placement history anymore
                        // (multiple same-year changes are allowed). This only fires if some other unique
                        // constraint is ever added; surface it safely rather than as a raw error.
                        try { tx.Rollback(); } catch { }
                        error = "This placement change appears to duplicate an existing record. Please review and try again.";
                        return false;
                    }
                    catch (Exception)
                    {
                        try { tx.Rollback(); } catch { }
                        error = "The student could not be updated due to a system error. Please try again.";
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Updates ONLY demographic fields — never AcademicYearID / SectionID / Shift — so a
        /// demographic-only edit can never rewrite placement (and can't revert a concurrent move).
        /// No StudentPromotions row is ever written here.
        /// </summary>
        private bool SaveDemographicsOnly(string photoPath, int? guardianId, out string error)
        {
            error = null;
            try
            {
                ExecuteNonQuery(@"
                    UPDATE Students SET
                        FirstName=@FirstName, LastName=@LastName, Gender=@Gender, Status=@Status,
                        DateOfBirth=@DateOfBirth, EnrollmentDate=@EnrollmentDate, GuardianID=@GuardianID,
                        Address=@Address, MedicalNotes=@MedicalNotes, PhotoPath=@PhotoPath, UpdatedAt=GETDATE()
                    WHERE StudentID=@StudentID",
                    new[]
                    {
                        new SqlParameter("@FirstName", txtFirstName.Text.Trim()),
                        new SqlParameter("@LastName", txtLastName.Text.Trim()),
                        new SqlParameter("@Gender", ddlGender.SelectedValue),
                        new SqlParameter("@Status", ddlStatus.SelectedValue),
                        new SqlParameter("@DateOfBirth", DateTime.Parse(txtDateOfBirth.Text)),
                        new SqlParameter("@EnrollmentDate", DateTime.Parse(txtEnrollmentDate.Text)),
                        new SqlParameter("@GuardianID", (object)guardianId ?? DBNull.Value),
                        new SqlParameter("@Address", string.IsNullOrEmpty(txtAddress.Text.Trim()) ? (object)DBNull.Value : txtAddress.Text.Trim()),
                        new SqlParameter("@MedicalNotes", string.IsNullOrEmpty(txtMedicalNotes.Text.Trim()) ? (object)DBNull.Value : txtMedicalNotes.Text.Trim()),
                        new SqlParameter("@PhotoPath", (object)photoPath ?? DBNull.Value),
                        new SqlParameter("@StudentID", StudentId)
                    });
                return true;
            }
            catch (Exception)
            {
                error = "The student could not be updated due to a system error. Please try again.";
                return false;
            }
        }

        private void DeletePhotoIfExists(string relativePath)
        {
            try
            {
                if (string.IsNullOrEmpty(relativePath)) return;
                string physical = Server.MapPath("~/" + relativePath.TrimStart('/', '~'));
                if (System.IO.File.Exists(physical)) System.IO.File.Delete(physical);
            }
            catch { /* non-critical cleanup */ }
        }

        #endregion

        private void ShowConfirmError(string message)
        {
            lblPcError.Text = Server.HtmlEncode(message);
            pnlPcError.Visible = true;
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
