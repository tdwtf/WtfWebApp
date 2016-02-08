EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Comments_GetCommentsByToken',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataTable',
    /* DataTableNames_Csv      */ 'Comments',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Comments_GetCommentsByToken]') IS NOT NULL
	DROP PROCEDURE [Comments_GetCommentsByToken]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Comments_GetCommentsByToken]
(
	@User_Token varchar(max)
)
AS
BEGIN

    SELECT * FROM [Comments]
            WHERE [User_Token] = @User_Token
         ORDER BY [Posted_Date] ASC

END
GO

GRANT EXECUTE ON [Comments_GetCommentsByToken] TO [TheDailyWtfUser_Role]
GO
