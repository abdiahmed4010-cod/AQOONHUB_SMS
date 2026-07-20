using System;

namespace AQOONHUB_SMS.App_Code.Models
{
    /// <summary>
    /// Represents a document or file stored in the system
    /// </summary>
    public class Document
    {
        private int _documentID;
        private string _documentName;
        private string _filePath;
        private string _fileType;
        private long _fileSize;
        private string _category;
        private string _description;
        private int _uploadedBy;
        private string _uploadedByName;
        private int _relatedID;
        private string _relatedType;
        private string _status;
        private DateTime _uploadedAt;

        public int DocumentID
        {
            get { return _documentID; }
            set { _documentID = value; }
        }

        public string DocumentName
        {
            get { return _documentName; }
            set { _documentName = value; }
        }

        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; }
        }

        public string FileType
        {
            get { return _fileType; }
            set { _fileType = value; }
        }

        public long FileSize
        {
            get { return _fileSize; }
            set { _fileSize = value; }
        }

        public string Category
        {
            get { return _category; }
            set { _category = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public int UploadedBy
        {
            get { return _uploadedBy; }
            set { _uploadedBy = value; }
        }

        public string UploadedByName
        {
            get { return _uploadedByName; }
            set { _uploadedByName = value; }
        }

        public int RelatedID
        {
            get { return _relatedID; }
            set { _relatedID = value; }
        }

        public string RelatedType
        {
            get { return _relatedType; }
            set { _relatedType = value; }
        }

        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public DateTime UploadedAt
        {
            get { return _uploadedAt; }
            set { _uploadedAt = value; }
        }

        public string FileSizeFormatted
        {
            get
            {
                if (_fileSize >= 1073741824)
                    return (_fileSize / 1073741824.0).ToString("F2") + " GB";
                else if (_fileSize >= 1048576)
                    return (_fileSize / 1048576.0).ToString("F2") + " MB";
                else if (_fileSize >= 1024)
                    return (_fileSize / 1024.0).ToString("F2") + " KB";
                else
                    return _fileSize.ToString() + " B";
            }
        }

        public string FileIconClass
        {
            get
            {
                switch (_fileType.ToLower())
                {
                    case "pdf": return "fa-file-pdf";
                    case "doc":
                    case "docx": return "fa-file-word";
                    case "xls":
                    case "xlsx": return "fa-file-excel";
                    case "ppt":
                    case "pptx": return "fa-file-powerpoint";
                    case "jpg":
                    case "jpeg":
                    case "png":
                    case "gif": return "fa-file-image";
                    case "zip":
                    case "rar": return "fa-file-archive";
                    default: return "fa-file-alt";
                }
            }
        }

        public string StatusBadgeClass
        {
            get
            {
                switch (_status)
                {
                    case "Active": return "bg-success";
                    case "Archived": return "bg-secondary";
                    case "Deleted": return "bg-danger";
                    default: return "bg-info";
                }
            }
        }
    }
}