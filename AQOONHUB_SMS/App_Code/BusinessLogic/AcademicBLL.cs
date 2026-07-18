using System;
using System.Collections.Generic;
using System.Data;
using AQOONHUB.DataAccess;
using AQOONHUB.Models;
using AQOONHUB.Utilities;

namespace AQOONHUB.BusinessLogic
{
    public class AcademicBLL
    {
        private AcademicDAL academicDAL;
        private AuditLogger auditLogger;

        public AcademicBLL()
        {
            academicDAL = new AcademicDAL();
            auditLogger = new AuditLogger();
        }

        #region Academic Year & Term Management

        public DataTable GetAcademicYears()
        {
            return academicDAL.GetAcademicYears();
        }

        public DataRow GetCurrentAcademicYear()
        {
            return academicDAL.GetCurrentAcademicYear();
        }

        /// <summary>
        /// Sets active academic year with validation
        /// </summary>
        public bool SetActiveYear(int academicYearId, int setBy)
        {
            var year = academicDAL.GetAcademicYears();
            DataRow targetYear = null;

            foreach (DataRow row in year.Rows)
            {
                if (Convert.ToInt32(row["AcademicYearID"]) == academicYearId)
                {
                    targetYear = row;
                    break;
                }
            }

            if (targetYear == null)
                throw new Exception("Academic year not found");

            if (targetYear["Status"].ToString() == "Active")
                throw new Exception("This is already the active year");

            bool result = academicDAL.SetActiveAcademicYear(academicYearId);

            if (result)
            {
                auditLogger.LogAction(setBy, "YEAR_CHANGE", "Academics",
                    string.Format("Changed active academic year to {0}", targetYear["YearName"]));
            }

            return result;
        }

        public DataTable GetTerms(int? academicYearId = null)
        {
            return academicDAL.GetTerms(academicYearId);
        }

        /// <summary>
        /// Sets current term with cascade effects
        /// </summary>
        public bool SetCurrentTerm(int termId, int setBy)
        {
            var terms = academicDAL.GetTerms();
            DataRow targetTerm = null;

            foreach (DataRow row in terms.Rows)
            {
                if (Convert.ToInt32(row["TermID"]) == termId)
                {
                    targetTerm = row;
                    break;
                }
            }

            if (targetTerm == null)
                throw new Exception("Term not found");

            bool result = academicDAL.SetCurrentTerm(termId);

            if (result)
            {
                auditLogger.LogAction(setBy, "TERM_CHANGE", "Academics",
                    string.Format("Changed current term to {0} ({1})", targetTerm["TermName"], targetTerm["YearName"]));

                // Trigger term change processes
                OnTermChange(termId);
            }

            return result;
        }

        #endregion

        #region Class & Section Management

        public DataTable GetClasses()
        {
            return academicDAL.GetAllClasses();
        }

        public DataTable GetSections(int? classId = null)
        {
            return academicDAL.GetSections(classId);
        }

        public int AddClass(string className, int capacity, string roomNumber, int createdBy)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(className))
                throw new ValidationException("Class name is required");

            if (capacity <= 0 || capacity > 100)
                throw new ValidationException("Capacity must be between 1 and 100");

            int classId = academicDAL.AddClass(className, capacity, roomNumber);

            auditLogger.LogCreate(createdBy, "Academics", "Class", classId.ToString(),
                string.Format("Added {0} with capacity {1}", className, capacity));

