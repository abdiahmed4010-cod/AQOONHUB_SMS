# Database Setup

Use a dedicated SQL login with only the permissions required by the application. Back up an existing database before applying migrations.

## Deployment sequence

1. Run `Database/Deployment/Deploy-All.ps1` with an empty, uniquely named database, or execute `00_CreateDatabase.sql` through `07_VerifyDeployment.sql` in numeric order.
2. Run `Database/Deployment/TrustForeignKeys.sql`; it aborts rather than trusting a constraint when orphan rows exist.
3. Confirm `07_VerifyDeployment.sql` reports the expected object/reference counts and zero disabled or untrusted foreign keys.
4. Apply future idempotent module migrations after the baseline and rerun verification.
5. Configure `AQOONHUB_DB` securely. Production credentials must not be stored in documentation or committed transforms.

Required reference data includes roles, permissions, academic years, terms, and grading scales. Do not seed private users, students, payments, payroll, or audit history.

The schema-only baseline was deployed successfully to a disposable database: 62 tables, 107 trusted foreign keys, 338 total indexes, 7 stored procedures, 10 roles, 21 permissions, and zero private operational rows. The disposable database and temporary site copy were removed afterward.

Backups must be encrypted, access-controlled, tested by restore, and retained according to school policy.
