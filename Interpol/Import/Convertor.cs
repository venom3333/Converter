using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Npgsql;
using NpgsqlTypes;
using GenericParsing;
using System.Configuration;
using System.Data;
using SZI.Import.Templates;
using SZI.Import.Mapping;
using System.Windows.Forms;

namespace SZI.Import
{
    public class Convertor : IDisposable
    {
        /// <summary>
        /// Строка подключения к БД
        /// </summary>
        protected string ConnectionString { get; set; }

        /// <summary>
        /// Подключение к БД
        /// </summary>
        protected NpgsqlConnection Connection { get; set; }

        /// <summary>
        /// Полная информация о файле импорта
        /// </summary>
        protected FileInfo ImportFile { get; set; }

        /// <summary>
        /// Датасет используемых таблиц
        /// </summary>
        protected DataSet DataSet { get; set; }

        /// <summary>
        /// Шаблон для импортируемого файла
        /// </summary>
        protected Template Template { get; set; }

        /// <summary>
        /// Заполненные данные длясоставления insert запросов
        /// </summary>
        protected ImportData ImportData { get; set; }


        public Convertor(FileInfo fileInfo)
        {
            ImportFile = fileInfo;

            ConnectionString = "Server=194.168.0.117;Port=5432;Database=freska;User Id=postgres;Password=%CW_&1|t;";

            Connection = new NpgsqlConnection(ConnectionString);

            // Выбираем темплейт для импортированного файла
            SelectTemplate();

            // Наполняем данные для импорта
            FillDataForImport();

            // Генерация запросов и инсерт в БД
            InsertData();
        }

