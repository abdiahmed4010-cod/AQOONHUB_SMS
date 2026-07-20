using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;


namespace AQOONHUB_SMS.App_Code.DataAccess
{
    public class TimetableDAL
    {
        private DatabaseHelper db;

        public TimetableDAL()
        {
            db = new DatabaseHelper();
        }

        /// <summary>
        /// Gets all timetable entries with optional filters
        /// </summary>
        public List<Timetable> GetAllTimetables(int sectionId, int teacherId, string dayOfWeek, int academicYearId, int termId)
        {
            List<Timetable> timetables = new List<Timetable>();

            string query = @"
                SELECT t.*, sec.SectionName, c.ClassName, sub.SubjectName, tea.FullName as TeacherName,
                       ay.YearName as AcademicYearName, tr.TermName
                FROM Timetables t
                INNER JOIN Sections sec ON t.SectionID = sec.SectionID
                INNER JOIN Classes c ON sec.ClassID = c.ClassID
                INNER JOIN Subjects sub ON t.SubjectID = sub.SubjectID
                INNER JOIN Teachers tea ON t.TeacherID = tea.TeacherID
                INNER JOIN AcademicYears ay ON t.AcademicYearID = ay.AcademicYearID
                INNER JOIN Terms tr ON t.TermID = tr.TermID
                WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (sectionId > 0)
            {
                query += " AND t.SectionID = @SectionID";
                parameters.Add(new SqlParameter("@SectionID", sectionId));
            }

            if (teacherId > 0)
            {
                query += " AND t.TeacherID = @TeacherID";
                parameters.Add(new SqlParameter("@TeacherID", teacherId));
            }

            if (!string.IsNullOrEmpty(dayOfWeek))
            {
                query += " AND t.DayOfWeek = @DayOfWeek";
                parameters.Add(new SqlParameter("@DayOfWeek", dayOfWeek));
            }

            if (academicYearId > 0)
            {
                query += " AND t.AcademicYearID = @AcademicYearID";
                parameters.Add(new SqlParameter("@AcademicYearID", academicYearId));
            }

            if (termId > 0)
            {
                query += " AND t.TermID = @TermID";
                parameters.Add(new SqlParameter("@TermID", termId));
            }

            query += " ORDER BY t.DayOfWeek, t.StartTime";

            DataTable dt = db.ExecuteQuery(query, parameters.ToArray());

            foreach (DataRow row in dt.Rows)
            {
                timetables.Add(MapToTimetable(row));
            }

            return timetables;
        }

        /// <summary>
        /// Gets all timetable entries
        /// </summary>
        public List<Timetable> GetAllTimetables()
        {
            return GetAllTimetables(0, 0, null, 0, 0);
        }

        /// <summary>
        /// Gets timetable entry by ID
        /// </summary>
        public Timetable GetTimetableById(int timetableId)
        {
            string query = @"
                SELECT t.*, sec.SectionName, c.ClassName, sub.SubjectName, tea.FullName as TeacherName,
                       ay.YearName as AcademicYearName, tr.TermName
                FROM Timetables t
                INNER JOIN Sections sec ON t.SectionID = sec.SectionID
                INNER JOIN Classes c ON sec.ClassID = c.ClassID
                INNER JOIN Subjects sub ON t.SubjectID = sub.SubjectID
                INNER JOIN Teachers tea ON t.TeacherID = tea.TeacherID
                INNER JOIN AcademicYears ay ON t.AcademicYearID = ay.AcademicYearID
                INNER JOIN Terms tr ON t.TermID = tr.TermID
                WHERE t.TimetableID = @TimetableID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@TimetableID", timetableId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
                return MapToTimetable(dt.Rows[0]);

            return null;
        }

