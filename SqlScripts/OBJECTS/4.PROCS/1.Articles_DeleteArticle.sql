EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Articles_DeleteArticle',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'void',
    /* DataTableNames_Csv      */ NULL,
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Articles_DeleteArticle]') IS NOT NULL
	DROP PROCEDURE [Articles_DeleteArticle]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Articles_DeleteArticle]
(
	@Article_Id INT
)
AS
BEGIN

           DELETE [Articles_Extended]
            WHERE [Article_Id] = @Article_Id

END
GO

GRANT EXECUTE ON [Articles_DeleteArticle] TO [TheDailyWtfUser_Role]
GO
