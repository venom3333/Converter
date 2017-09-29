using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SZI.Import.Mapping
{
    public enum DataType
    {
        /// <summary>
        /// Текст напрямую в столбец таблицы Без обработки (конкатенируется со значением из конструктора столбца шаблона).
        /// </summary>
        PlainText,

        /// <summary>
        /// Текст с обработкой через формулу (например конкатинированные значения из других столбцов) (Дефолтное значение отсутстует)
        /// </summary>
        FormulaText,

        /// <summary>
        /// Дата которая должна будет преобразована в int в формате timestamp (Дефолтное значение берется из файла и приводится)
        /// </summary>
        Date,

        /// <summary>
        /// ID, ссылающийся на объект другой таблицы, или просто целое число (В большинстве случаев предопределено для шаблона)
        /// </summary>
        IdReference,

        /// <summary>
        /// Точное значение текста
        /// </summary>
        ExactText,

        /// <summary>
        /// Id импортируемого объекта, присвоенное в основной таблице
        /// </summary>
        MainObjectId,

        /// <summary>
        /// Фотография
        /// </summary>
        Media,

        /// <summary>
        /// Значение из словаря
        /// </summary>
        Dictionary,
    }
}
