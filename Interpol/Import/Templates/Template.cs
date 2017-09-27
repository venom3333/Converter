using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SZI.Import.Mapping;

namespace SZI.Import.Templates
{
    /// <summary>
    /// Шаблон для файла Template
    /// </summary>
    public class Template
    {
        /// <summary>
        /// Id созданного при импорте объекта для использования в свазанных таблицах
        /// </summary>
        public int ObjectId { get; set; }

        /// <summary>
        /// Список строк основной таблицы
        /// </summary>
        public RowItem MainTableRow { get; set; }

        /// <summary>
        /// Списки строк связанных таблиц
        /// </summary>
        public List<RowItem> AdditionalTablesRows { get; set; }
    }
}