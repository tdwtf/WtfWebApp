EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Authors_CreateOrUpdateAuthor',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'void',
    /* DataTableNames_Csv      */ NULL,
    /* OutputPropertyNames_Csv */ 'Author_Slug',
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Authors_CreateOrUpdateAuthor]') IS NOT NULL
	DROP PROCEDURE [Authors_CreateOrUpdateAuthor]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Authors_CreateOrUpdateAuthor]
(
    @Author_Slug NVARCHAR(255) OUT,
    @Display_Name NVARCHAR(255),
    @Admin_Indicator YNINDICATOR,
    @Bio_Html NVARCHAR(MAX),
    @ShortBio_Text NVARCHAR(MAX),
    @Image_Url NVARCHAR(255)
)
AS
BEGIN

    IF (EXISTS(SELECT * FROM [Authors] WHERE [Author_Slug] = @Author_Slug))
    BEGIN

        UPDATE [Authors]
           SET 
                [Author_Slug] = @Author_Slug
               ,[Display_Name] = @Display_Name
               ,[Admin_Indicator] = @Admin_Indicator
               ,[Bio_Html] = @Bio_Html
               ,[ShortBio_Text] = @ShortBio_Text
               ,[Image_Url] = @Image_Url
         WHERE [Author_Slug] = @Author_Slug

    END
    ELSE
    BEGIN

        INSERT INTO [Authors]
        (
            [Author_Slug]
           ,[Display_Name]
           ,[Admin_Indicator]
           ,[Bio_Html]
           ,[ShortBio_Text]
           ,[Image_Url]
        )
        VALUES
        (
            @Author_Slug
           ,@Display_Name
           ,@Admin_Indicator
           ,@Bio_Html
           ,@ShortBio_Text
           ,@Image_Url
        )

    END

END
GO

GRANT EXECUTE ON [Authors_CreateOrUpdateAuthor] TO [TheDailyWtfUser_Role]
GO
