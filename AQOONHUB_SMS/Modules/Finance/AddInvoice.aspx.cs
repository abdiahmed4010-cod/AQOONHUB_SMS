using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Finance
{
    public partial class AddInvoice : System.Web.UI.Page
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

        private object ExecuteScalar(SqlConnection conn, SqlTransaction tx, string query, SqlParameter[] parameters = null)
        {
            using (SqlCommand cmd = new SqlCommand(query, conn, tx))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteScalar();
            }
        }

        private int SelectedStudentId
        {
            get { return ViewState["SelStudentId"] == null ? 0 : (int)ViewState["SelStudentId"]; }
            set { ViewState["SelStudentId"] = value; }
        }

        private int SelectedClassId
        {
            get { return ViewState["SelClassId"] == null ? 0 : (int)ViewState["SelClassId"]; }
            set { ViewState["SelClassId"] = value; }
        }

        #region Authorization

        private string NormalizeRole(string role)
        {
            if (string.IsNullOrEmpty(role)) return string.Empty;
            return role.Trim().Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();
        }

        private static readonly string[] FullAccessRoles = { "superadmin", "admin", "accountant" };

        private bool CanManageFinance()
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
            if (!CanManageFinance())
            {
                ShowError("You do not have permission to generate invoices.");
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
                txtDueDate.Text = DateTime.Now.AddDays(14).ToString("yyyy-MM-dd");
            }
        }

        protected void btnFindStudent_Click(object sender, EventArgs e)
        {
            string search = txtStudentSearch.Text.Trim();
            string query = @"
                SELECT s.StudentID, s.StudentCode, sec.ClassID,
                       LTRIM(RTRIM(ISNULL(s.FirstName,'') + ' ' + ISNULL(s.LastName,''))) AS FullName,
                       c.ClassName
                FROM Students s
                INNER JOIN Sections sec ON s.SectionID = sec.SectionID
                INNER JOIN Classes c ON sec.ClassID = c.ClassID
                WHERE s.Status <> 'Deleted'
                  AND (s.StudentCode LIKE @Search OR s.FirstName LIKE @Search OR s.LastName LIKE @Search)
                ORDER BY s.FirstName";

            DataTable dt = ExecuteQuery(query, new[] { new SqlParameter("@Search", "%" + search + "%") });
            gvStudentResults.DataSource = dt;
            gvStudentResults.DataBind();
            gvStudentResults.Visible = true;
        }

        protected void gvStudentResults_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Select") return;

            string[] parts = e.CommandArgument.ToString().Split('|');
            int studentId = Convert.ToInt32(parts[0]);
            int classId = Convert.ToInt32(parts[1]);

            SelectedStudentId = studentId;
            SelectedClassId = classId;
            hdnStudentId.Value = studentId.ToString();

            DataTable dt = ExecuteQuery(@"
                SELECT LTRIM(RTRIM(ISNULL(FirstName,'') + ' ' + ISNULL(LastName,''))) AS FullName, StudentCode
                FROM Students WHERE StudentID = @Id", new[] { new SqlParameter("@Id", studentId) });

            if (dt.Rows.Count > 0)
            {
                lblSelectedStudent.Text = dt.Rows[0]["FullName"] + " (" + dt.Rows[0]["StudentCode"] + ")";
                lblSelectedStudent.Visible = true;
            }

            gvStudentResults.Visible = false;
            LoadAcademicYears();
            LoadTerms(0);
            LoadApplicableFees();
            pnlInvoiceForm.Visible = true;
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

        private void LoadTerms(int academicYearId)
        {
            ddlTerm.Items.Clear();
            ddlTerm.Items.Add(new ListItem("Select Term", "0"));

            DataTable dt = academicYearId > 0
                ? ExecuteQuery("SELECT TermID, TermName FROM Terms WHERE AcademicYearID = @AY ORDER BY StartDate", new[] { new SqlParameter("@AY", academicYearId) })
                : ExecuteQuery("SELECT TermID, TermName FROM Terms ORDER BY StartDate DESC");

            foreach (DataRow row in dt.Rows)
                ddlTerm.Items.Add(new ListItem(row["TermName"].ToString(), row["TermID"].ToString()));
        }

        protected void ddlTerm_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadApplicableFees();
        }

        protected void cvStudentSelected_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = SelectedStudentId > 0;
        }

        /// <summary>
        /// Loads FeeStructures matching the selected student's class (or school-wide,
        /// ClassID IS NULL) and the selected academic year, as a checkbox list.
        /// </summary>
        private void LoadApplicableFees()
        {
            int academicYearId;
            int.TryParse(ddlAcademicYear.SelectedValue, out academicYearId);
            if (academicYearId <= 0) { cblFees.Items.Clear(); return; }

            string query = @"
                SELECT FeeStructureID, FeeName, Category, Amount, BillingTerm
                FROM FeeStructures
                WHERE IsActive = 1 AND AcademicYearID = @AY AND (ClassID = @ClassID OR ClassID IS NULL)
                ORDER BY FeeName";

            DataTable dt = ExecuteQuery(query, new[]
            {
                new SqlParameter("@AY", academicYearId),
                new SqlParameter("@ClassID", SelectedClassId)
            });

            cblFees.Items.Clear();
            foreach (DataRow row in dt.Rows)
            {
                decimal amount = Convert.ToDecimal(row["Amount"]);
                string label = string.Format("{0} ({1}, {2}) — ${3:N2}", row["FeeName"], row["Category"], row["BillingTerm"], amount);
                ListItem item = new ListItem(label, row["FeeStructureID"] + "|" + amount.ToString("F2"));
                cblFees.Items.Add(item);
            }
            lblNoFees.Visible = dt.Rows.Count == 0;
        }

        #region Invoice Number Generation (INV-{year}-{0000})

        private string GenerateUniqueInvoiceNo(SqlConnection conn, SqlTransaction tx)
        {
            int year = DateTime.Now.Year;
            string prefix = "INV-" + year + "-";

            for (int attempt = 0; attempt < 5; attempt++)
            {
                object result;
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT TOP 1 InvoiceNo FROM Invoices WITH (UPDLOCK, HOLDLOCK) WHERE InvoiceNo LIKE @Prefix + '%' ORDER BY InvoiceNo DESC", conn, tx))
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
                    "SELECT COUNT(1) FROM Invoices WITH (UPDLOCK, HOLDLOCK) WHERE InvoiceNo = @No",
                    new[] { new SqlParameter("@No", candidate) });
                if (Convert.ToInt32(exists) == 0) return candidate;
            }
            throw new InvalidOperationException("Could not generate a unique Invoice Number after several attempts.");
        }

        #endregion

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            if (!CanManageFinance()) { ShowError("You do not have permission to generate invoices."); return; }
            if (!Page.IsValid) return;

            if (SelectedStudentId <= 0) { ShowError("Please select a student."); return; }

            int academicYearId, termId;
            if (!int.TryParse(ddlAcademicYear.SelectedValue, out academicYearId) || academicYearId <= 0)
            { ShowError("Please select an academic year."); return; }
            if (!int.TryParse(ddlTerm.SelectedValue, out termId) || termId <= 0)
            { ShowError("Please select a term."); return; }

            DateTime dueDate;
            if (!DateTime.TryParse(txtDueDate.Text, out dueDate))
            { ShowError("Please provide a valid due date."); return; }

            decimal total = 0;
            List<Tuple<int, string, decimal>> selectedFees = new List<Tuple<int, string, decimal>>();
            foreach (ListItem item in cblFees.Items)
            {
                if (!item.Selected) continue;
                string[] parts = item.Value.Split('|');
                int feeId = int.Parse(parts[0]);
                decimal amount = decimal.Parse(parts[1]);
                total += amount;
                selectedFees.Add(new Tuple<int, string, decimal>(feeId, item.Text, amount));
            }

            if (selectedFees.Count == 0)
            {
                ShowError("Please select at least one fee to include on this invoice.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        string invoiceNo = GenerateUniqueInvoiceNo(conn, tx);
                        object userIdObj = Session["UserID"];
                        object userId = userIdObj != null ? (object)Convert.ToInt32(userIdObj) : DBNull.Value;

                        int invoiceId;
                        using (SqlCommand cmd = new SqlCommand(@"
                            INSERT INTO Invoices (InvoiceNo, StudentID, AcademicYearID, TermID, TotalAmount, PaidAmount, DueDate, Status, GeneratedBy, GeneratedAt)
                            OUTPUT INSERTED.InvoiceID
                            VALUES (@InvoiceNo, @StudentID, @AcademicYearID, @TermID, @TotalAmount, 0, @DueDate, 'Unpaid', @GeneratedBy, GETDATE())", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
                            cmd.Parameters.AddWithValue("@StudentID", SelectedStudentId);
                            cmd.Parameters.AddWithValue("@AcademicYearID", academicYearId);
                            cmd.Parameters.AddWithValue("@TermID", termId);
                            cmd.Parameters.AddWithValue("@TotalAmount", total);
                            cmd.Parameters.AddWithValue("@DueDate", dueDate);
                            cmd.Parameters.AddWithValue("@GeneratedBy", userId);
                            invoiceId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        foreach (Tuple<int, string, decimal> fee in selectedFees)
                        {
                            using (SqlCommand cmd = new SqlCommand(@"
                                INSERT INTO InvoiceItems (InvoiceID, FeeStructureID, Description, Amount)
                                VALUES (@InvoiceID, @FeeStructureID, @Description, @Amount)", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);
                                cmd.Parameters.AddWithValue("@FeeStructureID", fee.Item1);
                                cmd.Parameters.AddWithValue("@Description", fee.Item2);
                                cmd.Parameters.AddWithValue("@Amount", fee.Item3);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                        Response.Redirect("~/Modules/Finance/InvoiceDetails.aspx?id=" + invoiceId, true);
                    }
                    catch (Exception)
                    {
                        try { tx.Rollback(); } catch { }
                        ShowError("The invoice could not be generated due to a system error. Please try again.");
                    }
                }
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/Finance/Invoices.aspx", true);
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            pnlError.Visible = true;
        }
    }
}