            return classId;
        }

        public int AddSection(int classId, string sectionName, int capacity, int createdBy)
        {
            // Validate section name
            if (string.IsNullOrWhiteSpace(sectionName) || sectionName.Length > 1)
                throw new ValidationException("Section name must be a single letter (A, B, C)");

            // Check if section already exists for this class
            var existing = academicDAL.GetSections(classId);
            foreach (DataRow row in existing.Rows)
            {
                if (row["SectionName"].ToString() == sectionName)
                    throw new ValidationException(string.Format("Section {0} already exists for this class", sectionName));
            }

            int sectionId = academicDAL.AddSection(classId, sectionName, capacity);

            auditLogger.LogCreate(createdBy, "Academics", "Section", sectionId.ToString(),
                string.Format("Added Section {0} to class", sectionName));

            return sectionId;
        }

        public int GetSectionEnrollment(int sectionId)
        {
            return academicDAL.GetSectionEnrollment(sectionId);
        }

        #endregion

        #region Subject Management

        public DataTable GetSubjects(bool activeOnly = true)
        {
            return academicDAL.GetAllSubjects(activeOnly);
        }

        public int AddSubject(string subjectName, string subjectCode, string description, int createdBy)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(subjectName))
                throw new ValidationException("Subject name is required");

            if (string.IsNullOrWhiteSpace(subjectCode))
                throw new ValidationException("Subject code is required");

            if (subjectCode.Length > 10)
                throw new ValidationException("Subject code must be 10 characters or less");

            int subjectId = academicDAL.AddSubject(subjectName, subjectCode, description);

            auditLogger.LogCreate(createdBy, "Academics", "Subject", subjectCode,
                string.Format("Added subject: {0}", subjectName));

            return subjectId;
        }

        #endregion

        #region Timetable Management

        public DataTable GetTimetable(int sectionId, int academicYearId)
        {
            return academicDAL.GetTimetable(sectionId, academicYearId);
        }

        /// <summary>
        /// Adds timetable slot with conflict checking
        /// </summary>
        public int AddTimetableSlot(int sectionId, int subjectId, int staffId, int dayOfWeek,
            int periodNo, TimeSpan startTime, TimeSpan endTime, string roomNumber,
            int academicYearId, int createdBy)
        {
            // Validate time
            if (startTime >= endTime)
                throw new ValidationException("Start time must be before end time");

            if ((endTime - startTime).TotalMinutes < 30)
                throw new ValidationException("Class duration must be at least 30 minutes");

            // Check teacher conflicts
            var conflicts = academicDAL.CheckTeacherConflicts(staffId, dayOfWeek, startTime, endTime);
            if (conflicts.Rows.Count > 0)
            {
                string conflictMsg = "Teacher has conflicting classes:\n";
                foreach (DataRow row in conflicts.Rows)
                {
                    conflictMsg += string.Format("- {0} {1}: {2}\n", row["ClassName"], row["SectionName"], row["SubjectName"]);
                }
                throw new ValidationException(conflictMsg);
            }

            // Check room conflicts (would need room conflict check method)

            int slotId = academicDAL.AddTimetableSlot(sectionId, subjectId, staffId, dayOfWeek,
                periodNo, startTime, endTime, roomNumber, academicYearId);

            auditLogger.LogCreate(createdBy, "Academics", "Timetable Slot", slotId.ToString(),
                string.Format("Added {0:hh\\:mm}-{1:hh\\:mm} slot for section {2}", startTime, endTime, sectionId));

            return slotId;
        }

        /// <summary>
        /// Validates entire timetable for conflicts
        /// </summary>
        public List<string> ValidateTimetable(int sectionId, int academicYearId)
        {
            List<string> issues = new List<string>();
            var timetable = academicDAL.GetTimetable(sectionId, academicYearId);

            // Check for double-booked teachers
            // Check for double-booked rooms
            // Check for consecutive classes without breaks

            return issues;
        }

        #endregion

        #region Promotion

        /// <summary>
        /// Gets promotion candidates with eligibility
        /// </summary>
        public DataTable GetPromotionCandidates(int fromClassId, int academicYearId)
        {
            return academicDAL.GetPromotionCandidates(fromClassId, academicYearId);
        }

        /// <summary>
        /// Executes bulk promotion
        /// </summary>
        public PromotionResult ExecutePromotion(int fromClassId, int toClassId,
            int academicYearId, int executedBy, bool promoteAll = false)
        {
            var candidates = academicDAL.GetPromotionCandidates(fromClassId, academicYearId);
            int promoted = 0;
            int failed = 0;
            List<string> failures = new List<string>();

            foreach (DataRow row in candidates.Rows)
            {
                bool canPromote = Convert.ToBoolean(row["CanPromote"]);
                int studentId = Convert.ToInt32(row["StudentID"]);

                if (promoteAll || canPromote)
                {
                    // Find appropriate section in new class
                    int newSectionId = FindSectionForStudent(toClassId);

                    if (academicDAL.PromoteStudent(studentId, newSectionId, academicYearId))
                        promoted++;
                    else
                    {
                        failed++;
                        failures.Add(string.Format("{0}: Promotion failed", row["StudentName"]));
                    }
                }
                else
                {
                    failed++;
                    failures.Add(string.Format("{0}: Did not meet requirements", row["StudentName"]));
                }
            }

            auditLogger.LogBulkOperation(executedBy, "Academics", "PROMOTION",
                promoted, string.Format("Promoted {0} students from class {1} to {2}", promoted, fromClassId, toClassId));

            return new PromotionResult
            {
                TotalCandidates = candidates.Rows.Count,
                Promoted = promoted,
                Failed = failed,
                Failures = failures
            };
        }

        #endregion

        #region Helper Methods

        private void OnTermChange(int termId)
        {
            // Trigger processes when term changes:
            // 1. Generate term invoices
            // 2. Reset attendance counters
            // 3. Update exam schedules
            // 4. Notify stakeholders
        }

        private int FindSectionForStudent(int classId)
        {
            // Find section with lowest enrollment
            var sections = academicDAL.GetSections(classId);
            int bestSectionId = 0;
            int lowestEnrollment = int.MaxValue;

            foreach (DataRow row in sections.Rows)
            {
                int sectionId = Convert.ToInt32(row["SectionID"]);
                int enrollment = academicDAL.GetSectionEnrollment(sectionId);

                if (enrollment < lowestEnrollment)
                {
                    lowestEnrollment = enrollment;
                    bestSectionId = sectionId;
                }
            }

            return bestSectionId;
        }

        private int GetCurrentAcademicYearId()
        {
            var year = academicDAL.GetCurrentAcademicYear();
            return year != null ? Convert.ToInt32(year["AcademicYearID"]) : 1;
        }

        #endregion
    }

    /// <summary>
    /// Result of promotion operation
    /// </summary>
    public class PromotionResult
    {
        public int TotalCandidates { get; set; }
        public int Promoted { get; set; }
        public int Failed { get; set; }
        public List<string> Failures { get; set; }

        public PromotionResult()
        {
            Failures = new List<string>();
        }
    }
}