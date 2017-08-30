using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ConvertorForSOI.SQLs;

namespace ConvertorForSOI
{
    class DocCardsConvertor
    {
        // Парсер для DocКарточек
        public static void DocCardsTypeConvert(DataSet dsSourceRus, DataSet dsSourceEng, ref DataSet dsResult, bool isMissingPersons, Dictionary<string, string> tableSet)
        {
            var rows = new List<DataRow>();
            // Набор таблиц в зависимости от Розыска или Пропавших без вести
            tableSet = BaseSQL.GetTableSet(isMissingPersons);
            DataTable dtHeader = dsSourceRus.Tables["Header"];
            // Получаем из таблицы dtHeader тип документа и в соответствии с типом выбираем правильную карту конвертации. 
            MapSourceItem.TypeDocument typeDocument = MapSourceItem.GetTypeDocumentByString(dtHeader.Rows[2][0].ToString());

            DataTable dtBodyRus = dsSourceRus.Tables["Body"];
            DataTable dtBodyEng = dsSourceEng.Tables["Body"];

            // Получаем номера code и номер num, на основе последних номеров в базе данных nake_data.
            FullName fullNameRus = null;
            FullName fullNameEng = null;
            BirthDate birthDate = null;

            // Добавляем новый ряд в таблицу и доп таблицу.
            DataRow newRow = dsResult.Tables[tableSet["main"]].NewRow();
            DataRow addedRow = dsResult.Tables[tableSet["misc"]].NewRow();

            // логика сode
            string code = Map.GetNextCode();

            // Проходим по строкам документа / документов
            DataRowCollection rowsRus = dtBodyRus.Rows;
            DataRowCollection rowsEng = new DataTable().Rows;
            // Если в английской версии заполнен DataTable
            if (dtBodyEng != null)
            {
                rowsEng = dtBodyEng.Rows;
            }
            for (int rowIdx = 0; rowIdx < rowsRus.Count; rowIdx++)
            {
                //foreach (DataRow rowsRus[rowIdx] in dtBodyRus.Rows)    // rowsRus[rowIdx] строка из документа (еще не парсенная)
                //{
                // Пропускаем пустые строки.
                if (!rowsRus[rowIdx].ItemArray.Any(g => (!string.IsNullOrWhiteSpace(g.ToString()))))
                    continue;

                // если начинается новое лицо
                if (rowsRus[rowIdx].Field<string>(0).ToLower().Contains("поле") || rowsRus[rowIdx].Field<string>(1).ToLower().Contains("заполнен"))
                {
                    // Это поле нам не нужно (и не везде присутствует), так что пока пропускаем
                    continue;
                }
                // ФИО
                else if (rowsRus[rowIdx].Field<string>(0).ToLower().Contains("установоч"))
                {
                    // заполняем поле code 
                    newRow[1] = code;
                    addedRow[0] = code;

                    fullNameRus = new FullName(rowsRus[rowIdx].Field<string>(1));
                    // Фамилия
                    newRow[2] = fullNameRus.Names[0];
                    // Имя
                    newRow[3] = fullNameRus.Names[1];
                    // Отчество
                    newRow[4] = fullNameRus.Names[2];

                    if (dtBodyEng != null)
                    {
                        fullNameEng = new FullName(rowsEng[rowIdx].Field<string>(1));
                        // Фамилия
                        newRow[8] = fullNameEng.Names[0];
                        // Имя
                        newRow[9] = fullNameEng.Names[1];
                        // Отчество
                        newRow[10] = fullNameEng.Names[2];
                    }

                }

                // Дата рождения
                else if (rowsRus[rowIdx].Field<string>(0).ToLower().Contains("дата рожден"))
                {
                    birthDate = new BirthDate(rowsRus[rowIdx].Field<string>(1));
                    // День
                    short result;
                    if (short.TryParse(birthDate.BirthDates[0], out result))
                    {
                        newRow[5] = result;
                    }
                    // Месяц
                    if (!String.IsNullOrWhiteSpace(birthDate.BirthDates[1].ToString()))
                    {
                        newRow[6] = birthDate.BirthDates[1].ToString();
                    }
                    // Год
                    if (short.TryParse(birthDate.BirthDates[2], out result))
                    {
                        newRow[7] = result;
                    }
                }

                // Место рождения
                else if (rowsRus[rowIdx].Field<string>(0).ToLower().Contains("место рожден"))
                {
                    newRow[12] = rowsRus[rowIdx].Field<string>(1);

                    if (dtBodyEng != null)
                    {
                        newRow[13] = rowsEng[rowIdx].Field<string>(1);
                    }
                }

                // Национальность
                else if (rowsRus[rowIdx].Field<string>(0).ToLower().Contains("национальн"))
                {
                    //newRow[14] = rowsRus[rowIdx].Field<string>(1); // не понятно как программа заносит
                    //newRow[28] = rowsRus[rowIdx].Field<string>(1); // --//--//--
                    // запишем в доп.таблицу чтобы не терять информацию
                    if (!String.IsNullOrWhiteSpace(rowsRus[rowIdx].Field<string>(1)))
                    {
                        addedRow[1] += "Национальность: " + rowsRus[rowIdx].Field<string>(1) + "\n";
                    }

                    if (dtBodyEng != null)
                    {
                        if (!String.IsNullOrWhiteSpace(rowsEng[rowIdx].Field<string>(1)))
                        {
                            addedRow[2] += "Nationality: " + rowsEng[rowIdx].Field<string>(1) + "\n";

                        }
                    }
                }

                // Адрес регистрации (добавляем в misc таблицу)
                else if (rowsRus[rowIdx].Field<string>(0).ToLower().Contains("регистрац"))
                {
                    if (!String.IsNullOrWhiteSpace(rowsRus[rowIdx].Field<string>(1)))
                    {
                        addedRow[1] += "Адрес регистрации: " + rowsRus[rowIdx].Field<string>(1) + "\n";

                    }

                    if (dtBodyEng != null)
                    {
                        if (!String.IsNullOrWhiteSpace(rowsEng[rowIdx].Field<string>(1)))
                        {
                            addedRow[2] += "Address of registration: " + rowsEng[rowIdx].Field<string>(1) + "\n";

                        }
                    }
                }

                // Фотография
                // TODO: Придумать что делать с этим полем
                else if (rowsRus[rowIdx].Field<string>(0).ToLower().Contains("фото"))
                {
                    if (rowsRus[rowIdx].Field<string>(1) == String.Empty)
                    {
                        continue;
                    }
                    // Разбиваем названия файлов фоток на лист
                    char[] separator = {';'};
                    List<string> fotoFileList = rowsRus[rowIdx].Field<string>(1).Split(separator, StringSplitOptions.RemoveEmptyEntries).ToList();

                    if (fotoFileList.Count == 0)
                        newRow[29] = null;
                    // Иначе записываем путь к фото
                    else
                        newRow[29] = Path.GetFileName(fotoFileList[0]);
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
                }

                // Документ, удостоверяющий личность
                else if (rowsRus[rowIdx].Field<string>(0).ToLower().Contains("документ"))
                {
                    PersonDocumentType personDoc = new PersonDocumentType();
                    if (!String.IsNullOrWhiteSpace(rowsRus[rowIdx].Field<string>(1)) && personDoc.TryParse(rowsRus[rowIdx].Field<string>(1)))
                    {
                        // Наименование документа
                        newRow[25] = personDoc.documentNames.Count > 0 ? personDoc.documentNames[0] : "";

                        // Номер документа
                        newRow[26] = personDoc.documentData.Count > 0 ? personDoc.documentData[0].Trim() : "";
                    }
                    // Если больше одного, то заполняем вспомогательную таблицу
                    if (personDoc.documentNames.Count > 1)
                    {
                        for (int i = 1; i < personDoc.documentNames.Count; i++)
                        {
                            DataRow pddRow = dsResult.Tables[tableSet["docs"]].NewRow();
                            pddRow["Code"] = code;
                            pddRow["Data"] = personDoc.documentData[i];
                            pddRow["DocumentType"] = personDoc.documentNames[i];
                            dsResult.Tables[tableSet["docs"]].Rows.Add(pddRow);
                        }
                    }

                    // Если есть паспорт гражданина РФ, то внесем гражданство
                    foreach (var docName in personDoc.documentNames)
                    {
                        if (docName.Contains("РФ"))
                        {
                            newRow[14] = "Россия (Российская Федерация, РФ)";
                        }
                    }

                    // Запишем еще в доп информацию из англ версии, т.к. в строке может быть инфо о том кто выдал документ и др.
                    //if (!String.IsNullOrWhiteSpace(rowsRus[rowIdx].Field<string>(1)))
                    //{
                    //    addedRow[1] += "Дополнительная информация о документах: " + rowsRus[rowIdx].Field<string>(1) + "\n";
                    //}

                    if (dtBodyEng != null)
                    {
                        if (!String.IsNullOrWhiteSpace(rowsEng[rowIdx].Field<string>(1)))
                        {
                            addedRow[2] += "Documents additional info: " + rowsEng[rowIdx].Field<string>(1) + "\n";
                        }
                    }

                }

                // Кличка, позывной (добавляем в misc таблицу)
                else if (rowsRus[rowIdx].Field<string>(0).ToLower().Contains("кличк") || rowsRus[rowIdx].Field<string>(0).ToLower().Contains("позывн"))
                {
                    if (!String.IsNullOrWhiteSpace(rowsRus[rowIdx].Field<string>(1)))
                    {
                        addedRow[1] += "Кличка, позывной: " + rowsRus[rowIdx].Field<string>(1) + "\n";
                    }

                    if (dtBodyEng != null)
                    {
                        if (!String.IsNullOrWhiteSpace(rowsEng[rowIdx].Field<string>(1)))
                        {
                            addedRow[2] += "Alias: " + rowsEng[rowIdx].Field<string>(1) + "\n";

                        }
                    }
                }

                // Описание внешности, приметы
                else if (rowsRus[rowIdx].Field<string>(0).ToLower().Contains("примет"))
                {
                    newRow[15] = rowsRus[rowIdx].Field<string>(1);

                    if (dtBodyEng != null)
                    {
                        newRow[16] = rowsEng[rowIdx].Field<string>(1);
                    }
                }

                // Вид розыска + Инициатор розыска
                else if (rowsRus[rowIdx].Field<string>(0).ToLower().Contains("вид") && rowsRus[rowIdx].Field<string>(0).ToLower().Contains("розыск"))
                {
                    // Вид розыска
                    string source = String.Empty;
                    source = rowsRus[rowIdx].Field<string>(1);
                    // Если нет розыска, то и не пишем ничего
                    if (source == "нет" || String.IsNullOrWhiteSpace(source))
                    {
                        newRow[21] = "не объявлялся";
                        newRow[17] = null;
                        continue;
                    }
                    List<string> searchTypes = SearchType.GetSearchTypes(typeDocument, source);
                    if (searchTypes.Count == 0)
                        newRow[21] = null;
                    else
                        newRow[21] = searchTypes[0];
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

                    // Инициатор розыска (для этих документов всегда будет МВД России)
                    string initiator = string.Empty;
                    string newInitiatorName = WantedInitiator.GetWantedInitiator(initiator);
                    newRow[17] = newInitiatorName;
                }

                // Дата объявления в розыск
                else if (rowsRus[rowIdx].Field<string>(0).ToLower().Contains("дата") && rowsRus[rowIdx].Field<string>(0).ToLower().Contains("розыск"))
                {
                    string strDate = Map.StringDateString(rowsRus[rowIdx].Field<string>(1));

                    if (string.IsNullOrWhiteSpace(strDate))
                        newRow[22] = DBNull.Value;
                    else
                        newRow[22] = strDate;
                }


                // Реквизиты уголовного дела
                else if (rowsRus[rowIdx].Field<string>(0).ToLower().Contains("реквизиты"))
                {
                    if (!String.IsNullOrWhiteSpace(rowsRus[rowIdx].Field<string>(1)))
                    {
                        addedRow[1] += "Реквизиты уголовного дела: " + rowsRus[rowIdx].Field<string>(1) + "\n";
                    }

                    if (dtBodyEng != null)
                    {
                        if (!String.IsNullOrWhiteSpace(rowsEng[rowIdx].Field<string>(1)))
                        {
                            addedRow[2] += "Criminal Case details: " + rowsEng[rowIdx].Field<string>(1) + "\n";

                        }
                    }
                }

                // Фактические данные о причастности к противоправной деятельности (в основания розыска)
                // это последнее поле, значит добавляем туда логику заполнения таблиц
                else if (rowsRus[rowIdx].Field<string>(0).ToLower().Contains("противоправн"))
                {
                    newRow[18] = rowsRus[rowIdx].Field<string>(1);

                    if (dtBodyEng != null)
                    {
                        newRow[19] = rowsEng[rowIdx].Field<string>(1);
                    }
                    // Логика добавления данных
                    // Проверочная логика (на существующих лиц и на снятие с розыска/поисков)
                    // и занесение в таблицы
                    Convertor.CheckLogics(newRow, addedRow, isMissingPersons, ref dsResult);

                    // Обновляем наши ряды в таблице
                    newRow = dsResult.Tables[tableSet["main"]].NewRow();
                    addedRow = dsResult.Tables[tableSet["misc"]].NewRow();

                    // логика сode
                    code = Map.GetNextCode();
                }
            }
            return;
        }

    }
}
