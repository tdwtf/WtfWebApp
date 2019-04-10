--AH:ScriptId=8e81e0a0-49f7-49cb-87e2-cbc692df66e9;2799

ALTER TABLE [Comments]
  ALTER COLUMN [Discourse_Post_Id] INT NULL
GO

ALTER TABLE [Comments]
  ADD [Featured_Indicator] YNINDICATOR NULL 
GO

UPDATE [Comments]
   SET [Featured_Indicator] = 'N'
GO

ALTER TABLE [Comments]
  ALTER COLUMN [Featured_Indicator] YNINDICATOR NOT NULL
GO