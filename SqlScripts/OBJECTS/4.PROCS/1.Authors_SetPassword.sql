EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Authors_SetPassword',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'void',
    /* DataTableNames_Csv      */ NULL,
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Authors_SetPassword]') IS NOT NULL
	DROP PROCEDURE [Authors_SetPassword]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Authors_SetPassword]
(
    @Author_Slug NVARCHAR(255),
	@Password_Text VARCHAR(255)
)
AS
BEGIN

    DECLARE @Salt_Bytes BINARY(10)
	DECLARE @Password_Bytes BINARY(20)
	
	SET @Salt_Bytes = CAST(NEWID() AS BINARY(10))
	SET @Password_Bytes = HASHBYTES('SHA1', @Salt_Bytes + CAST(@Password_Text AS VARBINARY))
	
	UPDATE [Authors]
	   SET [Password_Bytes] = @Password_Bytes
	      ,[Salt_Bytes] = @Salt_Bytes
	 WHERE [Author_Slug] = @Author_Slug

END
GO

GRANT EXECUTE ON [Authors_SetPassword] TO [TheDailyWtfUser_Role]
GO
