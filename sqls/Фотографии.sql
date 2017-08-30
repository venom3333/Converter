SELECT 
ISNULL(lastName, '') + ISNULL(' '+firstName, '')  + ISNULL(' ' + secondName, '') AS defaultComment,
ISNULL(lastName_en, '') + ISNULL(' '+firstName_en, '') + ISNULL(' ' + secondName1_en, '') AS defaultComment_en,
'/NAK_DB/nakPerson/' + photoList  + '@cmsFile.images' as [file],
'/NAK_DB/nakPerson/' + photoList  + '@cmsFile.images' as [file_en],
photoList as [fileName]
FROM NAK_TEMP_WANTED
WHERE photoList IS NOT NULL AND NOT EXISTS (SELECT 1 A_ID FROM NAK_PHOTO WHERE A_FILE = '/NAK_DB/nakPerson/' + photoList  + '@cmsFile.images' )
UNION
SELECT     ISNULL(dbo.NAK_TEMP_WANTED.lastName, '') + ISNULL(' ' + dbo.NAK_TEMP_WANTED.firstName, '') 
                      + ISNULL(' ' + dbo.NAK_TEMP_WANTED.secondName, '') AS defaultComment, ISNULL(dbo.NAK_TEMP_WANTED.lastName_en, '') 
                      + ISNULL(' ' + dbo.NAK_TEMP_WANTED.firstName_en, '') + ISNULL(' ' + dbo.NAK_TEMP_WANTED.secondName1_en, '') AS defaultComment_en, 
                      '/NAK_DB/nakPerson/' + dbo.NAK_TEMP_WANTED.photoList + '@cmsFile.images' AS [file], 
                      '/NAK_DB/nakPerson/' + dbo.NAK_TEMP_WANTED.photoList + '@cmsFile.images' AS file_en, dbo.NAK_TEMP_PHOTO.FILENAME
FROM         dbo.NAK_TEMP_WANTED RIGHT OUTER JOIN
                      dbo.NAK_TEMP_PHOTO ON dbo.NAK_TEMP_WANTED.Code = dbo.NAK_TEMP_PHOTO.CODE
WHERE     (dbo.NAK_TEMP_WANTED.photoList IS NOT NULL) AND (NOT EXISTS
                          (SELECT     1 AS A_ID
                            FROM          dbo.NAK_PHOTO
                            WHERE      (A_FILE = '/NAK_DB/nakPerson/' + dbo.NAK_TEMP_WANTED.photoList + '@cmsFile.images')))