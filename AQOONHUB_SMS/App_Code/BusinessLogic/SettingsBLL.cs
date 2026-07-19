using System;
using System.Collections.Generic;
using AQOONHUB.DataAccess;
using AQOONHUB.Models;
using AQOONHUB.Utilities;

namespace AQOONHUB.BusinessLogic
{
    public class SettingsBLL
    {
        private SettingsDAL settingsDAL;
        private AuditLogger auditLogger;

        public SettingsBLL()
        {
            settingsDAL = new SettingsDAL();
            auditLogger = new AuditLogger();
        }

        #region Validation

        /// <summary>
        /// Validates setting data before save
        /// </summary>
        private List<string> ValidateSetting(Settings setting, bool isNew)
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(setting.SettingKey))
                errors.Add("Setting key is required");

            if (string.IsNullOrWhiteSpace(setting.SettingValue))
                errors.Add("Setting value is required");

            if (string.IsNullOrWhiteSpace(setting.Category))
                errors.Add("Category is required");

            if (string.IsNullOrWhiteSpace(setting.DataType))
                errors.Add("Data type is required");

            if (setting.UpdatedBy <= 0)
                errors.Add("Updated by user is required");

            return errors;
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Gets all settings with optional filters
        /// </summary>
        public List<Settings> GetSettings(string category = null, string search = null)
        {
            return settingsDAL.GetAllSettings(category, search);
        }

        /// <summary>
        /// Gets setting by ID
        /// </summary>
        public Settings GetSetting(int settingId)
        {
            if (settingId <= 0)
                throw new ArgumentException("Invalid setting ID");

            return settingsDAL.GetSettingById(settingId);
        }

        /// <summary>
        /// Gets setting by key
        /// </summary>
        public Settings GetSettingByKey(string settingKey)
        {
            if (string.IsNullOrWhiteSpace(settingKey))
                throw new ArgumentException("Setting key is required");

            return settingsDAL.GetSettingByKey(settingKey);
        }

        /// <summary>
        /// Creates new setting
        /// </summary>
        public Settings CreateSetting(Settings setting, int createdBy, string ipAddress)
        {
            // Validate
            var errors = ValidateSetting(setting, true);
            if (errors.Count > 0)
                throw new ValidationException(string.Join(", ", errors));

            // Check for duplicate key
            var existing = settingsDAL.GetSettingByKey(setting.SettingKey);
            if (existing != null)
                throw new Exception("Setting key already exists");

            // Set defaults
            setting.IsEditable = true;
            setting.UpdatedAt = DateTime.Now;
            setting.CreatedAt = DateTime.Now;

            // Save to database
            int newId = settingsDAL.AddSetting(setting);
            setting.SettingID = newId;

            // Log audit
            auditLogger.LogCreate(createdBy, "Settings", "Setting", setting.SettingKey,
                string.Format("Created setting: {0} in category {1}", setting.SettingKey, setting.Category));

            return setting;
        }

        /// <summary>
        /// Updates existing setting
        /// </summary>
        public bool UpdateSetting(Settings setting, int updatedBy, string ipAddress)
        {
            var errors = ValidateSetting(setting, false);
            if (errors.Count > 0)
                throw new ValidationException(string.Join(", ", errors));

            // Get old data for audit
            var oldSetting = settingsDAL.GetSettingById(setting.SettingID);
            if (oldSetting == null)
                throw new Exception("Setting not found");

            // Check if editable
            if (!oldSetting.IsEditable)
                throw new Exception("This setting is not editable");

            // Update
            setting.UpdatedAt = DateTime.Now;
            bool result = settingsDAL.UpdateSetting(setting);

            if (result)
            {
                // Log changes
                if (oldSetting.SettingValue != setting.SettingValue)
                {
                    auditLogger.LogUpdate(updatedBy, "Settings", "Setting", setting.SettingKey,
                        "Value", oldSetting.SettingValue, setting.SettingValue);
                }
                if (oldSetting.Category != setting.Category)
                {
                    auditLogger.LogUpdate(updatedBy, "Settings", "Setting", setting.SettingKey,
                        "Category", oldSetting.Category, setting.Category);
                }
            }

            return result;
        }

