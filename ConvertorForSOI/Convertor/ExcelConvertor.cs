using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;
using System.Windows;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using ExcelDataReader;

namespace ConvertorForSOI
{
    class ExcelConvertor
    {
        public static DataSet XlsXToDataSet(string sourceFile, Map map)
        {
            //Reading from a binary Excel file ('97-2003 format; *.xls)
            //IExcelDataReader excelReader2003 = ExcelReaderFactory.CreateBinaryReader(stream);

            //Reading from a OpenXml Excel file (2007 format; *.xlsx)
            FileStream stream = new FileStream(sourceFile, FileMode.Open);
            IExcelDataReader excelReader2007 = ExcelReaderFactory.CreateOpenXmlReader(stream);

            //DataSet - The result of each spreadsheet will be created in the result.Tables
            DataSet ds = excelReader2007.AsDataSet();
            stream.Close();
            return ds;
        }
        /// <summary>
        /// Открываем поданный на вход xls файл, берем из него первый лист (если нет листов возвращаем ошибку).
        /// Возвращаем данные листа в виде DataTable и ещё одну таблицу с названием файла и типом документа.
        /// </summary>
        /// <param name="sourceFile">Месторасположение входящего excel(xls) файла.</param>
        /// <returns>Таблица в виде DataTable. Если произошла ошибка, то возвращаем Null.</returns>
        public static DataSet XlsToTable(string sourceFile, Map map)
        {
            if (!File.Exists(sourceFile) || Path.GetExtension(sourceFile).ToLower() != ".xls")
            {
                MessageBox.Show("Укажите корректный источник данных.");
                return null;
            }
            DataSet ds = new DataSet();
            DataTable dtBody = new DataTable();
            DataTable dtHeader = new DataTable();

            // Создание строки подключения для OleDB.
            string connectionOleDb = ConnectionToOleDb(sourceFile);
            try
            {
                // Создаём подключение к OleDB.
                using (OleDbConnection conn = new OleDbConnection(connectionOleDb))
                {
                    conn.Open();
                    OleDbCommand cmd = new OleDbCommand
                    {
                        Connection = conn
                    };

                    // Получение листов Excel файла.
                    DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                    // Если листов менее одного, возвращаем null.
                    if (dtSheet.Rows.Count < 1)
                        return null;

                    // Заполняем dtHeader.
                    dtHeader.TableName = "Header";
                    dtHeader.Columns.Add("Header");
                    dtHeader.Rows.Add(sourceFile);
                    dtHeader.Rows.Add(" ");
                    dtHeader.Rows.Add(MapSourceItem.TypeDocument.Xls.ToString());

                    int rowNumber = map.GetRowNumberByTypeDocument(MapSourceItem.TypeDocument.Xls);

                    if (dtSheet.Rows.Count < 1)
                        throw new ArgumentException("В xls файле нет листов.");

                    // Loop through all Sheets to get data.
                    foreach (DataRow dr in dtSheet.Rows)
                    {
                        string sheetName = dr["TABLE_NAME"].ToString();

                        if (sheetName.EndsWith("$"))
                            continue;

                        // Get all rows from the Sheet.
                        cmd.CommandText = "SELECT * FROM [" + sheetName + "]";

                        ds.DataSetName = sheetName.Trim().Replace(" ", "_").Replace("$", "").Replace("'", "").Replace("-", "_");
                        dtBody.TableName = "Body";
                        OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                        da.Fill(dtBody);
                    }

                    cmd = null;
                    conn.Close();

                    // Удаляем лишние строки из таблицы. 
                    // Так как в таблице ряды нумеруются с нуля, а в MapSourceItem считается, что с 1, поэтому -1 и так как первый ряд не записывается так как идет в названия колонок, то ещё -1.
                    if (rowNumber > 2)
                    {
                        for (int i = 0; i < rowNumber - 2; i++)
                        {
                            dtBody.Rows.RemoveAt(i);
                        }
                    }

                    ds.Tables.Add(dtHeader);
                    ds.Tables.Add(dtBody);
                    return ds;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }

        }

        /// <summary>
        ///  Метод создаёт строку подключения к OleDd для файла источника данных xls.
        /// </summary>
        /// <param name="sourceFile">Путь к файлу xls, источнику данных.</param>
        /// <returns>Строка подключения.</returns>
        public static string ConnectionToOleDb(string sourceFile)
        {
            // Создание строки подключения для OleDB.
            return string.Format("Provider=" + Config.ConnectionOleDb + ";" + "Extended Properties=" + Config.ExtendedProperties + ";" + "Data Source=" + sourceFile);
        }

        /// <summary>
        /// Записываем таблицу DataTable в xls файл.
        /// </summary>
        /// <param name="dt">Таблица с данными.</param>
        /// <param name="dataPath">Путь к создаваемому файлу, куда записываются данные.</param>
        /// <returns>Путь к создаваемому файлу, куда записываются данные. В случае ошибки возвращаем Null.</returns>
        public static string TableToXls(DataTable dt, string dataPath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(dataPath)))
            {
                MessageBox.Show("Разместите xls файл, куда сохранять данные, в корректной папке.");
                return null;
            }
            if (File.Exists(dataPath))
            {
                File.Delete(dataPath);
            }
            try
            {
                string connetionOleDb = ConnectionToOleDb(dataPath);
                using (OleDbConnection conn = new OleDbConnection(connetionOleDb))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    string strTableName = string.IsNullOrWhiteSpace(dt.TableName) ? "Lica1" : dt.TableName;
                    StringBuilder sb = new StringBuilder("INSERT INTO " + strTableName + " (");
                    StringBuilder sbCreateTable = new StringBuilder("CREATE TABLE " + strTableName + " (");
                    foreach (DataColumn col in dt.Columns)
                    {
                        sb.Append("[" + col.ToString() + "]" + ",");
                        sbCreateTable.Append(" [" + col.ToString() + "] VARCHAR ,");
                    }
                    string commandCreateTable = sbCreateTable.ToString().Replace("/", "").TrimEnd(',') + ")";
                    cmd.CommandText = commandCreateTable; // Create Sheet 
                    cmd.ExecuteNonQuery();
                    string commandTextColumn = sb.ToString().Replace("/", "").TrimEnd(',') + ") VALUES( ";

                    foreach (DataRow row in dt.Rows)
                    {
                        cmd = new OleDbCommand();
                        cmd.Connection = conn;
                        StringBuilder sbRow = new StringBuilder(commandTextColumn);
                        int i = 0;
                        foreach (var item in row.ItemArray)
                        {
                            string value = string.IsNullOrWhiteSpace(item.ToString()) ? " " : item.ToString();
                            sbRow.Append("@" + i.ToString() + ",");    //("'" + value + "'" + ",");
                            cmd.Parameters.AddWithValue("@" + i.ToString(), value);
                            i++;
                        }
                        string commandText = sbRow.ToString().TrimEnd(',') + " )";
                        cmd.CommandText = commandText;
                        cmd.ExecuteNonQuery(); // Execute insert query against excel file.
                    }
                }
                return dataPath;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }

        }
    }
}
