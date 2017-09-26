IF OBJECT_ID('[Comments_Extended]') IS NOT NULL DROP VIEW [Comments_Extended]
GO

CREATE VIEW [Comments_Extended] AS

    SELECT C.[Comment_Id],
           CAST(ROW_NUMBER() OVER(PARTITION BY C.[Article_Id] ORDER BY C.[Posted_Date] ASC, C.[Comment_Id] ASC) AS INT) [Comment_Index],
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
           (SELECT PC.RN
              FROM (SELECT [Comment_Id] ID,
                           CAST(ROW_NUMBER() OVER(PARTITION BY [Article_Id] ORDER BY [Posted_Date] ASC, [Comment_Id] ASC) AS INT) RN
                      FROM [Comments]
                     WHERE [Article_Id] = P.[Article_Id]) PC
            WHERE PC.ID = C.[Parent_Comment_Id]) [Parent_Comment_Index],
           P.[User_Name] [Parent_Comment_User_Name]
      FROM [Comments] C
     INNER JOIN [Articles] A
             ON C.[Article_Id] = A.[Article_Id]
      LEFT OUTER JOIN [Comments] P
             ON C.[Parent_Comment_Id] = P.[Comment_Id]

GO

