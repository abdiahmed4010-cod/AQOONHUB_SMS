# System Overview

## Architecture

The application is a .NET Framework 4.8 Web Forms site. Pages and code-behind files live under `Modules`, shared layout is in `MasterPages/MainMaster.Master`, shared business/data-access code is under `App_Code`, and static resources are under `Assets`. Module repositories use parameterized ADO.NET against SQL Server.

## Canonical routes

| Area | Main route | Principal data | Supported scope |
|---|---|---|---|
| Authentication | `/Modules/Authentication/Login.aspx` | Users, LoginActivity | Login/logout |
| Dashboard | `/Modules/Dashboard/Dashboard.aspx` | Aggregate operational tables | View |
| Students | `/Modules/Students/Students.aspx` | Students, Classes, Sections | View/create/edit/status and related workflows by role |
| Guardians | `/Modules/Parents/Parents.aspx` | Guardians, StudentGuardians | View/create/edit/assign by role |
| Teachers and Staff | `/Modules/Staff/Staff.aspx` | Staff, Users | View/create/edit by role |
| Academics | `/Modules/Academic/Academics.aspx` | AcademicYears, Terms, Subjects, Timetable | View/manage by academic role |
| Classes and Sections | `/Modules/Academic/ClassesSections.aspx` | Classes, Sections | View/manage by academic role |
| Attendance | `/Modules/Attendance/Attendance.aspx` | AttendanceSessions, AttendanceRecords, AttendanceSettings | View/mark/import/report by assigned permission |
| Examinations | `/Modules/Examinations/Examinations.aspx` | Exams, ExamSchedules, ExamResults, GradingScale | View/manage/marks/publish by role |
| Finance and Fees | `/Modules/Finance/FeeManagement.aspx` | FeeCategories, FeeStructures, FeeInvoices, FeePayments | View/manage/payment/report by finance role |
| Payroll | `/Modules/Payroll/Payroll.aspx` | PayrollPeriods, PayrollRecords, PayrollAdjustments | View/process/report by payroll role |
| Reports | `/Modules/Reports/Reports.aspx` | Operational tables plus SavedReports, ScheduledReports, ReportExports, ReportAuditLogs | Role-filtered view/export/save/schedule |
| Notifications | No canonical UI route | Notifications | Data/model support exists; UI module is not implemented |
| Users and Roles | `/Modules/Administration/Users.aspx` | Users, Roles, Permissions, UserRoles, RolePermissions | Admin management |
| Audit Log | `/Modules/Administration/AuditLog.aspx` | AuditLog | Admin/Security view/export |
| Login Activity | `/Modules/Administration/LoginActivity.aspx` | LoginActivity, Users | Admin/Security view/export |
| Settings | `/Modules/Settings/Settings.aspx` | SchoolSettings | Admin view/update |

The sidebar is convenience navigation only. Each sensitive page must retain its server-side authorization gate.
