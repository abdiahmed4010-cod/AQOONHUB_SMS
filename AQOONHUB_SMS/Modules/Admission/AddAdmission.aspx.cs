using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Admission
{
    public partial class AddAdmission : System.Web.UI.Page
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
            if (!CanManageAdmissions())
            {
                ShowError("You do not have permission to add admission applications. This page is available to Super Admin, Admin, and Registrar roles only.");
                pnlFormBody.Visible = false;
                return false;
            }
            return true;
        }

        #endregion

        private int EditId
        {
            get
            {
                int id;
                return int.TryParse(Request.QueryString["id"], out id) ? id : 0;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!CheckAuthorization()) return;

            if (!IsPostBack)
            {
                LoadClasses();
                LoadAcademicYears();
                LoadExistingGuardians();

                if (EditId > 0)
                {
                    LoadForEdit(EditId);
                }
                else
                {
                    GenerateAndDisplayApplicationNo();
                }
            }
        }

        private void LoadAcademicYears()
        {
            ddlAcademicYear.Items.Clear();
            ddlAcademicYear.Items.Add(new ListItem("Select Year", "0"));
            try
            {
                DataTable dt = ExecuteQuery("SELECT AcademicYearID, YearName FROM AcademicYears ORDER BY AcademicYearID DESC");
                foreach (DataRow row in dt.Rows)
                    ddlAcademicYear.Items.Add(new ListItem(row["YearName"].ToString(), row["AcademicYearID"].ToString()));
            }
            catch { /* AcademicYears optional */ }
        }

        private void LoadForEdit(int admissionId)
        {
            DataTable dt = ExecuteQuery(
                "SELECT * FROM Admissions WHERE AdmissionID = @Id",
                new[] { new SqlParameter("@Id", admissionId) });

            if (dt.Rows.Count == 0)
            {
                ShowError("The application could not be found.");
                pnlFormBody.Visible = false;
                return;
            }

            DataRow r = dt.Rows[0];

            // Editable statuses: Pending, Under Review, Approved, Enrolled and Rejected.
            string status = r["Status"] == DBNull.Value ? "" : r["Status"].ToString();
            string[] editableStatuses = { "Pending", "Under Review", "Approved", "Enrolled", "Rejected" };
            if (Array.IndexOf(editableStatuses, status) < 0)
            {
                ShowError("This application cannot be edited.");
                pnlFormBody.Visible = false;
                return;
            }

            litPageTitle.Text = "Edit Admission Application";
            litPageSubtitle.Text = "Update the applicant details or change the application status below.";

            // Status can be changed from the edit form (e.g. Enrolled -> Rejected, Rejected -> Under Review).
            pnlStatus.Visible = true;
            ddlStatus.Items.Clear();
            ddlStatus.Items.Add(new ListItem("Pending", "Pending"));
            ddlStatus.Items.Add(new ListItem("Under Review", "Under Review"));
            ddlStatus.Items.Add(new ListItem("Enrolled", "Enrolled"));
            ddlStatus.Items.Add(new ListItem("Rejected", "Rejected"));
            if (!string.IsNullOrEmpty(status) && ddlStatus.Items.FindByValue(status) == null)
                ddlStatus.Items.Add(new ListItem(status, status));
            SelectIfPresent(ddlStatus, status);

            string appNo = r["ApplicationNo"].ToString();
            hdnApplicationNo.Value = appNo;
            lblApplicationNo.Text = appNo;

            txtFirstName.Text = r["FirstName"].ToString();
            txtLastName.Text = r["LastName"].ToString();
            ddlGender.SelectedValue = r["Gender"].ToString();
            if (r["DateOfBirth"] != DBNull.Value)
                txtDateOfBirth.Text = Convert.ToDateTime(r["DateOfBirth"]).ToString("yyyy-MM-dd");

            SelectIfPresent(ddlClass, r["ApplyingForClassID"].ToString());
            if (r["AcademicYearID"] != DBNull.Value)
                SelectIfPresent(ddlAcademicYear, r["AcademicYearID"].ToString());
            if (r["Shift"] != DBNull.Value)
                SelectIfPresent(ddlShift, r["Shift"].ToString());
            txtPreviousSchool.Text = r["PreviousSchool"] == DBNull.Value ? "" : r["PreviousSchool"].ToString();
            txtLastGradeCompleted.Text = r["LastGradeCompleted"] == DBNull.Value ? "" : r["LastGradeCompleted"].ToString();
            txtNotes.Text = r["Notes"] == DBNull.Value ? "" : r["Notes"].ToString();

            // In edit mode we keep the guardian already linked (select-existing mode).
            rblGuardianMode.SelectedValue = "Existing";
            pnlExistingGuardian.Visible = true;
            pnlNewGuardian.Visible = false;
            if (r["GuardianID"] != DBNull.Value)
                SelectIfPresent(ddlExistingGuardian, r["GuardianID"].ToString());
        }

        private void SelectIfPresent(DropDownList ddl, string value)
        {
            ListItem item = ddl.Items.FindByValue(value);
            if (item != null)
            {
                ddl.ClearSelection();
                item.Selected = true;
            }
        }

        private void LoadExistingGuardians()
        {
            DataTable dt = ExecuteQuery("SELECT GuardianID, FullName, Phone FROM Guardians WHERE IsActive = 1 ORDER BY FullName");
            ddlExistingGuardian.Items.Clear();
            ddlExistingGuardian.Items.Add(new ListItem("Select Guardian", "0"));
            foreach (DataRow row in dt.Rows)
                ddlExistingGuardian.Items.Add(new ListItem(row["FullName"] + " — " + row["Phone"], row["GuardianID"].ToString()));
        }

        protected void rblGuardianMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isNew = rblGuardianMode.SelectedValue == "New";
            pnlExistingGuardian.Visible = !isNew;
            pnlNewGuardian.Visible = isNew;
        }

        private void LoadClasses()
        {
            DataTable dt = ExecuteQuery("SELECT ClassID, ClassName FROM Classes ORDER BY ClassName");
            ddlClass.Items.Clear();
            ddlClass.Items.Add(new ListItem("Select Class", "0"));
            foreach (DataRow row in dt.Rows)
                ddlClass.Items.Add(new ListItem(row["ClassName"].ToString(), row["ClassID"].ToString()));
        }

        #region Application Number Generation (APP-{year}-{0000}, same numeric-safe pattern as AdmissionNo)

        private string GenerateApplicationNo()
        {
            int year = DateTime.Now.Year;
            string prefix = "APP-" + year + "-";

            string query = @"
                SELECT TOP 1 ApplicationNo FROM Admissions
                WHERE ApplicationNo LIKE @Prefix + '%'
                ORDER BY ApplicationNo DESC";
            DataTable dt = ExecuteQuery(query, new[] { new SqlParameter("@Prefix", prefix) });

            int nextNumber = 1;
            if (dt.Rows.Count > 0)
            {
                string last = dt.Rows[0]["ApplicationNo"].ToString();
                string numPart = last.Substring(last.LastIndexOf('-') + 1);
                int lastNum;
                if (int.TryParse(numPart, out lastNum)) nextNumber = lastNum + 1;
            }
            return prefix + nextNumber.ToString("D4");
        }

        private string GenerateUniqueApplicationNo(SqlConnection conn, SqlTransaction tx)
        {
            for (int attempt = 0; attempt < 5; attempt++)
            {
                string candidate = GenerateApplicationNo();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(1) FROM Admissions WITH (UPDLOCK, HOLDLOCK) WHERE ApplicationNo = @No", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@No", candidate);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0) return candidate;
                }
            }
            throw new InvalidOperationException("Could not generate a unique Application Number after several attempts.");
        }

        private void GenerateAndDisplayApplicationNo()
        {
            string appNo = GenerateApplicationNo();
            hdnApplicationNo.Value = appNo;
            lblApplicationNo.Text = appNo;
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

        private bool ValidateApplication(out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
            { errorMessage = "First and last name are required."; return false; }

            if (ddlGender.SelectedValue != "Male" && ddlGender.SelectedValue != "Female")
            { errorMessage = "Please select a valid gender."; return false; }

            DateTime dob;
            if (!DateTime.TryParse(txtDateOfBirth.Text, out dob) || dob.Date > DateTime.Now.Date)
            { errorMessage = "Please provide a valid date of birth."; return false; }

            int classId;
            if (!int.TryParse(ddlClass.SelectedValue, out classId) || classId <= 0)
            { errorMessage = "Please select a class."; return false; }

            if (rblGuardianMode.SelectedValue == "Existing")
            {
                int existingGuardianId;
                if (!int.TryParse(ddlExistingGuardian.SelectedValue, out existingGuardianId) || existingGuardianId <= 0)
                { errorMessage = "Please select a guardian."; return false; }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtGuardianName.Text))
                { errorMessage = "Guardian name is required."; return false; }
                if (string.IsNullOrEmpty(ddlGuardianRelationship.SelectedValue))
                { errorMessage = "Please select a guardian relationship."; return false; }
                if (string.IsNullOrWhiteSpace(txtGuardianPhone.Text))
                { errorMessage = "Guardian phone is required."; return false; }
            }

            if (txtNotes.Text.Length > 500)
            { errorMessage = "Notes must be 500 characters or fewer."; return false; }

            return true;
        }

        #endregion

        #region Save

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!CanManageAdmissions()) { ShowError("You do not have permission to add applications."); return; }
            if (!Page.IsValid) return;

            string validationError;
            if (!ValidateApplication(out validationError))
            {
                ShowError(validationError);
                return;
            }

            if (EditId > 0)
            {
                UpdateApplication(EditId);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        int guardianId;
                        string guardianName, guardianPhone, guardianEmail;

                        if (rblGuardianMode.SelectedValue == "Existing")
                        {
                            guardianId = int.Parse(ddlExistingGuardian.SelectedValue);

                            using (SqlCommand cmd = new SqlCommand("SELECT FullName, Phone, Email FROM Guardians WHERE GuardianID = @Id", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@Id", guardianId);
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (!reader.Read())
                                    {
                                        tx.Rollback();
                                        ShowError("The selected guardian could not be found.");
                                        return;
                                    }
                                    guardianName = reader["FullName"].ToString();
                                    guardianPhone = reader["Phone"].ToString();
                                    guardianEmail = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString();
                                }
                            }
                        }
                        else
                        {
                            // Duplicate check (warn-once, same pattern as AddParent.aspx): only
                            // blocks the first submit; resubmitting after seeing the warning proceeds.
                            if (!pnlGuardianDuplicateWarning.Visible)
                            {
                                DataTable dupCheck;
                                using (SqlCommand cmd = new SqlCommand(
                                    "SELECT FullName FROM Guardians WHERE Phone = @Phone OR (@Email <> '' AND Email = @Email)", conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@Phone", txtGuardianPhone.Text.Trim());
                                    cmd.Parameters.AddWithValue("@Email", txtGuardianEmail.Text.Trim());
                                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                                    {
                                        dupCheck = new DataTable();
                                        da.Fill(dupCheck);
                                    }
                                }
                                if (dupCheck.Rows.Count > 0)
                                {
                                    tx.Rollback();
                                    lblGuardianDuplicateWarning.Text = "A guardian with this phone or email may already exist: " + dupCheck.Rows[0]["FullName"] + ". Save again to continue anyway, or switch to \"Select Existing Guardian\".";
                                    pnlGuardianDuplicateWarning.Visible = true;
                                    return;
                                }
                            }

                            guardianName = txtGuardianName.Text.Trim();
                            guardianPhone = txtGuardianPhone.Text.Trim();
                            guardianEmail = string.IsNullOrEmpty(txtGuardianEmail.Text.Trim()) ? null : txtGuardianEmail.Text.Trim();

                            using (SqlCommand cmd = new SqlCommand(@"
                                INSERT INTO Guardians (FullName, Relationship, Phone, Email, IsActive, CreatedAt, UpdatedAt)
                                OUTPUT INSERTED.GuardianID
                                VALUES (@FullName, @Relationship, @Phone, @Email, 1, SYSDATETIME(), SYSDATETIME())", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@FullName", guardianName);
                                cmd.Parameters.AddWithValue("@Relationship", ddlGuardianRelationship.SelectedValue);
                                cmd.Parameters.AddWithValue("@Phone", guardianPhone);
                                cmd.Parameters.AddWithValue("@Email", (object)guardianEmail ?? DBNull.Value);
                                guardianId = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                        }

                        string appNo = GenerateUniqueApplicationNo(conn, tx);

                        string insertQuery = @"
                            INSERT INTO Admissions
                                (ApplicationNo, FirstName, LastName, Gender, DateOfBirth,
                                 ApplyingForClassID, GuardianID, GuardianName, GuardianPhone, GuardianEmail,
                                 PreviousSchool, LastGradeCompleted, AcademicYearID, Shift,
                                 ApplicationDate, Status, Notes)
                            VALUES
                                (@ApplicationNo, @FirstName, @LastName, @Gender, @DateOfBirth,
                                 @ClassID, @GuardianID, @GuardianName, @GuardianPhone, @GuardianEmail,
                                 @PreviousSchool, @LastGrade, @AcademicYearID, @Shift,
                                 GETDATE(), 'Pending', @Notes)";

                        using (SqlCommand cmd = new SqlCommand(insertQuery, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@ApplicationNo", appNo);
                            cmd.Parameters.AddWithValue("@FirstName", txtFirstName.Text.Trim());
                            cmd.Parameters.AddWithValue("@LastName", txtLastName.Text.Trim());
                            cmd.Parameters.AddWithValue("@Gender", ddlGender.SelectedValue);
                            cmd.Parameters.AddWithValue("@DateOfBirth", DateTime.Parse(txtDateOfBirth.Text));
                            cmd.Parameters.AddWithValue("@ClassID", int.Parse(ddlClass.SelectedValue));
                            cmd.Parameters.AddWithValue("@GuardianID", guardianId);
                            cmd.Parameters.AddWithValue("@GuardianName", guardianName);
                            cmd.Parameters.AddWithValue("@GuardianPhone", guardianPhone);
                            cmd.Parameters.AddWithValue("@GuardianEmail", (object)guardianEmail ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@PreviousSchool", EmptyToDb(txtPreviousSchool.Text));
                            cmd.Parameters.AddWithValue("@LastGrade", EmptyToDb(txtLastGradeCompleted.Text));
                            int ayId; cmd.Parameters.AddWithValue("@AcademicYearID",
                                int.TryParse(ddlAcademicYear.SelectedValue, out ayId) && ayId > 0 ? (object)ayId : DBNull.Value);
                            cmd.Parameters.AddWithValue("@Shift", EmptyToDb(ddlShift.SelectedValue));
                            string notes = txtNotes.Text.Trim();
                            cmd.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(notes) ? (object)DBNull.Value : notes);
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        Response.Redirect("~/Modules/Admission/Admissions.aspx", true);
                    }
                    catch (Exception)
                    {
                        try { tx.Rollback(); } catch { }
                        ShowError("The application could not be saved due to a system error. Please try again.");
                    }
                }
            }
        }

        private static object EmptyToDb(string value)
        {
            string v = (value ?? string.Empty).Trim();
            return string.IsNullOrEmpty(v) ? (object)DBNull.Value : v;
        }

        private void UpdateApplication(int admissionId)
        {
            // Edit mode keeps the linked guardian (Existing mode); refresh the
            // denormalised guardian snapshot from the selected guardian record.
            int guardianId;
            if (!int.TryParse(ddlExistingGuardian.SelectedValue, out guardianId) || guardianId <= 0)
            {
                ShowError("Please select a guardian.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        string guardianName, guardianPhone, guardianEmail;
                        using (SqlCommand cmd = new SqlCommand("SELECT FullName, Phone, Email FROM Guardians WHERE GuardianID = @Id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Id", guardianId);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    tx.Rollback();
                                    ShowError("The selected guardian could not be found.");
                                    return;
                                }
                                guardianName = reader["FullName"].ToString();
                                guardianPhone = reader["Phone"].ToString();
                                guardianEmail = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString();
                            }
                        }

                        string updateQuery = @"
                            UPDATE Admissions SET
                                FirstName = @FirstName, LastName = @LastName, Gender = @Gender, DateOfBirth = @DateOfBirth,
                                ApplyingForClassID = @ClassID, GuardianID = @GuardianID,
                                GuardianName = @GuardianName, GuardianPhone = @GuardianPhone, GuardianEmail = @GuardianEmail,
                                PreviousSchool = @PreviousSchool, LastGradeCompleted = @LastGrade, AcademicYearID = @AcademicYearID,
                                Shift = @Shift, Notes = @Notes, Status = @Status
                            WHERE AdmissionID = @Id AND Status IN ('Pending', 'Under Review', 'Approved', 'Enrolled', 'Rejected')";

                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Id", admissionId);
                            cmd.Parameters.AddWithValue("@FirstName", txtFirstName.Text.Trim());
                            cmd.Parameters.AddWithValue("@LastName", txtLastName.Text.Trim());
                            cmd.Parameters.AddWithValue("@Gender", ddlGender.SelectedValue);
                            cmd.Parameters.AddWithValue("@DateOfBirth", DateTime.Parse(txtDateOfBirth.Text));
                            cmd.Parameters.AddWithValue("@ClassID", int.Parse(ddlClass.SelectedValue));
                            cmd.Parameters.AddWithValue("@GuardianID", guardianId);
                            cmd.Parameters.AddWithValue("@GuardianName", guardianName);
                            cmd.Parameters.AddWithValue("@GuardianPhone", guardianPhone);
                            cmd.Parameters.AddWithValue("@GuardianEmail", (object)guardianEmail ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@PreviousSchool", EmptyToDb(txtPreviousSchool.Text));
                            cmd.Parameters.AddWithValue("@LastGrade", EmptyToDb(txtLastGradeCompleted.Text));
                            int ayId; cmd.Parameters.AddWithValue("@AcademicYearID",
                                int.TryParse(ddlAcademicYear.SelectedValue, out ayId) && ayId > 0 ? (object)ayId : DBNull.Value);
                            string notes = txtNotes.Text.Trim();
                            cmd.Parameters.AddWithValue("@Shift", EmptyToDb(ddlShift.SelectedValue));
                            cmd.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(notes) ? (object)DBNull.Value : notes);
                            cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        Response.Redirect("~/Modules/Admission/Admissions.aspx", true);
                    }
                    catch (System.Threading.ThreadAbortException)
                    {
                        throw; // Response.Redirect
                    }
                    catch (Exception)
                    {
                        try { tx.Rollback(); } catch { }
                        ShowError("The application could not be updated due to a system error. Please try again.");
                    }
                }
            }
        }

        protected void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            if (!CanManageAdmissions()) { ShowError("You do not have permission to change the status."); return; }
            if (EditId <= 0) { ShowError("Invalid application."); return; }

            string newStatus = ddlStatus.SelectedValue;
            string[] allowed = { "Pending", "Under Review", "Enrolled", "Rejected", "Approved" };
            if (Array.IndexOf(allowed, newStatus) < 0) { ShowError("Invalid status."); return; }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("UPDATE Admissions SET Status = @Status WHERE AdmissionID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    cmd.Parameters.AddWithValue("@Id", EditId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                Response.Redirect("~/Modules/Admission/Admissions.aspx", true);
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw; // Response.Redirect
            }
            catch (Exception)
            {
                ShowError("The status could not be updated due to a system error. Please try again.");
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtFirstName.Text = "";
            txtLastName.Text = "";
            ddlGender.SelectedIndex = 0;
            txtDateOfBirth.Text = "";
            ddlClass.SelectedIndex = 0;
            ddlShift.SelectedIndex = 0;
            rblGuardianMode.SelectedValue = "Existing";
            pnlExistingGuardian.Visible = true;
            pnlNewGuardian.Visible = false;
            ddlExistingGuardian.SelectedIndex = 0;
            txtGuardianName.Text = "";
            ddlGuardianRelationship.SelectedIndex = 0;
            txtGuardianPhone.Text = "";
            txtGuardianEmail.Text = "";
            txtNotes.Text = "";
            pnlGuardianDuplicateWarning.Visible = false;
            GenerateAndDisplayApplicationNo();
            pnlError.Visible = false;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/Admission/Admissions.aspx", true);
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
