using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Finance
{
    public partial class RecordPayment : System.Web.UI.Page
    {
        readonly FeeRepository r = new FeeRepository();

        // True when the page was opened via Collect (invoice + student locked).
        private bool Locked
        {
            get { return ViewState["Locked"] != null && (bool)ViewState["Locked"]; }
            set { ViewState["Locked"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthorizeFinance()) return;

            if (!IsPostBack)
            {
                date.Text = DateTime.Today.ToString("yyyy-MM-dd");

                int invId = ParseInvoiceParam();
                if (invId > 0)
                {
                    // Opened via Collect — lock to this invoice/student.
                    Locked = true;
                    pnlSelect.Visible = false;
                    LoadInvoice(invId);
                }
                else
                {
                    // Free selection mode.
                    Locked = false;
                    pnlSelect.Visible = true;
                    student.DataSource = r.GetStudents();
                    student.DataTextField = "StudentName";
                    student.DataValueField = "StudentID";
                    student.DataBind();
                    student.Items.Insert(0, new ListItem("Select Student", ""));
                    BindInvoices(null);
                }
            }
        }

        #region Authorization

        private bool AuthorizeFinance()
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx", true);
                return false;
            }
            string role = Convert.ToString(Session["Role"]).Replace(" ", "").ToLowerInvariant();
            if (role != "superadmin" && role != "admin" && role != "accountant" && role != "finance")
            {
                Response.Redirect("~/Default.aspx", true);
                return false;
            }
            return true;
        }

        #endregion

        private int ParseInvoiceParam()
        {
            // Accept both ?invoiceId= (preferred) and legacy ?invoice=.
            string raw = Request.QueryString["invoiceId"] ?? Request.QueryString["invoice"];
            int id;
            return int.TryParse(raw, out id) && id > 0 ? id : 0;
        }

        #region Free-selection mode

        void BindInvoices(int? studentId)
        {
            invoice.DataSource = r.GetOpenInvoices(studentId);
            invoice.DataTextField = "InvoiceNumber";
            invoice.DataValueField = "InvoiceID";
            invoice.DataBind();
            invoice.Items.Insert(0, new ListItem("Select Invoice", ""));
        }

        protected void student_Changed(object s, EventArgs e)
        {
            BindInvoices(student.SelectedValue == "" ? (int?)null : int.Parse(student.SelectedValue));
            pnlInfo.Visible = false;
            pnlPay.Visible = false;
        }

        protected void invoice_Changed(object s, EventArgs e)
        {
            int id;
            if (!int.TryParse(invoice.SelectedValue, out id) || id <= 0)
            {
                pnlInfo.Visible = false;
                pnlPay.Visible = false;
                return;
            }
            LoadInvoice(id);
        }

        #endregion

        #region Load invoice (shared)

        private void LoadInvoice(int invoiceId)
        {
            DataTable dt = r.GetInvoice(invoiceId);
            if (dt.Rows.Count == 0)
            {
                ShowError("The selected invoice could not be found.");
                pnlInfo.Visible = false;
                pnlPay.Visible = false;
                return;
            }

            DataRow row = dt.Rows[0];
            decimal total = Convert.ToDecimal(row["TotalAmount"]);
            decimal balance = Convert.ToDecimal(row["Balance"]);
            decimal paid = total - balance;
            string status = Convert.ToString(row["Status"]);

            hidInvoiceId.Value = invoiceId.ToString();
            hidBalance.Value = balance.ToString("0.00");

            litStudent.Text = Enc(row["StudentName"]);
            litStudentCode.Text = Enc(row["StudentCode"]);
            litClass.Text = Enc(row["ClassName"]);
            litInvoiceNo.Text = Enc(row["InvoiceNumber"]);
            litInvAmount.Text = total.ToString("N2");
            litPaid.Text = paid.ToString("N2");
            litPrevBalance.Text = balance.ToString("N2");
            litDueDate.Text = row["DueDate"] == DBNull.Value ? "—" : Convert.ToDateTime(row["DueDate"]).ToString("dd MMM yyyy");
            litStatus.Text = Enc(EffectiveStatus(status, balance, row["DueDate"]));

            pnlInfo.Visible = true;

            // Do not allow payment against a cancelled/void or fully-settled invoice.
            if (string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "Void", StringComparison.OrdinalIgnoreCase))
            {
                pnlPay.Visible = false;
                ShowInfo("This invoice is " + status + " and cannot accept payments.");
                return;
            }
            if (balance <= 0)
            {
                pnlPay.Visible = false;
                ShowInfo("This invoice is already fully paid. No balance is outstanding.");
                return;
            }

            pnlPay.Visible = true;
        }

        private string EffectiveStatus(string status, decimal balance, object dueDate)
        {
            if (string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase)) return "Cancelled";
            if (balance <= 0) return "Paid";
            bool overdue = dueDate != DBNull.Value && Convert.ToDateTime(dueDate).Date < DateTime.Today;
            if (overdue) return (status == "Partial" || status == "Paid") ? "Partial · Overdue" : "Unpaid · Overdue";
            return status;
        }

        #endregion

        #region Save

        protected void save_Click(object sender, EventArgs e)
        {
            msg.Visible = false;

            // Resolve invoice id from the locked hidden field or the free-mode dropdown.
            int invoiceId;
            if (Locked)
                int.TryParse(hidInvoiceId.Value, out invoiceId);
            else
                int.TryParse(invoice.SelectedValue, out invoiceId);

            if (invoiceId <= 0) { ShowError("Please select a valid invoice."); return; }

            decimal amt;
            if (!decimal.TryParse(amount.Text, out amt) || amt <= 0)
            {
                ShowError("Please enter a valid payment amount greater than zero.");
                return;
            }

            if (string.IsNullOrEmpty(method.SelectedValue))
            {
                ShowError("Please select a payment method.");
                return;
            }

            DateTime payDate;
            if (!DateTime.TryParse(date.Text, out payDate))
            {
                ShowError("Please provide a valid payment date.");
                return;
            }

            string reference = (reference_Text());
            string methodValue = method.SelectedValue;
            if (string.IsNullOrEmpty(reference) &&
                (methodValue == "Bank Transfer" || methodValue == "Mobile Money" || methodValue == "Cheque"))
            {
                ShowError("A reference / transaction ID is required for " + methodValue + " payments.");
                return;
            }

            try
            {
                // The repository reloads and locks the invoice inside a serializable
                // transaction (UPDLOCK, HOLDLOCK), recalculates the real balance,
                // rejects overpayments, inserts the payment, generates a unique
                // receipt number and updates the invoice status atomically.
                int paymentId = r.RecordPayment(invoiceId, amt, methodValue, payDate,
                    reference, notes.Text.Trim(), Convert.ToInt32(Session["UserID"]));

                ShowSuccess(paymentId, invoiceId);
            }
            catch (Exception ex)
            {
                // Never surface SQL/stack details to the user.
                ShowError(FriendlyError(ex.Message));
            }
        }

        private string reference_Text()
        {
            return (reference.Text ?? string.Empty).Trim();
        }

        private void ShowSuccess(int paymentId, int invoiceId)
        {
            DataTable pay = r.GetPayment(paymentId);
            DataTable inv = r.GetInvoice(invoiceId);

            string receipt = "", amtStr = "0.00", newBal = "0.00", status = "";
            if (pay.Rows.Count > 0)
            {
                DataRow p = pay.Rows[0];
                receipt = Convert.ToString(p["ReceiptNumber"]);
                amtStr = Convert.ToDecimal(p["AmountPaid"]).ToString("N2");
                if (pay.Columns.Contains("NewBalance") && p["NewBalance"] != DBNull.Value)
                    newBal = Convert.ToDecimal(p["NewBalance"]).ToString("N2");
            }
            if (inv.Rows.Count > 0)
            {
                DataRow i = inv.Rows[0];
                decimal bal = Convert.ToDecimal(i["Balance"]);
                newBal = bal.ToString("N2");
                status = EffectiveStatus(Convert.ToString(i["Status"]), bal, i["DueDate"]);
            }

            litRcpNumber.Text = Enc(receipt);
            litRcpAmount.Text = amtStr;
            litRcpBalance.Text = newBal;
            litRcpStatus.Text = Enc(status);
            lnkReceipt.NavigateUrl = "PrintReceipt.aspx?id=" + paymentId;

            pnlForm.Visible = false;
            pnlSuccess.Visible = true;
        }

        private string FriendlyError(string raw)
        {
            if (!string.IsNullOrEmpty(raw) &&
                (raw.IndexOf("exceed", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 raw.IndexOf("greater than zero", StringComparison.OrdinalIgnoreCase) >= 0))
                return "The payment must be greater than zero and cannot exceed the current outstanding balance.";
            if (!string.IsNullOrEmpty(raw) && raw.IndexOf("Cancelled", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Cancelled invoices cannot accept payments.";
            return "The payment could not be recorded due to a system error. Please try again.";
        }

        #endregion

        private string Enc(object value)
        {
            return HttpUtility.HtmlEncode(value == null || value == DBNull.Value ? "" : value.ToString());
        }

        private void ShowError(string message)
        {
            msg.CssClass = "p-3 mb-4 rounded-lg bg-amber-50 text-amber-800 border border-amber-200 text-sm dark:bg-amber-500/10 dark:text-amber-300 dark:border-amber-500/30";
            msg.Visible = true;
            msgText.Text = HttpUtility.HtmlEncode(message);
        }

        private void ShowInfo(string message)
        {
            msg.CssClass = "p-3 mb-4 rounded-lg bg-blue-50 text-blue-800 border border-blue-200 text-sm dark:bg-blue-500/10 dark:text-blue-300 dark:border-blue-500/30";
            msg.Visible = true;
            msgText.Text = HttpUtility.HtmlEncode(message);
        }
    }
}
