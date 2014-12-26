EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'AdRedirectUrls_GetRedirectUrls',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataTable',
    /* DataTableNames_Csv      */ 'AdRedirectUrls',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[AdRedirectUrls_GetRedirectUrls]') IS NOT NULL
	DROP PROCEDURE [AdRedirectUrls_GetRedirectUrls]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [AdRedirectUrls_GetRedirectUrls]
AS
BEGIN

    SELECT * FROM [AdRedirectUrls]

END
GO

GRANT EXECUTE ON [AdRedirectUrls_GetRedirectUrls] TO [TheDailyWtfUser_Role]
GO
