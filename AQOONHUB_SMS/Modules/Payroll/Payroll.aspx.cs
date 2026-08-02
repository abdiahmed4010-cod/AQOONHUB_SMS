using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
namespace AQOONHUB_SMS.Modules.Payroll
{
    public partial class Payroll : System.Web.UI.Page
    {
        private readonly PayrollRepository _repository =
            new PayrollRepository();
        private const string DefaultSortColumn = "PeriodName";
        private const string DefaultSortDirection = "DESC";

        private static readonly HashSet<string> AllowedSortColumns =
            new HashSet<string>(StringComparer.Ordinal)
            {
            "PeriodName",
            "EmployeeID",
            "Department",
            "Position",
            "BasicSalary",
            "GrossSalary",
            "TotalDeductions",
            "NetSalary",
            "PaymentStatus",
            "PaidDate"
            };

        private string SortColumn
        {
            get
            {
                string value = Convert.ToString(ViewState["SortColumn"]);

                return AllowedSortColumns.Contains(value)
                    ? value
                    : DefaultSortColumn;
            }
            set
            {
                ViewState["SortColumn"] =
                    AllowedSortColumns.Contains(value)
                        ? value
                        : DefaultSortColumn;
            }
        }

        private string SortDirection
        {
            get
            {
                string value = Convert.ToString(ViewState["SortDirection"]);

                return string.Equals(
                    value,
                    "ASC",
                    StringComparison.OrdinalIgnoreCase)
                    ? "ASC"
                    : "DESC";
            }
            set
            {
                ViewState["SortDirection"] = string.Equals(
                    value,
                    "ASC",
                    StringComparison.OrdinalIgnoreCase)
                    ? "ASC"
                    : "DESC";
            }
        }

        private int CurrentPageIndex
        {
            get
            {
                int pageIndex;

                return int.TryParse(
                    Convert.ToString(ViewState["CurrentPageIndex"]),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out pageIndex) &&
                    pageIndex >= 0
                    ? pageIndex
                    : 0;
            }
            set
            {
                ViewState["CurrentPageIndex"] = Math.Max(0, value);
            }
        }

        private int TotalPageCount
        {
            get
            {
                int pageCount;

                return int.TryParse(
                    Convert.ToString(ViewState["TotalPageCount"]),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out pageCount) &&
                    pageCount >= 0
                    ? pageCount
                    : 0;
            }
            set
            {
                ViewState["TotalPageCount"] = Math.Max(0, value);
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
                SortColumn = DefaultSortColumn;
                SortDirection = DefaultSortDirection;
                CurrentPageIndex = 0;
                BindInitialData();
            }
        }

        private bool EnsureAuthorized()
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect(
                    ResolveUrl("~/Modules/Authentication/Login.aspx"),
                    false);
                Context.ApplicationInstance.CompleteRequest();
                return false;
            }

            int? currentUserId = GetCurrentUserId();

