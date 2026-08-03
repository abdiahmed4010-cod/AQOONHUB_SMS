SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH('dbo.Announcements','Priority') IS NULL ALTER TABLE dbo.Announcements ADD Priority nvarchar(20) NOT NULL CONSTRAINT DF_Announcements_Priority DEFAULT('Normal');
IF COL_LENGTH('dbo.Announcements','AudienceType') IS NULL ALTER TABLE dbo.Announcements ADD AudienceType nvarchar(30) NOT NULL CONSTRAINT DF_Announcements_AudienceType DEFAULT('All Users');
IF COL_LENGTH('dbo.Announcements','TargetSectionID') IS NULL ALTER TABLE dbo.Announcements ADD TargetSectionID int NULL;
IF COL_LENGTH('dbo.Announcements','TargetRole') IS NULL ALTER TABLE dbo.Announcements ADD TargetRole nvarchar(50) NULL;
IF COL_LENGTH('dbo.Announcements','PublishDate') IS NULL ALTER TABLE dbo.Announcements ADD PublishDate datetime2(0) NULL;
IF COL_LENGTH('dbo.Announcements','ExpiryDate') IS NULL ALTER TABLE dbo.Announcements ADD ExpiryDate datetime2(0) NULL;
IF COL_LENGTH('dbo.Announcements','Status') IS NULL ALTER TABLE dbo.Announcements ADD Status nvarchar(20) NOT NULL CONSTRAINT DF_Announcements_Status DEFAULT('Draft');
IF COL_LENGTH('dbo.Announcements','IsPublished') IS NULL ALTER TABLE dbo.Announcements ADD IsPublished bit NOT NULL CONSTRAINT DF_Announcements_IsPublished DEFAULT(0);
IF COL_LENGTH('dbo.Announcements','UpdatedAt') IS NULL ALTER TABLE dbo.Announcements ADD UpdatedAt datetime2(0) NULL;
GO

IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_Announcements_TargetSection') ALTER TABLE dbo.Announcements WITH CHECK ADD CONSTRAINT FK_Announcements_TargetSection FOREIGN KEY(TargetSectionID) REFERENCES dbo.Sections(SectionID);
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name='CK_Announcements_Priority') ALTER TABLE dbo.Announcements WITH CHECK ADD CONSTRAINT CK_Announcements_Priority CHECK(Priority IN('Normal','Important','Urgent'));
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name='CK_Announcements_Status') ALTER TABLE dbo.Announcements WITH CHECK ADD CONSTRAINT CK_Announcements_Status CHECK(Status IN('Draft','Scheduled','Published','Expired','Unpublished','Cancelled'));

IF OBJECT_ID('dbo.AnnouncementRecipients','U') IS NULL CREATE TABLE dbo.AnnouncementRecipients(
 AnnouncementRecipientID bigint IDENTITY PRIMARY KEY, AnnouncementID int NOT NULL, RecipientType nvarchar(30) NOT NULL,
 RecipientID int NULL, UserID int NULL, IsRead bit NOT NULL CONSTRAINT DF_AR_IsRead DEFAULT(0), ReadAt datetime2(0) NULL, CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_AR_Created DEFAULT(SYSDATETIME()),
 CONSTRAINT FK_AR_Announcement FOREIGN KEY(AnnouncementID) REFERENCES dbo.Announcements(AnnouncementID) ON DELETE CASCADE,
 CONSTRAINT FK_AR_User FOREIGN KEY(UserID) REFERENCES dbo.Users(UserID));
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.AnnouncementRecipients') AND name='UX_AR_Announcement_User') CREATE UNIQUE INDEX UX_AR_Announcement_User ON dbo.AnnouncementRecipients(AnnouncementID,UserID) WHERE UserID IS NOT NULL;
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.AnnouncementRecipients') AND name='UX_AR_Announcement_Recipient') CREATE UNIQUE INDEX UX_AR_Announcement_Recipient ON dbo.AnnouncementRecipients(AnnouncementID,RecipientType,RecipientID) WHERE RecipientID IS NOT NULL;

