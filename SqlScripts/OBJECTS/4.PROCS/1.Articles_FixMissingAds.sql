EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Articles_FixMissingAds',
    /* Internal_Indicator      */ 'Y',
    /* ReturnType_Name         */ 'void',
    /* DataTableNames_Csv      */ NULL,
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Articles_FixMissingAds]') IS NOT NULL
	DROP PROCEDURE [Articles_FixMissingAds]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Articles_FixMissingAds]
AS
BEGIN

    UPDATE [Articles]
       SET [Ad_Id] = (SELECT TOP 1 [Ad_Id] FROM [Ads] ORDER BY NEWID())
     WHERE [Ad_Id] IS NULL

END
GO

GRANT EXECUTE ON [Articles_FixMissingAds] TO [TheDailyWtfUser_Role]
GO
