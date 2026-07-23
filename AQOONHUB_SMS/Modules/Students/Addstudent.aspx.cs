using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Students
{
    public partial class AddStudent : System.Web.UI.Page
    {
        // Ku isticmaal ConfigurationManager toos ah
        private string connString = ConfigurationManager.ConnectionStrings["AQOONHUB_DB"].ConnectionString;

        private SqlConnection GetConnection()
        {
            return new SqlConnection(connString);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
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

        private void LoadAcademicYears()
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(
                        @"SELECT AcademicYearID, AcademicYearName 
                          FROM AcademicYears 
                          WHERE IsActive = 1 
                          ORDER BY StartDate DESC", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    ddlAcademicYear.DataSource = dt;
                    ddlAcademicYear.DataTextField = "AcademicYearName";
                    ddlAcademicYear.DataValueField = "AcademicYearID";
                    ddlAcademicYear.DataBind();
                    ddlAcademicYear.Items.Insert(0, new ListItem("Select Academic Year", "0"));
                }
            }
            catch (Exception ex)
            {
                ShowError("Error loading academic years: " + ex.Message);
            }
        }

        private void LoadClasses()
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(
                        @"SELECT ClassID, ClassName 
                          FROM Classes 
                          WHERE IsActive = 1 
                          ORDER BY SortOrder, ClassName", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    ddlClass.DataSource = dt;
                    ddlClass.DataTextField = "ClassName";
                    ddlClass.DataValueField = "ClassID";
                    ddlClass.DataBind();
                    ddlClass.Items.Insert(0, new ListItem("Select Class", "0"));
                }
            }
            catch (Exception ex)
            {
                ShowError("Error loading classes: " + ex.Message);
            }
        }

        private void LoadSections(int classId)
        {
            try
            {
                ddlSection.Items.Clear();
                ddlSection.Items.Insert(0, new ListItem("Select Section", "0"));

                if (classId <= 0) return;

                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        @"SELECT SectionID, SectionName 
                          FROM Sections 
                          WHERE ClassID = @ClassID AND IsActive = 1 
                          ORDER BY SectionName", conn);
                    cmd.Parameters.AddWithValue("@ClassID", classId);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    ddlSection.DataSource = dt;
                    ddlSection.DataTextField = "SectionName";
                    ddlSection.DataValueField = "SectionID";
                    ddlSection.DataBind();
                    ddlSection.Items.Insert(0, new ListItem("Select Section", "0"));
                }
            }
            catch (Exception ex)
            {
                ShowError("Error loading sections: " + ex.Message);
            }
        }

        private void LoadGuardians()
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(
                        @"SELECT GuardianID, 
                                 FirstName + ' ' + LastName + ' (' + ISNULL(Phone, '') + ')' AS GuardianName 
                          FROM Guardians 
                          WHERE IsActive = 1 
                          ORDER BY FirstName, LastName", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        pnlGuardianField.Visible = false;
                        pnlNoGuardians.Visible = true;
                    }
                    else
                    {
                        pnlGuardianField.Visible = true;
                        pnlNoGuardians.Visible = false;
                        ddlGuardian.DataSource = dt;
                        ddlGuardian.DataTextField = "GuardianName";
                        ddlGuardian.DataValueField = "GuardianID";
                        ddlGuardian.DataBind();
                        ddlGuardian.Items.Insert(0, new ListItem("Select Guardian (Optional)", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Error loading guardians: " + ex.Message);
            }
        }

        // ============================================
        // STUDENT CODE GENERATION
        // ============================================
        private void GenerateAndDisplayStudentCode()
        {
            string studentCode = GenerateStudentCode();
            hdnStudentCode.Value = studentCode;
            lblStudentCode.Text = studentCode;
        }

        private string GenerateStudentCode()
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        @"SELECT ISNULL(MAX(CAST(SUBSTRING(StudentCode, 4, LEN(StudentCode)) AS INT)), 0) + 1
                          FROM Students WHERE StudentCode LIKE 'STU-%'", conn);
                    object result = cmd.ExecuteScalar();
                    int nextNumber = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 1;
                    return "STU-" + nextNumber.ToString("D4");
                }
            }
            catch
            {
                return "STU-" + DateTime.Now.Ticks.ToString().Substring(0, 4);
            }
        }

        // ============================================
        // ADMISSION NUMBER GENERATION
        // ============================================
        private void GenerateAndDisplayAdmissionNumber()
        {
            string admissionNo = GenerateAdmissionNumber();
            hdnAdmissionNo.Value = admissionNo;
            lblAdmissionNo.Text = admissionNo;
        }

        private string GenerateAdmissionNumber()
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        @"SELECT ISNULL(MAX(TRY_CONVERT(INT, SUBSTRING(AdmissionNo, 5, LEN(AdmissionNo)))), 0) + 1
                          FROM Students WHERE AdmissionNo LIKE 'ADM-%'", conn);
                    object result = cmd.ExecuteScalar();
                    int nextNumber = (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 1;
                    return "ADM-" + nextNumber.ToString("D4");
                }
            }
            catch
            {
                return GenerateAdmissionNumberFallback();
            }
        }

        private string GenerateAdmissionNumberFallback()
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(
                        "SELECT AdmissionNo FROM Students WHERE AdmissionNo LIKE 'ADM-%'", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    int maxNumber = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        string numPart = row["AdmissionNo"].ToString().Substring(4);
                        if (int.TryParse(numPart, out int num) && num > maxNumber)
                            maxNumber = num;
                    }
                    return "ADM-" + (maxNumber + 1).ToString("D4");
                }
            }
            catch
            {
                return "ADM-" + DateTime.Now.ToString("yyyyMMdd") + "-001";
            }
        }

        // ============================================
        // EVENT HANDLERS
        // ============================================
        protected void ddlClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            int classId = 0;
            int.TryParse(ddlClass.SelectedValue, out classId);
            LoadSections(classId);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/Students/Students.aspx");
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            int studentId = SaveStudent();
            if (studentId > 0)
            {
                ShowSuccess("Student saved successfully. Admission Number: " + lblAdmissionNo.Text);
                ClearForm();
            }
        }

        protected void btnSaveAndAddAnother_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            int studentId = SaveStudent();
            if (studentId > 0)
            {
                ShowSuccess("Student saved successfully. Admission Number: " + lblAdmissionNo.Text);
                ClearForm();
            }
        }

        // ============================================
        // SAVE STUDENT (Transaction-safe)
        // ============================================
        private int SaveStudent()
        {
            SqlConnection conn = null;
            SqlTransaction tx = null;
            try
            {
                conn = GetConnection();
                conn.Open();
                tx = conn.BeginTransaction();

                string admissionNo = GenerateUniqueAdmissionNumber(conn, tx);

                string photoPath = null;
                if (fuPhoto.HasFile)
                    photoPath = SavePhoto(fuPhoto.PostedFile);

                string query = @"
                    INSERT INTO Students (
                        StudentCode, AdmissionNo, FirstName, LastName, Gender, 
                        Status, DateOfBirth, EnrollmentDate, AcademicYearID, 
                        ClassID, SectionID, GuardianID, Address, MedicalNotes, 
                        PhotoPath, CreatedDate, CreatedBy
                    ) VALUES (
                        @StudentCode, @AdmissionNo, @FirstName, @LastName, @Gender,
                        @Status, @DateOfBirth, @EnrollmentDate, @AcademicYearID,
                        @ClassID, @SectionID, @GuardianID, @Address, @MedicalNotes,
                        @PhotoPath, GETDATE(), @CreatedBy
                    );
                    SELECT SCOPE_IDENTITY();";

                SqlCommand cmd = new SqlCommand(query, conn, tx);
                cmd.Parameters.Add(new SqlParameter("@StudentCode", hdnStudentCode.Value));
                cmd.Parameters.Add(new SqlParameter("@AdmissionNo", admissionNo));
                cmd.Parameters.Add(new SqlParameter("@FirstName", txtFirstName.Text.Trim()));
                cmd.Parameters.Add(new SqlParameter("@LastName", txtLastName.Text.Trim()));
                cmd.Parameters.Add(new SqlParameter("@Gender", ddlGender.SelectedValue));
                cmd.Parameters.Add(new SqlParameter("@Status", ddlStatus.SelectedValue));
                cmd.Parameters.Add(new SqlParameter("@DateOfBirth", Convert.ToDateTime(txtDateOfBirth.Text)));
                cmd.Parameters.Add(new SqlParameter("@EnrollmentDate", Convert.ToDateTime(txtEnrollmentDate.Text)));
                cmd.Parameters.Add(new SqlParameter("@AcademicYearID", Convert.ToInt32(ddlAcademicYear.SelectedValue)));
                cmd.Parameters.Add(new SqlParameter("@ClassID", Convert.ToInt32(ddlClass.SelectedValue)));
                cmd.Parameters.Add(new SqlParameter("@SectionID", Convert.ToInt32(ddlSection.SelectedValue)));

                if (!string.IsNullOrEmpty(ddlGuardian.SelectedValue))
                    cmd.Parameters.Add(new SqlParameter("@GuardianID", Convert.ToInt32(ddlGuardian.SelectedValue)));
                else
                    cmd.Parameters.Add(new SqlParameter("@GuardianID", DBNull.Value));

                cmd.Parameters.Add(new SqlParameter("@Address", string.IsNullOrEmpty(txtAddress.Text) ? (object)DBNull.Value : txtAddress.Text.Trim()));
                cmd.Parameters.Add(new SqlParameter("@MedicalNotes", string.IsNullOrEmpty(txtMedicalNotes.Text) ? (object)DBNull.Value : txtMedicalNotes.Text.Trim()));
                cmd.Parameters.Add(new SqlParameter("@PhotoPath", photoPath ?? (object)DBNull.Value));

                int currentUserId = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 1;
                cmd.Parameters.Add(new SqlParameter("@CreatedBy", currentUserId));

                object result = cmd.ExecuteScalar();
                int newStudentId = Convert.ToInt32(result);

                hdnAdmissionNo.Value = admissionNo;
                lblAdmissionNo.Text = admissionNo;

                tx.Commit();
                return newStudentId;
            }
            catch (Exception ex)
            {
                if (tx != null) tx.Rollback();
                ShowError("Error saving student: " + ex.Message);
                return 0;
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        private string GenerateUniqueAdmissionNumber(SqlConnection conn, SqlTransaction tx)
        {
            string query = @"
                SELECT ISNULL(MAX(TRY_CONVERT(INT, SUBSTRING(AdmissionNo, 5, LEN(AdmissionNo)))), 0) + 1
                FROM Students WITH (UPDLOCK, HOLDLOCK) WHERE AdmissionNo LIKE 'ADM-%'";
            SqlCommand cmd = new SqlCommand(query, conn, tx);
            object result = cmd.ExecuteScalar();
            int nextNumber = (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 1;
            string candidate = "ADM-" + nextNumber.ToString("D4");

            string verify = "SELECT COUNT(*) FROM Students WHERE AdmissionNo = @AdmissionNo";
            SqlCommand verifyCmd = new SqlCommand(verify, conn, tx);
            verifyCmd.Parameters.AddWithValue("@AdmissionNo", candidate);
            while (Convert.ToInt32(verifyCmd.ExecuteScalar()) > 0)
            {
                nextNumber++;
                candidate = "ADM-" + nextNumber.ToString("D4");
                verifyCmd.Parameters["@AdmissionNo"].Value = candidate;
            }
            return candidate;
        }

        // ============================================
        // VALIDATION
        // ============================================
        protected void cvDateOfBirth_ServerValidate(object source, ServerValidateEventArgs args)
        {
            DateTime dob;
            if (DateTime.TryParse(args.Value, out dob))
            {
                int age = DateTime.Now.Year - dob.Year;
                if (dob > DateTime.Now.AddYears(-age)) age--;
                args.IsValid = (age >= 3 && age <= 25 && dob < DateTime.Now);
            }
            else
            {
                args.IsValid = false;
            }
        }

        protected void cvPhoto_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (!fuPhoto.HasFile)
            {
                args.IsValid = true;
                return;
            }
            if (fuPhoto.PostedFile.ContentLength > 2 * 1024 * 1024)
            {
                args.IsValid = false;
                return;
            }
            string ext = Path.GetExtension(fuPhoto.FileName).ToLower();
            string[] allowed = { ".jpg", ".jpeg", ".png", ".webp" };
            args.IsValid = allowed.Contains(ext);
        }

        // ============================================
        // HELPERS
        // ============================================
        private string SavePhoto(System.Web.HttpPostedFile file)
        {
            string uploadFolder = Server.MapPath("~/Uploads/Students/");
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadFolder, fileName);
            file.SaveAs(filePath);
            return "~/Uploads/Students/" + fileName;
        }

        private void ClearForm()
        {
            txtFirstName.Text = "";
            txtLastName.Text = "";
            ddlGender.SelectedIndex = 0;
            ddlStatus.SelectedIndex = 0;
            txtDateOfBirth.Text = "";
            txtEnrollmentDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            ddlAcademicYear.SelectedIndex = 0;
            ddlClass.SelectedIndex = 0;
            LoadSections(0);
            ddlSection.SelectedIndex = 0;
            ddlGuardian.SelectedIndex = 0;
            txtAddress.Text = "";
            txtMedicalNotes.Text = "";
            fuPhoto.Attributes.Clear();

            GenerateAndDisplayStudentCode();
            GenerateAndDisplayAdmissionNumber();

            ScriptManager.RegisterStartupScript(this, GetType(), "resetPhoto",
                "document.getElementById('imgPreview').style.display='none';" +
                "document.getElementById('imgPreviewFallback').style.display='flex';", true);
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