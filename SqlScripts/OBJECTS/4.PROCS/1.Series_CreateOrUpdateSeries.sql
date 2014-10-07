EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Series_CreateOrUpdateSeries',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataRow',
    /* DataTableNames_Csv      */ 'Articles_Extended',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Series_CreateOrUpdateSeries]') IS NOT NULL
	DROP PROCEDURE [Series_CreateOrUpdateSeries]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Series_CreateOrUpdateSeries]
(
    @Series_Slug NVARCHAR(255),
    @Title_Text NVARCHAR(255),
    @Description_Text NVARCHAR(MAX) = NULL
)
AS
BEGIN

    IF (EXISTS(SELECT * FROM [Series] WHERE [Series_Slug] = @Series_Slug))
    BEGIN

        UPDATE [Series]
           SET [Title_Text] = @Title_Text
              ,[Description_Text] = @Description_Text
         WHERE [Series_Slug] = @Series_Slug

    END
    ELSE
    BEGIN

        INSERT INTO [Series]
        (
            [Series_Slug]
           ,[Title_Text]
           ,[Description_Text]
        )
        VALUES
        (
            @Series_Slug
           ,@Title_Text
           ,@Description_Text
        )

    END

END
GO

GRANT EXECUTE ON [Series_CreateOrUpdateSeries] TO [TheDailyWtfUser_Role]
GO
