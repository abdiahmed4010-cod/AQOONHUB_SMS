using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Finance
{
    public partial class AddPayment : System.Web.UI.Page
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

        private int ExecuteNonQuery(SqlConnection conn, SqlTransaction tx, string query, SqlParameter[] parameters = null)
        {
            using (SqlCommand cmd = new SqlCommand(query, conn, tx))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        private int InvoiceId
        {
            get { return ViewState["InvoiceId"] == null ? 0 : (int)ViewState["InvoiceId"]; }
            set { ViewState["InvoiceId"] = value; }
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
                ShowError("You do not have permission to record payments.");
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
                int.TryParse(Request.QueryString["invoiceId"], out id);
                InvoiceId = id;

                if (!LoadInvoice())
                {
                    pnlFormBody.Visible = false;
                    pnlNotFound.Visible = true;
                    return;
                }

                GenerateAndDisplayReceiptNo();
                txtPaymentDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            }
        }

        private bool LoadInvoice()
        {
            if (InvoiceId <= 0) return false;

            string query = @"
                SELECT i.InvoiceNo, i.TotalAmount, i.PaidAmount, i.Status,
                       LTRIM(RTRIM(ISNULL(s.FirstName,'') + ' ' + ISNULL(s.LastName,''))) AS StudentName,
                       s.StudentCode
                FROM Invoices i
                INNER JOIN Students s ON i.StudentID = s.StudentID
                WHERE i.InvoiceID = @Id";

            DataTable dt = ExecuteQuery(query, new[] { new SqlParameter("@Id", InvoiceId) });
            if (dt.Rows.Count == 0) return false;

            DataRow row = dt.Rows[0];
            decimal total = Convert.ToDecimal(row["TotalAmount"]);
            decimal paid = Convert.ToDecimal(row["PaidAmount"]);

            if (row["Status"].ToString() == "Paid" || paid >= total)
            {
                ShowError("This invoice is already fully paid.");
                pnlFormBody.Visible = false;
                return true; // still a valid invoice, just nothing to record
            }

            lblInvoiceNo.Text = row["InvoiceNo"].ToString();
            lblStudentInfo.Text = row["StudentName"] + " (" + row["StudentCode"] + ")";
            lblTotalAmount.Text = "$" + total.ToString("N2");
            lblPaidSoFar.Text = "$" + paid.ToString("N2");
            lblBalanceDue.Text = "$" + (total - paid).ToString("N2");

            return true;
        }

        #region Receipt Number Generation (RCT-{0000})

        private string GenerateReceiptNo()
        {
            string query = @"
                SELECT ISNULL(MAX(TRY_CONVERT(INT, SUBSTRING(ReceiptNo, 5, LEN(ReceiptNo)))), 0) + 1 AS NextNumber
                FROM Payments WHERE ReceiptNo LIKE 'RCT-%'";
            object result = ExecuteQuery(query).Rows[0]["NextNumber"];
            int nextNumber = (result == null || result == DBNull.Value) ? 1 : Convert.ToInt32(result);
            return "RCT-" + nextNumber.ToString("D4");
        }

        private string GenerateUniqueReceiptNo(SqlConnection conn, SqlTransaction tx)
        {
            for (int attempt = 0; attempt < 5; attempt++)
            {
                object result = ExecuteScalar(conn, tx, @"
                    SELECT ISNULL(MAX(TRY_CONVERT(INT, SUBSTRING(ReceiptNo, 5, LEN(ReceiptNo)))), 0) + 1
                    FROM Payments WITH (UPDLOCK, HOLDLOCK) WHERE ReceiptNo LIKE 'RCT-%'");

                int nextNumber = (result == null || result == DBNull.Value) ? 1 : Convert.ToInt32(result);
                string candidate = "RCT-" + nextNumber.ToString("D4");

                object exists = ExecuteScalar(conn, tx,
                    "SELECT COUNT(1) FROM Payments WITH (UPDLOCK, HOLDLOCK) WHERE ReceiptNo = @No",
                    new[] { new SqlParameter("@No", candidate) });
                if (Convert.ToInt32(exists) == 0) return candidate;
            }
            throw new InvalidOperationException("Could not generate a unique Receipt Number after several attempts.");
        }

        private void GenerateAndDisplayReceiptNo()
        {
            lblReceiptNo.Text = GenerateReceiptNo();
        }

        #endregion

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!CanManageFinance()) { ShowError("You do not have permission to record payments."); return; }
            if (!Page.IsValid) return;

            decimal amount;
            if (!decimal.TryParse(txtAmount.Text, out amount) || amount <= 0)
            { ShowError("Please provide a valid payment amount."); return; }

            DateTime paymentDate;
            if (!DateTime.TryParse(txtPaymentDate.Text, out paymentDate))
            { ShowError("Please provide a valid payment date."); return; }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        DataTable dt;
                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT TotalAmount, PaidAmount FROM Invoices WITH (UPDLOCK, HOLDLOCK) WHERE InvoiceID = @Id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Id", InvoiceId);
                            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            {
                                dt = new DataTable();
                                da.Fill(dt);
                            }
                        }
                        if (dt.Rows.Count == 0)
                        {
                            tx.Rollback();
                            ShowError("Invoice not found.");
                            return;
                        }

                        decimal total = Convert.ToDecimal(dt.Rows[0]["TotalAmount"]);
                        decimal paidSoFar = Convert.ToDecimal(dt.Rows[0]["PaidAmount"]);
                        decimal balance = total - paidSoFar;

                        if (amount > balance)
                        {
                            tx.Rollback();
                            ShowError(string.Format("Payment amount (${0:N2}) exceeds the remaining balance (${1:N2}).", amount, balance));
                            return;
                        }

                        string receiptNo = GenerateUniqueReceiptNo(conn, tx);
                        object userIdObj = Session["UserID"];
                        int receivedBy = userIdObj != null ? Convert.ToInt32(userIdObj) : 0;

                        using (SqlCommand cmd = new SqlCommand(@"
                            INSERT INTO Payments (ReceiptNo, InvoiceID, Amount, PaymentMethod, PaymentDate, ReceivedBy, Notes, CreatedAt)
                            VALUES (@ReceiptNo, @InvoiceID, @Amount, @PaymentMethod, @PaymentDate, @ReceivedBy, @Notes, GETDATE())", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@ReceiptNo", receiptNo);
                            cmd.Parameters.AddWithValue("@InvoiceID", InvoiceId);
                            cmd.Parameters.AddWithValue("@Amount", amount);
                            cmd.Parameters.AddWithValue("@PaymentMethod", ddlPaymentMethod.SelectedValue);
                            cmd.Parameters.AddWithValue("@PaymentDate", paymentDate);
                            cmd.Parameters.AddWithValue("@ReceivedBy", receivedBy);
                            string notes = txtNotes.Text.Trim();
                            cmd.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(notes) ? (object)DBNull.Value : notes);
                            cmd.ExecuteNonQuery();
                        }

                        decimal newPaidAmount = paidSoFar + amount;
                        string newStatus = newPaidAmount >= total ? "Paid" : "Partially Paid";

                        ExecuteNonQuery(conn, tx,
                            "UPDATE Invoices SET PaidAmount = @PaidAmount, Status = @Status WHERE InvoiceID = @Id",
                            new[]
                            {
                                new SqlParameter("@PaidAmount", newPaidAmount),
                                new SqlParameter("@Status", newStatus),
                                new SqlParameter("@Id", InvoiceId)
                            });

                        tx.Commit();
                        Response.Redirect("~/Modules/Finance/InvoiceDetails.aspx?id=" + InvoiceId, true);
                    }
                    catch (Exception)
                    {
                        try { tx.Rollback(); } catch { }
                        ShowError("The payment could not be recorded due to a system error. Please try again.");
                    }
                }
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/Finance/InvoiceDetails.aspx?id=" + InvoiceId, true);
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            pnlError.Visible = true;
        }
    }
}
