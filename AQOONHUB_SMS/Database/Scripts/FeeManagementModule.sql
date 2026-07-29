/*
 AQOONHUB SMS - Fee Management Module
 Idempotent, non-destructive migration for SQL Server.
 Existing Students, Classes, Sections, AcademicYears and Users tables are reused.
*/
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID('dbo.FeeCategories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.FeeCategories (
        FeeCategoryID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FeeCategories PRIMARY KEY,
        CategoryName NVARCHAR(100) NOT NULL,
        CategoryCode NVARCHAR(20) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        DefaultBillingTerm NVARCHAR(20) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_FeeCategories_IsActive DEFAULT (1),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_FeeCategories_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2(0) NULL,
        CONSTRAINT CK_FeeCategories_BillingTerm CHECK (DefaultBillingTerm IN ('Monthly','Per Term','Annual','One Time'))
    );
    CREATE UNIQUE INDEX UX_FeeCategories_Name ON dbo.FeeCategories(CategoryName);
    CREATE UNIQUE INDEX UX_FeeCategories_Code ON dbo.FeeCategories(CategoryCode);
END;

IF OBJECT_ID('dbo.ClassFeeStructures', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClassFeeStructures (
        ClassFeeStructureID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ClassFeeStructures PRIMARY KEY,
        AcademicYearID INT NOT NULL,
        ClassID INT NOT NULL,
        SectionID INT NULL,
        FeeCategoryID INT NOT NULL,
        BillingTerm NVARCHAR(20) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        DiscountType NVARCHAR(20) NOT NULL CONSTRAINT DF_ClassFeeStructures_DiscountType DEFAULT ('No Discount'),
        DiscountAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_ClassFeeStructures_DiscountAmount DEFAULT (0),
        [Description] NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_ClassFeeStructures_IsActive DEFAULT (1),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ClassFeeStructures_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2(0) NULL,
        CONSTRAINT FK_ClassFeeStructures_AcademicYears FOREIGN KEY (AcademicYearID) REFERENCES dbo.AcademicYears(AcademicYearID),
        CONSTRAINT FK_ClassFeeStructures_Classes FOREIGN KEY (ClassID) REFERENCES dbo.Classes(ClassID),
        CONSTRAINT FK_ClassFeeStructures_Sections FOREIGN KEY (SectionID) REFERENCES dbo.Sections(SectionID),
        CONSTRAINT FK_ClassFeeStructures_Categories FOREIGN KEY (FeeCategoryID) REFERENCES dbo.FeeCategories(FeeCategoryID),
        CONSTRAINT CK_ClassFeeStructures_Amount CHECK (Amount >= 0),
        CONSTRAINT CK_ClassFeeStructures_BillingTerm CHECK (BillingTerm IN ('Monthly','Per Term','Annual','One Time')),
        CONSTRAINT CK_ClassFeeStructures_DiscountType CHECK (DiscountType IN ('No Discount','Fixed Amount','Percentage')),
        CONSTRAINT CK_ClassFeeStructures_Discount CHECK (DiscountAmount >= 0)
    );
    CREATE UNIQUE INDEX UX_ClassFeeStructures_Active
        ON dbo.ClassFeeStructures(AcademicYearID, ClassID, SectionID, FeeCategoryID)
        WHERE IsActive = 1;
    CREATE INDEX IX_ClassFeeStructures_Lookup
        ON dbo.ClassFeeStructures(AcademicYearID, ClassID, SectionID, IsActive);
END;

IF OBJECT_ID('dbo.FeeInvoices', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.FeeInvoices (
        InvoiceID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FeeInvoices PRIMARY KEY,
        InvoiceNumber NVARCHAR(30) NOT NULL,
        StudentID INT NOT NULL,
        AcademicYearID INT NOT NULL,
        InvoiceDate DATE NOT NULL,
        DueDate DATE NOT NULL,
        InvoiceType NVARCHAR(50) NOT NULL,
        Subtotal DECIMAL(18,2) NOT NULL,
        DiscountAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_FeeInvoices_Discount DEFAULT (0),
        TotalAmount DECIMAL(18,2) NOT NULL,
        PaidAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_FeeInvoices_Paid DEFAULT (0),
        [Status] NVARCHAR(20) NOT NULL CONSTRAINT DF_FeeInvoices_Status DEFAULT ('Unpaid'),
        Remarks NVARCHAR(1000) NULL,
        PaymentInstructions NVARCHAR(1000) NULL,
        CreatedBy INT NOT NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_FeeInvoices_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2(0) NULL,
        CONSTRAINT UX_FeeInvoices_Number UNIQUE (InvoiceNumber),
        CONSTRAINT FK_FeeInvoices_Students FOREIGN KEY (StudentID) REFERENCES dbo.Students(StudentID),
        CONSTRAINT FK_FeeInvoices_AcademicYears FOREIGN KEY (AcademicYearID) REFERENCES dbo.AcademicYears(AcademicYearID),
        CONSTRAINT FK_FeeInvoices_Users FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(UserID),
        CONSTRAINT CK_FeeInvoices_Amounts CHECK (Subtotal >= 0 AND DiscountAmount >= 0 AND TotalAmount >= 0 AND PaidAmount >= 0 AND PaidAmount <= TotalAmount),
        CONSTRAINT CK_FeeInvoices_Status CHECK ([Status] IN ('Unpaid','Partial','Paid','Overdue','Cancelled'))
    );
    CREATE INDEX IX_FeeInvoices_StudentStatus ON dbo.FeeInvoices(StudentID, [Status], DueDate);
    CREATE INDEX IX_FeeInvoices_AcademicYear ON dbo.FeeInvoices(AcademicYearID, InvoiceDate);
END;

IF OBJECT_ID('dbo.FeeInvoiceItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.FeeInvoiceItems (
        InvoiceItemID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FeeInvoiceItems PRIMARY KEY,
        InvoiceID INT NOT NULL,
        FeeCategoryID INT NULL,
        FeeCategoryName NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        Amount DECIMAL(18,2) NOT NULL,
        DiscountAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_FeeInvoiceItems_Discount DEFAULT (0),
        TotalAmount DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_FeeInvoiceItems_Invoices FOREIGN KEY (InvoiceID) REFERENCES dbo.FeeInvoices(InvoiceID),
        CONSTRAINT FK_FeeInvoiceItems_Categories FOREIGN KEY (FeeCategoryID) REFERENCES dbo.FeeCategories(FeeCategoryID),
        CONSTRAINT CK_FeeInvoiceItems_Amounts CHECK (Amount >= 0 AND DiscountAmount >= 0 AND TotalAmount >= 0)
    );
    CREATE INDEX IX_FeeInvoiceItems_Invoice ON dbo.FeeInvoiceItems(InvoiceID);
END;

IF OBJECT_ID('dbo.FeePayments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.FeePayments (
        PaymentID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FeePayments PRIMARY KEY,
        InvoiceID INT NOT NULL,
        StudentID INT NOT NULL,
        ReceiptNumber NVARCHAR(30) NOT NULL,
        AmountPaid DECIMAL(18,2) NOT NULL,
        PaymentMethod NVARCHAR(30) NOT NULL,
        PaymentDate DATE NOT NULL,
        ReferenceNumber NVARCHAR(100) NULL,
        Notes NVARCHAR(1000) NULL,
        PreviousBalance DECIMAL(18,2) NOT NULL,
        NewBalance DECIMAL(18,2) NOT NULL,
        ReceivedBy INT NOT NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_FeePayments_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UX_FeePayments_Receipt UNIQUE (ReceiptNumber),
        CONSTRAINT FK_FeePayments_Invoices FOREIGN KEY (InvoiceID) REFERENCES dbo.FeeInvoices(InvoiceID),
        CONSTRAINT FK_FeePayments_Students FOREIGN KEY (StudentID) REFERENCES dbo.Students(StudentID),
        CONSTRAINT FK_FeePayments_Users FOREIGN KEY (ReceivedBy) REFERENCES dbo.Users(UserID),
        CONSTRAINT CK_FeePayments_Amount CHECK (AmountPaid > 0),
        CONSTRAINT CK_FeePayments_Method CHECK (PaymentMethod IN ('Cash','Mobile Money','Bank Transfer','Card','Cheque','Other'))
    );
    CREATE INDEX IX_FeePayments_Invoice ON dbo.FeePayments(InvoiceID, PaymentDate);
    CREATE INDEX IX_FeePayments_Date ON dbo.FeePayments(PaymentDate);
END;

COMMIT TRANSACTION;
