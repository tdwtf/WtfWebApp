EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Articles_GetOtherRecentArticles',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataTable',
    /* DataTableNames_Csv      */ 'Articles_Extended',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Articles_GetOtherRecentArticles]') IS NOT NULL
	DROP PROCEDURE [Articles_GetOtherRecentArticles]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Articles_GetOtherRecentArticles]
(
    @PublishedStatus_Name VARCHAR(15),
    @Article_Count INT = NULL
)
AS
BEGIN

    IF @Article_Count IS NOT NULL
        SET ROWCOUNT @Article_Count


    SELECT * FROM [Articles_Extended]
            WHERE (@PublishedStatus_Name IS NULL OR [PublishedStatus_Name] = @PublishedStatus_Name)
              AND ([Series_Slug] NOT IN ('feature-articles', 'code-sod', 'errord'))
              AND [Published_Date] < GETDATE()
         ORDER BY [Published_Date] DESC

END
GO

GRANT EXECUTE ON [Articles_GetOtherRecentArticles] TO [TheDailyWtfUser_Role]
GO
