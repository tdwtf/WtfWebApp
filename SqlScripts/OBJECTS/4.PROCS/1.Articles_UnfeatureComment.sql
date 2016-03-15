EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Articles_UnfeatureComment',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'void',
    /* DataTableNames_Csv      */ NULL,
    /* OutputPropertyNames_Csv */ 'Valid_Indicator',
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Articles_UnfeatureComment]') IS NOT NULL
    DROP PROCEDURE [Articles_UnfeatureComment]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Articles_UnfeatureComment]
(
    @Article_Id INT,
    @Comment_Id INT,
    @Valid_Indicator YNINDICATOR = NULL OUT
)
AS
BEGIN

    IF (EXISTS(SELECT * FROM [COMMENTS] WHERE [Article_Id] = @Article_Id AND [Comment_Id] = @Comment_Id))
    BEGIN
        SET @Valid_Indicator = 'Y'

        UPDATE [Comments]
           SET [Featured_Indicator] = 'N'
         WHERE [Comment_Id] = @Comment_Id
    END
    ELSE
    BEGIN
        SET @Valid_Indicator = 'N'
    END
END
GO

GRANT EXECUTE ON [Articles_UnfeatureComment] TO [TheDailyWtfUser_Role]
GO
