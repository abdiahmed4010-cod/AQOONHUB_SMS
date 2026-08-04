/* ============================================================================
   AQOONHUB SMS — Section Shift Integration (Stage 4)
   Adds a canonical Shift to dbo.Sections so Morning/Afternoon can be validated
   against student placement instead of living only as free text on Students.

   SAFE / IDEMPOTENT:
     - Adds Sections.Shift only if missing.
     - Adds a CHECK constraint restricting Shift to Morning/Afternoon (NULL allowed
       = "not yet assigned"), only if missing.
     - Backfills Sections.Shift ONLY for sections whose non-deleted students all
       share a single non-null shift (unambiguous). Ambiguous / empty sections are
       left NULL and reported — never guessed.
     - No data is deleted. Re-runnable with identical end state.
   ============================================================================ */
SET XACT_ABORT ON;
SET NOCOUNT ON;
GO

BEGIN TRANSACTION;

/* 1. Column ------------------------------------------------------------------ */
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID(N'dbo.Sections') AND name = N'Shift')
BEGIN
    ALTER TABLE dbo.Sections ADD Shift nvarchar(20) NULL;
END
GO

/* 2. Check constraint (NULL allowed = unassigned) ---------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
               WHERE parent_object_id = OBJECT_ID(N'dbo.Sections') AND name = N'CK_Sections_Shift')
BEGIN
    ALTER TABLE dbo.Sections WITH CHECK
        ADD CONSTRAINT CK_Sections_Shift
        CHECK (Shift IS NULL OR Shift IN (N'Morning', N'Afternoon'));
END
GO

/* 3. Safe backfill: only unambiguous sections, only where still NULL --------- */
;WITH DerivableShift AS (
    SELECT s.SectionID, MIN(s.Shift) AS DerivedShift
    FROM dbo.Students s
    WHERE s.Shift IS NOT NULL
      AND s.Status <> N'Deleted'
    GROUP BY s.SectionID
    HAVING COUNT(DISTINCT s.Shift) = 1
)
UPDATE se
    SET se.Shift = d.DerivedShift
FROM dbo.Sections se
JOIN DerivableShift d ON d.SectionID = se.SectionID
WHERE se.Shift IS NULL
  AND d.DerivedShift IN (N'Morning', N'Afternoon');
GO

COMMIT TRANSACTION;
GO

/* 4. Post-migration report (informational; changes nothing) ------------------ */
PRINT '--- Sections with an assigned shift ---';
SELECT SectionID, SectionName, Shift
FROM dbo.Sections
WHERE Shift IS NOT NULL
ORDER BY SectionID;

PRINT '--- AMBIGUOUS sections requiring manual shift assignment (students span both shifts) ---';
SELECT s.SectionID,
       MAX(se.SectionName)        AS SectionName,
       COUNT(DISTINCT s.Shift)    AS DistinctShifts,
       STRING_AGG(CONVERT(nvarchar(20), s.Shift), N',') AS ObservedShifts
FROM dbo.Students s
JOIN dbo.Sections se ON se.SectionID = s.SectionID
WHERE s.Shift IS NOT NULL
  AND s.Status <> N'Deleted'
GROUP BY s.SectionID
HAVING COUNT(DISTINCT s.Shift) > 1
ORDER BY s.SectionID;
GO
