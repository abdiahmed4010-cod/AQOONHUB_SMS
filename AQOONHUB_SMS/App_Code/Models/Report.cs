using System;

namespace AQOONHUB.Models
{
    /// <summary>
    /// Represents a system report
    /// </summary>
    public class Report
    {
        private int _reportID;
        private string _reportName;
        private string _reportType;
        private string _description;
        private string _parameters;
        private string _filePath;
        private string _status;
        private int _generatedBy;
        private string _generatedByName;
        private DateTime _generatedAt;
        private DateTime _createdAt;

        public int ReportID
        {
            get { return _reportID; }
            set { _reportID = value; }
        }

        public string ReportName
        {
            get { return _reportName; }
            set { _reportName = value; }
        }

        public string ReportType
        {
            get { return _reportType; }
            set { _reportType = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public string Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; }
        }

        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public int GeneratedBy
        {
            get { return _generatedBy; }
            set { _generatedBy = value; }
        }

        public string GeneratedByName
        {
            get { return _generatedByName; }
            set { _generatedByName = value; }
        }

        public DateTime GeneratedAt
        {
            get { return _generatedAt; }
            set { _generatedAt = value; }
        }

        public DateTime CreatedAt
        {
            get { return _createdAt; }
            set { _createdAt = value; }
        }

        public string StatusBadgeClass
        {
            get
            {
                switch (_status)
                {
                    case "Completed": return "success";
                    case "Processing": return "info";
                    case "Failed": return "danger";
                    case "Pending": return "warning";
                    default: return "secondary";
                }
            }
        }

        public string ReportIcon
        {
            get
            {
                switch (_reportType)
                {
                    case "PDF": return "fa-file-pdf";
                    case "Excel": return "fa-file-excel";
                    case "CSV": return "fa-file-csv";
                    case "Word": return "fa-file-word";
                    default: return "fa-file-alt";
                }
            }
        }

        public bool IsCompleted
        {
            get { return _status == "Completed"; }
        }
    }
}