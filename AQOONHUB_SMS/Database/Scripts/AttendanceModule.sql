/* =====================================================================
   AQOONHUB_SMS - Attendance module schema (Stage 1)
   Session-based attendance with historical scope integrity.
   Idempotent, non-destructive, FK-protected, safely rerunnable. No DROPs.

   NOTE: a legacy flat 'Attendance' table exists but is EMPTY (0 rows) and
   has no session/workflow/academic-year scope. It is NOT equivalent to the
   required Draft/Submit/Lock session design, so it is left untouched
   (non-destructive) and the module standardises on the normalised tables
   below: AttendanceSessions + AttendanceRecords + AttendanceSettings.
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

/* ---------------------------------------------------------------------
   AttendanceSessions : one row per marked scope (date+year+class+section
   +optional subject/session-type). Preserves historical scope so promoting
   or transferring a student never rewrites past attendance.
   --------------------------------------------------------------------- */
IF OBJECT_ID('dbo.AttendanceSessions','U') IS NULL
CREATE TABLE dbo.AttendanceSessions (
    AttendanceSessionID int IDENTITY(1,1) PRIMARY KEY,
    AcademicYearID  int          NOT NULL,
    TermID          int          NULL,
    AttendanceDate  date         NOT NULL,
    ClassID         int          NOT NULL,
    SectionID       int          NOT NULL,
    SubjectID       int          NULL,
    SessionType     nvarchar(20) NOT NULL CONSTRAINT DF_AS_Type   DEFAULT('Daily'),
    Status          nvarchar(20) NOT NULL CONSTRAINT DF_AS_Status DEFAULT('Draft'),
    MarkedBy        int          NULL,
    SubmittedBy     int          NULL,
    SubmittedAt     datetime     NULL,
    ReopenedBy      int          NULL,
    ReopenedAt      datetime     NULL,
    ReopenReason    nvarchar(300) NULL,
    CreatedAt       datetime     NOT NULL CONSTRAINT DF_AS_Cre DEFAULT(GETDATE()),
    UpdatedAt       datetime     NULL
);
GO

/* ---------------------------------------------------------------------
   AttendanceRecords : one row per student per session.
   --------------------------------------------------------------------- */
IF OBJECT_ID('dbo.AttendanceRecords','U') IS NULL
CREATE TABLE dbo.AttendanceRecords (
    AttendanceRecordID  int IDENTITY(1,1) PRIMARY KEY,
    AttendanceSessionID int          NOT NULL,
    StudentID           int          NOT NULL,
    AttendanceStatus    nvarchar(20) NOT NULL CONSTRAINT DF_AR_Status DEFAULT('Present'),
    CheckInTime         time         NULL,
    LateMinutes         int          NULL,
    Remarks             nvarchar(300) NULL,
    RecordedBy          int          NULL,
    CreatedAt           datetime     NOT NULL CONSTRAINT DF_AR_Cre DEFAULT(GETDATE()),
    UpdatedAt           datetime     NULL
);
GO

/* ---------------------------------------------------------------------
   AttendanceSettings : single-row configuration.
   --------------------------------------------------------------------- */
IF OBJECT_ID('dbo.AttendanceSettings','U') IS NULL
CREATE TABLE dbo.AttendanceSettings (
    AttendanceSettingsID    int IDENTITY(1,1) PRIMARY KEY,
    AllowTeachersToMark     bit          NOT NULL CONSTRAINT DF_ASet_Mark  DEFAULT(1),
    AllowEditAfterSubmission bit         NOT NULL CONSTRAINT DF_ASet_Edit  DEFAULT(0),
    EditWindowHours         int          NOT NULL CONSTRAINT DF_ASet_Win   DEFAULT(24),
    AttendanceStartTime     time         NOT NULL CONSTRAINT DF_ASet_Start DEFAULT('07:00'),
    AttendanceEndTime       time         NOT NULL CONSTRAINT DF_ASet_End   DEFAULT('10:00'),
    LateAfterMinutes        int          NOT NULL CONSTRAINT DF_ASet_Late  DEFAULT(15),
    ExcusedRequiresRemarks  bit          NOT NULL CONSTRAINT DF_ASet_Exc   DEFAULT(1),
    IncludeLateAsAttended   bit          NOT NULL CONSTRAINT DF_ASet_IncL  DEFAULT(1),
    ExcludeExcusedFromRate  bit          NOT NULL CONSTRAINT DF_ASet_ExR   DEFAULT(1),
    AllowFutureDate         bit          NOT NULL CONSTRAINT DF_ASet_Fut   DEFAULT(0),
    EnableParentNotifications bit        NOT NULL CONSTRAINT DF_ASet_PN    DEFAULT(0),
    EnableEmailNotifications  bit        NOT NULL CONSTRAINT DF_ASet_EN    DEFAULT(0),
    EnableSMSNotifications    bit        NOT NULL CONSTRAINT DF_ASet_SN    DEFAULT(0),
    ConsecutiveAbsenceAlert   int        NOT NULL CONSTRAINT DF_ASet_CA    DEFAULT(3),
    LowAttendanceThreshold    decimal(5,2) NOT NULL CONSTRAINT DF_ASet_LA  DEFAULT(85.00),
    UpdatedBy               int          NULL,
    UpdatedAt               datetime     NULL,
    CreatedAt               datetime     NOT NULL CONSTRAINT DF_ASet_Cre   DEFAULT(GETDATE())
);
GO

