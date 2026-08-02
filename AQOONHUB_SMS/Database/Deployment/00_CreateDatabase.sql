SET NOCOUNT ON;
IF DB_ID(N'$(DatabaseName)') IS NULL
BEGIN
    DECLARE @sql nvarchar(max) = N'CREATE DATABASE ' + QUOTENAME(N'$(DatabaseName)') + N';';
    EXEC sys.sp_executesql @sql;
END;
