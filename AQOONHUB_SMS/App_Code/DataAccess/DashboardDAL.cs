using System;
using System.Data;
using System.Data.SqlClient;
using AQOONHUB_SMS.App_Code.Models;

namespace AQOONHUB_SMS.App_Code.DataAccess
{
    /// <summary>
    /// Data Access Layer for Dashboard statistics.
    /// Handles all database operations for the Unified Role-Based Dashboard.
    /// Uses ADO.NET with stored procedures only.
    /// </summary>
    public class DashboardDAL
    {
        /// <summary>
        /// Gets the connection string from web.config.
        /// </summary>
        private string ConnectionString
        {
            get
            {
                return System.Configuration.ConfigurationManager
                    .ConnectionStrings["AQOONHUB_DB"].ConnectionString;
            }
        }

        /// <summary>
        /// Retrieves comprehensive dashboard statistics from the database.
        /// Calls sp_GetDashboardStats stored procedure.
        /// </summary>
        /// <returns>DashboardStats model populated with current data.</returns>
        public DashboardStats GetDashboardStats()
        {
            DashboardStats stats = new DashboardStats();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_GetDashboardStats", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 30;

                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Student statistics
                                stats.TotalStudents = reader["TotalStudents"] != DBNull.Value
                                    ? Convert.ToInt32(reader["TotalStudents"]) : 0;
                                stats.ActiveStudents = reader["ActiveStudents"] != DBNull.Value
                                    ? Convert.ToInt32(reader["ActiveStudents"]) : 0;
                                stats.SuspendedStudents = reader["SuspendedStudents"] != DBNull.Value
                                    ? Convert.ToInt32(reader["SuspendedStudents"]) : 0;
                                stats.NewAdmissions = reader["NewAdmissions"] != DBNull.Value
                                    ? Convert.ToInt32(reader["NewAdmissions"]) : 0;

                                // Staff statistics
                                stats.TotalStaff = reader["TotalStaff"] != DBNull.Value
                                    ? Convert.ToInt32(reader["TotalStaff"]) : 0;
                                stats.ActiveStaff = reader["ActiveStaff"] != DBNull.Value
                                    ? Convert.ToInt32(reader["ActiveStaff"]) : 0;
                                stats.OnLeaveStaff = reader["OnLeaveStaff"] != DBNull.Value
                                    ? Convert.ToInt32(reader["OnLeaveStaff"]) : 0;

                                // Fee statistics
                                stats.TotalBilled = reader["TotalBilled"] != DBNull.Value
                                    ? Convert.ToDecimal(reader["TotalBilled"]) : 0;
                                stats.TotalCollected = reader["TotalCollected"] != DBNull.Value
                                    ? Convert.ToDecimal(reader["TotalCollected"]) : 0;
                                stats.TotalOutstanding = reader["TotalOutstanding"] != DBNull.Value
                                    ? Convert.ToDecimal(reader["TotalOutstanding"]) : 0;

                                // Attendance statistics
                                stats.PresentToday = reader["PresentToday"] != DBNull.Value
                                    ? Convert.ToInt32(reader["PresentToday"]) : 0;
                                stats.AbsentToday = reader["AbsentToday"] != DBNull.Value
                                    ? Convert.ToInt32(reader["AbsentToday"]) : 0;
                                stats.LateToday = reader["LateToday"] != DBNull.Value
                                    ? Convert.ToInt32(reader["LateToday"]) : 0;

                                // Exam statistics
                                stats.UpcomingExams = reader["UpcomingExams"] != DBNull.Value
                                    ? Convert.ToInt32(reader["UpcomingExams"]) : 0;
                                stats.ActiveExams = reader["ActiveExams"] != DBNull.Value
                                    ? Convert.ToInt32(reader["ActiveExams"]) : 0;

                                // Application statistics
                                stats.PendingApplications = reader["PendingApplications"] != DBNull.Value
                                    ? Convert.ToInt32(reader["PendingApplications"]) : 0;
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Log SQL-specific errors
                System.Diagnostics.Debug.WriteLine(
                    string.Format("[DashboardDAL] SQL Error {0}: {1}", ex.Number, ex.Message));
                // Return empty stats on error - BLL will handle validation
            }
            catch (Exception ex)
            {
                // Log general errors
                System.Diagnostics.Debug.WriteLine(
                    string.Format("[DashboardDAL] Error: {0}", ex.Message));
            }

