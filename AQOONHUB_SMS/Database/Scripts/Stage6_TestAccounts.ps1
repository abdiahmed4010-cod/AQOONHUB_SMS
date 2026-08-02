<#  =====================================================================
    AQOONHUB_SMS - Stage 6 TEMPORARY test-account provisioning  (TEST ONLY)
    ---------------------------------------------------------------------
    Creates throwaway login accounts for browser acceptance testing, using
    the SAME PBKDF2-SHA256 hash format the app's Login verifies
    (iterations:base64(salt):base64(hash)).

    * Passwords are generated at RUNTIME and printed ONCE to this console.
      Nothing secret is written to disk or committed to source control.
    * All accounts use the '@stage6test.local' domain so the cleanup
      script (CleanupAttendanceTestData.sql) can remove them precisely.
    * Reversible teacher wiring: a temp Staff row is created and section 13
      is temporarily made its class-teacher; the ORIGINAL Sections.StaffID
      is printed so it can be restored on teardown.

    RUN:   powershell -ExecutionPolicy Bypass -File Stage6_TestAccounts.ps1
    UNDO:  run Stage6_TestAccounts.ps1 -Remove   (or the cleanup SQL)
    ===================================================================== #>
param([switch]$Remove)

$conn = "Data Source=.\SQLEXPRESS;Initial Catalog=AQOONHUB_DB;Integrated Security=True;MultipleActiveResultSets=True;Connect Timeout=30"
function Exec($sql) { $cn=New-Object System.Data.SqlClient.SqlConnection($conn);$cn.Open();(New-Object System.Data.SqlClient.SqlCommand($sql,$cn)).ExecuteNonQuery()|Out-Null;$cn.Close() }
function Scalar($sql){ $cn=New-Object System.Data.SqlClient.SqlConnection($conn);$cn.Open();$o=(New-Object System.Data.SqlClient.SqlCommand($sql,$cn)).ExecuteScalar();$cn.Close();return $o }

if ($Remove) {
    Write-Host "Removing Stage-6 test accounts and reversible wiring..."
    # restore section 13 class teacher to the original Staff (recorded below as 1)
    Exec "UPDATE Sections SET StaffID=1 WHERE SectionID=13 AND StaffID IN (SELECT StaffID FROM Staff WHERE EmployeeID='STG6-TCH')"
    Exec "DELETE FROM StudentGuardians WHERE GuardianID IN (SELECT GuardianID FROM Guardians WHERE UserID IN (SELECT UserID FROM Users WHERE Email LIKE '%@stage6test.local'))"
    Exec "DELETE FROM Guardians WHERE UserID IN (SELECT UserID FROM Users WHERE Email LIKE '%@stage6test.local')"
    Exec "DELETE FROM ClassSubjectTeachers WHERE StaffID IN (SELECT StaffID FROM Staff WHERE EmployeeID='STG6-TCH')"
    Exec "DELETE FROM Staff WHERE EmployeeID='STG6-TCH'"
    Exec "DELETE FROM Users WHERE Email LIKE '%@stage6test.local'"
    Write-Host "Removed. Verify: SELECT * FROM Users WHERE Email LIKE '%@stage6test.local'"
    return
}

function New-Hash([string]$password) {
    $iterations = 100000
    $salt = New-Object byte[] 16
    [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($salt)
    $pbkdf2 = New-Object System.Security.Cryptography.Rfc2898DeriveBytes($password, $salt, $iterations, [System.Security.Cryptography.HashAlgorithmName]::SHA256)
    $hash = $pbkdf2.GetBytes(32)
    return "{0}:{1}:{2}" -f $iterations, [Convert]::ToBase64String($salt), [Convert]::ToBase64String($hash)
}
function New-Password() {
    # strong random temp password (12 chars, mixed)
    $chars = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#%'.ToCharArray()
    -join (1..14 | ForEach-Object { $chars[(Get-Random -Max $chars.Length)] })
}

# clean any prior temp accounts first (idempotent)
& $PSCommandPath -Remove | Out-Null

$accounts = @(
    @{ Email='admin@stage6test.local';     Role='admin';            Name='Stage6 Admin' },
    @{ Email='teacher@stage6test.local';   Role='teacher';          Name='Stage6 Teacher' },
    @{ Email='registrar@stage6test.local'; Role='registrar';        Name='Stage6 Registrar' },
    @{ Email='parent@stage6test.local';    Role='parent';           Name='Stage6 Parent' }
)

Write-Host "`n================ STAGE 6 TEMP ACCOUNTS (copy the passwords now) ================`n"
foreach ($a in $accounts) {
    $pwd = New-Password
    $hash = (New-Hash $pwd).Replace("'","''")
    Exec "INSERT INTO Users (FullName,Email,PasswordHash,Role,IsActive,CreatedAt) VALUES (N'$($a.Name)','$($a.Email)','$hash','$($a.Role)',1,GETDATE())"
    Write-Host ("  {0,-32} {1,-12} password: {2}" -f $a.Email, $a.Role, $pwd)
}

# ---- reversible teacher wiring: temp Staff -> class teacher of section 13 ----
$teacherUserId = [int](Scalar "SELECT UserID FROM Users WHERE Email='teacher@stage6test.local'")
Exec "INSERT INTO Staff (UserID,EmployeeID,Department,Position,Status) VALUES ($teacherUserId,'STG6-TCH','Test','Teacher','Active')"
$origStaff = Scalar "SELECT StaffID FROM Sections WHERE SectionID=13"
$tempStaff = [int](Scalar "SELECT StaffID FROM Staff WHERE EmployeeID='STG6-TCH'")
Exec "UPDATE Sections SET StaffID=$tempStaff WHERE SectionID=13"
Write-Host "`n  Teacher wired as class teacher of Section 13 (temp StaffID=$tempStaff). ORIGINAL Sections.StaffID was: $origStaff  (teardown restores to 1)."

# ---- parent wiring: temp Guardian -> 2 students in section 13 ----
$parentUserId = [int](Scalar "SELECT UserID FROM Users WHERE Email='parent@stage6test.local'")
Exec "INSERT INTO Guardians (UserID,FullName,Relationship,IsActive,CreatedAt) VALUES ($parentUserId,N'Stage6 Parent','Guardian',1,GETDATE())"
$gid = [int](Scalar "SELECT GuardianID FROM Guardians WHERE UserID=$parentUserId")
Exec "INSERT INTO StudentGuardians (StudentID,GuardianID,IsPrimary) SELECT TOP 2 StudentID,$gid, CASE WHEN ROW_NUMBER() OVER(ORDER BY StudentID)=1 THEN 1 ELSE 0 END FROM Students WHERE SectionID=13 AND ISNULL(Status,'Active')='Active'"
Write-Host "  Parent linked to 2 students in Section 13 (GuardianID=$gid)."

Write-Host "`nAll accounts use domain @stage6test.local. When finished, run:  Stage6_TestAccounts.ps1 -Remove`n"
