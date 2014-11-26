EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Ads_CreateOrUpdateAd',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataRow',
    /* DataTableNames_Csv      */ NULL,
    /* OutputPropertyNames_Csv */ 'Ad_Id',
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Ads_CreateOrUpdateAd]') IS NOT NULL
	DROP PROCEDURE [Ads_CreateOrUpdateAd]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Ads_CreateOrUpdateAd]
(
    @Ad_Html NVARCHAR(MAX),
    @Ad_Id INT = NULL OUT
)
AS
BEGIN

    IF (EXISTS(SELECT * FROM [Ads] WHERE [Ad_Id] = @Ad_Id))
    BEGIN

        UPDATE [Ads]
           SET [Ad_Html] = @Ad_Html
         WHERE [Ad_Id] = @Ad_Id

    END
    ELSE
    BEGIN

        INSERT INTO [Ads]
        (
            [Ad_Html]
        )
        VALUES
        (
            @Ad_Html
        )

    END

END
GO

GRANT EXECUTE ON [Ads_CreateOrUpdateAd] TO [TheDailyWtfUser_Role]
GO
