EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'AdRedirectUrls_AddRedirectUrl',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'void',
    /* DataTableNames_Csv      */ NULL,
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[AdRedirectUrls_AddRedirectUrl]') IS NOT NULL
	DROP PROCEDURE [AdRedirectUrls_AddRedirectUrl]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [AdRedirectUrls_AddRedirectUrl]
(
    @Redirect_Url NVARCHAR(255)
)
AS
BEGIN

    IF NOT EXISTS(SELECT * FROM [AdRedirectUrls] WHERE [Redirect_Url] = @Redirect_Url)
    BEGIN

        INSERT INTO [AdRedirectUrls] ([Ad_Guid], [Redirect_Url], [Click_Count])
        VALUES (NEWID(), @Redirect_Url, 0)

    END

END
GO

GRANT EXECUTE ON [AdRedirectUrls_AddRedirectUrl] TO [TheDailyWtfUser_Role]
GO
