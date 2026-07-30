/* =====================================================================
   AQOONHUB_SMS - Examinations Stage 6
   Historical result integrity + immutable published snapshots + index fix.
   Idempotent, non-destructive, FK-protected, safely rerunnable. No DROPs
   of data. Safe to run multiple times.
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

/* ---------------------------------------------------------------------
   1) StudentExamSummaries : immutable per-student result snapshot.
      Written at calculation/publication. Report cards + published
      Results read this, so promoting/moving a student never alters an
      already-published result.
   --------------------------------------------------------------------- */
IF OBJECT_ID('dbo.StudentExamSummaries','U') IS NULL
CREATE TABLE dbo.StudentExamSummaries (
    StudentExamSummaryID int IDENTITY(1,1) PRIMARY KEY,
    ExamID              int            NOT NULL,
    StudentID           int            NOT NULL,
    AcademicYearID      int            NULL,
    ClassID             int            NULL,
    SectionID           int            NULL,
    TotalObtained       decimal(9,2)   NOT NULL CONSTRAINT DF_SES_Obt  DEFAULT(0),
    TotalMaximum        decimal(9,2)   NOT NULL CONSTRAINT DF_SES_Max  DEFAULT(0),
    AveragePercentage   decimal(6,2)   NOT NULL CONSTRAINT DF_SES_Avg  DEFAULT(0),
    OverallGrade        nvarchar(10)   NULL,
    Rank                int            NULL,
    ResultStatus        nvarchar(20)   NULL,
    PublicationStatus   nvarchar(20)   NOT NULL CONSTRAINT DF_SES_Pub  DEFAULT('Draft'),
    CalculatedBy        int            NULL,
    CalculatedAt        datetime       NULL,
    PublishedBy         int            NULL,
    PublishedAt         datetime       NULL,
    CreatedAt           datetime       NOT NULL CONSTRAINT DF_SES_Cre  DEFAULT(GETDATE()),
    UpdatedAt           datetime       NULL
);
GO

/* Unique business key: one summary row per exam+student */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_SES_ExamStudent' AND object_id=OBJECT_ID('dbo.StudentExamSummaries'))
   AND NOT EXISTS (SELECT ExamID FROM dbo.StudentExamSummaries GROUP BY ExamID, StudentID HAVING COUNT(*)>1)
    CREATE UNIQUE INDEX UX_SES_ExamStudent ON dbo.StudentExamSummaries(ExamID, StudentID);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_SES_ClassSection' AND object_id=OBJECT_ID('dbo.StudentExamSummaries'))
    CREATE INDEX IX_SES_ClassSection ON dbo.StudentExamSummaries(ExamID, ClassID, SectionID);
GO

/* FKs (WITH NOCHECK so existing rows are never blocked) */
IF OBJECT_ID('dbo.FK_SES_Exam','F')    IS NULL ALTER TABLE dbo.StudentExamSummaries WITH NOCHECK ADD CONSTRAINT FK_SES_Exam    FOREIGN KEY (ExamID)    REFERENCES dbo.Exams(ExamID);
IF OBJECT_ID('dbo.FK_SES_Student','F') IS NULL ALTER TABLE dbo.StudentExamSummaries WITH NOCHECK ADD CONSTRAINT FK_SES_Student FOREIGN KEY (StudentID) REFERENCES dbo.Students(StudentID);
GO

/* ---------------------------------------------------------------------
   2) Fix ResultPublications uniqueness so republish is possible.
      Old index UX_ResultPub_Unique filtered only on SectionID IS NOT NULL
      (NOT on Status), which blocks a fresh Published row after a prior row
      is set to Unpublished. Replace with Status='Published' filtered
      indexes so ONE active publication is allowed per scope while full
      Unpublished history is preserved.
   Guard: only reindex when no duplicate ACTIVE publications exist.
   --------------------------------------------------------------------- */
IF NOT EXISTS (
    SELECT ExamID FROM dbo.ResultPublications
    WHERE ISNULL(Status,'Published')='Published'
    GROUP BY ExamID, ClassID, ISNULL(SectionID,0) HAVING COUNT(*)>1)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_ResultPub_Unique' AND object_id=OBJECT_ID('dbo.ResultPublications'))
        DROP INDEX UX_ResultPub_Unique ON dbo.ResultPublications;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_ResultPub_ActiveSection' AND object_id=OBJECT_ID('dbo.ResultPublications'))
        CREATE UNIQUE INDEX UX_ResultPub_ActiveSection ON dbo.ResultPublications(ExamID, ClassID, SectionID)
            WHERE Status='Published' AND SectionID IS NOT NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_ResultPub_ActiveNoSection' AND object_id=OBJECT_ID('dbo.ResultPublications'))
        CREATE UNIQUE INDEX UX_ResultPub_ActiveNoSection ON dbo.ResultPublications(ExamID, ClassID)
            WHERE Status='Published' AND SectionID IS NULL;
END
ELSE
    PRINT 'WARNING: duplicate active publications exist - index not changed. Resolve first.';
GO

/* ---------------------------------------------------------------------
   3) Backfill snapshots for exams already Published before this table
      existed, so historical results are captured immutably right now.
      Mirrors the C# ComputeResults logic (dense rank per section,
      grade from the exam-year GradingScale, pass = avg >= exam PassingMark).
      Only inserts rows that are missing (idempotent).
   --------------------------------------------------------------------- */
