IF OBJECT_ID('[__StoredProcInfo]') IS NOT NULL
	DROP TABLE [__StoredProcInfo]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [__StoredProcInfo]
(
    [StoredProc_Name] SYSNAME,
    CONSTRAINT [PK__StoredProcInfo]
        PRIMARY KEY ([StoredProc_Name]),
    [Internal_Indicator] YNINDICATOR,
    [ReturnType_Name] VARCHAR(100) NULL,
    CONSTRAINT [CK__StoredProcInfo__ReturnType_Name_Domain]
        CHECK ([ReturnType_Name] IS NULL OR [ReturnType_Name] IN ('void', 'DataRow', 'DataTable', 'DataSet')),
    [DataTableNames_Csv] VARCHAR(255) NULL,
    [OutputPropertyNames_Csv] VARCHAR(255) NULL,
    [Description_Text] VARCHAR(MAX) NULL,
	[Remarks_Text] VARCHAR(MAX) NULL
)
GO

IF OBJECT_ID('[__AddStoredProcInfo]') IS NOT NULL
	DROP PROCEDURE [__AddStoredProcInfo]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [__AddStoredProcInfo]
(
	@StoredProc_Name SYSNAME,
	@Internal_Indicator YNINDICATOR,
	@ReturnType_Name VARCHAR(100),
	@DataTableNames_Csv VARCHAR(255),
	@OutputPropertyNames_Csv VARCHAR(100),
	@Description_Text VARCHAR(MAX)
)
AS
BEGIN
	
	IF EXISTS(SELECT * FROM [__StoredProcInfo] WHERE [StoredProc_Name]=@StoredProc_Name)
		DELETE [__StoredProcInfo] WHERE [StoredProc_Name]=@StoredProc_Name

    INSERT INTO [__StoredProcInfo]
    (
         [StoredProc_Name]
        ,[Internal_Indicator]
        ,[ReturnType_Name]
        ,[DataTableNames_Csv]
        ,[OutputPropertyNames_Csv]
        ,[Description_Text]
    )
    VALUES
    (
         @StoredProc_Name
	    ,@Internal_Indicator
	    ,@ReturnType_Name
	    ,@DataTableNames_Csv
	    ,@OutputPropertyNames_Csv
	    ,@Description_Text
    )

END
GO

IF OBJECT_ID('[__GetStoredProcInfo]') IS NOT NULL
	DROP PROCEDURE [__GetStoredProcInfo]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [__GetStoredProcInfo]
(
	@StoredProc_Name SYSNAME = NULL
)
AS
BEGIN
    
    SET NOCOUNT ON
    
	IF @StoredProc_Name IS NULL
		SELECT * FROM [__StoredProcInfo]
	ELSE
		SELECT * FROM [__StoredProcInfo] WHERE [StoredProc_Name] = @StoredProc_Name

END
GO

EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ '__GetStoredProcInfo',
    /* Internal_Indicator      */ 'Y',
    /* ReturnType_Name         */ 'DataTable',
    /* DataTableNames_Csv      */ '__StoredProcInfo',
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ 'Stored procedure for internal use only.'
GO
EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ '__AddStoredProcInfo',
    /* Internal_Indicator      */ 'Y',
    /* ReturnType_Name         */ NULL,
    /* DataTableNames_Csv      */ NULL,
    /* OutputPropertyNames_Csv */ NULL,
    /* Description_Text        */ 'Stored procedure for internal use only.'
GO