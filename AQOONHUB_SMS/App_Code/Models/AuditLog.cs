using System;

namespace AQOONHUB_SMS.App_Code.Models
{
    /// <summary>
    /// Represents an audit log entry for tracking system changes
    /// </summary>
    public class AuditLog
    {
        private int _auditLogID;
        private int _userID;
        private string _action;
        private string _tableName;
        private string _recordID;
        private string _oldValue;
        private string _newValue;
        private string _ipAddress;
        private string _userAgent;
        private DateTime _createdAt;

        public int AuditLogID
        {
            get { return _auditLogID; }
            set { _auditLogID = value; }
        }

        public int UserID
        {
            get { return _userID; }
            set { _userID = value; }
        }

        public string Action
        {
            get { return _action; }
            set { _action = value; }
        }

        public string TableName
        {
            get { return _tableName; }
            set { _tableName = value; }
        }

        public string RecordID
        {
            get { return _recordID; }
            set { _recordID = value; }
        }

        public string OldValue
        {
            get { return _oldValue; }
            set { _oldValue = value; }
        }

        public string NewValue
        {
            get { return _newValue; }
            set { _newValue = value; }
        }

        public string IPAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; }
        }

        public string UserAgent
        {
            get { return _userAgent; }
            set { _userAgent = value; }
        }

        public DateTime CreatedAt
        {
            get { return _createdAt; }
            set { _createdAt = value; }
        }

        public string ActionBadgeClass
        {
            get
            {
                switch (_action)
                {
                    case "CREATE": return "bg-success";
                    case "UPDATE": return "bg-info";
                    case "DELETE": return "bg-danger";
                    case "LOGIN": return "bg-primary";
                    case "LOGOUT": return "bg-secondary";
                    case "PROMOTE": return "bg-warning";
                    case "TRANSFER": return "bg-warning";
                    case "SUSPEND": return "bg-dark";
                    case "REACTIVATE": return "bg-success";
                    case "COMPLETE": return "bg-success";
                    case "FAIL": return "bg-danger";
                    case "REGENERATE": return "bg-info";
                    default: return "bg-secondary";
                }
            }
        }

        public bool HasChanges
        {
            get { return !string.IsNullOrEmpty(_oldValue) || !string.IsNullOrEmpty(_newValue); }
        }
    }
}