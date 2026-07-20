using System;
using System.Collections.Generic;
using AQOONHUB_SMS.App_Code.DataAccess;
using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;

namespace AQOONHUB_SMS.App_Code.BusinessLogic
{
    /// <summary>
    /// Business logic layer for managing notifications
    /// </summary>
    public class NotificationBLL
    {
        private NotificationDAL notificationDAL;
        private AuditLogger auditLogger;

        /// <summary>
        /// Initializes a new instance of the NotificationBLL class
        /// </summary>
        public NotificationBLL()
        {
            notificationDAL = new NotificationDAL();
            auditLogger = new AuditLogger();
        }

        #region Validation

        /// <summary>
        /// Validates notification data before save
        /// </summary>
        /// <param name="notification">The notification to validate</param>
        /// <returns>List of validation error messages</returns>
        private List<string> ValidateNotification(Notification notification)
        {
            List<string> errors = new List<string>();

            if (notification == null)
            {
                errors.Add("Notification cannot be null");
                return errors;
            }

            if (notification.UserID <= 0)
                errors.Add("User is required");

            if (string.IsNullOrWhiteSpace(notification.Title))
                errors.Add("Title is required");
            else if (notification.Title.Length > 200)
                errors.Add("Title cannot exceed 200 characters");

            if (string.IsNullOrWhiteSpace(notification.Message))
                errors.Add("Message is required");
            else if (notification.Message.Length > 2000)
                errors.Add("Message cannot exceed 2000 characters");

            if (!string.IsNullOrEmpty(notification.Priority))
            {
                string[] validPriorities = new string[] { "Low", "Normal", "High", "Urgent" };
                bool isValidPriority = false;
                for (int i = 0; i < validPriorities.Length; i++)
                {
                    if (validPriorities[i] == notification.Priority)
                    {
                        isValidPriority = true;
                        break;
                    }
                }
                if (!isValidPriority)
                    errors.Add("Priority must be Low, Normal, High, or Urgent");
            }

            if (!string.IsNullOrEmpty(notification.NotificationType))
            {
                string[] validTypes = new string[] { "System", "Alert", "Reminder", "Message" };
                bool isValidType = false;
                for (int i = 0; i < validTypes.Length; i++)
                {
                    if (validTypes[i] == notification.NotificationType)
                    {
                        isValidType = true;
                        break;
                    }
                }
                if (!isValidType)
                    errors.Add("Invalid notification type");
            }

            return errors;
        }

        #endregion

        #region Retrieval Methods

