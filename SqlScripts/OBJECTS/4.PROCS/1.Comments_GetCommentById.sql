EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Comments_GetCommentById',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataTable',
    /* DataTableNames_Csv      */ 'Comments_Extended',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Comments_GetCommentById]') IS NOT NULL
    DROP PROCEDURE [Comments_GetCommentById]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Comments_GetCommentById]
(
    @Comment_Id INT
)
AS
BEGIN

    SELECT C.*, CI.[Comment_Index], PI.[Comment_Index] [Parent_Comment_Index]
      FROM [Comments_Extended_Slim] C
     INNER JOIN [Comments_Index] CI
             ON C.[Comment_Id] = CI.[Comment_Id]
      LEFT OUTER JOIN [Comments_Index] PI
                   ON C.[Parent_Comment_Id] = PI.[Comment_Id]
     WHERE C.[Comment_Id] = @Comment_Id

END
GO

GRANT EXECUTE ON [Comments_GetCommentById] TO [TheDailyWtfUser_Role]
GO
