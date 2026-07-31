/* =====================================================================
   AQOONHUB_SMS - Attendance Stage 4
   Import-audit table. Idempotent, non-destructive, FK-protected.
   Stores batch metadata + a SHA-256 file hash for duplicate detection.
   The uploaded CSV content itself is NOT stored.
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

IF OBJECT_ID('dbo.AttendanceImportBatches','U') IS NULL
CREATE TABLE dbo.AttendanceImportBatches (
    AttendanceImportBatchID int IDENTITY(1,1) PRIMARY KEY,
    OriginalFileName  nvarchar(260) NULL,
    StoredFileName    nvarchar(260) NULL,
    FileHash          char(64)      NULL,        -- SHA-256 hex of the file bytes
    AcademicYearID    int           NOT NULL,
    ClassID           int           NOT NULL,
    SectionID         int           NOT NULL,
    SubjectID         int           NULL,
    SessionType       nvarchar(20)  NOT NULL,
    ImportStatus      nvarchar(20)  NOT NULL,     -- Draft / Submitted / Failed
    TotalRows         int           NOT NULL CONSTRAINT DF_AIB_Total  DEFAULT(0),
    ValidRows         int           NOT NULL CONSTRAINT DF_AIB_Valid  DEFAULT(0),
    ErrorRows         int           NOT NULL CONSTRAINT DF_AIB_Error  DEFAULT(0),
    ImportedSessions  int           NOT NULL CONSTRAINT DF_AIB_Sess   DEFAULT(0),
    ImportedRecords   int           NOT NULL CONSTRAINT DF_AIB_Recs   DEFAULT(0),
    ImportedBy        int           NULL,
    ImportedAt        datetime      NULL,
    CreatedAt         datetime      NOT NULL CONSTRAINT DF_AIB_Cre    DEFAULT(GETDATE())
);
GO

IF OBJECT_ID('dbo.FK_AIB_Year','F')    IS NULL ALTER TABLE dbo.AttendanceImportBatches WITH CHECK ADD CONSTRAINT FK_AIB_Year    FOREIGN KEY (AcademicYearID) REFERENCES dbo.AcademicYears(AcademicYearID);
IF OBJECT_ID('dbo.FK_AIB_Class','F')   IS NULL ALTER TABLE dbo.AttendanceImportBatches WITH CHECK ADD CONSTRAINT FK_AIB_Class   FOREIGN KEY (ClassID)   REFERENCES dbo.Classes(ClassID);
IF OBJECT_ID('dbo.FK_AIB_Section','F') IS NULL ALTER TABLE dbo.AttendanceImportBatches WITH CHECK ADD CONSTRAINT FK_AIB_Section FOREIGN KEY (SectionID) REFERENCES dbo.Sections(SectionID);
GO

/* Fast lookup for duplicate-file detection by hash + scope. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_AIB_HashScope' AND object_id=OBJECT_ID('dbo.AttendanceImportBatches'))
    CREATE INDEX IX_AIB_HashScope ON dbo.AttendanceImportBatches(FileHash, AcademicYearID, ClassID, SectionID) WHERE FileHash IS NOT NULL;
GO

PRINT 'Attendance Stage 4 import-audit schema applied.';
