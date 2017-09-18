EXEC [__AddStoredProcInfo]
    /* StoredProc_Name         */ 'Comments_UserHasApprovedComment',
    /* Internal_Indicator      */ 'N',
    /* ReturnType_Name         */ 'void',
    /* DataTableNames_Csv      */ NULL,
    /* OutputPropertyNames_Csv */ 'Exists_Indicator',
    /* Description_Text        */ NULL
GO

IF OBJECT_ID('[Comments_UserHasApprovedComment]') IS NOT NULL
    DROP PROCEDURE [Comments_UserHasApprovedComment]
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Comments_UserHasApprovedComment]
(
    @User_IP VARCHAR(45),
    @User_Token VARCHAR(MAX),
    @Exists_Indicator YNINDICATOR = NULL OUT
)
AS
BEGIN

    IF (EXISTS(SELECT * FROM [Comments]
                WHERE [Hidden_Indicator] = 'N'
                  AND ([User_IP] = @User_IP
                   OR [User_Token] = @User_Token)))
    BEGIN
        SET @Exists_Indicator = 'Y'
    END
    ELSE
    BEGIN
        SET @Exists_Indicator = 'N'
    END
END
GO

GRANT EXECUTE ON [Comments_UserHasApprovedComment] TO [TheDailyWtfUser_Role]
GO
