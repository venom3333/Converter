using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SZI.Import.Mapping
{
    /// <summary>
    /// Представляет собой одну колонку от записи
    /// </summary>
    public class ColumnItem
    {
        /// <summary>
        /// Название колонки в файле
        /// </summary>
        public string FileColumnName { get; set; }

        /// <summary>
        /// Название столбца таблицы
        /// </summary>
        public string TableColumnName { get; set; }

        /// <summary>
        /// Тип данных (enum DataType)
        /// </summary>
        public DataType ColumnType { get; set; }

        /// <summary>
        /// Если ColumnType = FormulaText, то используем для конкретизации
        /// </summary>
        public Formula ColumnFormula { get; set; }

        /// <summary>
        /// Значение до обработки
        /// </summary>
        public string RawValue { get; set; }

        /// <summary>
        /// Конечное значение, которое будет записано в БД
        /// </summary>
        public string ComputedValue { get; set; }

        /// <summary>
        /// Текущая строка (при необходимости)
        /// </summary>
        public RowItem CurentRow { get; set; }

        /// <summary>
        /// Для шаблона колонки
        /// </summary>
        public ColumnItem() { }

        /// <summary>
        /// Для значений из файла
        /// </summary>
        /// <param name="columnTemplate"></param>
        /// <param name="rawValue"></param>
        public ColumnItem(ColumnItem columnTemplate, string rawValue)
        {
            FileColumnName = columnTemplate.FileColumnName;
            ColumnType = columnTemplate.ColumnType;
            ColumnFormula = columnTemplate.ColumnFormula;
            TableColumnName = columnTemplate.TableColumnName;
            RawValue = rawValue.Trim();

            ComputeValue();
        }

        /// <summary>
        /// Для значений из шаблона текста с формулой
        /// </summary>
        /// <param name="columnTemplate"></param>
        /// <param name="rawValue"></param>
        public ColumnItem(ColumnItem columnTemplate, RowItem currentRow)
        {
            FileColumnName = columnTemplate.FileColumnName;
            ColumnType = columnTemplate.ColumnType;
            ColumnFormula = columnTemplate.ColumnFormula;
            TableColumnName = columnTemplate.TableColumnName;

            if (columnTemplate.RawValue != null)
            {
                RawValue = columnTemplate.RawValue.Trim();
            }

            CurentRow = currentRow;

            ComputeValue();
        }

        /// <summary>
        /// Вычисляет конечное значение
        /// </summary>
        private void ComputeValue()
        {
            switch (ColumnType)
            {
                case DataType.Date:
                    ComputedValue = ParseLogic.ParseDate(RawValue);
                    break;
                case DataType.FormulaText:
                    ComputedValue = ParseLogic.ParseFormulaText(this);
                    break;
                case DataType.MainObjectId:
                    break;

                case DataType.ExactText:
                case DataType.IdReference:
                case DataType.PlainText:
                    ComputedValue = RawValue;
                    break;
                default:
                    throw new NotImplementedException("Тип колонки не распознан!");
            }
        }
    }
}
