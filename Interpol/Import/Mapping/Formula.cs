using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SZI.Import.Mapping
{
    public class Formula
    {
        /// <summary>
        /// Оператор формулы
        /// </summary>
        public FormulaOperator Operator { get; set; }

        /// <summary>
        /// Поля (из импортируемого файла) над значениями которых производится операция
        /// </summary>
        public List<string> Fields { get; set; }

        /// <summary>
        /// Разделитель при конкатенации / сплите
        /// </summary>
        public string[] Separator { get; set; }

        /// <summary>
        /// Дополнительный указатель, где это необходимо (например при операции single, указывает на индекс распарсенной множественной информации, такой как ФИО и т.п.)
        /// </summary>
        int AdditionalPointer { get; set; }
    }
}
