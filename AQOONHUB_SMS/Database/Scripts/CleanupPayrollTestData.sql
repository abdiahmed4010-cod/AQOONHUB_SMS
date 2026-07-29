/* ============================================================================
   AQOONHUB_SMS — Payroll test-data cleanup (SAFE / PREVIEW BY DEFAULT)
   ----------------------------------------------------------------------------
   Purpose:
     Remove ONLY the payroll test data created during development testing:
       - Payroll period "July 2026"
       - Its PayrollRecords (incl. test reference "TXN-778899" and the
         "Bank rejected the transfer" note)
       - Any PayrollAdjustments belonging to those records

   Safety:
     * This script RUNS INSIDE A TRANSACTION AND ROLLS BACK BY DEFAULT.
       Nothing is deleted until you deliberately change ROLLBACK to COMMIT
       (see the bottom of the script) AFTER reviewing the preview output.
     * It NEVER touches dbo.Staff, dbo.Users, or unrelated payroll periods.
     * Child rows (PayrollAdjustments -> PayrollRecords) are removed before
       the parent period.

   How to use:
     1. Run the script as-is and review the four preview result sets.
     2. Confirm the rows shown are exactly the test data you intend to remove.
     3. Only then, comment out the DELETE guard / change ROLLBACK -> COMMIT
        (instructions inline) and run again.
   ============================================================================ */

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @TestPeriodName NVARCHAR(100) = N'July 2026';

BEGIN TRANSACTION;

-- Resolve the candidate period id(s) for the test period only.
DECLARE @PeriodIds TABLE (PayrollPeriodID INT PRIMARY KEY);
INSERT INTO @PeriodIds (PayrollPeriodID)
SELECT PayrollPeriodID
FROM dbo.PayrollPeriods
WHERE PeriodName = @TestPeriodName;

-- ---------------------------------------------------------------------------
-- PREVIEW 1: the payroll period(s) that will be affected
-- ---------------------------------------------------------------------------
SELECT 'PREVIEW 1 - PayrollPeriods to delete' AS Preview,
       pp.PayrollPeriodID, pp.PeriodName, pp.Status, pp.StartDate, pp.EndDate
FROM dbo.PayrollPeriods pp
INNER JOIN @PeriodIds p ON p.PayrollPeriodID = pp.PayrollPeriodID;

-- ---------------------------------------------------------------------------
-- PREVIEW 2: payroll records that will be deleted (with test markers)
-- ---------------------------------------------------------------------------
SELECT 'PREVIEW 2 - PayrollRecords to delete' AS Preview,
       pr.PayrollRecordID, pr.PayrollPeriodID, s.EmployeeID,
       pr.NetSalary, pr.PaymentStatus, pr.PaymentReference, pr.Notes
FROM dbo.PayrollRecords pr
INNER JOIN @PeriodIds p ON p.PayrollPeriodID = pr.PayrollPeriodID
INNER JOIN dbo.Staff s ON s.StaffID = pr.StaffID;

-- ---------------------------------------------------------------------------
-- PREVIEW 3: payroll adjustments belonging to those records
-- ---------------------------------------------------------------------------
SELECT 'PREVIEW 3 - PayrollAdjustments to delete' AS Preview, pa.*
FROM dbo.PayrollAdjustments pa
WHERE pa.PayrollRecordID IN
(
    SELECT pr.PayrollRecordID
    FROM dbo.PayrollRecords pr
    INNER JOIN @PeriodIds p ON p.PayrollPeriodID = pr.PayrollPeriodID
);

-- ---------------------------------------------------------------------------
-- PREVIEW 4: safety confirmation — no Staff / Users are ever touched
-- ---------------------------------------------------------------------------
SELECT 'PREVIEW 4 - Safety' AS Preview,
       (SELECT COUNT(*) FROM @PeriodIds) AS PeriodsMatched,
       'Staff and Users are NEVER modified by this script.' AS Note;

/* ---------------------------------------------------------------------------
   DELETE STATEMENTS — DISABLED BY DEFAULT.
   To actually delete after reviewing the previews above:
     1. Remove the surrounding block comment markers on the DELETEs below.
     2. Change the final ROLLBACK TRANSACTION to COMMIT TRANSACTION.
   Child rows are deleted before parent rows.
   ---------------------------------------------------------------------------

DELETE pa
FROM dbo.PayrollAdjustments pa
WHERE pa.PayrollRecordID IN
(
    SELECT pr.PayrollRecordID
    FROM dbo.PayrollRecords pr
    INNER JOIN @PeriodIds p ON p.PayrollPeriodID = pr.PayrollPeriodID
);

DELETE pr
FROM dbo.PayrollRecords pr
INNER JOIN @PeriodIds p ON p.PayrollPeriodID = pr.PayrollPeriodID;

DELETE pp
FROM dbo.PayrollPeriods pp
INNER JOIN @PeriodIds p ON p.PayrollPeriodID = pp.PayrollPeriodID;

--------------------------------------------------------------------------- */

-- Rolls back by default so running the script is always non-destructive.
-- After review, change ROLLBACK to COMMIT (and enable the DELETEs above).
ROLLBACK TRANSACTION;
-- COMMIT TRANSACTION;
