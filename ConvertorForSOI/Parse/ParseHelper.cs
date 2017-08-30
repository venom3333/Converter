using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace ConvertorForSOI
{
    static class ParseHelper
    {
        // Список для нераспознанных типов розыска
        public static List<string> missedSearchTypes = new List<string>();
        // Список для нераспознанных типов розыска в БД
        public static List<string> missedSearchTypesDB = new List<string>();

        // Список для нераспознанных инициаторов поиска
        public static List<string> missedWantedInitiators = new List<string>();
        // Список для нераспознанных инициаторов поиска в БД
        public static List<string> missedWantedInitiatorsDB = new List<string>();

        public static void DeleteInfoFiles()
        {
            List<string> fileNames = new List<string>()
            {
                "SearchTypesDB.txt",
                "SearchTypes.txt",
                "WantedInitiators.txt",
                "WantedInitiatorsDB.txt"
            };
            string path = Directory.GetCurrentDirectory();
            fileNames.ForEach(delegate (String name)
            {
                File.Delete(System.IO.Path.Combine(path, name));
            });
        }

        public static void ClearMissedLists()
        {
            ParseHelper.missedSearchTypes.Clear();
            ParseHelper.missedSearchTypesDB.Clear();
            ParseHelper.missedWantedInitiators.Clear();
            ParseHelper.missedWantedInitiatorsDB.Clear();
        }

        public static void SortMissedLists()
        {
            missedSearchTypes.Sort();
            missedSearchTypesDB.Sort();
            missedWantedInitiators.Sort();
            missedWantedInitiatorsDB.Sort();
        }

        public static int MonthToInt(string month)
        {
            switch (month)
            {
                case "ЯНВАРЬ":
                    return 1;
                case "ФЕВРАЛЬ":
                    return 2;
                case "МАРТ":
                    return 3;
                case "АПРЕЛЬ":
                    return 4;
                case "МАЙ":
                    return 5;
                case "ИЮНЬ":
                    return 6;
                case "ИЮЛЬ":
                    return 7;
                case "АВГУСТ":
                    return 8;
                case "СЕНТЯБРЬ":
                    return 9;
                case "ОКТЯБРЬ":
                    return 10;
                case "НОЯБРЬ":
                    return 11;
                case "ДЕКАБРЬ":
                    return 12;
                default:
                    return 0;
            }
        }

        // Чистим спец символы в строке
        public static string ClearString (string str)
        {
            Regex rgx = new Regex("[^\\S ]");
            str = rgx.Replace(str, " ");
            str = str.Trim('\r', '\a', '\n', '\t', ' ');
            return str;
        }
    }
}
