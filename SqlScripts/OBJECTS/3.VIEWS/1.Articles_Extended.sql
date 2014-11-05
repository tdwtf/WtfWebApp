IF OBJECT_ID('[Articles_Extended]') IS NOT NULL DROP VIEW [Articles_Extended]
GO

CREATE VIEW [Articles_Extended] AS

    SELECT 
             ART.[Article_Id]
            ,ART.[Article_Slug]
            ,ART.[Published_Date]
            ,ART.[PublishedStatus_Name]
            ,ART.[Author_Slug]
            ,ART.[Title_Text]
            ,ART.[Series_Slug]
            ,[Body_Html] = CONCAT(ART.[Body_Html], ADS.[Ad_Html])
            ,ART.[Discourse_Topic_Id]
            ,ART.[Discourse_Topic_Opened]

            ,[Previous_Article_Id] = ART_PREV.[Article_Id]
            ,[Previous_Article_Slug] = ART_PREV.[Article_Slug]
            ,[Previous_Title_Text] = ART_PREV.[Title_Text]

            ,[Next_Article_Id] = ART_NEXT.[Article_Id]
            ,[Next_Article_Slug] = ART_NEXT.[Article_Slug]
            ,[Next_Title_Text] = ART_NEXT.[Title_Text]

            ,[Author_Display_Name] = AUTH.[Display_Name]
            ,[Author_Admin_Indicator] = AUTH.[Admin_Indicator]
            ,[Author_Bio_Html] = AUTH.[Bio_Html]
            ,[Author_ShortBio_Text] = AUTH.[ShortBio_Text]
            ,[Author_Image_Url] = AUTH.[Image_Url]
            ,[Author_Active_Indicator] = AUTH.[Active_Indicator]

            ,[Series_Title_Text] = S.[Title_Text]
            ,[Series_Description_Text] = S.[Description_Text]

            ,[Cached_Comment_Count] = (SELECT COUNT(*) FROM [Comments] WHERE [Article_Id] = ART.[Article_Id])
            ,[Last_Comment_Date] = (SELECT MAX([Posted_Date]) FROM [Comments] WHERE [Article_Id] = ART.[Article_Id])

            ,[Ad_Html] = ADS.[Ad_Html]

      FROM [Articles] ART    
    
  INNER JOIN [Series] S 
          ON S.[Series_Slug] = ART.[Series_Slug]
    
  INNER JOIN [Authors] AUTH
          ON AUTH.[Author_Slug] = ART.[Author_Slug]

   LEFT JOIN [Ads] ADS
          ON ADS.[Ad_Id] = ART.[Ad_Id]

   LEFT JOIN [Articles] ART_PREV
          ON ART_PREV.[Article_Id] = (SELECT TOP 1 [Article_Id] 
                                        FROM [Articles] 
                                       WHERE [PublishedStatus_Name] = 'Published'
                                         AND [Published_Date] < ART.[Published_Date] 
                                    ORDER BY [Published_Date] DESC)

   LEFT JOIN [Articles] ART_NEXT
          ON ART_NEXT.[Article_Id] = (SELECT TOP 1 [Article_Id] 
                                        FROM [Articles] 
                                       WHERE [PublishedStatus_Name] = 'Published'
                                         AND [Published_Date] > ART.[Published_Date] 
                                         AND [Published_Date] < GETDATE()
                                    ORDER BY [Published_Date] ASC)

GO

