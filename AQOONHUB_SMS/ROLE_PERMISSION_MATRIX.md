# Role and Permission Matrix

This matrix records implemented authorization intent. A role being visible in the UI does not replace server-side checks.

| Module | Super Admin | Admin | Academic | Registrar | Accountant | Teacher | Parent | Security |
|---|---|---|---|---|---|---|---|---|
| Dashboard | View | View | View | View | View | View | View | View |
| Students/Guardians | Full | Full | Manage | Manage | View where exposed | Assigned/view | Own linked students | No |
| Academics/Classes | Full | Full | Manage | View/manage where gated | No | Assigned/view | No | No |
| Attendance | Full | Full | Manage | View | No | Assigned mark/view | Own linked students | No |
| Examinations | Full | Full | Manage | View | No | Assigned marks/view | Published linked results | No |
| Finance | Full | Full | No | No | Manage/export | No | No | No |
| Payroll | Full | Full | No | No | Manage/export | No | No | No |
| Reports | All | All except protected Super Admin operations | Academic categories | Student/guardian categories | Finance/payroll categories | Restricted assigned-scope categories | No management reports | No |
| Users/Roles/Settings | Full, protected | Manage except protected Super Admin controls | No | No | No | No | No | No |
| Audit/Login Activity | View/export | View/export | No | No | No | No | No | View/export |

Delete/approve/export capability varies by page and is denied unless its code-behind/repository gate permits it. Super Admin self-deactivation, last-active-Super-Admin deactivation, and Super Admin permission editing remain blocked server-side.

Authenticated direct-URL runtime checks covered the 15 representative routes for all eight roles. Super Admin and Admin were allowed throughout; Academic and Registrar were limited to academic/student/report routes; Accountant to finance/payroll/report routes; Teacher to assigned academic/student/report routes; Parent had no SYSTEM or management-report access; Security had Audit/Login Activity but no Users/Settings access. Two denial redirects that targeted nonexistent pages were corrected to the canonical Dashboard denial route. Browser UI confirmation was completed for Super Admin; the remaining seven browser sessions require rerun after the local browser-controller/Windows SSPI environment is restored.
