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
    }
}
