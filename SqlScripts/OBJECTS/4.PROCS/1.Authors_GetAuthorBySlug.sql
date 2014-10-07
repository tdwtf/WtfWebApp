EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Authors_GetAuthorBySlug',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataRow',
    /* DataTableNames_Csv      */ 'Authors',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Authors_GetAuthorBySlug]') IS NOT NULL
	DROP PROCEDURE [Authors_GetAuthorBySlug]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Authors_GetAuthorBySlug]
(
	@Author_Slug NVARCHAR(255)
)
AS
BEGIN

    SELECT * FROM [Authors]
            WHERE [Author_Slug] = @Author_Slug

END
GO

GRANT EXECUTE ON [Authors_GetAuthorBySlug] TO [TheDailyWtfUser_Role]
GO
