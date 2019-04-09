EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Articles_GetArticlesSlim',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataTable',
    /* DataTableNames_Csv      */ 'Articles_Slim',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Articles_GetArticlesSlim]') IS NOT NULL
	DROP PROCEDURE [Articles_GetArticlesSlim]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Articles_GetArticlesSlim]
AS
BEGIN

    SELECT *
	  FROM [Articles_Slim]

END
GO

GRANT EXECUTE ON [Articles_GetArticlesSlim] TO [TheDailyWtfUser_Role]
GO
