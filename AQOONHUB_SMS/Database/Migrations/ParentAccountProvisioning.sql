/* ============================================================================
   AQOONHUB SMS — Parent Account Provisioning support

   Adds dbo.Users.MustChangePassword so a freshly provisioned Parent account can
   be flagged to change its temporary password. Additive, idempotent, safe:
     - Adds the column only if missing, NOT NULL DEFAULT 0.
     - Existing users are unaffected (default 0 = no forced change).
     - No destructive updates. Re-runnable with identical end state.
   ============================================================================ */
SET XACT_ABORT ON;
SET NOCOUNT ON;
GO

BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = N'MustChangePassword')
BEGIN
    ALTER TABLE dbo.Users
        ADD MustChangePassword bit NOT NULL
        CONSTRAINT DF_Users_MustChangePassword DEFAULT (0);
END

COMMIT TRANSACTION;
GO

/* Verification (informational) */
SELECT COUNT(*) AS MustChangePassword_Column_Present
FROM sys.columns
WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = N'MustChangePassword';
GO
