EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Comments_CountCommentsByToken',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'void',
    /* DataTableNames_Csv      */ '',
    /* OutputPropertyNames_Csv */ 'Comments_Count',
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Comments_CountCommentsByToken]') IS NOT NULL
	DROP PROCEDURE [Comments_CountCommentsByToken]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Comments_CountCommentsByToken]
(
    @User_Token VARCHAR(MAX),
    @Comments_Count INT = NULL OUT
)
AS
BEGIN

    SET @Comments_Count = (SELECT COUNT(*) FROM [Comments] C
                            WHERE C.[User_Token] = @User_Token)

END
GO

GRANT EXECUTE ON [Comments_CountCommentsByToken] TO [TheDailyWtfUser_Role]
GO
