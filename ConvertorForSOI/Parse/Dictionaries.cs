using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;


namespace ConvertorForSOI.Parse
{
    // Класс отвечает за обработку словарей и списков
    abstract public class Dictionaries
    {
        // Все словари и списки (из файла)
        protected static string[] dictionaries = new string[0];

        // Ведомства
        // Список ведомств, добавлять по количеству символов (чем важнее (более обобщенные) - тем выше) , ФСБ должна быть под индексом 0!
        protected static List<string> departments = new List<string>();

        // Инициаторы розыска
        protected static Dictionary<string, string> wantedInitiators = new Dictionary<string, string>();

        // Типы розыска
        protected static Dictionary<string, string> searchTypes = new Dictionary<string, string>();

        // Документы Лица
        protected static Dictionary<string, string> personalDocs = new Dictionary<string, string>();


        // Конструктор, наполняем dictionaries из файла + раскладываем по секциям
        static Dictionaries()
        {
            // наполняем словари (если первый раз)
            if (dictionaries.Length == 0)
            {
                // общий
                dictionaries = GetDictionariesFromFile();

                // ведомства
                departments = GetListOf("Departments");

                // инициаторы розыска
                wantedInitiators = GetDictionaryOf("WantedInitiators");

                // типы розыска
                searchTypes = GetDictionaryOf("SearchTypes");

                // документы лица
                personalDocs = GetDictionaryOf("PersonalDocs");
            }

        }

        // Загружает в переменную dictionaries информацию из файла со словарями и списками
        private static string[] GetDictionariesFromFile()
        {
            string[] dictionaries;
            string path = Path.Combine(Directory.GetCurrentDirectory(), ConfigurationManager.AppSettings["Dictionaries"].ToString());
            try
            {
                dictionaries = File.ReadAllLines(path, Encoding.UTF8);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace + "\n" + e.Message);
                return new string[0];
            }
            return dictionaries;
        }

        // Наполняет List из соответствующей секции (раздела)
        private static List<string> GetListOf(string section) // section - название секции в ini-файле
        {
            // Список ведомств
            List<string> list = new List<string>();


            for (int i = 0; i < dictionaries.Length; i++)
            {
                // флаг, показывающий что внутренний цикл сделал дело
                bool done = false;

                // Если встретили нужную секцию + приведем все к верхнему регистру
                if (dictionaries[i].ToUpper().Contains(section.ToUpper()))
                {
                    for (int j = i + 1; j < dictionaries.Length; j++)
                    {
                        // Если встретилась пустая строка или комментарий
                        if (String.IsNullOrWhiteSpace(dictionaries[j]) || dictionaries[j].Contains("#"))
                        {
                            continue;
                        }
                        // Если началась следующая секция
                        if (dictionaries[j].Contains("["))
                        {
                            done = true;
                            break;
                        }
                        else
                        {
                            // Добавляем строку в список
                            list.Add(dictionaries[j].Trim());
                        }
                    }
                }
                // Если флаг true
                if (done)
                {
                    break;
                }
            }
            return list;
        }

        // Наполняет List из соответствующей секции (раздела)
        private static Dictionary<string, string> GetDictionaryOf(string section) // section - название секции в ini-файле
        {
            // Список ведомств
            Dictionary<string, string> dictionary = new Dictionary<string, string>();


            for (int i = 0; i < dictionaries.Length; i++)
            {
                // флаг, показывающий что внутренний цикл сделал дело
                bool done = false;

                // Если встретили нужную секцию + приведем все к верхнему регистру
                if (dictionaries[i].ToUpper().Contains(section.ToUpper()))
                {
                    for (int j = i + 1; j < dictionaries.Length; j++)
                    {
                        // Если встретилась пустая строка или комментарий
                        if (String.IsNullOrWhiteSpace(dictionaries[j]) || dictionaries[j].Contains("#"))
                        {
                            continue;
                        }
                        // Если началась следующая секция
                        if (dictionaries[j].Contains("["))
                        {
                            done = true;
                            break;
                        }
                        else
                        {
                            // Разбиваем строку на массив
                            string[] keyValue = dictionaries[j].Split(',');
                            // Добавляем ключ => значение в словарь
                            dictionary.Add(keyValue[0].Trim(), keyValue[1].Trim());
                        }
                    }
                }
                // Если флаг true
                if (done)
                {
                    break;
                }
            }
            return dictionary;
        }
    }
}
