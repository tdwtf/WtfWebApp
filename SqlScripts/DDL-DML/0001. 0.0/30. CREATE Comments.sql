--AH:ScriptId=785d6943-6fcc-47fc-823e-ce8a77f71791;2798
CREATE TABLE [Comments]
(
    [Comment_Id] INT IDENTITY(1, 1) NOT NULL,
    [Article_Id] INT NOT NULL,
    [Body_Html] NVARCHAR(MAX) NOT NULL,
    [User_Name] NVARCHAR(255) NOT NULL,
    [Posted_Date] DATETIME NOT NULL,
    [Discourse_Post_Id] INT NOT NULL,

    CONSTRAINT [PK__Comments] PRIMARY KEY ([Comment_Id]),
    CONSTRAINT [FK__Comments__Articles] FOREIGN KEY ([Article_Id])
        REFERENCES [Articles] ([Article_Id])
)
GO

CREATE NONCLUSTERED INDEX [IX__Comments__Discourse_Post_Id] ON [Comments] ([Discourse_Post_Id])
GO