/* Seed exactly one settings row. */
IF NOT EXISTS (SELECT 1 FROM dbo.AttendanceSettings)
    INSERT INTO dbo.AttendanceSettings DEFAULT VALUES;
GO

/* ---------------------------------------------------------------------
   Foreign keys (WITH NOCHECK so existing data is never blocked).
   --------------------------------------------------------------------- */
IF OBJECT_ID('dbo.FK_AS_Year','F')    IS NULL ALTER TABLE dbo.AttendanceSessions WITH NOCHECK ADD CONSTRAINT FK_AS_Year    FOREIGN KEY (AcademicYearID) REFERENCES dbo.AcademicYears(AcademicYearID);
IF OBJECT_ID('dbo.FK_AS_Class','F')   IS NULL ALTER TABLE dbo.AttendanceSessions WITH NOCHECK ADD CONSTRAINT FK_AS_Class   FOREIGN KEY (ClassID)   REFERENCES dbo.Classes(ClassID);
IF OBJECT_ID('dbo.FK_AS_Section','F') IS NULL ALTER TABLE dbo.AttendanceSessions WITH NOCHECK ADD CONSTRAINT FK_AS_Section FOREIGN KEY (SectionID) REFERENCES dbo.Sections(SectionID);
IF OBJECT_ID('dbo.FK_AR_Session','F') IS NULL ALTER TABLE dbo.AttendanceRecords  WITH NOCHECK ADD CONSTRAINT FK_AR_Session FOREIGN KEY (AttendanceSessionID) REFERENCES dbo.AttendanceSessions(AttendanceSessionID);
IF OBJECT_ID('dbo.FK_AR_Student','F') IS NULL ALTER TABLE dbo.AttendanceRecords  WITH NOCHECK ADD CONSTRAINT FK_AR_Student FOREIGN KEY (StudentID) REFERENCES dbo.Students(StudentID);
GO

/* ---------------------------------------------------------------------
   Uniqueness / indexes. Handle nullable SubjectID with two filtered
   unique indexes so one session per scope is enforced either way.
   --------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_AS_Scope_Subject' AND object_id=OBJECT_ID('dbo.AttendanceSessions'))
    CREATE UNIQUE INDEX UX_AS_Scope_Subject ON dbo.AttendanceSessions(AttendanceDate, AcademicYearID, ClassID, SectionID, SubjectID, SessionType)
        WHERE SubjectID IS NOT NULL AND Status <> 'Cancelled';
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_AS_Scope_NoSubject' AND object_id=OBJECT_ID('dbo.AttendanceSessions'))
    CREATE UNIQUE INDEX UX_AS_Scope_NoSubject ON dbo.AttendanceSessions(AttendanceDate, AcademicYearID, ClassID, SectionID, SessionType)
        WHERE SubjectID IS NULL AND Status <> 'Cancelled';
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_AR_SessionStudent' AND object_id=OBJECT_ID('dbo.AttendanceRecords'))
    CREATE UNIQUE INDEX UX_AR_SessionStudent ON dbo.AttendanceRecords(AttendanceSessionID, StudentID);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_AS_DateScope' AND object_id=OBJECT_ID('dbo.AttendanceSessions'))
    CREATE INDEX IX_AS_DateScope ON dbo.AttendanceSessions(AcademicYearID, AttendanceDate, ClassID, SectionID);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_AR_Student' AND object_id=OBJECT_ID('dbo.AttendanceRecords'))
    CREATE INDEX IX_AR_Student ON dbo.AttendanceRecords(StudentID);
GO

PRINT 'Attendance module schema (Stage 1) applied successfully.';
