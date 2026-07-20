using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;


namespace AQOONHUB_SMS.App_Code.DataAccess
{
    /// <summary>
    /// Data access layer for notification operations
    /// </summary>
    public class NotificationDAL
    {
        private DatabaseHelper db;

        /// <summary>
        /// Initializes a new instance of the NotificationDAL class
        /// </summary>
        public NotificationDAL()
        {
            db = new DatabaseHelper();
        }

        #region Retrieval Methods

        /// <summary>
        /// Retrieves all notifications from the database
        /// </summary>
        /// <returns>List of Notification objects</returns>
        public List<Notification> GetNotifications()
        {
            List<Notification> notifications = new List<Notification>();

            string query = @"
                SELECT n.*, u.FullName as CreatedByName
                FROM Notifications n
                LEFT JOIN Users u ON n.CreatedBy = u.UserID
                ORDER BY n.CreatedAt DESC";

            DataTable dt = db.ExecuteQuery(query);

            foreach (DataRow row in dt.Rows)
            {
                notifications.Add(MapToNotification(row));
            }

            return notifications;
        }

        /// <summary>
        /// Retrieves a single notification by its ID
        /// </summary>
        /// <param name="notificationId">The notification ID</param>
        /// <returns>Notification object or null if not found</returns>
        public Notification GetNotificationById(int notificationId)
        {
            string query = @"
                SELECT n.*, u.FullName as CreatedByName
                FROM Notifications n
                LEFT JOIN Users u ON n.CreatedBy = u.UserID
                WHERE n.NotificationID = @NotificationID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@NotificationID", notificationId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
                return MapToNotification(dt.Rows[0]);

            return null;
        }

        /// <summary>
        /// Retrieves all notifications for a specific user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of Notification objects</returns>
        public List<Notification> GetUserNotifications(int userId)
        {
            List<Notification> notifications = new List<Notification>();

            string query = @"
                SELECT n.*, u.FullName as CreatedByName
                FROM Notifications n
                LEFT JOIN Users u ON n.CreatedBy = u.UserID
                WHERE n.UserID = @UserID
                ORDER BY n.CreatedAt DESC";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                notifications.Add(MapToNotification(row));
            }

            return notifications;
        }

        /// <summary>
        /// Retrieves unread notifications for a specific user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of unread Notification objects</returns>
        public List<Notification> GetUnreadNotifications(int userId)
        {
            List<Notification> notifications = new List<Notification>();

            string query = @"
                SELECT n.*, u.FullName as CreatedByName
                FROM Notifications n
                LEFT JOIN Users u ON n.CreatedBy = u.UserID
                WHERE n.UserID = @UserID AND n.IsRead = 0
                ORDER BY n.CreatedAt DESC";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                notifications.Add(MapToNotification(row));
            }