        /// <summary>
        /// Adds a new timetable entry
        /// </summary>
        public int AddTimetable(Timetable timetable)
        {
            string query = @"
                INSERT INTO Timetables (SectionID, SubjectID, TeacherID, DayOfWeek, StartTime, EndTime,
                    RoomNumber, AcademicYearID, TermID, Status, CreatedAt, UpdatedAt)
                VALUES (@SectionID, @SubjectID, @TeacherID, @DayOfWeek, @StartTime, @EndTime,
                    @RoomNumber, @AcademicYearID, @TermID, @Status, GETDATE(), GETDATE());
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SectionID", timetable.SectionID),
                new SqlParameter("@SubjectID", timetable.SubjectID),
                new SqlParameter("@TeacherID", timetable.TeacherID),
                new SqlParameter("@DayOfWeek", timetable.DayOfWeek),
                new SqlParameter("@StartTime", timetable.StartTime),
                new SqlParameter("@EndTime", timetable.EndTime),
                new SqlParameter("@RoomNumber", string.IsNullOrEmpty(timetable.RoomNumber) ? (object)DBNull.Value : timetable.RoomNumber),
                new SqlParameter("@AcademicYearID", timetable.AcademicYearID),
                new SqlParameter("@TermID", timetable.TermID),
                new SqlParameter("@Status", timetable.Status)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        /// <summary>
        /// Updates an existing timetable entry
        /// </summary>
        public bool UpdateTimetable(Timetable timetable)
        {
            string query = @"
                UPDATE Timetables SET
                    SectionID = @SectionID,
                    SubjectID = @SubjectID,
                    TeacherID = @TeacherID,
                    DayOfWeek = @DayOfWeek,
                    StartTime = @StartTime,
                    EndTime = @EndTime,
                    RoomNumber = @RoomNumber,
                    AcademicYearID = @AcademicYearID,
                    TermID = @TermID,
                    Status = @Status,
                    UpdatedAt = GETDATE()
                WHERE TimetableID = @TimetableID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@TimetableID", timetable.TimetableID),
                new SqlParameter("@SectionID", timetable.SectionID),
                new SqlParameter("@SubjectID", timetable.SubjectID),
                new SqlParameter("@TeacherID", timetable.TeacherID),
                new SqlParameter("@DayOfWeek", timetable.DayOfWeek),
                new SqlParameter("@StartTime", timetable.StartTime),
                new SqlParameter("@EndTime", timetable.EndTime),
                new SqlParameter("@RoomNumber", string.IsNullOrEmpty(timetable.RoomNumber) ? (object)DBNull.Value : timetable.RoomNumber),
                new SqlParameter("@AcademicYearID", timetable.AcademicYearID),
                new SqlParameter("@TermID", timetable.TermID),
                new SqlParameter("@Status", timetable.Status)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Soft deletes a timetable entry
        /// </summary>
        public bool SoftDeleteTimetable(int timetableId)
        {
            string query = "UPDATE Timetables SET Status = 'Cancelled', UpdatedAt = GETDATE() WHERE TimetableID = @TimetableID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@TimetableID", timetableId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Checks for scheduling conflicts
        /// </summary>
        public bool HasConflict(int sectionId, int teacherId, string dayOfWeek, TimeSpan startTime, TimeSpan endTime, int excludeTimetableId)
        {
            string query = @"
                SELECT COUNT(*) FROM Timetables
                WHERE DayOfWeek = @DayOfWeek
                AND Status = 'Active'
                AND TimetableID != @ExcludeTimetableID
                AND (
                    (SectionID = @SectionID AND 
                     ((StartTime <= @StartTime AND EndTime > @StartTime) OR
                      (StartTime < @EndTime AND EndTime >= @EndTime) OR
                      (StartTime >= @StartTime AND EndTime <= @EndTime)))
                    OR
                    (TeacherID = @TeacherID AND
                     ((StartTime <= @StartTime AND EndTime > @StartTime) OR
                      (StartTime < @EndTime AND EndTime >= @EndTime) OR
                      (StartTime >= @StartTime AND EndTime <= @EndTime)))
                )";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SectionID", sectionId),
                new SqlParameter("@TeacherID", teacherId),
                new SqlParameter("@DayOfWeek", dayOfWeek),
                new SqlParameter("@StartTime", startTime),
                new SqlParameter("@EndTime", endTime),
                new SqlParameter("@ExcludeTimetableID", excludeTimetableId)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters)) > 0;
        }

        private Timetable MapToTimetable(DataRow row)
        {
            Timetable timetable = new Timetable();
            timetable.TimetableID = Convert.ToInt32(row["TimetableID"]);
            timetable.SectionID = Convert.ToInt32(row["SectionID"]);
            timetable.SectionName = row["SectionName"].ToString();
            timetable.SubjectID = Convert.ToInt32(row["SubjectID"]);
            timetable.SubjectName = row["SubjectName"].ToString();
            timetable.TeacherID = Convert.ToInt32(row["TeacherID"]);
            timetable.TeacherName = row["TeacherName"].ToString();
            timetable.DayOfWeek = row["DayOfWeek"].ToString();
            timetable.StartTime = (TimeSpan)row["StartTime"];
            timetable.EndTime = (TimeSpan)row["EndTime"];
            timetable.RoomNumber = row["RoomNumber"] == DBNull.Value ? null : row["RoomNumber"].ToString();
            timetable.AcademicYearID = Convert.ToInt32(row["AcademicYearID"]);
            timetable.AcademicYearName = row["AcademicYearName"].ToString();
            timetable.TermID = Convert.ToInt32(row["TermID"]);
            timetable.TermName = row["TermName"].ToString();
            timetable.Status = row["Status"].ToString();
            timetable.CreatedAt = Convert.ToDateTime(row["CreatedAt"]);
            timetable.UpdatedAt = Convert.ToDateTime(row["UpdatedAt"]);
            return timetable;
        }
    }
}