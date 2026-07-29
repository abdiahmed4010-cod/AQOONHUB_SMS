/* =====================================================================
   AQOONHUB_SMS - Academics test-data cleanup (Stage 4/5 acceptance runs)
   ---------------------------------------------------------------------
   SAFE BY DEFAULT: wrapped in a transaction that ROLLS BACK.
   Review the preview SELECTs, then to actually delete change the final
   ROLLBACK to COMMIT (see the clearly marked line at the bottom).

   This script NEVER touches Students, Staff or Users.
   It only removes the artificial rows created while testing:
     - Class "Form 5" (code F5) and its sections "Form 5A" / "Form 5B"
     - Subject "Geography" (GEO-01)
     - The ClassSubjectTeachers rows for those sections/subject
     - The Timetable rows for those sections
   Children are deleted before parents.
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @ClassId   int = (SELECT ClassID   FROM Classes  WHERE ClassCode = 'F5');
DECLARE @GeoId     int = (SELECT SubjectID FROM Subjects WHERE SubjectCode = 'GEO-01');

/* ---------- PREVIEW: what would be removed ---------- */
PRINT '--- Class targeted (Form 5 / F5) ---';
SELECT ClassID, ClassName, ClassCode, Status FROM Classes WHERE ClassID = @ClassId;

PRINT '--- Sections under Form 5 ---';
SELECT SectionID, SectionName, ClassID FROM Sections WHERE ClassID = @ClassId;

PRINT '--- Subject targeted (Geography / GEO-01) ---';
SELECT SubjectID, SubjectCode, SubjectName FROM Subjects WHERE SubjectID = @GeoId;

PRINT '--- ClassSubjectTeachers rows to remove ---';
SELECT cst.* FROM ClassSubjectTeachers cst
JOIN Sections s ON cst.SectionID = s.SectionID
WHERE s.ClassID = @ClassId OR cst.SubjectID = @GeoId;

PRINT '--- Timetable rows to remove ---';
SELECT t.* FROM Timetable t
JOIN Sections s ON t.SectionID = s.SectionID
WHERE s.ClassID = @ClassId;

PRINT '--- SAFETY: students that reference these sections (MUST be zero before deleting) ---';
SELECT StudentID, StudentCode, SectionID FROM Students
WHERE SectionID IN (SELECT SectionID FROM Sections WHERE ClassID = @ClassId);

/* ---------- DELETE (children first). Runs inside a rolled-back tx. ---------- */
BEGIN TRAN;

    -- Do not proceed if any real student is enrolled in these sections.
    IF EXISTS (SELECT 1 FROM Students WHERE SectionID IN (SELECT SectionID FROM Sections WHERE ClassID = @ClassId))
    BEGIN
        PRINT 'ABORT: real students are enrolled in these sections. No deletion performed.';
    END
    ELSE
    BEGIN
        DELETE t FROM Timetable t
        JOIN Sections s ON t.SectionID = s.SectionID
        WHERE s.ClassID = @ClassId;

        DELETE cst FROM ClassSubjectTeachers cst
        JOIN Sections s ON cst.SectionID = s.SectionID
        WHERE s.ClassID = @ClassId OR cst.SubjectID = @GeoId;

        DELETE FROM Sections WHERE ClassID = @ClassId;
        DELETE FROM Classes  WHERE ClassID = @ClassId;
        DELETE FROM Subjects WHERE SubjectID = @GeoId;

        PRINT 'Test rows deleted (still inside the transaction).';
    END

/* ---------------------------------------------------------------------
   DEFAULT = ROLLBACK (nothing is actually changed).
   To PERFORM the cleanup for real, comment the ROLLBACK line and
   uncomment the COMMIT line below, then run again.
   --------------------------------------------------------------------- */
ROLLBACK TRAN;   -- <== safe default
-- COMMIT TRAN;  -- <== uncomment to apply the cleanup permanently
