--AH:ScriptId=f4a6584f-6f50-42ec-a190-38a5de8d60e3;3281
ALTER TABLE [Comments]
	ADD [Comment_Index] INT
GO

;WITH C AS
(
	SELECT [Comment_Id],
	       [Comment_Index],
	       [Generated_Comment_Index] = CAST(ROW_NUMBER() OVER (PARTITION BY [Article_Id] ORDER BY [Posted_Date] ASC, [Comment_Id] ASC) AS INT)
	  FROM [Comments]
)
UPDATE C
   SET [Comment_Index] = [Generated_Comment_Index]

GO

ALTER TABLE [Comments]
	ALTER COLUMN [Comment_Index] INT NOT NULL
GO

ALTER TABLE [Comments]
	ADD CONSTRAINT [UQ__Comments__Comment_Index]
		UNIQUE ([Article_Id], [Comment_Index])
