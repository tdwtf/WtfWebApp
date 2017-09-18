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
    @Parent_Comment_Id INT = NULL,
    @Hidden_Indicator YNINDICATOR = NULL
)
AS
BEGIN

    IF (EXISTS(SELECT * FROM [Comments] WHERE [Comment_Id] = @Comment_Id))
    BEGIN

        UPDATE [Comments]
           SET [Body_Html] = @Body_Html,
               [User_Name] = @User_Name,
               [Hidden_Indicator] = COALESCE(@Hidden_Indicator, [Hidden_Indicator])
         WHERE [Comment_Id] = @Comment_Id

    END
    ELSE
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
            [Parent_Comment_Id],
            [Hidden_Indicator]
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
            @Parent_Comment_Id,
            COALESCE(@Hidden_Indicator, 'N')
        )

        SET @Comment_Id = SCOPE_IDENTITY()

    END

END
GO

GRANT EXECUTE ON [Comments_CreateOrUpdateComment] TO [TheDailyWtfUser_Role]
GO
