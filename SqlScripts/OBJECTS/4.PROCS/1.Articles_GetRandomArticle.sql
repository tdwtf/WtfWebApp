EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Articles_GetRandomArticle',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataRow',
    /* DataTableNames_Csv      */ 'Articles_Extended',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Articles_GetRandomArticle]') IS NOT NULL
	DROP PROCEDURE [Articles_GetRandomArticle]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Articles_GetRandomArticle]
AS
BEGIN

    SELECT * FROM [Articles_Extended]
            ORDER BY NEWID()

END
GO

GRANT EXECUTE ON [Articles_GetRandomArticle] TO [TheDailyWtfUser_Role]
GO
