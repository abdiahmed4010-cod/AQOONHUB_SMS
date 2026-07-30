/* =====================================================================
   AQOONHUB_SMS - Examinations module schema
   Idempotent, non-destructive. Extends the existing Exams / ExamResults /
   GradingScale tables and adds the scheduling, rooms, invigilation,
   subject-scope and result-publication tables the module needs.
   Safe to run multiple times. No DROPs. Existing data preserved.
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;

/* ---------- Exams: scope / marks / publish columns ---------- */
IF COL_LENGTH('dbo.Exams','AcademicYearID') IS NULL ALTER TABLE dbo.Exams ADD AcademicYearID int NULL;
IF COL_LENGTH('dbo.Exams','TotalMarks')     IS NULL ALTER TABLE dbo.Exams ADD TotalMarks int NOT NULL CONSTRAINT DF_Exams_Total DEFAULT(100);
IF COL_LENGTH('dbo.Exams','PassingMark')    IS NULL ALTER TABLE dbo.Exams ADD PassingMark int NOT NULL CONSTRAINT DF_Exams_Pass DEFAULT(40);
IF COL_LENGTH('dbo.Exams','Weight')         IS NULL ALTER TABLE dbo.Exams ADD Weight int NOT NULL CONSTRAINT DF_Exams_Weight DEFAULT(100);
IF COL_LENGTH('dbo.Exams','UpdatedAt')      IS NULL ALTER TABLE dbo.Exams ADD UpdatedAt datetime NULL;
IF COL_LENGTH('dbo.Exams','PublishedBy')    IS NULL ALTER TABLE dbo.Exams ADD PublishedBy int NULL;
IF COL_LENGTH('dbo.Exams','PublishedAt')    IS NULL ALTER TABLE dbo.Exams ADD PublishedAt datetime NULL;
GO
/* Backfill AcademicYearID from the exam's term */
UPDATE e SET e.AcademicYearID = t.AcademicYearID
FROM dbo.Exams e JOIN dbo.Terms t ON e.TermID = t.TermID
WHERE e.AcademicYearID IS NULL;
GO

/* ---------- GradingScale: description / pass flag / status ---------- */
IF COL_LENGTH('dbo.GradingScale','Description') IS NULL ALTER TABLE dbo.GradingScale ADD Description nvarchar(100) NULL;
IF COL_LENGTH('dbo.GradingScale','IsPass')      IS NULL ALTER TABLE dbo.GradingScale ADD IsPass bit NOT NULL CONSTRAINT DF_Grade_Pass DEFAULT(1);
IF COL_LENGTH('dbo.GradingScale','Status')      IS NULL ALTER TABLE dbo.GradingScale ADD Status nvarchar(20) NOT NULL CONSTRAINT DF_Grade_Status DEFAULT('Active');
GO
-- A failing grade is one whose minimum is below the school's default pass mark (40)
UPDATE dbo.GradingScale SET IsPass = CASE WHEN MinMarks < 40 THEN 0 ELSE 1 END WHERE Description IS NULL;
GO

/* ---------- ExamRooms ---------- */
IF OBJECT_ID('dbo.ExamRooms','U') IS NULL
CREATE TABLE dbo.ExamRooms (
    ExamRoomID int IDENTITY(1,1) PRIMARY KEY,
    RoomName   nvarchar(60) NOT NULL,
    Capacity   int NOT NULL DEFAULT(40),
    Location   nvarchar(100) NULL,
    Status     nvarchar(20) NOT NULL DEFAULT('Active'),
    CreatedAt  datetime NOT NULL DEFAULT(GETDATE())
);
GO

/* ---------- ExamClasses (which class/section an exam covers) ---------- */
IF OBJECT_ID('dbo.ExamClasses','U') IS NULL
CREATE TABLE dbo.ExamClasses (
    ExamClassID int IDENTITY(1,1) PRIMARY KEY,
    ExamID      int NOT NULL,
    ClassID     int NOT NULL,
    SectionID   int NULL,
    CreatedAt   datetime NOT NULL DEFAULT(GETDATE())
);
GO

