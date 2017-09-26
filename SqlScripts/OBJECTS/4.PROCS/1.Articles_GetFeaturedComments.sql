EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Articles_GetFeaturedComments',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataTable',
    /* DataTableNames_Csv      */ 'Comments_Extended',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Articles_GetFeaturedComments]') IS NOT NULL
	DROP PROCEDURE [Articles_GetFeaturedComments]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Articles_GetFeaturedComments]
(
	@Article_Id INT
)
AS
BEGIN


    SELECT C.* FROM [Comments_Extended] C
              WHERE C.[Article_Id] = @Article_Id
                AND C.[Featured_Indicator] = 'Y'
           ORDER BY C.[Posted_Date] ASC, C.[Comment_Id] ASC

END
GO

GRANT EXECUTE ON [Articles_GetFeaturedComments] TO [TheDailyWtfUser_Role]
GO
