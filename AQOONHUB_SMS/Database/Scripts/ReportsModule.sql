/* =====================================================================
   AQOONHUB_SMS - Reports module schema  (Stage 1)
   Centralised reporting: saved reports, scheduled reports, export
   history and a report-specific audit log.
   Idempotent, non-destructive, FK-protected, safely rerunnable. No DROPs.

   NOTE: a legacy 'dbo.Reports' table exists (0 rows) and is referenced
   ONLY by dormant, uncompiled App_Code (ReportBLL.cs/ReportDAL.cs are
   <Content>, not built). It is left UNTOUCHED here. Optional non-
   destructive removal is documented at the end of this script.
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

/* ---------- SavedReports ---------- */
IF OBJECT_ID('dbo.SavedReports','U') IS NULL
CREATE TABLE dbo.SavedReports (
    SavedReportID     int IDENTITY(1,1) PRIMARY KEY,
    ReportName        nvarchar(150) NOT NULL,
    ReportKey         nvarchar(80)  NOT NULL,       -- catalog key (whitelisted)
    Category          nvarchar(60)  NOT NULL,
    ConfigurationJson nvarchar(MAX) NULL,           -- filters/columns/sort (no SQL)
    Visibility        nvarchar(20)  NOT NULL CONSTRAINT DF_SR_Vis DEFAULT('Private'),  -- Private / Role-Based / School-Wide
    OwnerUserID       int           NULL,
    IsActive          bit           NOT NULL CONSTRAINT DF_SR_Act DEFAULT(1),
    CreatedAt         datetime      NOT NULL CONSTRAINT DF_SR_Cre DEFAULT(GETDATE()),
    UpdatedAt         datetime      NULL,
    LastRunAt         datetime      NULL
);
GO

/* ---------- ScheduledReports ---------- */
IF OBJECT_ID('dbo.ScheduledReports','U') IS NULL
CREATE TABLE dbo.ScheduledReports (
    ScheduledReportID int IDENTITY(1,1) PRIMARY KEY,
    SavedReportID     int           NULL,
    Frequency         nvarchar(20)  NOT NULL,       -- Daily / Weekly / Monthly / Termly / Yearly
    RunTime           time          NULL,
    DayOfWeek         int           NULL,
    DayOfMonth        int           NULL,
    Recipients        nvarchar(500) NULL,
    ExportFormat      nvarchar(20)  NOT NULL CONSTRAINT DF_SCH_Fmt DEFAULT('CSV'),
    Status            nvarchar(30)  NOT NULL CONSTRAINT DF_SCH_St DEFAULT('Pending Scheduler Configuration'),
    LastRunAt         datetime      NULL,
    NextRunAt         datetime      NULL,
    CreatedBy         int           NULL,
    CreatedAt         datetime      NOT NULL CONSTRAINT DF_SCH_Cre DEFAULT(GETDATE()),
    UpdatedAt         datetime      NULL
);
GO

/* ---------- ReportExports (metadata only - never the file bytes) ---------- */
IF OBJECT_ID('dbo.ReportExports','U') IS NULL
CREATE TABLE dbo.ReportExports (
    ReportExportID    int IDENTITY(1,1) PRIMARY KEY,
    ReportKey         nvarchar(80)  NOT NULL,
    ReportName        nvarchar(150) NOT NULL,
    Category          nvarchar(60)  NOT NULL,
    ExportFormat      nvarchar(20)  NOT NULL,        -- CSV / PDF(Print)
    FilterSummary     nvarchar(500) NULL,
    FileName          nvarchar(260) NULL,
    FilePath          nvarchar(400) NULL,
    FileSize          bigint        NULL,
    Status            nvarchar(30)  NOT NULL CONSTRAINT DF_RE_St DEFAULT('Generated'),
    GeneratedBy       int           NULL,
    GeneratedAt       datetime      NOT NULL CONSTRAINT DF_RE_Gen DEFAULT(GETDATE()),
    ExpiresAt         datetime      NULL
);
GO

/* ---------- ReportAuditLogs (append-only) ---------- */
IF OBJECT_ID('dbo.ReportAuditLogs','U') IS NULL
CREATE TABLE dbo.ReportAuditLogs (
    ReportAuditLogID  int IDENTITY(1,1) PRIMARY KEY,
    UserID            int           NULL,
    Action            nvarchar(40)  NOT NULL,        -- Viewed / Generated / Exported / Printed / Saved / Updated / Deleted / Scheduled / ScheduleDisabled
    ReportKey         nvarchar(80)  NULL,
    ReportName        nvarchar(150) NULL,
    Category          nvarchar(60)  NULL,
    FilterSummary     nvarchar(500) NULL,
    ResultStatus      nvarchar(20)  NOT NULL CONSTRAINT DF_RAL_St DEFAULT('Success'),
    IpAddress         nvarchar(60)  NULL,
    CreatedAt         datetime      NOT NULL CONSTRAINT DF_RAL_Cre DEFAULT(GETDATE())
);
GO

/* ---------- Foreign keys (WITH CHECK so trusted; only when Users exists) ---------- */
IF OBJECT_ID('dbo.Users','U') IS NOT NULL
BEGIN
    IF OBJECT_ID('dbo.FK_SR_Owner','F')   IS NULL ALTER TABLE dbo.SavedReports     WITH CHECK ADD CONSTRAINT FK_SR_Owner   FOREIGN KEY (OwnerUserID) REFERENCES dbo.Users(UserID);
    IF OBJECT_ID('dbo.FK_SCH_Saved','F')  IS NULL ALTER TABLE dbo.ScheduledReports WITH CHECK ADD CONSTRAINT FK_SCH_Saved  FOREIGN KEY (SavedReportID) REFERENCES dbo.SavedReports(SavedReportID);
    IF OBJECT_ID('dbo.FK_SCH_By','F')     IS NULL ALTER TABLE dbo.ScheduledReports WITH CHECK ADD CONSTRAINT FK_SCH_By     FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(UserID);
    IF OBJECT_ID('dbo.FK_RE_By','F')      IS NULL ALTER TABLE dbo.ReportExports    WITH CHECK ADD CONSTRAINT FK_RE_By      FOREIGN KEY (GeneratedBy) REFERENCES dbo.Users(UserID);
    IF OBJECT_ID('dbo.FK_RAL_User','F')   IS NULL ALTER TABLE dbo.ReportAuditLogs  WITH CHECK ADD CONSTRAINT FK_RAL_User   FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID);
END
GO

/* ---------- Indexes ---------- */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_RAL_CreatedAt' AND object_id=OBJECT_ID('dbo.ReportAuditLogs'))
    CREATE INDEX IX_RAL_CreatedAt ON dbo.ReportAuditLogs(CreatedAt DESC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_RE_GeneratedAt' AND object_id=OBJECT_ID('dbo.ReportExports'))
    CREATE INDEX IX_RE_GeneratedAt ON dbo.ReportExports(GeneratedAt DESC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_SR_Owner' AND object_id=OBJECT_ID('dbo.SavedReports'))
    CREATE INDEX IX_SR_Owner ON dbo.SavedReports(OwnerUserID, IsActive);
GO

PRINT 'Reports module schema (Stage 1) applied successfully.';

/* =====================================================================
   OPTIONAL - legacy 'dbo.Reports' table (0 rows, old Reports-only,
   referenced only by dormant uncompiled App_Code). Safe to drop but
   DEFERRED (non-destructive default). To remove manually:
       -- IF (SELECT COUNT(*) FROM dbo.Reports) = 0 DROP TABLE dbo.Reports;
   ===================================================================== */
