--AH:ScriptId=ec196ac6-fa76-4b9c-92e7-5c909914537e;3006
ALTER TABLE [Comments]
  ADD User_IP VARCHAR(45) NULL,
      User_Token VARCHAR(MAX) NULL,
      Parent_Comment_Id INT NULL
      CONSTRAINT [FK__Comments__Comments]
      FOREIGN KEY REFERENCES [Comments] ([Comment_Id])
