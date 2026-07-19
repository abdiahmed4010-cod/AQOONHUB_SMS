using System;

namespace AQOONHUB.Models
{
    /// <summary>
    /// Represents a system setting
    /// </summary>
    public class Settings
    {
        private int _settingID;
        private string _settingKey;
        private string _settingValue;
        private string _category;
        private string _description;
        private string _dataType;
        private bool _isEditable;
        private int _updatedBy;
        private string _updatedByName;
        private DateTime _updatedAt;
        private DateTime _createdAt;

        public int SettingID
        {
            get { return _settingID; }
            set { _settingID = value; }
        }

        public string SettingKey
        {
            get { return _settingKey; }
            set { _settingKey = value; }
        }

        public string SettingValue
        {
            get { return _settingValue; }
            set { _settingValue = value; }
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

        public string DataType
        {
            get { return _dataType; }
            set { _dataType = value; }
        }

        public bool IsEditable
        {
            get { return _isEditable; }
            set { _isEditable = value; }
        }

        public int UpdatedBy
        {
            get { return _updatedBy; }
            set { _updatedBy = value; }
        }

        public string UpdatedByName
        {
            get { return _updatedByName; }
            set { _updatedByName = value; }
        }

        public DateTime UpdatedAt
        {
            get { return _updatedAt; }
            set { _updatedAt = value; }
        }

        public DateTime CreatedAt
        {
            get { return _createdAt; }
            set { _createdAt = value; }
        }

        public bool IsSystemSetting
        {
            get { return _category == "System"; }
        }

        public string DisplayValue
        {
            get
            {
                if (string.IsNullOrEmpty(_settingValue))
                    return "[Not Set]";

                if (_dataType == "Boolean")
                {
                    if (_settingValue.ToLower() == "true" || _settingValue == "1")
                        return "Yes";
                    else
                        return "No";
                }

                if (_dataType == "Password" || _dataType == "Secret")
                {
                    return "********";
                }

                return _settingValue;
            }
        }
    }
}