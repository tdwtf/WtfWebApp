EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'AdImpressions_IncrementCount',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'void',
    /* DataTableNames_Csv      */ NULL,
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[AdImpressions_IncrementCount]') IS NOT NULL
	DROP PROCEDURE [AdImpressions_IncrementCount]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [AdImpressions_IncrementCount]
(
    @Banner_Name VARCHAR(100),
    @Impression_Date DATE,
    @Impression_Count INT
)
AS
BEGIN

    IF EXISTS(SELECT * FROM [AdImpressions] WHERE [Banner_Name] = @Banner_Name AND [Impression_Date] = @Impression_Date)
    BEGIN

        UPDATE [AdImpressions]
           SET [Impression_Count] = [Impression_Count] + @Impression_Count
         WHERE [Banner_Name] = @Banner_Name
           AND [Impression_Date] = @Impression_Date

    END
    ELSE
    BEGIN

        INSERT INTO [AdImpressions] ([Banner_Name], [Impression_Date], [Impression_Count])
        VALUES (@Banner_Name, @Impression_Date, @Impression_Count)

    END

END
GO

GRANT EXECUTE ON [AdImpressions_IncrementCount] TO [TheDailyWtfUser_Role]
GO
