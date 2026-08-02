SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

-- ----------------------------------------------------------------------------
-- 7. sp_GetAttendanceByClass
-- Returns today's attendance summary grouped by class
-- Uses existing tables: Attendance, Students, Classes, Sections
-- ----------------------------------------------------------------------------
GO
CREATE   PROCEDURE dbo.sp_GetAttendanceByClass
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.ClassID,
        c.ClassName,
        c.RoomNumber,
        COUNT(DISTINCT s.StudentID) AS TotalStudents,
        ISNULL(SUM(CASE WHEN a.Status = 'Present' THEN 1 ELSE 0 END), 0) AS PresentCount,
        ISNULL(SUM(CASE WHEN a.Status = 'Absent' THEN 1 ELSE 0 END), 0) AS AbsentCount,
        ISNULL(SUM(CASE WHEN a.Status = 'Late' THEN 1 ELSE 0 END), 0) AS LateCount,
        ISNULL(SUM(CASE WHEN a.Status = 'Excused' THEN 1 ELSE 0 END), 0) AS ExcusedCount,
        ISNULL(SUM(CASE WHEN a.AttendanceID IS NULL THEN 1 ELSE 0 END), 0) AS NotMarkedCount
    FROM dbo.Classes c
    LEFT JOIN dbo.Sections sec ON sec.ClassID = c.ClassID
    LEFT JOIN dbo.Students s ON s.SectionID = sec.SectionID AND s.Status = 'Active'
    LEFT JOIN dbo.Attendance a ON a.StudentID = s.StudentID 
                              AND a.AttendanceDate = CAST(GETDATE() AS DATE)
    GROUP BY c.ClassID, c.ClassName, c.RoomNumber
    ORDER BY c.ClassName;
END
GO

SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
-- ============================================================================
-- AQOONHUB_SMS Dashboard Stored Procedures (FIXED)
-- ============================================================================
-- Rules followed:
--   - NO new tables created
--   - NO DAL or BLL modifications
--   - Students.CreatedAt used instead of non-existent AdmissionDate
--   - Notifications seed data includes CreatedBy (NOT NULL column)
--   - All output columns match DashboardDAL exactly
-- ============================================================================

-- ----------------------------------------------------------------------------
-- 1. sp_GetDashboardStats
-- Returns comprehensive dashboard statistics in a single row
-- ----------------------------------------------------------------------------
GO
CREATE   PROCEDURE dbo.sp_GetDashboardStats
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        -- Student Statistics
        ISNULL((SELECT COUNT(*) FROM dbo.Students), 0) AS TotalStudents,
        ISNULL((SELECT COUNT(*) FROM dbo.Students WHERE Status = 'Active'), 0) AS ActiveStudents,
        ISNULL((SELECT COUNT(*) FROM dbo.Students WHERE Status = 'Suspended'), 0) AS SuspendedStudents,
        ISNULL((SELECT COUNT(*) FROM dbo.Students 
                WHERE CreatedAt >= DATEADD(day, -30, CAST(GETDATE() AS DATE)) 
                  AND CreatedAt < DATEADD(day, 1, CAST(GETDATE() AS DATE))), 0) AS NewAdmissions,

        -- Staff Statistics
        ISNULL((SELECT COUNT(*) FROM dbo.Staff), 0) AS TotalStaff,
        ISNULL((SELECT COUNT(*) FROM dbo.Staff WHERE Status = 'Active'), 0) AS ActiveStaff,
        ISNULL((SELECT COUNT(*) FROM dbo.Staff WHERE Status = 'OnLeave'), 0) AS OnLeaveStaff,

        -- Fee Statistics (from Invoices and Payments)
        ISNULL((SELECT SUM(TotalAmount) FROM dbo.Invoices), 0) AS TotalBilled,
        ISNULL((SELECT SUM(Amount) FROM dbo.Payments), 0) AS TotalCollected,
        ISNULL((SELECT SUM(TotalAmount - PaidAmount) FROM dbo.Invoices WHERE Status IN ('Pending', 'Partial', 'Overdue')), 0) AS TotalOutstanding,

        -- Attendance Statistics (for today)
        ISNULL((SELECT COUNT(*) FROM dbo.Attendance 
                WHERE AttendanceDate = CAST(GETDATE() AS DATE) 
                  AND Status = 'Present'), 0) AS PresentToday,
        ISNULL((SELECT COUNT(*) FROM dbo.Attendance 
                WHERE AttendanceDate = CAST(GETDATE() AS DATE) 
                  AND Status = 'Absent'), 0) AS AbsentToday,
        ISNULL((SELECT COUNT(*) FROM dbo.Attendance 
                WHERE AttendanceDate = CAST(GETDATE() AS DATE) 
                  AND Status = 'Late'), 0) AS LateToday,

        -- Exam Statistics
        ISNULL((SELECT COUNT(*) FROM dbo.Exams 
                WHERE StartDate >= CAST(GETDATE() AS DATE) 
                  AND StartDate <= DATEADD(day, 30, CAST(GETDATE() AS DATE))
                  AND Status IN ('Scheduled', 'Upcoming')), 0) AS UpcomingExams,
        ISNULL((SELECT COUNT(*) FROM dbo.Exams 
                WHERE Status = 'Active'), 0) AS ActiveExams,

        -- Application Statistics
        ISNULL((SELECT COUNT(*) FROM dbo.Applications WHERE Status = 'Pending'), 0) AS PendingApplications;
