using System;
using System.Collections.Generic;
using System.Data;
using AQOONHUB_SMS.App_Code.DataAccess;
using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;

namespace AQOONHUB_SMS.App_Code.BusinessLogic
{
    /// <summary>
    /// Business logic layer for timetable management
    /// </summary>
    public class TimetableBLL
    {
        private TimetableDAL timetableDAL;
        private SectionDAL sectionDAL;
        private SubjectDAL subjectDAL;
        private AuditLogger auditLogger;

        /// <summary>
        /// Initializes a new instance of the TimetableBLL class
        /// </summary>
        public TimetableBLL()
        {
            timetableDAL = new TimetableDAL();
            sectionDAL = new SectionDAL();
            subjectDAL = new SubjectDAL();
            auditLogger = new AuditLogger();
        }

        #region Retrieval Operations

        /// <summary>
        /// Gets timetable entries for a section and academic year
        /// </summary>
        public List<Timetable> GetTimetable(int sectionID, int academicYearID)
        {
            try
            {
                if (sectionID <= 0)
                    throw new ValidationException("Invalid section ID");

                if (academicYearID <= 0)
                    throw new ValidationException("Invalid academic year ID");

                return timetableDAL.GetTimetable(sectionID, academicYearID);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Timetable",
                    string.Format("Failed to retrieve timetable for section ID {1}, academic year ID {2}: {0}", ex.Message, sectionID, academicYearID));
                throw;
            }
        }

