EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Authors_ValidateLogin',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'void',
    /* DataTableNames_Csv      */ NULL,
    /* OutputPropertyNames_Csv */ 'Valid_Indicator',
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Authors_ValidateLogin]') IS NOT NULL
	DROP PROCEDURE [Authors_ValidateLogin]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Authors_ValidateLogin]
(
    @Author_Slug NVARCHAR(255),
	@Password_Text VARCHAR(255),
    @Valid_Indicator YNINDICATOR = NULL OUT
)
AS
BEGIN

    SET @Valid_Indicator = COALESCE(
		(SELECT 'Y' FROM [Authors] 
		  WHERE [Author_Slug] = @Author_Slug
		    AND [Password_Bytes] = HASHBYTES('SHA1', [Salt_Bytes] + CAST(@Password_Text AS VARBINARY))),
		'N')

END
GO

GRANT EXECUTE ON [Authors_ValidateLogin] TO [TheDailyWtfUser_Role]
GO
