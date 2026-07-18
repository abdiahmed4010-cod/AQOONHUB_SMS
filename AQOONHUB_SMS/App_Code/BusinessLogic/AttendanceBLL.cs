using System;
using System.Collections.Generic;
using System.Data;
using AQOONHUB.DataAccess;
using AQOONHUB.Models;
using AQOONHUB.Utilities;

namespace AQOONHUB.BusinessLogic
{
    public class AttendanceBLL
    {
        private AttendanceDAL attendanceDAL;
        private StudentDAL studentDAL;
        private AuditLogger auditLogger;

        public AttendanceBLL()
        {
            attendanceDAL = new AttendanceDAL();
            studentDAL = new StudentDAL();
            auditLogger = new AuditLogger();
        }

        #region Daily Attendance

        /// <summary>
        /// Gets attendance register for a class
        /// </summary>
        public DataTable GetAttendanceRegister(int sectionId, DateTime date, string period = null)
        {
            // Validate it's a school day
            if (!IsSchoolDay(date))
                throw new ValidationException("Selected date is not a school day");

            return attendanceDAL.GetAttendanceByClass(sectionId, date, period);
        }

        /// <summary>
        /// Marks attendance for a single student
        /// </summary>
        public bool MarkAttendance(int studentId, int sectionId, int? subjectId,
            DateTime date, string period, string status, int markedBy, string remarks = null)
        {
            // Validate status
            string[] validStatuses = { "Present", "Absent", "Late", "Excused" };
            if (!Array.Exists(validStatuses, s => s == status))
                throw new ValidationException("Invalid attendance status");

            // Validate time - can't mark future attendance
            if (date > DateTime.Now.Date)
                throw new ValidationException("Cannot mark attendance for future dates");

            // Check if student is active
            var student = studentDAL.GetStudentById(studentId);
            if (student == null || student.Status != "Active")
                throw new Exception("Student not found or not active");

            bool result = attendanceDAL.MarkAttendance(studentId, sectionId, subjectId,
                date, period, status, markedBy, remarks);

            if (result && status == "Absent")
            {
                // Trigger absent notification to parent
                NotifyParentOfAbsence(studentId, date);
            }

            return result;
        }

        /// <summary>
        /// Bulk mark all students as present
        /// </summary>
        public int MarkAllPresent(int sectionId, DateTime date, string period, int markedBy)
        {
            if (!IsSchoolDay(date))
                throw new ValidationException("Selected date is not a school day");

            int count = attendanceDAL.BulkMarkAttendance(sectionId, date, period, markedBy);

            auditLogger.LogAction(markedBy, "BULK_ATTENDANCE", "Attendance",
                string.Format("Marked all {0} students present for section {1} on {2:yyyy-MM-dd}", count, sectionId, date));

            return count;
        }

        /// <summary>
        /// Saves complete attendance register
        /// </summary>
        public bool SaveAttendanceRegister(Dictionary<int, string> attendanceData,
            int sectionId, DateTime date, string period, int markedBy)
        {
            int savedCount = 0;
            foreach (var entry in attendanceData)
            {
                if (MarkAttendance(entry.Key, sectionId, null, date, period, entry.Value, markedBy))
                    savedCount++;
            }

            auditLogger.LogAction(markedBy, "ATTENDANCE_REGISTER", "Attendance",
                string.Format("Saved attendance register for section {0} on {1:yyyy-MM-dd}. {2} students marked.", sectionId, date, savedCount));

            return savedCount > 0;
        }

        #endregion

        #region Reports & Analytics

        /// <summary>
        /// Gets attendance summary for dashboard
        /// </summary>
        public DataTable GetTodaySummary()
        {
            return attendanceDAL.GetTodayAttendanceSummary();
        }

        /// <summary>
        /// Gets monthly attendance for a class
        /// </summary>
        public DataTable GetMonthlyAttendance(int sectionId, int year, int month)
        {
            return attendanceDAL.GetMonthlyAttendance(sectionId, year, month);
        }

        /// <summary>
        /// Gets students with chronic absenteeism
        /// </summary>
        public DataTable GetChronicAbsentees(int sectionId, decimal threshold = 75)
        {
            return attendanceDAL.GetLowAttendanceStudents(sectionId, threshold);
        }

        /// <summary>
        /// Gets attendance trend for charts
        /// </summary>
        public DataTable GetAttendanceTrend(int? sectionId = null)
        {
            return attendanceDAL.GetAttendanceTrend(sectionId);
        }

        /// <summary>
        /// Gets student attendance history
        /// </summary>
        public DataTable GetStudentHistory(int studentId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return attendanceDAL.GetStudentAttendanceHistory(studentId, fromDate, toDate);
        }

        /// <summary>
        /// Calculates attendance percentage for a student
        /// </summary>
        public decimal GetStudentPercentage(int studentId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return attendanceDAL.GetStudentAttendancePercentage(studentId, fromDate, toDate);
        }

        #endregion

        #region Validation & Rules

        /// <summary>
        /// Checks if date is a school day
        /// </summary>
        public bool IsSchoolDay(DateTime date)
        {
            // Weekend check (Saturday = 0 in our system, but adjust as needed)
            // Assuming Saturday-Thursday are school days, Friday is weekend
            if (date.DayOfWeek == DayOfWeek.Friday)
                return false;

            // Check holidays
            // TODO: Check against holidays table

            return true;
        }

        /// <summary>
        /// Validates if attendance can be modified
        /// </summary>
        public bool CanModifyAttendance(DateTime date)
        {
            // Can't modify attendance older than 7 days without admin approval
            if ((DateTime.Now.Date - date).Days > 7)
                return false;

            return true;
        }

        #endregion

        #region Notifications

        private void NotifyParentOfAbsence(int studentId, DateTime date)
        {
            var student = studentDAL.GetStudentById(studentId);
            if (student == null) return;

            // Send SMS/Email to parent
            // Implementation depends on notification service
            string message = string.Format("Your child {0} was marked absent on {1:yyyy-MM-dd}. Please contact the school if this is incorrect.", student.FullName, date);

            // NotificationService.SendSMS(student.GuardianPhone, message);
        }

        #endregion
    }
}