using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SZI.Import.Mapping
{
    public enum FormulaOperator
    {
        /// <summary>
        /// Конкатенация
        /// </summary>
        concat,

        /// <summary>
        /// Взятие одиночного значения
        /// </summary>
        single,

        /// <summary>
        /// Точное соответствие из конструктора
        /// </summary>
        exact
    }
}