/* ---------- ExamSubjects (subject scope + per-subject marks) ---------- */
IF OBJECT_ID('dbo.ExamSubjects','U') IS NULL
CREATE TABLE dbo.ExamSubjects (
    ExamSubjectID int IDENTITY(1,1) PRIMARY KEY,
    ExamID        int NOT NULL,
    ClassID       int NOT NULL,
    SectionID     int NULL,
    SubjectID     int NOT NULL,
    TotalMarks    int NOT NULL DEFAULT(100),
    PassingMark   int NOT NULL DEFAULT(40),
    CreatedAt     datetime NOT NULL DEFAULT(GETDATE())
);
GO

/* ---------- ExamSchedules ---------- */
IF OBJECT_ID('dbo.ExamSchedules','U') IS NULL
CREATE TABLE dbo.ExamSchedules (
    ExamScheduleID     int IDENTITY(1,1) PRIMARY KEY,
    ExamID             int NOT NULL,
    ClassID            int NOT NULL,
    SectionID          int NULL,
    SubjectID          int NOT NULL,
    ExamDate           date NOT NULL,
    StartTime          time NOT NULL,
    EndTime            time NOT NULL,
    ExamRoomID         int NULL,
    InvigilatorStaffID int NULL,
    Notes              nvarchar(300) NULL,
    Status             nvarchar(20) NOT NULL DEFAULT('Scheduled'),
    IsActive           bit NOT NULL DEFAULT(1),
    CreatedAt          datetime NOT NULL DEFAULT(GETDATE())
);
GO

/* ---------- ExamResults: marks status / totals / submission audit ---------- */
IF COL_LENGTH('dbo.ExamResults','TotalMarks')  IS NULL ALTER TABLE dbo.ExamResults ADD TotalMarks int NOT NULL CONSTRAINT DF_ER_Total DEFAULT(100);
IF COL_LENGTH('dbo.ExamResults','Status')      IS NULL ALTER TABLE dbo.ExamResults ADD Status nvarchar(20) NOT NULL CONSTRAINT DF_ER_Status DEFAULT('Draft');
IF COL_LENGTH('dbo.ExamResults','SubmittedBy') IS NULL ALTER TABLE dbo.ExamResults ADD SubmittedBy int NULL;
IF COL_LENGTH('dbo.ExamResults','SubmittedAt') IS NULL ALTER TABLE dbo.ExamResults ADD SubmittedAt datetime NULL;
IF COL_LENGTH('dbo.ExamResults','IsAbsent')    IS NULL ALTER TABLE dbo.ExamResults ADD IsAbsent bit NOT NULL CONSTRAINT DF_ER_Absent DEFAULT(0);
GO

/* ---------- ExamResults: allow NULL marks (Not Entered / Absent) ---------- */
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='ExamResults' AND COLUMN_NAME='Marks' AND IS_NULLABLE='NO')
    ALTER TABLE dbo.ExamResults ALTER COLUMN Marks decimal(6,2) NULL;
GO

/* ---------- ExamResults: attendance + reopen audit (Stage 4) ---------- */
IF COL_LENGTH('dbo.ExamResults','AttendanceStatus') IS NULL ALTER TABLE dbo.ExamResults ADD AttendanceStatus nvarchar(20) NOT NULL CONSTRAINT DF_ER_Att DEFAULT('Present');
IF COL_LENGTH('dbo.ExamResults','UpdatedAt')        IS NULL ALTER TABLE dbo.ExamResults ADD UpdatedAt datetime NULL;
IF COL_LENGTH('dbo.ExamResults','ReopenedBy')       IS NULL ALTER TABLE dbo.ExamResults ADD ReopenedBy int NULL;
IF COL_LENGTH('dbo.ExamResults','ReopenedAt')       IS NULL ALTER TABLE dbo.ExamResults ADD ReopenedAt datetime NULL;
IF COL_LENGTH('dbo.ExamResults','ReopenReason')     IS NULL ALTER TABLE dbo.ExamResults ADD ReopenReason nvarchar(300) NULL;
GO

/* ---------- ResultPublications ---------- */
IF OBJECT_ID('dbo.ResultPublications','U') IS NULL
CREATE TABLE dbo.ResultPublications (
    PublicationID int IDENTITY(1,1) PRIMARY KEY,
    ExamID        int NOT NULL,
    ClassID       int NOT NULL,
    SectionID     int NULL,
    Status        nvarchar(20) NOT NULL DEFAULT('Published'),
    PublishedBy   int NULL,
    PublishedAt   datetime NOT NULL DEFAULT(GETDATE()),
    Reason        nvarchar(300) NULL,
    CreatedAt     datetime NOT NULL DEFAULT(GETDATE())
);
GO

