TRUNCATE TABLE [TheDailyWtf2]..[Comments]
GO

-- BEGIN test database only
/*
DELETE FROM [TheDailyWtf2]..[Articles]
DELETE FROM [TheDailyWtf2]..[ArticlePostMappings]
DBCC CHECKIDENT ('TheDailyWtf2.dbo.Articles', RESEED, 0)

DECLARE the_article CURSOR FAST_FORWARD
FOR SELECT [Article_Id], [Publish_Date], [PublishStatus_Name], [Title_Text], [Author_Name], [Series_Name], [Body_Text], [UrlTitle_Text]
      FROM [WorseThanFailure]..[Articles]
     WHERE [Language_Code] = 'EN'
OPEN the_article

DECLARE @Post_Id int,
        @Published_Date datetime,
		@PublishedStatus_Name varchar(15),
		@Title_Text nvarchar(200),
		@Author_Name nvarchar(65),
		@Series_Name nvarchar(100),
		@Body_Html nvarchar(max),
		@Article_Slug varchar(500),
		@Author_Slug nvarchar(255),
		@Series_Slug nvarchar(255)

FETCH NEXT FROM the_article INTO @Post_Id, @Published_Date, @PublishedStatus_Name, @Title_Text, @Author_Name, @Series_Name, @Body_Html, @Article_Slug
WHILE @@FETCH_STATUS = 0
BEGIN
SELECT @Author_Slug = [Author_Slug] FROM [TheDailyWtf2]..[Authors] WHERE [Display_Name] = @Author_Name
SELECT @Series_Slug = [Series_Slug] FROM [TheDailyWtf2]..[Series] WHERE [Title_Text] = CASE WHEN @Series_Name = 'Announcement' THEN 'Announcements' ELSE @Series_Name END

INSERT INTO [TheDailyWtf2]..[Articles] ([Article_Slug], [Published_Date], [PublishedStatus_Name], [Author_Slug], [Title_Text], [Series_Slug], [Body_Html], [Discourse_Topic_Opened])
VALUES (@Article_Slug, @Published_Date, @PublishedStatus_Name, @Author_Slug, @Title_Text, @Series_Slug, @Body_Html, 'N')

INSERT INTO [TheDailyWtf2]..[ArticlePostMappings] ([Article_Id], [Post_Id])
VALUES (SCOPE_IDENTITY(), @Post_Id)

FETCH NEXT FROM the_article INTO @Post_Id, @Published_Date, @PublishedStatus_Name, @Title_Text, @Author_Name, @Series_Name, @Body_Html, @Article_Slug
END

CLOSE the_article
DEALLOCATE the_article
*/
-- END test database only
GO

DECLARE the_comment CURSOR FAST_FORWARD
FOR SELECT c.[Comment_Id], a.[Article_Id], c.[Parent_Comment_Id], c.[Publish_Date], c.[Author_Name], c.[Body_Text], c.[Featured_Indicator], CASE WHEN c.[Author_User_Id] = 1001 THEN NULL ELSE c.[Author_User_Id] END
      FROM [WorseThanFailure]..[Comments] c
INNER JOIN [TheDailyWtf2]..[ArticlePostMappings] a
        ON c.[Article_Id] = a.[Post_Id]
OPEN the_comment

DECLARE @CommentIdMappings TABLE (
	New_Id int,
	Old_Id int
)

DECLARE @Comment_Id int, @Article_Id int, @Parent_Comment_Id int, @Posted_Date datetime, @User_Name nvarchar(65), @Body_Html nvarchar(max), @Featured_Indicator YNINDICATOR, @User_Id int

FETCH NEXT FROM the_comment INTO @Comment_Id, @Article_Id, @Parent_Comment_Id, @Posted_Date, @User_Name, @Body_Html, @Featured_Indicator, @User_Id
WHILE @@FETCH_STATUS = 0
BEGIN
INSERT INTO [TheDailyWtf2]..[Comments] ([Article_Id], [Body_Html], [User_Name], [Posted_Date], [Featured_Indicator], [User_Token])
VALUES (@Article_Id, @Body_Html, @User_Name, @Posted_Date, @Featured_Indicator, 'cs:' + CAST(@User_Id AS varchar(max)))

INSERT INTO @CommentIdMappings ([New_Id], [Old_Id])
VALUES (SCOPE_IDENTITY(), @Comment_Id)

FETCH NEXT FROM the_comment INTO @Comment_Id, @Article_Id, @Parent_Comment_Id, @Posted_Date, @User_Name, @Body_Html, @Featured_Indicator, @User_Id
END

CLOSE the_comment
DEALLOCATE the_comment

UPDATE [TheDailyWtf2]..[Comments]
   SET [Parent_Comment_Id] = parent.[New_Id]
  FROM [WorseThanFailure]..[Comments] old
  JOIN @CommentIdMappings child
    ON child.[Old_Id] = old.[Comment_Id]
  JOIN @CommentIdMappings parent
    ON parent.[Old_Id] = old.[Parent_Comment_Id]
 WHERE [TheDailyWtf2]..[Comments].[Comment_Id] = child.[New_Id]
   AND old.[Parent_Comment_Id] IS NOT NULL

GO