        /// <summary>
        /// Gets a timetable slot by ID
        /// </summary>
        public Timetable GetTimetableSlotById(int timetableID)
        {
            try
            {
                if (timetableID <= 0)
                    throw new ValidationException("Invalid timetable ID");

                return timetableDAL.GetTimetableById(timetableID);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Timetable",
                    string.Format("Failed to retrieve timetable slot ID {1}: {0}", ex.Message, timetableID));
                throw;
            }
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// Adds a new timetable slot with conflict checking
        /// </summary>
        public int AddTimetableSlot(Timetable slot, int createdBy)
        {
            // Validate required fields
            if (slot == null)
                throw new ValidationException("Timetable slot data is required");

            if (slot.SectionID <= 0)
                throw new ValidationException("Section is required");

            if (slot.SubjectID <= 0)
                throw new ValidationException("Subject is required");

            if (slot.StaffID <= 0)
                throw new ValidationException("Staff member is required");

            if (slot.DayOfWeek < 1 || slot.DayOfWeek > 7)
                throw new ValidationException("Day of week must be between 1 (Monday) and 7 (Sunday)");

            if (slot.PeriodNo <= 0)
                throw new ValidationException("Period number is required");

            if (slot.StartTime == TimeSpan.Zero)
                throw new ValidationException("Start time is required");

            if (slot.EndTime == TimeSpan.Zero)
                throw new ValidationException("End time is required");

            if (slot.StartTime >= slot.EndTime)
                throw new ValidationException("Start time must be before end time");

            if ((slot.EndTime - slot.StartTime).TotalMinutes < 30)
                throw new ValidationException("Class duration must be at least 30 minutes");

            if (string.IsNullOrWhiteSpace(slot.RoomNumber))
                throw new ValidationException("Room number is required");

            if (slot.AcademicYearID <= 0)
                throw new ValidationException("Academic year is required");

            // Verify the section exists
            Section section = sectionDAL.GetSectionById(slot.SectionID);
            if (section == null)
                throw new ValidationException("Specified section does not exist");

            // Verify the subject exists
            Subject subject = subjectDAL.GetSubjectById(slot.SubjectID);
            if (subject == null)
                throw new ValidationException("Specified subject does not exist");

            // Check teacher conflicts
            DataTable teacherConflicts = timetableDAL.CheckTeacherConflicts(slot.StaffID, slot.DayOfWeek, slot.StartTime, slot.EndTime);
            if (teacherConflicts.Rows.Count > 0)
            {
                string conflictMsg = "Teacher has conflicting classes:\n";
                foreach (DataRow row in teacherConflicts.Rows)
                {
                    conflictMsg += string.Format("- {0} {1}: {2}\n", row["ClassName"], row["SectionName"], row["SubjectName"]);
                }
                throw new ValidationException(conflictMsg);
            }

            // Check room conflicts
            DataTable roomConflicts = timetableDAL.CheckRoomConflicts(slot.RoomNumber, slot.DayOfWeek, slot.StartTime, slot.EndTime);
            if (roomConflicts.Rows.Count > 0)
            {
                string conflictMsg = "Room has conflicting bookings:\n";
                foreach (DataRow row in roomConflicts.Rows)
                {
                    conflictMsg += string.Format("- {0} {1}: {2} ({3:hh\\:mm}-{4:hh\\:mm})\n",
                        row["ClassName"], row["SectionName"], row["SubjectName"],
                        Convert.ToDateTime(row["StartTime"]).TimeOfDay,
                        Convert.ToDateTime(row["EndTime"]).TimeOfDay);
                }
                throw new ValidationException(conflictMsg);
            }

            // Check for duplicate period in the same section on the same day
            List<Timetable> existingTimetable = timetableDAL.GetTimetable(slot.SectionID, slot.AcademicYearID);
            for (int i = 0; i < existingTimetable.Count; i++)
            {
                if (existingTimetable[i].DayOfWeek == slot.DayOfWeek &&
                    existingTimetable[i].PeriodNo == slot.PeriodNo)
                {
                    throw new ValidationException(string.Format("Period {0} on day {1} is already assigned in this section",
                        slot.PeriodNo, slot.DayOfWeek));
                }

                // Check for time overlap in the same section on the same day
                if (existingTimetable[i].DayOfWeek == slot.DayOfWeek &&
                    slot.StartTime < existingTimetable[i].EndTime &&
                    slot.EndTime > existingTimetable[i].StartTime)
                {
                    throw new ValidationException(string.Format("Time slot overlaps with existing {0} class ({1:hh\\:mm}-{2:hh\\:mm})",
                        existingTimetable[i].SubjectName, existingTimetable[i].StartTime, existingTimetable[i].EndTime));
                }
            }

            try
            {
                int timetableID = timetableDAL.AddTimetableSlot(slot);

                auditLogger.LogCreate(createdBy, "Timetable", "Timetable Slot", timetableID.ToString(),
                    string.Format("Added {0:hh\\:mm}-{1:hh\\:mm} slot for section {2}, subject {3}, day {4}, period {5}",
                        slot.StartTime, slot.EndTime, slot.SectionID, subject.SubjectName, slot.DayOfWeek, slot.PeriodNo));

                return timetableID;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(createdBy, "ERROR", "Timetable",
                    string.Format("Failed to add timetable slot for section ID {1}: {0}", ex.Message, slot.SectionID));
                throw;
            }
        }

        #endregion

        #region Update Operations

