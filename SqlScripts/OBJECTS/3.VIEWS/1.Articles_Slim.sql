IF OBJECT_ID('[Articles_Slim]') IS NOT NULL DROP VIEW [Articles_Slim]
GO

CREATE VIEW [Articles_Slim] AS

    SELECT [Article_Id],
	       [Article_Slug]
	  FROM [Articles]

GO

