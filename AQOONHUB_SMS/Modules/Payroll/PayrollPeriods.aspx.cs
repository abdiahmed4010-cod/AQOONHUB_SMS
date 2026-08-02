using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Payroll
{
    public partial class PayrollPeriods : System.Web.UI.Page
    {
        private readonly PayrollRepository _repository =
            new PayrollRepository();

        private const string DefaultSortColumn = "StartDate";
        private const string DefaultSortDirection = "DESC";
        private const string DateInputFormat = "yyyy-MM-dd";

        private static readonly HashSet<string> AllowedSortColumns =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "PeriodName",
                "StartDate",
                "EndDate",
                "PaymentDate",
                "Status",
                "CreatedAt"
            };

        private string PeriodSortColumn
        {
            get
            {
                string value =
                    Convert.ToString(ViewState["PeriodSortColumn"]);

                return AllowedSortColumns.Contains(value)
                    ? value
                    : DefaultSortColumn;
            }
            set
            {
                ViewState["PeriodSortColumn"] =
                    AllowedSortColumns.Contains(value)
                        ? value
                        : DefaultSortColumn;
            }
        }

        private string PeriodSortDirection
        {
            get
            {
                string value =
                    Convert.ToString(ViewState["PeriodSortDirection"]);

                return string.Equals(
                    value,
                    "ASC",
                    StringComparison.OrdinalIgnoreCase)
                    ? "ASC"
                    : "DESC";
            }
            set
            {
                ViewState["PeriodSortDirection"] =
                    string.Equals(
                        value,
                        "ASC",
                        StringComparison.OrdinalIgnoreCase)
                        ? "ASC"
                        : "DESC";
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!EnsureAuthorized())
            {
                return;
            }

            if (!IsPostBack)
            {
                PeriodSortColumn = DefaultSortColumn;
                PeriodSortDirection = DefaultSortDirection;
                BindPayrollPeriods();
            }
        }

        private bool EnsureAuthorized()
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect(
                    ResolveUrl(
                        "~/Modules/Authentication/Login.aspx"),
                    false);
                Context.ApplicationInstance.CompleteRequest();
                return false;
            }

            int? currentUserId = GetCurrentUserId();

            if (!currentUserId.HasValue)
            {
                Session.Clear();

                Response.Redirect(
                    ResolveUrl(
                        "~/Modules/Authentication/Login.aspx"),
                    false);
                Context.ApplicationInstance.CompleteRequest();
                return false;
            }

            string role = _repository.NormalizeRole(
                Convert.ToString(Session["Role"]));

            bool authorized =
                role == "superadmin" ||
                role == "admin" ||
                role == "accountant" ||
                role == "finance";

            if (!authorized)
            {
                Response.Redirect(
                    ResolveUrl("~/Modules/Dashboard/Dashboard.aspx?denied=payroll"),
                    false);
                Context.ApplicationInstance.CompleteRequest();
                return false;
            }

            return true;
        }

        private int? GetCurrentUserId()
        {
            object sessionValue = Session["UserID"];

            if (sessionValue == null ||
                sessionValue == DBNull.Value)
            {
                return null;
            }

            int userId;

            if (int.TryParse(
                    Convert.ToString(sessionValue),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out userId) &&
                userId > 0)
            {
                return userId;
            }

            return null;
        }

        private void BindPayrollPeriods()
        {
            try
            {
                DataTable periods =
                    _repository.GetPayrollPeriods() ??
                    new DataTable();

                BindSummaryCards(periods);

                DataTable filteredPeriods =
                    ApplyPeriodFilters(periods);

                DataView view =
                    new DataView(filteredPeriods);

                view.Sort =
                    PeriodSortColumn + " " +
                    PeriodSortDirection;

                if (gvPayrollPeriods.PageIndex > 0 &&
                    filteredPeriods.Rows.Count <=
                    gvPayrollPeriods.PageIndex *
                    gvPayrollPeriods.PageSize)
                {
                    gvPayrollPeriods.PageIndex = 0;
                }

                gvPayrollPeriods.DataSource = view;
                gvPayrollPeriods.DataBind();
            }
            catch (Exception ex)
            {
                gvPayrollPeriods.DataSource = null;
                gvPayrollPeriods.DataBind();

                ShowError(GetSafeErrorMessage(
                    ex,
                    "Payroll periods could not be loaded."));
            }
        }

        private DataTable ApplyPeriodFilters(
            DataTable periods)
        {
            if (periods == null)
            {
                return new DataTable();
            }

            DataTable filtered = periods.Clone();

            string selectedStatus =
                (ddlStatusFilter.SelectedValue ??
                 string.Empty).Trim();

            string search =
                (txtSearch.Text ??
                 string.Empty).Trim();

            foreach (DataRow row in periods.Rows)
            {
                string status = GetString(row, "Status");

                if (!string.IsNullOrEmpty(selectedStatus) &&
                    !string.Equals(
                        status,
                        selectedStatus,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!MatchesSearch(row, search))
                {
                    continue;
                }

                filtered.ImportRow(row);
            }

            return filtered;
        }

        private void BindSummaryCards(DataTable periods)
        {
            int total = 0;
            int draft = 0;
            int processing = 0;
            int completed = 0;

            if (periods != null)
            {
                total = periods.Rows.Count;

                foreach (DataRow row in periods.Rows)
                {
                    string status =
                        GetString(row, "Status");

                    if (string.Equals(
                        status,
                        "Draft",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        draft++;
                    }
                    else if (string.Equals(
                        status,
                        "Processing",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        processing++;
                    }
                    else if (string.Equals(
                        status,
                        "Completed",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        completed++;
                    }
                }
            }

            lblTotalPeriods.Text =
                total.ToString(
                    "N0",
                    CultureInfo.CurrentCulture);

            lblDraftCount.Text =
                draft.ToString(
                    "N0",
                    CultureInfo.CurrentCulture);

            lblProcessingCount.Text =
                processing.ToString(
                    "N0",
                    CultureInfo.CurrentCulture);

            lblCompletedCount.Text =
                completed.ToString(
                    "N0",
                    CultureInfo.CurrentCulture);
        }

        private void OpenNewPeriodModal()
        {
            ClearPeriodForm();
            hfPayrollPeriodID.Value = string.Empty;
            lblPeriodModalTitle.Text = "New Payroll Period";
            pnlPeriodModal.Visible = true;
        }

        private void OpenEditPeriodModal(
            int payrollPeriodId)
        {
            PayrollPeriodData period =
                _repository.GetPayrollPeriod(
                    payrollPeriodId);

            if (period == null)
            {
                ShowError(
                    "The selected payroll period was not found.");
                return;
            }

            ClearPeriodForm();

            hfPayrollPeriodID.Value =
                period.PayrollPeriodID.ToString(
                    CultureInfo.InvariantCulture);

            txtPeriodName.Text =
                period.PeriodName ?? string.Empty;

            txtStartDate.Text =
                period.StartDate.ToString(
                    DateInputFormat,
                    CultureInfo.InvariantCulture);

            txtEndDate.Text =
                period.EndDate.ToString(
                    DateInputFormat,
                    CultureInfo.InvariantCulture);

            txtPaymentDate.Text =
                period.PaymentDate.HasValue
                    ? period.PaymentDate.Value.ToString(
                        DateInputFormat,
                        CultureInfo.InvariantCulture)
                    : string.Empty;

            lblPeriodModalTitle.Text =
                "Edit Payroll Period";

            pnlPeriodModal.Visible = true;
        }

        private void ClosePeriodModal()
        {
            pnlPeriodModal.Visible = false;
            ClearPeriodForm();
        }

        private void OpenStatusModal(
            int payrollPeriodId,
            string requestedStatus)
        {
            if (payrollPeriodId <= 0 ||
                !IsAllowedRequestedStatus(requestedStatus))
            {
                ShowError(
                    "The requested payroll status change is invalid.");
                return;
            }

            PayrollPeriodData period =
                _repository.GetPayrollPeriod(
                    payrollPeriodId);

            if (period == null)
            {
                ShowError(
                    "The selected payroll period was not found.");
                return;
            }

            if (!IsAllowedTransition(
                period.Status,
                requestedStatus))
            {
                ShowWarning(
                    "The requested payroll period status transition is not allowed.");
                return;
            }

            hfStatusPayrollPeriodID.Value =
                payrollPeriodId.ToString(
                    CultureInfo.InvariantCulture);

            hfRequestedStatus.Value =
                requestedStatus;

            string title;
            string message;

            switch (requestedStatus)
            {
                case "Processing":
                    title = "Start Payroll Processing";
                    message =
                        "Start processing the payroll period \"" +
                        (period.PeriodName ?? string.Empty) +
                        "\"? The period will no longer be editable.";
                    break;

                case "Completed":
                    title = "Complete Payroll Period";
                    message =
                        "Complete the payroll period \"" +
                        (period.PeriodName ?? string.Empty) +
                        "\"? Completion requires payroll records and no Pending or Failed payments.";
                    break;

                case "Cancelled":
                    title = "Cancel Payroll Period";
                    message =
                        "Cancel the payroll period \"" +
                        (period.PeriodName ?? string.Empty) +
                        "\"? This status change cannot be reversed.";
                    break;

                default:
                    ShowError(
                        "The requested payroll status change is invalid.");
                    return;
            }

            lblStatusModalTitle.Text =
                HttpUtility.HtmlEncode(title);

            lblStatusModalMessage.Text =
                HttpUtility.HtmlEncode(message);

            pnlStatusModal.Visible = true;
        }

        private void CloseStatusModal()
        {
            pnlStatusModal.Visible = false;
            hfStatusPayrollPeriodID.Value = string.Empty;
            hfRequestedStatus.Value = string.Empty;
            lblStatusModalTitle.Text = string.Empty;
            lblStatusModalMessage.Text = string.Empty;
        }

        private PayrollPeriodInput BuildPayrollPeriodInput()
        {
            string periodName =
                (txtPeriodName.Text ??
                 string.Empty).Trim();

            if (periodName.Length == 0)
            {
                throw new InvalidOperationException(
                    "Period name is required.");
            }

            if (periodName.Length > 100)
            {
                throw new InvalidOperationException(
                    "Period name cannot exceed 100 characters.");
            }

            DateTime startDate;
            DateTime endDate;

            if (!TryParseInputDate(
                txtStartDate.Text,
                out startDate))
            {
                throw new InvalidOperationException(
                    "Enter a valid start date.");
            }

            if (!TryParseInputDate(
                txtEndDate.Text,
                out endDate))
            {
                throw new InvalidOperationException(
                    "Enter a valid end date.");
            }

            if (endDate.Date < startDate.Date)
            {
                throw new InvalidOperationException(
                    "End date cannot be earlier than start date.");
            }

            DateTime? paymentDate = null;

            if (!string.IsNullOrWhiteSpace(
                txtPaymentDate.Text))
            {
                DateTime parsedPaymentDate;

                if (!TryParseInputDate(
                    txtPaymentDate.Text,
                    out parsedPaymentDate))
                {
                    throw new InvalidOperationException(
                        "Enter a valid payment date.");
                }

                if (parsedPaymentDate.Date <
                    startDate.Date)
                {
                    throw new InvalidOperationException(
                        "Payment date cannot be earlier than start date.");
                }

                paymentDate =
                    parsedPaymentDate.Date;
            }

            int? payrollPeriodId = null;

            if (!string.IsNullOrWhiteSpace(
                hfPayrollPeriodID.Value))
            {
                payrollPeriodId =
                    ParsePositiveInt(
                        hfPayrollPeriodID.Value);

                if (!payrollPeriodId.HasValue)
                {
                    throw new InvalidOperationException(
                        "The payroll period identifier is invalid.");
                }
            }

            return new PayrollPeriodInput
            {
                PayrollPeriodID = payrollPeriodId,
                PeriodName = periodName,
                StartDate = startDate.Date,
                EndDate = endDate.Date,
                PaymentDate = paymentDate
            };
        }

        private void ClearPeriodForm()
        {
            hfPayrollPeriodID.Value = string.Empty;
            txtPeriodName.Text = string.Empty;
            txtStartDate.Text = string.Empty;
            txtEndDate.Text = string.Empty;
            txtPaymentDate.Text = string.Empty;
        }

        private void ShowSuccess(string message)
        {
            SetMessage(
                message,
                "rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-emerald-800 shadow-sm");
        }

        private void ShowWarning(string message)
        {
            SetMessage(
                message,
                "rounded-xl border border-amber-200 bg-amber-50 px-4 py-3 text-amber-800 shadow-sm");
        }

        private void ShowError(string message)
        {
            SetMessage(
                message,
                "rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-rose-800 shadow-sm");
        }

        private void ClearMessage()
        {
            pnlMessage.Visible = false;
            lblMessage.Text = string.Empty;
        }

        private void SetMessage(
            string message,
            string cssClass)
        {
            pnlMessage.Visible = true;
            pnlMessage.CssClass = cssClass;

            lblMessage.Text = HttpUtility.HtmlEncode(
                string.IsNullOrWhiteSpace(message)
                    ? "An unexpected error occurred."
                    : message.Trim());
        }

        private static string GetSafeErrorMessage(
            Exception exception,
            string fallbackMessage)
        {
            if (exception == null ||
                string.IsNullOrWhiteSpace(
                    exception.Message))
            {
                return fallbackMessage;
            }

            string message =
                exception.Message.Trim();

            if (message.Length > 500)
            {
                message =
                    message.Substring(0, 500);
            }

            string lowerMessage =
                message.ToLowerInvariant();

            if (lowerMessage.Contains("stack trace") ||
                lowerMessage.Contains("system.data.sqlclient") ||
                lowerMessage.Contains(" at system.") ||
                lowerMessage.Contains("connection string") ||
                lowerMessage.Contains("server=") ||
                lowerMessage.Contains("data source=") ||
                lowerMessage.Contains("select ") ||
                lowerMessage.Contains("insert ") ||
                lowerMessage.Contains("update ") ||
                lowerMessage.Contains("delete ") ||
                lowerMessage.Contains("dbo."))
            {
                return fallbackMessage;
            }

            return message;
        }

        private static int? ParsePositiveInt(
            string value)
        {
            int parsedValue;

            if (int.TryParse(
                    value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out parsedValue) &&
                parsedValue > 0)
            {
                return parsedValue;
            }

            return null;
        }

        private static int GetInt32(
            DataRow row,
            string columnName)
        {
            if (!HasValue(row, columnName))
            {
                return 0;
            }

            int value;

            return int.TryParse(
                Convert.ToString(
                    row[columnName],
                    CultureInfo.InvariantCulture),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out value)
                ? value
                : 0;
        }

        private static string GetString(
            DataRow row,
            string columnName)
        {
            if (!HasValue(row, columnName))
            {
                return string.Empty;
            }

            return Convert.ToString(
                row[columnName],
                CultureInfo.CurrentCulture).Trim();
        }

        private static DateTime GetDateTime(
            DataRow row,
            string columnName)
        {
            DateTime value;

            return HasValue(row, columnName) &&
                DateTime.TryParse(
                    Convert.ToString(
                        row[columnName],
                        CultureInfo.InvariantCulture),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out value)
                ? value
                : DateTime.MinValue;
        }

        private static DateTime? GetNullableDateTime(
            DataRow row,
            string columnName)
        {
            DateTime value = GetDateTime(
                row,
                columnName);

            return value == DateTime.MinValue
                ? (DateTime?)null
                : value;
        }

        protected string GetPeriodStatusCss(
            string status)
        {
            const string common =
                "inline-flex items-center rounded-full px-2.5 py-1 text-xs font-semibold ";

            switch ((status ?? string.Empty)
                .Trim()
                .ToLowerInvariant())
            {
                case "draft":
                    return common +
                        "bg-blue-100 text-blue-700";

                case "processing":
                    return common +
                        "bg-amber-100 text-amber-700";

                case "completed":
                    return common +
                        "bg-emerald-100 text-emerald-700";

                case "cancelled":
                    return common +
                        "bg-rose-100 text-rose-700";

                default:
                    return common +
                        "bg-slate-100 text-slate-700";
            }
        }

        protected bool CanShowPeriodAction(
            string status,
            string action)
        {
            string normalizedStatus =
                (status ?? string.Empty)
                .Trim()
                .ToLowerInvariant();

            string normalizedAction =
                (action ?? string.Empty).Trim();

            if (normalizedStatus == "draft")
            {
                return normalizedAction == "EditPeriod" ||
                    normalizedAction == "StartProcessing" ||
                    normalizedAction == "CancelPeriod";
            }

            if (normalizedStatus == "processing")
            {
                return normalizedAction == "CompletePeriod" ||
                    normalizedAction == "CancelPeriod";
            }

            return false;
        }

        protected void btnNewPeriod_Click(
            object sender,
            EventArgs e)
        {
            ClearMessage();
            OpenNewPeriodModal();
        }

        protected void btnSearch_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                ClearMessage();
                gvPayrollPeriods.PageIndex = 0;
                BindPayrollPeriods();
            }
            catch (Exception ex)
            {
                ShowError(GetSafeErrorMessage(
                    ex,
                    "The payroll period search could not be completed."));
            }
        }

        protected void btnReset_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                ClearMessage();

                if (ddlStatusFilter.Items.Count > 0)
                {
                    ddlStatusFilter.SelectedIndex = 0;
                }

                txtSearch.Text = string.Empty;
                PeriodSortColumn = DefaultSortColumn;
                PeriodSortDirection =
                    DefaultSortDirection;
                gvPayrollPeriods.PageIndex = 0;

                BindPayrollPeriods();
            }
            catch (Exception ex)
            {
                ShowError(GetSafeErrorMessage(
                    ex,
                    "The payroll period filters could not be reset."));
            }
        }

        protected void btnSavePeriod_Click(
            object sender,
            EventArgs e)
        {
            Page.Validate("PayrollPeriodForm");

            if (!Page.IsValid)
            {
                pnlPeriodModal.Visible = true;
                return;
            }

            try
            {
                PayrollPeriodInput input =
                    BuildPayrollPeriodInput();

                bool isNew =
                    !input.PayrollPeriodID.HasValue;

                _repository.SavePayrollPeriod(
                    input,
                    GetCurrentUserId());

                ClosePeriodModal();
                gvPayrollPeriods.PageIndex = 0;
                BindPayrollPeriods();

                ShowSuccess(
                    isNew
                        ? "Payroll period created successfully."
                        : "Payroll period updated successfully.");
            }
            catch (Exception ex)
            {
                pnlPeriodModal.Visible = true;

                ShowError(GetSafeErrorMessage(
                    ex,
                    "The payroll period could not be saved."));
            }
        }

        protected void btnClosePeriodModal_Click(
            object sender,
            EventArgs e)
        {
            ClosePeriodModal();
        }

        protected void gvPayrollPeriods_RowCommand(
            object sender,
            GridViewCommandEventArgs e)
        {
            string commandName =
                e.CommandName ?? string.Empty;

            if (commandName != "EditPeriod" &&
                commandName != "StartProcessing" &&
                commandName != "CompletePeriod" &&
                commandName != "CancelPeriod")
            {
                return;
            }

            int? payrollPeriodId =
                ParsePositiveInt(
                    Convert.ToString(
                        e.CommandArgument,
                        CultureInfo.InvariantCulture));

            if (!payrollPeriodId.HasValue)
            {
                ShowError(
                    "The selected payroll period identifier is invalid.");
                return;
            }

            try
            {
                ClearMessage();

                switch (commandName)
                {
                    case "EditPeriod":
                        OpenEditPeriodModal(
                            payrollPeriodId.Value);
                        break;

                    case "StartProcessing":
                        OpenStatusModal(
                            payrollPeriodId.Value,
                            "Processing");
                        break;

                    case "CompletePeriod":
                        OpenStatusModal(
                            payrollPeriodId.Value,
                            "Completed");
                        break;

                    case "CancelPeriod":
                        OpenStatusModal(
                            payrollPeriodId.Value,
                            "Cancelled");
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowError(GetSafeErrorMessage(
                    ex,
                    "The selected payroll period action could not be opened."));
            }
        }

        protected void gvPayrollPeriods_PageIndexChanging(
            object sender,
            GridViewPageEventArgs e)
        {
            if (e.NewPageIndex < 0)
            {
                return;
            }

            try
            {
                ClearMessage();
                gvPayrollPeriods.PageIndex =
                    e.NewPageIndex;
                BindPayrollPeriods();
            }
            catch (Exception ex)
            {
                ShowError(GetSafeErrorMessage(
                    ex,
                    "The selected payroll period page could not be loaded."));
            }
        }

        protected void gvPayrollPeriods_Sorting(
            object sender,
            GridViewSortEventArgs e)
        {
            string requestedColumn =
                e.SortExpression ?? string.Empty;

            if (!AllowedSortColumns.Contains(
                requestedColumn))
            {
                ShowWarning(
                    "The selected payroll period sorting option is not available.");
                return;
            }

            if (string.Equals(
                PeriodSortColumn,
                requestedColumn,
                StringComparison.Ordinal))
            {
                PeriodSortDirection =
                    PeriodSortDirection == "ASC"
                        ? "DESC"
                        : "ASC";
            }
            else
            {
                PeriodSortColumn =
                    requestedColumn;

                PeriodSortDirection = "ASC";
            }

            try
            {
                ClearMessage();
                gvPayrollPeriods.PageIndex = 0;
                BindPayrollPeriods();
            }
            catch (Exception ex)
            {
                ShowError(GetSafeErrorMessage(
                    ex,
                    "The payroll periods could not be sorted."));
            }
        }

        protected void btnConfirmStatus_Click(
            object sender,
            EventArgs e)
        {
            int? payrollPeriodId =
                ParsePositiveInt(
                    hfStatusPayrollPeriodID.Value);

            string requestedStatus =
                (hfRequestedStatus.Value ??
                 string.Empty).Trim();

            if (!payrollPeriodId.HasValue ||
                !IsAllowedRequestedStatus(
                    requestedStatus))
            {
                CloseStatusModal();
                ShowError(
                    "The requested payroll status change is invalid.");
                return;
            }

            try
            {
                PayrollPeriodData currentPeriod =
                    _repository.GetPayrollPeriod(
                        payrollPeriodId.Value);

                if (currentPeriod == null)
                {
                    CloseStatusModal();
                    ShowError(
                        "The selected payroll period was not found.");
                    return;
                }

                if (!IsAllowedTransition(
                    currentPeriod.Status,
                    requestedStatus))
                {
                    CloseStatusModal();
                    ShowWarning(
                        "The requested payroll period status transition is not allowed.");
                    return;
                }

                _repository.SetPayrollPeriodStatus(
                    payrollPeriodId.Value,
                    requestedStatus,
                    GetCurrentUserId());

                CloseStatusModal();
                gvPayrollPeriods.PageIndex = 0;
                BindPayrollPeriods();

                ShowSuccess(
                    "Payroll period status changed to " +
                    requestedStatus +
                    " successfully.");
            }
            catch (Exception ex)
            {
                pnlStatusModal.Visible = true;

                ShowError(GetSafeErrorMessage(
                    ex,
                    "The payroll period status could not be changed."));
            }
        }

        protected void btnCancelStatus_Click(
            object sender,
            EventArgs e)
        {
            CloseStatusModal();
        }

        private static bool MatchesSearch(
            DataRow row,
            string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return true;
            }

            string value =
                search.Trim();

            if (ContainsIgnoreCase(
                    GetString(row, "PeriodName"),
                    value) ||
                ContainsIgnoreCase(
                    GetString(row, "Status"),
                    value))
            {
                return true;
            }

            DateTime startDate =
                GetDateTime(row, "StartDate");

            DateTime endDate =
                GetDateTime(row, "EndDate");

            DateTime? paymentDate =
                GetNullableDateTime(
                    row,
                    "PaymentDate");

            return DateMatchesSearch(
                    startDate,
                    value) ||
                DateMatchesSearch(
                    endDate,
                    value) ||
                (paymentDate.HasValue &&
                 DateMatchesSearch(
                     paymentDate.Value,
                     value));
        }

        private static bool DateMatchesSearch(
            DateTime date,
            string search)
        {
            if (date == DateTime.MinValue)
            {
                return false;
            }

            return ContainsIgnoreCase(
                    date.ToString(
                        "dd MMM yyyy",
                        CultureInfo.CurrentCulture),
                    search) ||
                ContainsIgnoreCase(
                    date.ToString(
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture),
                    search) ||
                ContainsIgnoreCase(
                    date.ToString(
                        "dd/MM/yyyy",
                        CultureInfo.InvariantCulture),
                    search);
        }

        private static bool ContainsIgnoreCase(
            string source,
            string value)
        {
            return !string.IsNullOrEmpty(source) &&
                !string.IsNullOrEmpty(value) &&
                source.IndexOf(
                    value,
                    StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private static bool TryParseInputDate(
            string value,
            out DateTime date)
        {
            return DateTime.TryParseExact(
                    (value ?? string.Empty).Trim(),
                    DateInputFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out date) ||
                DateTime.TryParse(
                    value,
                    CultureInfo.CurrentCulture,
                    DateTimeStyles.None,
                    out date);
        }

        private static bool HasValue(
            DataRow row,
            string columnName)
        {
            return row != null &&
                row.Table != null &&
                row.Table.Columns.Contains(
                    columnName) &&
                row[columnName] != DBNull.Value;
        }

        private static bool IsAllowedRequestedStatus(
            string status)
        {
            return status == "Processing" ||
                status == "Completed" ||
                status == "Cancelled";
        }

        private static bool IsAllowedTransition(
            string currentStatus,
            string requestedStatus)
        {
            return
                (string.Equals(
                    currentStatus,
                    "Draft",
                    StringComparison.OrdinalIgnoreCase) &&
                 (requestedStatus == "Processing" ||
                  requestedStatus == "Cancelled")) ||
                (string.Equals(
                    currentStatus,
                    "Processing",
                    StringComparison.OrdinalIgnoreCase) &&
                 (requestedStatus == "Completed" ||
                  requestedStatus == "Cancelled"));
        }
    }
}
