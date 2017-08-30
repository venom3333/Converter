using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.RegularExpressions;

namespace ConvertorForSOI
{
    // Класс для обработки документов.
    public class PersonDocumentType : Parse.Dictionaries
    {
        //private string[] codeDocuments;
        //private string[] nameDocuments;

        // Для кодов документов.
        public List<string> documentData = new List<string>();

        // Для наименований документов.
        public List<string> documentNames = new List<string>();

        //// Карта соответствий названий документов в источниках документах, названиям в базе данных.
        //private static Dictionary<string, string> PersonalDocTable = new Dictionary<string, string>()
        //{
        //    {"ЗАГРАНПАСПОРТ", "Заграничный паспорт"},
        //    {"ПАСПОРТ ИНОСТРАНЦА И СНГ","паспорт иностранца и СНГ"},
        //    {"СВИДЕТЕЛЬСТВО О РОЖДЕНИИ","Свидетельство о рождении"},
        //    {"СПРАВКА ОБ УТЕРЕ ПАСПОРТА",""},
        //    {"УДОСТОВЕРЕНИЕ ЛИЧНОСТИ","Удостоверение личности"},
        //    {"ВИД НА ЖИТЕЛЬСТВО","вид на жительство"},
        //    {"ВОЕННЫЙ БИЛЕТ","Военный билет"},
        //    {"ВТОРОЙ ПАСПОРТ",""},
        //    {"ВОДИТЕЛЬСКОЕ УДОСТОВЕРЕН","Водительское удостоверение"},
        //    {"Паспорт","Паспорт"},
        //    {"Паспорт гражданина РФ","Паспорт гражданина РФ"},
        //    {"Паспорт гражданина СССР","Паспорт гражданина СССР"},
        //};

