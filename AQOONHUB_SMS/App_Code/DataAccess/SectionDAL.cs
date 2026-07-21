using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace AQOONHUB_SMS.App_Code.DataAccess
{
    public class SectionDAL
    {
        private DatabaseHelper db;

        public SectionDAL()
        {
            db = new DatabaseHelper();
        }

        /// <summary>
        /// Gets all sections with optional filters
        /// </summary>
        public List<Section> GetAllSections(int classId, int teacherId, string status)
        {
            List<Section> sections = new List<Section>();

            string query = @"
                SELECT s.*, c.ClassName, t.FullName as TeacherName,
                       (SELECT COUNT(*) FROM Students st WHERE st.SectionID = s.SectionID AND st.Status = 'Active') as CurrentEnrollment
                FROM Sections s
                INNER JOIN Classes c ON s.ClassID = c.ClassID
                LEFT JOIN Teachers t ON s.TeacherID = t.TeacherID
                WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (classId > 0)
            {
                query += " AND s.ClassID = @ClassID";
                parameters.Add(new SqlParameter("@ClassID", classId));
            }

            if (teacherId > 0)
            {
                query += " AND s.TeacherID = @TeacherID";
                parameters.Add(new SqlParameter("@TeacherID", teacherId));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query += " AND s.Status = @Status";
                parameters.Add(new SqlParameter("@Status", status));
            }

            query += " ORDER BY c.GradeLevel, c.ClassName, s.SectionName";

            DataTable dt = db.ExecuteQuery(query, parameters.ToArray());

            foreach (DataRow row in dt.Rows)
            {
                sections.Add(MapToSection(row));
            }

            return sections;
        }

        /// <summary>
        /// Gets all sections
        /// </summary>
        public List<Section> GetAllSections()
        {
            return GetAllSections(0, 0, null);
        }

        /// <summary>
        /// Gets sections by class ID
        /// </summary>
        public List<Section> GetSectionsByClass(int classId)
        {
            List<Section> sections = new List<Section>();

            string query = @"
                SELECT s.*, c.ClassName, t.FullName as TeacherName,
                       (SELECT COUNT(*) FROM Students st WHERE st.SectionID = s.SectionID AND st.Status = 'Active') as CurrentEnrollment
                FROM Sections s
                INNER JOIN Classes c ON s.ClassID = c.ClassID
                LEFT JOIN Teachers t ON s.TeacherID = t.TeacherID
                WHERE s.ClassID = @ClassID
                ORDER BY s.SectionName";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClassID", classId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                sections.Add(MapToSection(row));
            }

            return sections;
        }

        /// <summary>
        /// Gets section enrollment count
        /// </summary>
        public int GetSectionEnrollment(int sectionId)
        {
            string query = @"
                SELECT COUNT(*)
                FROM Students
                WHERE SectionID = @SectionID
                AND Status = 'Active'";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SectionID", sectionId)
            };

            object result = db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        /// <summary>
        /// Gets section by ID
        /// </summary>
        public Section GetSectionById(int sectionId)
        {
            string query = @"
                SELECT s.*, c.ClassName, t.FullName as TeacherName,
                       (SELECT COUNT(*) FROM Students st WHERE st.SectionID = s.SectionID AND st.Status = 'Active') as CurrentEnrollment
                FROM Sections s
                INNER JOIN Classes c ON s.ClassID = c.ClassID
                LEFT JOIN Teachers t ON s.TeacherID = t.TeacherID
                WHERE s.SectionID = @SectionID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SectionID", sectionId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
                return MapToSection(dt.Rows[0]);

            return null;
        }

        /// <summary>
        /// Adds a new section
        /// </summary>
        public int AddSection(Section section)
        {
            string query = @"
                INSERT INTO Sections (SectionName, ClassID, TeacherID, Capacity, RoomNumber, Status, CreatedAt, UpdatedAt)
                VALUES (@SectionName, @ClassID, @TeacherID, @Capacity, @RoomNumber, @Status, GETDATE(), GETDATE());
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SectionName", section.SectionName),
                new SqlParameter("@ClassID", section.ClassID),
                new SqlParameter("@TeacherID", section.TeacherID),
                new SqlParameter("@Capacity", section.Capacity),
                new SqlParameter("@RoomNumber", string.IsNullOrEmpty(section.RoomNumber) ? (object)DBNull.Value : section.RoomNumber),
                new SqlParameter("@Status", section.Status)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        /// <summary>
        /// Updates an existing section
        /// </summary>
        public bool UpdateSection(Section section)
        {
            string query = @"
                UPDATE Sections SET
                    SectionName = @SectionName,
                    ClassID = @ClassID,
                    TeacherID = @TeacherID,
                    Capacity = @Capacity,
                    RoomNumber = @RoomNumber,
                    Status = @Status,
                    UpdatedAt = GETDATE()
                WHERE SectionID = @SectionID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SectionID", section.SectionID),
                new SqlParameter("@SectionName", section.SectionName),
                new SqlParameter("@ClassID", section.ClassID),
                new SqlParameter("@TeacherID", section.TeacherID),
                new SqlParameter("@Capacity", section.Capacity),
                new SqlParameter("@RoomNumber", string.IsNullOrEmpty(section.RoomNumber) ? (object)DBNull.Value : section.RoomNumber),
                new SqlParameter("@Status", section.Status)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Soft deletes a section
        /// </summary>
        public bool SoftDeleteSection(int sectionId)
        {
            string query = "UPDATE Sections SET Status = 'Inactive', UpdatedAt = GETDATE() WHERE SectionID = @SectionID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SectionID", sectionId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        private Section MapToSection(DataRow row)
        {
            Section section = new Section();
            section.SectionID = Convert.ToInt32(row["SectionID"]);
            section.SectionName = row["SectionName"].ToString();
            section.ClassID = Convert.ToInt32(row["ClassID"]);
            section.ClassName = row["ClassName"].ToString();
            section.TeacherID = row["TeacherID"] == DBNull.Value ? 0 : Convert.ToInt32(row["TeacherID"]);
            section.TeacherName = row["TeacherName"] == DBNull.Value ? null : row["TeacherName"].ToString();
            section.Capacity = Convert.ToInt32(row["Capacity"]);
            section.CurrentEnrollment = Convert.ToInt32(row["CurrentEnrollment"]);
            section.RoomNumber = row["RoomNumber"] == DBNull.Value ? null : row["RoomNumber"].ToString();
            section.Status = row["Status"].ToString();
            section.CreatedAt = Convert.ToDateTime(row["CreatedAt"]);
            section.UpdatedAt = Convert.ToDateTime(row["UpdatedAt"]);
            return section;
        }
    }
}