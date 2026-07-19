using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using AQOONHUB.Models;

namespace AQOONHUB.DataAccess
{
    public class AcademicYearDAL
    {
        private DatabaseHelper db;

        public AcademicYearDAL()
        {
            db = new DatabaseHelper();
        }

        /// <summary>
        /// Gets all academic years with optional filters
        /// </summary>
        public List<AcademicYear> GetAllAcademicYears(string status)
        {
            List<AcademicYear> academicYears = new List<AcademicYear>();

            string query = @"
                SELECT * FROM AcademicYears
                WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(status))
            {
                query += " AND Status = @Status";
                parameters.Add(new SqlParameter("@Status", status));
            }

            query += " ORDER BY StartDate DESC";

            DataTable dt = db.ExecuteQuery(query, parameters.ToArray());

            foreach (DataRow row in dt.Rows)
            {
                academicYears.Add(MapToAcademicYear(row));
            }

            return academicYears;
        }

        /// <summary>
        /// Gets all academic years
        /// </summary>
        public List<AcademicYear> GetAllAcademicYears()
        {
            return GetAllAcademicYears(null);
        }

        /// <summary>
        /// Gets academic year by ID
        /// </summary>
        public AcademicYear GetAcademicYearById(int academicYearId)
        {
            string query = "SELECT * FROM AcademicYears WHERE AcademicYearID = @AcademicYearID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@AcademicYearID", academicYearId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
                return MapToAcademicYear(dt.Rows[0]);

            return null;
        }

        /// <summary>
        /// Gets the current academic year
        /// </summary>
        public AcademicYear GetCurrentAcademicYear()
        {
            string query = "SELECT TOP 1 * FROM AcademicYears WHERE IsCurrent = 1 AND Status = 'Active' ORDER BY StartDate DESC";

            DataTable dt = db.ExecuteQuery(query);

            if (dt.Rows.Count > 0)
                return MapToAcademicYear(dt.Rows[0]);

            return null;
        }

        /// <summary>
        /// Adds a new academic year
        /// </summary>
        public int AddAcademicYear(AcademicYear academicYear)
        {
            string query = @"
                INSERT INTO AcademicYears (YearName, StartDate, EndDate, IsCurrent, Status, CreatedAt, UpdatedAt)
                VALUES (@YearName, @StartDate, @EndDate, @IsCurrent, @Status, GETDATE(), GETDATE());
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@YearName", academicYear.YearName),
                new SqlParameter("@StartDate", academicYear.StartDate),
                new SqlParameter("@EndDate", academicYear.EndDate),
                new SqlParameter("@IsCurrent", academicYear.IsCurrent),
                new SqlParameter("@Status", academicYear.Status)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        /// <summary>
        /// Updates an existing academic year
        /// </summary>
        public bool UpdateAcademicYear(AcademicYear academicYear)
        {
            string query = @"
                UPDATE AcademicYears SET
                    YearName = @YearName,
                    StartDate = @StartDate,
                    EndDate = @EndDate,
                    IsCurrent = @IsCurrent,
                    Status = @Status,
                    UpdatedAt = GETDATE()
                WHERE AcademicYearID = @AcademicYearID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@AcademicYearID", academicYear.AcademicYearID),
                new SqlParameter("@YearName", academicYear.YearName),
                new SqlParameter("@StartDate", academicYear.StartDate),
                new SqlParameter("@EndDate", academicYear.EndDate),
                new SqlParameter("@IsCurrent", academicYear.IsCurrent),
                new SqlParameter("@Status", academicYear.Status)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Soft deletes an academic year
        /// </summary>
        public bool SoftDeleteAcademicYear(int academicYearId)
        {
            string query = "UPDATE AcademicYears SET Status = 'Closed', UpdatedAt = GETDATE() WHERE AcademicYearID = @AcademicYearID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@AcademicYearID", academicYearId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Clears the current flag from all academic years
        /// </summary>
        public bool ClearCurrentFlag()
        {
            string query = "UPDATE AcademicYears SET IsCurrent = 0 WHERE IsCurrent = 1";
            return db.ExecuteNonQuery(query) > 0;
        }

        private AcademicYear MapToAcademicYear(DataRow row)
        {
            AcademicYear academicYear = new AcademicYear();
            academicYear.AcademicYearID = Convert.ToInt32(row["AcademicYearID"]);
            academicYear.YearName = row["YearName"].ToString();
            academicYear.StartDate = Convert.ToDateTime(row["StartDate"]);
            academicYear.EndDate = Convert.ToDateTime(row["EndDate"]);
            academicYear.IsCurrent = Convert.ToBoolean(row["IsCurrent"]);
            academicYear.Status = row["Status"].ToString();
            academicYear.CreatedAt = Convert.ToDateTime(row["CreatedAt"]);
            academicYear.UpdatedAt = Convert.ToDateTime(row["UpdatedAt"]);
            return academicYear;
        }
    }
}