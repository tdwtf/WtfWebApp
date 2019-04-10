--AH:ScriptId=025f192a-9a9c-442f-bf0c-49d0d9cbf731;2819
ALTER TABLE [Articles]
  ADD [Ad_Id] INT NULL
GO

CREATE TABLE [Ads]
(
    [Ad_Id] INT IDENTITY(1, 1) NOT NULL,
    [Ad_Html] NVARCHAR(MAX) NOT NULL,

    CONSTRAINT [PK__Ads] PRIMARY KEY ([Ad_Id])
)
GO

ALTER TABLE [Articles]
  ADD CONSTRAINT [FK__Articles__Ads] FOREIGN KEY ([Ad_Id])
      REFERENCES [Ads] ([Ad_Id])
GO

