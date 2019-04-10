--AH:ScriptId=240861d8-3459-4b38-9b21-c289968594bb;3201
ALTER TABLE [Comments]
  ADD Hidden_Indicator YNINDICATOR NOT NULL DEFAULT 'N'
GO

CREATE NONCLUSTERED INDEX [IX__Comments__HiddenIndicator__PostedDate] ON [Comments] ([Hidden_Indicator], [Posted_Date])
CREATE NONCLUSTERED INDEX [IX__Comments__HiddenIndicator__UserIP] ON [Comments] ([Hidden_Indicator], [User_IP])
CREATE NONCLUSTERED INDEX [IX__Comments__ArticleId__HiddenIndicator__PostedDate] ON [Comments] ([Article_Id], [Hidden_Indicator], [Posted_Date])
GO
