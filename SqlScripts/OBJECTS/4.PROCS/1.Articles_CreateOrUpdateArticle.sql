EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Articles_CreateOrUpdateArticle',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'void',
    /* DataTableNames_Csv      */ NULL,
    /* OutputPropertyNames_Csv */ 'Article_Id',
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Articles_CreateOrUpdateArticle]') IS NOT NULL
	DROP PROCEDURE [Articles_CreateOrUpdateArticle]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Articles_CreateOrUpdateArticle]
(
	@Article_Id INT OUT,
    @Article_Slug NVARCHAR(255) = NULL,
    @Published_Date DATETIME = NULL,
    @PublishedStatus_Name VARCHAR(15) = NULL,
    @Author_Slug NVARCHAR(255) = NULL,
    @Title_Text NVARCHAR(255) = NULL,
    @Series_Slug NVARCHAR(255) = NULL,
    @Body_Html NVARCHAR(MAX) = NULL,
    @Discourse_Topic_Id INT = NULL,
    @Discourse_Topic_Opened YNINDICATOR = NULL
)
AS
BEGIN

    IF (EXISTS(SELECT * FROM [Articles] WHERE [Article_Id] = @Article_Id))
    BEGIN

        UPDATE [Articles]
           SET 
                 [Article_Slug] = COALESCE(@Article_Slug, [Article_Slug])
                ,[Published_Date] = COALESCE(@Published_Date, [Published_Date])
                ,[PublishedStatus_Name] = COALESCE(@PublishedStatus_Name, [PublishedStatus_Name])
                ,[Author_Slug] = COALESCE(@Author_Slug, [Author_Slug])
                ,[Title_Text] = COALESCE(@Title_Text, [Title_Text])
                ,[Series_Slug] = COALESCE(@Series_Slug, [Series_Slug])
                ,[Body_Html] = COALESCE(@Body_Html, [Body_Html])
                ,[Discourse_Topic_Id] = @Discourse_Topic_Id
                ,[Discourse_Topic_Opened] = COALESCE(@Discourse_Topic_Opened, [Discourse_Topic_Opened])
         WHERE [Article_Id] = @Article_Id

    END
    ELSE
    BEGIN

        INSERT INTO [Articles]
        (
            [Article_Slug]
           ,[Published_Date]
           ,[PublishedStatus_Name]
           ,[Author_Slug]
           ,[Title_Text]
           ,[Series_Slug]
           ,[Body_Html]
           ,[Discourse_Topic_Id]
           ,[Discourse_Topic_Opened]
        )
        VALUES
        (
            @Article_Slug
           ,@Published_Date
           ,@PublishedStatus_Name
           ,@Author_Slug
           ,@Title_Text
           ,@Series_Slug
           ,@Body_Html
           ,@Discourse_Topic_Id
           ,@Discourse_Topic_Opened
        )

        SET @Article_Id = SCOPE_IDENTITY()

    END

END
GO

GRANT EXECUTE ON [Articles_CreateOrUpdateArticle] TO [TheDailyWtfUser_Role]
GO
