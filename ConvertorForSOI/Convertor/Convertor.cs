using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Data;
using System.Windows;
using ConvertorForSOI.SQLs;

namespace ConvertorForSOI
{
    public static class Convertor
    {
        // Счетчик количества снятых с розыска
        private static int endWantedCounter = 0;

        // Набор используемых таблиц
        private static Dictionary<string, string> tableSet;

        /// <summary>
        /// Метод конвертирует таблицу DataTable взятую из источника данных и таблицу с Header из DataSet, в таблицу пригодную для заполнения результирующего файла.
        /// </summary>
        /// <param name="map">Карта соответствий полей перевода данных из источника данных в результирующую таблицу.</param>
        /// <param name="dsSourceRus">Dataset после конвертации из файла</param>
        /// <param name="dsResult">Загруженные через адаптер таблицы в которые мы вносим данные</param>
        /// <param name="fotoPath">Путь к фотографиям</param>
        /// <param name="isMissingPersons">Флаг "Пропавшие без вести"</param>
        /// <returns>Результирующая таблица DataTable. В случае ошибки конвертации возвращает Null.</returns>
        public static bool ConvertToCorrectData(DataSet dsSourceRus, DataSet dsSourceEng, ref DataSet dsResult, Map map, string fotoPath, bool isMissingPersons)
        {
            try
            {
                // Набор таблиц в зависимости от Розыска или Пропавших без вести
                tableSet = BaseSQL.GetTableSet(isMissingPersons);
                DataTable dtHeader = dsSourceRus.Tables["Header"];
                // Получаем из таблицы dtHeader тип документа и в соответствии с типом выбираем правильную карту конвертации. 
                MapSourceItem.TypeDocument typeDocument = MapSourceItem.GetTypeDocumentByString(dtHeader.Rows[2][0].ToString());

                // Логика разделения по типам документа
                // Если вордовские документы в виде карточек
                if (typeDocument.ToString() == "DocCards")
                {
                    DocCardsConvertor.DocCardsTypeConvert(dsSourceRus, dsSourceEng, ref dsResult, isMissingPersons, tableSet);
                }

                // Если базовые типы документов (первоначальные таблицы)
                else if (typeDocument.ToString() == "Form2" ||
                         typeDocument.ToString() == "Form3" ||
                         typeDocument.ToString() == "Form4" ||
                         typeDocument.ToString() == "Xls")
                {
                    BaseTypesConvertor.BaseTypeConvert(dsSourceRus, ref dsResult, map, fotoPath, isMissingPersons, tableSet);
                }
                // Отсортируем листы с недостающими данными (типы и инициаторы розыска в словаре программы и в БД)
                ParseHelper.SortMissedLists();

                // Если снимали с розыска, выведем инфу сколько человек
                if (endWantedCounter > 0)
                {
                    MessageBox.Show("Снято с розыска: " + endWantedCounter + " чел.");
                    // сбросим счетчик снимаемых с розыска
                    endWantedCounter = 0;
                }

                return true;

            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace + "\n" + e.Message);
                return false;
            }
            return true;
        }