        /// <summary>
        /// Updates an existing timetable slot with conflict checking
        /// </summary>
        public bool UpdateTimetableSlot(Timetable slot, int updatedBy)
        {
            // Validate required fields
            if (slot == null)
                throw new ValidationException("Timetable slot data is required");

            if (slot.TimetableID <= 0)
                throw new ValidationException("Invalid timetable ID");

            if (slot.SectionID <= 0)
                throw new ValidationException("Section is required");

            if (slot.SubjectID <= 0)
                throw new ValidationException("Subject is required");

            if (slot.StaffID <= 0)
                throw new ValidationException("Staff member is required");

            if (slot.DayOfWeek < 1 || slot.DayOfWeek > 7)
                throw new ValidationException("Day of week must be between 1 (Monday) and 7 (Sunday)");

            if (slot.PeriodNo <= 0)
                throw new ValidationException("Period number is required");

            if (slot.StartTime == TimeSpan.Zero)
                throw new ValidationException("Start time is required");

            if (slot.EndTime == TimeSpan.Zero)
                throw new ValidationException("End time is required");

            if (slot.StartTime >= slot.EndTime)
                throw new ValidationException("Start time must be before end time");

            if ((slot.EndTime - slot.StartTime).TotalMinutes < 30)
                throw new ValidationException("Class duration must be at least 30 minutes");

            if (string.IsNullOrWhiteSpace(slot.RoomNumber))
                throw new ValidationException("Room number is required");

            if (slot.AcademicYearID <= 0)
                throw new ValidationException("Academic year is required");

            // Verify the slot exists
            Timetable existing = timetableDAL.GetTimetableById(slot.TimetableID);
            if (existing == null)
                throw new ValidationException("Timetable slot not found");

            // Verify the section exists
            Section section = sectionDAL.GetSectionById(slot.SectionID);
            if (section == null)
                throw new ValidationException("Specified section does not exist");

            // Verify the subject exists
            Subject subject = subjectDAL.GetSubjectById(slot.SubjectID);
            if (subject == null)
                throw new ValidationException("Specified subject does not exist");

            // Check teacher conflicts (excluding current slot)
            DataTable teacherConflicts = timetableDAL.CheckTeacherConflicts(slot.StaffID, slot.DayOfWeek, slot.StartTime, slot.EndTime, slot.TimetableID);
            if (teacherConflicts.Rows.Count > 0)
            {
                string conflictMsg = "Teacher has conflicting classes:\n";
                foreach (DataRow row in teacherConflicts.Rows)
                {
                    conflictMsg += string.Format("- {0} {1}: {2}\n", row["ClassName"], row["SectionName"], row["SubjectName"]);
                }
                throw new ValidationException(conflictMsg);
            }

            // Check room conflicts (excluding current slot)
            DataTable roomConflicts = timetableDAL.CheckRoomConflicts(slot.RoomNumber, slot.DayOfWeek, slot.StartTime, slot.EndTime, slot.TimetableID);
            if (roomConflicts.Rows.Count > 0)
            {
                string conflictMsg = "Room has conflicting bookings:\n";
                foreach (DataRow row in roomConflicts.Rows)
                {
                    conflictMsg += string.Format("- {0} {1}: {2} ({3:hh\\:mm}-{4:hh\\:mm})\n",
                        row["ClassName"], row["SectionName"], row["SubjectName"],
                        Convert.ToDateTime(row["StartTime"]).TimeOfDay,
                        Convert.ToDateTime(row["EndTime"]).TimeOfDay);
                }
                throw new ValidationException(conflictMsg);
            }

            // Check for duplicate period in the same section on the same day (excluding current slot)
            List<Timetable> existingTimetable = timetableDAL.GetTimetable(slot.SectionID, slot.AcademicYearID);
            for (int i = 0; i < existingTimetable.Count; i++)
            {
                if (existingTimetable[i].TimetableID != slot.TimetableID &&
                    existingTimetable[i].DayOfWeek == slot.DayOfWeek &&
                    existingTimetable[i].PeriodNo == slot.PeriodNo)
                {
                    throw new ValidationException(string.Format("Period {0} on day {1} is already assigned in this section",
                        slot.PeriodNo, slot.DayOfWeek));
                }

                // Check for time overlap in the same section on the same day (excluding current slot)
                if (existingTimetable[i].TimetableID != slot.TimetableID &&
                    existingTimetable[i].DayOfWeek == slot.DayOfWeek &&
                    slot.StartTime < existingTimetable[i].EndTime &&
                    slot.EndTime > existingTimetable[i].StartTime)
                {
                    throw new ValidationException(string.Format("Time slot overlaps with existing {0} class ({1:hh\\:mm}-{2:hh\\:mm})",
                        existingTimetable[i].SubjectName, existingTimetable[i].StartTime, existingTimetable[i].EndTime));
                }
            }

            try
            {
                bool result = timetableDAL.UpdateTimetableSlot(slot);

                if (result)
                {
                    // Log field changes
                    if (existing.SubjectID != slot.SubjectID)
                    {
                        auditLogger.LogUpdate(updatedBy, "Timetable", "Timetable Slot",
                            slot.TimetableID.ToString(), "SubjectID",
                            existing.SubjectID.ToString(), slot.SubjectID.ToString());
                    }
                    if (existing.StaffID != slot.StaffID)
                    {
                        auditLogger.LogUpdate(updatedBy, "Timetable", "Timetable Slot",
                            slot.TimetableID.ToString(), "StaffID",
                            existing.StaffID.ToString(), slot.StaffID.ToString());
                    }
                    if (existing.DayOfWeek != slot.DayOfWeek)
                    {
                        auditLogger.LogUpdate(updatedBy, "Timetable", "Timetable Slot",
                            slot.TimetableID.ToString(), "DayOfWeek",
                            existing.DayOfWeek.ToString(), slot.DayOfWeek.ToString());
                    }
                    if (existing.PeriodNo != slot.PeriodNo)
                    {
                        auditLogger.LogUpdate(updatedBy, "Timetable", "Timetable Slot",
                            slot.TimetableID.ToString(), "PeriodNo",
                            existing.PeriodNo.ToString(), slot.PeriodNo.ToString());
                    }
                    if (existing.StartTime != slot.StartTime)
                    {
                        auditLogger.LogUpdate(updatedBy, "Timetable", "Timetable Slot",
                            slot.TimetableID.ToString(), "StartTime",
                            existing.StartTime.ToString(), slot.StartTime.ToString());
                    }
                    if (existing.EndTime != slot.EndTime)
                    {
                        auditLogger.LogUpdate(updatedBy, "Timetable", "Timetable Slot",
                            slot.TimetableID.ToString(), "EndTime",
                            existing.EndTime.ToString(), slot.EndTime.ToString());
                    }
                    if (existing.RoomNumber != slot.RoomNumber)
                    {
                        auditLogger.LogUpdate(updatedBy, "Timetable", "Timetable Slot",
                            slot.TimetableID.ToString(), "RoomNumber",
                            existing.RoomNumber, slot.RoomNumber);
                    }
                }

                return result;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(updatedBy, "ERROR", "Timetable",
                    string.Format("Failed to update timetable slot ID {1}: {0}", ex.Message, slot.TimetableID));
                throw;
            }
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Deletes a timetable slot
        /// </summary>
        public bool DeleteTimetableSlot(int timetableID, int deletedBy)
        {
            if (timetableID <= 0)
                throw new ValidationException("Invalid timetable ID");

            Timetable existing = timetableDAL.GetTimetableById(timetableID);
            if (existing == null)
                throw new ValidationException("Timetable slot not found");

            try
            {
                bool result = timetableDAL.DeleteTimetableSlot(timetableID);

                if (result)
                {
                    auditLogger.LogDelete(deletedBy, "Timetable", "Timetable Slot",
                        timetableID.ToString(),
                        string.Format("Deleted {0:hh\\:mm}-{1:hh\\:mm} slot for section {2}, day {3}, period {4}",
                            existing.StartTime, existing.EndTime, existing.SectionID, existing.DayOfWeek, existing.PeriodNo));
                }

                return result;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(deletedBy, "ERROR", "Timetable",
                    string.Format("Failed to delete timetable slot ID {1}: {0}", ex.Message, timetableID));
                throw;
            }
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// Validates the entire timetable for a section and academic year
        /// </summary>
        public List<string> ValidateTimetable(int sectionID, int academicYearID)
        {
            List<string> issues = new List<string>();

            if (sectionID <= 0)
            {
                issues.Add("Invalid section ID");
                return issues;
            }

            if (academicYearID <= 0)
            {
                issues.Add("Invalid academic year ID");
                return issues;
            }

            List<Timetable> timetable = timetableDAL.GetTimetable(sectionID, academicYearID);

            // Check for double-booked teachers
            for (int i = 0; i < timetable.Count; i++)
            {
                for (int j = i + 1; j < timetable.Count; j++)
                {
                    if (timetable[i].StaffID == timetable[j].StaffID &&
                        timetable[i].DayOfWeek == timetable[j].DayOfWeek &&
                        timetable[i].StartTime < timetable[j].EndTime &&
                        timetable[i].EndTime > timetable[j].StartTime)
                    {
                        issues.Add(string.Format("Teacher double-booked on day {0} between {1:hh\\:mm} and {2:hh\\:mm}",
                            timetable[i].DayOfWeek, timetable[i].StartTime, timetable[j].EndTime));
                    }
                }
            }

            // Check for double-booked rooms
            for (int i = 0; i < timetable.Count; i++)
            {
                for (int j = i + 1; j < timetable.Count; j++)
                {
                    if (timetable[i].RoomNumber == timetable[j].RoomNumber &&
                        timetable[i].DayOfWeek == timetable[j].DayOfWeek &&
                        timetable[i].StartTime < timetable[j].EndTime &&
                        timetable[i].EndTime > timetable[j].StartTime)
                    {
                        issues.Add(string.Format("Room '{0}' double-booked on day {1} between {2:hh\\:mm} and {3:hh\\:mm}",
                            timetable[i].RoomNumber, timetable[i].DayOfWeek, timetable[i].StartTime, timetable[j].EndTime));
                    }
                }
            }

            // Check for consecutive classes without breaks (less than 5 minutes gap)
            for (int i = 0; i < timetable.Count; i++)
            {
                for (int j = 0; j < timetable.Count; j++)
                {
                    if (i != j &&
                        timetable[i].DayOfWeek == timetable[j].DayOfWeek &&
                        timetable[i].SectionID == timetable[j].SectionID)
                    {
                        TimeSpan gap = timetable[j].StartTime - timetable[i].EndTime;
                        if (gap.TotalMinutes > 0 && gap.TotalMinutes < 5)
                        {
                            issues.Add(string.Format("Insufficient break between classes on day {0} ({1:hh\\:mm}-{2:hh\\:mm} and {3:hh\\:mm}-{4:hh\\:mm})",
                                timetable[i].DayOfWeek, timetable[i].StartTime, timetable[i].EndTime,
                                timetable[j].StartTime, timetable[j].EndTime));
                        }
                    }
                }
            }

            // Check for duplicate periods in the same section on the same day
            for (int i = 0; i < timetable.Count; i++)
            {
                for (int j = i + 1; j < timetable.Count; j++)
                {
                    if (timetable[i].DayOfWeek == timetable[j].DayOfWeek &&
                        timetable[i].PeriodNo == timetable[j].PeriodNo)
                    {
                        issues.Add(string.Format("Duplicate period {0} on day {1}",
                            timetable[i].PeriodNo, timetable[i].DayOfWeek));
                    }
                }
            }

            return issues;
        }

        /// <summary>
        /// Validates whether a new timetable slot can be created
        /// </summary>
        public List<string> ValidateNewTimetableSlot(Timetable slot)
        {
            List<string> errors = new List<string>();

            if (slot == null)
            {
                errors.Add("Timetable slot data is required");
                return errors;
            }

            if (slot.SectionID <= 0)
                errors.Add("Section is required");

            if (slot.SubjectID <= 0)
                errors.Add("Subject is required");

            if (slot.StaffID <= 0)
                errors.Add("Staff member is required");

            if (slot.DayOfWeek < 1 || slot.DayOfWeek > 7)
                errors.Add("Day of week must be between 1 and 7");

            if (slot.PeriodNo <= 0)
                errors.Add("Period number is required");

            if (slot.StartTime