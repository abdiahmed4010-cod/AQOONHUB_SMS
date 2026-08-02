/* =====================================================================
   AQOONHUB_SMS - Attendance test-data cleanup  (Stage 6)
   ---------------------------------------------------------------------
   SAFETY MODEL
     * ROLLBACK-FIRST. This script ROLLS BACK by default and changes
       NOTHING. Review the PREVIEW output, then - only if correct -
       change the single word ROLLBACK to COMMIT near the end and rerun.
     * One transaction, TRY/CATCH, child rows before parent rows.
     * Targets ONLY identifiable test data. NEVER touches production
       Students, Users, Staff, Guardians, Classes, Sections,
       AcademicYears, Terms or Subjects.

   TEST-DATA MARKERS (adjust only after verifying with the preview):
     * Temporary Stage-6 accounts use the email domain '@stage6test.local'.
     * Test import batches use a FileHash beginning with 'hash' (harness) -
       real imports use a 64-char SHA-256 hex string.
     * Attendance sessions/records/alerts created during testing are
       identified per run; edit @TestFromDate/@TestToDate/@TestSectionID
       below to match the exact scope you tested.

   NOTE: At the time of writing, all runtime test data from Stages 2-5 was
   already removed by each stage's harness (Sessions/Records/Alerts/Batches
   = 0). This script is the durable, reusable cleanup for any future test
   data and for the Stage-6 temporary accounts.
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @TestSectionID int = 13;                 -- section used for test attendance
DECLARE @TestFromDate  date = '2026-05-01';      -- inclusive
DECLARE @TestToDate    date = '2026-05-31';      -- inclusive
DECLARE @TestGuardianID int = 1;                 -- guardian used for temp parent links

/* Collect the exact test session ids in scope (non-production dates/section). */
DECLARE @TestSessions TABLE (AttendanceSessionID int PRIMARY KEY);
INSERT INTO @TestSessions
SELECT AttendanceSessionID FROM dbo.AttendanceSessions
WHERE SectionID = @TestSectionID AND AttendanceDate BETWEEN @TestFromDate AND @TestToDate;

/* --------------------------- PREVIEW --------------------------- */
PRINT '=================== PREVIEW (no changes yet) ===================';
PRINT 'Temporary Stage-6 accounts:';
SELECT UserID, Email, Role FROM dbo.Users WHERE Email LIKE '%@stage6test.local';

PRINT 'Test attendance sessions in scope:';
SELECT s.AttendanceSessionID, s.AttendanceDate, s.SectionID, s.Status
FROM dbo.AttendanceSessions s JOIN @TestSessions t ON t.AttendanceSessionID = s.AttendanceSessionID;

PRINT 'Counts that WOULD be deleted:';
SELECT
  (SELECT COUNT(*) FROM dbo.AttendanceRecords r JOIN @TestSessions t ON t.AttendanceSessionID=r.AttendanceSessionID) AS TestRecords,
  (SELECT COUNT(*) FROM @TestSessions) AS TestSessions,
  (SELECT COUNT(*) FROM dbo.AttendanceAlerts WHERE AttendanceSessionID IN (SELECT AttendanceSessionID FROM @TestSessions)) AS SessionAlerts,
  (SELECT COUNT(*) FROM dbo.AttendanceImportBatches WHERE FileHash LIKE 'hash%') AS TestImportBatches,
  (SELECT COUNT(*) FROM dbo.StudentGuardians sg JOIN dbo.Users u ON 1=0) AS _unused,
  (SELECT COUNT(*) FROM dbo.Users WHERE Email LIKE '%@stage6test.local') AS TempAccounts;

/* --------------------------- DELETE (test data only) --------------------------- */
BEGIN TRY
    BEGIN TRAN;

    /* child rows before parents */
    DELETE r FROM dbo.AttendanceRecords r JOIN @TestSessions t ON t.AttendanceSessionID = r.AttendanceSessionID;
    DELETE a FROM dbo.AttendanceAlerts  a WHERE a.AttendanceSessionID IN (SELECT AttendanceSessionID FROM @TestSessions);
    /* student-scoped alerts created during testing (only for the test section's students) */
    DELETE a FROM dbo.AttendanceAlerts a
    WHERE a.StudentID IN (SELECT StudentID FROM dbo.Students WHERE SectionID = @TestSectionID)
      AND a.CreatedAt >= @TestFromDate;
    DELETE s FROM dbo.AttendanceSessions s JOIN @TestSessions t ON t.AttendanceSessionID = s.AttendanceSessionID;

    /* harness/test import batches (hash marker only - never 64-char real hashes) */
    DELETE FROM dbo.AttendanceImportBatches WHERE FileHash LIKE 'hash%';

    /* temporary parent links created for testing (guardian used only for tests).
       Only remove links to students in the test section to avoid touching real links. */
    DELETE sg FROM dbo.StudentGuardians sg
    WHERE sg.GuardianID = @TestGuardianID
      AND sg.StudentID IN (SELECT StudentID FROM dbo.Students WHERE SectionID = @TestSectionID);

    /* temporary Stage-6 test accounts (identifiable domain) */
    DELETE FROM dbo.Users WHERE Email LIKE '%@stage6test.local';

    /* --- OPTIONAL, MANUAL restoration of temporarily-moved students ---
       If a test moved a student's SectionID/AcademicYearID, restore the
       EXACT recorded original values here, e.g.:
         -- UPDATE dbo.Students SET SectionID = 13 WHERE StudentID = 4;
       Do not guess values. */

    PRINT 'Delete statements executed inside transaction.';

    /* ============================================================
       DEFAULT = ROLLBACK (nothing is changed).
       To APPLY: change the next word ROLLBACK to COMMIT and rerun.
       ============================================================ */
    ROLLBACK TRAN;
    PRINT 'ROLLED BACK - no changes committed. Change ROLLBACK to COMMIT to apply.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    PRINT 'ERROR - transaction rolled back.';
    PRINT ERROR_MESSAGE();
END CATCH;

/* --------------------------- VERIFY (after COMMIT) --------------------------- */
PRINT '=================== POST-CLEANUP VERIFICATION ===================';
SELECT 'Sessions' AS T, COUNT(*) AS N FROM dbo.AttendanceSessions
UNION ALL SELECT 'Records', COUNT(*) FROM dbo.AttendanceRecords
UNION ALL SELECT 'Alerts', COUNT(*) FROM dbo.AttendanceAlerts
UNION ALL SELECT 'ImportBatches', COUNT(*) FROM dbo.AttendanceImportBatches
UNION ALL SELECT 'TempAccounts', COUNT(*) FROM dbo.Users WHERE Email LIKE '%@stage6test.local';

/* =====================================================================
   OPTIONAL - LEGACY 'Attendance' TABLE + DORMANT App_Code CLASSES
   ---------------------------------------------------------------------
   The legacy flat dbo.Attendance table is EMPTY (0 rows) and is NOT
   referenced by any compiled code (App_Code\DataAccess\AttendanceDAL.cs,
   App_Code\BusinessLogic\AttendanceBLL.cs and App_Code\Models\
   AttendanceRecord.cs are marked <Content>, i.e. NOT compiled). The
   session-based module uses AttendanceSessions + AttendanceRecords.

   Removal is SAFE but DEFERRED (non-destructive by default). To drop the
   legacy table manually, review then run:

       -- IF (SELECT COUNT(*) FROM dbo.Attendance) = 0
       --     DROP TABLE dbo.Attendance;

   And optionally delete the three dormant App_Code files from the project.
   Do NOT run automatically.
   ===================================================================== */
