--AH:ScriptId=D2EB24D7-1D7A-442F-AE3C-DAA39D3CB08B

DROP INDEX [IX__Comments__Discourse_Post_Id] ON [Comments]

ALTER TABLE [Comments]
	DROP COLUMN	[Discourse_Post_Id]
