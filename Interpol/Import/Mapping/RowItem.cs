using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SZI.Import.Mapping
{
    public class RowItem
    {
        /// <summary>
        /// Наименование таблицы
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Список строк
        /// </summary>
        public List<ColumnItem> RowTemplate { get; set; }
    }
}