        /// <summary>
        /// Метод для парсинга входящих строк с документами удостоверяющими личность.
        /// </summary>
        /// <param name="strInputDoc">Строка с документами.</param>
        /// <returns>True в случае успеха (если не нашли ни одного документа, то тоже true). False в случае ошибки.</returns>
        public bool TryParse(string strInputDoc)
        {
            if (string.IsNullOrWhiteSpace(strInputDoc))
                return true;
            try
            {
                // Regex для паспорта РФ /((\d{4}\s)|(\d{2}\s?\d{2}))\s?((№?)|(номер)?)\s?(\d{6}(\s|$)){1}/
                // Парсинг строки с документами.

                // Делим строку с документами по разделителям
                char[] delims = { ',', ';' };
                string[] docs = strInputDoc.Split(delims, StringSplitOptions.RemoveEmptyEntries);



                // Определяем тип документа + наполняем лист наименований типов документов
                List<string> keys = personalDocs.Keys.ToList();
                List<string> values = personalDocs.Values.ToList();

                // Регулярка Паспорт РФ
                Regex pasportRF = new Regex (@"((\d{4}\s)|(\d{2}\s?\d{2}))\s?((№?)|(номер)?)\s?(\d{6}(\s|$)){1}");

                for (int i = 0; i < docs.Length; i++)
                {
                    if (pasportRF.IsMatch(docs[i]))
                    {
                        documentNames.Add("Паспорт гражданина РФ");
                    }
                    else
                    {
                        bool found = false;
                        for (int key = 0; key < keys.Count; key++)
                        {
                            if (docs[i].ToUpper().Contains(keys[key].ToUpper()))
                            {
                                documentNames.Add(values[key]);
                                found = true;
                                break;
                            }
                        }

                        // Если не найдено в ключах
                        if (!found)
                        {
                            documentNames.Insert(i, "");
                        }

                        // Если после проверки по ключам тип документа не распознан
                        if (String.IsNullOrWhiteSpace(documentNames[i]))
                        {
                            documentNames[i] = GetPasportType(documentNames[i]);
                        }
                    }
                    // Обрезаем начало строк до первой встретившейся цифры + наполняем лист с данными
                    Regex rgxBefore = new Regex("^([^\\d]+)");
                    //Regex rgxAfter = new Regex("([^\\d]+)$");
                    docs[i] = rgxBefore.Replace(docs[i], "").Trim('.', ' ');
                    //codeDocuments[cdIdx] = rgxAfter.Replace(codeDocuments[cdIdx], "").Trim('.', ' ');
                    documentData.Add(docs[i]);
                }
                /*
                     // Добавляем наименование документа код которого идет в начале строки без наименования документа.
                if (CodeDocuments.Count - NameDocuments.Count == 1)
                {
                    NameDocuments.Insert(0, GetPasportType(CodeDocuments[0]));
                }
            for (int i = 0; i < NameDocuments.Count; i++)
            {
                if (NameDocuments[i] == "ВТОРОЙ ПАСПОРТ")
                {
                    NameDocuments[i] = GetPasportType(CodeDocuments[i]);
                }
            }

            // Преобразовать наименования документов в соответствии с картой, в наименования понятные базе данных.
            // TODO: Переделать этот ужас в лямбду



            NameDocuments = newList;
            //NameDocuments = NameDocuments.Select(g => personalDocs[g]).ToList();
            */




                //for (int doc = 0; doc < docs.Length; doc++)
                //{
                //    // Удаляем информацию когда выдан документ (т.к. нет в базе ячеек для этого)
                //    // из парсера запишем эту инфу в доп.таблицу
                //    if (docs[doc].Contains("выдан"))
                //    {
                //        docs[doc] = docs[doc].Remove(docs[doc].IndexOf("выдан")).Trim();
                //    }
                //}

                // strInputDoc = string.Join(";", docs);

                //strInputDoc = docs[0];

                //// Добавляем ; в качестве разделителя к списку типов документов поданных в исходном файле.
                //string[] splits = personalDocs.Keys.ToArray<string>();
                //Array.Resize(ref splits, splits.Length + 2);
                //splits[splits.Length - 1] = ";";
                //splits[splits.Length - 2] = ",";



                //string[] codeDocuments = strInputDoc.Trim().Split(splits, StringSplitOptions.RemoveEmptyEntries);

                //for (int cdIdx = 0; cdIdx < codeDocuments.Length; cdIdx++)
                //{
                //    Regex rgxBefore = new Regex("^([^\\d]+)");
                //    Regex rgxAfter = new Regex("([^\\d]+)$");
                //    codeDocuments[cdIdx] = rgxBefore.Replace(codeDocuments[cdIdx], "").Trim('.', ' ');
                //    codeDocuments[cdIdx] = rgxAfter.Replace(codeDocuments[cdIdx], "").Trim('.', ' ');
                //}

                // Добавляем к кодам документов ; в качестве разделителя для упрощения получения наименований документов.
                //Array.Resize(ref codeDocuments, codeDocuments.Length + 1);
                //codeDocuments[codeDocuments.Length - 1] = ";";

                //// Парсинг наименований документов.
                //string[] nameDocuments = strInputDoc.Trim().Split(codeDocuments, StringSplitOptions.RemoveEmptyEntries);

                //CodeDocuments = codeDocuments.ToList();
                //NameDocuments = nameDocuments.ToList();

                //// Удаляем последний элемент(;) из списка кодов документов. 
                //CodeDocuments.Remove(CodeDocuments.Last());

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace + "\n" + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Метод получает наименование паспорта по коду документа (номер и серия).
        /// </summary>
        /// <param name="code">Код документа. На вход подаётся исключительно код, который находится в начале строки и подходит только для паспортов.</param>
        /// <returns>Наименование паспорта.</returns>
        private static string GetPasportType(string code)
        {
            code = code.Trim();
            if (Regex.IsMatch(code, @"^\d{2}[\s,-]{0,}\d{2}[\s,-]{0,}\d{6}$"))
            {
                return "Паспорт гражданина РФ";
            }
            else if (Regex.IsMatch(code, @"^(X{0,2}([IXV]|I?(V|X)|(V|X)?I{0,3}))-([A-Z]{2}|[\u0410-\u042F]{2}) \d{6}$"))
            {
                return "Паспорт гражданина СССР";
            }
            else
                return "Паспорт";
        }
    }
}