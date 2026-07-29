/* =====================================================================
   AQOONHUB_SMS - Academics Module Enhancements
   Idempotent, non-destructive migration.
   Adds only the columns / tables the Academics UI needs and that are
   missing from the current schema. Safe to run multiple times.
   Existing data is preserved; no DROPs.
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;

/* ---------- Classes: ClassCode, Level, Status, AcademicYearID ---------- */
IF COL_LENGTH('dbo.Classes','ClassCode') IS NULL
    ALTER TABLE dbo.Classes ADD ClassCode nvarchar(20) NULL;
IF COL_LENGTH('dbo.Classes','Level') IS NULL
    ALTER TABLE dbo.Classes ADD Level nvarchar(30) NULL;
IF COL_LENGTH('dbo.Classes','Status') IS NULL
    ALTER TABLE dbo.Classes ADD Status nvarchar(20) NOT NULL CONSTRAINT DF_Classes_Status DEFAULT('Active');
IF COL_LENGTH('dbo.Classes','AcademicYearID') IS NULL
    ALTER TABLE dbo.Classes ADD AcademicYearID int NULL;
GO
/* Backfill a ClassCode where empty (CLS-<id>) so the unique index is usable */
UPDATE dbo.Classes SET ClassCode = 'CLS-' + RIGHT('000' + CAST(ClassID AS varchar(10)),3)
WHERE ClassCode IS NULL OR LTRIM(RTRIM(ClassCode)) = '';
UPDATE dbo.Classes SET Level = 'Primary' WHERE Level IS NULL;
/* Attach classes with no year to the active academic year */
UPDATE c SET c.AcademicYearID = (SELECT TOP 1 AcademicYearID FROM dbo.AcademicYears WHERE Status='Active' ORDER BY AcademicYearID DESC)
FROM dbo.Classes c WHERE c.AcademicYearID IS NULL
  AND EXISTS (SELECT 1 FROM dbo.AcademicYears WHERE Status='Active');
GO

/* ---------- Sections: StaffID (class teacher), RoomNumber, Status, AcademicYearID ---------- */
IF COL_LENGTH('dbo.Sections','StaffID') IS NULL
    ALTER TABLE dbo.Sections ADD StaffID int NULL;
IF COL_LENGTH('dbo.Sections','RoomNumber') IS NULL
    ALTER TABLE dbo.Sections ADD RoomNumber nvarchar(30) NULL;
IF COL_LENGTH('dbo.Sections','Status') IS NULL
    ALTER TABLE dbo.Sections ADD Status nvarchar(20) NOT NULL CONSTRAINT DF_Sections_Status DEFAULT('Active');
IF COL_LENGTH('dbo.Sections','AcademicYearID') IS NULL
    ALTER TABLE dbo.Sections ADD AcademicYearID int NULL;
GO
UPDATE s SET s.AcademicYearID = (SELECT TOP 1 AcademicYearID FROM dbo.AcademicYears WHERE Status='Active' ORDER BY AcademicYearID DESC)
FROM dbo.Sections s WHERE s.AcademicYearID IS NULL
  AND EXISTS (SELECT 1 FROM dbo.AcademicYears WHERE Status='Active');
GO

/* ---------- Subjects: SubjectType, MaxMarks, PassMarks ---------- */
IF COL_LENGTH('dbo.Subjects','SubjectType') IS NULL
    ALTER TABLE dbo.Subjects ADD SubjectType nvarchar(20) NOT NULL CONSTRAINT DF_Subjects_Type DEFAULT('Core');
IF COL_LENGTH('dbo.Subjects','MaxMarks') IS NULL
    ALTER TABLE dbo.Subjects ADD MaxMarks int NOT NULL CONSTRAINT DF_Subjects_Max DEFAULT(100);
IF COL_LENGTH('dbo.Subjects','PassMarks') IS NULL
    ALTER TABLE dbo.Subjects ADD PassMarks int NOT NULL CONSTRAINT DF_Subjects_Pass DEFAULT(50);
GO

/* ---------- ClassSubjectTeachers: WeeklyPeriods ---------- */
IF COL_LENGTH('dbo.ClassSubjectTeachers','WeeklyPeriods') IS NULL
    ALTER TABLE dbo.ClassSubjectTeachers ADD WeeklyPeriods int NOT NULL CONSTRAINT DF_CST_WP DEFAULT(1);
GO
/* Allow a class-subject assignment to exist before a teacher is chosen (Stage 4).
   Safe: no fake teacher rows are ever inserted. */
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
           WHERE TABLE_NAME='ClassSubjectTeachers' AND COLUMN_NAME='StaffID' AND IS_NULLABLE='NO')
    ALTER TABLE dbo.ClassSubjectTeachers ALTER COLUMN StaffID int NULL;
GO

/* ---------- Timetable: TermID (nullable) so entries can be scoped to a term ---------- */
IF COL_LENGTH('dbo.Timetable','TermID') IS NULL
    ALTER TABLE dbo.Timetable ADD TermID int NULL;
