using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;

namespace ConvertorForSOI
{
    /// <summary>
    /// Класс конфиг для получения и записи значений в файл App.config.
    /// </summary>
    public class Config
    {
        // Путь к папке с источником данных.
        public static string SourceFolder
        {
            get
            {
                if (Directory.Exists(ConfigurationManager.AppSettings["FilesSourceFolder"]))
                    return ConfigurationManager.AppSettings["FilesSourceFolder"];
                else return string.Empty;
            }
            set
            {
                if (Directory.Exists(Path.GetDirectoryName(value)))
                    ConfigurationManager.AppSettings["FilesSourceFolder"] = Path.GetDirectoryName(value);
            }
        }

        // Путь к папке результирующей таблице в .xls файле.
        public static string DataFolder
        {
            get
            {
                if (Directory.Exists(ConfigurationManager.AppSettings["FilesConvertFolder"]))
                    return ConfigurationManager.AppSettings["FilesConvertFolder"];
                else return string.Empty;
            }
            set
            {
                if (Directory.Exists(Path.GetDirectoryName(value)))
                    ConfigurationManager.AppSettings["FilesConvertFolder"] = Path.GetDirectoryName(value);
            }
        }

        // Строка подключения к OleDb.
        public static string ConnectionOleDb
        {
            get { return ConfigurationManager.AppSettings["OledbConnection"]; }

        }

        // Для подключения к OleDb.
        public static string ExtendedProperties
        {
            get { return ConfigurationManager.AppSettings["ExtendedProperties"]; }
        }

        // Разрешенные форматы для файлов источника данных.
        public static string FilesAllowExtensions
        {
            get { return ConfigurationManager.AppSettings["FilesAllowExtensions"]; }
        }

        // Получаем строку подключения (connectionstring) к базе данных nak_data.
        public static string ConnectionStringNakData
        {
            get { return ConfigurationManager.ConnectionStrings["Nake_data"].ConnectionString; }
        }
    }
}
