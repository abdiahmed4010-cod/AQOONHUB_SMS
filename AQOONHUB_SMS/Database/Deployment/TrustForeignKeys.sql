SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @fk int,@name sysname,@ps sysname,@pt sysname,@rs sysname,@rt sysname,
        @join nvarchar(max),@nonnull nvarchar(max),@sql nvarchar(max),@orphans bigint;
DECLARE fk_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT fk.object_id,fk.name,SCHEMA_NAME(p.schema_id),p.name,SCHEMA_NAME(r.schema_id),r.name
FROM sys.foreign_keys fk
JOIN sys.tables p ON p.object_id=fk.parent_object_id
JOIN sys.tables r ON r.object_id=fk.referenced_object_id
WHERE fk.is_disabled=0 AND fk.is_not_trusted=1
ORDER BY p.name,fk.name;

OPEN fk_cursor;
FETCH NEXT FROM fk_cursor INTO @fk,@name,@ps,@pt,@rs,@rt;
WHILE @@FETCH_STATUS=0
BEGIN
    SELECT @join=STUFF((SELECT N' AND p.'+QUOTENAME(pc.name)+N'=r.'+QUOTENAME(rc.name)
      FROM sys.foreign_key_columns fkc
      JOIN sys.columns pc ON pc.object_id=fkc.parent_object_id AND pc.column_id=fkc.parent_column_id
      JOIN sys.columns rc ON rc.object_id=fkc.referenced_object_id AND rc.column_id=fkc.referenced_column_id
      WHERE fkc.constraint_object_id=@fk ORDER BY fkc.constraint_column_id FOR XML PATH(''),TYPE).value('.','nvarchar(max)'),1,5,N''),
      @nonnull=STUFF((SELECT N' AND p.'+QUOTENAME(pc.name)+N' IS NOT NULL'
      FROM sys.foreign_key_columns fkc
      JOIN sys.columns pc ON pc.object_id=fkc.parent_object_id AND pc.column_id=fkc.parent_column_id
      WHERE fkc.constraint_object_id=@fk ORDER BY fkc.constraint_column_id FOR XML PATH(''),TYPE).value('.','nvarchar(max)'),1,5,N'');
    SET @sql=N'SELECT @n=COUNT_BIG(*) FROM '+QUOTENAME(@ps)+N'.'+QUOTENAME(@pt)+N' p WHERE '+@nonnull+
             N' AND NOT EXISTS(SELECT 1 FROM '+QUOTENAME(@rs)+N'.'+QUOTENAME(@rt)+N' r WHERE '+@join+N')';
    EXEC sys.sp_executesql @sql,N'@n bigint OUTPUT',@orphans OUTPUT;
    IF @orphans<>0
    BEGIN
        SELECT @name AS ConstraintName,@ps AS ParentSchema,@pt AS ParentTable,
               @rs AS ReferencedSchema,@rt AS ReferencedTable,@orphans AS OrphanCount;
        CLOSE fk_cursor; DEALLOCATE fk_cursor;
        THROW 50010,'Foreign-key orphans exist; no rows were modified.',1;
    END;
    SET @sql=N'ALTER TABLE '+QUOTENAME(@ps)+N'.'+QUOTENAME(@pt)+N' WITH CHECK CHECK CONSTRAINT '+QUOTENAME(@name)+N';';
    EXEC sys.sp_executesql @sql;
    FETCH NEXT FROM fk_cursor INTO @fk,@name,@ps,@pt,@rs,@rt;
END;
CLOSE fk_cursor; DEALLOCATE fk_cursor;

IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE is_disabled=1 OR is_not_trusted=1)
    THROW 50011,'Foreign-key trust verification did not reach zero.',1;
