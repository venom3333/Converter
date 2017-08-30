SELECT 
 p.A_ID AS person /* Лицо */
, ISNULL(CAST(t.A_ID AS VARCHAR) + '@nakPersonDocumentType', w.documentType) AS documentType /* Название документа */
, w.data /* Данные */
, w.data as data_en /* Data */
FROM NAK_TEMP_WANTED w
INNER JOIN NAK_PERSON p ON w.Code = p.A_CODE AND w.DATA IS NOT NULL
LEFT JOIN NAK_PERSONDOCUMENTTYPE t ON w.documentType = t.A_NAME
WHERE NOT EXISTS 
(SELECT NAK_PERSONDOCUMENT.A_ID 
FROM NAK_PERSONDOCUMENT WHERE NAK_PERSONDOCUMENT.A_PERSON = p.A_ID
 AND NAK_PERSONDOCUMENT.A_DATA = w.data)
 UNION
 SELECT     p.A_ID AS person, ISNULL(CAST(t.A_ID AS VARCHAR) + '@nakPersonDocumentType', w.DOCUMENTTYPE) AS documentType, w.DATA, 
                      w.DATA AS data_en
FROM         dbo.NAK_PERSONDOCUMENTTYPE AS t INNER JOIN
                      dbo.NAK_TEMP_PERSONDOCUMENT AS w INNER JOIN
                      dbo.NAK_PERSON AS p ON w.CODE = p.A_CODE AND w.DATA IS NOT NULL ON t.A_NAME = w.DOCUMENTTYPE
WHERE     (NOT EXISTS
                          (SELECT     A_ID
                            FROM          dbo.NAK_PERSONDOCUMENT
                            WHERE      (A_PERSON = p.A_ID) AND (A_DATA = w.DATA)))