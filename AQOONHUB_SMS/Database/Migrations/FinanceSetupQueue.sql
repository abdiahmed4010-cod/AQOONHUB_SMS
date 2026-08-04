/* ============================================================================
   AQOONHUB SMS — Finance Setup Queue support

   Adds a DB-level guard against DUPLICATE INITIAL invoices, complementing the
   in-transaction application check in FeeRepository.CreateInitialInvoice.

   SAFE / IDEMPOTENT:
     - A FILTERED unique index on (StudentID, AcademicYearID) restricted to
       InvoiceType='Initial' AND Status<>'Cancelled'. It blocks only a second
       *initial* invoice per student per year — it does NOT block legitimate
       term, monthly, installment, transport or examination invoices (those use
       other InvoiceType values).
     - Created only if missing. No column is added to Students (Finance status is
       computed, never stored). No data is modified. Re-runnable.

   NOTE: existing FeeInvoices use InvoiceType='Regular' only, so no current row is
   affected and the index creates cleanly.
   ============================================================================ */
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
GO

BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'UX_FeeInvoices_InitialPerStudentYear'
                 AND object_id = OBJECT_ID(N'dbo.FeeInvoices'))
BEGIN
    CREATE UNIQUE INDEX UX_FeeInvoices_InitialPerStudentYear
        ON dbo.FeeInvoices (StudentID, AcademicYearID)
        WHERE InvoiceType = N'Initial' AND [Status] <> N'Cancelled';
END

COMMIT TRANSACTION;
GO

/* Verification (informational) */
SELECT COUNT(*) AS InitialInvoiceGuard_Present
FROM sys.indexes
WHERE name = N'UX_FeeInvoices_InitialPerStudentYear'
  AND object_id = OBJECT_ID(N'dbo.FeeInvoices');
GO
