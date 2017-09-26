EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Comments_GetCommentsByToken',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataTable',
    /* DataTableNames_Csv      */ 'Comments_Extended',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Comments_GetCommentsByToken]') IS NOT NULL
	DROP PROCEDURE [Comments_GetCommentsByToken]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Comments_GetCommentsByToken]
(
	@User_Token VARCHAR(MAX),
    @Skip_Count INT = NULL,
    @Limit_Count INT = NULL
)
AS
BEGIN

    SELECT CR.*, CI.[Comment_Index], PI.[Comment_Index] [Parent_Comment_Index]
      FROM (SELECT C.*,
                   ROW_NUMBER() OVER (ORDER BY C.[Posted_Date] ASC, C.[Comment_Id] ASC) [Row_Number]
              FROM [Comments_Extended_Slim] C
             WHERE C.[User_Token] = @User_Token) CR
     INNER JOIN [Comments_Index] CI
             ON CR.[Comment_Id] = CI.[Comment_Id]
      LEFT OUTER JOIN [Comments_Index] PI
                   ON CR.[Parent_Comment_Id] = PI.[Comment_Id]
     WHERE (CR.[Row_Number] > @Skip_Count AND CR.[Row_Number] <= @Skip_Count + @Limit_Count) OR (@Skip_Count IS NULL AND @Limit_Count IS NULL)
     ORDER BY CR.[Row_Number] ASC

END
GO

GRANT EXECUTE ON [Comments_GetCommentsByToken] TO [TheDailyWtfUser_Role]
GO
