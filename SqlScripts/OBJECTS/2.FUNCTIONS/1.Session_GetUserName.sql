
IF OBJECT_ID('Session_GetUserName') IS NOT NULL DROP FUNCTION Session_GetUserName
GO

CREATE FUNCTION Session_GetUserName () RETURNS VARCHAR(MAX)
AS BEGIN

	DECLARE @Context_Info VARCHAR(MAX)
	SET @Context_Info = CAST(CONTEXT_INFO() AS VARCHAR(MAX))

	IF CHARINDEX(CHAR(0), @Context_Info) > 0
		SET @Context_Info = SUBSTRING(@Context_Info, 0, CHARINDEX(CHAR(0), @Context_Info))
	RETURN COALESCE(@Context_Info, 'UNKNOWN')

END
GO