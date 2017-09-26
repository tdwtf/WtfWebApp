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


    SELECT C.*, CI.[Comment_Index], PI.[Comment_Index] [Parent_Comment_Index]
      FROM [Comments_Extended_Slim] C
     INNER JOIN [Comment_Index] CI
             ON C.[Comment_Id] = CI.[Comment_Id]
      LEFT OUTER JOIN [Comment_Index] PI
                   ON C.[Parent_Comment_Id] = PI.[Comment_Id]
     WHERE C.[Article_Id] = @Article_Id
       AND C.[Featured_Indicator] = 'Y'
     ORDER BY C.[Posted_Date] ASC, C.[Comment_Id] ASC

END
GO

GRANT EXECUTE ON [Articles_GetFeaturedComments] TO [TheDailyWtfUser_Role]
GO
