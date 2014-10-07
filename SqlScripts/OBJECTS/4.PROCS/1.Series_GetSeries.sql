EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Series_GetSeries',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataTable',
    /* DataTableNames_Csv      */ 'Series',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Series_GetSeries]') IS NOT NULL
	DROP PROCEDURE [Series_GetSeries]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Series_GetSeries]
AS
BEGIN

    SELECT * FROM [Series]

END
GO

GRANT EXECUTE ON [Series_GetSeries] TO [TheDailyWtfUser_Role]
GO
