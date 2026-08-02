# Database Deployment Package

This package coordinates the repository's incremental migrations. It deliberately contains no production records or credentials.

Apply to a backed-up database in this order:

1. `../Scripts/AcademicsModuleEnhancements.sql`
2. `../Scripts/AttendanceModule.sql`
3. `../Scripts/AttendanceStage2.sql`
4. `../Scripts/AttendanceStage4.sql`
5. `../Scripts/AttendanceStage5.sql`
6. `../Scripts/ExaminationsModule.sql`
7. `../Scripts/ExaminationsStage6.sql`
8. `../Scripts/FeeManagementModule.sql`
9. `../Scripts/ReportsModule.sql`
10. `../Scripts/ReportsStage5.sql`
11. `VerifyDeployment.sql`

The repository does not currently contain a complete base-schema script for a blank database. Obtain and review an approved schema-only export before new-environment deployment. Cleanup scripts and `Stage6_TestAccounts.ps1` are test utilities and are not deployment migrations.
