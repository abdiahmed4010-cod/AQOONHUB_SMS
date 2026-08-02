# Deployment Guide

1. Provision Windows Server with IIS and .NET Framework 4.8 ASP.NET features.
2. Use an application pool with `.NET CLR v4.0`, Integrated pipeline, and a dedicated low-privilege identity.
3. Publish to a versioned release directory. Grant the app-pool identity read/execute access and write access only to explicitly approved upload/log directories.
4. Configure the production connection string through a protected transform or deployment secret. Encrypt sensitive configuration where operationally supported.
5. Set `customErrors`/HTTP errors to avoid raw stack traces; log detailed failures to an access-controlled server sink.
6. Require HTTPS, secure cookies, appropriate SameSite settings, and an HSTS policy at IIS/reverse proxy level.
7. Confirm session timeout and authentication timeout match policy.
8. Deploy all static assets and verify MIME types, cache headers, and content hashes/version query strings.
9. For an empty database, run `Database/Deployment/Deploy-All.ps1`; for an existing database, apply reviewed migrations after a tested backup. Run `Database/Deployment/TrustForeignKeys.sql` and `07_VerifyDeployment.sql`.
10. Smoke-test login, dashboard, one read/write flow per module, exports, authorization denials, and logout.

Before go-live, test backup restoration, rotate deployment credentials, remove temporary artifacts, and retain a rollback package containing the previous binaries and database backup.
