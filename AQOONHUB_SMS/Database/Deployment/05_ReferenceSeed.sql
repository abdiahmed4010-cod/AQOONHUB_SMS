SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

MERGE dbo.Roles AS target
USING (VALUES
 (N'Super Admin',N'Full system access with protected administrative controls'),
 (N'Admin',N'System administration without protected Super Admin operations'),
 (N'Academic',N'Academic management and reporting'),
 (N'Registrar',N'Student registration and academic records management'),
 (N'Accountant',N'Finance, fees and payroll reporting'),
 (N'Teacher',N'Assigned teaching, attendance and marks workflows'),
 (N'Parent',N'Own linked student information'),
 (N'Security',N'Audit and login-activity monitoring'),
 (N'Student',N'Own student information'),
 (N'HR',N'Staff management and payroll')
) AS source(RoleName,Description)
ON target.RoleName=source.RoleName
WHEN MATCHED THEN UPDATE SET Description=source.Description,IsActive=1,UpdatedAt=GETDATE()
WHEN NOT MATCHED THEN INSERT(RoleName,Description,IsActive,CreatedAt,UpdatedAt)
VALUES(source.RoleName,source.Description,1,GETDATE(),GETDATE());

MERGE dbo.Permissions AS target
USING (VALUES
 (N'Dashboard.View',N'Dashboard',N'View dashboard overview'),
 (N'Students.View',N'Students',N'View student records'),
 (N'Students.Create',N'Students',N'Create student records'),
 (N'Students.Edit',N'Students',N'Edit student records'),
 (N'Students.Delete',N'Students',N'Delete or deactivate student records'),
 (N'Teachers.View',N'Teachers',N'View teacher records'),
 (N'Teachers.Create',N'Teachers',N'Create teacher records'),
 (N'Teachers.Edit',N'Teachers',N'Edit teacher records'),
 (N'Teachers.Delete',N'Teachers',N'Delete or deactivate teacher records'),
 (N'Attendance.View',N'Attendance',N'View attendance records'),
 (N'Attendance.Manage',N'Attendance',N'Manage attendance records'),
 (N'Exams.View',N'Exams',N'View examination records and results'),
 (N'Exams.Manage',N'Exams',N'Manage examinations and results'),
 (N'Finance.View',N'Finance',N'View financial records and reports'),
 (N'Finance.Manage',N'Finance',N'Manage invoices, fees and payments'),
 (N'Reports.View',N'Reports',N'View and generate reports'),
 (N'Settings.Manage',N'Settings',N'Manage system settings'),
 (N'Notifications.Manage',N'Notifications',N'Send and manage notifications'),
 (N'Users.Manage',N'Users',N'Manage user accounts'),
 (N'Roles.Manage',N'Roles',N'Manage roles and assignments'),
 (N'Permissions.Manage',N'Permissions',N'Manage role permissions')
) AS source(PermissionName,Module,Description)
ON target.PermissionName=source.PermissionName
WHEN MATCHED THEN UPDATE SET Module=source.Module,Description=source.Description,IsActive=1
WHEN NOT MATCHED THEN INSERT(PermissionName,Module,Description,IsActive,CreatedAt)
VALUES(source.PermissionName,source.Module,source.Description,1,GETDATE());

IF NOT EXISTS(SELECT 1 FROM dbo.SchoolSettings)
    INSERT dbo.SchoolSettings(SchoolName,Currency,TimeZone,Language,UpdatedAt)
    VALUES(N'AQOONHUB School',N'USD',N'E. Africa Standard Time',N'English',GETDATE());

COMMIT TRANSACTION;
