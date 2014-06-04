EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Authors_GetAuthors',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataTable',
    /* DataTableNames_Csv      */ 'Authors',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Authors_GetAuthors]') IS NOT NULL
	DROP PROCEDURE [Authors_GetAuthors]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Authors_GetAuthors]
AS
BEGIN

    SELECT * FROM [Authors]

END
GO

GRANT EXECUTE ON [Authors_GetAuthors] TO [TheDailyWtfUser_Role]
GO