IF OBJECT_ID('dbo.MessageThreads','U') IS NULL CREATE TABLE dbo.MessageThreads(
 ThreadID bigint IDENTITY PRIMARY KEY, Subject nvarchar(200) NOT NULL, CreatedByUserID int NOT NULL, CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_MT_Created DEFAULT(SYSDATETIME()), LastMessageAt datetime2(0) NOT NULL CONSTRAINT DF_MT_Last DEFAULT(SYSDATETIME()), IsArchived bit NOT NULL CONSTRAINT DF_MT_Archived DEFAULT(0),
 CONSTRAINT FK_MT_User FOREIGN KEY(CreatedByUserID) REFERENCES dbo.Users(UserID));
IF OBJECT_ID('dbo.Messages','U') IS NULL CREATE TABLE dbo.Messages(
 MessageID bigint IDENTITY PRIMARY KEY, ThreadID bigint NOT NULL, SenderUserID int NOT NULL, Body nvarchar(4000) NOT NULL, ParentMessageID bigint NULL, CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_M_Created DEFAULT(SYSDATETIME()), IsDraft bit NOT NULL CONSTRAINT DF_M_Draft DEFAULT(0),
 CONSTRAINT FK_M_Thread FOREIGN KEY(ThreadID) REFERENCES dbo.MessageThreads(ThreadID), CONSTRAINT FK_M_Sender FOREIGN KEY(SenderUserID) REFERENCES dbo.Users(UserID), CONSTRAINT FK_M_Parent FOREIGN KEY(ParentMessageID) REFERENCES dbo.Messages(MessageID));
IF OBJECT_ID('dbo.MessageRecipients','U') IS NULL CREATE TABLE dbo.MessageRecipients(
 MessageRecipientID bigint IDENTITY PRIMARY KEY, MessageID bigint NOT NULL, RecipientUserID int NOT NULL, IsRead bit NOT NULL CONSTRAINT DF_MR_Read DEFAULT(0), ReadAt datetime2(0) NULL, IsArchived bit NOT NULL CONSTRAINT DF_MR_Archived DEFAULT(0), DeletedByRecipient bit NOT NULL CONSTRAINT DF_MR_Deleted DEFAULT(0),
 CONSTRAINT FK_MR_Message FOREIGN KEY(MessageID) REFERENCES dbo.Messages(MessageID) ON DELETE CASCADE, CONSTRAINT FK_MR_User FOREIGN KEY(RecipientUserID) REFERENCES dbo.Users(UserID), CONSTRAINT UQ_MR_Message_User UNIQUE(MessageID,RecipientUserID));

IF OBJECT_ID('dbo.CommunicationTemplates','U') IS NULL CREATE TABLE dbo.CommunicationTemplates(
 TemplateID int IDENTITY PRIMARY KEY, Name nvarchar(120) NOT NULL, Channel nvarchar(10) NOT NULL, SubjectTemplate nvarchar(200) NULL, BodyTemplate nvarchar(2000) NOT NULL, Category nvarchar(50) NOT NULL, IsActive bit NOT NULL CONSTRAINT DF_CT_Active DEFAULT(1), CreatedByUserID int NOT NULL, CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_CT_Created DEFAULT(SYSDATETIME()), UpdatedAt datetime2(0) NULL,
 CONSTRAINT CK_CT_Channel CHECK(Channel IN('SMS','Email')), CONSTRAINT FK_CT_User FOREIGN KEY(CreatedByUserID) REFERENCES dbo.Users(UserID));
IF OBJECT_ID('dbo.CommunicationCampaigns','U') IS NULL CREATE TABLE dbo.CommunicationCampaigns(
 CampaignID bigint IDENTITY PRIMARY KEY, Channel nvarchar(10) NOT NULL, Subject nvarchar(200) NULL, MessageBody nvarchar(2000) NOT NULL, AudienceType nvarchar(40) NOT NULL, AudienceValue nvarchar(100) NULL, RecipientCount int NOT NULL CONSTRAINT DF_CC_Count DEFAULT(0), ScheduleAt datetime2(0) NULL, Status nvarchar(60) NOT NULL, CreatedByUserID int NOT NULL, CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_CC_Created DEFAULT(SYSDATETIME()), UpdatedAt datetime2(0) NULL,
 CONSTRAINT CK_CC_Channel CHECK(Channel IN('SMS','Email')), CONSTRAINT CK_CC_Status CHECK(Status IN('Draft','Pending','Pending Scheduler Configuration','Sent','Failed','Cancelled')), CONSTRAINT FK_CC_User FOREIGN KEY(CreatedByUserID) REFERENCES dbo.Users(UserID));