            return stats;
        }

        /// <summary>
        /// Retrieves recent activities for the dashboard activity feed.
        /// Calls sp_GetRecentActivities stored procedure.
        /// </summary>
        /// <param name="count">Number of activities to retrieve.</param>
        /// <returns>DataTable containing recent activities.</returns>
        public DataTable GetRecentActivities(int count)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_GetRecentActivities", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Count", count);
                        cmd.CommandTimeout = 30;

                        conn.Open();

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    string.Format("[DashboardDAL] SQL Error {0}: {1}", ex.Number, ex.Message));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    string.Format("[DashboardDAL] Error: {0}", ex.Message));
            }

            return dt;
        }

        /// <summary>
        /// Retrieves notifications for the current user.
        /// Calls sp_GetUserNotifications stored procedure.
        /// </summary>
        /// <param name="userId">The user ID to get notifications for.</param>
        /// <param name="count">Maximum number of notifications.</param>
        /// <returns>DataTable containing notifications.</returns>
        public DataTable GetUserNotifications(int userId, int count)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_GetUserNotifications", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@Count", count);
                        cmd.CommandTimeout = 30;

                        conn.Open();

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    string.Format("[DashboardDAL] SQL Error {0}: {1}", ex.Number, ex.Message));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    string.Format("[DashboardDAL] Error: {0}", ex.Message));
            }

            return dt;
        }

        /// <summary>
        /// Retrieves upcoming exams for the dashboard.
        /// Calls sp_GetUpcomingExams stored procedure.
        /// </summary>
        /// <param name="daysAhead">Number of days to look ahead.</param>
        /// <returns>DataTable containing upcoming exams.</returns>
        public DataTable GetUpcomingExams(int daysAhead)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_GetUpcomingExams", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@DaysAhead", daysAhead);
                        cmd.CommandTimeout = 30;

                        conn.Open();

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    string.Format("[DashboardDAL] SQL Error {0}: {1}", ex.Number, ex.Message));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    string.Format("[DashboardDAL] Error: {0}", ex.Message));
            }

            return dt;
        }

        /// <summary>
        /// Retrieves upcoming events for the dashboard.
        /// Calls sp_GetUpcomingEvents stored procedure.
        /// </summary>
        /// <param name="daysAhead">Number of days to look ahead.</param>
        /// <returns>DataTable containing upcoming events.</returns>
        public DataTable GetUpcomingEvents(int daysAhead)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_GetUpcomingEvents", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@DaysAhead", daysAhead);
                        cmd.CommandTimeout = 30;

                        conn.Open();

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    string.Format("[DashboardDAL] SQL Error {0}: {1}", ex.Number, ex.Message));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    string.Format("[DashboardDAL] Error: {0}", ex.Message));
            }

            return dt;
        }

        /// <summary>
        /// Retrieves fee collection summary by month for chart data.
        /// Calls sp_GetFeeCollectionSummary stored procedure.
        /// </summary>
        /// <param name="months">Number of months to retrieve.</param>
        /// <returns>DataTable containing monthly fee collection data.</returns>
        public DataTable GetFeeCollectionSummary(int months)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_GetFeeCollectionSummary", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Months", months);
                        cmd.CommandTimeout = 30;

                        conn.Open();

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    string.Format("[DashboardDAL] SQL Error {0}: {1}", ex.Number, ex.Message));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    string.Format("[DashboardDAL] Error: {0}", ex.Message));
            }

            return dt;
        }

        /// <summary>
        /// Retrieves attendance summary by class for the current day.
        /// Calls sp_GetAttendanceByClass stored procedure.
        /// </summary>
        /// <returns>DataTable containing class-wise attendance.</returns>
        public DataTable GetAttendanceByClass()
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_GetAttendanceByClass", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 30;

                        conn.Open();

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    string.Format("[DashboardDAL] SQL Error {0}: {1}", ex.Number, ex.Message));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    string.Format("[DashboardDAL] Error: {0}", ex.Message));
            }

            return dt;
        }
    }
}