;WITH scope AS (
    SELECT e.ExamID, e.AcademicYearID, e.PassingMark, e.PublishedBy, e.PublishedAt,
           st.StudentID, sec.SectionID, sec.ClassID,
           reqd.ReqCnt, reqd.MaxM
    FROM dbo.Exams e
    JOIN dbo.ExamClasses ec ON ec.ExamID=e.ExamID
    JOIN dbo.Sections sec ON sec.ClassID=ec.ClassID AND (ec.SectionID IS NULL OR ec.SectionID=sec.SectionID)
    JOIN dbo.Students st ON st.SectionID=sec.SectionID AND st.AcademicYearID=e.AcademicYearID AND ISNULL(st.Status,'Active')='Active'
    CROSS APPLY (SELECT COUNT(*) AS ReqCnt, ISNULL(SUM(TotalMarks),0) AS MaxM FROM dbo.ExamSubjects es WHERE es.ExamID=e.ExamID) reqd
    WHERE ISNULL(e.Status,'')='Published'
      AND NOT EXISTS (SELECT 1 FROM dbo.StudentExamSummaries s WHERE s.ExamID=e.ExamID AND s.StudentID=st.StudentID)
),
marks AS (
    SELECT sc.ExamID, sc.StudentID, sc.SectionID, sc.ClassID, sc.AcademicYearID, sc.PassingMark, sc.ReqCnt, sc.MaxM,
           sc.PublishedBy, sc.PublishedAt,
           SUM(CASE WHEN r.AttendanceStatus IN ('Absent','Excused') THEN 0 ELSE ISNULL(r.Marks,0) END) AS Obtained,
           SUM(CASE WHEN ISNULL(r.Status,'')='Submitted' THEN 1 ELSE 0 END) AS SubmittedCnt,
           SUM(CASE WHEN r.AttendanceStatus='Withheld' THEN 1 ELSE 0 END) AS WithheldCnt,
           SUM(CASE WHEN r.AttendanceStatus IN ('Absent','Excused') THEN 1 ELSE 0 END) AS AbsentCnt
    FROM scope sc
    JOIN dbo.ExamSubjects es ON es.ExamID=sc.ExamID
    LEFT JOIN dbo.ExamResults r ON r.ExamID=sc.ExamID AND r.SubjectID=es.SubjectID AND r.StudentID=sc.StudentID AND ISNULL(r.Status,'')='Submitted'
    GROUP BY sc.ExamID, sc.StudentID, sc.SectionID, sc.ClassID, sc.AcademicYearID, sc.PassingMark, sc.ReqCnt, sc.MaxM, sc.PublishedBy, sc.PublishedAt
),
calc AS (
    SELECT m.*,
           CAST(CASE WHEN m.MaxM>0 THEN ROUND(m.Obtained*100.0/m.MaxM,2) ELSE 0 END AS decimal(6,2)) AS Avg,
           CASE WHEN m.SubmittedCnt >= m.ReqCnt AND m.ReqCnt>0 THEN 1 ELSE 0 END AS IsComplete
    FROM marks m
),
res AS (
    SELECT c.*,
           CASE
             WHEN c.IsComplete=0 THEN 'Incomplete'
             WHEN c.WithheldCnt>0 THEN 'Withheld'
             WHEN c.AbsentCnt=c.ReqCnt THEN 'Absent'
             WHEN c.Avg >= c.PassingMark THEN 'Passed'
             ELSE 'Failed'
           END AS ResultStatus
    FROM calc c
),
ranked AS (
    SELECT r.*,
           CASE WHEN r.ResultStatus IN ('Passed','Failed')
                THEN DENSE_RANK() OVER (PARTITION BY r.ExamID, r.SectionID
                     ORDER BY CASE WHEN r.ResultStatus IN ('Passed','Failed') THEN r.Avg END DESC)
                ELSE NULL END AS Rnk
    FROM res r
)
INSERT INTO dbo.StudentExamSummaries
    (ExamID, StudentID, AcademicYearID, ClassID, SectionID, TotalObtained, TotalMaximum,
     AveragePercentage, OverallGrade, Rank, ResultStatus, PublicationStatus,
     CalculatedBy, CalculatedAt, PublishedBy, PublishedAt, CreatedAt, UpdatedAt)
SELECT rk.ExamID, rk.StudentID, rk.AcademicYearID, rk.ClassID, rk.SectionID,
       rk.Obtained, rk.MaxM, rk.Avg,
       CASE WHEN rk.IsComplete=1 THEN
            (SELECT TOP 1 gs.GradeLetter FROM dbo.GradingScale gs
             WHERE gs.AcademicYearID=rk.AcademicYearID AND ISNULL(gs.Status,'Active')='Active'
               AND rk.Avg BETWEEN gs.MinMarks AND gs.MaxMarks ORDER BY gs.MinMarks DESC)
            ELSE NULL END,
       rk.Rnk, rk.ResultStatus, 'Published',
       NULL, GETDATE(), rk.PublishedBy, rk.PublishedAt, GETDATE(), GETDATE()
FROM ranked rk;
GO

PRINT 'Examinations Stage 6 schema + snapshot backfill applied successfully.';
