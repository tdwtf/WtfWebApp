IF OBJECT_ID('[Comments_Extended_Slim]') IS NOT NULL DROP VIEW [Comments_Extended_Slim]
GO
IF OBJECT_ID('[Comments_Index]') IS NOT NULL DROP VIEW [Comments_Index]
GO
IF OBJECT_ID('[Comments_Extended]') IS NOT NULL DROP VIEW [Comments_Extended]
GO

CREATE VIEW [Comments_Extended] AS

    SELECT C.[Comment_Id],
           C.[Article_Id],
           A.[Title_Text] [Article_Title],
           C.[Body_Html],
           C.[User_Name],
           C.[Posted_Date],
           C.[Discourse_Post_Id],
           C.[Featured_Indicator],
           C.[Hidden_Indicator],
           C.[User_IP],
           C.[User_Token],
           C.[Parent_Comment_Id],
           P.[User_Name] [Parent_Comment_User_Name]
      FROM [Comments] C
     INNER JOIN [Articles] A
             ON C.[Article_Id] = A.[Article_Id]
      LEFT OUTER JOIN [Comments] P
                   ON C.[Parent_Comment_Id] = P.[Comment_Id]

GO

