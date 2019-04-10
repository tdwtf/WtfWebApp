--AH:ScriptId=2d3119d4-a862-49a7-91d9-683c1a77ebf5;2872
ALTER TABLE [Articles]
  ADD CONSTRAINT [CK__Articles__Discourse_Topic_Id_Valid]
    CHECK ([Discourse_Topic_Id] IS NULL OR [Discourse_Topic_Id] > 0)