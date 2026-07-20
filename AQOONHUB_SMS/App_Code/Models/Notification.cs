using System;

namespace AQOONHUB_SMS.App_Code.Models
{
    /// <summary>
    /// Represents a system notification for users
    /// </summary>
    public class Notification
    {
        private int _notificationID;
        private int _userID;
        private string _title;
        private string _message;
        private string _notificationType;
        private string _priority;
        private bool _isRead;
        private DateTime? _readAt;
        private string _linkUrl;
        private string _icon;
        private int _createdBy;
        private DateTime _createdAt;
        private string _createdByName;

        public int NotificationID
        {
            get { return _notificationID; }
            set { _notificationID = value; }
        }

        public int UserID
        {
            get { return _userID; }
            set { _userID = value; }
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public string NotificationType
        {
            get { return _notificationType; }
            set { _notificationType = value; }
        }

        public string Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        public bool IsRead
        {
            get { return _isRead; }
            set { _isRead = value; }
        }

        public DateTime? ReadAt
        {
            get { return _readAt; }
            set { _readAt = value; }
        }

        public string LinkUrl
        {
            get { return _linkUrl; }
            set { _linkUrl = value; }
        }

        public string Icon
        {
            get { return _icon; }
            set { _icon = value; }
        }

        public int CreatedBy
        {
            get { return _createdBy; }
            set { _createdBy = value; }
        }

        public DateTime CreatedAt
        {
            get { return _createdAt; }
            set { _createdAt = value; }
        }

        public string CreatedByName
        {
            get { return _createdByName; }
            set { _createdByName = value; }
        }

        public string TimeAgo
        {
            get
            {
                TimeSpan span = DateTime.Now - _createdAt;
                if (span.TotalMinutes < 1)
                    return "Just now";
                else if (span.TotalMinutes < 60)
                    return string.Format("{0} minutes ago", (int)span.TotalMinutes);
                else if (span.TotalHours < 24)
                    return string.Format("{0} hours ago", (int)span.TotalHours);
                else if (span.TotalDays < 7)
                    return string.Format("{0} days ago", (int)span.TotalDays);
                else
                    return _createdAt.ToString("yyyy-MM-dd");
            }
        }

        public string PriorityBadgeClass
        {
            get
            {
                switch (_priority)
                {
                    case "Urgent": return "danger";
                    case "High": return "warning";
                    case "Normal": return "info";
                    case "Low": return "secondary";
                    default: return "secondary";
                }
            }
        }
    }
}