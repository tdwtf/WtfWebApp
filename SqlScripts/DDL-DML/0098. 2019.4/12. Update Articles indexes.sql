--AH:ScriptId=2BD3E5FC-DD0D-46AD-89A2-BE399E5BE0D5

DROP INDEX [IX__Articles__NextPrevious] ON [Articles]

DROP INDEX [IX__Articles__PublishedDate] ON [Articles]

CREATE INDEX [IX__Articles__Published_Date]
	ON [Articles] ([Published_Date])
	INCLUDE ([Series_Slug], [Author_Slug])
