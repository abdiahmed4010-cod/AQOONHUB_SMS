SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[AcademicYears](
	[AcademicYearID] [int] IDENTITY(1,1) NOT NULL,
	[YearName] [nvarchar](20) NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NOT NULL,
	[Status] [nvarchar](20) NULL,
	[CreatedAt] [datetime] NULL,
	[UpdatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[AcademicYearID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[AcademicYears] ADD  DEFAULT ('Upcoming') FOR [Status]
ALTER TABLE [dbo].[AcademicYears] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[AcademicYears] ADD  DEFAULT (getdate()) FOR [UpdatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Activities](
	[ActivityID] [int] IDENTITY(1,1) NOT NULL,
	[ActivityType] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](500) NOT NULL,
	[ActivityDate] [datetime2](7) NOT NULL,
	[PerformedBy] [nvarchar](100) NOT NULL,
	[RelatedID] [int] NULL,
	[RelatedTable] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[ActivityID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Activities] ADD  DEFAULT (getdate()) FOR [ActivityDate]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Admissions](
	[AdmissionID] [int] IDENTITY(1,1) NOT NULL,
	[ApplicationNo] [nvarchar](20) NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[Gender] [nvarchar](10) NOT NULL,
	[DateOfBirth] [date] NOT NULL,
	[ApplyingForClassID] [int] NOT NULL,
	[GuardianName] [nvarchar](100) NOT NULL,
	[GuardianPhone] [nvarchar](20) NOT NULL,
	[GuardianEmail] [nvarchar](100) NULL,
	[ApplicationDate] [date] NULL,
	[Status] [nvarchar](20) NULL,
	[ReviewedBy] [int] NULL,
	[ReviewedAt] [datetime] NULL,
	[Notes] [nvarchar](500) NULL,
	[GuardianID] [int] NULL,
	[StudentID] [int] NULL,
	[EnrolledBy] [int] NULL,
	[EnrolledAt] [datetime2](7) NULL,
	[PreviousSchool] [nvarchar](150) NULL,
	[LastGradeCompleted] [nvarchar](50) NULL,
	[AcademicYearID] [int] NULL,
	[Shift] [nvarchar](20) NULL,
PRIMARY KEY CLUSTERED 
(
	[AdmissionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[ApplicationNo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Admissions] ADD  DEFAULT (getdate()) FOR [ApplicationDate]
ALTER TABLE [dbo].[Admissions] ADD  DEFAULT ('Pending') FOR [Status]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Announcements](
	[AnnouncementID] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[Body] [nvarchar](max) NOT NULL,
	[Audience] [nvarchar](50) NOT NULL,
	[TargetClassID] [int] NULL,
	[IsPinned] [bit] NULL,
	[AuthorID] [int] NOT NULL,
	[CreatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[AnnouncementID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[Announcements] ADD  DEFAULT ((0)) FOR [IsPinned]
ALTER TABLE [dbo].[Announcements] ADD  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Applications](
	[ApplicationID] [int] IDENTITY(1,1) NOT NULL,
	[ApplicantName] [nvarchar](200) NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
	[ApplicationDate] [date] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ApplicationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Applications] ADD  DEFAULT ('Pending') FOR [Status]
ALTER TABLE [dbo].[Applications] ADD  DEFAULT (getdate()) FOR [ApplicationDate]
ALTER TABLE [dbo].[Applications] ADD  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Attendance](
	[AttendanceID] [int] IDENTITY(1,1) NOT NULL,
	[StudentID] [int] NOT NULL,
	[SectionID] [int] NOT NULL,
	[SubjectID] [int] NULL,
	[AttendanceDate] [date] NOT NULL,
	[Period] [nvarchar](20) NULL,
	[Status] [nvarchar](20) NOT NULL,
	[MarkedBy] [int] NOT NULL,
	[Remarks] [nvarchar](200) NULL,
	[CreatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[AttendanceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Attendance] ADD  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[AttendanceAlerts](
	[AttendanceAlertID] [int] IDENTITY(1,1) NOT NULL,
	[AlertType] [nvarchar](40) NOT NULL,
	[AlertKey] [nvarchar](120) NOT NULL,
	[StudentID] [int] NULL,
	[ClassID] [int] NULL,
	[SectionID] [int] NULL,
	[AttendanceSessionID] [int] NULL,
	[Title] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[Severity] [nvarchar](20) NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
	[TriggerValue] [decimal](9, 2) NULL,
	[ThresholdValue] [decimal](9, 2) NULL,
	[IsVisibleToParent] [bit] NOT NULL,
	[FirstDetectedAt] [datetime] NOT NULL,
	[LastDetectedAt] [datetime] NOT NULL,
	[ReviewedBy] [int] NULL,
	[ReviewedAt] [datetime] NULL,
	[ResolvedBy] [int] NULL,
	[ResolvedAt] [datetime] NULL,
	[ResolutionNotes] [nvarchar](500) NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[AttendanceAlertID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[AttendanceAlerts] ADD  CONSTRAINT [DF_AA_Sev]  DEFAULT ('Info') FOR [Severity]
ALTER TABLE [dbo].[AttendanceAlerts] ADD  CONSTRAINT [DF_AA_St]  DEFAULT ('New') FOR [Status]
ALTER TABLE [dbo].[AttendanceAlerts] ADD  CONSTRAINT [DF_AA_Par]  DEFAULT ((0)) FOR [IsVisibleToParent]
ALTER TABLE [dbo].[AttendanceAlerts] ADD  CONSTRAINT [DF_AA_FD]  DEFAULT (getdate()) FOR [FirstDetectedAt]
ALTER TABLE [dbo].[AttendanceAlerts] ADD  CONSTRAINT [DF_AA_LD]  DEFAULT (getdate()) FOR [LastDetectedAt]
ALTER TABLE [dbo].[AttendanceAlerts] ADD  CONSTRAINT [DF_AA_Cre]  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[AttendanceImportBatches](
	[AttendanceImportBatchID] [int] IDENTITY(1,1) NOT NULL,
	[OriginalFileName] [nvarchar](260) NULL,
	[StoredFileName] [nvarchar](260) NULL,
	[FileHash] [char](64) NULL,
	[AcademicYearID] [int] NOT NULL,
	[ClassID] [int] NOT NULL,
	[SectionID] [int] NOT NULL,
	[SubjectID] [int] NULL,
	[SessionType] [nvarchar](20) NOT NULL,
	[ImportStatus] [nvarchar](20) NOT NULL,
	[TotalRows] [int] NOT NULL,
	[ValidRows] [int] NOT NULL,
	[ErrorRows] [int] NOT NULL,
	[ImportedSessions] [int] NOT NULL,
	[ImportedRecords] [int] NOT NULL,
	[ImportedBy] [int] NULL,
	[ImportedAt] [datetime] NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[AttendanceImportBatchID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[AttendanceImportBatches] ADD  CONSTRAINT [DF_AIB_Total]  DEFAULT ((0)) FOR [TotalRows]
ALTER TABLE [dbo].[AttendanceImportBatches] ADD  CONSTRAINT [DF_AIB_Valid]  DEFAULT ((0)) FOR [ValidRows]
ALTER TABLE [dbo].[AttendanceImportBatches] ADD  CONSTRAINT [DF_AIB_Error]  DEFAULT ((0)) FOR [ErrorRows]
ALTER TABLE [dbo].[AttendanceImportBatches] ADD  CONSTRAINT [DF_AIB_Sess]  DEFAULT ((0)) FOR [ImportedSessions]
ALTER TABLE [dbo].[AttendanceImportBatches] ADD  CONSTRAINT [DF_AIB_Recs]  DEFAULT ((0)) FOR [ImportedRecords]
ALTER TABLE [dbo].[AttendanceImportBatches] ADD  CONSTRAINT [DF_AIB_Cre]  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[AttendanceRecords](
	[AttendanceRecordID] [int] IDENTITY(1,1) NOT NULL,
	[AttendanceSessionID] [int] NOT NULL,
	[StudentID] [int] NOT NULL,
	[AttendanceStatus] [nvarchar](20) NOT NULL,
	[CheckInTime] [time](7) NULL,
	[LateMinutes] [int] NULL,
	[Remarks] [nvarchar](300) NULL,
	[RecordedBy] [int] NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[AttendanceRecordID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[AttendanceRecords] ADD  CONSTRAINT [DF_AR_Status]  DEFAULT ('Present') FOR [AttendanceStatus]
ALTER TABLE [dbo].[AttendanceRecords] ADD  CONSTRAINT [DF_AR_Cre]  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[AttendanceSessions](
	[AttendanceSessionID] [int] IDENTITY(1,1) NOT NULL,
	[AcademicYearID] [int] NOT NULL,
	[TermID] [int] NULL,
	[AttendanceDate] [date] NOT NULL,
	[ClassID] [int] NOT NULL,
	[SectionID] [int] NOT NULL,
	[SubjectID] [int] NULL,
	[SessionType] [nvarchar](20) NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
	[MarkedBy] [int] NULL,
	[SubmittedBy] [int] NULL,
	[SubmittedAt] [datetime] NULL,
	[ReopenedBy] [int] NULL,
	[ReopenedAt] [datetime] NULL,
	[ReopenReason] [nvarchar](300) NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[AttendanceSessionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[AttendanceSessions] ADD  CONSTRAINT [DF_AS_Type]  DEFAULT ('Daily') FOR [SessionType]
ALTER TABLE [dbo].[AttendanceSessions] ADD  CONSTRAINT [DF_AS_Status]  DEFAULT ('Draft') FOR [Status]
ALTER TABLE [dbo].[AttendanceSessions] ADD  CONSTRAINT [DF_AS_Cre]  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[AttendanceSettings](
	[AttendanceSettingsID] [int] IDENTITY(1,1) NOT NULL,
	[AllowTeachersToMark] [bit] NOT NULL,
	[AllowEditAfterSubmission] [bit] NOT NULL,
	[EditWindowHours] [int] NOT NULL,
	[AttendanceStartTime] [time](7) NOT NULL,
	[AttendanceEndTime] [time](7) NOT NULL,
	[LateAfterMinutes] [int] NOT NULL,
	[ExcusedRequiresRemarks] [bit] NOT NULL,
	[IncludeLateAsAttended] [bit] NOT NULL,
	[ExcludeExcusedFromRate] [bit] NOT NULL,
	[AllowFutureDate] [bit] NOT NULL,
	[EnableParentNotifications] [bit] NOT NULL,
	[EnableEmailNotifications] [bit] NOT NULL,
	[EnableSMSNotifications] [bit] NOT NULL,
	[ConsecutiveAbsenceAlert] [int] NOT NULL,
	[LowAttendanceThreshold] [decimal](5, 2) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedAt] [datetime] NULL,
	[CreatedAt] [datetime] NOT NULL,
	[FrequentLateThreshold] [int] NOT NULL,
	[UnsubmittedSessionAgeHours] [int] NOT NULL,
	[AlertLookbackDays] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[AttendanceSettingsID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[AttendanceSettings] ADD  CONSTRAINT [DF_ASet_Mark]  DEFAULT ((1)) FOR [AllowTeachersToMark]
ALTER TABLE [dbo].[AttendanceSettings] ADD  CONSTRAINT [DF_ASet_Edit]  DEFAULT ((0)) FOR [AllowEditAfterSubmission]
ALTER TABLE [dbo].[AttendanceSettings] ADD  CONSTRAINT [DF_ASet_Win]  DEFAULT ((24)) FOR [EditWindowHours]
ALTER TABLE [dbo].[AttendanceSettings] ADD  CONSTRAINT [DF_ASet_Start]  DEFAULT ('07:00') FOR [AttendanceStartTime]
ALTER TABLE [dbo].[AttendanceSettings] ADD  CONSTRAINT [DF_ASet_End]  DEFAULT ('10:00') FOR [AttendanceEndTime]
ALTER TABLE [dbo].[AttendanceSettings] ADD  CONSTRAINT [DF_ASet_Late]  DEFAULT ((15)) FOR [LateAfterMinutes]
ALTER TABLE [dbo].[AttendanceSettings] ADD  CONSTRAINT [DF_ASet_Exc]  DEFAULT ((1)) FOR [ExcusedRequiresRemarks]
ALTER TABLE [dbo].[AttendanceSettings] ADD  CONSTRAINT [DF_ASet_IncL]  DEFAULT ((1)) FOR [IncludeLateAsAttended]
ALTER TABLE [dbo].[AttendanceSettings] ADD  CONSTRAINT [DF_ASet_ExR]  DEFAULT ((1)) FOR [ExcludeExcusedFromRate]
ALTER TABLE [dbo].[AttendanceSettings] ADD  CONSTRAINT [DF_ASet_Fut]  DEFAULT ((0)) FOR [AllowFutureDate]
ALTER TABLE [dbo].[AttendanceSettings] ADD  CONSTRAINT [DF_ASet_PN]  DEFAULT ((0)) FOR [EnableParentNotifications]
ALTER TABLE [dbo].[AttendanceSettings] ADD  CONSTRAINT [DF_ASet_EN]  DEFAULT ((0)) FOR [EnableEmailNotifications]
ALTER TABLE [dbo].[AttendanceSettings] ADD  CONSTRAINT [DF_ASet_SN]  DEFAULT ((0)) FOR [EnableSMSNotifications]
ALTER TABLE [dbo].[AttendanceSettings] ADD  CONSTRAINT [DF_ASet_CA]  DEFAULT ((3)) FOR [ConsecutiveAbsenceAlert]
ALTER TABLE [dbo].[AttendanceSettings] ADD  CONSTRAINT [DF_ASet_LA]  DEFAULT ((85.00)) FOR [LowAttendanceThreshold]
ALTER TABLE [dbo].[AttendanceSettings] ADD  CONSTRAINT [DF_ASet_Cre]  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[AttendanceSettings] ADD  CONSTRAINT [DF_ASet_FLate]  DEFAULT ((3)) FOR [FrequentLateThreshold]
ALTER TABLE [dbo].[AttendanceSettings] ADD  CONSTRAINT [DF_ASet_UAge]  DEFAULT ((48)) FOR [UnsubmittedSessionAgeHours]
ALTER TABLE [dbo].[AttendanceSettings] ADD  CONSTRAINT [DF_ASet_Look]  DEFAULT ((30)) FOR [AlertLookbackDays]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[AuditLog](
	[AuditID] [bigint] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NULL,
	[Action] [nvarchar](50) NOT NULL,
	[Module] [nvarchar](100) NOT NULL,
	[Detail] [nvarchar](max) NOT NULL,
	[IPAddress] [nvarchar](50) NULL,
	[ActionTime] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[AuditID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[AuditLog] ADD  DEFAULT (getdate()) FOR [ActionTime]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Classes](
	[ClassID] [int] IDENTITY(1,1) NOT NULL,
	[ClassName] [nvarchar](50) NOT NULL,
	[Capacity] [int] NOT NULL,
	[RoomNumber] [nvarchar](20) NULL,
	[CreatedAt] [datetime] NULL,
	[ClassCode] [nvarchar](20) NULL,
	[Level] [nvarchar](30) NULL,
	[Status] [nvarchar](20) NOT NULL,
	[AcademicYearID] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[ClassID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Classes] ADD  DEFAULT ((30)) FOR [Capacity]
ALTER TABLE [dbo].[Classes] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[Classes] ADD  CONSTRAINT [DF_Classes_Status]  DEFAULT ('Active') FOR [Status]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[ClassFeeStructures](
	[ClassFeeStructureID] [int] IDENTITY(1,1) NOT NULL,
	[AcademicYearID] [int] NOT NULL,
	[ClassID] [int] NOT NULL,
	[SectionID] [int] NULL,
	[FeeCategoryID] [int] NOT NULL,
	[BillingTerm] [nvarchar](20) NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[DiscountType] [nvarchar](20) NOT NULL,
	[DiscountAmount] [decimal](18, 2) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[UpdatedAt] [datetime2](0) NULL,
 CONSTRAINT [PK_ClassFeeStructures] PRIMARY KEY CLUSTERED 
(
	[ClassFeeStructureID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[ClassFeeStructures] ADD  CONSTRAINT [DF_ClassFeeStructures_DiscountType]  DEFAULT ('No Discount') FOR [DiscountType]
ALTER TABLE [dbo].[ClassFeeStructures] ADD  CONSTRAINT [DF_ClassFeeStructures_DiscountAmount]  DEFAULT ((0)) FOR [DiscountAmount]
ALTER TABLE [dbo].[ClassFeeStructures] ADD  CONSTRAINT [DF_ClassFeeStructures_IsActive]  DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[ClassFeeStructures] ADD  CONSTRAINT [DF_ClassFeeStructures_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
ALTER TABLE [dbo].[ClassFeeStructures]  WITH CHECK ADD  CONSTRAINT [CK_ClassFeeStructures_Amount] CHECK  (([Amount]>=(0)))
ALTER TABLE [dbo].[ClassFeeStructures] CHECK CONSTRAINT [CK_ClassFeeStructures_Amount]
ALTER TABLE [dbo].[ClassFeeStructures]  WITH CHECK ADD  CONSTRAINT [CK_ClassFeeStructures_BillingTerm] CHECK  (([BillingTerm]='One Time' OR [BillingTerm]='Annual' OR [BillingTerm]='Per Term' OR [BillingTerm]='Monthly'))
ALTER TABLE [dbo].[ClassFeeStructures] CHECK CONSTRAINT [CK_ClassFeeStructures_BillingTerm]
ALTER TABLE [dbo].[ClassFeeStructures]  WITH CHECK ADD  CONSTRAINT [CK_ClassFeeStructures_Discount] CHECK  (([DiscountAmount]>=(0)))
ALTER TABLE [dbo].[ClassFeeStructures] CHECK CONSTRAINT [CK_ClassFeeStructures_Discount]
ALTER TABLE [dbo].[ClassFeeStructures]  WITH CHECK ADD  CONSTRAINT [CK_ClassFeeStructures_DiscountType] CHECK  (([DiscountType]='Percentage' OR [DiscountType]='Fixed Amount' OR [DiscountType]='No Discount'))
ALTER TABLE [dbo].[ClassFeeStructures] CHECK CONSTRAINT [CK_ClassFeeStructures_DiscountType]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[ClassSubjectTeachers](
	[CSTID] [int] IDENTITY(1,1) NOT NULL,
	[SectionID] [int] NOT NULL,
	[SubjectID] [int] NOT NULL,
	[StaffID] [int] NULL,
	[AcademicYearID] [int] NOT NULL,
	[IsActive] [bit] NULL,
	[WeeklyPeriods] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[CSTID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[ClassSubjectTeachers] ADD  DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[ClassSubjectTeachers] ADD  CONSTRAINT [DF_CST_WP]  DEFAULT ((1)) FOR [WeeklyPeriods]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Documents](
	[DocumentID] [int] IDENTITY(1,1) NOT NULL,
	[EntityType] [nvarchar](50) NOT NULL,
	[EntityID] [int] NOT NULL,
	[DocumentName] [nvarchar](200) NOT NULL,
	[FilePath] [nvarchar](500) NOT NULL,
	[DocumentType] [nvarchar](50) NULL,
	[UploadedBy] [int] NOT NULL,
	[UploadedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[DocumentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Documents] ADD  DEFAULT (getdate()) FOR [UploadedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Events](
	[EventID] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[EventDate] [date] NOT NULL,
	[EventType] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[EventID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Events] ADD  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[ExamClasses](
	[ExamClassID] [int] IDENTITY(1,1) NOT NULL,
	[ExamID] [int] NOT NULL,
	[ClassID] [int] NOT NULL,
	[SectionID] [int] NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ExamClassID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[ExamClasses] ADD  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[ExamResults](
	[ResultID] [int] IDENTITY(1,1) NOT NULL,
	[ExamID] [int] NOT NULL,
	[StudentID] [int] NOT NULL,
	[SubjectID] [int] NOT NULL,
	[Marks] [decimal](6, 2) NULL,
	[Grade] [nvarchar](5) NULL,
	[GPA] [decimal](3, 2) NULL,
	[Remarks] [nvarchar](200) NULL,
	[EnteredBy] [int] NOT NULL,
	[EnteredAt] [datetime] NULL,
	[TotalMarks] [int] NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
	[SubmittedBy] [int] NULL,
	[SubmittedAt] [datetime] NULL,
	[IsAbsent] [bit] NOT NULL,
	[AttendanceStatus] [nvarchar](20) NOT NULL,
	[UpdatedAt] [datetime] NULL,
	[ReopenedBy] [int] NULL,
	[ReopenedAt] [datetime] NULL,
	[ReopenReason] [nvarchar](300) NULL,
PRIMARY KEY CLUSTERED 
(
	[ResultID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[ExamResults] ADD  DEFAULT (getdate()) FOR [EnteredAt]
ALTER TABLE [dbo].[ExamResults] ADD  CONSTRAINT [DF_ER_Total]  DEFAULT ((100)) FOR [TotalMarks]
ALTER TABLE [dbo].[ExamResults] ADD  CONSTRAINT [DF_ER_Status]  DEFAULT ('Draft') FOR [Status]
ALTER TABLE [dbo].[ExamResults] ADD  CONSTRAINT [DF_ER_Absent]  DEFAULT ((0)) FOR [IsAbsent]
ALTER TABLE [dbo].[ExamResults] ADD  CONSTRAINT [DF_ER_Att]  DEFAULT ('Present') FOR [AttendanceStatus]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[ExamRooms](
	[ExamRoomID] [int] IDENTITY(1,1) NOT NULL,
	[RoomName] [nvarchar](60) NOT NULL,
	[Capacity] [int] NOT NULL,
	[Location] [nvarchar](100) NULL,
	[Status] [nvarchar](20) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ExamRoomID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[ExamRooms] ADD  DEFAULT ((40)) FOR [Capacity]
ALTER TABLE [dbo].[ExamRooms] ADD  DEFAULT ('Active') FOR [Status]
ALTER TABLE [dbo].[ExamRooms] ADD  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Exams](
	[ExamID] [int] IDENTITY(1,1) NOT NULL,
	[ExamName] [nvarchar](100) NOT NULL,
	[ExamType] [nvarchar](50) NOT NULL,
	[TermID] [int] NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NOT NULL,
	[Status] [nvarchar](20) NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedAt] [datetime] NULL,
	[AcademicYearID] [int] NULL,
	[TotalMarks] [int] NOT NULL,
	[PassingMark] [int] NOT NULL,
	[Weight] [int] NOT NULL,
	[UpdatedAt] [datetime] NULL,
	[PublishedBy] [int] NULL,
	[PublishedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ExamID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Exams] ADD  DEFAULT ('Draft') FOR [Status]
ALTER TABLE [dbo].[Exams] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[Exams] ADD  CONSTRAINT [DF_Exams_Total]  DEFAULT ((100)) FOR [TotalMarks]
ALTER TABLE [dbo].[Exams] ADD  CONSTRAINT [DF_Exams_Pass]  DEFAULT ((40)) FOR [PassingMark]
ALTER TABLE [dbo].[Exams] ADD  CONSTRAINT [DF_Exams_Weight]  DEFAULT ((100)) FOR [Weight]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[ExamSchedules](
	[ExamScheduleID] [int] IDENTITY(1,1) NOT NULL,
	[ExamID] [int] NOT NULL,
	[ClassID] [int] NOT NULL,
	[SectionID] [int] NULL,
	[SubjectID] [int] NOT NULL,
	[ExamDate] [date] NOT NULL,
	[StartTime] [time](7) NOT NULL,
	[EndTime] [time](7) NOT NULL,
	[ExamRoomID] [int] NULL,
	[InvigilatorStaffID] [int] NULL,
	[Notes] [nvarchar](300) NULL,
	[Status] [nvarchar](20) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ExamScheduleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[ExamSchedules] ADD  DEFAULT ('Scheduled') FOR [Status]
ALTER TABLE [dbo].[ExamSchedules] ADD  DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[ExamSchedules] ADD  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[ExamSubjects](
	[ExamSubjectID] [int] IDENTITY(1,1) NOT NULL,
	[ExamID] [int] NOT NULL,
	[ClassID] [int] NOT NULL,
	[SectionID] [int] NULL,
	[SubjectID] [int] NOT NULL,
	[TotalMarks] [int] NOT NULL,
	[PassingMark] [int] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ExamSubjectID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[ExamSubjects] ADD  DEFAULT ((100)) FOR [TotalMarks]
ALTER TABLE [dbo].[ExamSubjects] ADD  DEFAULT ((40)) FOR [PassingMark]
ALTER TABLE [dbo].[ExamSubjects] ADD  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[FeeCategories](
	[FeeCategoryID] [int] IDENTITY(1,1) NOT NULL,
	[CategoryName] [nvarchar](100) NOT NULL,
	[CategoryCode] [nvarchar](20) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[DefaultBillingTerm] [nvarchar](20) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[UpdatedAt] [datetime2](0) NULL,
 CONSTRAINT [PK_FeeCategories] PRIMARY KEY CLUSTERED 
(
	[FeeCategoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[FeeCategories] ADD  CONSTRAINT [DF_FeeCategories_IsActive]  DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[FeeCategories] ADD  CONSTRAINT [DF_FeeCategories_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
ALTER TABLE [dbo].[FeeCategories]  WITH CHECK ADD  CONSTRAINT [CK_FeeCategories_BillingTerm] CHECK  (([DefaultBillingTerm]='One Time' OR [DefaultBillingTerm]='Annual' OR [DefaultBillingTerm]='Per Term' OR [DefaultBillingTerm]='Monthly'))
ALTER TABLE [dbo].[FeeCategories] CHECK CONSTRAINT [CK_FeeCategories_BillingTerm]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[FeeInvoiceItems](
	[InvoiceItemID] [int] IDENTITY(1,1) NOT NULL,
	[InvoiceID] [int] NOT NULL,
	[FeeCategoryID] [int] NULL,
	[FeeCategoryName] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[DiscountAmount] [decimal](18, 2) NOT NULL,
	[TotalAmount] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_FeeInvoiceItems] PRIMARY KEY CLUSTERED 
(
	[InvoiceItemID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[FeeInvoiceItems] ADD  CONSTRAINT [DF_FeeInvoiceItems_Discount]  DEFAULT ((0)) FOR [DiscountAmount]
ALTER TABLE [dbo].[FeeInvoiceItems]  WITH CHECK ADD  CONSTRAINT [CK_FeeInvoiceItems_Amounts] CHECK  (([Amount]>=(0) AND [DiscountAmount]>=(0) AND [TotalAmount]>=(0)))
ALTER TABLE [dbo].[FeeInvoiceItems] CHECK CONSTRAINT [CK_FeeInvoiceItems_Amounts]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[FeeInvoices](
	[InvoiceID] [int] IDENTITY(1,1) NOT NULL,
	[InvoiceNumber] [nvarchar](30) NOT NULL,
	[StudentID] [int] NOT NULL,
	[AcademicYearID] [int] NOT NULL,
	[InvoiceDate] [date] NOT NULL,
	[DueDate] [date] NOT NULL,
	[InvoiceType] [nvarchar](50) NOT NULL,
	[Subtotal] [decimal](18, 2) NOT NULL,
	[DiscountAmount] [decimal](18, 2) NOT NULL,
	[TotalAmount] [decimal](18, 2) NOT NULL,
	[PaidAmount] [decimal](18, 2) NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
	[Remarks] [nvarchar](1000) NULL,
	[PaymentInstructions] [nvarchar](1000) NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[UpdatedAt] [datetime2](0) NULL,
 CONSTRAINT [PK_FeeInvoices] PRIMARY KEY CLUSTERED 
(
	[InvoiceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_FeeInvoices_Number] UNIQUE NONCLUSTERED 
(
	[InvoiceNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[FeeInvoices] ADD  CONSTRAINT [DF_FeeInvoices_Discount]  DEFAULT ((0)) FOR [DiscountAmount]
ALTER TABLE [dbo].[FeeInvoices] ADD  CONSTRAINT [DF_FeeInvoices_Paid]  DEFAULT ((0)) FOR [PaidAmount]
ALTER TABLE [dbo].[FeeInvoices] ADD  CONSTRAINT [DF_FeeInvoices_Status]  DEFAULT ('Unpaid') FOR [Status]
ALTER TABLE [dbo].[FeeInvoices] ADD  CONSTRAINT [DF_FeeInvoices_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
ALTER TABLE [dbo].[FeeInvoices]  WITH CHECK ADD  CONSTRAINT [CK_FeeInvoices_Amounts] CHECK  (([Subtotal]>=(0) AND [DiscountAmount]>=(0) AND [TotalAmount]>=(0) AND [PaidAmount]>=(0) AND [PaidAmount]<=[TotalAmount]))
ALTER TABLE [dbo].[FeeInvoices] CHECK CONSTRAINT [CK_FeeInvoices_Amounts]
ALTER TABLE [dbo].[FeeInvoices]  WITH CHECK ADD  CONSTRAINT [CK_FeeInvoices_Status] CHECK  (([Status]='Cancelled' OR [Status]='Overdue' OR [Status]='Paid' OR [Status]='Partial' OR [Status]='Unpaid'))
ALTER TABLE [dbo].[FeeInvoices] CHECK CONSTRAINT [CK_FeeInvoices_Status]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[FeePayments](
	[PaymentID] [int] IDENTITY(1,1) NOT NULL,
	[InvoiceID] [int] NOT NULL,
	[StudentID] [int] NOT NULL,
	[ReceiptNumber] [nvarchar](30) NOT NULL,
	[AmountPaid] [decimal](18, 2) NOT NULL,
	[PaymentMethod] [nvarchar](30) NOT NULL,
	[PaymentDate] [date] NOT NULL,
	[ReferenceNumber] [nvarchar](100) NULL,
	[Notes] [nvarchar](1000) NULL,
	[PreviousBalance] [decimal](18, 2) NOT NULL,
	[NewBalance] [decimal](18, 2) NOT NULL,
	[ReceivedBy] [int] NOT NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_FeePayments] PRIMARY KEY CLUSTERED 
(
	[PaymentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_FeePayments_Receipt] UNIQUE NONCLUSTERED 
(
	[ReceiptNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[FeePayments] ADD  CONSTRAINT [DF_FeePayments_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
ALTER TABLE [dbo].[FeePayments]  WITH CHECK ADD  CONSTRAINT [CK_FeePayments_Amount] CHECK  (([AmountPaid]>(0)))
ALTER TABLE [dbo].[FeePayments] CHECK CONSTRAINT [CK_FeePayments_Amount]
ALTER TABLE [dbo].[FeePayments]  WITH CHECK ADD  CONSTRAINT [CK_FeePayments_Method] CHECK  (([PaymentMethod]='Other' OR [PaymentMethod]='Cheque' OR [PaymentMethod]='Card' OR [PaymentMethod]='Bank Transfer' OR [PaymentMethod]='Mobile Money' OR [PaymentMethod]='Cash'))
ALTER TABLE [dbo].[FeePayments] CHECK CONSTRAINT [CK_FeePayments_Method]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[FeeStructures](
	[FeeStructureID] [int] IDENTITY(1,1) NOT NULL,
	[FeeName] [nvarchar](100) NOT NULL,
	[Category] [nvarchar](50) NOT NULL,
	[ClassID] [int] NULL,
	[Amount] [decimal](10, 2) NOT NULL,
	[BillingTerm] [nvarchar](50) NOT NULL,
	[AcademicYearID] [int] NOT NULL,
	[IsActive] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[FeeStructureID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[FeeStructures] ADD  DEFAULT ((1)) FOR [IsActive]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[GradingScale](
	[GradeID] [int] IDENTITY(1,1) NOT NULL,
	[GradeLetter] [nvarchar](5) NOT NULL,
	[MinMarks] [int] NOT NULL,
	[MaxMarks] [int] NOT NULL,
	[GPA] [decimal](3, 2) NOT NULL,
	[AcademicYearID] [int] NOT NULL,
	[Description] [nvarchar](100) NULL,
	[IsPass] [bit] NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[GradeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[GradingScale] ADD  CONSTRAINT [DF_Grade_Pass]  DEFAULT ((1)) FOR [IsPass]
ALTER TABLE [dbo].[GradingScale] ADD  CONSTRAINT [DF_Grade_Status]  DEFAULT ('Active') FOR [Status]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Guardians](
	[GuardianID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NULL,
	[FullName] [nvarchar](100) NOT NULL,
	[Relationship] [nvarchar](50) NOT NULL,
	[Phone] [nvarchar](20) NOT NULL,
	[Email] [nvarchar](100) NULL,
	[Address] [nvarchar](200) NULL,
	[IsActive] [bit] NULL,
	[AlternatePhone] [nvarchar](30) NULL,
	[Occupation] [nvarchar](100) NULL,
	[NationalID] [nvarchar](30) NULL,
	[EmergencyContact] [nvarchar](100) NULL,
	[CreatedAt] [datetime2](7) NULL,
	[UpdatedAt] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[GuardianID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Guardians] ADD  DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[Guardians] ADD  CONSTRAINT [DF_Guardians_CreatedAt]  DEFAULT (sysdatetime()) FOR [CreatedAt]
ALTER TABLE [dbo].[Guardians] ADD  CONSTRAINT [DF_Guardians_UpdatedAt]  DEFAULT (sysdatetime()) FOR [UpdatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[InvoiceItems](
	[InvoiceItemID] [int] IDENTITY(1,1) NOT NULL,
	[InvoiceID] [int] NOT NULL,
	[FeeStructureID] [int] NOT NULL,
	[Description] [nvarchar](200) NOT NULL,
	[Amount] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[InvoiceItemID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Invoices](
	[InvoiceID] [int] IDENTITY(1,1) NOT NULL,
	[InvoiceNo] [nvarchar](20) NOT NULL,
	[StudentID] [int] NOT NULL,
	[AcademicYearID] [int] NOT NULL,
	[TermID] [int] NOT NULL,
	[TotalAmount] [decimal](10, 2) NOT NULL,
	[PaidAmount] [decimal](10, 2) NULL,
	[DueDate] [date] NOT NULL,
	[Status] [nvarchar](20) NULL,
	[GeneratedBy] [int] NULL,
	[GeneratedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[InvoiceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[InvoiceNo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Invoices] ADD  DEFAULT ((0)) FOR [PaidAmount]
ALTER TABLE [dbo].[Invoices] ADD  DEFAULT ('Pending') FOR [Status]
ALTER TABLE [dbo].[Invoices] ADD  DEFAULT (getdate()) FOR [GeneratedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[LeaveRequests](
	[LeaveID] [int] IDENTITY(1,1) NOT NULL,
	[StaffID] [int] NOT NULL,
	[LeaveType] [nvarchar](50) NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NOT NULL,
	[Days] [int] NOT NULL,
	[Reason] [nvarchar](500) NULL,
	[Status] [nvarchar](20) NULL,
	[ApprovedBy] [int] NULL,
	[ApprovedAt] [datetime] NULL,
	[CreatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[LeaveID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[LeaveRequests] ADD  DEFAULT ('Pending') FOR [Status]
ALTER TABLE [dbo].[LeaveRequests] ADD  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[LoginActivity](
	[LoginID] [bigint] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NULL,
	[IPAddress] [nvarchar](50) NULL,
	[DeviceInfo] [nvarchar](200) NULL,
	[LoginTime] [datetime] NULL,
	[Status] [nvarchar](20) NOT NULL,
	[FailureReason] [nvarchar](200) NULL,
	[Email] [nvarchar](255) NULL,
	[Device] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[LoginID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[LoginActivity] ADD  DEFAULT (getdate()) FOR [LoginTime]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Notifications](
	[NotificationID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[Message] [nvarchar](2000) NOT NULL,
	[NotificationType] [nvarchar](50) NULL,
	[Priority] [nvarchar](20) NULL,
	[IsRead] [bit] NOT NULL,
	[ReadAt] [datetime] NULL,
	[LinkUrl] [nvarchar](500) NULL,
	[Icon] [nvarchar](100) NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[NotificationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Notifications] ADD  DEFAULT ('System') FOR [NotificationType]
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT ('Normal') FOR [Priority]
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT ((0)) FOR [IsRead]
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Payments](
	[PaymentID] [int] IDENTITY(1,1) NOT NULL,
	[ReceiptNo] [nvarchar](20) NOT NULL,
	[InvoiceID] [int] NOT NULL,
	[Amount] [decimal](10, 2) NOT NULL,
	[PaymentMethod] [nvarchar](50) NOT NULL,
	[PaymentDate] [date] NOT NULL,
	[ReceivedBy] [int] NOT NULL,
	[Notes] [nvarchar](500) NULL,
	[CreatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[PaymentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[ReceiptNo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Payments] ADD  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[PayrollAdjustments](
	[PayrollAdjustmentID] [int] IDENTITY(1,1) NOT NULL,
	[PayrollRecordID] [int] NOT NULL,
	[AdjustmentType] [nvarchar](20) NOT NULL,
	[AdjustmentName] [nvarchar](100) NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[Notes] [nvarchar](500) NULL,
	[CreatedBy] [int] NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_PayrollAdjustments] PRIMARY KEY CLUSTERED 
(
	[PayrollAdjustmentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[PayrollAdjustments] ADD  CONSTRAINT [DF_PayrollAdjustments_CreatedAt]  DEFAULT (sysdatetime()) FOR [CreatedAt]
ALTER TABLE [dbo].[PayrollAdjustments]  WITH CHECK ADD  CONSTRAINT [CK_PayrollAdjustments_Amount] CHECK  (([Amount]>(0)))
ALTER TABLE [dbo].[PayrollAdjustments] CHECK CONSTRAINT [CK_PayrollAdjustments_Amount]
ALTER TABLE [dbo].[PayrollAdjustments]  WITH CHECK ADD  CONSTRAINT [CK_PayrollAdjustments_Type] CHECK  (([AdjustmentType]=N'Bonus' OR [AdjustmentType]=N'Deduction' OR [AdjustmentType]=N'Allowance'))
ALTER TABLE [dbo].[PayrollAdjustments] CHECK CONSTRAINT [CK_PayrollAdjustments_Type]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[PayrollPeriods](
	[PayrollPeriodID] [int] IDENTITY(1,1) NOT NULL,
	[PeriodName] [nvarchar](100) NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NOT NULL,
	[PaymentDate] [date] NULL,
	[Status] [nvarchar](20) NOT NULL,
	[CreatedBy] [int] NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedAt] [datetime2](0) NULL,
 CONSTRAINT [PK_PayrollPeriods] PRIMARY KEY CLUSTERED 
(
	[PayrollPeriodID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_PayrollPeriods_StartDate_EndDate] UNIQUE NONCLUSTERED 
(
	[StartDate] ASC,
	[EndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[PayrollPeriods] ADD  CONSTRAINT [DF_PayrollPeriods_Status]  DEFAULT (N'Draft') FOR [Status]
ALTER TABLE [dbo].[PayrollPeriods] ADD  CONSTRAINT [DF_PayrollPeriods_CreatedAt]  DEFAULT (sysdatetime()) FOR [CreatedAt]
ALTER TABLE [dbo].[PayrollPeriods]  WITH CHECK ADD  CONSTRAINT [CK_PayrollPeriods_Dates] CHECK  (([StartDate]<=[EndDate]))
ALTER TABLE [dbo].[PayrollPeriods] CHECK CONSTRAINT [CK_PayrollPeriods_Dates]
ALTER TABLE [dbo].[PayrollPeriods]  WITH CHECK ADD  CONSTRAINT [CK_PayrollPeriods_PaymentDate] CHECK  (([PaymentDate] IS NULL OR [PaymentDate]>=[StartDate]))
ALTER TABLE [dbo].[PayrollPeriods] CHECK CONSTRAINT [CK_PayrollPeriods_PaymentDate]
ALTER TABLE [dbo].[PayrollPeriods]  WITH CHECK ADD  CONSTRAINT [CK_PayrollPeriods_Status] CHECK  (([Status]=N'Cancelled' OR [Status]=N'Completed' OR [Status]=N'Processing' OR [Status]=N'Draft'))
ALTER TABLE [dbo].[PayrollPeriods] CHECK CONSTRAINT [CK_PayrollPeriods_Status]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[PayrollRecords](
	[PayrollRecordID] [int] IDENTITY(1,1) NOT NULL,
	[PayrollPeriodID] [int] NOT NULL,
	[StaffID] [int] NOT NULL,
	[BasicSalary] [decimal](18, 2) NOT NULL,
	[HousingAllowance] [decimal](18, 2) NOT NULL,
	[TransportAllowance] [decimal](18, 2) NOT NULL,
	[OtherAllowance] [decimal](18, 2) NOT NULL,
	[TaxDeduction] [decimal](18, 2) NOT NULL,
	[OtherDeduction] [decimal](18, 2) NOT NULL,
	[TotalAllowances] [decimal](18, 2) NOT NULL,
	[BonusAmount] [decimal](18, 2) NOT NULL,
	[TotalDeductions] [decimal](18, 2) NOT NULL,
	[GrossSalary] [decimal](18, 2) NOT NULL,
	[NetSalary] [decimal](18, 2) NOT NULL,
	[PaymentStatus] [nvarchar](20) NOT NULL,
	[PaymentMethod] [nvarchar](30) NULL,
	[PaymentReference] [nvarchar](100) NULL,
	[PaidDate] [datetime2](0) NULL,
	[Notes] [nvarchar](500) NULL,
	[CreatedBy] [int] NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedAt] [datetime2](0) NULL,
 CONSTRAINT [PK_PayrollRecords] PRIMARY KEY CLUSTERED 
(
	[PayrollRecordID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_PayrollRecords_Period_Staff] UNIQUE NONCLUSTERED 
(
	[PayrollPeriodID] ASC,
	[StaffID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[PayrollRecords] ADD  CONSTRAINT [DF_PayrollRecords_BasicSalary]  DEFAULT ((0)) FOR [BasicSalary]
ALTER TABLE [dbo].[PayrollRecords] ADD  CONSTRAINT [DF_PayrollRecords_HousingAllowance]  DEFAULT ((0)) FOR [HousingAllowance]
ALTER TABLE [dbo].[PayrollRecords] ADD  CONSTRAINT [DF_PayrollRecords_TransportAllowance]  DEFAULT ((0)) FOR [TransportAllowance]
ALTER TABLE [dbo].[PayrollRecords] ADD  CONSTRAINT [DF_PayrollRecords_OtherAllowance]  DEFAULT ((0)) FOR [OtherAllowance]
ALTER TABLE [dbo].[PayrollRecords] ADD  CONSTRAINT [DF_PayrollRecords_TaxDeduction]  DEFAULT ((0)) FOR [TaxDeduction]
ALTER TABLE [dbo].[PayrollRecords] ADD  CONSTRAINT [DF_PayrollRecords_OtherDeduction]  DEFAULT ((0)) FOR [OtherDeduction]
ALTER TABLE [dbo].[PayrollRecords] ADD  CONSTRAINT [DF_PayrollRecords_TotalAllowances]  DEFAULT ((0)) FOR [TotalAllowances]
ALTER TABLE [dbo].[PayrollRecords] ADD  CONSTRAINT [DF_PayrollRecords_BonusAmount]  DEFAULT ((0)) FOR [BonusAmount]
ALTER TABLE [dbo].[PayrollRecords] ADD  CONSTRAINT [DF_PayrollRecords_TotalDeductions]  DEFAULT ((0)) FOR [TotalDeductions]
ALTER TABLE [dbo].[PayrollRecords] ADD  CONSTRAINT [DF_PayrollRecords_GrossSalary]  DEFAULT ((0)) FOR [GrossSalary]
ALTER TABLE [dbo].[PayrollRecords] ADD  CONSTRAINT [DF_PayrollRecords_NetSalary]  DEFAULT ((0)) FOR [NetSalary]
ALTER TABLE [dbo].[PayrollRecords] ADD  CONSTRAINT [DF_PayrollRecords_PaymentStatus]  DEFAULT (N'Pending') FOR [PaymentStatus]
ALTER TABLE [dbo].[PayrollRecords] ADD  CONSTRAINT [DF_PayrollRecords_CreatedAt]  DEFAULT (sysdatetime()) FOR [CreatedAt]
ALTER TABLE [dbo].[PayrollRecords]  WITH CHECK ADD  CONSTRAINT [CK_PayrollRecords_Amounts] CHECK  (([BasicSalary]>=(0) AND [HousingAllowance]>=(0) AND [TransportAllowance]>=(0) AND [OtherAllowance]>=(0) AND [TaxDeduction]>=(0) AND [OtherDeduction]>=(0) AND [TotalAllowances]>=(0) AND [BonusAmount]>=(0) AND [TotalDeductions]>=(0) AND [GrossSalary]>=(0) AND [NetSalary]>=(0)))
ALTER TABLE [dbo].[PayrollRecords] CHECK CONSTRAINT [CK_PayrollRecords_Amounts]
ALTER TABLE [dbo].[PayrollRecords]  WITH CHECK ADD  CONSTRAINT [CK_PayrollRecords_PaidFields] CHECK  (([PaymentStatus]<>N'Paid' OR [PaidDate] IS NOT NULL AND [PaymentMethod] IS NOT NULL))
ALTER TABLE [dbo].[PayrollRecords] CHECK CONSTRAINT [CK_PayrollRecords_PaidFields]
ALTER TABLE [dbo].[PayrollRecords]  WITH CHECK ADD  CONSTRAINT [CK_PayrollRecords_PaymentStatus] CHECK  (([PaymentStatus]=N'Cancelled' OR [PaymentStatus]=N'Failed' OR [PaymentStatus]=N'Paid' OR [PaymentStatus]=N'Pending'))
ALTER TABLE [dbo].[PayrollRecords] CHECK CONSTRAINT [CK_PayrollRecords_PaymentStatus]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Permissions](
	[PermissionID] [int] IDENTITY(1,1) NOT NULL,
	[PermissionName] [nvarchar](100) NOT NULL,
	[Module] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_Permissions] PRIMARY KEY CLUSTERED 
(
	[PermissionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Permissions_PermissionName] UNIQUE NONCLUSTERED 
(
	[PermissionName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Permissions] ADD  CONSTRAINT [DF_Permissions_IsActive]  DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[Permissions] ADD  CONSTRAINT [DF_Permissions_CreatedAt]  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[ReportAuditLogs](
	[ReportAuditLogID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NULL,
	[Action] [nvarchar](40) NOT NULL,
	[ReportKey] [nvarchar](80) NULL,
	[ReportName] [nvarchar](150) NULL,
	[Category] [nvarchar](60) NULL,
	[FilterSummary] [nvarchar](500) NULL,
	[ResultStatus] [nvarchar](20) NOT NULL,
	[IpAddress] [nvarchar](60) NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ReportAuditLogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[ReportAuditLogs] ADD  CONSTRAINT [DF_RAL_St]  DEFAULT ('Success') FOR [ResultStatus]
ALTER TABLE [dbo].[ReportAuditLogs] ADD  CONSTRAINT [DF_RAL_Cre]  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[ReportExports](
	[ReportExportID] [int] IDENTITY(1,1) NOT NULL,
	[ReportKey] [nvarchar](80) NOT NULL,
	[ReportName] [nvarchar](150) NOT NULL,
	[Category] [nvarchar](60) NOT NULL,
	[ExportFormat] [nvarchar](20) NOT NULL,
	[FilterSummary] [nvarchar](500) NULL,
	[FileName] [nvarchar](260) NULL,
	[FilePath] [nvarchar](400) NULL,
	[FileSize] [bigint] NULL,
	[Status] [nvarchar](30) NOT NULL,
	[GeneratedBy] [int] NULL,
	[GeneratedAt] [datetime] NOT NULL,
	[ExpiresAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ReportExportID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[ReportExports] ADD  CONSTRAINT [DF_RE_St]  DEFAULT ('Generated') FOR [Status]
ALTER TABLE [dbo].[ReportExports] ADD  CONSTRAINT [DF_RE_Gen]  DEFAULT (getdate()) FOR [GeneratedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Reports](
	[ReportID] [int] IDENTITY(1,1) NOT NULL,
	[ReportName] [nvarchar](200) NOT NULL,
	[ReportType] [nvarchar](50) NULL,
	[Description] [nvarchar](1000) NULL,
	[Parameters] [nvarchar](2000) NULL,
	[FilePath] [nvarchar](500) NULL,
	[Status] [nvarchar](50) NOT NULL,
	[GeneratedBy] [int] NOT NULL,
	[GeneratedAt] [datetime] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ReportID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Reports] ADD  DEFAULT ('PDF') FOR [ReportType]
ALTER TABLE [dbo].[Reports] ADD  DEFAULT ('Pending') FOR [Status]
ALTER TABLE [dbo].[Reports] ADD  DEFAULT (getdate()) FOR [GeneratedAt]
ALTER TABLE [dbo].[Reports] ADD  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[ResultPublications](
	[PublicationID] [int] IDENTITY(1,1) NOT NULL,
	[ExamID] [int] NOT NULL,
	[ClassID] [int] NOT NULL,
	[SectionID] [int] NULL,
	[Status] [nvarchar](20) NOT NULL,
	[PublishedBy] [int] NULL,
	[PublishedAt] [datetime] NOT NULL,
	[Reason] [nvarchar](300) NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UnpublishedBy] [int] NULL,
	[UnpublishedAt] [datetime] NULL,
	[UnpublishReason] [nvarchar](300) NULL,
PRIMARY KEY CLUSTERED 
(
	[PublicationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[ResultPublications] ADD  DEFAULT ('Published') FOR [Status]
ALTER TABLE [dbo].[ResultPublications] ADD  DEFAULT (getdate()) FOR [PublishedAt]
ALTER TABLE [dbo].[ResultPublications] ADD  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[RolePermissions](
	[RolePermissionID] [int] IDENTITY(1,1) NOT NULL,
	[RoleID] [int] NOT NULL,
	[PermissionID] [int] NOT NULL,
	[AssignedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_RolePermissions] PRIMARY KEY CLUSTERED 
(
	[RolePermissionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[RolePermissions] ADD  CONSTRAINT [DF_RolePermissions_AssignedAt]  DEFAULT (getdate()) FOR [AssignedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Roles](
	[RoleID] [int] IDENTITY(1,1) NOT NULL,
	[RoleName] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED 
(
	[RoleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Roles_RoleName] UNIQUE NONCLUSTERED 
(
	[RoleName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Roles] ADD  CONSTRAINT [DF_Roles_IsActive]  DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[Roles] ADD  CONSTRAINT [DF_Roles_CreatedAt]  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[Roles] ADD  CONSTRAINT [DF_Roles_UpdatedAt]  DEFAULT (getdate()) FOR [UpdatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[SavedReports](
	[SavedReportID] [int] IDENTITY(1,1) NOT NULL,
	[ReportName] [nvarchar](150) NOT NULL,
	[ReportKey] [nvarchar](80) NOT NULL,
	[Category] [nvarchar](60) NOT NULL,
	[ConfigurationJson] [nvarchar](max) NULL,
	[Visibility] [nvarchar](20) NOT NULL,
	[OwnerUserID] [int] NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NULL,
	[LastRunAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[SavedReportID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[SavedReports] ADD  CONSTRAINT [DF_SR_Vis]  DEFAULT ('Private') FOR [Visibility]
ALTER TABLE [dbo].[SavedReports] ADD  CONSTRAINT [DF_SR_Act]  DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[SavedReports] ADD  CONSTRAINT [DF_SR_Cre]  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[ScheduledReports](
	[ScheduledReportID] [int] IDENTITY(1,1) NOT NULL,
	[SavedReportID] [int] NULL,
	[Frequency] [nvarchar](20) NOT NULL,
	[RunTime] [time](7) NULL,
	[DayOfWeek] [int] NULL,
	[DayOfMonth] [int] NULL,
	[Recipients] [nvarchar](500) NULL,
	[ExportFormat] [nvarchar](20) NOT NULL,
	[Status] [nvarchar](60) NOT NULL,
	[LastRunAt] [datetime] NULL,
	[NextRunAt] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ScheduledReportID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[ScheduledReports] ADD  CONSTRAINT [DF_SCH_Fmt]  DEFAULT ('CSV') FOR [ExportFormat]
ALTER TABLE [dbo].[ScheduledReports] ADD  CONSTRAINT [DF_SCH_St]  DEFAULT ('Pending Scheduler Configuration') FOR [Status]
ALTER TABLE [dbo].[ScheduledReports] ADD  CONSTRAINT [DF_SCH_Cre]  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[SchoolSettings](
	[SettingID] [int] IDENTITY(1,1) NOT NULL,
	[SchoolName] [nvarchar](200) NOT NULL,
	[Address] [nvarchar](300) NULL,
	[Phone] [nvarchar](20) NULL,
	[Email] [nvarchar](100) NULL,
	[Currency] [nvarchar](10) NULL,
	[TimeZone] [nvarchar](100) NULL,
	[Language] [nvarchar](20) NULL,
	[LogoPath] [nvarchar](500) NULL,
	[CurrentAcademicYearID] [int] NULL,
	[CurrentTermID] [int] NULL,
	[UpdatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[SettingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[SchoolSettings] ADD  DEFAULT ('USD') FOR [Currency]
ALTER TABLE [dbo].[SchoolSettings] ADD  DEFAULT ('(GMT+03:00) East Africa Time') FOR [TimeZone]
ALTER TABLE [dbo].[SchoolSettings] ADD  DEFAULT ('English') FOR [Language]
ALTER TABLE [dbo].[SchoolSettings] ADD  DEFAULT (getdate()) FOR [UpdatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Sections](
	[SectionID] [int] IDENTITY(1,1) NOT NULL,
	[ClassID] [int] NOT NULL,
	[SectionName] [nvarchar](10) NOT NULL,
	[Capacity] [int] NOT NULL,
	[StaffID] [int] NULL,
	[RoomNumber] [nvarchar](30) NULL,
	[Status] [nvarchar](20) NOT NULL,
	[AcademicYearID] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[SectionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Sections] ADD  DEFAULT ((30)) FOR [Capacity]
ALTER TABLE [dbo].[Sections] ADD  CONSTRAINT [DF_Sections_Status]  DEFAULT ('Active') FOR [Status]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Staff](
	[StaffID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[EmployeeID] [nvarchar](20) NOT NULL,
	[Department] [nvarchar](50) NOT NULL,
	[Position] [nvarchar](100) NOT NULL,
	[HireDate] [date] NOT NULL,
	[Salary] [decimal](10, 2) NULL,
	[LeaveBalance] [int] NULL,
	[Status] [nvarchar](20) NULL,
PRIMARY KEY CLUSTERED 
(
	[StaffID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[EmployeeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Staff] ADD  DEFAULT ((0)) FOR [LeaveBalance]
ALTER TABLE [dbo].[Staff] ADD  DEFAULT ('Active') FOR [Status]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[StaffSalaryStructures](
	[SalaryStructureID] [int] IDENTITY(1,1) NOT NULL,
	[StaffID] [int] NOT NULL,
	[BasicSalary] [decimal](18, 2) NOT NULL,
	[HousingAllowance] [decimal](18, 2) NOT NULL,
	[TransportAllowance] [decimal](18, 2) NOT NULL,
	[OtherAllowance] [decimal](18, 2) NOT NULL,
	[TaxDeduction] [decimal](18, 2) NOT NULL,
	[OtherDeduction] [decimal](18, 2) NOT NULL,
	[EffectiveFrom] [date] NOT NULL,
	[EffectiveTo] [date] NULL,
	[Status] [nvarchar](20) NOT NULL,
	[CreatedBy] [int] NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedAt] [datetime2](0) NULL,
 CONSTRAINT [PK_StaffSalaryStructures] PRIMARY KEY CLUSTERED 
(
	[SalaryStructureID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_StaffSalaryStructures_Staff_EffectiveFrom] UNIQUE NONCLUSTERED 
(
	[StaffID] ASC,
	[EffectiveFrom] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[StaffSalaryStructures] ADD  CONSTRAINT [DF_StaffSalaryStructures_BasicSalary]  DEFAULT ((0)) FOR [BasicSalary]
ALTER TABLE [dbo].[StaffSalaryStructures] ADD  CONSTRAINT [DF_StaffSalaryStructures_HousingAllowance]  DEFAULT ((0)) FOR [HousingAllowance]
ALTER TABLE [dbo].[StaffSalaryStructures] ADD  CONSTRAINT [DF_StaffSalaryStructures_TransportAllowance]  DEFAULT ((0)) FOR [TransportAllowance]
ALTER TABLE [dbo].[StaffSalaryStructures] ADD  CONSTRAINT [DF_StaffSalaryStructures_OtherAllowance]  DEFAULT ((0)) FOR [OtherAllowance]
ALTER TABLE [dbo].[StaffSalaryStructures] ADD  CONSTRAINT [DF_StaffSalaryStructures_TaxDeduction]  DEFAULT ((0)) FOR [TaxDeduction]
ALTER TABLE [dbo].[StaffSalaryStructures] ADD  CONSTRAINT [DF_StaffSalaryStructures_OtherDeduction]  DEFAULT ((0)) FOR [OtherDeduction]
ALTER TABLE [dbo].[StaffSalaryStructures] ADD  CONSTRAINT [DF_StaffSalaryStructures_Status]  DEFAULT (N'Active') FOR [Status]
ALTER TABLE [dbo].[StaffSalaryStructures] ADD  CONSTRAINT [DF_StaffSalaryStructures_CreatedAt]  DEFAULT (sysdatetime()) FOR [CreatedAt]
ALTER TABLE [dbo].[StaffSalaryStructures]  WITH CHECK ADD  CONSTRAINT [CK_StaffSalaryStructures_Amounts] CHECK  (([BasicSalary]>=(0) AND [HousingAllowance]>=(0) AND [TransportAllowance]>=(0) AND [OtherAllowance]>=(0) AND [TaxDeduction]>=(0) AND [OtherDeduction]>=(0)))
ALTER TABLE [dbo].[StaffSalaryStructures] CHECK CONSTRAINT [CK_StaffSalaryStructures_Amounts]
ALTER TABLE [dbo].[StaffSalaryStructures]  WITH CHECK ADD  CONSTRAINT [CK_StaffSalaryStructures_Dates] CHECK  (([EffectiveTo] IS NULL OR [EffectiveFrom]<=[EffectiveTo]))
ALTER TABLE [dbo].[StaffSalaryStructures] CHECK CONSTRAINT [CK_StaffSalaryStructures_Dates]
ALTER TABLE [dbo].[StaffSalaryStructures]  WITH CHECK ADD  CONSTRAINT [CK_StaffSalaryStructures_Status] CHECK  (([Status]=N'Inactive' OR [Status]=N'Active'))
ALTER TABLE [dbo].[StaffSalaryStructures] CHECK CONSTRAINT [CK_StaffSalaryStructures_Status]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[StudentExamSummaries](
	[StudentExamSummaryID] [int] IDENTITY(1,1) NOT NULL,
	[ExamID] [int] NOT NULL,
	[StudentID] [int] NOT NULL,
	[AcademicYearID] [int] NULL,
	[ClassID] [int] NULL,
	[SectionID] [int] NULL,
	[TotalObtained] [decimal](9, 2) NOT NULL,
	[TotalMaximum] [decimal](9, 2) NOT NULL,
	[AveragePercentage] [decimal](6, 2) NOT NULL,
	[OverallGrade] [nvarchar](10) NULL,
	[Rank] [int] NULL,
	[ResultStatus] [nvarchar](20) NULL,
	[PublicationStatus] [nvarchar](20) NOT NULL,
	[CalculatedBy] [int] NULL,
	[CalculatedAt] [datetime] NULL,
	[PublishedBy] [int] NULL,
	[PublishedAt] [datetime] NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[StudentExamSummaryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[StudentExamSummaries] ADD  CONSTRAINT [DF_SES_Obt]  DEFAULT ((0)) FOR [TotalObtained]
ALTER TABLE [dbo].[StudentExamSummaries] ADD  CONSTRAINT [DF_SES_Max]  DEFAULT ((0)) FOR [TotalMaximum]
ALTER TABLE [dbo].[StudentExamSummaries] ADD  CONSTRAINT [DF_SES_Avg]  DEFAULT ((0)) FOR [AveragePercentage]
ALTER TABLE [dbo].[StudentExamSummaries] ADD  CONSTRAINT [DF_SES_Pub]  DEFAULT ('Draft') FOR [PublicationStatus]
ALTER TABLE [dbo].[StudentExamSummaries] ADD  CONSTRAINT [DF_SES_Cre]  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[StudentGuardians](
	[StudentGuardianID] [int] IDENTITY(1,1) NOT NULL,
	[StudentID] [int] NOT NULL,
	[GuardianID] [int] NOT NULL,
	[IsPrimary] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[StudentGuardianID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[StudentGuardians] ADD  DEFAULT ((0)) FOR [IsPrimary]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[StudentPromotions](
	[PromotionID] [int] IDENTITY(1,1) NOT NULL,
	[StudentID] [int] NOT NULL,
	[FromAcademicYearID] [int] NOT NULL,
	[ToAcademicYearID] [int] NOT NULL,
	[FromSectionID] [int] NULL,
	[ToSectionID] [int] NULL,
	[Status] [nvarchar](20) NOT NULL,
	[ActionDate] [datetime] NOT NULL,
	[PromotedBy] [int] NULL,
	[Notes] [nvarchar](400) NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PromotionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[StudentPromotions] ADD  DEFAULT ('Promoted') FOR [Status]
ALTER TABLE [dbo].[StudentPromotions] ADD  DEFAULT (getdate()) FOR [ActionDate]
ALTER TABLE [dbo].[StudentPromotions] ADD  DEFAULT (getdate()) FOR [CreatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Students](
	[StudentID] [int] IDENTITY(1,1) NOT NULL,
	[StudentCode] [nvarchar](20) NOT NULL,
	[AdmissionNo] [nvarchar](20) NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[FullName]  AS (([FirstName]+' ')+[LastName]),
	[Gender] [nvarchar](10) NOT NULL,
	[DateOfBirth] [date] NOT NULL,
	[GuardianID] [int] NOT NULL,
	[SectionID] [int] NOT NULL,
	[AcademicYearID] [int] NOT NULL,
	[Status] [nvarchar](20) NULL,
	[PhotoPath] [nvarchar](500) NULL,
	[MedicalNotes] [nvarchar](500) NULL,
	[Address] [nvarchar](200) NULL,
	[EnrollmentDate] [date] NULL,
	[CreatedAt] [datetime] NULL,
	[UpdatedAt] [datetime] NULL,
	[Shift] [nvarchar](20) NULL,
PRIMARY KEY CLUSTERED 
(
	[StudentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[StudentCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[AdmissionNo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Students] ADD  DEFAULT ('Active') FOR [Status]
ALTER TABLE [dbo].[Students] ADD  DEFAULT (getdate()) FOR [EnrollmentDate]
ALTER TABLE [dbo].[Students] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[Students] ADD  DEFAULT (getdate()) FOR [UpdatedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[StudentTransfers](
	[StudentTransferID] [int] IDENTITY(1,1) NOT NULL,
	[StudentID] [int] NOT NULL,
	[TransferType] [nvarchar](30) NOT NULL,
	[FromAcademicYearID] [int] NULL,
	[FromSectionID] [int] NULL,
	[DestinationSchool] [nvarchar](150) NULL,
	[DestinationLocation] [nvarchar](200) NULL,
	[DestinationContactPerson] [nvarchar](100) NULL,
	[DestinationPhone] [nvarchar](30) NULL,
	[TransferDate] [date] NOT NULL,
	[TransferReason] [nvarchar](300) NOT NULL,
	[TransferCertificateNo] [nvarchar](50) NULL,
	[TransferNotes] [nvarchar](500) NULL,
	[TransferStatus] [nvarchar](20) NOT NULL,
	[ReturnedDate] [date] NULL,
	[ReturnAcademicYearID] [int] NULL,
	[ReturnSectionID] [int] NULL,
	[ReturnReason] [nvarchar](300) NULL,
	[ReturnNotes] [nvarchar](500) NULL,
	[TransferredBy] [int] NULL,
	[ReturnedBy] [int] NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[StudentTransferID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[StudentTransfers] ADD  CONSTRAINT [DF_StudentTransfers_TransferType]  DEFAULT ('External Transfer') FOR [TransferType]
ALTER TABLE [dbo].[StudentTransfers] ADD  CONSTRAINT [DF_StudentTransfers_Status]  DEFAULT ('Active') FOR [TransferStatus]
ALTER TABLE [dbo].[StudentTransfers] ADD  CONSTRAINT [DF_StudentTransfers_CreatedAt]  DEFAULT (sysdatetime()) FOR [CreatedAt]
ALTER TABLE [dbo].[StudentTransfers] ADD  CONSTRAINT [DF_StudentTransfers_UpdatedAt]  DEFAULT (sysdatetime()) FOR [UpdatedAt]
ALTER TABLE [dbo].[StudentTransfers]  WITH CHECK ADD  CONSTRAINT [CK_StudentTransfers_Status] CHECK  (([TransferStatus]='Cancelled' OR [TransferStatus]='Returned' OR [TransferStatus]='Active'))
ALTER TABLE [dbo].[StudentTransfers] CHECK CONSTRAINT [CK_StudentTransfers_Status]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Subjects](
	[SubjectID] [int] IDENTITY(1,1) NOT NULL,
	[SubjectName] [nvarchar](100) NOT NULL,
	[SubjectCode] [nvarchar](20) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[IsActive] [bit] NULL,
	[SubjectType] [nvarchar](20) NOT NULL,
	[MaxMarks] [int] NOT NULL,
	[PassMarks] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[SubjectID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[SubjectCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Subjects] ADD  DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[Subjects] ADD  CONSTRAINT [DF_Subjects_Type]  DEFAULT ('Core') FOR [SubjectType]
ALTER TABLE [dbo].[Subjects] ADD  CONSTRAINT [DF_Subjects_Max]  DEFAULT ((100)) FOR [MaxMarks]
ALTER TABLE [dbo].[Subjects] ADD  CONSTRAINT [DF_Subjects_Pass]  DEFAULT ((50)) FOR [PassMarks]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Terms](
	[TermID] [int] IDENTITY(1,1) NOT NULL,
	[AcademicYearID] [int] NOT NULL,
	[TermName] [nvarchar](50) NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NOT NULL,
	[Status] [nvarchar](20) NULL,
	[IsCurrentTerm] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[TermID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Terms] ADD  DEFAULT ('Upcoming') FOR [Status]
ALTER TABLE [dbo].[Terms] ADD  DEFAULT ((0)) FOR [IsCurrentTerm]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Timetable](
	[TimetableID] [int] IDENTITY(1,1) NOT NULL,
	[SectionID] [int] NOT NULL,
	[SubjectID] [int] NOT NULL,
	[StaffID] [int] NOT NULL,
	[DayOfWeek] [int] NOT NULL,
	[PeriodNo] [int] NOT NULL,
	[StartTime] [time](7) NOT NULL,
	[EndTime] [time](7) NOT NULL,
	[RoomNumber] [nvarchar](20) NULL,
	[AcademicYearID] [int] NOT NULL,
	[IsActive] [bit] NULL,
	[TermID] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[TimetableID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Timetable] ADD  DEFAULT ((1)) FOR [IsActive]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[UserRoles](
	[UserRoleID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[RoleID] [int] NOT NULL,
	[AssignedBy] [int] NOT NULL,
	[AssignedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED 
(
	[UserRoleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[UserRoles] ADD  CONSTRAINT [DF_UserRoles_AssignedAt]  DEFAULT (getdate()) FOR [AssignedAt]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Users](
	[UserID] [int] IDENTITY(1,1) NOT NULL,
	[FullName] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](100) NOT NULL,
	[PasswordHash] [nvarchar](256) NOT NULL,
	[Phone] [nvarchar](20) NULL,
	[Role] [nvarchar](50) NOT NULL,
	[IsActive] [bit] NULL,
	[LastLogin] [datetime] NULL,
	[CreatedAt] [datetime] NULL,
	[UpdatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Users] ADD  DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getdate()) FOR [CreatedAt]
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getdate()) FOR [UpdatedAt]