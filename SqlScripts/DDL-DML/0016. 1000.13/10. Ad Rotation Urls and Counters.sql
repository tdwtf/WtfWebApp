--AH:ScriptId=6e4c370b-9984-4a8e-8cab-7eef6ab1d63b;2844
CREATE TABLE [AdImpressions]
(
    [Banner_Name] VARCHAR(100) NOT NULL,
    [Impression_Date] DATE NOT NULL,
    [Impression_Count] INT NOT NULL,

    CONSTRAINT [PK__AdImpressions] PRIMARY KEY ([Banner_Name], [Impression_Date])
)
GO

CREATE TABLE [AdRedirectUrls]
(
    [Ad_Guid] UNIQUEIDENTIFIER NOT NULL,
    [Redirect_Url] NVARCHAR(255) NOT NULL,
    [Click_Count] INT NOT NULL,

    CONSTRAINT [PK__AdRedirectUrls] PRIMARY KEY ([Ad_Guid], [Redirect_Url])
)
GO