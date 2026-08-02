SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'AQOONHUB_DB'
    THROW 50000, 'Run this verification only against the intended AQOONHUB database.', 1;

DECLARE @Missing table (ObjectName sysname NOT NULL);
INSERT @Missing(ObjectName)
SELECT v.ObjectName
FROM (VALUES
 (N'Users'),(N'Roles'),(N'Permissions'),(N'Students'),(N'Guardians'),
 (N'Classes'),(N'Sections'),(N'AttendanceSessions'),(N'AttendanceRecords'),
 (N'Exams'),(N'ExamResults'),(N'FeeInvoices'),(N'FeePayments'),
 (N'PayrollPeriods'),(N'PayrollRecords'),(N'SavedReports'),(N'ReportAuditLogs')
) v(ObjectName)
WHERE OBJECT_ID(N'dbo.' + v.ObjectName, N'U') IS NULL;

IF EXISTS (SELECT 1 FROM @Missing)
BEGIN
    SELECT N'MISSING_TABLE' AS Finding, ObjectName FROM @Missing ORDER BY ObjectName;
    THROW 50001, 'Required tables are missing.', 1;
END;

DBCC CHECKCONSTRAINTS WITH ALL_CONSTRAINTS;
IF @@ROWCOUNT > 0
    THROW 50002, 'Constraint violations exist. Correct data before trusting constraints.', 1;

SELECT fk.name AS UntrustedForeignKey, OBJECT_SCHEMA_NAME(fk.parent_object_id) AS SchemaName,
       OBJECT_NAME(fk.parent_object_id) AS TableName
FROM sys.foreign_keys fk
WHERE fk.is_disabled = 1 OR fk.is_not_trusted = 1
ORDER BY TableName, UntrustedForeignKey;

SELECT Email, COUNT(*) AS DuplicateCount
FROM dbo.Users
WHERE Email IS NOT NULL AND LTRIM(RTRIM(Email)) <> N''
GROUP BY Email
HAVING COUNT(*) > 1;

SELECT N'INVALID_USER_ROLE' AS Finding, ur.UserRoleID AS RecordID
FROM dbo.UserRoles ur
LEFT JOIN dbo.Users u ON u.UserID = ur.UserID
LEFT JOIN dbo.Roles r ON r.RoleID = ur.RoleID
WHERE u.UserID IS NULL OR r.RoleID IS NULL;

SELECT N'INVALID_ROLE_PERMISSION' AS Finding, rp.RolePermissionID AS RecordID
FROM dbo.RolePermissions rp
LEFT JOIN dbo.Roles r ON r.RoleID = rp.RoleID
LEFT JOIN dbo.Permissions p ON p.PermissionID = rp.PermissionID
WHERE r.RoleID IS NULL OR p.PermissionID IS NULL;

SELECT N'Verification completed. Review every result set before deployment.' AS Result;
