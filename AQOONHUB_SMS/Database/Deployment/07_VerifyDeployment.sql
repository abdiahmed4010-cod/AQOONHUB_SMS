SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Required table(ObjectName sysname NOT NULL);
INSERT @Required VALUES
(N'Users'),(N'Roles'),(N'Permissions'),(N'Students'),(N'Guardians'),(N'Staff'),
(N'Classes'),(N'Sections'),(N'AttendanceSessions'),(N'AttendanceRecords'),
(N'Exams'),(N'ExamResults'),(N'FeeInvoices'),(N'FeePayments'),
(N'PayrollPeriods'),(N'PayrollRecords'),(N'SavedReports'),(N'ReportAuditLogs');

IF EXISTS(SELECT 1 FROM @Required WHERE OBJECT_ID(N'dbo.'+ObjectName,N'U') IS NULL)
BEGIN
    SELECT ObjectName AS MissingTable FROM @Required WHERE OBJECT_ID(N'dbo.'+ObjectName,N'U') IS NULL;
    THROW 50001,'Required deployment tables are missing.',1;
END;

IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE is_disabled=1 OR is_not_trusted=1)
BEGIN
    SELECT name,OBJECT_SCHEMA_NAME(parent_object_id) AS ParentSchema,
           OBJECT_NAME(parent_object_id) AS ParentTable,is_disabled,is_not_trusted
    FROM sys.foreign_keys WHERE is_disabled=1 OR is_not_trusted=1;
    THROW 50002,'Disabled or untrusted foreign keys remain.',1;
END;

IF NOT EXISTS(SELECT 1 FROM dbo.Roles WHERE RoleName=N'Super Admin' AND IsActive=1)
    THROW 50003,'Super Admin reference role is missing.',1;
IF (SELECT COUNT(*) FROM dbo.Permissions WHERE IsActive=1)<21
    THROW 50004,'Required permission reference rows are missing.',1;

DBCC CHECKCONSTRAINTS WITH ALL_CONSTRAINTS;
SELECT DB_NAME() AS DatabaseName,
 (SELECT COUNT(*) FROM sys.tables WHERE is_ms_shipped=0) AS TableCount,
 (SELECT COUNT(*) FROM sys.foreign_keys) AS ForeignKeyCount,
 (SELECT COUNT(*) FROM sys.indexes WHERE index_id>0 AND is_hypothetical=0) AS IndexCount,
 (SELECT COUNT(*) FROM sys.procedures WHERE is_ms_shipped=0) AS ProcedureCount,
 (SELECT COUNT(*) FROM dbo.Roles WHERE IsActive=1) AS ActiveRoleCount,
 (SELECT COUNT(*) FROM dbo.Permissions WHERE IsActive=1) AS ActivePermissionCount;
