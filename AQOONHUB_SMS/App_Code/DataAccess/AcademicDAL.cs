using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace AQOONHUB.DataAccess
{
    public class AcademicDAL
    {
        private DatabaseHelper db;

        public AcademicDAL()
        {
            db = new DatabaseHelper();
        }

        #region Academic Years

        /// <summary>
        /// Gets all academic years
        /// </summary>
        public DataTable GetAcademicYears()
        {
            string query = "SELECT * FROM AcademicYears ORDER BY StartDate DESC";
            return db.ExecuteQuery(query);
        }

        /// <summary>
        /// Gets current academic year
        /// </summary>
        public DataRow GetCurrentAcademicYear()
        {
            string query = "SELECT TOP 1 * FROM AcademicYears WHERE Status = 'Active' ORDER BY StartDate DESC";
            DataTable dt = db.ExecuteQuery(query);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>
        /// Adds academic year
        /// </summary>
        public int AddAcademicYear(string yearName, DateTime startDate, DateTime endDate)
        {
            string query = @"
                INSERT INTO AcademicYears (YearName, StartDate, EndDate, Status)
                VALUES (@YearName, @StartDate, @EndDate, 'Upcoming');
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@YearName", yearName),
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        /// <summary>
        /// Sets active academic year
        /// </summary>
        public bool SetActiveAcademicYear(int academicYearId)
        {
            string query = @"
                UPDATE AcademicYears SET Status = 'Closed' WHERE Status = 'Active';
                UPDATE AcademicYears SET Status = 'Active' WHERE AcademicYearID = @AcademicYearID;";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@AcademicYearID", academicYearId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        #endregion

        #region Terms

        /// <summary>
        /// Gets terms by academic year
        /// </summary>
        public DataTable GetTerms(int? academicYearId = null)
        {
            string query = @"
                SELECT t.*, ay.YearName
                FROM Terms t
                INNER JOIN AcademicYears ay ON t.AcademicYearID = ay.AcademicYearID
                WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (academicYearId.HasValue)
            {
                query += " AND t.AcademicYearID = @AcademicYearID";
                parameters.Add(new SqlParameter("@AcademicYearID", academicYearId.Value));
            }

            query += " ORDER BY t.StartDate";

            return db.ExecuteQuery(query, parameters.ToArray());
        }

        /// <summary>
        /// Gets current term
        /// </summary>
        public DataRow GetCurrentTerm()
        {
            string query = "SELECT TOP 1 * FROM Terms WHERE IsCurrentTerm = 1";
            DataTable dt = db.ExecuteQuery(query);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>
        /// Sets current term
        /// </summary>
        public bool SetCurrentTerm(int termId)
        {
            string query = @"
                UPDATE Terms SET IsCurrentTerm = 0, Status = 'Closed' WHERE IsCurrentTerm = 1;
                UPDATE Terms SET IsCurrentTerm = 1, Status = 'Active' WHERE TermID = @TermID;";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@TermID", termId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        #endregion

        #region Classes

        /// <summary>
        /// Gets all classes
        /// </summary>
        public DataTable GetAllClasses()
        {
            string query = "SELECT * FROM Classes ORDER BY ClassName";
            return db.ExecuteQuery(query);
        }

        /// <summary>
        /// Gets class by ID
        /// </summary>
        public DataRow GetClassById(int classId)
        {
            string query = "SELECT * FROM Classes WHERE ClassID = @ClassID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClassID", classId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>
        /// Adds class
        /// </summary>
        public int AddClass(string className, int capacity, string roomNumber)
        {
            string query = @"
                INSERT INTO Classes (ClassName, Capacity, RoomNumber)
                VALUES (@ClassName, @Capacity, @RoomNumber);
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClassName", className),
                new SqlParameter("@Capacity", capacity),
                new SqlParameter("@RoomNumber", roomNumber)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        #endregion

        #region Sections

        /// <summary>
        /// Gets sections by class
        /// </summary>
        public DataTable GetSections(int? classId = null)
        {
            string query = @"
                SELECT sec.*, c.ClassName
                FROM Sections sec
                INNER JOIN Classes c ON sec.ClassID = c.ClassID
                WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (classId.HasValue)
            {
                query += " AND sec.ClassID = @ClassID";
                parameters.Add(new SqlParameter("@ClassID", classId.Value));
            }

            query += " ORDER BY c.ClassName, sec.SectionName";

            return db.ExecuteQuery(query, parameters.ToArray());
        }

        /// <summary>
        /// Adds section
        /// </summary>
        public int AddSection(int classId, string sectionName, int capacity)
        {
            string query = @"
                INSERT INTO Sections (ClassID, SectionName, Capacity)
                VALUES (@ClassID, @SectionName, @Capacity);
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClassID", classId),
                new SqlParameter("@SectionName", sectionName),
                new SqlParameter("@Capacity", capacity)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        /// <summary>
        /// Gets section enrollment count
        /// </summary>
        public int GetSectionEnrollment(int sectionId)
        {
            string query = "SELECT COUNT(*) FROM Students WHERE SectionID = @SectionID AND Status = 'Active'";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SectionID", sectionId)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        #endregion

        #region Subjects

        /// <summary>
        /// Gets all subjects
        /// </summary>
        public DataTable GetAllSubjects(bool activeOnly = true)
        {
            string query = "SELECT * FROM Subjects WHERE 1=1";

            if (activeOnly)
                query += " AND IsActive = 1";

            query += " ORDER BY SubjectName";

            return db.ExecuteQuery(query);
        }

        /// <summary>
        /// Gets subject by ID
        /// </summary>
        public DataRow GetSubjectById(int subjectId)
        {
            string query = "SELECT * FROM Subjects WHERE SubjectID = @SubjectID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SubjectID", subjectId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>
        /// Adds subject
        /// </summary>
        public int AddSubject(string subjectName, string subjectCode, string description)
        {
            string query = @"
                INSERT INTO Subjects (SubjectName, SubjectCode, Description, IsActive)
                VALUES (@SubjectName, @SubjectCode, @Description, 1);
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SubjectName", subjectName),
                new SqlParameter("@SubjectCode", subjectCode),
                new SqlParameter("@Description", (object)description ?? DBNull.Value)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        #endregion

        #region Class-Subject-Teacher Assignment

        /// <summary>
        /// Gets class subject teachers
        /// </summary>
        public DataTable GetClassSubjectTeachers(int? sectionId = null, int? academicYearId = null)
        {
            string query = @"
                SELECT cst.*, c.ClassName, sec.SectionName, sub.SubjectName, sub.SubjectCode,
                       u.FullName as TeacherName, s.EmployeeID
                FROM ClassSubjectTeachers cst
                INNER JOIN Sections sec ON cst.SectionID = sec.SectionID
                INNER JOIN Classes c ON sec.ClassID = c.ClassID
                INNER JOIN Subjects sub ON cst.SubjectID = sub.SubjectID
                INNER JOIN Staff s ON cst.StaffID = s.StaffID
                INNER JOIN Users u ON s.UserID = u.UserID
                WHERE cst.IsActive = 1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (sectionId.HasValue)
            {
                query += " AND cst.SectionID = @SectionID";
                parameters.Add(new SqlParameter("@SectionID", sectionId.Value));
            }

            if (academicYearId.HasValue)
            {
                query += " AND cst.AcademicYearID = @AcademicYearID";
                parameters.Add(new SqlParameter("@AcademicYearID", academicYearId.Value));
            }

            query += " ORDER BY c.ClassName, sec.SectionName, sub.SubjectName";

            return db.ExecuteQuery(query, parameters.ToArray());
        }

        /// <summary>
        /// Assigns teacher to class-subject
        /// </summary>
        public int AssignTeacher(int sectionId, int subjectId, int staffId, int academicYearId)
        {
            // Deactivate existing assignment
            string deactivateQuery = @"
                UPDATE ClassSubjectTeachers SET IsActive = 0
                WHERE SectionID = @SectionID AND SubjectID = @SubjectID AND AcademicYearID = @AcademicYearID";

            SqlParameter[] deactivateParams = new SqlParameter[]
            {
                new SqlParameter("@SectionID", sectionId),
                new SqlParameter("@SubjectID", subjectId),
                new SqlParameter("@AcademicYearID", academicYearId)
            };

            db.ExecuteNonQuery(deactivateQuery, deactivateParams);

            // Create new assignment
            string query = @"
                INSERT INTO ClassSubjectTeachers (SectionID, SubjectID, StaffID, AcademicYearID, IsActive)
                VALUES (@SectionID, @SubjectID, @StaffID, @AcademicYearID, 1);
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SectionID", sectionId),
                new SqlParameter("@SubjectID", subjectId),
                new SqlParameter("@StaffID", staffId),
                new SqlParameter("@AcademicYearID", academicYearId)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        #endregion

        #region Timetable

        /// <summary>
        /// Gets timetable for section
        /// </summary>
        public DataTable GetTimetable(int sectionId, int academicYearId)
        {
            string query = @"
                SELECT t.*, sub.SubjectName, sub.SubjectCode,
                       u.FullName as TeacherName, c.ClassName, sec.SectionName
                FROM Timetable t
                INNER JOIN Sections sec ON t.SectionID = sec.SectionID
                INNER JOIN Classes c ON sec.ClassID = c.ClassID
                INNER JOIN Subjects sub ON t.SubjectID = sub.SubjectID
                INNER JOIN Staff s ON t.StaffID = s.StaffID
                INNER JOIN Users u ON s.UserID = u.UserID
                WHERE t.SectionID = @SectionID
                AND t.AcademicYearID = @AcademicYearID
                AND t.IsActive = 1
                ORDER BY t.DayOfWeek, t.StartTime";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SectionID", sectionId),
                new SqlParameter("@AcademicYearID", academicYearId)
            };

            return db.ExecuteQuery(query, parameters);
        }

        /// <summary>
        /// Adds timetable slot
        /// </summary>
        public int AddTimetableSlot(int sectionId, int subjectId, int staffId, int dayOfWeek,
            int periodNo, TimeSpan startTime, TimeSpan endTime, string roomNumber, int academicYearId)
        {
            string query = @"
                INSERT INTO Timetable (SectionID, SubjectID, StaffID, DayOfWeek, PeriodNo, StartTime, EndTime, RoomNumber, AcademicYearID, IsActive)
                VALUES (@SectionID, @SubjectID, @StaffID, @DayOfWeek, @PeriodNo, @StartTime, @EndTime, @RoomNumber, @AcademicYearID, 1);
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SectionID", sectionId),
                new SqlParameter("@SubjectID", subjectId),
                new SqlParameter("@StaffID", staffId),
                new SqlParameter("@DayOfWeek", dayOfWeek),
                new SqlParameter("@PeriodNo", periodNo),
                new SqlParameter("@StartTime", startTime),
                new SqlParameter("@EndTime", endTime),
                new SqlParameter("@RoomNumber", roomNumber),
                new SqlParameter("@AcademicYearID", academicYearId)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        /// <summary>
        /// Checks for teacher conflicts
        /// </summary>
        public DataTable CheckTeacherConflicts(int staffId, int dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
        {
            string query = @"
                SELECT t.*, c.ClassName, sec.SectionName, sub.SubjectName
                FROM Timetable t
                INNER JOIN Sections sec ON t.SectionID = sec.SectionID
                INNER JOIN Classes c ON sec.ClassID = c.ClassID
                INNER JOIN Subjects sub ON t.SubjectID = sub.SubjectID
                WHERE t.StaffID = @StaffID
                AND t.DayOfWeek = @DayOfWeek
                AND t.IsActive = 1
                AND ((t.StartTime <= @StartTime AND t.EndTime > @StartTime)
                     OR (t.StartTime < @EndTime AND t.EndTime >= @EndTime)
                     OR (t.StartTime >= @StartTime AND t.EndTime <= @EndTime))";

            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter("@StaffID", staffId),
                new SqlParameter("@DayOfWeek", dayOfWeek),
                new SqlParameter("@StartTime", startTime),
                new SqlParameter("@EndTime", endTime)
            };

            if (excludeId.HasValue)
            {
                query += " AND t.TimetableID != @ExcludeID";
                parameters.Add(new SqlParameter("@ExcludeID", excludeId.Value));
            }

            return db.ExecuteQuery(query, parameters.ToArray());
        }

        #endregion

        #region Promotion

        /// <summary>
        /// Gets students eligible for promotion
        /// </summary>
        public DataTable GetPromotionCandidates(int fromClassId, int academicYearId)
        {
            string query = @"
                SELECT s.StudentID, s.StudentCode, s.FirstName + ' ' + s.LastName as StudentName,
                       s.Gender, c.ClassName, sec.SectionName,
                       AVG(er.Marks) as AverageMarks,
                       CASE WHEN AVG(er.Marks) >= 60 THEN 1 ELSE 0 END as CanPromote
                FROM Students s
                INNER JOIN Sections sec ON s.SectionID = sec.SectionID
                INNER JOIN Classes c ON sec.ClassID = c.ClassID
                LEFT JOIN ExamResults er ON s.StudentID = er.StudentID
                WHERE c.ClassID = @ClassID
                AND s.AcademicYearID = @AcademicYearID
                AND s.Status = 'Active'
                GROUP BY s.StudentID, s.StudentCode, s.FirstName, s.LastName, s.Gender, c.ClassName, sec.SectionName";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClassID", fromClassId),
                new SqlParameter("@AcademicYearID", academicYearId)
            };

            return db.ExecuteQuery(query, parameters);
        }

        /// <summary>
        /// Promotes student to next class
        /// </summary>
        public bool PromoteStudent(int studentId, int newSectionId, int newAcademicYearId)
        {
            string query = @"
                UPDATE Students SET
                    SectionID = @NewSectionID,
                    AcademicYearID = @NewAcademicYearID,
                    UpdatedAt = GETDATE()
                WHERE StudentID = @StudentID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@StudentID", studentId),
                new SqlParameter("@NewSectionID", newSectionId),
                new SqlParameter("@NewAcademicYearID", newAcademicYearId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        #endregion
    }
}