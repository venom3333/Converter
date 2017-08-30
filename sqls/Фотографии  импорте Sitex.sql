SELECT 
ISNULL(lastName, '') + ISNULL(' '+firstName, '')  + ISNULL(' ' + secondName, '') AS defaultComment,
ISNULL(lastName_en, '') + ISNULL(' '+firstName_en, '') + ISNULL(' ' + secondName1_en, '') AS defaultComment_en,
'/NAK_DB/nakPerson/' + photoList  + '@cmsFile.images' as [file],
'/NAK_DB/nakPerson/' + photoList  + '@cmsFile.images' as [file_en],
photoList as [fileName]
FROM NAK_TEMP_WANTED
WHERE photoList IS NOT NULL AND NOT EXISTS (SELECT 1 A_ID FROM NAK_PHOTO WHERE A_FILE = '/NAK_DB/nakPerson/' + photoList  + '@cmsFile.images' )
UNION
SELECT     ISNULL(NAK_TEMP_WANTED_1.lastName, '') + ISNULL(' ' + NAK_TEMP_WANTED_1.firstName, '') + ISNULL(' ' + NAK_TEMP_WANTED_1.secondName,
                       '') AS defaultComment, ISNULL(NAK_TEMP_WANTED_1.lastName_en, '') + ISNULL(' ' + NAK_TEMP_WANTED_1.firstName_en, '') 
                      + ISNULL(' ' + NAK_TEMP_WANTED_1.secondName1_en, '') AS defaultComment_en, 
                      '/NAK_DB/nakPerson/' + dbo.NAK_TEMP_PHOTO.FILENAME + '@cmsFile.images' AS [file], 
                      '/NAK_DB/nakPerson/' + dbo.NAK_TEMP_PHOTO.FILENAME + '@cmsFile.images' AS file_en, dbo.NAK_TEMP_PHOTO.FILENAME
FROM         dbo.NAK_TEMP_WANTED AS NAK_TEMP_WANTED_1 RIGHT OUTER JOIN
                      dbo.NAK_TEMP_PHOTO ON NAK_TEMP_WANTED_1.Code = dbo.NAK_TEMP_PHOTO.CODE
WHERE     (NAK_TEMP_WANTED_1.photoList IS NOT NULL) AND (NOT EXISTS
                          (SELECT     1 AS A_ID
                            FROM          dbo.NAK_PHOTO AS NAK_PHOTO_1
                            WHERE      (A_FILE = '/NAK_DB/nakPerson/' + NAK_TEMP_WANTED_1.photoList + '@cmsFile.images')))