-- ============================================================
-- AQOONHUB SMS · Admissions module — additional fields
-- Adds PreviousSchool, LastGradeCompleted and AcademicYearID
-- to support the redesigned Admissions screen (inline New
-- Admission form). Safe to run multiple times (guarded).
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Admissions' AND COLUMN_NAME = 'PreviousSchool')
BEGIN
    ALTER TABLE Admissions ADD PreviousSchool NVARCHAR(150) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Admissions' AND COLUMN_NAME = 'LastGradeCompleted')
BEGIN
    ALTER TABLE Admissions ADD LastGradeCompleted NVARCHAR(50) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Admissions' AND COLUMN_NAME = 'AcademicYearID')
BEGIN
    ALTER TABLE Admissions ADD AcademicYearID INT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Admissions' AND COLUMN_NAME = 'Shift')
BEGIN
    ALTER TABLE Admissions ADD Shift NVARCHAR(20) NULL;
END
GO
