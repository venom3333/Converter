SELECT 
ISNULL(lastName, '') + ISNULL(' '+firstName, '')  + ISNULL(' ' + secondName, '') AS defaultComment,
ISNULL(lastName_en, '') + ISNULL(' '+firstName_en, '') + ISNULL(' ' + secondName1_en, '') AS defaultComment_en,
'/NAK_DB/nakPerson/' + photoList  + '@cmsFile.images' as [file],
'/NAK_DB/nakPerson/' + photoList  + '@cmsFile.images' as [file_en],
photoList as [fileName]
FROM NAK_TEMP_WANTED
WHERE photoList IS NOT NULL AND NOT EXISTS (SELECT 1 A_ID FROM NAK_PHOTO WHERE A_FILE = '/NAK_DB/nakPerson/' + photoList  + '@cmsFile.images' )
UNION
SELECT
