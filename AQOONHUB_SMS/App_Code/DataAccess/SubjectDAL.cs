using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;


namespace AQOONHUB_SMS.App_Code.DataAccess
{
    public class SubjectDAL
    {
        private DatabaseHelper db;

        public SubjectDAL()
        {
            db = new DatabaseHelper();
        }

        /// <summary>
        /// Gets all subjects with optional filters
        /// </summary>
        public List<Subject> GetAllSubjects(string category, string status)
        {
            List<Subject> subjects = new List<Subject>();

            string query = @"
                SELECT * FROM Subjects
                WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(category))
            {
                query += " AND Category = @Category";
                parameters.Add(new SqlParameter("@Category", category));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query += " AND Status = @Status";
                parameters.Add(new SqlParameter("@Status", status));
            }

            query += " ORDER BY Category, SubjectName";

            DataTable dt = db.ExecuteQuery(query, parameters.ToArray());

            foreach (DataRow row in dt.Rows)
            {
                subjects.Add(MapToSubject(row));
            }

            return subjects;
        }

        /// <summary>
        /// Gets all subjects
        /// </summary>
        public List<Subject> GetAllSubjects()
        {
            return GetAllSubjects(null, null);
        }

        /// <summary>
        /// Gets all subjects filtered by active status
        /// </summary>
        public List<Subject> GetAllSubjects(bool activeOnly)
        {
            if (activeOnly)
            {
                return GetAllSubjects(null, "Active");
            }
            else
            {
                return GetAllSubjects(null, null);
            }
        }

        /// <summary>
        /// Gets subject by ID
        /// </summary>
        public Subject GetSubjectById(int subjectId)
        {
            string query = "SELECT * FROM Subjects WHERE SubjectID = @SubjectID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SubjectID", subjectId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
                return MapToSubject(dt.Rows[0]);

            return null;
        }

        /// <summary>
        /// Adds a new subject
        /// </summary>
        public int AddSubject(Subject subject)
        {
            string query = @"
                INSERT INTO Subjects (SubjectName, SubjectCode, Description, CreditHours, Category, Status, CreatedAt, UpdatedAt)
                VALUES (@SubjectName, @SubjectCode, @Description, @CreditHours, @Category, @Status, GETDATE(), GETDATE());
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SubjectName", subject.SubjectName),
                new SqlParameter("@SubjectCode", subject.SubjectCode),
                new SqlParameter("@Description", string.IsNullOrEmpty(subject.Description) ? (object)DBNull.Value : subject.Description),
                new SqlParameter("@CreditHours", subject.CreditHours),
                new SqlParameter("@Category", string.IsNullOrEmpty(subject.Category) ? (object)DBNull.Value : subject.Category),
                new SqlParameter("@Status", subject.Status)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        /// <summary>
        /// Updates an existing subject
        /// </summary>
        public bool UpdateSubject(Subject subject)
        {
            string query = @"
                UPDATE Subjects SET
                    SubjectName = @SubjectName,
                    SubjectCode = @SubjectCode,
                    Description = @Description,
                    CreditHours = @CreditHours,
                    Category = @Category,
                    Status = @Status,
                    UpdatedAt = GETDATE()
                WHERE SubjectID = @SubjectID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SubjectID", subject.SubjectID),
                new SqlParameter("@SubjectName", subject.SubjectName),
                new SqlParameter("@SubjectCode", subject.SubjectCode),
                new SqlParameter("@Description", string.IsNullOrEmpty(subject.Description) ? (object)DBNull.Value : subject.Description),
                new SqlParameter("@CreditHours", subject.CreditHours),
                new SqlParameter("@Category", string.IsNullOrEmpty(subject.Category) ? (object)DBNull.Value : subject.Category),
                new SqlParameter("@Status", subject.Status)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Soft deletes a subject
        /// </summary>
        public bool SoftDeleteSubject(int subjectId)
        {
            string query = "UPDATE Subjects SET Status = 'Inactive', UpdatedAt = GETDATE() WHERE SubjectID = @SubjectID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SubjectID", subjectId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        private Subject MapToSubject(DataRow row)
        {
            Subject subject = new Subject();
            subject.SubjectID = Convert.ToInt32(row["SubjectID"]);
            subject.SubjectName = row["SubjectName"].ToString();
            subject.SubjectCode = row["SubjectCode"].ToString();
            subject.Description = row["Description"] == DBNull.Value ? null : row["Description"].ToString();
            subject.CreditHours = Convert.ToInt32(row["CreditHours"]);
            subject.Category = row["Category"] == DBNull.Value ? null : row["Category"].ToString();
            subject.Status = row["Status"].ToString();
            subject.CreatedAt = Convert.ToDateTime(row["CreatedAt"]);
            subject.UpdatedAt = Convert.ToDateTime(row["UpdatedAt"]);
            return subject;
        }
    }
}