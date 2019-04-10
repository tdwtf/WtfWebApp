--AH:ScriptId=9cc68c9c-9295-4ab0-a368-fa9cfe414328;2817
ALTER TABLE [Authors]
  ADD [Active_Indicator] YNINDICATOR NULL
GO

UPDATE [Authors]
   SET [Active_Indicator] = 'Y'
GO

ALTER TABLE [Authors]
  ALTER COLUMN [Active_Indicator] YNINDICATOR NOT NULL
GO