        public static void CheckLogics(DataRow newRow, DataRow addedRow, bool isMissingPersons, ref DataSet dsResult)
        {
            // Проверяем newRow.ItemArray[2,3,4,5,6,7] на наличие в NAK_AJUSTINGDATA
            // если нет, выполняем код на вставку в NAK_TEMP_WANTED...
            /// Эти костыли только для проверки на повтор лиц, т.к. если не известна дата рождения, то шанс встретить полного тёзку велик

            string lastName;
            string firstName;
            string secondName1;
            int dayBirthDate;
            int monthBirthDate;
            int yearBirthDate;
            try
            {
                lastName = newRow.ItemArray[2].ToString().ToUpper();
                firstName = newRow.ItemArray[3].ToString().ToUpper();
                secondName1 = newRow.ItemArray[4].ToString().ToUpper();
                dayBirthDate = int.TryParse(newRow.ItemArray[5].ToString(), out dayBirthDate) ? dayBirthDate : 0;
                monthBirthDate = ParseHelper.MonthToInt(newRow.ItemArray[6].ToString().ToUpper());
                yearBirthDate = int.TryParse(newRow.ItemArray[7].ToString(), out yearBirthDate) ? yearBirthDate : 0;
            }
            catch (Exception ex)
            {
                lastName = " ";
                firstName = " ";
                secondName1 = " ";
                dayBirthDate = 0;
                monthBirthDate = 0;
                yearBirthDate = 0;
            }

            // В зависимости от пропавших или в розыске проверяем на наличие человека в базе
            switch (isMissingPersons)
            {
                // если разыскиваемые преступники
                case false:
                    {
                        // Если персона существует, начинаем колдовать с добавлением доп.вантедов и/или снятием с розыска
                        if (WantedSQL.PersonExists(lastName, firstName, secondName1, monthBirthDate, yearBirthDate, dayBirthDate))
                        {
                            // Находим A_ID в NAK_WANTED
                            int wantedID = WantedSQL.GetWantedID(lastName, firstName, secondName1, monthBirthDate, yearBirthDate, dayBirthDate);

                            // Находим A_ID в NAK_SEARCHTYPE
                            string searchType = newRow.Field<string>(21) == null ? "" : newRow.Field<string>(21).ToUpper();
                            string accusation = newRow.Field<string>(18) == null ? "" : newRow.Field<string>(18);
                            int searchTypeID = WantedSQL.GetSearchTypeID(searchType);

                            // Добавляем в NAK_SEARCHTYPE связку NAK_WANTED.A_ID <--> NAK_SEARCHTYPE.A_ID (Если такой связки еще не существует!)
                            WantedSQL.AddExtraWantedTypeSearch(wantedID, searchTypeID, accusation);

                            // Проверка на снятие с розыска и добавление записи об этом существующему Лицу (NAK_WANTED.A_NOTE - основания снятия)
                            // Если присутствует информация о снятии с розыска
                            if (!String.IsNullOrWhiteSpace(newRow.Field<string>(23)))
                            {
                                // Получаем всю необходимую информацию о снятии с розыска для добавления
                                BirthDate endDate = new BirthDate(newRow.Field<string>(23));
                                string endNote = newRow.Field<string>(24);
                                // добавляем инфо о снятии с розыска  в зависимости от того пропавший это без вести или преступник
                                if (WantedSQL.AddSearchEndInfo(wantedID, endDate, endNote))
                                {
                                    endWantedCounter++;
                                }
                            }
                            return;
                        }
                        else
                        {
                            // если это информация о снятии с розыска, то не должно добавляться лицо для импорта
                            if (!String.IsNullOrWhiteSpace(newRow.Field<string>(23)))
                            {
                                return;
                            }
                            // иначе смело добавляем лицо для импорта
                            else
                            {
                                dsResult.Tables[tableSet["main"]].Rows.Add(newRow);    // Заполненная строка для NAK_TEMP_WANTED откуда ведется импорт средствами sitex
                                dsResult.Tables[tableSet["misc"]].Rows.Add(addedRow);    // Строка для добавления всего что непонятно. (ФИО в скобках, рядом с основным ФИО, поле "Известен также как".)
                            }

                        }
                        break;
                    }
                // TODO: зачатки для MISSING
                // если пропавшие безвести
                case true:
                    // Если персона существует, начинаем колдовать со снятием с поисков
                    if (MissingSQL.PersonExists(lastName, firstName, secondName1, monthBirthDate, yearBirthDate, dayBirthDate))
                    {
                        // Находим A_ID в NAK_MISSING
                        int missingSearchID = MissingSQL.GetMissingSearchID(lastName, firstName, secondName1, monthBirthDate, yearBirthDate, dayBirthDate);

                        // Проверка на снятие с розыска и добавление записи об этом существующему Лицу (NAK_WANTED.A_NOTE - основания снятия)
                        // Если присутствует информация о снятии с розыска
                        if (!String.IsNullOrWhiteSpace(newRow.Field<string>(23)))
                        {
                            // Получаем всю необходимую информацию о снятии с розыска для добавления
                            BirthDate endDate = new BirthDate(newRow.Field<string>(23));
                            string endNote = newRow.Field<string>(24);
                            // добавляем инфо о снятии с розыска  в зависимости от того пропавший это без вести или преступник
                            if (MissingSQL.AddSearchEndInfo(missingSearchID, endDate, endNote))
                            {
                                endWantedCounter++;
                            }
                        }
                        return;
                    }
                    else
                    {
                        // если это информация о снятии с розыска, то не должно добавляться лицо для импорта
                        if (!String.IsNullOrWhiteSpace(newRow.Field<string>(23)))
                        {
                            return;
                        }
                        // иначе смело добавляем лицо для импорта
                        else
                        {
                            dsResult.Tables[tableSet["main"]].Rows.Add(newRow);    // Заполненная строка для NAK_TEMP_WANTED откуда ведется импорт средствами sitex
                            dsResult.Tables[tableSet["misc"]].Rows.Add(addedRow);    // Строка для добавления всего что непонятно. (ФИО в скобках, рядом с основным ФИО, поле "Известен также как".)
                        }

                    }
                    break;
            }
        }

