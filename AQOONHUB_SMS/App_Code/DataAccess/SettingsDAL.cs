using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;


namespace AQOONHUB_SMS.App_Code.DataAccess
{
    public class SettingsDAL
    {
        private DatabaseHelper db;

        public SettingsDAL()
        {
            db = new DatabaseHelper();
        }

        public List<Settings> GetAllSettings(string category, string search)
        {
            List<Settings> settings = new List<Settings>();

            string query = @"
                SELECT s.*, u.FullName as UpdatedByName
                FROM Settings s
                INNER JOIN Users u ON s.UpdatedBy = u.UserID
                WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(category))
            {
                query += " AND s.Category = @Category";
                parameters.Add(new SqlParameter("@Category", category));
            }

            if (!string.IsNullOrEmpty(search))
            {
                query += @" AND (s.SettingKey LIKE @Search OR s.SettingValue LIKE @Search 
                          OR s.Description LIKE @Search)";
                parameters.Add(new SqlParameter("@Search", "%" + search + "%"));
            }

            query += " ORDER BY s.Category, s.SettingKey";

            DataTable dt = db.ExecuteQuery(query, parameters.ToArray());

            foreach (DataRow row in dt.Rows)
            {
                settings.Add(MapToSettings(row));
            }

            return settings;
        }

        public List<Settings> GetAllSettings()
        {
            return GetAllSettings(null, null);
        }

        public Settings GetSettingById(int settingId)
        {
            string query = @"
                SELECT s.*, u.FullName as UpdatedByName
                FROM Settings s
                INNER JOIN Users u ON s.UpdatedBy = u.UserID
                WHERE s.SettingID = @SettingID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SettingID", settingId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
                return MapToSettings(dt.Rows[0]);

            return null;
        }

        public Settings GetSettingByKey(string settingKey)
        {
            string query = @"
                SELECT s.*, u.FullName as UpdatedByName
                FROM Settings s
                INNER JOIN Users u ON s.UpdatedBy = u.UserID
                WHERE s.SettingKey = @SettingKey";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SettingKey", settingKey)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
                return MapToSettings(dt.Rows[0]);

            return null;
        }

        public int AddSetting(Settings setting)
        {
            string query = @"
                INSERT INTO Settings (SettingKey, SettingValue, Category, Description,
                    DataType, IsEditable, UpdatedBy, UpdatedAt, CreatedAt)
                VALUES (@SettingKey, @SettingValue, @Category, @Description,
                    @DataType, @IsEditable, @UpdatedBy, @UpdatedAt, @CreatedAt);
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SettingKey", setting.SettingKey),
                new SqlParameter("@SettingValue", string.IsNullOrEmpty(setting.SettingValue) ? (object)DBNull.Value : setting.SettingValue),
                new SqlParameter("@Category", string.IsNullOrEmpty(setting.Category) ? (object)DBNull.Value : setting.Category),
                new SqlParameter("@Description", string.IsNullOrEmpty(setting.Description) ? (object)DBNull.Value : setting.Description),
                new SqlParameter("@DataType", string.IsNullOrEmpty(setting.DataType) ? (object)DBNull.Value : setting.DataType),
                new SqlParameter("@IsEditable", setting.IsEditable),
                new SqlParameter("@UpdatedBy", setting.UpdatedBy),
                new SqlParameter("@UpdatedAt", setting.UpdatedAt),
                new SqlParameter("@CreatedAt", setting.CreatedAt)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        public bool UpdateSetting(Settings setting)
        {
            string query = @"
                UPDATE Settings SET
                    SettingKey = @SettingKey,
                    SettingValue = @SettingValue,
                    Category = @Category,
                    Description = @Description,
                    DataType = @DataType,
                    IsEditable = @IsEditable,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = @UpdatedAt
                WHERE SettingID = @SettingID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SettingID", setting.SettingID),
                new SqlParameter("@SettingKey", setting.SettingKey),
                new SqlParameter("@SettingValue", string.IsNullOrEmpty(setting.SettingValue) ? (object)DBNull.Value : setting.SettingValue),
                new SqlParameter("@Category", string.IsNullOrEmpty(setting.Category) ? (object)DBNull.Value : setting.Category),
                new SqlParameter("@Description", string.IsNullOrEmpty(setting.Description) ? (object)DBNull.Value : setting.Description),
                new SqlParameter("@DataType", string.IsNullOrEmpty(setting.DataType) ? (object)DBNull.Value : setting.DataType),
                new SqlParameter("@IsEditable", setting.IsEditable),
                new SqlParameter("@UpdatedBy", setting.UpdatedBy),
                new SqlParameter("@UpdatedAt", setting.UpdatedAt)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool UpdateSettingValue(int settingId, string settingValue, int updatedBy)
        {
            string query = @"
                UPDATE Settings SET
                    SettingValue = @SettingValue,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE SettingID = @SettingID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SettingID", settingId),
                new SqlParameter("@SettingValue", string.IsNullOrEmpty(settingValue) ? (object)DBNull.Value : settingValue),
                new SqlParameter("@UpdatedBy", updatedBy)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool DeleteSetting(int settingId)
        {
            string query = "DELETE FROM Settings WHERE SettingID = @SettingID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SettingID", settingId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        private Settings MapToSettings(DataRow row)
        {
            Settings setting = new Settings();
            setting.SettingID = Convert.ToInt32(row["SettingID"]);
            setting.SettingKey = row["SettingKey"].ToString();
            setting.SettingValue = row["SettingValue"] == DBNull.Value ? null : row["SettingValue"].ToString();
            setting.Category = row["Category"] == DBNull.Value ? null : row["Category"].ToString();
            setting.Description = row["Description"] == DBNull.Value ? null : row["Description"].ToString();
            setting.DataType = row["DataType"] == DBNull.Value ? null : row["DataType"].ToString();
            setting.IsEditable = Convert.ToBoolean(row["IsEditable"]);
            setting.UpdatedBy = Convert.ToInt32(row["UpdatedBy"]);
            setting.UpdatedByName = row["UpdatedByName"].ToString();
            setting.UpdatedAt = Convert.ToDateTime(row["UpdatedAt"]);
            setting.CreatedAt = Convert.ToDateTime(row["CreatedAt"]);
            return setting;
        }
    }
}