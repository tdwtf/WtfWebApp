EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Ads_DeleteAd',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataTable',
    /* DataTableNames_Csv      */ 'Ads',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Ads_DeleteAd]') IS NOT NULL
	DROP PROCEDURE [Ads_DeleteAd]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Ads_DeleteAd]
(
    @Ad_Id INT
)
AS
BEGIN

BEGIN TRY
	BEGIN TRANSACTION

    UPDATE [Articles] 
       SET [Ad_Id] = NULL 
     WHERE [Ad_Id] = @Ad_Id

    DELETE [Ads]
     WHERE [Ad_Id] = @Ad_Id

     COMMIT
    
END TRY BEGIN CATCH
	IF XACT_STATE()<>0 ROLLBACK
	EXEC [HandleError]
END CATCH

END
GO

GRANT EXECUTE ON [Ads_DeleteAd] TO [TheDailyWtfUser_Role]
GO
