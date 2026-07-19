using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using AQOONHUB.Models;

namespace AQOONHUB.DataAccess
{
    /// <summary>
    /// Data access layer for examination operations
    /// </summary>
    public class ExamDAL
    {
        private DatabaseHelper db;

        /// <summary>
        /// Initializes a new instance of the ExamDAL class
        /// </summary>
        public ExamDAL()
        {
            db = new DatabaseHelper();
        }

        #region Retrieval Methods

        /// <summary>
        /// Retrieves all exams from the database
        /// </summary>
        /// <returns>List of Exam objects</returns>
        public List<Exam> GetExams()
        {
            List<Exam> exams = new List<Exam>();

            string query = @"
                SELECT e.*, t.TermName, ay.YearName
                FROM Exams e
                INNER JOIN Terms t ON e.TermID = t.TermID
                INNER JOIN AcademicYears ay ON t.AcademicYearID = ay.AcademicYearID
                ORDER BY e.StartDate DESC";

            DataTable dt = db.ExecuteQuery(query);

            foreach (DataRow row in dt.Rows)
            {
                exams.Add(MapToExam(row));
            }

            return exams;
        }

        /// <summary>
        /// Retrieves a single exam by its ID
        /// </summary>
        /// <param name="examId">The exam ID</param>
        /// <returns>Exam object or null if not found</returns>
        public Exam GetExamById(int examId)
        {
            string query = @"
                SELECT e.*, t.TermName, ay.YearName
                FROM Exams e
                INNER JOIN Terms t ON e.TermID = t.TermID
                INNER JOIN AcademicYears ay ON t.AcademicYearID = ay.AcademicYearID
                WHERE e.ExamID = @ExamID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ExamID", examId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
                return MapToExam(dt.Rows[0]);

            return null;
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Adds a new exam to the database
        /// </summary>
        /// <param name="exam">The exam to add</param>
        /// <returns>The ID of the newly created exam</returns>
        public int AddExam(Exam exam)
        {
            string query = @"
                INSERT INTO Exams (ExamName, ExamType, TermID, StartDate, EndDate, Status, CreatedBy, CreatedAt)
                VALUES (@ExamName, @ExamType, @TermID, @StartDate, @EndDate, @Status, @CreatedBy, @CreatedAt);
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ExamName", exam.ExamName),
                new SqlParameter("@ExamType", exam.ExamType),
                new SqlParameter("@TermID", exam.TermID),
                new SqlParameter("@StartDate", exam.StartDate),
                new SqlParameter("@EndDate", exam.EndDate),
                new SqlParameter("@Status", exam.Status),
                new SqlParameter("@CreatedBy", exam.CreatedBy),
                new SqlParameter("@CreatedAt", exam.CreatedAt)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        /// <summary>
        /// Updates an existing exam
        /// </summary>
        /// <param name="exam">The exam with updated values</param>
        /// <returns>True if update succeeded</returns>
        public bool UpdateExam(Exam exam)
        {
            string query = @"
                UPDATE Exams SET
                    ExamName = @ExamName,
                    ExamType = @ExamType,
                    TermID = @TermID,
                    StartDate = @StartDate,
                    EndDate = @EndDate,
                    Status = @Status
                WHERE ExamID = @ExamID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ExamID", exam.ExamID),
                new SqlParameter("@ExamName", exam.ExamName),
                new SqlParameter("@ExamType", exam.ExamType),
                new SqlParameter("@TermID", exam.TermID),
                new SqlParameter("@StartDate", exam.StartDate),
                new SqlParameter("@EndDate", exam.EndDate),
                new SqlParameter("@Status", exam.Status)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Deletes an exam from the database
        /// </summary>
        /// <param name="examId">The exam ID to delete</param>
        /// <returns>True if deletion succeeded</returns>
        public bool DeleteExam(int examId)
        {
            string query = "DELETE FROM Exams WHERE ExamID = @ExamID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ExamID", examId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        #endregion

        #region Exam Status Operations

        /// <summary>
        /// Schedules an exam by updating its start date and status
        /// </summary>
        /// <param name="examId">The exam ID to schedule</param>
        /// <param name="examDate">The exam date</param>
        /// <returns>True if scheduling succeeded</returns>
        public bool ScheduleExam(int examId, DateTime examDate)
        {
            string query = @"
                UPDATE Exams SET
                    StartDate = @StartDate,
                    Status = 'Scheduled'
                WHERE ExamID = @ExamID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ExamID", examId),
                new SqlParameter("@StartDate", examDate)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Publishes an exam by updating its status
        /// </summary>
        /// <param name="examId">The exam ID to publish</param>
        /// <returns>True if publish succeeded</returns>
        public bool PublishExam(int examId)
        {
            string query = @"
                UPDATE Exams SET
                    Status = 'Published'
                WHERE ExamID = @ExamID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ExamID", examId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Cancels an exam by updating its status
        /// </summary>
        /// <param name="examId">The exam ID to cancel</param>
        /// <returns>True if cancellation succeeded</returns>
        public bool CancelExam(int examId)
        {
            string query = @"
                UPDATE Exams SET
                    Status = 'Cancelled'
                WHERE ExamID = @ExamID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ExamID", examId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        #endregion

        #region Marks Operations

        /// <summary>
        /// Enters marks for a student in an exam
        /// </summary>
        /// <param name="studentId">The student ID</param>
        /// <param name="examId">The exam ID</param>
        /// <param name="marks">The marks obtained</param>
        /// <returns>True if marks entry succeeded</returns>
        public bool EnterMarks(int studentId, int examId, decimal marks)
        {
            string query = @"
                INSERT INTO ExamMarks (ExamID, StudentID, Marks, CreatedAt)
                VALUES (@ExamID, @StudentID, @Marks, GETDATE())";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ExamID", examId),
                new SqlParameter("@StudentID", studentId),
                new SqlParameter("@Marks", marks)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Updates marks for a student in an exam
        /// </summary>
        /// <param name="studentId">The student ID</param>
        /// <param name="examId">The exam ID</param>
        /// <param name="marks">The updated marks</param>
        /// <returns>True if update succeeded</returns>
        public bool UpdateMarks(int studentId, int examId, decimal marks)
        {
            string query = @"
                UPDATE ExamMarks SET
                    Marks = @Marks
                WHERE ExamID = @ExamID AND StudentID = @StudentID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ExamID", examId),
                new SqlParameter("@StudentID", studentId),
                new SqlParameter("@Marks", marks)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Deletes marks for a student in an exam
        /// </summary>
        /// <param name="studentId">The student ID</param>
        /// <param name="examId">The exam ID</param>
        /// <returns>True if deletion succeeded</returns>
        public bool DeleteMarks(int studentId, int examId)
        {
            string query = @"
                DELETE FROM ExamMarks
                WHERE ExamID = @ExamID AND StudentID = @StudentID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ExamID", examId),
                new SqlParameter("@StudentID", studentId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Gets all student marks for a specific exam
        /// </summary>
        /// <param name="examId">The exam ID</param>
        /// <returns>DataTable containing student marks</returns>
        public DataTable GetStudentMarks(int examId)
        {
            string query = @"
                SELECT em.*, s.FirstName, s.LastName, s.StudentCode
                FROM ExamMarks em
                INNER JOIN Students s ON em.StudentID = s.StudentID
                WHERE em.ExamID = @ExamID
                ORDER BY s.FirstName, s.LastName";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ExamID", examId)
            };

            return db.ExecuteQuery(query, parameters);
        }

        #endregion

        #region Results Operations

        /// <summary>
        /// Generates results for all students in an exam
        /// </summary>
        /// <param name="examId">The exam ID</param>
        /// <returns>Number of result records generated</returns>
        public int GenerateResults(int examId)
        {
            // Clear existing results for this exam
            string deleteQuery = "DELETE FROM ExamResults WHERE ExamID = @ExamID";
            SqlParameter[] deleteParams = new SqlParameter[]
            {
                new SqlParameter("@ExamID", examId)
            };
            db.ExecuteNonQuery(deleteQuery, deleteParams);

            // Generate new results
            string query = @"
                INSERT INTO ExamResults (ExamID, StudentID, TotalMarks, ObtainedMarks, Average, Grade, Status, GeneratedAt)
                SELECT 
                    @ExamID,
                    em.StudentID,
                    COUNT(em.ExamMarkID) * 100 as TotalMarks,
                    SUM(em.Marks) as ObtainedMarks,
                    AVG(em.Marks) as Average,
                    'Pending' as Grade,
                    'Generated',
                    GETDATE()
                FROM ExamMarks em
                WHERE em.ExamID = @ExamID
                GROUP BY em.StudentID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ExamID", examId)
            };

            return db.ExecuteNonQuery(query, parameters);
        }

        /// <summary>
        /// Publishes results for an exam
        /// </summary>
        /// <param name="examId">The exam ID</param>
        /// <returns>True if publish succeeded</returns>
        public bool PublishResults(int examId)
        {
            string query = @"
                UPDATE ExamResults SET
                    Status = 'Published',
                    PublishedAt = GETDATE()
                WHERE ExamID = @ExamID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ExamID", examId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        #endregion

        #region Search

        /// <summary>
        /// Searches exams by keyword
        /// </summary>
        /// <param name="keyword">Search keyword for exam name</param>
        /// <returns>List of matching Exam objects</returns>
        public List<Exam> SearchExams(string keyword)
        {
            List<Exam> exams = new List<Exam>();

            string query = @"
                SELECT e.*, t.TermName, ay.YearName
                FROM Exams e
                INNER JOIN Terms t ON e.TermID = t.TermID
                INNER JOIN AcademicYears ay ON t.AcademicYearID = ay.AcademicYearID
                WHERE e.ExamName LIKE @Keyword
                ORDER BY e.StartDate DESC";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Keyword", "%" + keyword + "%")
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                exams.Add(MapToExam(row));
            }

            return exams;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Maps a DataRow to an Exam object
        /// </summary>
        /// <param name="row">The DataRow to map</param>
        /// <returns>Populated Exam object</returns>
        private Exam MapToExam(DataRow row)
        {
            Exam exam = new Exam();
            exam.ExamID = Convert.ToInt32(row["ExamID"]);
            exam.ExamName = row["ExamName"].ToString();
            exam.ExamType = row["ExamType"].ToString();
            exam.TermID = Convert.ToInt32(row["TermID"]);
            exam.StartDate = Convert.ToDateTime(row["StartDate"]);
            exam.EndDate = Convert.ToDateTime(row["EndDate"]);
            exam.Status = row["Status"].ToString();
            exam.CreatedBy = Convert.ToInt32(row["CreatedBy"]);
            exam.CreatedAt = Convert.ToDateTime(row["CreatedAt"]);

            if (row.Table.Columns.Contains("TermName"))
                exam.TermName = row["TermName"] == DBNull.Value ? null : row["TermName"].ToString();

            if (row.Table.Columns.Contains("YearName"))
                exam.YearName = row["YearName"] == DBNull.Value ? null : row["YearName"].ToString();

            if (row.Table.Columns.Contains("AcademicYearID"))
                exam.AcademicYearID = row["AcademicYearID"] == DBNull.Value ? 0 : Convert.ToInt32(row["AcademicYearID"]);

            return exam;
        }

        #endregion
    }
}