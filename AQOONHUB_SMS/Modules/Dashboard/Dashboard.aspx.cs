using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Dashboard
{
    public partial class Dashboard : System.Web.UI.Page
    {
        #region Private Fields
        private object _dashboardBLL;
        #endregion

        #region Page Lifecycle
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDashboardData();
            }
        }
        #endregion

        #region Data Loading
        private void LoadDashboardData()
        {
            try
            {
                Type bllType = Type.GetType("AQOONHUB_SMS.App_Code.BusinessLogic.DashboardBLL, App_Code");
                if (bllType == null)
                {
                    bllType = Type.GetType("AQOONHUB_SMS.App_Code.BusinessLogic.DashboardBLL");
                }
                if (bllType == null)
                {
                    bllType = Type.GetType("BusinessLogic.DashboardBLL");
                }

                if (bllType != null)
                {
                    _dashboardBLL = Activator.CreateInstance(bllType);
                }
                else
                {
                    return;
                }

                LoadStatistics();
                LoadAttendanceData();
                LoadFinanceData();
                LoadAcademicData();
                LoadRecentActivities();
                LoadNotifications();
                LoadAttendanceByClass();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Dashboard] LoadDashboardData failed: " + ex.ToString());
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    "alert('An error occurred while loading dashboard data. Please try again later.');", true);
            }
        }

        private void LoadStatistics()
        {
            try
            {
                object stats = CallBLLMethod("GetDashboardStats", null);
                if (stats != null)
                {
                    lblTotalStudents.Text = SafeFormatNumber(GetPropertyValue(stats, "TotalStudents"));
                    lblActiveStudents.Text = SafeFormatNumber(GetPropertyValue(stats, "ActiveStudents"));
                    lblTotalStaff.Text = SafeFormatNumber(GetPropertyValue(stats, "TotalStaff"));
                    lblFeeCollection.Text = SafeFormatCurrency(GetPropertyValue(stats, "TotalCollected"));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Dashboard] LoadStatistics failed: " + ex.ToString());
            }
        }

        private void LoadAttendanceData()
        {
            try
            {
                object stats = CallBLLMethod("GetDashboardStats", null);
                if (stats != null)
                {
                    lblPresentToday.Text = SafeFormatNumber(GetPropertyValue(stats, "PresentToday"));
                    lblAbsentToday.Text = SafeFormatNumber(GetPropertyValue(stats, "AbsentToday"));
                    lblLateToday.Text = SafeFormatNumber(GetPropertyValue(stats, "LateToday"));
                    lblAttendanceRate.Text = SafeFormatPercentage(GetPropertyValue(stats, "TodayAttendanceRate"));
                    lblAttendanceRateBar.Text = SafeFormatPercentage(GetPropertyValue(stats, "TodayAttendanceRate"));

                    decimal rate = Convert.ToDecimal(GetPropertyValue(stats, "TodayAttendanceRate") ?? 0);
                    attendanceProgress.Style["width"] = rate.ToString("0.0") + "%";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Dashboard] LoadAttendanceData failed: " + ex.ToString());
            }
        }

        private void LoadFinanceData()
        {
            try
            {
                object stats = CallBLLMethod("GetDashboardStats", null);
                if (stats != null)
                {
                    lblTotalBilled.Text = SafeFormatCurrency(GetPropertyValue(stats, "TotalBilled"));
                    lblTotalCollected.Text = SafeFormatCurrency(GetPropertyValue(stats, "TotalCollected"));
                    lblTotalOutstanding.Text = SafeFormatCurrency(GetPropertyValue(stats, "TotalOutstanding"));
                    lblCollectionRate.Text = SafeFormatPercentage(GetPropertyValue(stats, "CollectionRate"));

                    decimal rate = Convert.ToDecimal(GetPropertyValue(stats, "CollectionRate") ?? 0);
                    collectionProgress.Style["width"] = rate.ToString("0.0") + "%";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Dashboard] LoadFinanceData failed: " + ex.ToString());
            }
        }

        private void LoadAcademicData()
        {
            try
            {
                object stats = CallBLLMethod("GetDashboardStats", null);
                if (stats != null)
                {
                    lblUpcomingExams.Text = SafeFormatNumber(GetPropertyValue(stats, "UpcomingExams"));
                    lblActiveExams.Text = SafeFormatNumber(GetPropertyValue(stats, "ActiveExams"));
                    lblPendingApplications.Text = SafeFormatNumber(GetPropertyValue(stats, "PendingApplications"));
                    lblCurrentTerm.Text = SafeString(GetPropertyValue(stats, "CurrentTerm")?.ToString(), "-");

                    decimal termProgress = CalculateTermProgress(stats);
                    lblTermProgress.Text = termProgress.ToString("0.0") + "%";
                    termProgressBar.Style["width"] = termProgress.ToString("0.0") + "%";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Dashboard] LoadAcademicData failed: " + ex.ToString());
            }
        }

        private void LoadRecentActivities()
        {
            try
            {
                object result = CallBLLMethod("GetRecentActivities", new object[] { 10 });
                DataTable activities = result as DataTable;

                if (activities != null && activities.Rows.Count > 0)
                {
                    gvRecentActivities.DataSource = activities;
                    gvRecentActivities.DataBind();
                }
                else
                {
                    gvRecentActivities.DataSource = null;
                    gvRecentActivities.DataBind();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Dashboard] LoadRecentActivities failed: " + ex.ToString());
                gvRecentActivities.DataSource = null;
                gvRecentActivities.DataBind();
            }
        }

        private void LoadNotifications()
        {
            try
            {
                int userId = GetCurrentUserId();
                object result = CallBLLMethod("GetUserNotifications", new object[] { userId, 5 });
                DataTable notifications = result as DataTable;

                if (notifications != null)
                {
                    if (notifications.Rows.Count > 0)
                    {
                        lvNotifications.DataSource = notifications;
                        lvNotifications.DataBind();
                    }
                    else
                    {
                        lvNotifications.DataSource = null;
                        lvNotifications.DataBind();
                    }
                }
                else
                {
                    lvNotifications.DataSource = null;
                    lvNotifications.DataBind();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Dashboard] LoadNotifications failed: " + ex.ToString());
                lvNotifications.DataSource = null;
                lvNotifications.DataBind();
            }
        }

        private void LoadAttendanceByClass()
        {
            try
            {
                object result = CallBLLMethod("GetAttendanceByClass", null);
                DataTable attendanceData = result as DataTable;

                if (attendanceData != null && attendanceData.Rows.Count > 0)
                {
                    gvAttendanceByClass.DataSource = attendanceData;
                    gvAttendanceByClass.DataBind();
                }
                else
                {
                    gvAttendanceByClass.DataSource = null;
                    gvAttendanceByClass.DataBind();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Dashboard] LoadAttendanceByClass failed: " + ex.ToString());
                gvAttendanceByClass.DataSource = null;
                gvAttendanceByClass.DataBind();
            }
        }
        #endregion

        #region Reflection Helpers
        private object CallBLLMethod(string methodName, object[] parameters)
        {
            if (_dashboardBLL == null) return null;

            try
            {
                System.Reflection.MethodInfo method = _dashboardBLL.GetType().GetMethod(methodName);
                if (method != null)
                {
                    return method.Invoke(_dashboardBLL, parameters);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Dashboard] CallBLLMethod failed for " + methodName + ": " + ex.ToString());
            }
            return null;
        }

        private object GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null) return null;

            try
            {
                System.Reflection.PropertyInfo prop = obj.GetType().GetProperty(propertyName);
                if (prop != null)
                {
                    return prop.GetValue(obj);
                }
            }
            catch (Exception)
            {
                // Property not found
            }
            return null;
        }
        #endregion

        #region Event Handlers
        protected void gvAttendanceByClass_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                HiddenField hdnRate = (HiddenField)e.Row.FindControl("hdnRate");
                Panel pnlAttBar = (Panel)e.Row.FindControl("pnlAttBar");

                if (hdnRate != null && pnlAttBar != null)
                {
                    decimal rate = 0;
                    if (decimal.TryParse(hdnRate.Value, out rate))
                    {
                        pnlAttBar.Style["width"] = rate.ToString("0.0") + "%";
                        pnlAttBar.Style["background"] = GetAttendanceBarColor(rate);
                    }
                }
            }
        }
        #endregion

        #region Helper Methods
        private string SafeFormatNumber(object value)
        {
            if (value == null) return "0";
            int intVal;
            if (int.TryParse(value.ToString(), out intVal))
                return intVal.ToString("N0");
            return "0";
        }

        private string SafeFormatCurrency(object value)
        {
            if (value == null) return "$0.00";
            decimal decVal;
            if (decimal.TryParse(value.ToString(), out decVal))
                return decVal.ToString("C2");
            return "$0.00";
        }

        private string SafeFormatPercentage(object value)
        {
            if (value == null) return "0.0%";
            decimal decVal;
            if (decimal.TryParse(value.ToString(), out decVal))
                return decVal.ToString("0.0") + "%";
            return "0.0%";
        }

        private string SafeString(string value, string defaultValue)
        {
            return !string.IsNullOrEmpty(value) ? value : defaultValue;
        }

        private int GetCurrentUserId()
        {
            if (Session["UserID"] != null)
            {
                int userId;
                if (int.TryParse(Session["UserID"].ToString(), out userId))
                {
                    return userId;
                }
            }
            return 1; // Default fallback
        }

        private decimal CalculateTermProgress(object stats)
        {
            try
            {
                if (stats != null)
                {
                    object startDateObj = GetPropertyValue(stats, "TermStartDate");
                    object endDateObj = GetPropertyValue(stats, "TermEndDate");

                    if (startDateObj != null && endDateObj != null)
                    {
                        DateTime startDate = Convert.ToDateTime(startDateObj);
                        DateTime endDate = Convert.ToDateTime(endDateObj);
                        DateTime today = DateTime.Now;

                        if (today < startDate)
                            return 0;
                        if (today > endDate)
                            return 100;

                        int totalDays = (endDate - startDate).Days;
                        int elapsedDays = (today - startDate).Days;

                        if (totalDays > 0)
                        {
                            return Math.Round((decimal)elapsedDays / totalDays * 100, 1);
                        }
                    }
                }

                int dayOfYear = DateTime.Now.DayOfYear;
                int termLength = 90;
                return Math.Min(100, Math.Round((decimal)(dayOfYear % termLength) / termLength * 100, 1));
            }
            catch
            {
                return 0;
            }
        }
        #endregion

        #region UI Helper Methods
        protected string GetActivityBadgeClass(object activityType)
        {
            if (activityType == null || activityType == DBNull.Value)
                return "badge badge-secondary";

            string type = activityType.ToString().ToUpper();

            switch (type)
            {
                case "CREATE":
                case "INSERT":
                    return "badge badge-success";
                case "UPDATE":
                case "EDIT":
                    return "badge badge-primary";
                case "DELETE":
                case "REMOVE":
                    return "badge badge-danger";
                case "LOGIN":
                case "LOGOUT":
                    return "badge badge-info";
                case "EXPORT":
                case "PRINT":
                    return "badge badge-warning";
                case "APPROVE":
                case "ACTIVATE":
                    return "badge badge-success";
                case "REJECT":
                case "DEACTIVATE":
                    return "badge badge-danger";
                default:
                    return "badge badge-secondary";
            }
        }

        protected string GetNotificationBadgeClass(object priority)
        {
            if (priority == null || priority == DBNull.Value)
                return "badge badge-secondary";

            string p = priority.ToString().ToUpper();

            switch (p)
            {
                case "HIGH":
                case "URGENT":
                case "CRITICAL":
                    return "badge badge-danger";
                case "MEDIUM":
                case "NORMAL":
                    return "badge badge-warning";
                case "LOW":
                case "INFO":
                    return "badge badge-info";
                default:
                    return "badge badge-secondary";
            }
        }

        protected string GetNotificationIconClass(object priority)
        {
            if (priority == null || priority == DBNull.Value)
                return "blue";

            string p = priority.ToString().ToUpper();

            switch (p)
            {
                case "HIGH":
                case "URGENT":
                case "CRITICAL":
                    return "red";
                case "MEDIUM":
                case "NORMAL":
                    return "orange";
                case "LOW":
                case "INFO":
                    return "cyan";
                default:
                    return "blue";
            }
        }

        protected string GetNotificationIcon(object priority)
        {
            if (priority == null || priority == DBNull.Value)
                return GetInfoIcon();

            string p = priority.ToString().ToUpper();

            switch (p)
            {
                case "HIGH":
                case "URGENT":
                case "CRITICAL":
                    return GetAlertIcon();
                case "MEDIUM":
                case "NORMAL":
                    return GetWarningIcon();
                case "LOW":
                case "INFO":
                    return GetInfoIcon();
                default:
                    return GetInfoIcon();
            }
        }

        protected string GetAttendanceBarColor(decimal rate)
        {
            if (rate >= 90)
                return "#22C55E";
            else if (rate >= 75)
                return "#2563EB";
            else if (rate >= 60)
                return "#F59E0B";
            else
                return "#EF4444";
        }
        #endregion

        #region SVG Icon Helpers
        private string GetAlertIcon()
        {
            return "<path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126ZM12 15.75h.007v.008H12v-.008Z\" />";
        }

        private string GetWarningIcon()
        {
            return "<path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M12 9v3.75m9-.75a9 9 0 1 1-18 0 9 9 0 0 1 18 0Zm-9 3.75h.008v.008H12v-.008Z\" />";
        }

        private string GetInfoIcon()
        {
            return "<path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"m11.25 11.25.041-.02a.75.75 0 0 1 1.063.852l-.708 2.836a.75.75 0 0 0 1.063.853l.041-.021M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Zm-9-3.75h.008v.008H12V8.25Z\" />";
        }
        #endregion
    }
}