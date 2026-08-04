using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Finance
{
    public partial class PendingFinanceSetup : System.Web.UI.Page
    {
        private readonly FeeRepository _repo = new FeeRepository();
        private const int PageSize = 10;

        private string Role { get { return Convert.ToString(Session["Role"]).Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant(); } }
        private bool IsAuthorized { get { string r = Role; return r == "superadmin" || r == "admin" || r == "accountant" || r == "finance"; } }

        private int CurrentPage
        {
            get { return ViewState["pg"] == null ? 1 : (int)ViewState["pg"]; }
            set { ViewState["pg"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return; }
            if (!IsAuthorized) { Response.StatusCode = 403; Response.Redirect("~/Modules/Dashboard/Dashboard.aspx?denied=finance", true); return; }

            if (!IsPostBack)
            {
                BindYears();
                BindClasses();
                BindSections();
                txtDueDate.Text = DateTime.Today.AddDays(14).ToString("yyyy-MM-dd");
                BindSummary();
                BindQueue();
            }
        }

        // ---- filter binders ----
        private void BindYears()
        {
            ddlYear.Items.Clear();
            ddlYear.Items.Add(new ListItem("All Years", ""));
            foreach (DataRow r in _repo.GetAcademicYears().Rows)
                ddlYear.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
        }
        private void BindClasses()
        {
            ddlClass.Items.Clear();
            ddlClass.Items.Add(new ListItem("All Classes", ""));
            foreach (DataRow r in _repo.GetClasses().Rows)
                ddlClass.Items.Add(new ListItem(Convert.ToString(r["ClassName"]), Convert.ToString(r["ClassID"])));
        }
        private void BindSections()
        {
            ddlSection.Items.Clear();
            ddlSection.Items.Add(new ListItem("All Sections", ""));
            int classId;
            if (int.TryParse(ddlClass.SelectedValue, out classId) && classId > 0)
                foreach (DataRow r in _repo.GetSections(classId).Rows)
                    ddlSection.Items.Add(new ListItem(Convert.ToString(r["SectionName"]), Convert.ToString(r["SectionID"])));
        }

        protected void ddlClass_Changed(object sender, EventArgs e) { BindSections(); }

        private int? Sel(DropDownList d) { int v; return int.TryParse(d.SelectedValue, out v) && v > 0 ? v : (int?)null; }

        private void BindSummary()
        {
            DataRow r = _repo.PendingSetupSummary().Rows[0];
            litPending.Text = Convert.ToString(r["Pending"]);
            litReady.Text = Convert.ToString(r["Ready"]);
            litMissing.Text = Convert.ToString(r["MissingStructure"]);
            litCompleted.Text = Convert.ToString(r["CompletedToday"]);
        }

        private void BindQueue()
        {
            int total;
            DataTable dt = _repo.PendingSetupQueue(
                (txtSearch.Text ?? "").Trim(), Sel(ddlYear), Sel(ddlClass), Sel(ddlSection),
                ddlShift.SelectedValue, ddlStatus.SelectedValue, CurrentPage, PageSize, out total);

            rptQueue.DataSource = dt;
            rptQueue.DataBind();
            pnlEmpty.Visible = dt.Rows.Count == 0;

            int totalPages = total == 0 ? 1 : (int)Math.Ceiling(total / (double)PageSize);
            if (CurrentPage > totalPages) { CurrentPage = totalPages; }
            lblPageInfo.Text = total + " student(s) · Page " + CurrentPage + " of " + totalPages;
            btnPrev.Enabled = CurrentPage > 1;
            btnNext.Enabled = CurrentPage < totalPages;
        }

        protected void btnApply_Click(object sender, EventArgs e) { CurrentPage = 1; HideMessages(); BindSummary(); BindQueue(); }
        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = ""; ddlYear.SelectedIndex = 0; ddlClass.SelectedIndex = 0; BindSections();
            ddlSection.SelectedIndex = 0; ddlShift.SelectedIndex = 0; ddlStatus.SelectedIndex = 0;
            CurrentPage = 1; HideMessages(); BindSummary(); BindQueue();
        }
        protected void btnPrev_Click(object sender, EventArgs e) { if (CurrentPage > 1) CurrentPage--; BindQueue(); }
        protected void btnNext_Click(object sender, EventArgs e) { CurrentPage++; BindQueue(); }

        // ---- confirmation modal ----
        protected void rptQueue_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (!IsAuthorized) { Response.StatusCode = 403; Response.Redirect("~/Modules/Dashboard/Dashboard.aspx?denied=finance", true); return; }
            if (e.CommandName != "prepare") return;

            int studentId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out studentId)) return;
            PreparePreview(studentId);
        }

        private void PreparePreview(int studentId)
        {
            DataTable info = _repo.GetStudentPlacement(studentId);
            if (info.Rows.Count == 0) { ShowError("Student not found."); return; }
            DataRow s = info.Rows[0];

            hfStudentId.Value = studentId.ToString();
            lblCsName.Text = Server.HtmlEncode(Convert.ToString(s["StudentName"]));
            lblCsCode.Text = Server.HtmlEncode(Convert.ToString(s["StudentCode"]));
            lblCsYear.Text = Server.HtmlEncode(Convert.ToString(s["AcademicYear"]));
            lblCsClassSection.Text = Server.HtmlEncode(s["ClassName"] + " - " + s["SectionName"]);
            lblCsShift.Text = Server.HtmlEncode(Convert.ToString(s["Shift"]));

            DataTable items = _repo.GetApplicableStructures(studentId);
            rptPreview.DataSource = items;
            rptPreview.DataBind();

            decimal total = 0;
            foreach (DataRow r in items.Rows)
                total += Math.Max(0, Convert.ToDecimal(r["Amount"]) - Convert.ToDecimal(r["DiscountAmount"]));
            litPreviewTotal.Text = total.ToString("N2");

            if (string.IsNullOrEmpty(txtDueDate.Text)) txtDueDate.Text = DateTime.Today.AddDays(14).ToString("yyyy-MM-dd");
            pnlConfirm.Visible = true;
        }

        protected void btnCancelConfirm_Click(object sender, EventArgs e) { pnlConfirm.Visible = false; }

        protected void btnConfirmCreate_Click(object sender, EventArgs e)
        {
            if (!IsAuthorized) { Response.StatusCode = 403; Response.Redirect("~/Modules/Dashboard/Dashboard.aspx?denied=finance", true); return; }

            int studentId;
            if (!int.TryParse(hfStudentId.Value, out studentId)) { pnlConfirm.Visible = false; return; }

            DateTime invoiceDate = DateTime.Today;
            DateTime dueDate;
            if (!DateTime.TryParse(txtDueDate.Text, out dueDate)) { ShowConfirmError("Please provide a valid due date."); return; }
            if (dueDate.Date < invoiceDate.Date) { ShowConfirmError("The due date cannot be earlier than today."); return; }

            int actor;
            int.TryParse(Convert.ToString(Session["UserID"]), out actor);

            int invoiceId; string invoiceNumber; decimal total; string message;
            bool ok = _repo.CreateInitialInvoice(studentId, invoiceDate, dueDate, actor, Request.UserHostAddress,
                out invoiceId, out invoiceNumber, out total, out message);

            pnlConfirm.Visible = false;

            if (ok)
            {
                pnlMsgErr.Visible = false;
                pnlMsgOk.Visible = true;
                string view = ResolveUrl("~/Modules/Finance/ViewInvoice.aspx?id=" + invoiceId);
                string pay = ResolveUrl("~/Modules/Finance/RecordPayment.aspx?invoice=" + invoiceId);
                litMsgOk.Text =
                    "<p class='text-sm font-semibold' style='color:#15803D;'>Invoice " + Server.HtmlEncode(invoiceNumber) + " created — total " + total.ToString("N2") + ", due " + dueDate.ToString("MMM dd, yyyy") + ".</p>" +
                    "<div class='flex gap-2 mt-2 flex-wrap'>" +
                    "<a class='btn btn-secondary !py-1 !text-xs' href='" + view + "'>View Invoice</a>" +
                    "<a class='btn btn-secondary !py-1 !text-xs' href='" + pay + "'>Record Payment</a>" +
                    "<a class='btn btn-secondary !py-1 !text-xs' href='" + ResolveUrl("~/Modules/Finance/PendingFinanceSetup.aspx") + "'>Return to Pending Queue</a>" +
                    "</div>";
            }
            else
            {
                ShowError(message ?? "The invoice could not be created.");
            }

            BindSummary();
            BindQueue();
        }

        // ---- render helpers (used by markup) ----
        public string StatusStyle(string status)
        {
            switch (status)
            {
                case "Ready": return "background:#DCFCE7;color:#15803D";
                case "No Matching Fee Structure": return "background:#FEF3C7;color:#B45309";
                case "Multiple Fee Structures": return "background:#FEE2E2;color:#B91C1C";
                case "Invoice Already Exists": return "background:#DBEAFE;color:#1D4ED8";
                default: return "background:#F1F5F9;color:#64748B";
            }
        }

        public string LineTotal(object dataItem)
        {
            DataRowView r = (DataRowView)dataItem;
            decimal amount = Convert.ToDecimal(r["Amount"]);
            decimal disc = Convert.ToDecimal(r["DiscountAmount"]);
            return Math.Max(0, amount - disc).ToString("N2");
        }

        private void HideMessages() { pnlMsgOk.Visible = false; pnlMsgErr.Visible = false; }
        private void ShowError(string msg) { pnlMsgOk.Visible = false; pnlMsgErr.Visible = true; lblMsgErr.Text = Server.HtmlEncode(msg); }
        private void ShowConfirmError(string msg) { pnlConfirm.Visible = true; ShowError(msg); }
    }
}
