EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Comments_GetHiddenComments',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataTable',
    /* DataTableNames_Csv      */ 'Comments',
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
    @Author_Slug NVARCHAR(255) = NULL
)
AS
BEGIN

    SELECT C.* FROM [Comments] C
     INNER JOIN [Articles] A
             ON A.[Article_Id] = C.[Article_Id]
     WHERE C.[Hidden_Indicator] = 'Y'
       AND (@Author_Slug IS NULL OR A.[Author_Slug] = @Author_Slug)
     ORDER BY [Posted_Date] ASC

END
GO

GRANT EXECUTE ON [Comments_GetHiddenComments] TO [TheDailyWtfUser_Role]
GO
