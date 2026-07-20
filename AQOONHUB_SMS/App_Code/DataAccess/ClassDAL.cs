
using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace AQOONHUB_SMS.App_Code.DataAccess
{
    public class ClassDAL
    {
        private DatabaseHelper db;

        public ClassDAL()
        {
            db = new DatabaseHelper();
        }

        /// <summary>
        /// Gets all classes with optional filters
        /// </summary>
        public List<Class> GetAllClasses(string status)
        {
            List<Class> classes = new List<Class>();

            string query = @"
                SELECT * FROM Classes
                WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(status))
            {
                query += " AND Status = @Status";
                parameters.Add(new SqlParameter("@Status", status));
            }

            query += " ORDER BY GradeLevel, ClassName";

            DataTable dt = db.ExecuteQuery(query, parameters.ToArray());

            foreach (DataRow row in dt.Rows)
            {
                classes.Add(MapToClass(row));
            }

            return classes;
        }

        /// <summary>
        /// Gets all classes
        /// </summary>
        public List<Class> GetAllClasses()
        {
            return GetAllClasses(null);
        }

        /// <summary>
        /// Gets class by ID
        /// </summary>
        public Class GetClassById(int classId)
        {
            string query = "SELECT * FROM Classes WHERE ClassID = @ClassID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClassID", classId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
                return MapToClass(dt.Rows[0]);

            return null;
        }

        /// <summary>
        /// Adds a new class
        /// </summary>
        public int AddClass(Class cls)
        {
            string query = @"
                INSERT INTO Classes (ClassName, ClassCode, GradeLevel, Description, Status, CreatedAt, UpdatedAt)
                VALUES (@ClassName, @ClassCode, @GradeLevel, @Description, @Status, GETDATE(), GETDATE());
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClassName", cls.ClassName),
                new SqlParameter("@ClassCode", cls.ClassCode),
                new SqlParameter("@GradeLevel", cls.GradeLevel),
                new SqlParameter("@Description", string.IsNullOrEmpty(cls.Description) ? (object)DBNull.Value : cls.Description),
                new SqlParameter("@Status", cls.Status)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        /// <summary>
        /// Updates an existing class
        /// </summary>
        public bool UpdateClass(Class cls)
        {
            string query = @"
                UPDATE Classes SET
                    ClassName = @ClassName,
                    ClassCode = @ClassCode,
                    GradeLevel = @GradeLevel,
                    Description = @Description,
                    Status = @Status,
                    UpdatedAt = GETDATE()
                WHERE ClassID = @ClassID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClassID", cls.ClassID),
                new SqlParameter("@ClassName", cls.ClassName),
                new SqlParameter("@ClassCode", cls.ClassCode),
                new SqlParameter("@GradeLevel", cls.GradeLevel),
                new SqlParameter("@Description", string.IsNullOrEmpty(cls.Description) ? (object)DBNull.Value : cls.Description),
                new SqlParameter("@Status", cls.Status)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Soft deletes a class
        /// </summary>
        public bool SoftDeleteClass(int classId)
        {
            string query = "UPDATE Classes SET Status = 'Inactive', UpdatedAt = GETDATE() WHERE ClassID = @ClassID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClassID", classId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        private Class MapToClass(DataRow row)
        {
            Class cls = new Class();
            cls.ClassID = Convert.ToInt32(row["ClassID"]);
            cls.ClassName = row["ClassName"].ToString();
            cls.ClassCode = row["ClassCode"].ToString();
            cls.GradeLevel = Convert.ToInt32(row["GradeLevel"]);
            cls.Description = row["Description"] == DBNull.Value ? null : row["Description"].ToString();
            cls.Status = row["Status"].ToString();
            cls.CreatedAt = Convert.ToDateTime(row["CreatedAt"]);
            cls.UpdatedAt = Convert.ToDateTime(row["UpdatedAt"]);
            return cls;
        }
    }
}