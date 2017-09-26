IF OBJECT_ID('[Comments_Index]') IS NOT NULL DROP VIEW [Comments_Index]
GO

CREATE VIEW [Comments_Index] AS

    SELECT C.[Comment_Id],
           CAST(ROW_NUMBER() OVER(PARTITION BY C.[Article_Id] ORDER BY C.[Posted_Date] ASC, C.[Comment_Id] ASC) AS INT) [Comment_Index]
      FROM [Comments] C

GO