            return notifications;
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Adds a new notification to the database
        /// </summary>
        /// <param name="notification">The notification to add</param>
        /// <returns>The ID of the newly created notification</returns>
        public int AddNotification(Notification notification)
        {
            string query = @"
                INSERT INTO Notifications (UserID, Title, Message, NotificationType, Priority, IsRead, ReadAt, LinkUrl, Icon, CreatedBy, CreatedAt)
                VALUES (@UserID, @Title, @Message, @NotificationType, @Priority, @IsRead, @ReadAt, @LinkUrl, @Icon, @CreatedBy, @CreatedAt);
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", notification.UserID),
                new SqlParameter("@Title", notification.Title),
                new SqlParameter("@Message", notification.Message),
                new SqlParameter("@NotificationType", string.IsNullOrEmpty(notification.NotificationType) ? (object)DBNull.Value : notification.NotificationType),
                new SqlParameter("@Priority", string.IsNullOrEmpty(notification.Priority) ? (object)DBNull.Value : notification.Priority),
                new SqlParameter("@IsRead", notification.IsRead),
                new SqlParameter("@ReadAt", notification.ReadAt.HasValue ? (object)notification.ReadAt.Value : (object)DBNull.Value),
                new SqlParameter("@LinkUrl", string.IsNullOrEmpty(notification.LinkUrl) ? (object)DBNull.Value : notification.LinkUrl),
                new SqlParameter("@Icon", string.IsNullOrEmpty(notification.Icon) ? (object)DBNull.Value : notification.Icon),
                new SqlParameter("@CreatedBy", notification.CreatedBy),
                new SqlParameter("@CreatedAt", notification.CreatedAt)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        /// <summary>
        /// Updates an existing notification
        /// </summary>
        /// <param name="notification">The notification with updated values</param>
        /// <returns>True if update succeeded</returns>
        public bool UpdateNotification(Notification notification)
        {
            string query = @"
                UPDATE Notifications SET
                    UserID = @UserID,
                    Title = @Title,
                    Message = @Message,
                    NotificationType = @NotificationType,
                    Priority = @Priority,
                    IsRead = @IsRead,
                    ReadAt = @ReadAt,
                    LinkUrl = @LinkUrl,
                    Icon = @Icon
                WHERE NotificationID = @NotificationID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@NotificationID", notification.NotificationID),
                new SqlParameter("@UserID", notification.UserID),
                new SqlParameter("@Title", notification.Title),
                new SqlParameter("@Message", notification.Message),
                new SqlParameter("@NotificationType", string.IsNullOrEmpty(notification.NotificationType) ? (object)DBNull.Value : notification.NotificationType),
                new SqlParameter("@Priority", string.IsNullOrEmpty(notification.Priority) ? (object)DBNull.Value : notification.Priority),
                new SqlParameter("@IsRead", notification.IsRead),
                new SqlParameter("@ReadAt", notification.ReadAt.HasValue ? (object)notification.ReadAt.Value : (object)DBNull.Value),
                new SqlParameter("@LinkUrl", string.IsNullOrEmpty(notification.LinkUrl) ? (object)DBNull.Value : notification.LinkUrl),
                new SqlParameter("@Icon", string.IsNullOrEmpty(notification.Icon) ? (object)DBNull.Value : notification.Icon)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Deletes a notification from the database
        /// </summary>
        /// <param name="notificationId">The notification ID to delete</param>
        /// <returns>True if deletion succeeded</returns>
        public bool DeleteNotification(int notificationId)
        {
            string query = "DELETE FROM Notifications WHERE NotificationID = @NotificationID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@NotificationID", notificationId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        #endregion

        #region Status Operations

        /// <summary>
        /// Marks a notification as read
        /// </summary>
        /// <param name="notificationId">The notification ID</param>
        /// <returns>True if update succeeded</returns>
        public bool MarkAsRead(int notificationId)
        {
            string query = @"
                UPDATE Notifications SET
                    IsRead = 1,
                    ReadAt = GETDATE()
                WHERE NotificationID = @NotificationID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@NotificationID", notificationId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Marks all notifications as read for a specific user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Number of notifications updated</returns>
        public int MarkAllAsRead(int userId)
        {
            string query = @"
                UPDATE Notifications SET
                    IsRead = 1,
                    ReadAt = GETDATE()
                WHERE UserID = @UserID AND IsRead = 0";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId)
            };

            return db.ExecuteNonQuery(query, parameters);
        }

        #endregion

        #region Count and Search

        /// <summary>
        /// Gets the count of unread notifications for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Number of unread notifications</returns>
        public int GetNotificationCount(int userId)
        {
            string query = @"
                SELECT COUNT(*) FROM Notifications
                WHERE UserID = @UserID AND IsRead = 0";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        /// <summary>
        /// Searches notifications by keyword
        /// </summary>
        /// <param name="keyword">Search keyword for title or message</param>
        /// <returns>List of matching Notification objects</returns>
        public List<Notification> SearchNotifications(string keyword)
        {
            List<Notification> notifications = new List<Notification>();

            string query = @"
                SELECT n.*, u.FullName as CreatedByName
                FROM Notifications n
                LEFT JOIN Users u ON n.CreatedBy = u.UserID
                WHERE n.Title LIKE @Keyword OR n.Message LIKE @Keyword
                ORDER BY n.CreatedAt DESC";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Keyword", "%" + keyword + "%")
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                notifications.Add(MapToNotification(row));
            }

            return notifications;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Maps a DataRow to a Notification object
        /// </summary>
        /// <param name="row">The DataRow to map</param>
        /// <returns>Populated Notification object</returns>
        private Notification MapToNotification(DataRow row)
        {
            Notification notification = new Notification();
            notification.NotificationID = Convert.ToInt32(row["NotificationID"]);
            notification.UserID = Convert.ToInt32(row["UserID"]);
            notification.Title = row["Title"].ToString();
            notification.Message = row["Message"].ToString();
            notification.NotificationType = row["NotificationType"] == DBNull.Value ? null : row["NotificationType"].ToString();
            notification.Priority = row["Priority"] == DBNull.Value ? null : row["Priority"].ToString();
            notification.IsRead = Convert.ToBoolean(row["IsRead"]);
            notification.ReadAt = row["ReadAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ReadAt"]);
            notification.LinkUrl = row["LinkUrl"] == DBNull.Value ? null : row["LinkUrl"].ToString();
            notification.Icon = row["Icon"] == DBNull.Value ? null : row["Icon"].ToString();
            notification.CreatedBy = Convert.ToInt32(row["CreatedBy"]);
            notification.CreatedAt = Convert.ToDateTime(row["CreatedAt"]);

            if (row.Table.Columns.Contains("CreatedByName"))
                notification.CreatedByName = row["CreatedByName"] == DBNull.Value ? null : row["CreatedByName"].ToString();

            return notification;
        }

        #endregion
    }
}