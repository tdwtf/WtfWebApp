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

    UPDATE [Comments]
       SET [Parent_Comment_Id] = NULL
     WHERE [Parent_Comment_Id]
        IN (SELECT CAST([Value_Text] AS INT)
              FROM dbo.CsvToTable(@CommentIds_Csv, ','))

    DELETE FROM [Comments]
          WHERE [Comment_Id]
             IN (SELECT CAST([Value_Text] AS INT)
                   FROM dbo.CsvToTable(@CommentIds_Csv, ','))

END
GO

GRANT EXECUTE ON [Comments_DeleteComments] TO [TheDailyWtfUser_Role]
GO
