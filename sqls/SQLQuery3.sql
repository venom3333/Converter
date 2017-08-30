UPDATE dbo.NAK_WANTED
SET A_ACCUSATION = CAST(A_ACCUSATION AS nvarchar(max)) + ' ' + CAST('121212' as nvarchar(max))
WHERE A_ID = 1;