/* ---------- ResultPublications: unpublish audit (Stage 5) ---------- */
IF COL_LENGTH('dbo.ResultPublications','UnpublishedBy')  IS NULL ALTER TABLE dbo.ResultPublications ADD UnpublishedBy int NULL;
IF COL_LENGTH('dbo.ResultPublications','UnpublishedAt')  IS NULL ALTER TABLE dbo.ResultPublications ADD UnpublishedAt datetime NULL;
IF COL_LENGTH('dbo.ResultPublications','UnpublishReason') IS NULL ALTER TABLE dbo.ResultPublications ADD UnpublishReason nvarchar(300) NULL;
GO

/* ---------- Foreign keys (only when both sides exist and FK missing) ---------- */
IF OBJECT_ID('dbo.FK_ExamClasses_Exam','F') IS NULL ALTER TABLE dbo.ExamClasses WITH NOCHECK ADD CONSTRAINT FK_ExamClasses_Exam FOREIGN KEY (ExamID) REFERENCES dbo.Exams(ExamID);
IF OBJECT_ID('dbo.FK_ExamSubjects_Exam','F') IS NULL ALTER TABLE dbo.ExamSubjects WITH NOCHECK ADD CONSTRAINT FK_ExamSubjects_Exam FOREIGN KEY (ExamID) REFERENCES dbo.Exams(ExamID);
IF OBJECT_ID('dbo.FK_ExamSchedules_Exam','F') IS NULL ALTER TABLE dbo.ExamSchedules WITH NOCHECK ADD CONSTRAINT FK_ExamSchedules_Exam FOREIGN KEY (ExamID) REFERENCES dbo.Exams(ExamID);
IF OBJECT_ID('dbo.FK_ExamSchedules_Room','F') IS NULL ALTER TABLE dbo.ExamSchedules WITH NOCHECK ADD CONSTRAINT FK_ExamSchedules_Room FOREIGN KEY (ExamRoomID) REFERENCES dbo.ExamRooms(ExamRoomID);
IF OBJECT_ID('dbo.FK_ResultPub_Exam','F') IS NULL ALTER TABLE dbo.ResultPublications WITH NOCHECK ADD CONSTRAINT FK_ResultPub_Exam FOREIGN KEY (ExamID) REFERENCES dbo.Exams(ExamID);
GO

/* ---------- Unique constraints / indexes ---------- */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_ExamSubjects_Unique' AND object_id=OBJECT_ID('dbo.ExamSubjects'))
    CREATE UNIQUE INDEX UX_ExamSubjects_Unique ON dbo.ExamSubjects(ExamID, ClassID, SubjectID, SectionID) WHERE SectionID IS NOT NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_ExamSubjects_NoSection' AND object_id=OBJECT_ID('dbo.ExamSubjects'))
    CREATE UNIQUE INDEX UX_ExamSubjects_NoSection ON dbo.ExamSubjects(ExamID, ClassID, SubjectID) WHERE SectionID IS NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_ExamResults_StudentSubject' AND object_id=OBJECT_ID('dbo.ExamResults'))
   AND NOT EXISTS (SELECT ExamID FROM dbo.ExamResults GROUP BY ExamID, StudentID, SubjectID HAVING COUNT(*)>1)
    CREATE UNIQUE INDEX UX_ExamResults_StudentSubject ON dbo.ExamResults(ExamID, StudentID, SubjectID);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ExamSchedules_Exam' AND object_id=OBJECT_ID('dbo.ExamSchedules'))
    CREATE INDEX IX_ExamSchedules_Exam ON dbo.ExamSchedules(ExamID, ExamDate);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_ResultPub_Unique' AND object_id=OBJECT_ID('dbo.ResultPublications'))
    CREATE UNIQUE INDEX UX_ResultPub_Unique ON dbo.ResultPublications(ExamID, ClassID, SectionID) WHERE SectionID IS NOT NULL;
GO

/* ---------- Seed a couple of exam rooms if none exist ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.ExamRooms)
BEGIN
    INSERT INTO dbo.ExamRooms (RoomName, Capacity, Location, Status) VALUES
      ('Hall A', 60, 'Main Block', 'Active'),
      ('Hall B', 60, 'Main Block', 'Active'),
      ('Lab 1', 30, 'Science Block', 'Active');
END
GO

PRINT 'Examinations module schema applied successfully.';