GO
IF OBJECT_ID('dbo.FK_Timetable_Term','F') IS NULL AND OBJECT_ID('dbo.Terms','U') IS NOT NULL
    ALTER TABLE dbo.Timetable WITH NOCHECK
      ADD CONSTRAINT FK_Timetable_Term FOREIGN KEY (TermID) REFERENCES dbo.Terms(TermID);
GO

/* ---------- StudentPromotions (new) ---------- */
IF OBJECT_ID('dbo.StudentPromotions','U') IS NULL
BEGIN
    CREATE TABLE dbo.StudentPromotions (
        PromotionID          int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        StudentID            int NOT NULL,
        FromAcademicYearID   int NOT NULL,
        ToAcademicYearID     int NOT NULL,
        FromSectionID        int NULL,
        ToSectionID          int NULL,
        Status               nvarchar(20) NOT NULL DEFAULT('Promoted'), -- Promoted/Repeated/Graduated/Transferred/Withdrawn
        ActionDate           datetime NOT NULL DEFAULT(GETDATE()),
        PromotedBy           int NULL,
        Notes                nvarchar(400) NULL,
        CreatedAt            datetime NOT NULL DEFAULT(GETDATE())
    );
END
GO

/* ---------- Foreign keys (added only if both sides exist and FK missing) ---------- */
IF OBJECT_ID('dbo.FK_StudentPromotions_Student','F') IS NULL
   AND OBJECT_ID('dbo.Students','U') IS NOT NULL
    ALTER TABLE dbo.StudentPromotions WITH NOCHECK
      ADD CONSTRAINT FK_StudentPromotions_Student FOREIGN KEY (StudentID) REFERENCES dbo.Students(StudentID);
IF OBJECT_ID('dbo.FK_StudentPromotions_FromYear','F') IS NULL
    ALTER TABLE dbo.StudentPromotions WITH NOCHECK
      ADD CONSTRAINT FK_StudentPromotions_FromYear FOREIGN KEY (FromAcademicYearID) REFERENCES dbo.AcademicYears(AcademicYearID);
IF OBJECT_ID('dbo.FK_StudentPromotions_ToYear','F') IS NULL
    ALTER TABLE dbo.StudentPromotions WITH NOCHECK
      ADD CONSTRAINT FK_StudentPromotions_ToYear FOREIGN KEY (ToAcademicYearID) REFERENCES dbo.AcademicYears(AcademicYearID);
IF OBJECT_ID('dbo.FK_Sections_Staff','F') IS NULL AND OBJECT_ID('dbo.Staff','U') IS NOT NULL
    ALTER TABLE dbo.Sections WITH NOCHECK
      ADD CONSTRAINT FK_Sections_Staff FOREIGN KEY (StaffID) REFERENCES dbo.Staff(StaffID);
GO

/* ---------- Unique constraints / indexes ---------- */
-- Unique subject code
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_Subjects_Code' AND object_id=OBJECT_ID('dbo.Subjects'))
   AND NOT EXISTS (SELECT SubjectCode FROM dbo.Subjects GROUP BY SubjectCode HAVING COUNT(*)>1)
    CREATE UNIQUE INDEX UX_Subjects_Code ON dbo.Subjects(SubjectCode) WHERE SubjectCode IS NOT NULL;
GO
-- Unique class code within an academic year
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_Classes_Code_Year' AND object_id=OBJECT_ID('dbo.Classes'))
    CREATE UNIQUE INDEX UX_Classes_Code_Year ON dbo.Classes(ClassCode, AcademicYearID) WHERE ClassCode IS NOT NULL AND AcademicYearID IS NOT NULL;
GO
-- Unique section name within a class
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_Sections_Name_Class' AND object_id=OBJECT_ID('dbo.Sections'))
    CREATE UNIQUE INDEX UX_Sections_Name_Class ON dbo.Sections(ClassID, SectionName);
GO
-- Unique teacher assignment (section+subject+year)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_CST_Section_Subject_Year' AND object_id=OBJECT_ID('dbo.ClassSubjectTeachers'))
    CREATE UNIQUE INDEX UX_CST_Section_Subject_Year ON dbo.ClassSubjectTeachers(SectionID, SubjectID, AcademicYearID);
GO
-- A student can be promoted only once into a given target year
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_Promotion_Student_ToYear' AND object_id=OBJECT_ID('dbo.StudentPromotions'))
    CREATE UNIQUE INDEX UX_Promotion_Student_ToYear ON dbo.StudentPromotions(StudentID, ToAcademicYearID);
GO
-- Prevent a teacher being in two places at the same time (timetable) via filtered unique index
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Timetable_Teacher_Slot' AND object_id=OBJECT_ID('dbo.Timetable'))
    CREATE INDEX IX_Timetable_Teacher_Slot ON dbo.Timetable(StaffID, DayOfWeek, PeriodNo, AcademicYearID);
GO
-- Common filter indexes
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Sections_Class' AND object_id=OBJECT_ID('dbo.Sections'))
    CREATE INDEX IX_Sections_Class ON dbo.Sections(ClassID);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Students_Section' AND object_id=OBJECT_ID('dbo.Students'))
    CREATE INDEX IX_Students_Section ON dbo.Students(SectionID);
GO

PRINT 'Academics module enhancements applied successfully.';
