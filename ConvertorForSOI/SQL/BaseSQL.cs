using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ConvertorForSOI.SQLs
{
    /// <summary>
    /// Базовый Класс для работы с базой данных nak_data.
    /// </summary>
    public static class BaseSQL
    {
        // Набор таблиц для Пропавших без вести
        private static Dictionary<string, string> missingPersonsTables = new Dictionary<string, string>
        {
            {"main", "NAK_TEMP_MISSING" },
            {"ws","NAK_TEMP_WANTEDTYPESEARCH" },
            {"docs","NAK_TEMP_PERSONDOCUMENT" },
            {"photo","NAK_TEMP_PHOTO" },
            {"misc","NAK_TEMP_WANTED_ADDED" },

        };
        // Набор таблиц для Разыскиваемых
        private static Dictionary<string, string> wantedPersonsTables = new Dictionary<string, string>
        {
            {"main", "NAK_TEMP_WANTED" },
            {"ws","NAK_TEMP_WANTEDTYPESEARCH" },
            {"docs","NAK_TEMP_PERSONDOCUMENT" },
            {"photo","NAK_TEMP_PHOTO" },
            {"misc","NAK_TEMP_WANTED_ADDED" },

        };

        // Возвращает нужный набор таблиц
        public static Dictionary<string, string> GetTableSet(bool isMissingPersons)
        {
            return isMissingPersons ? missingPersonsTables : wantedPersonsTables;
        }

        // Очищает темповые таблицы от данных.
        public static bool TruncateTemps(bool isMissingPersons)
        {
            Dictionary<string, string> tableSet = GetTableSet(isMissingPersons);
            try
            {
                string TruncateTemps = String.Format(@"   TRUNCATE TABLE [nak_data].[dbo].[{0}];
                                            TRUNCATE TABLE [nak_data].[dbo].[{1}];
                                            TRUNCATE TABLE [nak_data].[dbo].[{2}];
                                            TRUNCATE TABLE [nak_data].[dbo].[{3}];
                                            TRUNCATE TABLE [nak_data].[dbo].[{4}];
                                        ",  tableSet["main"],
                                            tableSet["ws"],
                                            tableSet["docs"],
                                            tableSet["photo"],
                                            tableSet["misc"]);

                var conn = new SqlConnection(Config.ConnectionStringNakData);
                conn.Open();
                var sqlTruncate = new SqlCommand(TruncateTemps, conn);
                sqlTruncate.ExecuteNonQuery();
                sqlTruncate.Dispose();
                conn.Close();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace + "\n" + ex.Message);
                return false;
            }
        }

        // TODO: универсифицировать
        // Запросы для получения таблиц для результирующих данных.
        public static string SelectTableNakTempWanted = String.Format(@"SELECT * FROM [nak_data].[dbo].[NAK_TEMP_WANTED]");
        public static string SelectTableNakTempWantedSearchType = @"SELECT * FROM [nak_data].[dbo].[NAK_TEMP_WANTEDTYPESEARCH]";
        public static string SelectTableNakTempPersonDocument = @"SELECT * FROM [nak_data].[dbo].[NAK_TEMP_PERSONDOCUMENT]";
        public static string SelectTabTempPhoto = @"SELECT * FROM [nak_data].[dbo].[NAK_TEMP_PHOTO]";
        public static string SelectTableNakTempWantedAdded = @"SELECT * FROM [nak_data].[dbo].[NAK_TEMP_WANTED_ADDED]";

        /// <summary>
        /// Метод заполняет таблицы NAK_TEMP_WANTED, NAK_TEMP_WANTEDTYPESEARCH и остальных вспомогательных баз даных nak_data, данными собранными в DataSet.
        /// </summary>
        /// <param name="dsSourceRus">Dataset после конвертации из файла</param>
        /// <param name="map">Класс Map который отвечает за соответствия ячкеек исходной таблицы и результирующей</param>
        /// <param name="fotoPath">Путь к фотографиям</param>
        /// <param name="isMissingPersons">Стоит ли галочка "Пропавшие без вести"</param>
        /// <returns>Возвращаем true если данные успешно переданы в базу. False в случае ошибки.</returns>
        public static bool FillNakData(DataSet dsSourceRus, DataSet dsSourceEng, Map map, string fotoPath, bool isMissingPersons)
        {
            // Набор таблиц в зависимости от Розыска или Пропавших без вести
            Dictionary<string, string> tableSet = GetTableSet(isMissingPersons);

            // DataSet с таблицами которые будут загружаться в базу данных.
            var dsResult = new DataSet();

            try
            {
                // Делаем подключение к базе данных.
                using (SqlConnection conn = new SqlConnection(Config.ConnectionStringNakData))
                {
                    conn.Open();

                    // Очищаем таблицы от данных.
                    TruncateTemps(isMissingPersons);

                    // Добавляем таблицы в датасет.
                    dsResult.Tables.Add(new DataTable(tableSet["main"]));
                    dsResult.Tables.Add(new DataTable(tableSet["ws"]));
                    dsResult.Tables.Add(new DataTable(tableSet["docs"]));
                    dsResult.Tables.Add(new DataTable(tableSet["photo"]));
                    dsResult.Tables.Add(new DataTable(tableSet["misc"]));

                    // Создаём для каждой таблицы свой датаадаптер.
                    SqlDataAdapter adapterWanted = new SqlDataAdapter(SelectTableNakTempWanted, conn);
                    SqlDataAdapter adapterWantedAdded = new SqlDataAdapter(SelectTableNakTempWantedAdded, conn);
                    SqlDataAdapter adapterSearch = new SqlDataAdapter(SelectTableNakTempWantedSearchType, conn);
                    SqlDataAdapter adapterPersonDoc = new SqlDataAdapter(SelectTableNakTempPersonDocument, conn);
                    SqlDataAdapter adapterTempPhoto = new SqlDataAdapter(SelectTabTempPhoto, conn);

                    // Загружаем очищенные таблицы через датаадаптер.
                    adapterWanted.Fill(dsResult, tableSet["main"]);
                    adapterWantedAdded.Fill(dsResult, tableSet["misc"]);
                    adapterSearch.Fill(dsResult, tableSet["ws"]);
                    adapterPersonDoc.Fill(dsResult, tableSet["docs"]);
                    adapterTempPhoto.Fill(dsResult, tableSet["photo"]);

                    // Заполняем данными датасет dsResult.
                    if (!Convertor.ConvertToCorrectData(dsSourceRus, dsSourceEng, ref dsResult, map, fotoPath, isMissingPersons))
                        throw new ArgumentException("Ошибка при конвертации исходного при заполнении базы данных.");

                    // Формируем команды INSERT для заполнения базы данных новыми данными.
                    SqlCommand wantedInsert = GetInsertCommand(dsResult.Tables[tableSet["main"]], conn);
                    adapterWanted.InsertCommand = wantedInsert;
                    SqlCommand wantedAddedInsert = GetInsertCommand(dsResult.Tables[tableSet["misc"]], conn);
                    adapterWantedAdded.InsertCommand = wantedAddedInsert;
                    SqlCommand searchInsert = GetInsertCommand(dsResult.Tables[tableSet["ws"]], conn);
                    adapterSearch.InsertCommand = searchInsert;
                    SqlCommand personDocInsert = GetInsertCommand(dsResult.Tables[tableSet["docs"]], conn);
                    adapterPersonDoc.InsertCommand = personDocInsert;
                    SqlCommand tempPhotoInsert = GetInsertCommand(dsResult.Tables[tableSet["photo"]], conn);
                    adapterTempPhoto.InsertCommand = tempPhotoInsert;

                    // Заполняем таблицы базы данных.
                    adapterWanted.Update(dsResult, tableSet["main"]);
                    adapterWantedAdded.Update(dsResult, tableSet["misc"]);
                    adapterSearch.Update(dsResult, tableSet["ws"]);
                    adapterPersonDoc.Update(dsResult, tableSet["docs"]);
                    adapterTempPhoto.Update(dsResult, tableSet["photo"]);

                    // Закрываем подключение.
                    adapterWanted.Dispose();
                    adapterWantedAdded.Dispose();
                    adapterSearch.Dispose();
                    adapterPersonDoc.Dispose();
                    adapterTempPhoto.Dispose();
                    conn.Close();

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace + "\n" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Метод формирует команду инсерт на основе DataTable и подключения к базе.
        /// </summary>
        /// <param name="dt">Таблица с данными.</param>
        /// <param name="conn">Подключение к базе.</param>
        /// <returns>SqlCommand Insert.</returns>
        private static SqlCommand GetInsertCommand(DataTable dt, SqlConnection conn)
        {
            StringBuilder sb = new StringBuilder("INSERT " + dt.TableName + " (");
            StringBuilder sbParams = new StringBuilder(" VALUES(");
            foreach (DataColumn col in dt.Columns)
            {
                sb.Append("[" + col.ColumnName + "]" + ",");
                sbParams.Append("@" + col.ColumnName + ",");
            }
            string insert = sb.ToString().Trim(',') + ")" + sbParams.ToString().TrimEnd(',') + ")";

            SqlCommand sqlCommandInsert = new SqlCommand(insert, conn);

            foreach (DataColumn col in dt.Columns)
            {
                sqlCommandInsert.Parameters.AddWithValue("@" + col.ColumnName, col.DataType).SourceColumn = col.ColumnName;
            }

            return sqlCommandInsert;
        }
    }
}