EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Comments_GetComments',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataTable',
    /* DataTableNames_Csv      */ 'Comments',
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
	@Article_Id INT
)
AS
BEGIN

    SELECT * FROM [Comments]
            WHERE [Article_Id] = @Article_Id
         ORDER BY [Posted_Date] ASC

END
GO

GRANT EXECUTE ON [Comments_GetComments] TO [TheDailyWtfUser_Role]
GO
