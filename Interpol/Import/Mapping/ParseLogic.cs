using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SZI.Import.Mapping
{
    /// <summary>
    /// Преобразование RawValue в ComputedValue
    /// </summary>
    public static class ParseLogic
    {
        private static string ConnectionString = ConfigurationManager.ConnectionStrings["freska"].ConnectionString;

        /// <summary>
        /// Парсер даты
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        internal static string ParseDate(string date)
        {
            if (string.IsNullOrWhiteSpace(date))
            {
                return string.Empty;
            }

            string result = string.Empty;
            char separator = '.';
            DateTime dateTime = DateTime.Now;
            // 1-й вариант: 8 цифр подряд:
            Regex digitsOnly = new Regex(@"^(\d{4})(\d{2})(\d{2})$");

            // 2-й вариант: только год (4 цифры)
            Regex yearOnly = new Regex(@"^\d{4}$");

            if (digitsOnly.IsMatch(date))
            {
                date = digitsOnly.Replace(date, "$1.$2.$3");
                var dateParts = date.Split(separator);
                int year = int.Parse(dateParts[0]);
                int month = int.Parse(dateParts[1]) > 12 ? 1 : int.Parse(dateParts[1]);
                int day = int.Parse(dateParts[2]) > 31 ? 1 : int.Parse(dateParts[2]);
                dateTime = new DateTime(year, month, day);
            }
            else if (yearOnly.IsMatch(date))
            {
                dateTime = new DateTime(int.Parse(date), 1, 1);
            }

            var timestamp = dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            result = timestamp > int.MaxValue ? string.Empty : timestamp.ToString();

            return result;
        }

        /// <summary>
        /// Парсер для текста по формуле
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        internal static string ParseFormulaText(ColumnItem column)
        {
            var result = string.Empty;

            switch (column.ColumnFormula.Operator)
            {
                case FormulaOperator.concat:
                    var values = column.CurentRow.RowTemplate.Where(c => column.ColumnFormula.Fields.Contains(c.FileColumnName)).Select(c => c.ComputedValue).ToArray();
                    result = string.Join(column.ColumnFormula.Separator[0], values);
                    break;

                case FormulaOperator.single:
                    break;

                case FormulaOperator.exact:
                    break;

                default:
                    throw new NotImplementedException("Оператор формулы текста не распознан!");
            }

            return result;
        }

        /// <summary>
        /// Получение ID из словаря (dictionaryitem)
        /// </summary>
        /// <param name="columnItem"></param>
        /// <returns></returns>
        internal static string ParseDictionary(ColumnItem columnItem)
        {
            var result = string.Empty;

            if (columnItem.DictionaryTableName == null)
            {
                columnItem.DictionaryTableName = "dictionaryitem";
            }

            if (columnItem.DictionaryColumnName == null)
            {
                columnItem.DictionaryColumnName = "termin_ru";
            }

            var query = $@"SELECT id FROM {columnItem.DictionaryTableName}
                            WHERE ";

            if (columnItem.DictionaryDType != null)
            {
                query += $@"dtype = '{columnItem.DictionaryDType}'
                            AND ";
            }

            query += $"upper({columnItem.DictionaryColumnName}) LIKE upper('%{columnItem.RawValue}%')";

            var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();
            try
            {
                var command = new NpgsqlCommand(query, connection);
                var response = ((int)(command.ExecuteScalar() ?? 0));
                result = response == 0 ? string.Empty : response.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return result;
        }

        /// <summary>
        /// Обработка файла + получение его конечного имени
        /// </summary>
        /// <param name="columnItem"></param>
        /// <returns></returns>
        internal static string ParseMedia(FileInfo file, string fullName)
        {
            var result = string.Empty;

            // Проверяем есть ли папка с фотографиями
            string photoDir = file.DirectoryName + $"\\{file.Name.Replace($"{file.Extension}", "")}";
            string newPhotoDir = file.DirectoryName + $"\\{file.Name.Replace($"{file.Extension}", "(renamed)")}";
            if (!Directory.Exists(photoDir))
            {
                return string.Empty;
            }

            // Составляем список файлов в директории
            var files = Directory.EnumerateFiles(photoDir);
            // Ищем файл по имени
            var fileName = files.FirstOrDefault(f => f.ToUpper().Contains(fullName.ToUpper()));
            if (fileName == null)
            {
                return string.Empty;
            }

            FileInfo mediaFile = new FileInfo(fileName);

            // Проверяем на существование и создаем папку для переименованных файлов
            if (!Directory.Exists(newPhotoDir))
            {
                Directory.CreateDirectory(newPhotoDir);
            }

            // Генерируем новое имя файла
            string generatedFileName = CalculateMD5Hash(fileName + new Guid().ToString()) + mediaFile.Extension;

            // Копируем файл с новым именем в новую папку
            File.Copy(Path.Combine(photoDir, fileName), Path.Combine(newPhotoDir, generatedFileName));

            result = generatedFileName;
            return result; // сгенерированное имя файла
        }

        private static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
