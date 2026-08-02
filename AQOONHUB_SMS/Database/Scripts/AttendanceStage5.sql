/* =====================================================================
   AQOONHUB_SMS - Attendance Stage 5
   Alert threshold settings + AttendanceAlerts workflow table.
   Idempotent, non-destructive, FK-protected, safely rerunnable.
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

/* ---------- New alert threshold settings (only genuinely-needed ones) ---------- */
IF COL_LENGTH('dbo.AttendanceSettings','FrequentLateThreshold')     IS NULL ALTER TABLE dbo.AttendanceSettings ADD FrequentLateThreshold     int NOT NULL CONSTRAINT DF_ASet_FLate DEFAULT(3);
IF COL_LENGTH('dbo.AttendanceSettings','UnsubmittedSessionAgeHours') IS NULL ALTER TABLE dbo.AttendanceSettings ADD UnsubmittedSessionAgeHours int NOT NULL CONSTRAINT DF_ASet_UAge  DEFAULT(48);
IF COL_LENGTH('dbo.AttendanceSettings','AlertLookbackDays')          IS NULL ALTER TABLE dbo.AttendanceSettings ADD AlertLookbackDays          int NOT NULL CONSTRAINT DF_ASet_Look  DEFAULT(30);
GO

/* ---------- AttendanceAlerts ---------- */
IF OBJECT_ID('dbo.AttendanceAlerts','U') IS NULL
CREATE TABLE dbo.AttendanceAlerts (
    AttendanceAlertID   int IDENTITY(1,1) PRIMARY KEY,
    AlertType           nvarchar(40)  NOT NULL,   -- ConsecutiveAbsence / LowAttendance / FrequentLate / UnsubmittedSession
    AlertKey            nvarchar(120) NOT NULL,   -- stable dedup key (type + entity + scope)
    StudentID           int           NULL,
    ClassID             int           NULL,
    SectionID           int           NULL,
    AttendanceSessionID int           NULL,
    Title               nvarchar(200) NOT NULL,
    Description         nvarchar(500) NULL,
    Severity            nvarchar(20)  NOT NULL CONSTRAINT DF_AA_Sev DEFAULT('Info'),   -- Info / Warning / Critical
    Status              nvarchar(20)  NOT NULL CONSTRAINT DF_AA_St  DEFAULT('New'),    -- New / Reviewed / Resolved / Dismissed
    TriggerValue        decimal(9,2)  NULL,
    ThresholdValue      decimal(9,2)  NULL,
    IsVisibleToParent   bit           NOT NULL CONSTRAINT DF_AA_Par DEFAULT(0),
    FirstDetectedAt     datetime      NOT NULL CONSTRAINT DF_AA_FD  DEFAULT(GETDATE()),
    LastDetectedAt      datetime      NOT NULL CONSTRAINT DF_AA_LD  DEFAULT(GETDATE()),
    ReviewedBy          int           NULL,
    ReviewedAt          datetime      NULL,
    ResolvedBy          int           NULL,
    ResolvedAt          datetime      NULL,
    ResolutionNotes     nvarchar(500) NULL,
    CreatedAt           datetime      NOT NULL CONSTRAINT DF_AA_Cre DEFAULT(GETDATE()),
    UpdatedAt           datetime      NULL
);
GO

IF OBJECT_ID('dbo.FK_AA_Student','F') IS NULL ALTER TABLE dbo.AttendanceAlerts WITH CHECK ADD CONSTRAINT FK_AA_Student FOREIGN KEY (StudentID) REFERENCES dbo.Students(StudentID);
GO

/* One ACTIVE alert per rule/entity (New/Reviewed). Resolved/Dismissed history is preserved. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_AA_ActiveKey' AND object_id=OBJECT_ID('dbo.AttendanceAlerts'))
    CREATE UNIQUE INDEX UX_AA_ActiveKey ON dbo.AttendanceAlerts(AlertKey) WHERE Status IN ('New','Reviewed');
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_AA_Student' AND object_id=OBJECT_ID('dbo.AttendanceAlerts'))
    CREATE INDEX IX_AA_Student ON dbo.AttendanceAlerts(StudentID, Status);
GO

PRINT 'Attendance Stage 5 alert settings + AttendanceAlerts schema applied.';
