/* ============================================================================
   AQOONHUB SMS — Safe Section Shift Assignment (Stage 6: ambiguity handling)

   Admin-safe, explicit action to assign a Shift to a Section that the automatic
   backfill left NULL because its students span both shifts (e.g. Section 13).

   SAFETY GUARANTEES:
     - Requires an EXPLICIT @SectionID and @TargetShift decision (no guessing).
     - Reports Morning / Afternoon / unassigned student counts first.
     - BLOCKS the assignment when any active student is on the OPPOSITE shift
       (those students must be transferred first via StudentTransfer).
     - NEVER rewrites any student's Shift or placement.
     - Only writes dbo.Sections.Shift, and only when there is no conflict.
     - Re-runnable; leaves state unchanged when it blocks.

   USAGE: set the two variables, then run. Intended to back a Super Admin/Admin
   "Assign Section Shift" configuration action.
   ============================================================================ */
SET XACT_ABORT ON;
SET NOCOUNT ON;

DECLARE @SectionID  int          = 13;          -- <-- section to assign
DECLARE @TargetShift nvarchar(20) = N'Morning';  -- <-- 'Morning' or 'Afternoon'

IF @TargetShift NOT IN (N'Morning', N'Afternoon')
BEGIN
    RAISERROR('TargetShift must be Morning or Afternoon.', 16, 1);
    RETURN;
END

DECLARE @Morning   int = (SELECT COUNT(*) FROM dbo.Students WHERE SectionID=@SectionID AND Status<>N'Deleted' AND Shift=N'Morning');
DECLARE @Afternoon int = (SELECT COUNT(*) FROM dbo.Students WHERE SectionID=@SectionID AND Status<>N'Deleted' AND Shift=N'Afternoon');
DECLARE @Unset     int = (SELECT COUNT(*) FROM dbo.Students WHERE SectionID=@SectionID AND Status<>N'Deleted' AND Shift IS NULL);

PRINT 'Section ' + CONVERT(varchar,@SectionID) + ' current active students:';
PRINT '  Morning   = ' + CONVERT(varchar,@Morning);
PRINT '  Afternoon = ' + CONVERT(varchar,@Afternoon);
PRINT '  Unassigned= ' + CONVERT(varchar,@Unset);

DECLARE @Conflict int = CASE WHEN @TargetShift=N'Morning' THEN @Afternoon ELSE @Morning END;

IF @Conflict > 0
BEGIN
    PRINT '>>> BLOCKED: ' + CONVERT(varchar,@Conflict) + ' active student(s) are on the opposite shift.';
    PRINT '>>> Transfer those students to a matching section first (StudentTransfer). No change made.';
    RETURN;   -- production student placement is never silently changed
END

BEGIN TRANSACTION;
    UPDATE dbo.Sections SET Shift=@TargetShift WHERE SectionID=@SectionID AND (Shift IS NULL OR Shift=@TargetShift);
COMMIT TRANSACTION;

PRINT '>>> Section ' + CONVERT(varchar,@SectionID) + ' shift assigned: ' + @TargetShift;
GO
