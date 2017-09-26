IF OBJECT_ID('[Comments_Extended]') IS NOT NULL DROP VIEW [Comments_Extended]
GO

CREATE VIEW [Comments_Extended] AS

    SELECT CES.[Comment_Id],
           CI.[Comment_Index],
           CES.[Article_Id],
           CES.[Article_Title],
           CES.[Body_Html],
           CES.[User_Name],
           CES.[Posted_Date],
           CES.[Discourse_Post_Id],
           CES.[Featured_Indicator],
           CES.[Hidden_Indicator],
           CES.[User_IP],
           CES.[User_Token],
           CES.[Parent_Comment_Id],
           PI.[Comment_Index] [Parent_Comment_Index],
           CES.[Parent_Comment_User_Name]
      FROM [Comments_Extended_Slim] CES
     INNER JOIN [Comments_Index] CI
             ON CES.[Comment_Id] = CI.[Comment_Id]
      LEFT OUTER JOIN [Comments_Index] PI
                   ON CES.[Parent_Comment_Id] = PI.[Comment_Id]

GO

