EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Comments_CountCommentsByIP',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'void',
    /* DataTableNames_Csv      */ '',
    /* OutputPropertyNames_Csv */ 'Comments_Count',
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Comments_CountCommentsByIP]') IS NOT NULL
	DROP PROCEDURE [Comments_CountCommentsByIP]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Comments_CountCommentsByIP]
(
    @User_IP VARCHAR(45),
    @Comments_Count INT = NULL OUT
)
AS
BEGIN

    SET @Comments_Count = (SELECT COUNT(*) FROM [Comments] C
                            WHERE C.[User_IP] = @User_IP)

END
GO

GRANT EXECUTE ON [Comments_CountCommentsByIP] TO [TheDailyWtfUser_Role]
GO
