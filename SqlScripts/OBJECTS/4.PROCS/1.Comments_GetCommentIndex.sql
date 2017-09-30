EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Comments_GetCommentIndex',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'void',
    /* DataTableNames_Csv      */ '',
    /* OutputPropertyNames_Csv */ 'Comment_Index',
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Comments_GetCommentIndex]') IS NOT NULL
    DROP PROCEDURE [Comments_GetCommentIndex]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Comments_GetCommentIndex]
(
    @Comment_Id INT,
    @Comment_Index INT = NULL OUT
)
AS
BEGIN

    DECLARE @Article_Id INT
    SELECT @Article_Id = C.[Article_Id]
      FROM [Comments] C
     WHERE C.[Comment_Id] = @Comment_Id

    SELECT @Comment_Index = CAST(CI.[Comment_Index] AS INT)
      FROM (SELECT C.[Comment_Id],
                   ROW_NUMBER() OVER(ORDER BY C.[Posted_Date] ASC, C.[Comment_Id] ASC) [Comment_Index]
              FROM [Comments] C
             WHERE C.[Article_Id] = @Article_Id) CI
     WHERE CI.[Comment_Id] = @Comment_Id

END
GO

GRANT EXECUTE ON [Comments_GetCommentIndex] TO [TheDailyWtfUser_Role]
GO
