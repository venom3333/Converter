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
    // Класс с картой соответствий и другими данными для конкретного источника данных.
    public class MapSourceItem
    {
        // Тип документа. Даже если документ одного расширения, он может быть другого типа.
        public enum TypeDocument { Form2, Form3, Form4, Xls, DocCards, FormHz, NoType };

        public int RowNumber; // Номер ряда с которого начинаются данные, начиная с 1.
        public string Extention; // Расширение файла (например .xls).
        public TypeDocument TypeDoc; // Тип документа.
        public List<MapConvertItem> MapConvertList; // Карта преобразований из таблицы источника данных, в таблицу результат данных.

        /// <summary>
        /// Метод получает тип перечисления TypeDocument по строке.
        /// </summary>
        /// <param name="strTypeDocument">Строка с типом документа.</param>
        /// <returns>Тип перечисления TypeDocument.</returns>
        public static TypeDocument GetTypeDocumentByString(string strTypeDocument)
        {
            if (strTypeDocument == "Form2")
                return TypeDocument.Form2;
            else if (strTypeDocument == "Form3")
                return TypeDocument.Form3;
            else if (strTypeDocument == "Form4")
                return TypeDocument.Form4;
            else if (strTypeDocument == "DocCards")
                return TypeDocument.DocCards;
            else if (strTypeDocument == "FormHz")
                return TypeDocument.FormHz;
            else if (strTypeDocument == "Xls")
                return TypeDocument.Xls;
            else if (strTypeDocument == "NoType")
                return TypeDocument.NoType;
            else
                return TypeDocument.NoType;
        }

        /// <summary>
        /// Метод парсит шапку ворд файла и определяет какой формы документ.
        /// </summary>
        /// <param name="strHeader">Текст с шапкой документа.</param>
        /// <returns>Возвращаем тип документа. Если тип не найден, то возвращаем NoType.</returns>
        public static TypeDocument ParceHeader(string strHeader)
        {
            if (string.IsNullOrWhiteSpace(strHeader) || strHeader.Length < 7)
                return MapSourceItem.TypeDocument.NoType;
            try
            {
                if (strHeader.Contains("Форма 2"))
                    return MapSourceItem.TypeDocument.Form2;
                else if (strHeader.Contains("Форма 3"))
                    return MapSourceItem.TypeDocument.Form3;
                else if (strHeader.Contains("Форма 4"))
                    return MapSourceItem.TypeDocument.Form4;
                else if (strHeader.Contains("DocКарточки"))
                    return MapSourceItem.TypeDocument.DocCards;
                else if (strHeader.Contains("Форма Hz"))
                    return MapSourceItem.TypeDocument.FormHz;
                else
                    return MapSourceItem.TypeDocument.NoType;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return MapSourceItem.TypeDocument.NoType;
            }
        }
    }
}