            if (!currentUserId.HasValue)
            {
                Session.Clear();
                Response.Redirect(
                    ResolveUrl("~/Modules/Authentication/Login.aspx"),
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

        private void BindInitialData()
        {
            try
            {
                ClearMessage();
                BindPayrollPeriods();
                BindDepartments();
                BindPayrollRecords();
                BindPayrollSummary();
            }
            catch (Exception ex)
            {
                ShowError(GetSafeErrorMessage(
                    ex,
                    "Payroll information could not be loaded."));
            }
        }

        private void BindPayrollPeriods()
        {
            string selectedFilterValue =
                ddlPayrollPeriod.Items.Count > 0
                    ? ddlPayrollPeriod.SelectedValue
                    : string.Empty;

            DataTable periods = _repository.GetPayrollPeriods();

            ddlPayrollPeriod.Items.Clear();
            ddlPayrollPeriod.Items.Add(
                new ListItem("All Payroll Periods", string.Empty));

            foreach (DataRow row in periods.Rows)
            {
                int payrollPeriodId = GetInt32(
                    row,
                    "PayrollPeriodID");

                string periodName = GetString(
                    row,
                    "PeriodName");

                if (payrollPeriodId <= 0 ||
                    string.IsNullOrWhiteSpace(periodName))
                {
                    continue;
                }

                string value = payrollPeriodId.ToString(
                    CultureInfo.InvariantCulture);

                ddlPayrollPeriod.Items.Add(
                    new ListItem(periodName, value));
            }

            RestoreSelection(
                ddlPayrollPeriod,
                selectedFilterValue);
        }

        private void BindDepartments()
        {
            string selectedValue =
                ddlDepartment.Items.Count > 0
                    ? ddlDepartment.SelectedValue
                    : string.Empty;

            DataTable staff = _repository.GetEligibleStaff();

            // Total active/eligible employees for the dashboard KPI.
            lblTotalEmployees.Text = staff.Rows.Count.ToString("N0");

            SortedSet<string> departments =
                new SortedSet<string>(
                    StringComparer.CurrentCultureIgnoreCase);

            foreach (DataRow row in staff.Rows)
            {
                string department = GetString(
                    row,
                    "Department");

                if (!string.IsNullOrWhiteSpace(department))
                {
                    departments.Add(department.Trim());
                }
            }

            ddlDepartment.Items.Clear();
            ddlDepartment.Items.Add(
                new ListItem("All Departments", string.Empty));

            foreach (string department in departments)
            {
                ddlDepartment.Items.Add(
                    new ListItem(department, department));
            }

            RestoreSelection(
                ddlDepartment,
                selectedValue);
        }

        private void BindPayrollRecords()
        {
            PayrollListFilter filter = BuildFilter();

            PayrollListResult result =
                _repository.GetPayrollRecords(
                    filter,
                    CurrentPageIndex + 1,
                    gvPayroll.PageSize,
                    SortColumn,
                    SortDirection);

            if (result == null)
            {
                result = new PayrollListResult
                {
                    Records = new DataTable(),
                    TotalRecords = 0,
                    PageNumber = 1,
                    PageSize = gvPayroll.PageSize
                };
            }

            TotalPageCount = result.TotalPages;

            if (TotalPageCount > 0 &&
                CurrentPageIndex >= TotalPageCount)
            {
                CurrentPageIndex = TotalPageCount - 1;

                result = _repository.GetPayrollRecords(
                    filter,
                    CurrentPageIndex + 1,
                    gvPayroll.PageSize,
                    SortColumn,
                    SortDirection);

                TotalPageCount = result.TotalPages;
            }

            gvPayroll.AllowPaging = false;
            gvPayroll.DataSource =
                result.Records ?? new DataTable();
            gvPayroll.DataBind();

            long firstRecord =
                result.TotalRecords == 0
                    ? 0
                    : ((long)CurrentPageIndex *
                       gvPayroll.PageSize) + 1;

            long lastRecord = Math.Min(
                result.TotalRecords,
                ((long)CurrentPageIndex + 1) *
                gvPayroll.PageSize);

            lblPagingSummary.Text = string.Format(
                CultureInfo.CurrentCulture,
                "Showing {0:N0}–{1:N0} of {2:N0} records",
                firstRecord,
                lastRecord,
                result.TotalRecords);

            int displayedPage =
                TotalPageCount == 0
                    ? 0
                    : CurrentPageIndex + 1;

            lblPageNumber.Text = string.Format(
                CultureInfo.CurrentCulture,
                "Page {0:N0} of {1:N0}",
                displayedPage,
                TotalPageCount);

            btnPreviousPage.Enabled =
                CurrentPageIndex > 0;

            btnNextPage.Enabled =
                TotalPageCount > 0 &&
                CurrentPageIndex + 1 < TotalPageCount;

            ApplyPagerButtonState(btnPreviousPage);
            ApplyPagerButtonState(btnNextPage);
        }

        private void BindPayrollSummary()
        {
            int? payrollPeriodId =
                ParsePositiveInt(
                    ddlPayrollPeriod.SelectedValue);

            string department =
                (ddlDepartment.SelectedValue ??
                 string.Empty).Trim();

            DataTable summary =
                _repository.GetPayrollSummary(
                    payrollPeriodId,
                    department);

            if (summary == null ||
                summary.Rows.Count == 0)
            {
                SetEmptySummary();
                return;
            }

            DataRow row = summary.Rows[0];

            lblRecordCount.Text =
                GetInt64(row, "RecordCount").ToString(
                    "N0",
                    CultureInfo.CurrentCulture);

            lblGrossSalary.Text =
                FormatMoney(
                    GetDecimal(row, "GrossSalary"));

            lblTotalDeductions.Text =
                FormatMoney(
                    GetDecimal(row, "TotalDeductions"));

            lblNetSalary.Text =
                FormatMoney(
                    GetDecimal(row, "NetSalary"));

            lblPaidAmount.Text =
                FormatMoney(
                    GetDecimal(row, "Paid"));

            lblPendingAmount.Text =
                FormatMoney(
                    GetDecimal(row, "Pending"));

            // Mirror the figures into the Overview "Payroll Summary" panel.
            litGross2.Text = lblGrossSalary.Text;
            litNet2.Text = lblNetSalary.Text;
            litPaid2.Text = lblPaidAmount.Text;
            litPending2.Text = lblPendingAmount.Text;
        }

        private PayrollListFilter BuildFilter()
        {
            return new PayrollListFilter
            {
                PayrollPeriodId =
                    ParsePositiveInt(
                        ddlPayrollPeriod.SelectedValue),
                Department =
                    (ddlDepartment.SelectedValue ??
                     string.Empty).Trim(),
                PaymentStatus =
                    (ddlPaymentStatus.SelectedValue ??
                     string.Empty).Trim(),
                Search =
                    (txtSearch.Text ??
                     string.Empty).Trim()
            };
        }

        private void ShowSuccess(string message)
        {
            SetMessage(
                message,
                "rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-emerald-800 shadow-sm");
        }

        private void ShowError(string message)
        {
            SetMessage(
                message,
                "rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-rose-800 shadow-sm");
        }

        private void ShowWarning(string message)
        {
            SetMessage(
                message,
                "rounded-xl border border-amber-200 bg-amber-50 px-4 py-3 text-amber-800 shadow-sm");
        }

        private void ClearMessage()
        {
            pnlMessage.Visible = false;
            lblMessage.Text = string.Empty;
        }

        private string FormatMoney(decimal amount)
        {
            return PayrollFormat.Money(amount);
        }

        protected void btnSearch_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                ClearMessage();
                CurrentPageIndex = 0;
                BindPayrollRecords();
                BindPayrollSummary();
            }
            catch (Exception ex)
            {
                ShowError(GetSafeErrorMessage(
                    ex,
                    "The payroll search could not be completed."));
            }
        }

