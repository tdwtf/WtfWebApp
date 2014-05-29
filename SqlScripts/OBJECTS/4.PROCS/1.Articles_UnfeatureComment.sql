EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Articles_UnfeatureComment',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataTable',
    /* DataTableNames_Csv      */ 'FeaturedComments',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Articles_UnfeatureComment]') IS NOT NULL
	DROP PROCEDURE [Articles_UnfeatureComment]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Articles_UnfeatureComment]
(
	@Article_Id INT,
    @Discourse_Post_Id INT
)
AS
BEGIN


    DELETE [FeaturedComments]
    WHERE Article_Id = @Article_Id 
      AND [Discourse_Post_Id] = @Discourse_Post_Id

END
GO

GRANT EXECUTE ON [Articles_UnfeatureComment] TO [TheDailyWtfUser_Role]
GO
