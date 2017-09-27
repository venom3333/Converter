using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SZI.Import.Mapping
{
    public class ImportData
    {
        /// <summary>
        /// Заполненные данные для основной таблицы
        /// </summary>
        public List<RowItem> MainTable { get; set; }

        /// <summary>
        /// Заполненные данные для доп. таблиц
        /// </summary>
        public List<List<RowItem>> AdditionalTables { get; set; }
    }
}
