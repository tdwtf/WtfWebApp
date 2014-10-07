EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Articles_GetArticleBySlug',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataRow',
    /* DataTableNames_Csv      */ 'Articles_Extended',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Articles_GetArticleBySlug]') IS NOT NULL
	DROP PROCEDURE [Articles_GetArticleBySlug]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Articles_GetArticleBySlug]
(
	@Article_Slug NVARCHAR(255)
)
AS
BEGIN

    SELECT * FROM [Articles_Extended]
            WHERE [Article_Slug] = @Article_Slug

END
GO

GRANT EXECUTE ON [Articles_GetArticleBySlug] TO [TheDailyWtfUser_Role]
GO
