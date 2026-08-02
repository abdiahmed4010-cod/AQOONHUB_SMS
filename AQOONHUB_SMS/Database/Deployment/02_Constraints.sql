SET XACT_ABORT ON;
ALTER TABLE [dbo].[Admissions]  WITH CHECK ADD FOREIGN KEY([ApplyingForClassID])
REFERENCES [dbo].[Classes] ([ClassID])
ALTER TABLE [dbo].[Admissions]  WITH CHECK ADD FOREIGN KEY([ReviewedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[Admissions]  WITH CHECK ADD  CONSTRAINT [FK_Admissions_Guardians] FOREIGN KEY([GuardianID])
REFERENCES [dbo].[Guardians] ([GuardianID])
ALTER TABLE [dbo].[Admissions] CHECK CONSTRAINT [FK_Admissions_Guardians]
ALTER TABLE [dbo].[Admissions]  WITH CHECK ADD  CONSTRAINT [FK_Admissions_Students] FOREIGN KEY([StudentID])
REFERENCES [dbo].[Students] ([StudentID])
ALTER TABLE [dbo].[Admissions] CHECK CONSTRAINT [FK_Admissions_Students]
ALTER TABLE [dbo].[Announcements]  WITH CHECK ADD FOREIGN KEY([AuthorID])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[Announcements]  WITH CHECK ADD FOREIGN KEY([TargetClassID])
REFERENCES [dbo].[Classes] ([ClassID])
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD FOREIGN KEY([MarkedBy])
REFERENCES [dbo].[Staff] ([StaffID])
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD FOREIGN KEY([SectionID])
REFERENCES [dbo].[Sections] ([SectionID])
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD FOREIGN KEY([StudentID])
REFERENCES [dbo].[Students] ([StudentID])
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD FOREIGN KEY([SubjectID])
REFERENCES [dbo].[Subjects] ([SubjectID])
ALTER TABLE [dbo].[AttendanceAlerts]  WITH CHECK ADD  CONSTRAINT [FK_AA_Student] FOREIGN KEY([StudentID])
REFERENCES [dbo].[Students] ([StudentID])
ALTER TABLE [dbo].[AttendanceAlerts] CHECK CONSTRAINT [FK_AA_Student]
ALTER TABLE [dbo].[AttendanceImportBatches]  WITH CHECK ADD  CONSTRAINT [FK_AIB_Class] FOREIGN KEY([ClassID])
REFERENCES [dbo].[Classes] ([ClassID])
ALTER TABLE [dbo].[AttendanceImportBatches] CHECK CONSTRAINT [FK_AIB_Class]
ALTER TABLE [dbo].[AttendanceImportBatches]  WITH CHECK ADD  CONSTRAINT [FK_AIB_Section] FOREIGN KEY([SectionID])
REFERENCES [dbo].[Sections] ([SectionID])
ALTER TABLE [dbo].[AttendanceImportBatches] CHECK CONSTRAINT [FK_AIB_Section]
ALTER TABLE [dbo].[AttendanceImportBatches]  WITH CHECK ADD  CONSTRAINT [FK_AIB_Year] FOREIGN KEY([AcademicYearID])
REFERENCES [dbo].[AcademicYears] ([AcademicYearID])
ALTER TABLE [dbo].[AttendanceImportBatches] CHECK CONSTRAINT [FK_AIB_Year]
ALTER TABLE [dbo].[AttendanceRecords]  WITH CHECK ADD  CONSTRAINT [FK_AR_Session] FOREIGN KEY([AttendanceSessionID])
REFERENCES [dbo].[AttendanceSessions] ([AttendanceSessionID])
ALTER TABLE [dbo].[AttendanceRecords] CHECK CONSTRAINT [FK_AR_Session]
ALTER TABLE [dbo].[AttendanceRecords]  WITH CHECK ADD  CONSTRAINT [FK_AR_Student] FOREIGN KEY([StudentID])
REFERENCES [dbo].[Students] ([StudentID])
ALTER TABLE [dbo].[AttendanceRecords] CHECK CONSTRAINT [FK_AR_Student]
ALTER TABLE [dbo].[AttendanceSessions]  WITH CHECK ADD  CONSTRAINT [FK_AS_Class] FOREIGN KEY([ClassID])
REFERENCES [dbo].[Classes] ([ClassID])
ALTER TABLE [dbo].[AttendanceSessions] CHECK CONSTRAINT [FK_AS_Class]
ALTER TABLE [dbo].[AttendanceSessions]  WITH CHECK ADD  CONSTRAINT [FK_AS_Section] FOREIGN KEY([SectionID])
REFERENCES [dbo].[Sections] ([SectionID])
ALTER TABLE [dbo].[AttendanceSessions] CHECK CONSTRAINT [FK_AS_Section]
ALTER TABLE [dbo].[AttendanceSessions]  WITH CHECK ADD  CONSTRAINT [FK_AS_Year] FOREIGN KEY([AcademicYearID])
REFERENCES [dbo].[AcademicYears] ([AcademicYearID])
ALTER TABLE [dbo].[AttendanceSessions] CHECK CONSTRAINT [FK_AS_Year]
ALTER TABLE [dbo].[AuditLog]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[ClassFeeStructures]  WITH CHECK ADD  CONSTRAINT [FK_ClassFeeStructures_AcademicYears] FOREIGN KEY([AcademicYearID])
REFERENCES [dbo].[AcademicYears] ([AcademicYearID])
ALTER TABLE [dbo].[ClassFeeStructures] CHECK CONSTRAINT [FK_ClassFeeStructures_AcademicYears]
ALTER TABLE [dbo].[ClassFeeStructures]  WITH CHECK ADD  CONSTRAINT [FK_ClassFeeStructures_Categories] FOREIGN KEY([FeeCategoryID])
REFERENCES [dbo].[FeeCategories] ([FeeCategoryID])
ALTER TABLE [dbo].[ClassFeeStructures] CHECK CONSTRAINT [FK_ClassFeeStructures_Categories]
ALTER TABLE [dbo].[ClassFeeStructures]  WITH CHECK ADD  CONSTRAINT [FK_ClassFeeStructures_Classes] FOREIGN KEY([ClassID])
REFERENCES [dbo].[Classes] ([ClassID])
ALTER TABLE [dbo].[ClassFeeStructures] CHECK CONSTRAINT [FK_ClassFeeStructures_Classes]
ALTER TABLE [dbo].[ClassFeeStructures]  WITH CHECK ADD  CONSTRAINT [FK_ClassFeeStructures_Sections] FOREIGN KEY([SectionID])
REFERENCES [dbo].[Sections] ([SectionID])
ALTER TABLE [dbo].[ClassFeeStructures] CHECK CONSTRAINT [FK_ClassFeeStructures_Sections]
ALTER TABLE [dbo].[ClassSubjectTeachers]  WITH CHECK ADD FOREIGN KEY([AcademicYearID])
REFERENCES [dbo].[AcademicYears] ([AcademicYearID])
ALTER TABLE [dbo].[ClassSubjectTeachers]  WITH CHECK ADD FOREIGN KEY([SectionID])
REFERENCES [dbo].[Sections] ([SectionID])
ALTER TABLE [dbo].[ClassSubjectTeachers]  WITH CHECK ADD FOREIGN KEY([StaffID])
REFERENCES [dbo].[Staff] ([StaffID])
ALTER TABLE [dbo].[ClassSubjectTeachers]  WITH CHECK ADD FOREIGN KEY([SubjectID])
REFERENCES [dbo].[Subjects] ([SubjectID])
ALTER TABLE [dbo].[Documents]  WITH CHECK ADD FOREIGN KEY([UploadedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[Events]  WITH CHECK ADD FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[ExamClasses]  WITH CHECK ADD  CONSTRAINT [FK_ExamClasses_Exam] FOREIGN KEY([ExamID])
REFERENCES [dbo].[Exams] ([ExamID])
ALTER TABLE [dbo].[ExamClasses] CHECK CONSTRAINT [FK_ExamClasses_Exam]
ALTER TABLE [dbo].[ExamResults]  WITH CHECK ADD FOREIGN KEY([EnteredBy])
REFERENCES [dbo].[Staff] ([StaffID])
ALTER TABLE [dbo].[ExamResults]  WITH CHECK ADD FOREIGN KEY([ExamID])
REFERENCES [dbo].[Exams] ([ExamID])
ALTER TABLE [dbo].[ExamResults]  WITH CHECK ADD FOREIGN KEY([StudentID])
REFERENCES [dbo].[Students] ([StudentID])
ALTER TABLE [dbo].[ExamResults]  WITH CHECK ADD FOREIGN KEY([SubjectID])
REFERENCES [dbo].[Subjects] ([SubjectID])
ALTER TABLE [dbo].[Exams]  WITH CHECK ADD FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[Exams]  WITH CHECK ADD FOREIGN KEY([TermID])
REFERENCES [dbo].[Terms] ([TermID])
ALTER TABLE [dbo].[ExamSchedules]  WITH CHECK ADD  CONSTRAINT [FK_ExamSchedules_Exam] FOREIGN KEY([ExamID])
REFERENCES [dbo].[Exams] ([ExamID])
ALTER TABLE [dbo].[ExamSchedules] CHECK CONSTRAINT [FK_ExamSchedules_Exam]
ALTER TABLE [dbo].[ExamSchedules]  WITH CHECK ADD  CONSTRAINT [FK_ExamSchedules_Room] FOREIGN KEY([ExamRoomID])
REFERENCES [dbo].[ExamRooms] ([ExamRoomID])
ALTER TABLE [dbo].[ExamSchedules] CHECK CONSTRAINT [FK_ExamSchedules_Room]
ALTER TABLE [dbo].[ExamSubjects]  WITH CHECK ADD  CONSTRAINT [FK_ExamSubjects_Exam] FOREIGN KEY([ExamID])
REFERENCES [dbo].[Exams] ([ExamID])
ALTER TABLE [dbo].[ExamSubjects] CHECK CONSTRAINT [FK_ExamSubjects_Exam]
ALTER TABLE [dbo].[FeeInvoiceItems]  WITH CHECK ADD  CONSTRAINT [FK_FeeInvoiceItems_Categories] FOREIGN KEY([FeeCategoryID])
REFERENCES [dbo].[FeeCategories] ([FeeCategoryID])
ALTER TABLE [dbo].[FeeInvoiceItems] CHECK CONSTRAINT [FK_FeeInvoiceItems_Categories]
ALTER TABLE [dbo].[FeeInvoiceItems]  WITH CHECK ADD  CONSTRAINT [FK_FeeInvoiceItems_Invoices] FOREIGN KEY([InvoiceID])
REFERENCES [dbo].[FeeInvoices] ([InvoiceID])
ALTER TABLE [dbo].[FeeInvoiceItems] CHECK CONSTRAINT [FK_FeeInvoiceItems_Invoices]
ALTER TABLE [dbo].[FeeInvoices]  WITH CHECK ADD  CONSTRAINT [FK_FeeInvoices_AcademicYears] FOREIGN KEY([AcademicYearID])
REFERENCES [dbo].[AcademicYears] ([AcademicYearID])
ALTER TABLE [dbo].[FeeInvoices] CHECK CONSTRAINT [FK_FeeInvoices_AcademicYears]
ALTER TABLE [dbo].[FeeInvoices]  WITH CHECK ADD  CONSTRAINT [FK_FeeInvoices_Students] FOREIGN KEY([StudentID])
REFERENCES [dbo].[Students] ([StudentID])
ALTER TABLE [dbo].[FeeInvoices] CHECK CONSTRAINT [FK_FeeInvoices_Students]
ALTER TABLE [dbo].[FeeInvoices]  WITH CHECK ADD  CONSTRAINT [FK_FeeInvoices_Users] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[FeeInvoices] CHECK CONSTRAINT [FK_FeeInvoices_Users]
ALTER TABLE [dbo].[FeePayments]  WITH CHECK ADD  CONSTRAINT [FK_FeePayments_Invoices] FOREIGN KEY([InvoiceID])
REFERENCES [dbo].[FeeInvoices] ([InvoiceID])
ALTER TABLE [dbo].[FeePayments] CHECK CONSTRAINT [FK_FeePayments_Invoices]
ALTER TABLE [dbo].[FeePayments]  WITH CHECK ADD  CONSTRAINT [FK_FeePayments_Students] FOREIGN KEY([StudentID])
REFERENCES [dbo].[Students] ([StudentID])
ALTER TABLE [dbo].[FeePayments] CHECK CONSTRAINT [FK_FeePayments_Students]
ALTER TABLE [dbo].[FeePayments]  WITH CHECK ADD  CONSTRAINT [FK_FeePayments_Users] FOREIGN KEY([ReceivedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[FeePayments] CHECK CONSTRAINT [FK_FeePayments_Users]
ALTER TABLE [dbo].[FeeStructures]  WITH CHECK ADD FOREIGN KEY([AcademicYearID])
REFERENCES [dbo].[AcademicYears] ([AcademicYearID])
ALTER TABLE [dbo].[FeeStructures]  WITH CHECK ADD FOREIGN KEY([ClassID])
REFERENCES [dbo].[Classes] ([ClassID])
ALTER TABLE [dbo].[GradingScale]  WITH CHECK ADD FOREIGN KEY([AcademicYearID])
REFERENCES [dbo].[AcademicYears] ([AcademicYearID])
ALTER TABLE [dbo].[Guardians]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[InvoiceItems]  WITH CHECK ADD FOREIGN KEY([FeeStructureID])
REFERENCES [dbo].[FeeStructures] ([FeeStructureID])
ALTER TABLE [dbo].[InvoiceItems]  WITH CHECK ADD FOREIGN KEY([InvoiceID])
REFERENCES [dbo].[Invoices] ([InvoiceID])
ALTER TABLE [dbo].[Invoices]  WITH CHECK ADD FOREIGN KEY([AcademicYearID])
REFERENCES [dbo].[AcademicYears] ([AcademicYearID])
ALTER TABLE [dbo].[Invoices]  WITH CHECK ADD FOREIGN KEY([GeneratedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[Invoices]  WITH CHECK ADD FOREIGN KEY([StudentID])
REFERENCES [dbo].[Students] ([StudentID])
ALTER TABLE [dbo].[Invoices]  WITH CHECK ADD FOREIGN KEY([TermID])
REFERENCES [dbo].[Terms] ([TermID])
ALTER TABLE [dbo].[LeaveRequests]  WITH CHECK ADD FOREIGN KEY([ApprovedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[LeaveRequests]  WITH CHECK ADD FOREIGN KEY([StaffID])
REFERENCES [dbo].[Staff] ([StaffID])
ALTER TABLE [dbo].[LoginActivity]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD  CONSTRAINT [FK_Notifications_Users_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[Notifications] CHECK CONSTRAINT [FK_Notifications_Users_CreatedBy]
ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD  CONSTRAINT [FK_Notifications_Users_UserID] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[Notifications] CHECK CONSTRAINT [FK_Notifications_Users_UserID]
ALTER TABLE [dbo].[Payments]  WITH CHECK ADD FOREIGN KEY([InvoiceID])
REFERENCES [dbo].[Invoices] ([InvoiceID])
ALTER TABLE [dbo].[Payments]  WITH CHECK ADD FOREIGN KEY([ReceivedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[PayrollAdjustments]  WITH CHECK ADD  CONSTRAINT [FK_PayrollAdjustments_PayrollRecords] FOREIGN KEY([PayrollRecordID])
REFERENCES [dbo].[PayrollRecords] ([PayrollRecordID])
ALTER TABLE [dbo].[PayrollAdjustments] CHECK CONSTRAINT [FK_PayrollAdjustments_PayrollRecords]
ALTER TABLE [dbo].[PayrollRecords]  WITH CHECK ADD  CONSTRAINT [FK_PayrollRecords_PayrollPeriods] FOREIGN KEY([PayrollPeriodID])
REFERENCES [dbo].[PayrollPeriods] ([PayrollPeriodID])
ALTER TABLE [dbo].[PayrollRecords] CHECK CONSTRAINT [FK_PayrollRecords_PayrollPeriods]
ALTER TABLE [dbo].[PayrollRecords]  WITH CHECK ADD  CONSTRAINT [FK_PayrollRecords_Staff] FOREIGN KEY([StaffID])
REFERENCES [dbo].[Staff] ([StaffID])
ALTER TABLE [dbo].[PayrollRecords] CHECK CONSTRAINT [FK_PayrollRecords_Staff]
ALTER TABLE [dbo].[ReportAuditLogs]  WITH CHECK ADD  CONSTRAINT [FK_RAL_User] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[ReportAuditLogs] CHECK CONSTRAINT [FK_RAL_User]
ALTER TABLE [dbo].[ReportExports]  WITH CHECK ADD  CONSTRAINT [FK_RE_By] FOREIGN KEY([GeneratedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[ReportExports] CHECK CONSTRAINT [FK_RE_By]
ALTER TABLE [dbo].[Reports]  WITH CHECK ADD  CONSTRAINT [FK_Reports_Users_GeneratedBy] FOREIGN KEY([GeneratedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[Reports] CHECK CONSTRAINT [FK_Reports_Users_GeneratedBy]
ALTER TABLE [dbo].[ResultPublications]  WITH CHECK ADD  CONSTRAINT [FK_ResultPub_Exam] FOREIGN KEY([ExamID])
REFERENCES [dbo].[Exams] ([ExamID])
ALTER TABLE [dbo].[ResultPublications] CHECK CONSTRAINT [FK_ResultPub_Exam]
ALTER TABLE [dbo].[RolePermissions]  WITH CHECK ADD  CONSTRAINT [FK_RolePermissions_PermissionID] FOREIGN KEY([PermissionID])
REFERENCES [dbo].[Permissions] ([PermissionID])
ON UPDATE CASCADE
ON DELETE CASCADE
ALTER TABLE [dbo].[RolePermissions] CHECK CONSTRAINT [FK_RolePermissions_PermissionID]
ALTER TABLE [dbo].[RolePermissions]  WITH CHECK ADD  CONSTRAINT [FK_RolePermissions_RoleID] FOREIGN KEY([RoleID])
REFERENCES [dbo].[Roles] ([RoleID])
ON UPDATE CASCADE
ON DELETE CASCADE
ALTER TABLE [dbo].[RolePermissions] CHECK CONSTRAINT [FK_RolePermissions_RoleID]
ALTER TABLE [dbo].[SavedReports]  WITH CHECK ADD  CONSTRAINT [FK_SR_Owner] FOREIGN KEY([OwnerUserID])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[SavedReports] CHECK CONSTRAINT [FK_SR_Owner]
ALTER TABLE [dbo].[ScheduledReports]  WITH CHECK ADD  CONSTRAINT [FK_SCH_By] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[ScheduledReports] CHECK CONSTRAINT [FK_SCH_By]
ALTER TABLE [dbo].[ScheduledReports]  WITH CHECK ADD  CONSTRAINT [FK_SCH_Saved] FOREIGN KEY([SavedReportID])
REFERENCES [dbo].[SavedReports] ([SavedReportID])
ALTER TABLE [dbo].[ScheduledReports] CHECK CONSTRAINT [FK_SCH_Saved]
ALTER TABLE [dbo].[SchoolSettings]  WITH CHECK ADD FOREIGN KEY([CurrentAcademicYearID])
REFERENCES [dbo].[AcademicYears] ([AcademicYearID])
ALTER TABLE [dbo].[SchoolSettings]  WITH CHECK ADD FOREIGN KEY([CurrentTermID])
REFERENCES [dbo].[Terms] ([TermID])
ALTER TABLE [dbo].[Sections]  WITH CHECK ADD FOREIGN KEY([ClassID])
REFERENCES [dbo].[Classes] ([ClassID])
ALTER TABLE [dbo].[Sections]  WITH CHECK ADD  CONSTRAINT [FK_Sections_Staff] FOREIGN KEY([StaffID])
REFERENCES [dbo].[Staff] ([StaffID])
ALTER TABLE [dbo].[Sections] CHECK CONSTRAINT [FK_Sections_Staff]
ALTER TABLE [dbo].[Staff]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[StaffSalaryStructures]  WITH CHECK ADD  CONSTRAINT [FK_StaffSalaryStructures_Staff] FOREIGN KEY([StaffID])
REFERENCES [dbo].[Staff] ([StaffID])
ALTER TABLE [dbo].[StaffSalaryStructures] CHECK CONSTRAINT [FK_StaffSalaryStructures_Staff]
ALTER TABLE [dbo].[StudentExamSummaries]  WITH CHECK ADD  CONSTRAINT [FK_SES_Exam] FOREIGN KEY([ExamID])
REFERENCES [dbo].[Exams] ([ExamID])
ALTER TABLE [dbo].[StudentExamSummaries] CHECK CONSTRAINT [FK_SES_Exam]
ALTER TABLE [dbo].[StudentExamSummaries]  WITH CHECK ADD  CONSTRAINT [FK_SES_Student] FOREIGN KEY([StudentID])
REFERENCES [dbo].[Students] ([StudentID])
ALTER TABLE [dbo].[StudentExamSummaries] CHECK CONSTRAINT [FK_SES_Student]
ALTER TABLE [dbo].[StudentGuardians]  WITH CHECK ADD FOREIGN KEY([GuardianID])
REFERENCES [dbo].[Guardians] ([GuardianID])
ALTER TABLE [dbo].[StudentGuardians]  WITH CHECK ADD FOREIGN KEY([StudentID])
REFERENCES [dbo].[Students] ([StudentID])
ALTER TABLE [dbo].[StudentPromotions]  WITH CHECK ADD  CONSTRAINT [FK_StudentPromotions_FromYear] FOREIGN KEY([FromAcademicYearID])
REFERENCES [dbo].[AcademicYears] ([AcademicYearID])
ALTER TABLE [dbo].[StudentPromotions] CHECK CONSTRAINT [FK_StudentPromotions_FromYear]
ALTER TABLE [dbo].[StudentPromotions]  WITH CHECK ADD  CONSTRAINT [FK_StudentPromotions_Student] FOREIGN KEY([StudentID])
REFERENCES [dbo].[Students] ([StudentID])
ALTER TABLE [dbo].[StudentPromotions] CHECK CONSTRAINT [FK_StudentPromotions_Student]
ALTER TABLE [dbo].[StudentPromotions]  WITH CHECK ADD  CONSTRAINT [FK_StudentPromotions_ToYear] FOREIGN KEY([ToAcademicYearID])
REFERENCES [dbo].[AcademicYears] ([AcademicYearID])
ALTER TABLE [dbo].[StudentPromotions] CHECK CONSTRAINT [FK_StudentPromotions_ToYear]
ALTER TABLE [dbo].[Students]  WITH CHECK ADD FOREIGN KEY([AcademicYearID])
REFERENCES [dbo].[AcademicYears] ([AcademicYearID])
ALTER TABLE [dbo].[Students]  WITH CHECK ADD FOREIGN KEY([GuardianID])
REFERENCES [dbo].[Guardians] ([GuardianID])
ALTER TABLE [dbo].[Students]  WITH CHECK ADD FOREIGN KEY([SectionID])
REFERENCES [dbo].[Sections] ([SectionID])
ALTER TABLE [dbo].[StudentTransfers]  WITH CHECK ADD  CONSTRAINT [FK_StudentTransfers_FromAcademicYears] FOREIGN KEY([FromAcademicYearID])
REFERENCES [dbo].[AcademicYears] ([AcademicYearID])
ALTER TABLE [dbo].[StudentTransfers] CHECK CONSTRAINT [FK_StudentTransfers_FromAcademicYears]
ALTER TABLE [dbo].[StudentTransfers]  WITH CHECK ADD  CONSTRAINT [FK_StudentTransfers_FromSections] FOREIGN KEY([FromSectionID])
REFERENCES [dbo].[Sections] ([SectionID])
ALTER TABLE [dbo].[StudentTransfers] CHECK CONSTRAINT [FK_StudentTransfers_FromSections]
ALTER TABLE [dbo].[StudentTransfers]  WITH CHECK ADD  CONSTRAINT [FK_StudentTransfers_ReturnAcademicYears] FOREIGN KEY([ReturnAcademicYearID])
REFERENCES [dbo].[AcademicYears] ([AcademicYearID])
ALTER TABLE [dbo].[StudentTransfers] CHECK CONSTRAINT [FK_StudentTransfers_ReturnAcademicYears]
ALTER TABLE [dbo].[StudentTransfers]  WITH CHECK ADD  CONSTRAINT [FK_StudentTransfers_ReturnSections] FOREIGN KEY([ReturnSectionID])
REFERENCES [dbo].[Sections] ([SectionID])
ALTER TABLE [dbo].[StudentTransfers] CHECK CONSTRAINT [FK_StudentTransfers_ReturnSections]
ALTER TABLE [dbo].[StudentTransfers]  WITH CHECK ADD  CONSTRAINT [FK_StudentTransfers_Students] FOREIGN KEY([StudentID])
REFERENCES [dbo].[Students] ([StudentID])
ALTER TABLE [dbo].[StudentTransfers] CHECK CONSTRAINT [FK_StudentTransfers_Students]
ALTER TABLE [dbo].[Terms]  WITH CHECK ADD FOREIGN KEY([AcademicYearID])
REFERENCES [dbo].[AcademicYears] ([AcademicYearID])
ALTER TABLE [dbo].[Timetable]  WITH CHECK ADD FOREIGN KEY([AcademicYearID])
REFERENCES [dbo].[AcademicYears] ([AcademicYearID])
ALTER TABLE [dbo].[Timetable]  WITH CHECK ADD FOREIGN KEY([SectionID])
REFERENCES [dbo].[Sections] ([SectionID])
ALTER TABLE [dbo].[Timetable]  WITH CHECK ADD FOREIGN KEY([StaffID])
REFERENCES [dbo].[Staff] ([StaffID])
ALTER TABLE [dbo].[Timetable]  WITH CHECK ADD FOREIGN KEY([SubjectID])
REFERENCES [dbo].[Subjects] ([SubjectID])
ALTER TABLE [dbo].[Timetable]  WITH CHECK ADD  CONSTRAINT [FK_Timetable_Term] FOREIGN KEY([TermID])
REFERENCES [dbo].[Terms] ([TermID])
ALTER TABLE [dbo].[Timetable] CHECK CONSTRAINT [FK_Timetable_Term]
ALTER TABLE [dbo].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_UserRoles_AssignedBy] FOREIGN KEY([AssignedBy])
REFERENCES [dbo].[Users] ([UserID])
ALTER TABLE [dbo].[UserRoles] CHECK CONSTRAINT [FK_UserRoles_AssignedBy]
ALTER TABLE [dbo].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_UserRoles_RoleID] FOREIGN KEY([RoleID])
REFERENCES [dbo].[Roles] ([RoleID])
ON UPDATE CASCADE
ON DELETE CASCADE
ALTER TABLE [dbo].[UserRoles] CHECK CONSTRAINT [FK_UserRoles_RoleID]
ALTER TABLE [dbo].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_UserRoles_UserID] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
ON UPDATE CASCADE
ON DELETE CASCADE
ALTER TABLE [dbo].[UserRoles] CHECK CONSTRAINT [FK_UserRoles_UserID]