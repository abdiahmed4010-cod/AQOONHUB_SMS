using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace AQOONHUB.DataAccess
{
    public class AttendanceDAL
    {
        private DatabaseHelper db;

        public AttendanceDAL()
        {
            db = new DatabaseHelper();
        }

        #region Daily Attendance

        /// <summary>
        /// Gets attendance for a class on a specific date
        /// </summary>
        public DataTable GetAttendanceByClass(int sectionId, DateTime attendanceDate, string period = null)
        {
            string query = @"
                SELECT 
                    s.StudentID,
                    s.StudentCode,
                    s.FirstName + ' ' + s.LastName as StudentName,
                    s.Gender,
                    ISNULL(a.Status, 'Not Marked') as Status,
                    ISNULL(a.Remarks, '') as Remarks,
                    ISNULL(a.AttendanceID, 0) as AttendanceID
                FROM Students s
                LEFT JOIN Attendance a ON s.StudentID = a.StudentID 
                    AND a.AttendanceDate = @AttendanceDate
                    AND (@Period IS NULL OR a.Period = @Period)
                WHERE s.SectionID = @SectionID
                AND s.Status = 'Active'
                ORDER BY s.FirstName, s.LastName";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SectionID", sectionId),
                new SqlParameter("@AttendanceDate", attendanceDate),
                new SqlParameter("@Period", (object)period ?? DBNull.Value)
            };

            return db.ExecuteQuery(query, parameters);
        }

        /// <summary>
        /// Marks attendance for a student
        /// </summary>
        public bool MarkAttendance(int studentId, int sectionId, int? subjectId, DateTime attendanceDate,
            string period, string status, int markedBy, string remarks = null)
        {
            // Check if already marked
            string checkQuery = @"
                SELECT AttendanceID FROM Attendance 
                WHERE StudentID = @StudentID AND AttendanceDate = @AttendanceDate 
                AND (@Period IS NULL OR Period = @Period)";

            SqlParameter[] checkParams = new SqlParameter[]
            {
                new SqlParameter("@StudentID", studentId),
                new SqlParameter("@AttendanceDate", attendanceDate),
                new SqlParameter("@Period", (object)period ?? DBNull.Value)
            };

            DataTable existing = db.ExecuteQuery(checkQuery, checkParams);

            if (existing.Rows.Count > 0)
            {
                // Update existing
                int attendanceId = Convert.ToInt32(existing.Rows[0]["AttendanceID"]);
                string updateQuery = @"
                    UPDATE Attendance SET
                        Status = @Status,
                        SubjectID = @SubjectID,
                        Remarks = @Remarks,
                        MarkedBy = @MarkedBy
                    WHERE AttendanceID = @AttendanceID";

                SqlParameter[] updateParams = new SqlParameter[]
                {
                    new SqlParameter("@AttendanceID", attendanceId),
                    new SqlParameter("@Status", status),
                    new SqlParameter("@SubjectID", (object)subjectId ?? DBNull.Value),
                    new SqlParameter("@Remarks", (object)remarks ?? DBNull.Value),
                    new SqlParameter("@MarkedBy", markedBy)
                };

                return db.ExecuteNonQuery(updateQuery, updateParams) > 0;
            }
            else
            {
                // Insert new
                string insertQuery = @"
                    INSERT INTO Attendance (StudentID, SectionID, SubjectID, AttendanceDate, Period, Status, MarkedBy, Remarks)
                    VALUES (@StudentID, @SectionID, @SubjectID, @AttendanceDate, @Period, @Status, @MarkedBy, @Remarks)";

                SqlParameter[] insertParams = new SqlParameter[]
                {
                    new SqlParameter("@StudentID", studentId),
                    new SqlParameter("@SectionID", sectionId),
                    new SqlParameter("@SubjectID", (object)subjectId ?? DBNull.Value),
                    new SqlParameter("@AttendanceDate", attendanceDate),
                    new SqlParameter("@Period", (object)period ?? DBNull.Value),
                    new SqlParameter("@Status", status),
                    new SqlParameter("@MarkedBy", markedBy),
                    new SqlParameter("@Remarks", (object)remarks ?? DBNull.Value)
                };

                return db.ExecuteNonQuery(insertQuery, insertParams) > 0;
            }
        }

        /// <summary>
        /// Bulk mark attendance (all present)
        /// </summary>
        public int BulkMarkAttendance(int sectionId, DateTime attendanceDate, string period, int markedBy)
        {
            string query = @"
                INSERT INTO Attendance (StudentID, SectionID, AttendanceDate, Period, Status, MarkedBy)
                SELECT s.StudentID, @SectionID, @AttendanceDate, @Period, 'Present', @MarkedBy
                FROM Students s
                WHERE s.SectionID = @SectionID
                AND s.Status = 'Active'
                AND NOT EXISTS (
                    SELECT 1 FROM Attendance a 
                    WHERE a.StudentID = s.StudentID 
                    AND a.AttendanceDate = @AttendanceDate
                    AND (@Period IS NULL OR a.Period = @Period)
                )";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SectionID", sectionId),
                new SqlParameter("@AttendanceDate", attendanceDate),
                new SqlParameter("@Period", (object)period ?? DBNull.Value),
                new SqlParameter("@MarkedBy", markedBy)
            };

            return db.ExecuteNonQuery(query, parameters);
        }

        #endregion

        #region Attendance Reports

        /// <summary>
        /// Gets attendance summary by class
        /// </summary>
        public DataTable GetAttendanceSummaryByClass(DateTime? fromDate = null, DateTime? toDate = null)
        {
            string query = @"
                SELECT 
                    c.ClassName,
                    sec.SectionName,
                    COUNT(DISTINCT s.StudentID) as TotalStudents,
                    COUNT(a.AttendanceID) as TotalRecords,
                    SUM(CASE WHEN a.Status = 'Present' THEN 1 ELSE 0 END) as PresentCount,
                    SUM(CASE WHEN a.Status = 'Absent' THEN 1 ELSE 0 END) as AbsentCount,
                    SUM(CASE WHEN a.Status = 'Late' THEN 1 ELSE 0 END) as LateCount,
                    SUM(CASE WHEN a.Status = 'Excused' THEN 1 ELSE 0 END) as ExcusedCount,
                    CASE WHEN COUNT(a.AttendanceID) > 0 
                        THEN CAST(SUM(CASE WHEN a.Status = 'Present' THEN 1 ELSE 0 END) * 100.0 / COUNT(a.AttendanceID) AS DECIMAL(5,2))
                        ELSE 0 
                    END as AttendanceRate
                FROM Classes c
                INNER JOIN Sections sec ON c.ClassID = sec.ClassID
                INNER JOIN Students s ON sec.SectionID = s.SectionID AND s.Status = 'Active'
                LEFT JOIN Attendance a ON s.StudentID = a.StudentID
                WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (fromDate.HasValue)
            {
                query += " AND a.AttendanceDate >= @FromDate";
                parameters.Add(new SqlParameter("@FromDate", fromDate.Value));
            }

            if (toDate.HasValue)
            {
                query += " AND a.AttendanceDate <= @ToDate";
                parameters.Add(new SqlParameter("@ToDate", toDate.Value));
            }

            query += @"
                GROUP BY c.ClassName, sec.SectionName
                ORDER BY c.ClassName, sec.SectionName";

            return db.ExecuteQuery(query, parameters.ToArray());
        }

        /// <summary>
        /// Gets student attendance history
        /// </summary>
        public DataTable GetStudentAttendanceHistory(int studentId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            string query = @"
                SELECT 
                    a.AttendanceDate,
                    a.Period,
                    a.Status,
                    a.Remarks,
                    sub.SubjectName,
                    u.FullName as MarkedByName
                FROM Attendance a
                LEFT JOIN Subjects sub ON a.SubjectID = sub.SubjectID
                INNER JOIN Staff s ON a.MarkedBy = s.StaffID
                INNER JOIN Users u ON s.UserID = u.UserID
                WHERE a.StudentID = @StudentID";

            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter("@StudentID", studentId)
            };

            if (fromDate.HasValue)
            {
                query += " AND a.AttendanceDate >= @FromDate";
                parameters.Add(new SqlParameter("@FromDate", fromDate.Value));
            }

            if (toDate.HasValue)
            {
                query += " AND a.AttendanceDate <= @ToDate";
                parameters.Add(new SqlParameter("@ToDate", toDate.Value));
            }

            query += " ORDER BY a.AttendanceDate DESC, a.Period";

            return db.ExecuteQuery(query, parameters.ToArray());
        }

        /// <summary>
        /// Gets monthly attendance statistics
        /// </summary>
        public DataTable GetMonthlyAttendance(int sectionId, int year, int month)
        {
            string query = @"
                SELECT 
                    DAY(a.AttendanceDate) as Day,
                    COUNT(*) as Total,
                    SUM(CASE WHEN a.Status = 'Present' THEN 1 ELSE 0 END) as Present,
                    SUM(CASE WHEN a.Status = 'Absent' THEN 1 ELSE 0 END) as Absent,
                    SUM(CASE WHEN a.Status = 'Late' THEN 1 ELSE 0 END) as Late,
                    SUM(CASE WHEN a.Status = 'Excused' THEN 1 ELSE 0 END) as Excused
                FROM Attendance a
                INNER JOIN Students s ON a.StudentID = s.StudentID
                WHERE s.SectionID = @SectionID
                AND YEAR(a.AttendanceDate) = @Year
                AND MONTH(a.AttendanceDate) = @Month
                GROUP BY DAY(a.AttendanceDate)
                ORDER BY Day";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SectionID", sectionId),
                new SqlParameter("@Year", year),
                new SqlParameter("@Month", month)
            };

            return db.ExecuteQuery(query, parameters);
        }

        #endregion

        #region Student Attendance Stats

        /// <summary>
        /// Gets attendance percentage for a student
        /// </summary>
        public decimal GetStudentAttendancePercentage(int studentId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            string query = @"
                SELECT 
                    CASE WHEN COUNT(*) > 0 
                        THEN CAST(SUM(CASE WHEN Status = 'Present' THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(5,2))
                        ELSE 0 
                    END as Percentage
                FROM Attendance
                WHERE StudentID = @StudentID
                AND Status != 'Holiday'";

            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter("@StudentID", studentId)
            };

            if (fromDate.HasValue)
            {
                query += " AND AttendanceDate >= @FromDate";
                parameters.Add(new SqlParameter("@FromDate", fromDate.Value));
            }

            if (toDate.HasValue)
            {
                query += " AND AttendanceDate <= @ToDate";
                parameters.Add(new SqlParameter("@ToDate", toDate.Value));
            }

            DataTable dt = db.ExecuteQuery(query, parameters.ToArray());

            if (dt.Rows.Count > 0 && dt.Rows[0]["Percentage"] != DBNull.Value)
                return Convert.ToDecimal(dt.Rows[0]["Percentage"]);

            return 0;
        }

        /// <summary>
        /// Gets students with low attendance (chronic absenteeism)
        /// </summary>
        public DataTable GetLowAttendanceStudents(int sectionId, decimal threshold = 75)
        {
            string query = @"
                SELECT 
                    s.StudentID,
                    s.StudentCode,
                    s.FirstName + ' ' + s.LastName as StudentName,
                    COUNT(a.AttendanceID) as TotalDays,
                    SUM(CASE WHEN a.Status = 'Present' THEN 1 ELSE 0 END) as PresentDays,
                    CAST(SUM(CASE WHEN a.Status = 'Present' THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(5,2)) as AttendancePercentage
                FROM Students s
                LEFT JOIN Attendance a ON s.StudentID = a.StudentID
                WHERE s.SectionID = @SectionID
                AND s.Status = 'Active'
                GROUP BY s.StudentID, s.StudentCode, s.FirstName, s.LastName
                HAVING COUNT(a.AttendanceID) > 0 
                AND CAST(SUM(CASE WHEN a.Status = 'Present' THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(5,2)) < @Threshold
                ORDER BY AttendancePercentage";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SectionID", sectionId),
                new SqlParameter("@Threshold", threshold)
            };

            return db.ExecuteQuery(query, parameters);
        }

        #endregion

        #region Dashboard Stats

        /// <summary>
        /// Gets today's attendance summary for dashboard
        /// </summary>
        public DataTable GetTodayAttendanceSummary()
        {
            string query = @"
                SELECT 
                    SUM(CASE WHEN Status = 'Present' THEN 1 ELSE 0 END) as PresentToday,
                    SUM(CASE WHEN Status = 'Absent' THEN 1 ELSE 0 END) as AbsentToday,
                    SUM(CASE WHEN Status = 'Late' THEN 1 ELSE 0 END) as LateToday,
                    SUM(CASE WHEN Status = 'Excused' THEN 1 ELSE 0 END) as ExcusedToday,
                    COUNT(DISTINCT StudentID) as TotalMarked
                FROM Attendance
                WHERE CAST(AttendanceDate as DATE) = CAST(GETDATE() as DATE)";

            return db.ExecuteQuery(query);
        }

        /// <summary>
        /// Gets attendance trend (last 6 months)
        /// </summary>
        public DataTable GetAttendanceTrend(int? sectionId = null)
        {
            string query = @"
                SELECT 
                    DATENAME(MONTH, AttendanceDate) + ' ' + CAST(YEAR(AttendanceDate) as VARCHAR) as MonthLabel,
                    MONTH(AttendanceDate) as MonthNum,
                    YEAR(AttendanceDate) as YearNum,
                    COUNT(*) as TotalRecords,
                    SUM(CASE WHEN Status = 'Present' THEN 1 ELSE 0 END) as PresentCount,
                    CAST(SUM(CASE WHEN Status = 'Present' THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(5,2)) as Rate
                FROM Attendance
                WHERE AttendanceDate >= DATEADD(month, -6, GETDATE())";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (sectionId.HasValue)
            {
                query += " AND EXISTS (SELECT 1 FROM Students s WHERE s.StudentID = Attendance.StudentID AND s.SectionID = @SectionID)";
                parameters.Add(new SqlParameter("@SectionID", sectionId.Value));
            }

            query += @"
                GROUP BY YEAR(AttendanceDate), MONTH(AttendanceDate), DATENAME(MONTH, AttendanceDate)
                ORDER BY YearNum, MonthNum";

            return db.ExecuteQuery(query, parameters.ToArray());
        }

        #endregion
    }
}