        private void InsertData()
        {
            var start = DateTime.Now;
            int allRecords = ImportData.MainTable.Count;
            int doublesInFile = 0;
            int doublesInDB = 0;

            // Открываем коннекшн
            Connection.Open();

            NpgsqlTransaction transaction = Connection.BeginTransaction();

            try
            {
                // Общий цикл
                for (int i = 0; i < ImportData.MainTable.Count; i++)
                {
                    // Лист полей для проверки на дубль
                    var checkList = ImportData.MainTable[i].RowTemplate.Where(c => c.ForCompare == true).ToList();

                    // Проверка на дубли в таблице из файла
                    if (IsDoubleFile(checkList))
                    {
                        doublesInFile++;
                        // Удаляем текущую запись и записи с тем же индексом в связанных таблицах
                        ImportData.MainTable.RemoveAt(i);
                        foreach (var additionalTable in ImportData.AdditionalTables)
                        {
                            additionalTable.RemoveAt(i);
                        }
                        // Уменьшаем индекс чтобы не перепрыгнуть запись (т.к. все сдвинется после удаления на -1)
                        i--;
                        continue;
                    }

                    // Проверка на дубль в БД
                    if (IsDoubleDB(checkList, ImportData.MainTable[i].TableName))
                    {
                        doublesInDB++;
                        continue;
                    }

                    /// Составление запроса
                    string mainQuery = GetInsertQueryMain(i);
                    //if (mainQuery.Contains("253370764800"))
                    //{
                    //    var sdsd = 0;
                    //}
                    // Инсерт строки в основную таблицу
                    NpgsqlCommand command = new NpgsqlCommand(mainQuery, Connection, transaction);
                    // Инсерт и получение крайнего ID
                    int mainId = (int)command.ExecuteScalar();

                    // Миницикл связанных таблиц
                    foreach (var additionalTable in ImportData.AdditionalTables)
                    {
                        // Составление запроса
                        string additionalQuery = GetInsertQueryAdditional(additionalTable[i], mainId);
                        command = new NpgsqlCommand(additionalQuery, Connection, transaction);
                        var rows = command.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                MessageBox.Show(ex.Message);
            }
            var end = DateTime.Now;
            MessageBox.Show($@"Всего записей: {allRecords}
Дублей в файле: {doublesInFile}
Дублей в БД: {doublesInDB}
Импортировано: {allRecords - doublesInFile - doublesInDB}
Старт: {start.ToString()}
Финиш: {end.ToString()}");
        }

        /// <summary>
        /// Получает запрос на вставку строки доп таблицы
        /// </summary>
        /// <param name="i">Индекс объекта для вставки</param>
        /// <returns></returns>
        private string GetInsertQueryAdditional(RowItem rowItem, int mainId)
        {
            // Начало
            string query = $@"
                                INSERT INTO {rowItem.TableName}
                                (";

            // Перечисление столбцов
            for (int j = 0; j < rowItem.RowTemplate.Count; j++)
            {
                query += $"{rowItem.RowTemplate[j].TableColumnName}, ";
            }

            // Убрать крайние ", "
            query = query.Remove(query.Length - 2);

            // значения
            query += $@")
                                VALUES(";

            // Перечисление значений
            for (int j = 0; j < rowItem.RowTemplate.Count; j++)
            {
                if (rowItem.RowTemplate[j].ColumnType == DataType.MainObjectId)
                {
                    rowItem.RowTemplate[j].ComputedValue = mainId.ToString();
                }
                query += $"'{rowItem.RowTemplate[j].ComputedValue}', ";
            }

            // Убрать крайние ", "
            query = query.Remove(query.Length - 2);

            query += ");";

            return query;
        }

        /// <summary>
        /// Получает запрос на вставку строки основной таблицы
        /// </summary>
        /// <param name="i">Индекс объекта для вставки</param>
        /// <returns></returns>
        private string GetInsertQueryMain(int i)
        {
            // Начало
            string query = $@"
                                INSERT INTO {ImportData.MainTable[i].TableName}
                                (";

            // Перечисление столбцов
            for (int j = 0; j < ImportData.MainTable[i].RowTemplate.Count; j++)
            {
                // Если дата пуста
                if (ImportData.MainTable[i].RowTemplate[j].ColumnType == DataType.Date && string.IsNullOrWhiteSpace(ImportData.MainTable[i].RowTemplate[j].ComputedValue))
                {
                    continue;
                }
                query += $"{ImportData.MainTable[i].RowTemplate[j].TableColumnName}, ";
            }

            // Убрать крайние ", "
            query = query.Remove(query.Length - 2);

            // значения
            query += $@")
                                VALUES(";

            // Перечисление значений
            for (int j = 0; j < ImportData.MainTable[i].RowTemplate.Count; j++)
            {
                // Если дата пуста
                if (ImportData.MainTable[i].RowTemplate[j].ColumnType == DataType.Date && string.IsNullOrWhiteSpace(ImportData.MainTable[i].RowTemplate[j].ComputedValue))
                {
                    continue;
                }
                query += $"'{ImportData.MainTable[i].RowTemplate[j].ComputedValue}', ";
            }

            // Убрать крайние ", "
            query = query.Remove(query.Length - 2);

            query += ") RETURNING id;";

            return query;
        }

        /// <summary>
        /// Проверяет на дубли в данных из файла
        /// </summary>
        /// <param name="checkList"></param>
        /// <returns></returns>
        private bool IsDoubleFile(List<ColumnItem> checkList)
        {
            var existance = ImportData.MainTable;

            foreach (var checkItem in checkList)
            {
                existance = existance
                    .Where(ri => ri.RowTemplate
                        .Where(rt => rt.FileColumnName == checkItem.FileColumnName)
                        .Where(rt => rt.ComputedValue.ToUpper() == checkItem.ComputedValue.ToUpper()).ToList().Count > 0).ToList();
            }

            return existance.Count > 1 ? true : false;
        }

        /// <summary>
        /// Проверяет на дубли в БД
        /// </summary>
        /// <param name="checkList"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private bool IsDoubleDB(List<ColumnItem> checkList, string tableName)
        {
            long count = 0;
            string query = $@"SELECT COUNT(*) FROM {tableName}
                                WHERE ";

            foreach (var column in checkList)
            {
                if (string.IsNullOrWhiteSpace(column.ComputedValue))
                {
                    continue;
                }

                // Если поле Дата, то округляем до дней для сравнения
                /*to_timestamp(data_rozhdeniya)::date = to_timestamp('836956800')::date*/
                if (column.ColumnType == DataType.Date)
                {
                    query += $@"to_timestamp({column.TableColumnName})::date = to_timestamp('{column.ComputedValue}')::date
                            AND ";
                }
                else
                {
                    query += $@"upper({column.TableColumnName}) = upper('{column.ComputedValue}')
                            AND ";
                }
            }

            // убираем крайний "AND "
            query = query.Remove(query.Length - 4);

            // Define a query returning a single row result set
            NpgsqlCommand command = new NpgsqlCommand(query, Connection);

            // Execute the query and obtain the value of the first column of the first row
            count = (long)command.ExecuteScalar();

            return count > 0 ? true : false;
        }

        /// <summary>
        /// Читаем файл, парсим, наполняем ImportData
        /// </summary>
        private void FillDataForImport()
        {
            // Читаем файл, парсим, наполняем ImportData
            // Инициализация объекта для данных
            ImportData = new ImportData
            {
                MainTable = new List<RowItem>(),
                AdditionalTables = new List<List<RowItem>>()
            };

            // Добавляем нужное кол-во таблиц в ImportData.AdditionalTables
            for (int i = 0; i < Template.AdditionalTablesRows.Count; i++)
            {
                ImportData.AdditionalTables.Add(new List<RowItem>());
            }


            using (GenericParser parser = new GenericParser())
            {
                parser.SetDataSource(ImportFile.FullName);

                parser.ColumnDelimiter = ';';
                parser.FirstRowHasHeader = true;
                parser.SkipStartingDataRows = 0;
                parser.MaxBufferSize = 4096;
                parser.MaxRows = 20000;

                while (parser.Read())
                {
                    // Наполняем данные + параллельно парсим строку csv на значения если нужно

                    /// Основная таблица ///
                    // Инициализация строки
                    var newMainRow = new RowItem
                    {
                        TableName = Template.MainTableRow.TableName,
                        RowTemplate = new List<ColumnItem>()
                    };

                    // Набор столбцов в строку (из файла)
                    foreach (var column in Template.MainTableRow.RowTemplate.Where(c => c.FileColumnName != null))
                    {
                        string rawValue = parser[column.FileColumnName];
                        newMainRow.RowTemplate.Add(new ColumnItem(column, rawValue));
                    }

                    // Набор столбцов в строку (остальные)
                    foreach (var column in Template.MainTableRow.RowTemplate.Where(c => c.FileColumnName == null))
                    {
                        newMainRow.RowTemplate.Add(new ColumnItem(column, newMainRow));
                    }

                    // Добавление строки
                    ImportData.MainTable.Add(newMainRow);

                    /// Связанные таблицы ///
                    // Наполняем строки в каждой по очереди
                    for (int i = 0; i < Template.AdditionalTablesRows.Count; i++)
                    {
                        var newAdditionalRow = new RowItem
                        {
                            TableName = Template.AdditionalTablesRows[i].TableName,
                            RowTemplate = new List<ColumnItem>()
                        };

                        // Набор столбцов в строку (из файла)
                        foreach (var column in Template.AdditionalTablesRows[i].RowTemplate.Where(c => c.FileColumnName != null))
                        {
                            string rawValue = parser[column.FileColumnName];
                            newAdditionalRow.RowTemplate.Add(new ColumnItem(column, rawValue));
                        }

                        // Набор столбцов в строку (остальные)
                        foreach (var column in Template.AdditionalTablesRows[i].RowTemplate.Where(c => c.FileColumnName == null))
                        {
                            newAdditionalRow.RowTemplate.Add(new ColumnItem(column, newMainRow));
                        }

                        // добавление строки
                        ImportData.AdditionalTables[i].Add(newAdditionalRow);
                    }
                }
            }
        }

        /// <summary>
        /// Выбирает шаблона для файла
        /// TODO: Придумать и реализовать логику выбора
        /// </summary>
        void SelectTemplate()
        {
            Template = new FTF();
        }

        public void Dispose()
        {
            Connection.Close();
        }
    }
}
