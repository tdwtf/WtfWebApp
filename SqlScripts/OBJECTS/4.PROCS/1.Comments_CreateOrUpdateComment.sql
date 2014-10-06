EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Comments_CreateOrUpdateComment',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'void',
    /* DataTableNames_Csv      */ NULL,
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Comments_CreateOrUpdateComment]') IS NOT NULL
	DROP PROCEDURE [Comments_CreateOrUpdateComment]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Comments_CreateOrUpdateComment]
(
    @Article_Id INT,
    @Body_Html NVARCHAR(MAX),
    @User_Name NVARCHAR(255),
    @Posted_Date DATETIME,
    @Discourse_Post_Id INT
)
AS
BEGIN

    IF (EXISTS(SELECT * FROM [Comments] WHERE [Discourse_Post_Id] = @Discourse_Post_Id))
    BEGIN

        UPDATE [Comments]
           SET [Article_Id] = @Article_Id
              ,[Body_Html] = @Body_Html
              ,[User_Name] = @User_Name
              ,[Posted_Date] = @Posted_Date
         WHERE [Discourse_Post_Id] = @Discourse_Post_Id

    END
    ELSE
    BEGIN

        INSERT INTO [Comments]
        (
            [Article_Id],
            [Body_Html],
            [User_Name],
            [Posted_Date],
            [Discourse_Post_Id]
        )
        VALUES
        (
            @Article_Id,
            @Body_Html,
            @User_Name,
            @Posted_Date,
            @Discourse_Post_Id
        )

    END

END
GO

GRANT EXECUTE ON [Comments_CreateOrUpdateComment] TO [TheDailyWtfUser_Role]
GO
