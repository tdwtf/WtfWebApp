EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Comments_GetHiddenComments',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataTable',
    /* DataTableNames_Csv      */ 'Comments_Extended',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Comments_GetHiddenComments]') IS NOT NULL
    DROP PROCEDURE [Comments_GetHiddenComments]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Comments_GetHiddenComments]
(
    @Author_Slug NVARCHAR(255) = NULL,
    @Skip_Count INT = NULL,
    @Limit_Count INT = NULL
)
AS
BEGIN

    SELECT CR.*
      FROM (SELECT C.*,
                   ROW_NUMBER() OVER (ORDER BY C.[Posted_Date] ASC, C.[Comment_Id] ASC) [Row_Number]
              FROM [Comments_Extended] C
             INNER JOIN [Articles] A
                     ON A.[Article_Id] = C.[Article_Id]
             WHERE C.[Hidden_Indicator] = 'Y'
               AND (@Author_Slug IS NULL OR A.[Author_Slug] = @Author_Slug)) CR
     WHERE (CR.[Row_Number] > @Skip_Count AND CR.[Row_Number] <= @Skip_Count + @Limit_Count) OR (@Skip_Count IS NULL AND @Limit_Count IS NULL)
     ORDER BY CR.[Row_Number] ASC

END
GO

GRANT EXECUTE ON [Comments_GetHiddenComments] TO [TheDailyWtfUser_Role]
GO
