using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ConvertorForSOI.SQLs;

namespace ConvertorForSOI
{
    class BaseTypesConvertor
    {
        public static void BaseTypeConvert(DataSet dsSource_ru, ref DataSet dsResult, Map map, string fotoPath, bool isMissingPersons, Dictionary<string, string> tableSet)
        {
            var rows = new List<DataRow>();
            // Набор таблиц в зависимости от Розыска или Пропавших без вести
            tableSet = BaseSQL.GetTableSet(isMissingPersons);
            DataTable dtHeader = dsSource_ru.Tables["Header"];
            // Получаем из таблицы dtHeader тип документа и в соответствии с типом выбираем правильную карту конвертации. 
            MapSourceItem.TypeDocument typeDocument = MapSourceItem.GetTypeDocumentByString(dtHeader.Rows[2][0].ToString());

            DataTable dtBody_ru = dsSource_ru.Tables["Body"];

            List<MapConvertItem> listConvert = map.MapSource.Where(g => g.TypeDoc == typeDocument).Select(g => g.MapConvertList).First();

            if (listConvert == null || listConvert.Count < 1)
                throw new ArgumentException("Невозможно конвертировать данный тип документа.");

            // Получаем номера code и номер num, на основе последних номеров в базе данных nake_data.
            string code = Map.GetNextCode();

            System.Diagnostics.Debug.WriteLine("Основной цикл:");

            int counter = 0;

            // Проходим по строкам документа
            foreach (DataRow row in dtBody_ru.Rows)    // row строка из документа (еще не парсенная)
            {
                if (counter % 100 == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Строка: {counter}");
                }
                counter++;

                FullName fullName = null;
                BirthDate birthDate = null;
                string personnote = String.Empty;
                string personnote_en = string.Empty;
                // Пропускаем пустые строки.
                if (!row.ItemArray.Any(g => (!string.IsNullOrWhiteSpace(g.ToString()))))
                    continue;

                // Добавляем новый ряд в таблицу.
                DataRow newRow = dsResult.Tables[tableSet["main"]].NewRow();

                // Добавляем новый ряд в дополнительную таблицу.
                DataRow addedRow = dsResult.Tables[tableSet["misc"]].NewRow();

                try
                {
                    foreach (MapConvertItem mci in listConvert)
                    {
                        // Если нет нужного столбца (индекс маппинга вышел за пределы реальной таблицы из файла)
                        if ((row.ItemArray.Length - 1) < mci.SourceTableIndex)
                        {
                            continue;
                        }

                        switch (mci.SourceTableIndex)
                        {
                            // -1 означает, что данное поле не заполняется.
                            case -1:
                                continue;
                            // -3 Означает записываем в базу null.
                            case -3:
                                newRow[mci.ResultTableIndex] = DBNull.Value;
                                continue;
                        }

                        switch (mci.Category)
                        {
                            // Заполняем поле CODE.
                            case ConvertCategory.Code:
                                {
                                    newRow[mci.ResultTableIndex] = code;
                                    addedRow[0] = code;
                                    break;
                                }
                            // Парсим полное имя на составляющие и в соответстии с номером категории берем значение.
                            case ConvertCategory.FullName:
                                {
                                    FullName tempfullName = new FullName(row[mci.SourceTableIndex].ToString());
                                    newRow[mci.ResultTableIndex] = tempfullName.Names[mci.NumberCategory];

                                    if (fullName == null)
                                    {
                                        fullName = tempfullName;
                                    }
                                    break;
                                }
                            // Парсим дату рождения на составляющие и в соответстии с номером категории берем значение.
                            case ConvertCategory.BirthDate:
                                {
                                    birthDate = new BirthDate(row[mci.SourceTableIndex].ToString());
                                    if (mci.NumberCategory == -1)
                                    {
                                        if (birthDate.Date != DateTime.MinValue)
                                        {
                                            newRow[mci.ResultTableIndex] = birthDate.Date;
                                        }
                                        else
                                        {
                                            newRow[mci.ResultTableIndex] = DBNull.Value;
                                        }
                                    }
                                    else
                                    {
                                        if (string.IsNullOrWhiteSpace(birthDate.BirthDates[mci.NumberCategory].ToString()))
                                        {
                                            newRow[mci.ResultTableIndex] = DBNull.Value;
                                        }
                                        else
                                        {
                                            newRow[mci.ResultTableIndex] = birthDate.BirthDates[mci.NumberCategory];
                                        }
                                    }
                                    break;
                                }
                            // Заполняем поле пол.
                            case ConvertCategory.Sex:
                                {
                                    string sex = Map.GetSex(row[mci.SourceTableIndex].ToString());
                                    newRow[mci.ResultTableIndex] = sex;
                                    break;
                                }
                            // Тип розыска
                            case ConvertCategory.SearchType:
                                {
                                    string source = String.Empty;
                                    if (mci.SourceTableIndex == -2) // -2 Означает, что данные берутся из Header. 
                                    {
                                        source = dtHeader.Rows[1].ToString();
                                    }
                                    else
                                        source = row[mci.SourceTableIndex].ToString();

                                    List<string> searchTypes = SearchType.GetSearchTypes(typeDocument, source);
                                    if (searchTypes.Count == 0)
                                        newRow[mci.ResultTableIndex] = null;
                                    else
                                        newRow[mci.ResultTableIndex] = searchTypes[0];
                                    if (searchTypes.Count > 1)
                                    {
                                        for (int i = 1; i < searchTypes.Count; i++)
                                        {
                                            DataRow searchTypeRow = dsResult.Tables[tableSet["ws"]].NewRow();
                                            searchTypeRow["Code"] = code;
                                            searchTypeRow["SearchType"] = searchTypes[i];
                                            dsResult.Tables[tableSet["ws"]].Rows.Add(searchTypeRow);
                                        }
                                    }
                                    break;
                                }
                            // Фото
                            case ConvertCategory.Foto:
                                {
                                    List<string> fotoFileList = Map.GetFotoFileList(fotoPath, fullName, birthDate);
                                    // Если фото нет, то ячейка-пустая строка
                                    if (fotoFileList.Count == 0)
                                        newRow[mci.ResultTableIndex] = null;
                                    // Иначе записываем путь к фото
                                    else
                                        newRow[mci.ResultTableIndex] = Path.GetFileName(fotoFileList[0]);
                                    // Если больше одного, то заполняем вспомогательную таблицу
                                    if (fotoFileList.Count > 1)
                                    {
                                        for (int i = 1; i < fotoFileList.Count; i++)
                                        {
                                            DataRow tempPhotoRow = dsResult.Tables[tableSet["photo"]].NewRow();
                                            tempPhotoRow["Code"] = code;
                                            tempPhotoRow["FileName"] = Path.GetFileName(fotoFileList[i]);
                                            dsResult.Tables[tableSet["photo"]].Rows.Add(tempPhotoRow);
                                        }
                                    }
                                    break;
                                }
                            // Тип документа
                            case ConvertCategory.PersonDocumentType:
                                {
                                    PersonDocumentType pdt = new PersonDocumentType();
                                    if (pdt.TryParse(row[mci.SourceTableIndex].ToString()))
                                    {
                                        newRow[mci.ResultTableIndex] = pdt.documentNames.Count > 0 ? pdt.documentNames[0] : "";
                                    }
                                    break;
                                }

                            // Наименование документа
                            case ConvertCategory.PersonDocumentData:
                                {
                                    PersonDocumentType pdd = new PersonDocumentType();
                                    if (pdd.TryParse(row[mci.SourceTableIndex].ToString()))
                                    {
                                        newRow[mci.ResultTableIndex] = pdd.documentData.Count > 0 ? pdd.documentData[0] : "";
                                    }
                                    // Если больше одного, то заполняем вспомогательную таблицу
                                    if (pdd.documentData.Count > 1)
                                    {
                                        for (int i = 1; i < pdd.documentData.Count; i++)
                                        {
                                            DataRow pddRow = dsResult.Tables[tableSet["docs"]].NewRow();
                                            pddRow["Code"] = code;
                                            pddRow["Data"] = pdd.documentData[i];
                                            pddRow["DocumentType"] = pdd.documentNames[i];
                                            dsResult.Tables[tableSet["docs"]].Rows.Add(pddRow);
                                        }
                                    }
                                    break;
                                }
                            // TODO: Разобраться в целесообразности данной категории
                            case ConvertCategory.StringDateString:
                                {
                                    string strDate = Map.StringDateString(row[mci.SourceTableIndex].ToString());
                                    if (string.IsNullOrWhiteSpace(strDate))
                                        newRow[mci.ResultTableIndex] = DBNull.Value;
                                    else
                                        newRow[mci.ResultTableIndex] = strDate;
                                    break;
                                }

                            // Для заполнения поля note в таблице person. Сюда заносим всё, что непонятно. ФИО в скобках, рядом с основным ФИО, поле "Известен также как".
                            case ConvertCategory.PersonNote:
                                {
                                    if (mci.SourceTableIndex >= 0)
                                    { 
                                        if (!string.IsNullOrWhiteSpace(row[mci.SourceTableIndex].ToString()))
                                        {
                                            if (mci.SourceTableIndex == 18) // Хардкод для FormHz
                                            {
                                                personnote_en += row[mci.SourceTableIndex].ToString();
                                            }
                                            else
                                            {
                                                personnote += " " + row[mci.SourceTableIndex].ToString();
                                            }
                                        }
                                    }
                                    if (fullName != null && !string.IsNullOrWhiteSpace(fullName.AddedName))
                                    {
                                        personnote = $"{fullName.AddedName}, {personnote}";
                                    }
                                    if (string.IsNullOrWhiteSpace(personnote) && string.IsNullOrWhiteSpace(personnote_en))
                                        addedRow[mci.ResultTableIndex] = DBNull.Value;
                                    else
                                    {
                                        if (mci.SourceTableIndex == 18)
                                        {
                                            addedRow[mci.ResultTableIndex] = personnote_en.Trim();
                                        }
                                        else
                                        {
                                            addedRow[mci.ResultTableIndex] = personnote.Trim();
                                        }
                                    }
                                    break;
                                }

                            // Инициатор розыска
                            case ConvertCategory.WantedInitiator:
                                {
                                    string initiator = string.Empty;
                                    initiator = row[mci.SourceTableIndex].ToString();
                                    string newInitiatorName = WantedInitiator.GetWantedInitiator(initiator);
                                    newRow[mci.ResultTableIndex] = newInitiatorName;
                                    break;
                                }

                            //(ConvertCategory.NoCategory) Просто без изменений переносим содержимое ячейки из таблицы источника в результирующую таблицу.
                            case ConvertCategory.NoCategory:
                                {
                                    if (string.IsNullOrWhiteSpace((string)row[mci.SourceTableIndex]))
                                    {
                                        newRow[mci.ResultTableIndex] = DBNull.Value;
                                    }
                                    else
                                    {
                                        newRow[mci.ResultTableIndex] = row[mci.SourceTableIndex];
                                    }
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.StackTrace + "\n" + ex.Message + "\nТУТ!!!");
                }

                // Проверочная логика (на существующих лиц и на снятие с розыска/поисков)
                // и занесение в таблицы
                Convertor.CheckLogics(newRow, addedRow, isMissingPersons, ref dsResult, typeDocument);

                // логика сode
                code = Map.GetNextCode();
            }
            return;
        }
    }
}
