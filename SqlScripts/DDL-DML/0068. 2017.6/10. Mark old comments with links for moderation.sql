--AH:ScriptId=c9381049-b915-4a11-8641-0df8878314e6;3203
UPDATE [Comments]
   SET [Hidden_Indicator] = 'Y'
 WHERE [Posted_Date] < '2017-09-20'
   AND ([Body_Html] LIKE '%http:%'
    OR  [Body_Html] LIKE '%https:%'
    OR  [Body_Html] LIKE '%ftp:%'
	OR  [Body_Html] LIKE '%mailto:%'
	OR  [Body_Html] LIKE '%@%')
