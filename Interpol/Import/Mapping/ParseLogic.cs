using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SZI.Import.Mapping
{
    /// <summary>
    /// Преобразование RawValue в ComputedValue
    /// </summary>
    public static class ParseLogic
    {
        /// <summary>
        /// Парсер даты
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        internal static string ParseDate(string date)
        {
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

            result = dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString();
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
    }
}
