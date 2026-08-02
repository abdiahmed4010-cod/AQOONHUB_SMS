/* AQOONHUB SMS Reports Stage 5 - rerunnable schema correction. */
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.ScheduledReports', N'U') IS NOT NULL
AND COL_LENGTH(N'dbo.ScheduledReports', N'Status') < 120
BEGIN
    ALTER TABLE dbo.ScheduledReports ALTER COLUMN Status nvarchar(60) NOT NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.SavedReports') AND name=N'IX_SavedReports_Owner_Visibility')
    CREATE INDEX IX_SavedReports_Owner_Visibility ON dbo.SavedReports(OwnerUserID, Visibility, IsActive) INCLUDE(ReportName, Category, LastRunAt);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.ScheduledReports') AND name=N'IX_ScheduledReports_Creator_Status')
    CREATE INDEX IX_ScheduledReports_Creator_Status ON dbo.ScheduledReports(CreatedBy, Status, NextRunAt) INCLUDE(SavedReportID, Frequency);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.ReportExports') AND name=N'IX_ReportExports_User_Date')
    CREATE INDEX IX_ReportExports_User_Date ON dbo.ReportExports(GeneratedBy, GeneratedAt DESC) INCLUDE(ReportKey, Category, Status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.ReportAuditLogs') AND name=N'IX_ReportAuditLogs_User_Date')
    CREATE INDEX IX_ReportAuditLogs_User_Date ON dbo.ReportAuditLogs(UserID, CreatedAt DESC) INCLUDE(Action, ReportKey, ResultStatus);

COMMIT TRANSACTION;
