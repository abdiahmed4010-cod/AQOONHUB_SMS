# Testing Report

## Verified before this release-preparation pass

- Reports stages 1–6 build/database/runtime regression passed.
- SYSTEM module build, database/runtime, deactivate protections, responsive matrix, and light/dark browser verification passed.
- Temporary SYSTEM browser accounts and dependent rows were removed.

## Current acceptance findings

- Repository page/designer pairs: complete for all 97 ASPX pages.
- Project entries: no duplicate or broken `Content`/`Compile` paths detected.
- Navigation: dead Communication/Profile routes and demo identity/role controls were removed during release preparation.
- Database: all 12 previously untrusted foreign keys had zero orphan rows and were enabled/trusted; the final enabled-untrusted count is zero.
- Database deployment baseline: the complete ordered schema-only package deployed successfully to a disposable database and passed application startup checks. The database and temporary site were removed.
- Final Clean + Rebuild on .NET Framework 4.8 passed with `AQOONHUB_SMS.dll` generated, 0 errors, and 0 warnings.
- Final full-site ASP.NET precompile completed with exit code 0.
- A fresh temporary Super Admin session opened the 15 representative module routes without parser/runtime errors, full-page overflow, or JavaScript console errors at the desktop regression viewport.
- A 60-case light/dark responsive matrix covered Dashboard, Students, Reports, Users, and Settings at 1440×900, 1280×850, 1024×768, 768×900, 390×844, and 360×800. A Students tablet overflow was found and corrected; the affected viewports then passed.
- The Users modal fit at 360×800, closed with Escape, and returned focus to its trigger.
- A real Users CSV download had a safe timestamped filename and UTF-8 BOM, then was deleted. Representative Finance/Examination/Payroll CSV code was hardened for BOM/formula injection during this pass.
- Print CSS exists for representative attendance, examination, finance, payroll, report-viewer, and custom-report pages. Native browser Print Preview was not exercised in this automation context.
- All eight roles passed authenticated server-runtime direct-URL checks across 15 representative routes. Super Admin also passed in-browser. The browser controller then became stuck on an unrecoverable confirmation state.
- The final Clean/Rebuild generated `AQOONHUB_SMS.dll` with 0 errors and 0 warnings; full-site precompile exited 0.
- Priority Authentication/User and Reports `SELECT *` queries were replaced with explicit columns. Broad legacy row-detail queries outside the security/export priority remain intentionally unchanged pending module-specific mapping refactors.
- Final nine-download and six-PDF verification could not complete after Windows integrated SQL authentication began returning an SSPI principal error. A scoped SQL Express restart was denied by Windows, and the broader Kerberos-cache mutation was not performed.

## Acceptance status

Do not label the product Production Ready until the remaining seven browser-role sessions, nine downloaded CSV inspections, six print PDFs, and exact temporary-account cleanup are completed after the local SSPI environment is restored.
