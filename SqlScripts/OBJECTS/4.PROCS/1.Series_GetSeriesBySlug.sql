EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Series_GetSeriesBySlug',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataRow',
    /* DataTableNames_Csv      */ 'Series',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Series_GetSeriesBySlug]') IS NOT NULL
	DROP PROCEDURE [Series_GetSeriesBySlug]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Series_GetSeriesBySlug]
(
    @Series_Slug NVARCHAR(255)
)   
AS
BEGIN

    SELECT * FROM [Series]
            WHERE [Series_Slug] = @Series_Slug

END
GO

GRANT EXECUTE ON [Series_GetSeriesBySlug] TO [TheDailyWtfUser_Role]
GO
