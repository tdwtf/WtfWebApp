EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'AdRedirectUrls_IncrementClickCount',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'void',
    /* DataTableNames_Csv      */ NULL,
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[AdRedirectUrls_IncrementClickCount]') IS NOT NULL
	DROP PROCEDURE [AdRedirectUrls_IncrementClickCount]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [AdRedirectUrls_IncrementClickCount]
(
    @Ad_Guid UNIQUEIDENTIFIER,
    @Increment_Count INT
)
AS
BEGIN

    UPDATE [AdRedirectUrls]
       SET [Click_Count] = [Click_Count] + @Increment_Count
     WHERE [Ad_Guid] = @Ad_Guid

END
GO

GRANT EXECUTE ON [AdRedirectUrls_IncrementClickCount] TO [TheDailyWtfUser_Role]
GO
