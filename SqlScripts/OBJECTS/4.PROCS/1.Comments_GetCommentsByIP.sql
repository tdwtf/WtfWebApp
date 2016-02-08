EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Comments_GetCommentsByIP',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataTable',
    /* DataTableNames_Csv      */ 'Comments',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Comments_GetCommentsByIP]') IS NOT NULL
	DROP PROCEDURE [Comments_GetCommentsByIP]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Comments_GetCommentsByIP]
(
	@User_IP varchar(45)
)
AS
BEGIN

    SELECT * FROM [Comments]
            WHERE [User_IP] = @User_IP
         ORDER BY [Posted_Date] ASC

END
GO

GRANT EXECUTE ON [Comments_GetCommentsByIP] TO [TheDailyWtfUser_Role]
GO
