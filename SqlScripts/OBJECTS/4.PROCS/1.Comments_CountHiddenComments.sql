EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Comments_CountHiddenComments',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'void',
    /* DataTableNames_Csv      */ '',
    /* OutputPropertyNames_Csv */ 'Comments_Count',
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Comments_CountHiddenComments]') IS NOT NULL
	DROP PROCEDURE [Comments_CountHiddenComments]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Comments_CountHiddenComments]
(
    @Author_Slug NVARCHAR(255) = NULL,
    @Comments_Count INT = NULL OUT
)
AS
BEGIN

    SET @Comments_Count = (SELECT COUNT(*) FROM [Comments] C
                            INNER JOIN [Articles] A
                                    ON A.[Article_Id] = C.[Article_Id]
                            WHERE C.[Hidden_Indicator] = 'Y'
                              AND (@Author_Slug IS NULL OR A.[Author_Slug] = @Author_Slug))

END
GO

GRANT EXECUTE ON [Comments_CountHiddenComments] TO [TheDailyWtfUser_Role]
GO
