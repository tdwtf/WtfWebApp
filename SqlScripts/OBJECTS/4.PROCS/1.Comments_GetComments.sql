EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Comments_GetComments',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataTable',
    /* DataTableNames_Csv      */ 'Comments_Extended',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Comments_GetComments]') IS NOT NULL
	DROP PROCEDURE [Comments_GetComments]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Comments_GetComments]
(
	@Article_Id INT,
    @Skip_Count INT = NULL,
    @Limit_Count INT = NULL
)
AS
BEGIN

    SELECT CR.*
      FROM (SELECT C.*,
                   ROW_NUMBER() OVER (ORDER BY C.[Posted_Date] ASC, C.[Comment_Id] ASC) [Row_Number]
              FROM [Comments_Extended] C
             WHERE C.[Article_Id] = @Article_Id) CR
     WHERE CR.[Row_Number] > @Skip_Count AND CR.[Row_Number] <= @Skip_Count + @Limit_Count
     ORDER BY CR.[Row_Number] ASC

END
GO

GRANT EXECUTE ON [Comments_GetComments] TO [TheDailyWtfUser_Role]
GO
