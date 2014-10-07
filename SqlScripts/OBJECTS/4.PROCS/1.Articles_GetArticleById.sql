EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Articles_GetArticleById',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataRow',
    /* DataTableNames_Csv      */ 'Articles_Extended',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Articles_GetArticleById]') IS NOT NULL
	DROP PROCEDURE [Articles_GetArticleById]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Articles_GetArticleById]
(
	@Article_Id INT
)
AS
BEGIN

    SELECT * FROM [Articles_Extended]
            WHERE [Article_Id] = @Article_Id

END
GO

GRANT EXECUTE ON [Articles_GetArticleById] TO [TheDailyWtfUser_Role]
GO
