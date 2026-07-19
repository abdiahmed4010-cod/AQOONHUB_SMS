using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using AQOONHUB.Models;

namespace AQOONHUB.DataAccess
{
    public class TermDAL
    {
        private DatabaseHelper db;

        public TermDAL()
        {
            db = new DatabaseHelper();
        }

        /// <summary>
        /// Gets all terms with optional filters
        /// </summary>
        public List<Term> GetAllTerms(int academicYearId, string status)
        {
            List<Term> terms = new List<Term>();

            string query = @"
                SELECT t.*, ay.YearName as AcademicYearName
                FROM Terms t
                INNER JOIN AcademicYears ay ON t.AcademicYearID = ay.AcademicYearID
                WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (academicYearId > 0)
            {
                query += " AND t.AcademicYearID = @AcademicYearID";
                parameters.Add(new SqlParameter("@AcademicYearID", academicYearId));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query += " AND t.Status = @Status";
                parameters.Add(new SqlParameter("@Status", status));
            }

            query += " ORDER BY t.StartDate DESC";

            DataTable dt = db.ExecuteQuery(query, parameters.ToArray());

            foreach (DataRow row in dt.Rows)
            {
                terms.Add(MapToTerm(row));
            }

            return terms;
        }

        /// <summary>
        /// Gets all terms
        /// </summary>
        public List<Term> GetAllTerms()
        {
            return GetAllTerms(0, null);
        }

        /// <summary>
        /// Gets term by ID
        /// </summary>
        public Term GetTermById(int termId)
        {
            string query = @"
                SELECT t.*, ay.YearName as AcademicYearName
                FROM Terms t
                INNER JOIN AcademicYears ay ON t.AcademicYearID = ay.AcademicYearID
                WHERE t.TermID = @TermID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@TermID", termId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
                return MapToTerm(dt.Rows[0]);

            return null;
        }

        /// <summary>
        /// Gets the current term
        /// </summary>
        public Term GetCurrentTerm()
        {
            string query = @"
                SELECT TOP 1 t.*, ay.YearName as AcademicYearName
                FROM Terms t
                INNER JOIN AcademicYears ay ON t.AcademicYearID = ay.AcademicYearID
                WHERE t.IsCurrent = 1 AND t.Status = 'Active'
                ORDER BY t.StartDate DESC";

            DataTable dt = db.ExecuteQuery(query);

            if (dt.Rows.Count > 0)
                return MapToTerm(dt.Rows[0]);

            return null;
        }

        /// <summary>
        /// Adds a new term
        /// </summary>
        public int AddTerm(Term term)
        {
            string query = @"
                INSERT INTO Terms (TermName, AcademicYearID, StartDate, EndDate, IsCurrent, Status, CreatedAt, UpdatedAt)
                VALUES (@TermName, @AcademicYearID, @StartDate, @EndDate, @IsCurrent, @Status, GETDATE(), GETDATE());
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@TermName", term.TermName),
                new SqlParameter("@AcademicYearID", term.AcademicYearID),
                new SqlParameter("@StartDate", term.StartDate),
                new SqlParameter("@EndDate", term.EndDate),
                new SqlParameter("@IsCurrent", term.IsCurrent),
                new SqlParameter("@Status", term.Status)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        /// <summary>
        /// Updates an existing term
        /// </summary>
        public bool UpdateTerm(Term term)
        {
            string query = @"
                UPDATE Terms SET
                    TermName = @TermName,
                    AcademicYearID = @AcademicYearID,
                    StartDate = @StartDate,
                    EndDate = @EndDate,
                    IsCurrent = @IsCurrent,
                    Status = @Status,
                    UpdatedAt = GETDATE()
                WHERE TermID = @TermID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@TermID", term.TermID),
                new SqlParameter("@TermName", term.TermName),
                new SqlParameter("@AcademicYearID", term.AcademicYearID),
                new SqlParameter("@StartDate", term.StartDate),
                new SqlParameter("@EndDate", term.EndDate),
                new SqlParameter("@IsCurrent", term.IsCurrent),
                new SqlParameter("@Status", term.Status)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Soft deletes a term
        /// </summary>
        public bool SoftDeleteTerm(int termId)
        {
            string query = "UPDATE Terms SET Status = 'Closed', UpdatedAt = GETDATE() WHERE TermID = @TermID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@TermID", termId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        private Term MapToTerm(DataRow row)
        {
            Term term = new Term();
            term.TermID = Convert.ToInt32(row["TermID"]);
            term.TermName = row["TermName"].ToString();
            term.AcademicYearID = Convert.ToInt32(row["AcademicYearID"]);
            term.AcademicYearName = row["AcademicYearName"].ToString();
            term.StartDate = Convert.ToDateTime(row["StartDate"]);
            term.EndDate = Convert.ToDateTime(row["EndDate"]);
            term.IsCurrent = Convert.ToBoolean(row["IsCurrent"]);
            term.Status = row["Status"].ToString();
            term.CreatedAt = Convert.ToDateTime(row["CreatedAt"]);
            term.UpdatedAt = Convert.ToDateTime(row["UpdatedAt"]);
            return term;
        }
    }
}