--AH:ScriptId=8100d356-2068-43ba-9a1a-3832732eb818;1739
CREATE ROLE [TheDailyWtfUser_Role]
GO

CREATE TYPE YNINDICATOR FROM CHAR(1)
GO

CREATE RULE YNINDICATOR_Domain
    AS @Ind COLLATE Latin1_General_BIN IN ('Y','N')
GO

EXEC sp_bindrule 'YNINDICATOR_Domain', 'YNINDICATOR'
GO

CREATE TABLE [Authors]
(
    [Author_Slug] NVARCHAR(255) NOT NULL,
        CONSTRAINT [PK__Authors] PRIMARY KEY ([Author_Slug]),
    [Display_Name] NVARCHAR(255) NOT NULL,
    [Password_Bytes] BINARY(20) NULL,
    [Salt_Bytes] BINARY(10) NULL,
    [Admin_Indicator] YNINDICATOR NOT NULL,
    [Bio_Html] NVARCHAR(MAX) NULL,
    [ShortBio_Text] NVARCHAR(MAX) NULL,
    [Image_Url] NVARCHAR(255) NULL
)
GO

INSERT [Authors] ([Author_Slug], [Display_Name], [Admin_Indicator])
  SELECT 'alex-papadimoulis', 'Alex Papadimoulis', 'Y' UNION ALL
  SELECT 'mark-bowytz', 'Mark Bowytz', 'Y' UNION ALL
  SELECT 'remy-porter', 'Remy Porter', 'Y' UNION ALL
  SELECT 'john-rasch', 'John Rasch', 'Y'
GO

CREATE PROCEDURE [Authors_SetPassword]
(
    @Author_Slug NVARCHAR(255),
	@Password_Text VARCHAR(255)
)
AS
BEGIN

    DECLARE @Salt_Bytes BINARY(10)
	DECLARE @Password_Bytes BINARY(20)
	
	SET @Salt_Bytes = CAST(NEWID() AS BINARY(10))
	SET @Password_Bytes = HASHBYTES('SHA1', @Salt_Bytes + CAST(@Password_Text AS VARBINARY))
	
	UPDATE [Authors]
	   SET [Password_Bytes] = @Password_Bytes
	      ,[Salt_Bytes] = @Salt_Bytes
	 WHERE [Author_Slug] = @Author_Slug

END
GO

GRANT EXECUTE ON [Authors_SetPassword] TO [TheDailyWtfUser_Role]
GO

EXEC [Authors_SetPassword] 'alex-papadimoulis', 'changeme1000!'
EXEC [Authors_SetPassword] 'mark-bowytz', 'changeme1000!'
EXEC [Authors_SetPassword] 'remy-porter', 'changeme1000!'
EXEC [Authors_SetPassword] 'john-rasch', 'changeme1000!'

CREATE TABLE [Series]
(
    [Series_Slug] NVARCHAR(255) NOT NULL,
        CONSTRAINT [PK__Series] PRIMARY KEY ([Series_Slug]),
    [Title_Text] NVARCHAR(255) NOT NULL,
    [Description_Text] NVARCHAR(MAX) NULL
)
GO

  INSERT [Series] ([Series_Slug], [Title_Text], [Description_Text])
    SELECT 'alexs-soapbox', N'Alex''s Soapbox', N'Alex''s very own soapbox for all things software and technology.' UNION ALL
    SELECT 'announcements', N'Announcements', N'' UNION ALL
    SELECT 'best-of-the-sidebar', N'Best of the Sidebar', N'It''s the Best of the Sidebar. What, did you expect more from a title like that?' UNION ALL
    SELECT 'bring-your-own-code', N'Bring Your Own Code', N'The goal of BYOC is simple: provide an outlet for you, the enquiring software developer, to sharpen your programming skills on a problem a bit more interesting than the normal, boring stuff. That, and to put your code where you mouth is, so to say.' UNION ALL
    SELECT 'coded-smorgasbord', N'Coded Smorgasbord', N'Inspired by the Pop-up Potpourri, the examples presented here aren''t necessarily "bad" code nor do they imply the miserable failure that we''re all used to reading here. The examples are more-or-less fun snippets of code like ones that we''ve all written at one time or another.' UNION ALL
    SELECT 'code-sod', N'CodeSOD', N'Code Snippet Of the Day (CodeSOD) features interesting and usually incorrect code snippets taken from actual production code in a commercial and/or open source software projects.' UNION ALL
    SELECT 'errord', N'Error''d', N'Error''d features fun error messages and other visual oddities from the world of IT.' UNION ALL
    SELECT 'feature-articles', N'Feature Articles', N'' UNION ALL
    SELECT 'mandatory-fun-day', N'Mandatory Fun Day', N'A web comic about software, technology, and life as a corporate code monkey.' UNION ALL
    SELECT 'off-topic', N'Off Topic', N'' UNION ALL
    SELECT 'pop-up-potpourri', N'Pop-up Potpourri', N'A collection of humorous and off-beat, generally computer-generated, error messages.' UNION ALL
    SELECT 'representative-line', N'Representative Line', N'A single line of code from a large application that somehow manages to provide an almost endless insight into the pain that its maintainers face each day.' UNION ALL
    SELECT 'souvenir-potpourri', N'Souvenir Potpourri', N'' UNION ALL
    SELECT 'tales-from-the-interview', N'Tales from the Interview', N'Job interview stories.' UNION ALL
    SELECT 'virtudyne', N'Virtudyne', N'A four part series that tells of the rise and fall of Virtudyne, one of the largest privately-financed ($200M) disasters in our industry.'
GO

CREATE TABLE [Articles]
(
    [Article_Id] INT IDENTITY(1, 1) NOT NULL, 
        CONSTRAINT [PK__Articles] PRIMARY KEY ([Article_Id]),
    [Article_Slug] NVARCHAR(255) NOT NULL,
    [Published_Date] DATETIME NULL,
    [PublishedStatus_Name] VARCHAR(15) NOT NULL,
    [Author_Slug] NVARCHAR(255) NOT NULL,
        CONSTRAINT [FK__Articles__Authors] FOREIGN KEY ([Author_Slug])
            REFERENCES [Authors] ([Author_Slug]),
    [Title_Text] NVARCHAR(255) NOT NULL,
    [Series_Slug] NVARCHAR(255) NOT NULL,
        CONSTRAINT [FK__Articles__Series] FOREIGN KEY ([Series_Slug])
            REFERENCES [Series] ([Series_Slug]),
    [Body_Html] NVARCHAR(MAX) NOT NULL,
    [Discourse_Topic_Id] INT NULL,
    [Discourse_Topic_Opened] YNINDICATOR NOT NULL
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX__Articles__Article_Slug] ON [Articles] ([Article_Slug])
GO

CREATE TABLE [FeaturedComments]
(
    [Article_Id] INT NOT NULL,
    [Discourse_Post_Id] INT NOT NULL,

    CONSTRAINT [PK__FeaturedComments] PRIMARY KEY ([Article_Id], [Discourse_Post_Id]),

    CONSTRAINT [FK__FeaturedComments__Articles] FOREIGN KEY ([Article_Id])
        REFERENCES [Articles] ([Article_Id])
)
GO
