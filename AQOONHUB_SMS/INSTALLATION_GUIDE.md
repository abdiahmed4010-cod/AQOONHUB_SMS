# Installation Guide

## Prerequisites

- Windows with IIS Express or IIS
- Visual Studio 2019 or later with ASP.NET/.NET Framework tooling
- .NET Framework 4.8 Developer Pack
- SQL Server 2019+ (or a compatible supported version)

## Local installation

1. Restore NuGet packages referenced by `packages.config`.
2. Create a private SQL Server database and follow `DATABASE_SETUP.md`.
3. Set `AQOONHUB_DB` in `Web.config` or an environment-specific transform. Do not commit real credentials.
4. Build the project in Debug.
5. Run using IIS Express on the project URL and browse to `/Modules/Authentication/Login.aspx`.
6. Create the first Super Admin through an approved administrative seed process; never reuse production credentials in test environments.

If IIS returns a parser error, verify the application targets .NET Framework 4.8 and that `bin/AQOONHUB_SMS.dll` exists.
