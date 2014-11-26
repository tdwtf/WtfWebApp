EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Articles_GetArticleByLegacyPost',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataRow',
    /* DataTableNames_Csv      */ 'Articles_Extended',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Articles_GetArticleByLegacyPost]') IS NOT NULL
	DROP PROCEDURE [Articles_GetArticleByLegacyPost]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Articles_GetArticleByLegacyPost]
(
	@Post_Id INT
)
AS
BEGIN

    DECLARE @Article_Id INT
    SELECT @Article_Id = [Article_Id] 
      FROM [ArticlePostMappings]
     WHERE [Post_Id] = @Post_Id   


    SELECT * FROM [Articles_Extended]
            WHERE [Article_Id] = @Article_Id

END
GO

GRANT EXECUTE ON [Articles_GetArticleByLegacyPost] TO [TheDailyWtfUser_Role]
GO
