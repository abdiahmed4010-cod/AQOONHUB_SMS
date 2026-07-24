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

        private void LoadSections(int classId, int selectSectionId = 0)
        {
            ddlSection.Items.Clear();
            ddlSection.Items.Add(new ListItem("Select Section", "0"));
            if (classId <= 0) return;

            string query = "SELECT SectionID, SectionName FROM Sections WHERE ClassID = @ClassID ORDER BY SectionName";
            DataTable dt = ExecuteQuery(query, new[] { new SqlParameter("@ClassID", classId) });
            foreach (DataRow row in dt.Rows)
                ddlSection.Items.Add(new ListItem(row["SectionName"].ToString(), row["SectionID"].ToString()));

            if (selectSectionId > 0)
            {
                ListItem item = ddlSection.Items.FindByValue(selectSectionId.ToString());
                if (item != null) { ddlSection.ClearSelection(); item.Selected = true; }
            }
        }

        protected void ddlClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            int classId;
            int.TryParse(ddlClass.SelectedValue, out classId);
            LoadSections(classId);
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
            LoadSections(classId, Convert.ToInt32(row["SectionID"]));

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

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!CanEditStudent()) { ShowError("You do not have permission to edit students."); return; }
            if (!Page.IsValid) return;

            string validationError;
            if (!ValidateStudent(out validationError))
            {
                ShowError(validationError);
                return;
            }

            string newPhotoPath = CurrentPhotoPath;
            try
            {
                string uploaded = SaveUploadedPhoto();
                if (!string.IsNullOrEmpty(uploaded)) newPhotoPath = uploaded;
            }
            catch (Exception)
            {
                ShowError("The photo could not be saved. Please try a different file.");
                return;
            }

            int? guardianId = null;
            int parsedGuardianId;
            if (int.TryParse(ddlGuardian.SelectedValue, out parsedGuardianId) && parsedGuardianId > 0)
                guardianId = parsedGuardianId;

            string query = @"
                UPDATE Students SET
                    FirstName = @FirstName, LastName = @LastName, Gender = @Gender,
                    Status = @Status, DateOfBirth = @DateOfBirth, EnrollmentDate = @EnrollmentDate,
                    AcademicYearID = @AcademicYearID, SectionID = @SectionID, GuardianID = @GuardianID,
                    Address = @Address, MedicalNotes = @MedicalNotes, PhotoPath = @PhotoPath,
                    UpdatedAt = GETDATE()
                WHERE StudentID = @StudentID";

            SqlParameter[] parameters =
            {
                new SqlParameter("@FirstName", txtFirstName.Text.Trim()),
                new SqlParameter("@LastName", txtLastName.Text.Trim()),
                new SqlParameter("@Gender", ddlGender.SelectedValue),
                new SqlParameter("@Status", ddlStatus.SelectedValue),
                new SqlParameter("@DateOfBirth", DateTime.Parse(txtDateOfBirth.Text)),
                new SqlParameter("@EnrollmentDate", DateTime.Parse(txtEnrollmentDate.Text)),
                new SqlParameter("@AcademicYearID", int.Parse(ddlAcademicYear.SelectedValue)),
                new SqlParameter("@SectionID", int.Parse(ddlSection.SelectedValue)),
                new SqlParameter("@GuardianID", (object)guardianId ?? DBNull.Value),
                new SqlParameter("@Address", string.IsNullOrEmpty(txtAddress.Text.Trim()) ? (object)DBNull.Value : txtAddress.Text.Trim()),
                new SqlParameter("@MedicalNotes", string.IsNullOrEmpty(txtMedicalNotes.Text.Trim()) ? (object)DBNull.Value : txtMedicalNotes.Text.Trim()),
                new SqlParameter("@PhotoPath", (object)newPhotoPath ?? DBNull.Value),
                new SqlParameter("@StudentID", StudentId)
            };

            try
            {
                ExecuteNonQuery(query, parameters);
                CurrentPhotoPath = newPhotoPath;
                Response.Redirect("~/Modules/Students/StudentDetails.aspx?id=" + StudentId, true);
            }
            catch (Exception)
            {
                ShowError("The student could not be updated due to a system error. Please try again.");
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/Students/StudentDetails.aspx?id=" + StudentId, true);
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
