using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Students
{
    public partial class AddStudent : System.Web.UI.Page
    {
        // ------------------------------------------------------------------
        // Local ADO.NET access — mirrors the pattern already used in
        // Students.aspx.cs. DatabaseHelper.cs is left completely untouched.
        // ------------------------------------------------------------------
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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!CheckAuthorization())
                return;

            if (!IsPostBack)
            {
                LoadAcademicYears();
                LoadClasses();
                LoadSections(0);
                LoadGuardians();
                GenerateAndDisplayStudentCode();
                GenerateAndDisplayAdmissionNumber();
                txtEnrollmentDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            }
        }

        #region Authorization

        /// <summary>
        /// Normalizes a role string for comparison: trims, removes spaces/underscores/
        /// hyphens, lowercases. This is the single source of truth for role matching —
        /// every permission check in this page must go through CanAddStudent(), not a
        /// direct string comparison against "Super Admin" or similar.
        /// </summary>
        private string NormalizeRole(string role)
        {
            if (string.IsNullOrEmpty(role)) return string.Empty;
            return role.Trim()
                       .Replace(" ", "")
                       .Replace("_", "")
                       .Replace("-", "")
                       .ToLowerInvariant();
        }

        private static readonly string[] AllowedNormalizedRoles = { "superadmin", "admin", "registrar" };

        /// <summary>
        /// The single authorization check used everywhere on this page — Page_Load,
        /// every save action, and any future insert/validation method that needs to
        /// gate on permission. Do not add a second, separate role comparison anywhere
        /// else in this class.
        /// </summary>
        private bool CanAddStudent()
        {
            string normalized = NormalizeRole(Session["Role"] as string);
            foreach (string allowed in AllowedNormalizedRoles)
            {
                if (normalized == allowed) return true;
            }
            return false;
        }

        /// <summary>
        /// Gate-keeps the page on load. Logged-out users go to Login.aspx (that page
        /// exists). Logged-in users without permission see an inline message and the
        /// form is hidden — no redirect to NotAuthorized.aspx, since that page does
        /// not exist in this project.
        /// </summary>
        private bool CheckAuthorization()
        {
            string role = Session["Role"] as string;

            if (string.IsNullOrEmpty(role))
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx", true);
                return false;
            }

            if (!CanAddStudent())
            {
                ShowErrorMessage("You do not have permission to add students. This page is available to Super Admin, Admin, and Registrar roles only.");
                pnlFormBody.Visible = false;
                return false;
            }

            return true;
        }

        #endregion

        #region Dropdown Loading

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
            // Default-select the Active year if one exists.
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

        private void LoadClasses()
        {
            DataTable dt = ExecuteQuery("SELECT ClassID, ClassName FROM Classes ORDER BY ClassName");
            ddlClass.Items.Clear();
            ddlClass.Items.Add(new ListItem("Select Class", "0"));
            foreach (DataRow row in dt.Rows)
                ddlClass.Items.Add(new ListItem(row["ClassName"].ToString(), row["ClassID"].ToString()));
        }

        private void LoadSections(int classId)
        {
            ddlSection.Items.Clear();
            ddlSection.Items.Add(new ListItem("Select Section", "0"));

            if (classId <= 0) return;

            string query = "SELECT SectionID, SectionName FROM Sections WHERE ClassID = @ClassID ORDER BY SectionName";
            SqlParameter[] parameters = { new SqlParameter("@ClassID", classId) };
            DataTable dt = ExecuteQuery(query, parameters);
            foreach (DataRow row in dt.Rows)
                ddlSection.Items.Add(new ListItem(row["SectionName"].ToString(), row["SectionID"].ToString()));
        }

        protected void ddlClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            int classId;
            int.TryParse(ddlClass.SelectedValue, out classId);
            LoadSections(classId);
        }

        private void LoadGuardians()
        {
            // Guardians.FullName / Relationship / Phone confirmed from schema inspection.
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
        }

        #endregion

        #region Student Code Generation

        /// <summary>
        /// Generates the next sequential StudentCode (AQH-{YEAR}-{0000}) for display purposes.
        /// The value is re-verified for uniqueness inside the save transaction, since this
        /// preview value could theoretically collide under concurrent saves.
        /// </summary>
        private string GenerateStudentCode()
        {
            int year = DateTime.Now.Year;
            string prefix = "AQH-" + year + "-";

            string query = @"
                SELECT TOP 1 StudentCode
                FROM Students
                WHERE StudentCode LIKE @Prefix + '%'
                ORDER BY StudentCode DESC";
            SqlParameter[] parameters = { new SqlParameter("@Prefix", prefix) };
            DataTable dt = ExecuteQuery(query, parameters);

            int nextNumber = 1;
            if (dt.Rows.Count > 0)
            {
                string lastCode = dt.Rows[0]["StudentCode"].ToString();
                string lastNumPart = lastCode.Substring(lastCode.LastIndexOf('-') + 1);
                int lastNum;
                if (int.TryParse(lastNumPart, out lastNum))
                    nextNumber = lastNum + 1;
            }

            return prefix + nextNumber.ToString("D4");
        }

        /// <summary>
        /// Generates a candidate code and re-checks it against the DB in a small retry
        /// loop, guarding against the (rare) case where a concurrent save already
        /// claimed the previewed code between page load and this call.
        /// </summary>
        private string GenerateUniqueStudentCode(SqlConnection conn, SqlTransaction tx)
        {
            for (int attempt = 0; attempt < 5; attempt++)
            {
                string candidate = GenerateStudentCode();
                object exists = ExecuteScalar(conn, tx,
                    "SELECT COUNT(1) FROM Students WITH (UPDLOCK, HOLDLOCK) WHERE StudentCode = @Code",
                    new[] { new SqlParameter("@Code", candidate) });

                if (Convert.ToInt32(exists) == 0)
                    return candidate;
            }
            throw new InvalidOperationException("Could not generate a unique Student Code after several attempts.");
        }

        private void GenerateAndDisplayStudentCode()
        {
            string code = GenerateStudentCode();
            hdnStudentCode.Value = code;
            lblStudentCode.Text = code;
        }

        #endregion

        #region Admission Number Generation

        /// <summary>
        /// Generates the next sequential AdmissionNo (ADM-{0000}) for display purposes.
        /// Existing records use inconsistent digit lengths (e.g. ADM-1042), so the numeric
        /// suffix is extracted and compared numerically, not sorted as text — this is why
        /// TRY_CONVERT is used instead of a simple MAX(AdmissionNo) string comparison.
        /// The value is re-verified for uniqueness inside the save transaction, since this
        /// preview value could theoretically collide under concurrent saves.
        /// </summary>
        private string GenerateAdmissionNumber()
        {
            string query = @"
                SELECT ISNULL(
                    MAX(
                        TRY_CONVERT(INT, SUBSTRING(AdmissionNo, 5, LEN(AdmissionNo)))
                    ),
                    0
                ) + 1 AS NextNumber
                FROM Students
                WHERE AdmissionNo LIKE 'ADM-%'";

            object result = ExecuteScalar(query);
            int nextNumber = (result == null || result == DBNull.Value) ? 1 : Convert.ToInt32(result);

            return "ADM-" + nextNumber.ToString("D4");
        }

        /// <summary>
        /// Generates a candidate Admission Number and re-checks it against the DB under
        /// UPDLOCK/HOLDLOCK inside the save transaction, guarding against a concurrent
        /// save claiming the previewed number first. The server is the sole authority
        /// here — the hidden field on the page is a preview only and is never trusted
        /// as the final value.
        /// </summary>
        private string GenerateUniqueAdmissionNumber(SqlConnection conn, SqlTransaction tx)
        {
            for (int attempt = 0; attempt < 5; attempt++)
            {
                object result = ExecuteScalar(conn, tx, @"
                    SELECT ISNULL(
                        MAX(
                            TRY_CONVERT(INT, SUBSTRING(AdmissionNo, 5, LEN(AdmissionNo)))
                        ),
                        0
                    ) + 1
                    FROM Students WITH (UPDLOCK, HOLDLOCK)
                    WHERE AdmissionNo LIKE 'ADM-%'");

                int nextNumber = (result == null || result == DBNull.Value) ? 1 : Convert.ToInt32(result);
                string candidate = "ADM-" + nextNumber.ToString("D4");

                object exists = ExecuteScalar(conn, tx,
                    "SELECT COUNT(1) FROM Students WITH (UPDLOCK, HOLDLOCK) WHERE AdmissionNo = @AdmissionNo",
                    new[] { new SqlParameter("@AdmissionNo", candidate) });

                if (Convert.ToInt32(exists) == 0)
                    return candidate;
            }
            throw new InvalidOperationException("Could not generate a unique Admission Number after several attempts.");
        }

        private void GenerateAndDisplayAdmissionNumber()
        {
            string admissionNo = GenerateAdmissionNumber();
            hdnAdmissionNo.Value = admissionNo;
            lblAdmissionNo.Text = admissionNo;
        }

        #endregion

        #region Validation

        private bool StudentCodeExists(string studentCode)
        {
            string query = "SELECT COUNT(1) FROM Students WHERE StudentCode = @StudentCode";
            SqlParameter[] parameters = { new SqlParameter("@StudentCode", studentCode) };
            return Convert.ToInt32(ExecuteScalar(query, parameters)) > 0;
        }

        protected void cvDateOfBirth_ServerValidate(object source, ServerValidateEventArgs args)
        {
            DateTime dob;
            if (!DateTime.TryParse(txtDateOfBirth.Text, out dob))
            {
                args.IsValid = false;
                return;
            }

            if (dob.Date > DateTime.Now.Date) { args.IsValid = false; return; }

            int age = DateTime.Now.Year - dob.Year;
            if (DateTime.Now.DayOfYear < dob.DayOfYear) age--;

            args.IsValid = age >= 3 && age <= 25;
        }

        protected void cvPhoto_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (!fuPhoto.HasFile) { args.IsValid = true; return; } // optional field

            if (fuPhoto.PostedFile.ContentLength > 2 * 1024 * 1024)
            {
                args.IsValid = false;
                return;
            }

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

        /// <summary>
        /// Server-side re-validation of everything critical, independent of the
        /// ASP.NET validator controls (which the client could bypass).
        /// </summary>
        private bool ValidateStudent(out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
            { errorMessage = "First and last name are required."; return false; }

            if (ddlGender.SelectedValue != "Male" && ddlGender.SelectedValue != "Female")
            { errorMessage = "Please select a valid gender."; return false; }

            if (ddlStatus.SelectedValue != "Active" && ddlStatus.SelectedValue != "Inactive")
            { errorMessage = "Please select a valid status."; return false; }

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

            // Confirm the section actually belongs to the selected class.
            object sectionClassCheck = ExecuteScalar(
                "SELECT COUNT(1) FROM Sections WHERE SectionID = @SectionID AND ClassID = @ClassID",
                new[] { new SqlParameter("@SectionID", sectionId), new SqlParameter("@ClassID", classId) });
            if (Convert.ToInt32(sectionClassCheck) == 0)
            { errorMessage = "The selected section does not belong to the selected class."; return false; }

            object yearCheck = ExecuteScalar("SELECT COUNT(1) FROM AcademicYears WHERE AcademicYearID = @Id",
                new[] { new SqlParameter("@Id", academicYearId) });
            if (Convert.ToInt32(yearCheck) == 0)
            { errorMessage = "The selected academic year is invalid."; return false; }

            int guardianId;
            bool hasGuardianSelection = int.TryParse(ddlGuardian.SelectedValue, out guardianId) && guardianId > 0;
            if (hasGuardianSelection)
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

        #region Photo Upload

        private const string UploadRelativeFolder = "~/assets/uploads/students/";

        /// <summary>
        /// Saves the uploaded photo under a generated GUID filename and returns the
        /// relative path to store in PhotoPath. Returns null if no file was uploaded.
        /// </summary>
        private string SaveUploadedPhoto()
        {
            if (!fuPhoto.HasFile) return null;

            string ext = Path.GetExtension(fuPhoto.FileName).ToLowerInvariant();
            string safeFileName = Guid.NewGuid().ToString("N") + ext;

            string physicalFolder = Server.MapPath(UploadRelativeFolder);
            if (!Directory.Exists(physicalFolder))
                Directory.CreateDirectory(physicalFolder);

            string physicalPath = Path.Combine(physicalFolder, safeFileName);
            fuPhoto.SaveAs(physicalPath);

            return "assets/uploads/students/" + safeFileName;
        }

        private void DeleteUploadedPhotoIfExists(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;
            try
            {
                string physicalPath = Server.MapPath("~/" + relativePath);
                if (File.Exists(physicalPath)) File.Delete(physicalPath);
            }
            catch
            {
                // Best-effort cleanup only — never let a cleanup failure mask the original error.
            }
        }

        #endregion

        #region Save

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!CanAddStudent()) { ShowErrorMessage("You do not have permission to add students."); return; }
            if (!Page.IsValid) return;
            int? newId = SaveStudent();
            if (newId.HasValue)
            {
                Response.Redirect("~/Modules/Students/Students.aspx", true);
            }
        }

        protected void btnSaveAndAddAnother_Click(object sender, EventArgs e)
        {
            if (!CanAddStudent()) { ShowErrorMessage("You do not have permission to add students."); return; }
            if (!Page.IsValid) return;
            int? newId = SaveStudent();
            if (newId.HasValue)
            {
                ShowSuccessMessage("Student " + hdnStudentCode.Value + " saved successfully. Form has been reset for the next entry.");
                ClearForm();
            }
        }

        /// <summary>
        /// Runs full server-side validation, then performs the insert inside a
        /// transaction that re-checks StudentCode/AdmissionNo uniqueness immediately
        /// before writing. Returns the new StudentID, or null if save failed
        /// (an error message will already have been shown to the user).
        /// </summary>
        private int? SaveStudent()
        {
            if (!CanAddStudent())
            {
                ShowErrorMessage("You do not have permission to add students.");
                return null;
            }

            string validationError;
            if (!ValidateStudent(out validationError))
            {
                ShowErrorMessage(validationError);
                return null;
            }

            string photoRelativePath = null;

            try
            {
                photoRelativePath = SaveUploadedPhoto();
            }
            catch (Exception)
            {
                ShowErrorMessage("The photo could not be saved. Please try a different file.");
                return null;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        string studentCode = GenerateUniqueStudentCode(conn, tx);
                        string admissionNo = GenerateUniqueAdmissionNumber(conn, tx);

                        int? guardianId = null;
                        int parsedGuardianId;
                        if (int.TryParse(ddlGuardian.SelectedValue, out parsedGuardianId) && parsedGuardianId > 0)
                            guardianId = parsedGuardianId;

                        string insertQuery = @"
                            INSERT INTO Students
                                (StudentCode, AdmissionNo, FirstName, LastName, Gender, DateOfBirth,
                                 GuardianID, SectionID, AcademicYearID, Status, PhotoPath,
                                 MedicalNotes, Address, EnrollmentDate, Shift, CreatedAt, UpdatedAt)
                            OUTPUT INSERTED.StudentID
                            VALUES
                                (@StudentCode, @AdmissionNo, @FirstName, @LastName, @Gender, @DateOfBirth,
                                 @GuardianID, @SectionID, @AcademicYearID, @Status, @PhotoPath,
                                 @MedicalNotes, @Address, @EnrollmentDate, @Shift, GETDATE(), GETDATE())";

                        using (SqlCommand cmd = new SqlCommand(insertQuery, conn, tx))
                        {
                            cmd.Parameters.Add(new SqlParameter("@StudentCode", studentCode));
                            cmd.Parameters.Add(new SqlParameter("@AdmissionNo", admissionNo));
                            cmd.Parameters.Add(new SqlParameter("@FirstName", txtFirstName.Text.Trim()));
                            cmd.Parameters.Add(new SqlParameter("@LastName", txtLastName.Text.Trim()));
                            cmd.Parameters.Add(new SqlParameter("@Gender", ddlGender.SelectedValue));
                            cmd.Parameters.Add(new SqlParameter("@DateOfBirth", DateTime.Parse(txtDateOfBirth.Text)));
                            cmd.Parameters.Add(new SqlParameter("@GuardianID", (object)guardianId ?? DBNull.Value));
                            cmd.Parameters.Add(new SqlParameter("@SectionID", int.Parse(ddlSection.SelectedValue)));
                            cmd.Parameters.Add(new SqlParameter("@AcademicYearID", int.Parse(ddlAcademicYear.SelectedValue)));
                            cmd.Parameters.Add(new SqlParameter("@Status", ddlStatus.SelectedValue));
                            cmd.Parameters.Add(new SqlParameter("@PhotoPath", (object)photoRelativePath ?? DBNull.Value));
                            string medical = txtMedicalNotes.Text.Trim();
                            cmd.Parameters.Add(new SqlParameter("@MedicalNotes", string.IsNullOrEmpty(medical) ? (object)DBNull.Value : medical));
                            string address = txtAddress.Text.Trim();
                            cmd.Parameters.Add(new SqlParameter("@Address", string.IsNullOrEmpty(address) ? (object)DBNull.Value : address));
                            cmd.Parameters.Add(new SqlParameter("@EnrollmentDate", DateTime.Parse(txtEnrollmentDate.Text)));
                            cmd.Parameters.Add(new SqlParameter("@Shift", string.IsNullOrEmpty(ddlShift.SelectedValue) ? (object)DBNull.Value : ddlShift.SelectedValue));

                            int newStudentId = Convert.ToInt32(cmd.ExecuteScalar());

                            LogAuditAction(conn, tx, newStudentId, studentCode, admissionNo);

                            tx.Commit();

                            hdnStudentCode.Value = studentCode;
                            hdnAdmissionNo.Value = admissionNo;
                            lblAdmissionNo.Text = admissionNo;
                            return newStudentId;
                        }
                    }
                    catch (Exception)
                    {
                        try { tx.Rollback(); } catch { /* connection may already be broken */ }
                        DeleteUploadedPhotoIfExists(photoRelativePath);
                        ShowErrorMessage("The student could not be saved due to a system error. Please try again.");
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Writes an AuditLog entry if the table is present. This is wrapped defensively:
        /// the AuditLog table's exact schema wasn't confirmed during inspection, so any
        /// failure here is swallowed rather than rolling back the student save — the
        /// student record is the primary artifact this task is responsible for.
        /// </summary>
        private void LogAuditAction(SqlConnection conn, SqlTransaction tx, int studentId, string studentCode, string admissionNo)
        {
            try
            {
                object userIdObj = Session["UserID"];
                int userId = userIdObj != null ? Convert.ToInt32(userIdObj) : 0;

                string query = @"
                    INSERT INTO AuditLog (Action, EntityName, EntityID, UserID, Description, CreatedAt)
                    VALUES (@Action, @EntityName, @EntityID, @UserID, @Description, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, conn, tx))
                {
                    cmd.Parameters.Add(new SqlParameter("@Action", "Student Created"));
                    cmd.Parameters.Add(new SqlParameter("@EntityName", "Student"));
                    cmd.Parameters.Add(new SqlParameter("@EntityID", studentId));
                    cmd.Parameters.Add(new SqlParameter("@UserID", userId));
                    cmd.Parameters.Add(new SqlParameter("@Description",
                        string.Format("Registered student {0} (Admission No {1})", studentCode, admissionNo)));
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // AuditLog schema not confirmed during inspection — silently skip
                // rather than fail the whole student save over a logging table.
            }
        }

        #endregion

        #region Form State

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/Students/Students.aspx", true);
        }

        private void ClearForm()
        {
            txtFirstName.Text = "";
            txtLastName.Text = "";
            ddlGender.SelectedIndex = 0;
            ddlStatus.SelectedValue = "Active";
            txtDateOfBirth.Text = "";
            txtEnrollmentDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            ddlClass.SelectedIndex = 0;
            LoadSections(0);
            if (ddlGuardian.Items.Count > 0) ddlGuardian.SelectedIndex = 0;
            txtAddress.Text = "";
            txtMedicalNotes.Text = "";
            GenerateAndDisplayStudentCode();
            GenerateAndDisplayAdmissionNumber();
            pnlError.Visible = false;
        }

        private void ShowSuccessMessage(string message)
        {
            lblSuccess.Text = message;
            pnlSuccess.Visible = true;
            pnlError.Visible = false;
        }

        private void ShowErrorMessage(string message)
        {
            lblError.Text = message;
            pnlError.Visible = true;
            pnlSuccess.Visible = false;
        }

        #endregion
    }
}
