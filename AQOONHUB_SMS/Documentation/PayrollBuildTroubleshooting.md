# Payroll / Build Troubleshooting

## Windows Smart App Control blocks the freshly built DLL

**Symptom** — after a rebuild, IIS Express returns HTTP 500 and the error is:

```
Could not load file or assembly 'AQOONHUB_SMS' or one of its dependencies.
An Application Control policy has blocked this file. (Exception from HRESULT: 0x800711C7)
```

This is **Windows Smart App Control / Application Control policy** re-evaluating the
newly produced (unsigned) `bin\AQOONHUB_SMS.dll`. It is an OS security decision, not
an application bug — the build itself succeeds.

**Do NOT add application code to bypass Windows security.**

### Recovery steps (in order)

1. Stop IIS Express:
   ```
   taskkill /IM iisexpress.exe /F
   ```
2. Clean the solution (Visual Studio: *Build → Clean Solution*, or MSBuild `-t:Clean`).
3. Delete the build output so a fresh assembly is produced:
   - `bin\`
   - `obj\`
4. Clear the ASP.NET Temporary Files (shadow-copied assemblies) when needed:
   - `%LocalAppData%\Temp\Temporary ASP.NET Files\`
   - `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Temporary ASP.NET Files\`
5. Rebuild the solution and confirm:
   ```
   AQOONHUB_SMS -> bin\AQOONHUB_SMS.dll
   ```
6. Restart IIS Express and reload the site.

In practice a **Clean + Rebuild + clearing Temporary ASP.NET Files** is enough for
the OS to re-evaluate and allow the new assembly.

### Notes

- Do not disable Smart App Control / Windows Defender Application Control without
  understanding the security implications for the machine.
- The blocker is intermittent and tied to the rebuilt binary — it does not indicate
  a code defect.

## Local SQL Server connectivity (`Cannot generate SSPI context`)

If the local SQL Server rejects the integrated-security connection with
`Cannot generate SSPI context`, runtime/database testing is blocked even though the
build is fine. Resolve the SPN/Kerberos or use a working SQL login for the
`AQOONHUB_DB` connection string; do not weaken application security to work around it.
