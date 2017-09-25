using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpol.ImportSZI
{
    /// <summary>
    /// Мэппинг, название в CSV, название столбца таблицы
    /// </summary>
    class Mapping
    {
        /// <summary>
        /// Название в CSV, название столбца таблицы
        /// </summary>
        Dictionary<string, string> InterpolSZIMap = new Dictionary<string, string>
        {
            { "Фамилия_ru", "familiya_ru" },
            { "Имя_ru", "imya_ru" },
            { "Фамилия_en", "familiya_en" },
            { "Имя_en", "imya_en" },
            { "Дата рождения", "data_rozhdeniya" },
            { "Место рождения", "" }, // хз
            { "Страна инициатор розыска", "" } // хз
        };
    }
}
