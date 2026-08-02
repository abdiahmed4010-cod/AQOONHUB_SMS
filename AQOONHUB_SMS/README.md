# AQOONHUB SMS

AQOONHUB SMS is an ASP.NET Web Forms school-management application covering admissions, students, guardians, staff, academics, attendance, examinations, finance, payroll, reporting, and system administration.

## Stack

- ASP.NET Web Forms, C#, .NET Framework 4.8
- SQL Server with direct ADO.NET
- Tailwind-based UI, vanilla JavaScript, Lucide icons, Chart.js

## Start here

1. Read `INSTALLATION_GUIDE.md` and `DATABASE_SETUP.md`.
2. Configure the `AQOONHUB_DB` connection string without committing credentials.
3. Build `AQOONHUB_SMS.csproj` in Visual Studio 2019+.
4. Run with IIS Express and open `/Modules/Authentication/Login.aspx`.

Operational and release details are in `SYSTEM_OVERVIEW.md`, `ROLE_PERMISSION_MATRIX.md`, `MODULE_STATUS.md`, `TESTING_REPORT.md`, `DEPLOYMENT_GUIDE.md`, and `DEMO_GUIDE.md`.

An ordered blank-database deployment package is available under `Database/Deployment`.

Never place production passwords, database backups, or private student data in source control.
