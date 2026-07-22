using System;
using System.Data;
using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.DataAccess;

namespace AQOONHUB_SMS.App_Code.BusinessLogic
{
    /// <summary>
    /// Business Logic Layer for Dashboard operations.
    /// Acts as intermediary between Presentation Layer and Data Access Layer.
    /// Validates data and applies business rules before returning to UI.
    /// </summary>
    public class DashboardBLL
    {
        private readonly DashboardDAL _dashboardDAL;

        /// <summary>
        /// Initializes a new instance of the DashboardBLL class.
        /// </summary>
        public DashboardBLL()
        {
            _dashboardDAL = new DashboardDAL();
        }

        /// <summary>
        /// Retrieves comprehensive dashboard statistics.
        /// Validates returned data and ensures no negative values.
        /// </summary>
        /// <returns>Validated DashboardStats model.</returns>
        public DashboardStats GetDashboardStats()
        {
            DashboardStats stats = _dashboardDAL.GetDashboardStats();
            return ValidateStats(stats);
        }

        /// <summary>
        /// Retrieves recent activities for the dashboard activity feed.
        /// </summary>
        /// <param name="count">Number of activities to retrieve (default: 10).</param>
        /// <returns>DataTable containing recent activities.</returns>
        public DataTable GetRecentActivities(int count = 10)
        {
            if (count <= 0) count = 10;
            if (count > 50) count = 50; // Maximum limit

            return _dashboardDAL.GetRecentActivities(count);
        }

        /// <summary>
        /// Retrieves notifications for the current user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="count">Maximum number of notifications (default: 5).</param>
        /// <returns>DataTable containing user notifications.</returns>
        public DataTable GetUserNotifications(int userId, int count = 5)
        {
            if (userId <= 0) return new DataTable();
            if (count <= 0) count = 5;
            if (count > 20) count = 20; // Maximum limit

            return _dashboardDAL.GetUserNotifications(userId, count);
        }

        /// <summary>
        /// Retrieves upcoming exams for the dashboard.
        /// </summary>
        /// <param name="daysAhead">Number of days to look ahead (default: 30).</param>
        /// <returns>DataTable containing upcoming exams.</returns>
        public DataTable GetUpcomingExams(int daysAhead = 30)
        {
            if (daysAhead <= 0) daysAhead = 30;
            if (daysAhead > 365) daysAhead = 365; // Maximum 1 year

            return _dashboardDAL.GetUpcomingExams(daysAhead);
        }

        /// <summary>
        /// Retrieves upcoming events for the dashboard.
        /// </summary>
        /// <param name="daysAhead">Number of days to look ahead (default: 30).</param>
        /// <returns>DataTable containing upcoming events.</returns>
        public DataTable GetUpcomingEvents(int daysAhead = 30)
        {
            if (daysAhead <= 0) daysAhead = 30;
            if (daysAhead > 365) daysAhead = 365;

            return _dashboardDAL.GetUpcomingEvents(daysAhead);
        }

        /// <summary>
        /// Retrieves fee collection summary for chart data.
        /// </summary>
        /// <param name="months">Number of months (default: 6).</param>
        /// <returns>DataTable containing monthly fee collection.</returns>
        public DataTable GetFeeCollectionSummary(int months = 6)
        {
            if (months <= 0) months = 6;
            if (months > 24) months = 24; // Maximum 2 years

            return _dashboardDAL.GetFeeCollectionSummary(months);
        }

        /// <summary>
        /// Retrieves attendance summary by class for today.
        /// </summary>
        /// <returns>DataTable containing class-wise attendance.</returns>
        public DataTable GetAttendanceByClass()
        {
            return _dashboardDAL.GetAttendanceByClass();
        }

        /// <summary>
        /// Gets the collection rate percentage.
        /// Calculated as (TotalCollected / TotalBilled) * 100.
        /// </summary>
        /// <returns>Collection rate as a percentage (0-100).</returns>
        public decimal GetCollectionRate()
        {
            DashboardStats stats = GetDashboardStats();
            return stats.CollectionRate;
        }

        /// <summary>
        /// Gets today's attendance rate percentage.
        /// Calculated as (Present / Total) * 100.
        /// </summary>
        /// <returns>Attendance rate as a percentage (0-100).</returns>
        public decimal GetTodayAttendanceRate()
        {
            DashboardStats stats = GetDashboardStats();
            return stats.TodayAttendanceRate;
        }

        /// <summary>
        /// Gets the total number of students with attendance marked today.
        /// </summary>
        /// <returns>Total attendance count for today.</returns>
        public int GetTotalAttendanceToday()
        {
            DashboardStats stats = GetDashboardStats();
            return stats.PresentToday + stats.AbsentToday + stats.LateToday;
        }

        /// <summary>
        /// Validates DashboardStats values to ensure data integrity.
        /// Ensures no negative values and logical consistency.
        /// </summary>
        /// <param name="stats">The DashboardStats to validate.</param>
        /// <returns>Validated DashboardStats.</returns>
        private DashboardStats ValidateStats(DashboardStats stats)
        {
            // Ensure no negative values
            if (stats.TotalStudents < 0) stats.TotalStudents = 0;
            if (stats.ActiveStudents < 0) stats.ActiveStudents = 0;
            if (stats.SuspendedStudents < 0) stats.SuspendedStudents = 0;
            if (stats.NewAdmissions < 0) stats.NewAdmissions = 0;
            if (stats.TotalStaff < 0) stats.TotalStaff = 0;
            if (stats.ActiveStaff < 0) stats.ActiveStaff = 0;
            if (stats.OnLeaveStaff < 0) stats.OnLeaveStaff = 0;
            if (stats.TotalBilled < 0) stats.TotalBilled = 0;
            if (stats.TotalCollected < 0) stats.TotalCollected = 0;
            if (stats.TotalOutstanding < 0) stats.TotalOutstanding = 0;
            if (stats.PresentToday < 0) stats.PresentToday = 0;
            if (stats.AbsentToday < 0) stats.AbsentToday = 0;
            if (stats.LateToday < 0) stats.LateToday = 0;
            if (stats.UpcomingExams < 0) stats.UpcomingExams = 0;
            if (stats.ActiveExams < 0) stats.ActiveExams = 0;
            if (stats.PendingApplications < 0) stats.PendingApplications = 0;

            // Ensure logical consistency: Active + Suspended <= Total
            if (stats.ActiveStudents + stats.SuspendedStudents > stats.TotalStudents)
            {
                stats.TotalStudents = stats.ActiveStudents + stats.SuspendedStudents;
            }

            // Ensure logical consistency: Active + OnLeave <= TotalStaff
            if (stats.ActiveStaff + stats.OnLeaveStaff > stats.TotalStaff)
            {
                stats.TotalStaff = stats.ActiveStaff + stats.OnLeaveStaff;
            }

            // Ensure TotalOutstanding = TotalBilled - TotalCollected
            decimal calculatedOutstanding = stats.TotalBilled - stats.TotalCollected;
            if (stats.TotalOutstanding != calculatedOutstanding && stats.TotalBilled > 0)
            {
                stats.TotalOutstanding = calculatedOutstanding > 0 ? calculatedOutstanding : 0;
            }

            return stats;
        }
    }
}