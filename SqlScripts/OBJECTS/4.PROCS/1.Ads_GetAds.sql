EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Ads_GetAds',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataTable',
    /* DataTableNames_Csv      */ 'Ads',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Ads_GetAds]') IS NOT NULL
	DROP PROCEDURE [Ads_GetAds]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Ads_GetAds]
AS
BEGIN

    SELECT * FROM [Ads]

END
GO

GRANT EXECUTE ON [Ads_GetAds] TO [TheDailyWtfUser_Role]
GO
