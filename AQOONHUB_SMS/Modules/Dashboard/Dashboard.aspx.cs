using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Dashboard
{
    public partial class Dashboard : System.Web.UI.Page
    {
        #region Private Fields
        private string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["AQOONHUB_DB"].ConnectionString; }
        }
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
            }
        }

        private void LoadStatistics()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            (SELECT COUNT(*) FROM Students WHERE IsActive = 1) AS TotalStudents,
                            (SELECT COUNT(*) FROM Students WHERE Status = 'Active') AS ActiveStudents,
                            (SELECT COUNT(*) FROM Staff) AS TotalStaff,
                            (SELECT COALESCE(SUM(AmountPaid), 0) FROM FeePayments WHERE PaymentDate >= DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0)) AS TotalCollected";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lblTotalStudents.Text = SafeFormatNumber(reader["TotalStudents"]);
                                lblActiveStudents.Text = SafeFormatNumber(reader["ActiveStudents"]);
                                lblTotalStaff.Text = SafeFormatNumber(reader["TotalStaff"]);
                                lblFeeCollection.Text = SafeFormatCurrency(reader["TotalCollected"]);
                            }
                        }
                    }
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
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            COUNT(CASE WHEN Status = 'Present' THEN 1 END) AS PresentToday,
                            COUNT(CASE WHEN Status = 'Absent' THEN 1 END) AS AbsentToday,
                            COUNT(CASE WHEN Status = 'Late' THEN 1 END) AS LateToday,
                            COUNT(*) AS TotalToday
                        FROM Attendance
                        WHERE CAST(AttendanceDate AS DATE) = CAST(GETDATE() AS DATE)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int present = Convert.ToInt32(reader["PresentToday"] ?? 0);
                                int absent = Convert.ToInt32(reader["AbsentToday"] ?? 0);
                                int late = Convert.ToInt32(reader["LateToday"] ?? 0);
                                int total = Convert.ToInt32(reader["TotalToday"] ?? 0);

                                lblPresentToday.Text = present.ToString();
                                lblAbsentToday.Text = absent.ToString();
                                lblLateToday.Text = late.ToString();

                                decimal rate = total > 0 ? Math.Round((decimal)present / total * 100, 1) : 0;
                                lblAttendanceRate.Text = rate.ToString("0.0") + "%";
                                lblAttendanceRateBar.Text = rate.ToString("0.0") + "%";
                                attendanceProgress.Style["width"] = rate.ToString("0.0") + "%";
                            }
                        }
                    }
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
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            COALESCE(SUM(TotalAmount), 0) AS TotalBilled,
                            COALESCE(SUM(AmountPaid), 0) AS TotalCollected,
                            COALESCE(SUM(Balance), 0) AS TotalOutstanding
                        FROM FeePayments
                        WHERE PaymentDate >= DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                decimal billed = Convert.ToDecimal(reader["TotalBilled"] ?? 0);
                                decimal collected = Convert.ToDecimal(reader["TotalCollected"] ?? 0);
                                decimal outstanding = Convert.ToDecimal(reader["TotalOutstanding"] ?? 0);

                                lblTotalBilled.Text = billed.ToString("C2");
                                lblTotalCollected.Text = collected.ToString("C2");
                                lblTotalOutstanding.Text = outstanding.ToString("C2");

                                decimal rate = billed > 0 ? Math.Round(collected / billed * 100, 1) : 0;
                                lblCollectionRate.Text = rate.ToString("0.0") + "%";
                                collectionProgress.Style["width"] = rate.ToString("0.0") + "%";
                            }
                        }
                    }
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
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            (SELECT COUNT(*) FROM Exams WHERE ExamDate > GETDATE()) AS UpcomingExams,
                            (SELECT COUNT(*) FROM Exams WHERE ExamDate <= GETDATE() AND EndDate >= GETDATE()) AS ActiveExams,
                            (SELECT COUNT(*) FROM Admissions WHERE Status = 'Pending') AS PendingApplications,
                            (SELECT TOP 1 TermName FROM AcademicTerms WHERE StartDate <= GETDATE() AND EndDate >= GETDATE()) AS CurrentTerm,
                            (SELECT TOP 1 StartDate FROM AcademicTerms WHERE StartDate <= GETDATE() AND EndDate >= GETDATE()) AS TermStartDate,
                            (SELECT TOP 1 EndDate FROM AcademicTerms WHERE StartDate <= GETDATE() AND EndDate >= GETDATE()) AS TermEndDate";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lblUpcomingExams.Text = SafeFormatNumber(reader["UpcomingExams"]);
                                lblActiveExams.Text = SafeFormatNumber(reader["ActiveExams"]);
                                lblPendingApplications.Text = SafeFormatNumber(reader["PendingApplications"]);
                                lblCurrentTerm.Text = SafeString(reader["CurrentTerm"]?.ToString(), "-");

                                DateTime? startDate = reader["TermStartDate"] != DBNull.Value ? (DateTime?)reader["TermStartDate"] : null;
                                DateTime? endDate = reader["TermEndDate"] != DBNull.Value ? (DateTime?)reader["TermEndDate"] : null;

                                decimal termProgress = CalculateTermProgress(startDate, endDate);
                                lblTermProgress.Text = termProgress.ToString("0.0") + "%";
                                termProgressBar.Style["width"] = termProgress.ToString("0.0") + "%";
                            }
                        }
                    }
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
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT TOP 10 
                            ActivityType,
                            Description,
                            ActivityDate,
                            PerformedBy
                        FROM AuditLog
                        ORDER BY ActivityDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            gvRecentActivities.DataSource = dt;
                            gvRecentActivities.DataBind();
                        }
                    }
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
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT TOP 5 
                            NotificationID,
                            UserID,
                            Title,
                            Message,
                            NotificationType,
                            Priority,
                            IsRead,
                            CreatedAt
                        FROM Notifications
                        WHERE (UserID = @UserID OR UserID IS NULL)
                        AND IsRead = 0
                        ORDER BY CreatedAt DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            lvNotifications.DataSource = dt;
                            lvNotifications.DataBind();
                        }
                    }
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
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            c.ClassName,
                            COUNT(DISTINCT s.StudentID) AS TotalStudents,
                            COUNT(DISTINCT CASE WHEN a.Status = 'Present' THEN s.StudentID END) AS PresentCount,
                            COUNT(DISTINCT CASE WHEN a.Status = 'Absent' THEN s.StudentID END) AS AbsentCount,
                            COUNT(DISTINCT CASE WHEN a.Status = 'Late' THEN s.StudentID END) AS LateCount,
                            CASE 
                                WHEN COUNT(DISTINCT s.StudentID) = 0 THEN 0
                                ELSE CAST(COUNT(DISTINCT CASE WHEN a.Status = 'Present' THEN s.StudentID END) AS DECIMAL(10,2)) * 100.0 / COUNT(DISTINCT s.StudentID)
                            END AS AttendanceRate
                        FROM Classes c
                        LEFT JOIN Students s ON c.ClassID = s.ClassID AND s.IsActive = 1
                        LEFT JOIN Attendance a ON s.StudentID = a.StudentID AND CAST(a.AttendanceDate AS DATE) = CAST(GETDATE() AS DATE)
                        GROUP BY c.ClassID, c.ClassName
                        ORDER BY c.ClassName";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            gvAttendanceByClass.DataSource = dt;
                            gvAttendanceByClass.DataBind();
                        }
                    }
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

        #region Event Handlers
        protected void gvAttendanceByClass_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                HiddenField hdnRate = (HiddenField)e.Row.FindControl("hdnRate");
                Panel pnlAttBar = (Panel)e.Row.FindControl("pnlAttBar");
                Label lblAttRate = (Label)e.Row.FindControl("lblAttRate");

                if (hdnRate != null && pnlAttBar != null)
                {
                    decimal rate = 0;
                    if (decimal.TryParse(hdnRate.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out rate))
                    {
                        // Clamp to 0-100
                        rate = Math.Max(0, Math.Min(100, rate));

                        pnlAttBar.Style["width"] = rate.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "%";
                        pnlAttBar.Style["background"] = GetAttendanceBarColor(rate);

                        if (lblAttRate != null)
                        {
                            lblAttRate.Text = rate.ToString("0.0") + "%";
                        }
                    }
                    else
                    {
                        pnlAttBar.Style["width"] = "0%";
                        if (lblAttRate != null)
                        {
                            lblAttRate.Text = "0.0%";
                        }
                    }
                }
            }
        }
        #endregion

        #region Helper Methods
        private string SafeFormatNumber(object value)
        {
            if (value == null || value == DBNull.Value) return "0";
            int intVal;
            if (int.TryParse(value.ToString(), out intVal))
                return intVal.ToString("N0");
            return "0";
        }

        private string SafeFormatCurrency(object value)
        {
            if (value == null || value == DBNull.Value) return "$0.00";
            decimal decVal;
            if (decimal.TryParse(value.ToString(), out decVal))
                return decVal.ToString("C2");
            return "$0.00";
        }

        private string SafeFormatPercentage(object value)
        {
            if (value == null || value == DBNull.Value) return "0.0%";
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
            return 1;
        }

        private decimal CalculateTermProgress(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                if (startDate.HasValue && endDate.HasValue)
                {
                    DateTime today = DateTime.Now;

                    if (today < startDate.Value)
                        return 0;
                    if (today > endDate.Value)
                        return 100;

                    int totalDays = (endDate.Value - startDate.Value).Days;
                    int elapsedDays = (today - startDate.Value).Days;

                    if (totalDays > 0)
                    {
                        return Math.Round((decimal)elapsedDays / totalDays * 100, 1);
                    }
                }

                // Fallback: use day of year
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