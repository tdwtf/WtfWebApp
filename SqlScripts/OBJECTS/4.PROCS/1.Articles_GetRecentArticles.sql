EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Articles_GetRecentArticles',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataTable',
    /* DataTableNames_Csv      */ 'Articles_Extended',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Articles_GetRecentArticles]') IS NOT NULL
	DROP PROCEDURE [Articles_GetRecentArticles]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Articles_GetRecentArticles]
(
    @PublishedStatus_Name VARCHAR(15),
    @Series_Slug NVARCHAR(255) = NULL,
    @Author_Slug NVARCHAR(255) = NULL,
    @Article_Count INT = NULL
)
AS
BEGIN

    IF @Article_Count IS NOT NULL
        SET ROWCOUNT @Article_Count


    SELECT * FROM [Articles_Extended]
            WHERE (@PublishedStatus_Name IS NULL OR [PublishedStatus_Name] = @PublishedStatus_Name)
              AND (@Series_Slug IS NULL OR [Series_Slug] = @Series_Slug)
              AND (@Author_Slug IS NULL OR [Author_Slug] = @Author_Slug)

END
GO

GRANT EXECUTE ON [Articles_GetRecentArticles] TO [TheDailyWtfUser_Role]
GO