        /// <summary>
        /// Метод который конвертирует исходные данные, в пригодный для СОИ формат.
        /// </summary>
        /// <param name="sourceFileRus">Источник данных, файл либо word, либо excel.</param>
        /// <param name="fotoPath">Папка где лежат фотографии соответствующие данному файлу.</param>
        /// <param name="isMissingPersons">todo: describe isMissingPersons parameter on ConvertToXLS</param>
        /// <returns>True если данные успешно сконвертированы и отправлены в базу. False в случае неудачи.</returns>
        public static bool ConvertToXLS(string sourceFileRus, string sourceFileEng, string fotoPath, bool isMissingPersons)
        {
            sourceFileRus = sourceFileRus.ToLower();
            sourceFileEng = sourceFileEng.ToLower();

            if (!File.Exists(sourceFileRus))
            {
                MessageBox.Show("Файла источника данных по пути " + sourceFileRus + " не существует. Выберете корректный файл.");
                return false;
            }
            //else if (!File.Exists(sourceFileEng))
            //{
            //    MessageBox.Show("Файла источника данных по пути " + sourceFileEng + " не существует. Выберете корректный файл.");
            //    return false;
            //}

            // Проверяем расширение файла с источниом данных.
            string extentionRus = Path.GetExtension(sourceFileRus).ToLower(); // Расширение файла источника данных рус - контента.
            string extentionEng = Path.GetExtension(sourceFileEng).ToLower(); // Расширение файла источника данных eng - контента.

            // Если файл с англ-контентом выбран, но отличается расширением
            if (sourceFileEng != String.Empty && extentionRus != extentionEng)
            {
                MessageBox.Show("Файлы Русской и Английской версии ДОЛЖНЫ быть одного типа!");
                return false;
            }
            else if (string.IsNullOrWhiteSpace(extentionRus))
            {
                MessageBox.Show("Файла источника данных по пути " + sourceFileRus + " имеет некорректное расширение. Выберете корректный файл.");
                return false;
            }
            else if (!string.IsNullOrWhiteSpace(Config.FilesAllowExtensions))
            {
                string[] allowExtentsions = Config.FilesAllowExtensions.Split('|');
                if (!allowExtentsions.Contains(extentionRus.Trim('.')))
                {
                    MessageBox.Show("Файла источника данных по пути " + sourceFileRus + " имеет некорректное расширение. Выберете корректный файл.");
                    return false;
                }
            }

            try
            {
                Map map = new Map();
                List<MapSourceItem> msi = map.MapSource.Where(g => g.Extention == extentionRus).ToList();
                if (msi == null || msi.Count < 1)
                {
                    throw new ArgumentException("Метода для обработки \"" + extentionRus + "\" файлов, пока нет.");
                }

                DataSet dsRus = new DataSet();
                DataSet dsEng = new DataSet();
                if (extentionRus == ".xls")
                {
                    dsRus = ExcelConvertor.XlsToTable(sourceFileRus, map);
                }
                else if (extentionRus == ".doc")
                {
                    dsRus = WordConvertor.WordToTable(sourceFileRus, map);
                    // Если sourceFileEng ссылается на файл
                    if (sourceFileEng != String.Empty)
                    {
                        dsEng = WordConvertor.WordToTable(sourceFileEng, map);
                        // Если количество строк в таблицах обоих файлов после обработки конвертером не сошлось
                        if (dsRus.Tables[1].Rows.Count != dsEng.Tables[1].Rows.Count)
                        {
                            throw new Exception("Количество строк в таблицах рус и eng после конвертирования не равно!");
                        }
                    }
                }
                else if (extentionRus == ".docx")
                {
                    throw new ArgumentException("Метода для обработки .docx файлов, пока нет.");
                }
                else if (extentionRus == ".xlsx")
                {
                    throw new ArgumentException("Метода для обработки .xlsx файлов, пока нет.");
                }

                if (dsRus == null)
                {
                    throw new ArgumentException("Ошибка при конвертации исходного " + sourceFileRus + " файла.");
                }

                if (!BaseSQL.FillNakData(dsRus, dsEng, map, fotoPath, isMissingPersons))
                {
                    throw new ArgumentException("Ошибка при конвертации исходного " + sourceFileRus + " файла.");
                }

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace + "\n" + e.Message);
                return false;
            }
        }


        /// <summary>
        /// Получаем фильтр для отображения исходных файлов.
        /// </summary>
        /// <returns>Возвращаем строку фильтра для OpenFileDialog. В случае ошибки возвращаем пустую строку.</returns>
        public static string GetFilterSource()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Config.FilesAllowExtensions))
                    return string.Empty;
                StringBuilder filter = new StringBuilder();
                foreach (string str in Config.FilesAllowExtensions.Split('|'))
                {
                    filter.Append("*.");
                    filter.Append(str);
                    filter.Append(";");
                }
                return string.Format("Файлы Word и Excel ({0}) | {0}", filter.ToString().TrimEnd(';'));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace + "\n" + e.Message);
                return string.Empty;
            }
        }
    }
}