IF OBJECT_ID('dbo.CommunicationDeliveries','U') IS NULL CREATE TABLE dbo.CommunicationDeliveries(
 DeliveryID bigint IDENTITY PRIMARY KEY, CampaignID bigint NULL, UserID int NULL, RecipientAddress nvarchar(200) NOT NULL, Channel nvarchar(10) NOT NULL, Subject nvarchar(200) NULL, MessagePreview nvarchar(300) NULL, ProviderName nvarchar(100) NULL, Status nvarchar(20) NOT NULL, AttemptCount int NOT NULL CONSTRAINT DF_CD_Attempts DEFAULT(0), SentAt datetime2(0) NULL, DeliveredAt datetime2(0) NULL, ReadAt datetime2(0) NULL, FailureReason nvarchar(500) NULL, ProviderReference nvarchar(200) NULL, CreatedByUserID int NOT NULL, CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_CD_Created DEFAULT(SYSDATETIME()), UpdatedAt datetime2(0) NULL,
 CONSTRAINT CK_CD_Channel CHECK(Channel IN('SMS','Email','In-App')), CONSTRAINT CK_CD_Status CHECK(Status IN('Pending','Sent','Delivered','Read','Failed','Cancelled')), CONSTRAINT FK_CD_Campaign FOREIGN KEY(CampaignID) REFERENCES dbo.CommunicationCampaigns(CampaignID), CONSTRAINT FK_CD_User FOREIGN KEY(UserID) REFERENCES dbo.Users(UserID), CONSTRAINT FK_CD_CreatedBy FOREIGN KEY(CreatedByUserID) REFERENCES dbo.Users(UserID));
IF OBJECT_ID('dbo.CommunicationAttachments','U') IS NULL CREATE TABLE dbo.CommunicationAttachments(
 AttachmentID bigint IDENTITY PRIMARY KEY, AnnouncementID int NULL, MessageID bigint NULL, OriginalFileName nvarchar(255) NOT NULL, StoredFileName nvarchar(255) NOT NULL, RelativePath nvarchar(400) NOT NULL, ContentType nvarchar(100) NOT NULL, FileSize bigint NOT NULL, UploadedByUserID int NOT NULL, UploadedAt datetime2(0) NOT NULL CONSTRAINT DF_CA_Uploaded DEFAULT(SYSDATETIME()),
 CONSTRAINT CK_CA_OneOwner CHECK((CASE WHEN AnnouncementID IS NULL THEN 0 ELSE 1 END)+(CASE WHEN MessageID IS NULL THEN 0 ELSE 1 END)=1), CONSTRAINT FK_CA_Announcement FOREIGN KEY(AnnouncementID) REFERENCES dbo.Announcements(AnnouncementID) ON DELETE CASCADE, CONSTRAINT FK_CA_Message FOREIGN KEY(MessageID) REFERENCES dbo.Messages(MessageID) ON DELETE CASCADE, CONSTRAINT FK_CA_User FOREIGN KEY(UploadedByUserID) REFERENCES dbo.Users(UserID));
GO

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Announcements') AND name='IX_Announcements_Status_Publish') CREATE INDEX IX_Announcements_Status_Publish ON dbo.Announcements(Status,PublishDate DESC);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.MessageRecipients') AND name='IX_MR_User_State') CREATE INDEX IX_MR_User_State ON dbo.MessageRecipients(RecipientUserID,IsArchived,IsRead) INCLUDE(MessageID);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Messages') AND name='IX_Messages_Thread_Date') CREATE INDEX IX_Messages_Thread_Date ON dbo.Messages(ThreadID,CreatedAt);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.CommunicationCampaigns') AND name='IX_CC_Status_Schedule') CREATE INDEX IX_CC_Status_Schedule ON dbo.CommunicationCampaigns(Status,ScheduleAt);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.CommunicationDeliveries') AND name='IX_CD_Status_Date') CREATE INDEX IX_CD_Status_Date ON dbo.CommunicationDeliveries(Status,CreatedAt DESC) INCLUDE(Channel,CreatedByUserID);
COMMIT;