        /// <summary>
        /// Gets all notifications
        /// </summary>
        /// <returns>List of Notification objects</returns>
        public List<Notification> GetNotifications()
        {
            try
            {
                return notificationDAL.GetNotifications();
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Notifications", string.Format("GetNotifications failed: {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Gets notification by ID
        /// </summary>
        /// <param name="notificationId">The notification ID</param>
        /// <returns>Notification object or null if not found</returns>
        public Notification GetNotificationById(int notificationId)
        {
            try
            {
                if (notificationId <= 0)
                    throw new ArgumentException("Invalid notification ID", "notificationId");

                return notificationDAL.GetNotificationById(notificationId);
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Notifications", string.Format("GetNotificationById failed for ID {0}: {1}", notificationId, ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Gets all notifications for a specific user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of Notification objects</returns>
        public List<Notification> GetUserNotifications(int userId)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID", "userId");

                return notificationDAL.GetUserNotifications(userId);
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Notifications", string.Format("GetUserNotifications failed for UserID {0}: {1}", userId, ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Gets unread notifications for a specific user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of unread Notification objects</returns>
        public List<Notification> GetUnreadNotifications(int userId)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID", "userId");

                return notificationDAL.GetUnreadNotifications(userId);
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Notifications", string.Format("GetUnreadNotifications failed for UserID {0}: {1}", userId, ex.Message));
                throw;
            }
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Creates a new notification
        /// </summary>
        /// <param name="notification">The notification to create</param>
        /// <param name="createdBy">ID of the user creating the notification</param>
        /// <returns>The ID of the newly created notification</returns>
        public int AddNotification(Notification notification, int createdBy)
        {
            try
            {
                var errors = ValidateNotification(notification);
                if (errors.Count > 0)
                    throw new ValidationException(string.Join(", ", errors));

                notification.IsRead = false;
                notification.ReadAt = null;
                notification.CreatedAt = DateTime.Now;
                notification.CreatedBy = createdBy;

                int newId = notificationDAL.AddNotification(notification);

                auditLogger.LogCreate(createdBy, "Notifications", "Notification", newId.ToString(),
                    string.Format("Created notification '{0}' for UserID {1}", notification.Title, notification.UserID));

                return newId;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(createdBy, "ERROR", "Notifications", string.Format("AddNotification failed: {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Updates an existing notification
        /// </summary>
        /// <param name="notification">The notification with updated values</param>
        /// <param name="updatedBy">ID of the user updating the notification</param>
        /// <returns>True if update succeeded</returns>
        public bool UpdateNotification(Notification notification, int updatedBy)
        {
            try
            {
                if (notification == null)
                    throw new ArgumentNullException("notification");

                if (notification.NotificationID <= 0)
                    throw new ArgumentException("Invalid notification ID", "notification");

                var errors = ValidateNotification(notification);
                if (errors.Count > 0)
                    throw new ValidationException(string.Join(", ", errors));

                var oldNotification = notificationDAL.GetNotificationById(notification.NotificationID);
                if (oldNotification == null)
                    throw new Exception("Notification not found");

                bool result = notificationDAL.UpdateNotification(notification);

                if (result)
                {
                    if (oldNotification.Title != notification.Title)
                    {
                        auditLogger.LogUpdate(updatedBy, "Notifications", "Notification", notification.NotificationID.ToString(),
                            "Title", oldNotification.Title, notification.Title);
                    }
                    if (oldNotification.IsRead != notification.IsRead)
                    {
                        auditLogger.LogUpdate(updatedBy, "Notifications", "Notification", notification.NotificationID.ToString(),
                            "IsRead", oldNotification.IsRead.ToString(), notification.IsRead.ToString());
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(updatedBy, "ERROR", "Notifications", string.Format("UpdateNotification failed for ID {0}: {1}", notification.NotificationID, ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Deletes a notification
        /// </summary>
        /// <param name="notificationId">The notification ID to delete</param>
        /// <param name="deletedBy">ID of the user deleting the notification</param>
        /// <returns>True if deletion succeeded</returns>
        public bool DeleteNotification(int notificationId, int deletedBy)
        {
            try
            {
                if (notificationId <= 0)
                    throw new ArgumentException("Invalid notification ID", "notificationId");

                var notification = notificationDAL.GetNotificationById(notificationId);
                if (notification == null)
                    throw new Exception("Notification not found");

                bool result = notificationDAL.DeleteNotification(notificationId);

                if (result)
                {
                    auditLogger.LogDelete(deletedBy, "Notifications", "Notification", notificationId.ToString(),
                        string.Format("Deleted notification '{0}'", notification.Title));
                }

                return result;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(deletedBy, "ERROR", "Notifications", string.Format("DeleteNotification failed for ID {0}: {1}", notificationId, ex.Message));
                throw;
            }
        }

        #endregion

        #region Status Operations

        /// <summary>
        /// Marks a notification as read
        /// </summary>
        /// <param name="notificationId">The notification ID</param>
        /// <param name="readBy">ID of the user marking as read</param>
        /// <returns>True if update succeeded</returns>
        public bool MarkAsRead(int notificationId, int readBy)
        {
            try
            {
                if (notificationId <= 0)
                    throw new ArgumentException("Invalid notification ID", "notificationId");

                var notification = notificationDAL.GetNotificationById(notificationId);
                if (notification == null)
                    throw new Exception("Notification not found");

                if (notification.IsRead)
                    throw new InvalidOperationException("Notification is already marked as read");

                bool result = notificationDAL.MarkAsRead(notificationId);

                if (result)
                {
                    auditLogger.LogAction(readBy, "MARK_READ", "Notifications",
                        string.Format("Marked notification '{0}' as read", notification.Title));
                }

                return result;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(readBy, "ERROR", "Notifications", string.Format("MarkAsRead failed for ID {0}: {1}", notificationId, ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Marks all notifications as read for a specific user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="markedBy">ID of the user marking all as read</param>
        /// <returns>Number of notifications updated</returns>
        public int MarkAllAsRead(int userId, int markedBy)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID", "userId");

                int count = notificationDAL.MarkAllAsRead(userId);

                if (count > 0)
                {
                    auditLogger.LogAction(markedBy, "MARK_ALL_READ", "Notifications",
                        string.Format("Marked {0} notifications as read for UserID {1}", count, userId));
                }

                return count;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(markedBy, "ERROR", "Notifications", string.Format("MarkAllAsRead failed for UserID {0}: {1}", userId, ex.Message));
                throw;
            }
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
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID", "userId");

                return notificationDAL.GetNotificationCount(userId);
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Notifications", string.Format("GetNotificationCount failed for UserID {0}: {1}", userId, ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Searches notifications by keyword
        /// </summary>
        /// <param name="keyword">Search keyword for title or message</param>
        /// <returns>List of matching Notification objects</returns>
        public List<Notification> SearchNotifications(string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                    throw new ArgumentException("Search keyword cannot be empty", "keyword");

                return notificationDAL.SearchNotifications(keyword);
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Notifications", string.Format("SearchNotifications failed: {0}", ex.Message));
                throw;
            }
        }

        #endregion
    }
}