END
GO

SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

-- ----------------------------------------------------------------------------
-- 6. sp_GetFeeCollectionSummary
-- Returns monthly fee collection summary for chart data
-- Parameter: @Months INT
-- Uses existing Payments table columns only:
-- PaymentID, ReceiptNo, InvoiceID, Amount, PaymentMethod, PaymentDate, ReceivedBy, Notes, CreatedAt
-- ----------------------------------------------------------------------------
GO
CREATE   PROCEDURE dbo.sp_GetFeeCollectionSummary
    @Months INT
AS
BEGIN
    SET NOCOUNT ON;

    WITH MonthSeries AS (
        SELECT 
            DATEADD(month, -n, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)) AS MonthStart,
            n AS MonthOffset
        FROM (VALUES (0),(1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12),(13),(14),(15),(16),(17),(18),(19),(20),(21),(22),(23)) AS Numbers(n)
        WHERE n < @Months
    )
    SELECT
        ms.MonthStart AS MonthDate,
        DATENAME(month, ms.MonthStart) + ' ' + CAST(YEAR(ms.MonthStart) AS VARCHAR(4)) AS MonthName,
        ISNULL(SUM(p.Amount), 0) AS TotalCollected,
        COUNT(p.PaymentID) AS PaymentCount
    FROM MonthSeries ms
    LEFT JOIN dbo.Payments p 
        ON p.PaymentDate >= ms.MonthStart 
       AND p.PaymentDate < DATEADD(month, 1, ms.MonthStart)
    GROUP BY ms.MonthStart, ms.MonthOffset
    ORDER BY ms.MonthStart ASC;
END
GO

SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

-- ----------------------------------------------------------------------------
-- 2. sp_GetRecentActivities
-- Returns recent activities for the dashboard activity feed
-- Parameter: @Count INT
-- ----------------------------------------------------------------------------
GO
CREATE   PROCEDURE dbo.sp_GetRecentActivities
    @Count INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Count)
        ActivityType,
        Description,
        ActivityDate,
        PerformedBy
    FROM dbo.Activities
    ORDER BY ActivityDate DESC;
END
GO

SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

-- ----------------------------------------------------------------------------
-- 5. sp_GetUpcomingEvents
-- Returns upcoming events within the specified days ahead
-- Parameter: @DaysAhead INT
-- Uses existing Events table columns only:
-- EventID, Title, EventDate, EventType, Description, CreatedBy, CreatedAt
-- ----------------------------------------------------------------------------
GO
CREATE   PROCEDURE dbo.sp_GetUpcomingEvents
    @DaysAhead INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        EventID,
        Title,
        EventDate,
        EventType,
        Description,
        CreatedBy,
        CreatedAt
    FROM dbo.Events
    WHERE EventDate >= CAST(GETDATE() AS DATE)
      AND EventDate <= DATEADD(day, @DaysAhead, CAST(GETDATE() AS DATE))
    ORDER BY EventDate ASC;
END
GO

SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

-- ----------------------------------------------------------------------------
-- 4. sp_GetUpcomingExams
-- Returns upcoming exams within the specified days ahead
-- Parameter: @DaysAhead INT
-- Uses existing Exams table columns only:
-- ExamID, ExamName, ExamType, TermID, StartDate, EndDate, Status, CreatedBy, CreatedAt
-- ----------------------------------------------------------------------------
GO
CREATE   PROCEDURE dbo.sp_GetUpcomingExams
    @DaysAhead INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ExamID,
        ExamName,
        ExamType,
        TermID,
        StartDate,
        EndDate,
        Status,
        CreatedBy,
        CreatedAt
    FROM dbo.Exams
    WHERE StartDate >= CAST(GETDATE() AS DATE)
      AND StartDate <= DATEADD(day, @DaysAhead, CAST(GETDATE() AS DATE))
      AND Status IN ('Scheduled', 'Upcoming', 'Active')
    ORDER BY StartDate ASC;
END
GO

SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

-- ----------------------------------------------------------------------------
-- 3. sp_GetUserNotifications
-- Returns notifications for a specific user
-- Parameters: @UserID INT, @Count INT
-- ----------------------------------------------------------------------------
GO
CREATE   PROCEDURE dbo.sp_GetUserNotifications
    @UserID INT,
    @Count INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Count)
        NotificationID,
        UserID,
        Title,
        Message,
        NotificationType,
        Priority,
        IsRead,
        ReadAt,
        LinkUrl,
        Icon,
        CreatedAt
    FROM dbo.Notifications
    WHERE UserID = @UserID
    ORDER BY CreatedAt DESC;
END
