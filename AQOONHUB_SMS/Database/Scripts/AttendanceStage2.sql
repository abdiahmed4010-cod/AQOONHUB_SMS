/* =====================================================================
   AQOONHUB_SMS - Attendance Stage 2
   Constraint-trust audit: the Stage 1 attendance FKs were created
   WITH NOCHECK (untrusted). The tables are empty, so it is safe to
   re-validate them WITH CHECK so the optimizer trusts them and new
   inserts are enforced. Idempotent and non-destructive.
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;

/* Only trust a constraint when no existing row violates it. WITH CHECK
   validates existing data; on empty tables this always succeeds. */
IF OBJECT_ID('dbo.FK_AS_Year','F')    IS NOT NULL ALTER TABLE dbo.AttendanceSessions WITH CHECK CHECK CONSTRAINT FK_AS_Year;
IF OBJECT_ID('dbo.FK_AS_Class','F')   IS NOT NULL ALTER TABLE dbo.AttendanceSessions WITH CHECK CHECK CONSTRAINT FK_AS_Class;
IF OBJECT_ID('dbo.FK_AS_Section','F') IS NOT NULL ALTER TABLE dbo.AttendanceSessions WITH CHECK CHECK CONSTRAINT FK_AS_Section;
IF OBJECT_ID('dbo.FK_AR_Session','F') IS NOT NULL ALTER TABLE dbo.AttendanceRecords  WITH CHECK CHECK CONSTRAINT FK_AR_Session;
IF OBJECT_ID('dbo.FK_AR_Student','F') IS NOT NULL ALTER TABLE dbo.AttendanceRecords  WITH CHECK CHECK CONSTRAINT FK_AR_Student;
GO

SELECT name AS ForeignKey, is_not_trusted AS IsNotTrusted
FROM sys.foreign_keys WHERE name IN ('FK_AS_Year','FK_AS_Class','FK_AS_Section','FK_AR_Session','FK_AR_Student');
GO

PRINT 'Attendance Stage 2 constraint-trust audit applied.';
