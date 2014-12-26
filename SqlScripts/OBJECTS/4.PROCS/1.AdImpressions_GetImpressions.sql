EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'AdImpressions_GetImpressions',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'DataTable',
    /* DataTableNames_Csv      */ 'AdImpressions',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[AdImpressions_GetImpressions]') IS NOT NULL
	DROP PROCEDURE [AdImpressions_GetImpressions]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [AdImpressions_GetImpressions]
(
    @Start_Date DATE = NULL,
    @End_Date DATE = NULL
)
AS
BEGIN

    SELECT * 
      FROM [AdImpressions]
     WHERE (@Start_Date IS NULL OR [Impression_Date] >= @Start_Date)
       AND (@End_Date IS NULL OR [Impression_Date] <= @End_Date)

END
GO

GRANT EXECUTE ON [AdImpressions_GetImpressions] TO [TheDailyWtfUser_Role]
GO
