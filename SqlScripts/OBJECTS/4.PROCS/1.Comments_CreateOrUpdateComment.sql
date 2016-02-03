EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Comments_CreateOrUpdateComment',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'void',
    /* DataTableNames_Csv      */ NULL,
    /* OutputPropertyNames_Csv */ 'Comment_Id',
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Comments_CreateOrUpdateComment]') IS NOT NULL
	DROP PROCEDURE [Comments_CreateOrUpdateComment]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Comments_CreateOrUpdateComment]
(
    @Comment_Id INT OUT,
    @Article_Id INT,
    @Body_Html NVARCHAR(MAX),
    @User_Name NVARCHAR(255),
    @Posted_Date DATETIME,
    @User_IP VARCHAR(45),
    @User_Token VARCHAR(MAX),
    @Parent_Comment_Id INT NULL
)
AS
BEGIN

    INSERT INTO [Comments]
    (
        [Article_Id],
        [Body_Html],
        [User_Name],
        [Posted_Date],
        [Featured_Indicator],
        [User_IP],
        [User_Token],
        [Parent_Comment_Id]
    )
    VALUES
    (
        @Article_Id,
        @Body_Html,
        @User_Name,
        @Posted_Date,
        'N',
        @User_IP,
        @User_Token,
        @Parent_Comment_Id
    )

    SET @Comment_Id = SCOPE_IDENTITY()

END
GO

GRANT EXECUTE ON [Comments_CreateOrUpdateComment] TO [TheDailyWtfUser_Role]
GO
