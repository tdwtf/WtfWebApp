IF OBJECT_ID('[CsvToTable]') IS NOT NULL DROP FUNCTION [CsvToTable]
GO

CREATE FUNCTION [CsvToTable]
(
    @CSVString NVARCHAR(MAX),
    @Delimiter NVARCHAR(10)
) RETURNS @tbl TABLE ([Value_Text] NVARCHAR(1000))

AS BEGIN 

	DECLARE @i INT, @j INT

	SET @i = 1 WHILE @i <= (DATALENGTH(@CSVString)/2)
	BEGIN
		SET @j = CHARINDEX(@Delimiter, @CSVString, @i)
		IF @j = 0 SET @j = (DATALENGTH(@CSVString)/2) + 1

		INSERT @tbl SELECT SUBSTRING(@CSVString, @i, @j - @i)

		SET @i = @j + (DATALENGTH(@Delimiter)/2)
	END

	RETURN
END
GO