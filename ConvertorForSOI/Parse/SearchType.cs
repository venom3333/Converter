using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.RegularExpressions;
using ConvertorForSOI.SQLs;

namespace ConvertorForSOI
{
    // Класс для объектов тип розыска (searchtype).
    public class SearchType : Parse.Dictionaries
    {
        public int Id;
        public string Name;
        public string NameEn;

        // Карта соответствий ключевых слов в источнике, названиям типа розыска в базе данных nak_data.nak_searchtype.
        // Если ключевое слово присутствует в строке источника,то по этому ключевому слову подставляется значение из карты.
        /*
        private static Dictionary<string, string> SearchTypeMap = new Dictionary<string, string>()
        {
            {"азер", "Розыск Азербайджанской Республики"},
            {"беларус","Розыск Республики Беларусь"},
            {"груз","Розыск Грузии"},
            {"казах","Розыск Республики Казахстан"},
            {"кыргыз","Розыск Кыргызской Республики"},
            {"молдов","Розыск Республики Молдова"},
            {"рф","Федеральный розыск Российской Федерации"},
            {"таджик","Розыск Республики Таджикистан"},
            {"туркмен","Розыск Республики Туркменистан"},
            {"узбек","Розыск Республики Узбекистан"}
        };
        */

        /// <summary>
        /// Метод возвращает коллекцию строк с типами розыска и, при необходимости, заполняет список ненайденных.
        /// </summary>
        /// <param name="typeDocument">Тип документа.</param>
        /// <param name="source">Строка источник информации.</param>
        /// <returns>Масссив строк, где каждая строка это тип розыска. В случае неудачи возвращаем пустой массив.</returns>
        public static List<string> GetSearchTypes(MapSourceItem.TypeDocument typeDocument, string source)
        {
                List<string> searchType = new List<string>();

                // Пробуем получить соответствие из SearchTypeMap
                try
                {
                    searchType = searchTypes.Where(g => source.ToLower().Contains(g.Key)).Select(g => g.Value).ToList();
                }
                catch (Exception ex)
                {
                    // Пишем строки в которых не нашли соответствия с типами розыска из SearchTypeMap
                    if (ParseHelper.missedSearchTypes.IndexOf(source) == -1)
                    {
                        ParseHelper.missedSearchTypes.Add(source);
                    }
                }

                /// Проверяем есть ли такой инициатор розыска в БД (NAK_WANTEDINITIATOR)
                // есть ли имя вообще в переменной
                if (searchType.Count != 0 && !String.IsNullOrWhiteSpace(searchType[0]))
                {

                    // если до этого не добавляли в список, добавить
                    if (ParseHelper.missedSearchTypesDB.IndexOf(searchType[0]) == -1)
                    {
                        // проверка в БД
                        if (!WantedSQL.SearchTypeExists(searchType[0]))
                        {
                            ParseHelper.missedSearchTypesDB.Add(searchType[0]);
                        }
                    }
                return searchType;
            }
            return new List<string>();
        }




    }
}
