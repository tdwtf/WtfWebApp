EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Comments_DeleteComments',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'void',
    /* DataTableNames_Csv      */ NULL,
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Comments_DeleteComments]') IS NOT NULL
	DROP PROCEDURE [Comments_DeleteComments]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Comments_DeleteComments]
(
    @CommentIds_Csv VARCHAR(MAX)
)
AS
BEGIN

BEGIN TRY
	BEGIN TRANSACTION
	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	DECLARE @CommentsToDelete TABLE ([Comment_Id] INT NOT NULL)
	DECLARE @ArticlesTouched TABLE ([Article_Id] INT NOT NULL)

	INSERT INTO @CommentsToDelete
	(
		[Comment_Id]
	)
	SELECT CAST([Value_Text] AS INT)
      FROM dbo.CsvToTable(@CommentIds_Csv, ',')

	INSERT INTO @ArticlesTouched
	(
		[Article_Id]
	)
	SELECT DISTINCT [Article_Id]
	  FROM [Comments]
	 WHERE [Comment_Id] IN (SELECT [Comment_Id] FROM @CommentsToDelete)

	UPDATE [Comments]
	   SET [Parent_Comment_Id] = NULL
	 WHERE [Parent_Comment_Id] IN (SELECT [Comment_Id] FROM @CommentsToDelete)

    DELETE [Comments]
     WHERE [Comment_Id] IN (SELECT [Comment_Id] FROM @CommentsToDelete)

	 --regenerate sequence numbers for article comments
	;WITH C AS
	(
		SELECT [Comment_Id],
			   [Comment_Index],
			   [Generated_Comment_Index] = CAST(ROW_NUMBER() OVER (PARTITION BY [Article_Id] ORDER BY [Posted_Date] ASC, [Comment_Id] ASC) AS INT)
		  FROM [Comments]
		 WHERE [Article_Id] IN (SELECT [Article_Id] FROM @ArticlesTouched)
    )
	UPDATE C
	   SET [Comment_Index] = [Generated_Comment_Index]

	COMMIT
END TRY BEGIN CATCH
	IF XACT_STATE()<>0 ROLLBACK
	EXEC [HandleError]
END CATCH

END
GO

GRANT EXECUTE ON [Comments_DeleteComments] TO [TheDailyWtfUser_Role]
GO
