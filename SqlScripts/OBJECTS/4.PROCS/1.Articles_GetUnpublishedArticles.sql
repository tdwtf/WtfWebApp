EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Articles_GetUnpublishedArticles',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataTable',
    /* DataTableNames_Csv      */ 'Articles_Extended',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Articles_GetUnpublishedArticles]') IS NOT NULL
	DROP PROCEDURE [Articles_GetUnpublishedArticles]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Articles_GetUnpublishedArticles]
AS
BEGIN


    SELECT * FROM [Articles_Extended]
            WHERE [PublishedStatus_Name] <> 'Published'

END
GO

GRANT EXECUTE ON [Articles_GetUnpublishedArticles] TO [TheDailyWtfUser_Role]
GO