        protected void btnReset_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                ClearMessage();

                if (ddlPayrollPeriod.Items.Count > 0)
                {
                    ddlPayrollPeriod.SelectedIndex = 0;
                }

                if (ddlDepartment.Items.Count > 0)
                {
                    ddlDepartment.SelectedIndex = 0;
                }

                if (ddlPaymentStatus.Items.Count > 0)
                {
                    ddlPaymentStatus.SelectedIndex = 0;
                }

                txtSearch.Text = string.Empty;
                SortColumn = DefaultSortColumn;
                SortDirection = DefaultSortDirection;
                CurrentPageIndex = 0;

                BindPayrollRecords();
                BindPayrollSummary();
            }
            catch (Exception ex)
            {
                ShowError(GetSafeErrorMessage(
                    ex,
                    "The payroll filters could not be reset."));
            }
        }

        protected void btnGeneratePayroll_Click(
            object sender,
            EventArgs e)
        {
            // The official Create Pay Run workflow is the 4-step wizard.
            Response.Redirect("~/Modules/Payroll/CreatePayRun.aspx", true);
        }

        protected void gvPayroll_PageIndexChanging(
            object sender,
            GridViewPageEventArgs e)
        {
            if (e.NewPageIndex < 0)
            {
                return;
            }

            CurrentPageIndex = e.NewPageIndex;

            try
            {
                ClearMessage();
                BindPayrollRecords();
            }
            catch (Exception ex)
            {
                ShowError(GetSafeErrorMessage(
                    ex,
                    "The selected payroll page could not be loaded."));
            }
        }

        protected void gvPayroll_Sorting(
            object sender,
            GridViewSortEventArgs e)
        {
            string requestedColumn =
                e.SortExpression ?? string.Empty;

            if (!AllowedSortColumns.Contains(requestedColumn))
            {
                ShowWarning(
                    "The selected payroll sorting option is not available.");
                return;
            }

            if (string.Equals(
                SortColumn,
                requestedColumn,
                StringComparison.Ordinal))
            {
                SortDirection =
                    SortDirection == "ASC"
                        ? "DESC"
                        : "ASC";
            }
            else
            {
                SortColumn = requestedColumn;
                SortDirection = "ASC";
            }

            CurrentPageIndex = 0;

            try
            {
                ClearMessage();
                BindPayrollRecords();
            }
            catch (Exception ex)
            {
                ShowError(GetSafeErrorMessage(
                    ex,
                    "The payroll records could not be sorted."));
            }
        }

        protected void btnPreviousPage_Click(
            object sender,
            EventArgs e)
        {
            if (CurrentPageIndex <= 0)
            {
                return;
            }

            CurrentPageIndex--;

            try
            {
                ClearMessage();
                BindPayrollRecords();
            }
            catch (Exception ex)
            {
                ShowError(GetSafeErrorMessage(
                    ex,
                    "The previous payroll page could not be loaded."));
            }
        }

        protected void btnNextPage_Click(
            object sender,
            EventArgs e)
        {
            if (TotalPageCount <= 0 ||
                CurrentPageIndex + 1 >= TotalPageCount)
            {
                return;
            }

            CurrentPageIndex++;

            try
            {
                ClearMessage();
                BindPayrollRecords();
            }
            catch (Exception ex)
            {
                ShowError(GetSafeErrorMessage(
                    ex,
                    "The next payroll page could not be loaded."));
            }
        }

        protected string GetPaymentStatusCss(string status)
        {
            string commonClasses =
                "inline-flex items-center rounded-full px-2.5 py-1 text-xs font-semibold ";

            switch ((status ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "paid":
                    return commonClasses +
                        "bg-emerald-100 text-emerald-700";

                case "pending":
                    return commonClasses +
                        "bg-amber-100 text-amber-700";

                case "failed":
                    return commonClasses +
                        "bg-rose-100 text-rose-700";

                case "cancelled":
                    return commonClasses +
                        "bg-slate-200 text-slate-700";

                default:
                    return commonClasses +
                        "bg-blue-100 text-blue-700";
            }
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

        private void SetEmptySummary()
        {
            lblRecordCount.Text = "0";
            lblGrossSalary.Text = FormatMoney(0m);
            lblTotalDeductions.Text = FormatMoney(0m);
            lblNetSalary.Text = FormatMoney(0m);
            lblPaidAmount.Text = FormatMoney(0m);
            lblPendingAmount.Text = FormatMoney(0m);
            litGross2.Text = lblGrossSalary.Text;
            litNet2.Text = lblNetSalary.Text;
            litPaid2.Text = lblPaidAmount.Text;
            litPending2.Text = lblPendingAmount.Text;
        }

        private static int? ParsePositiveInt(string value)
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
            if (row == null ||
                row.Table == null ||
                !row.Table.Columns.Contains(columnName) ||
                row[columnName] == DBNull.Value)
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

        private static long GetInt64(
            DataRow row,
            string columnName)
        {
            if (row == null ||
                row.Table == null ||
                !row.Table.Columns.Contains(columnName) ||
                row[columnName] == DBNull.Value)
            {
                return 0L;
            }

            long value;

            return long.TryParse(
                Convert.ToString(
                    row[columnName],
                    CultureInfo.InvariantCulture),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out value)
                ? value
                : 0L;
        }

        private static decimal GetDecimal(
            DataRow row,
            string columnName)
        {
            if (row == null ||
                row.Table == null ||
                !row.Table.Columns.Contains(columnName) ||
                row[columnName] == DBNull.Value)
            {
                return 0m;
            }

            decimal value;

            return decimal.TryParse(
                Convert.ToString(
                    row[columnName],
                    CultureInfo.InvariantCulture),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out value)
                ? value
                : 0m;
        }

        private static string GetString(
            DataRow row,
            string columnName)
        {
            if (row == null ||
                row.Table == null ||
                !row.Table.Columns.Contains(columnName) ||
                row[columnName] == DBNull.Value)
            {
                return string.Empty;
            }

            return Convert.ToString(
                row[columnName],
                CultureInfo.CurrentCulture).Trim();
        }

        private static void RestoreSelection(
            ListControl control,
            string value)
        {
            if (control == null ||
                string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            ListItem item =
                control.Items.FindByValue(value);

            if (item != null)
            {
                control.ClearSelection();
                item.Selected = true;
            }
        }

        private static void ApplyPagerButtonState(
            WebControl button)
        {
            if (button == null)
            {
                return;
            }

            const string enabledCss =
                "inline-flex items-center gap-1 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-semibold text-slate-700 transition hover:bg-slate-50";

            const string disabledCss =
                "inline-flex cursor-not-allowed items-center gap-1 rounded-lg border border-slate-200 bg-slate-100 px-3 py-2 text-sm font-semibold text-slate-400 opacity-60";

            button.CssClass =
                button.Enabled
                    ? enabledCss
                    : disabledCss;
        }

        private static string GetSafeErrorMessage(
            Exception exception,
            string fallbackMessage)
        {
            if (exception == null ||
                string.IsNullOrWhiteSpace(exception.Message))
            {
                return fallbackMessage;
            }

            string message = exception.Message.Trim();

            if (message.Length > 500)
            {
                message = message.Substring(0, 500);
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
    }
}
