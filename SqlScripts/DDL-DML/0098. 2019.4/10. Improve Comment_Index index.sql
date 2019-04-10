--AH:ScriptId=1F56A02B-44DA-4CC2-8F8C-F7D8BA0DE61E

DROP INDEX [IX__Comments__Article_Id] ON [Comments]

ALTER TABLE [Comments]
	DROP CONSTRAINT [UQ__Comments__Comment_Index]

CREATE UNIQUE INDEX [IX__Comments__Article_Id__Comment_Index]
	ON [Comments] ([Article_Id], [Comment_Index])
	INCLUDE ([Featured_Indicator], [Hidden_Indicator])
