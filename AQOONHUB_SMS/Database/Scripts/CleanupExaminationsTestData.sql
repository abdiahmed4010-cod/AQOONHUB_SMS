/* =====================================================================
   AQOONHUB_SMS - Examinations test-data cleanup
   ---------------------------------------------------------------------
   SAFETY MODEL:
     * ROLLBACK-FIRST. This script ROLLS BACK by default and changes
       NOTHING. Review the PREVIEW output, then - only if correct -
       change the single word ROLLBACK to COMMIT near the end and rerun.
     * One transaction, TRY/CATCH, child rows before parent rows.
     * Targets ONLY explicitly identified test rows by stable ID.
     * NEVER uses blind conditions like DELETE ... WHERE Name LIKE '%Test%'.

   PRESERVED (never touched):
     Students, Staff, Users, Guardians, StudentGuardians, AcademicYears,
     Terms, Classes, Sections, Subjects, GradingScale, ClassSubjectTeachers.

   DEMONSTRATION DATA - PRESERVED:
     ExamID = 1  'Mid Term Examination 2026' (Published). This is the
     working demonstration/reference examination and its published
     results, snapshots and publication history are KEPT.

   TEST DATA - REMOVED (only on COMMIT):
     ExamID = 2  'Final Exam Test' (Cancelled) and all its child rows.

   If the real IDs in your database differ, adjust @TestExamIds below
   AFTER verifying with the preview. Do not guess.
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;

/* Stable identifiers of the examinations to remove. */
DECLARE @TestExamIds TABLE (ExamID int PRIMARY KEY);
INSERT INTO @TestExamIds (ExamID)
SELECT ExamID FROM dbo.Exams
WHERE ExamID = 2 AND ExamName = 'Final Exam Test' AND ISNULL(Status,'') = 'Cancelled';

/* --------------------------- PREVIEW --------------------------- */
PRINT '=================== PREVIEW (no changes yet) ===================';
PRINT 'Examinations targeted for removal:';
SELECT e.ExamID, e.ExamName, e.Status FROM dbo.Exams e JOIN @TestExamIds t ON t.ExamID = e.ExamID;

PRINT 'Child-row counts that WOULD be deleted:';
SELECT
  (SELECT COUNT(*) FROM dbo.StudentExamSummaries s JOIN @TestExamIds t ON t.ExamID=s.ExamID) AS StudentExamSummaries,
  (SELECT COUNT(*) FROM dbo.ResultPublications p JOIN @TestExamIds t ON t.ExamID=p.ExamID)    AS ResultPublications,
  (SELECT COUNT(*) FROM dbo.ExamResults r JOIN @TestExamIds t ON t.ExamID=r.ExamID)           AS ExamResults,
  (SELECT COUNT(*) FROM dbo.ExamSchedules c JOIN @TestExamIds t ON t.ExamID=c.ExamID)         AS ExamSchedules,
  (SELECT COUNT(*) FROM dbo.ExamSubjects es JOIN @TestExamIds t ON t.ExamID=es.ExamID)        AS ExamSubjects,
  (SELECT COUNT(*) FROM dbo.ExamClasses ec JOIN @TestExamIds t ON t.ExamID=ec.ExamID)         AS ExamClasses,
  (SELECT COUNT(*) FROM dbo.Exams e JOIN @TestExamIds t ON t.ExamID=e.ExamID)                 AS Exams;

PRINT 'PRESERVED demonstration examination (must remain):';
SELECT ExamID, ExamName, Status FROM dbo.Exams WHERE ExamID = 1;

/* --------------- OPTIONAL: restore temporary placements ---------------
   No student SectionID/AcademicYearID or ClassSubjectTeachers.StaffID
   were permanently changed for testing (the promotion survival test used
   a ROLLBACK transaction). If a future test temporarily moves a student,
   record the original values and restore them here, e.g.:
       UPDATE dbo.Students SET SectionID = <orig>, AcademicYearID = <orig>
       WHERE StudentID = <id>;
   -------------------------------------------------------------------- */

/* --------------------------- DELETE --------------------------- */
BEGIN TRY
    BEGIN TRAN;

    DELETE s FROM dbo.StudentExamSummaries s JOIN @TestExamIds t ON t.ExamID = s.ExamID;
    DELETE p FROM dbo.ResultPublications   p JOIN @TestExamIds t ON t.ExamID = p.ExamID;
    DELETE r FROM dbo.ExamResults          r JOIN @TestExamIds t ON t.ExamID = r.ExamID;
    DELETE c FROM dbo.ExamSchedules        c JOIN @TestExamIds t ON t.ExamID = c.ExamID;
    DELETE es FROM dbo.ExamSubjects        es JOIN @TestExamIds t ON t.ExamID = es.ExamID;
    DELETE ec FROM dbo.ExamClasses         ec JOIN @TestExamIds t ON t.ExamID = ec.ExamID;
    DELETE e FROM dbo.Exams                e  JOIN @TestExamIds t ON t.ExamID = e.ExamID;

    /* Test-only exam rooms: remove ONLY rooms that are not seeded defaults
       and have zero schedule bookings. Adjust the name list if needed. */
    DELETE rm FROM dbo.ExamRooms rm
    WHERE rm.RoomName = 'Hall C'
      AND NOT EXISTS (SELECT 1 FROM dbo.ExamSchedules sc WHERE sc.ExamRoomID = rm.ExamRoomID);

    PRINT 'Delete statements executed inside transaction.';

    /* ============================================================
       DEFAULT = ROLLBACK (nothing is changed).
       To APPLY the cleanup: change the next word ROLLBACK to COMMIT
       and rerun this script.
       ============================================================ */
    ROLLBACK TRAN;
    PRINT 'ROLLED BACK - no changes committed. Change ROLLBACK to COMMIT to apply.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    PRINT 'ERROR - transaction rolled back.';
    PRINT ERROR_MESSAGE();
END CATCH;
