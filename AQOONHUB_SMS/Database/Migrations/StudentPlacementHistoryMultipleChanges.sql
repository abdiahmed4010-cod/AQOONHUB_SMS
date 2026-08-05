/* ============================================================================
   AQOONHUB SMS — Multiple same-year placement history support

   PROBLEM: the unique index UX_Promotion_Student_ToYear (StudentID, ToAcademicYearID)
   allowed only ONE StudentPromotions row per student per target academic year,
   wrongly blocking legitimate repeated same-year placement changes
   (Section A→B, then B→C, shift changes, corrections, later returns).

   FIX (safe / idempotent / non-destructive):
     - Drop ONLY that restrictive unique index (never PK or FKs).
     - Add two NON-UNIQUE indexes to query chronological history efficiently.
     - Do NOT add a replacement unique/business-key index: exact-duplicate
       prevention is handled at the application layer (ViewState confirmation
       token consumed on success + UPDLOCK/HOLDLOCK placement-baseline
       concurrency check + single transaction). A DB unique key here would risk
       blocking valid same-destination-later-date moves and same-day corrections.
     - No existing history row is read, changed, or deleted. Re-runnable.
   ============================================================================ */
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
GO

BEGIN TRANSACTION;

/* 1. Drop the restrictive unique index if present (leaves PK + FKs intact). */
IF EXISTS (SELECT 1 FROM sys.indexes
           WHERE name = N'UX_Promotion_Student_ToYear'
             AND object_id = OBJECT_ID(N'dbo.StudentPromotions'))
BEGIN
    DROP INDEX UX_Promotion_Student_ToYear ON dbo.StudentPromotions;
END

/* 2. Chronological history per student (Student Details / reports). */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_StudentPromotions_Student_ActionDate'
                 AND object_id = OBJECT_ID(N'dbo.StudentPromotions'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_StudentPromotions_Student_ActionDate
        ON dbo.StudentPromotions (StudentID, ActionDate DESC)
        INCLUDE (FromAcademicYearID, ToAcademicYearID, FromSectionID, ToSectionID, Status, CreatedAt);
END

/* 3. Per-student, per-target-year history lookups (promotion candidate status, etc.). */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_StudentPromotions_Student_ToYear_ActionDate'
                 AND object_id = OBJECT_ID(N'dbo.StudentPromotions'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_StudentPromotions_Student_ToYear_ActionDate
        ON dbo.StudentPromotions (StudentID, ToAcademicYearID, ActionDate DESC)
        INCLUDE (Status, ToSectionID, CreatedAt);
END

COMMIT TRANSACTION;
GO

/* Verification (informational). */
SELECT
    (SELECT COUNT(*) FROM sys.indexes WHERE name=N'UX_Promotion_Student_ToYear' AND object_id=OBJECT_ID(N'dbo.StudentPromotions')) AS OldUniqueIndex_ShouldBe0,
    (SELECT COUNT(*) FROM sys.indexes WHERE name=N'IX_StudentPromotions_Student_ActionDate' AND object_id=OBJECT_ID(N'dbo.StudentPromotions')) AS Idx1_ShouldBe1,
    (SELECT COUNT(*) FROM sys.indexes WHERE name=N'IX_StudentPromotions_Student_ToYear_ActionDate' AND object_id=OBJECT_ID(N'dbo.StudentPromotions')) AS Idx2_ShouldBe1,
    (SELECT COUNT(*) FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID(N'dbo.StudentPromotions')) AS ForeignKeys_ShouldStay3,
    (SELECT COUNT(*) FROM dbo.StudentPromotions) AS RowCount_Unchanged;
GO
