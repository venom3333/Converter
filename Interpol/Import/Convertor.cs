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

            // Набираем датасет в соответствии с используемыми таблицами
            // FillDataSet();

            // Апдейтим Датасет в соответствии с импортируемыми данными
        }

        private void InsertData()
        {
            // Общий цикл
            for (int i = 0; i < ImportData.MainTable.Count; i++)
            {
                // Составление запроса

                // Инсерт строки в основную таблицу

                // Получение крайнего ID инсерта выше

                // Миницикл связанных таблиц
                foreach (var additionalTable in ImportData.AdditionalTables)
                {
                    // Составление запроса

                    // если ColumnType = DataType.MainObjectId, то ComputedValue = вычисленный ID вставки в основную таблицу
                }


            }
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

        /// <summary>
        /// Наполняет Датасет в зависимости от используемых таблиц в соответствии с шаблоном
        /// </summary>
        void FillDataSet()
        {
            DataSet = new DataSet();
            // основная таблица
            NpgsqlDataAdapter mainTableAdapter = new NpgsqlDataAdapter($"SELECT * FROM {Template.MainTableRow.TableName}", Connection);
            mainTableAdapter.Fill(DataSet, Template.MainTableRow.TableName);

            // Связанные таблицы
            foreach (var table in Template.AdditionalTablesRows)
            {
                NpgsqlDataAdapter additionalTableAdapter = new NpgsqlDataAdapter($"SELECT * FROM {table.TableName}", Connection);
                additionalTableAdapter.Fill(DataSet, table.TableName);
            }
        }

        public void Dispose()
        {
            Connection.Close();
        }

        /*
        private bool LogInsert()
        {
            try
            {
                SqlCommand insertCommand = new SqlCommand(
                    $@"
                    INSERT INTO [{_logTableName}]
                        ([Created]
                        ,[Event]
                        ,[UserName]
                        ,[IOType]
                        ,[Info]
                        ,[Misc])
                    VALUES
                        (convert(datetime2, '{_created.ToString()}', 104)
                        ,'{_eventType.ToString()}'
                        ,'{_user}'
                        ,'{_iOType}'
                        ,'{_info}'
                        ,'IP: {_userIP}<br>{_misc}')
                    ",
                    _connection);

                SqlDataAdapter dataAdapter =
                new SqlDataAdapter();

                _connection.Open();

                dataAdapter.InsertCommand = insertCommand;
                dataAdapter.InsertCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                LoggerObject.LogException(ex);
                return false;
            }
            finally
            {
                _connection.Close();
            }
            return true;
        }
        */
    }
}