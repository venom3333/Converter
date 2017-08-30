begin transaction;
IF EXISTS(SELECT *  FROM  nak_wantedtypesearch  WHERE A_fromid=50001 )
begin

print 1

end

else

INSERT INTO [nak_data].[dbo].[NAK_WANTEDTYPESEARCH]
SELECT r.A_ID AS person, t.A_ID
FROM NAK_TEMP_WANTEDTYPESEARCH AS w INNER JOIN
NAK_PERSON AS p ON p.A_CODE = w.Code INNER JOIN
NAK_WANTED AS r ON r.A_PERSON = p.A_ID	INNER JOIN
NAK_SEARCHTYPE AS t ON t.A_NAME = w.searchType
where r.A_ID in (
select max(r1.a_id)  from Nak_person as p1 inner join 
NAK_TEMP_WANTEDTYPESEARCH AS w1 on p1.a_code = w1.code
INNER JOIN
NAK_WANTED AS r1 ON r1.A_PERSON = p1.A_ID
group by p1.a_code
);

commit transaction;