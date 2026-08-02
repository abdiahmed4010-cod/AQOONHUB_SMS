param(
    [Parameter(Mandatory=$true)][string]$ServerInstance,
    [Parameter(Mandatory=$true)][ValidatePattern('^[A-Za-z0-9_]+$')][string]$DatabaseName
)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $MyInvocation.MyCommand.Path
$scripts=@('00_CreateDatabase.sql','01_BaseSchema.sql','02_Constraints.sql','03_Indexes.sql',
 '04_ViewsAndProcedures.sql','05_ReferenceSeed.sql','06_ModuleMigrations.sql','TrustForeignKeys.sql','07_VerifyDeployment.sql')
function Invoke-Script([string]$database,[string]$path){
    $connectionString="Data Source=$ServerInstance;Initial Catalog=$database;Integrated Security=True;TrustServerCertificate=True;"
    $text=[IO.File]::ReadAllText($path).Replace('$(DatabaseName)',$DatabaseName)
    $batches=[Text.RegularExpressions.Regex]::Split($text,'(?im)^\s*GO\s*(?:--.*)?$')
    $cn=New-Object Data.SqlClient.SqlConnection $connectionString
    $cn.Open()
    try{foreach($batch in $batches){if([string]::IsNullOrWhiteSpace($batch)){continue};$cmd=$cn.CreateCommand();$cmd.CommandTimeout=180;$cmd.CommandText=$batch;$null=$cmd.ExecuteNonQuery()}}
    finally{$cn.Close()}
}
foreach($name in $scripts){
    $database=if($name -eq '00_CreateDatabase.sql'){'master'}else{$DatabaseName}
    Write-Host "Running $name"
    Invoke-Script $database (Join-Path $root $name)
}
Write-Host "Deployment completed for $DatabaseName"