        /// <summary>
        /// Updates only the setting value
        /// </summary>
        public bool UpdateSettingValue(int settingId, string newValue, int updatedBy, string ipAddress)
        {
            if (settingId <= 0)
                throw new ArgumentException("Invalid setting ID");

            var setting = settingsDAL.GetSettingById(settingId);
            if (setting == null)
                throw new Exception("Setting not found");

            if (!setting.IsEditable)
                throw new Exception("This setting is not editable");

            string oldValue = setting.SettingValue;

            bool result = settingsDAL.UpdateSettingValue(settingId, newValue, updatedBy);

            if (result)
            {
                auditLogger.LogUpdate(updatedBy, "Settings", "Setting", setting.SettingKey,
                    "Value", oldValue, newValue);
            }

            return result;
        }

        /// <summary>
        /// Deletes setting
        /// </summary>
        public bool DeleteSetting(int settingId, int deletedBy, string ipAddress, string reason)
        {
            var setting = settingsDAL.GetSettingById(settingId);
            if (setting == null)
                throw new Exception("Setting not found");

            if (setting.IsSystemSetting)
                throw new Exception("System settings cannot be deleted");

            bool result = settingsDAL.DeleteSetting(settingId);

            if (result)
            {
                auditLogger.LogDelete(deletedBy, "Settings", "Setting", setting.SettingKey, reason);
            }

            return result;
        }

        #endregion

        #region Business Rules

        /// <summary>
        /// Gets setting value by key
        /// </summary>
        public string GetValue(string settingKey)
        {
            var setting = settingsDAL.GetSettingByKey(settingKey);
            if (setting == null)
                return null;

            return setting.SettingValue;
        }

        /// <summary>
        /// Gets setting value as boolean
        /// </summary>
        public bool GetBoolValue(string settingKey)
        {
            var setting = settingsDAL.GetSettingByKey(settingKey);
            if (setting == null)
                return false;

            if (setting.DataType == "Boolean")
            {
                return setting.SettingValue.ToLower() == "true" || setting.SettingValue == "1";
            }

            return false;
        }

        /// <summary>
        /// Gets setting value as integer
        /// </summary>
        public int GetIntValue(string settingKey)
        {
            var setting = settingsDAL.GetSettingByKey(settingKey);
            if (setting == null)
                return 0;

            int result;
            if (int.TryParse(setting.SettingValue, out result))
                return result;

            return 0;
        }

        /// <summary>
        /// Gets setting value as decimal
        /// </summary>
        public decimal GetDecimalValue(string settingKey)
        {
            var setting = settingsDAL.GetSettingByKey(settingKey);
            if (setting == null)
                return 0;

            decimal result;
            if (decimal.TryParse(setting.SettingValue, out result))
                return result;

            return 0;
        }

        /// <summary>
        /// Sets or updates a setting value by key
        /// </summary>
        public bool SetValue(string settingKey, string value, string category, int updatedBy)
        {
            var setting = settingsDAL.GetSettingByKey(settingKey);

            if (setting == null)
            {
                // Create new setting
                Settings newSetting = new Settings();
                newSetting.SettingKey = settingKey;
                newSetting.SettingValue = value;
                newSetting.Category = category;
                newSetting.DataType = "String";
                newSetting.IsEditable = true;
                newSetting.UpdatedBy = updatedBy;
                newSetting.UpdatedAt = DateTime.Now;
                newSetting.CreatedAt = DateTime.Now;

                CreateSetting(newSetting, updatedBy, null);
                return true;
            }
            else
            {
                return UpdateSettingValue(setting.SettingID, value, updatedBy, null);
            }
        }

        /// <summary>
        /// Gets settings by category
        /// </summary>
        public List<Settings> GetSettingsByCategory(string category)
        {
            return settingsDAL.GetAllSettings(category, null);
        }

        #endregion
    }
}