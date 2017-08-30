using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.RegularExpressions;

namespace ConvertorForSOI
{
    // Класс для соответствия данных колонки результирующей таблицы и таблицы источника.
    public class MapConvertItem
    {
        // Наименование колонки результирующей таблицы.
        public string ColumnName;

        // Индекс колонки результирующей таблицы, индекс начинается с 0, в соответствии с колонками DataTable.
        public int ResultTableIndex;

        // Индекс колонки таблицы источника данных, то есть беруться данные из этой колонки и переностся в таблицу результата.
        // Если данные не берутся и в результирующую таблицу ничего не переносится, то -1.
        public int SourceTableIndex;

        // Категория данных, которые переносятся из одной таблицы в другую. Если данные просто переносятся, то NoCategory, если их нужно парсить, преобразовывать, то действия делаются в соответствии с этой категорией.
        public ConvertCategory Category;

        // Если была выбрана категория, не NoCategory, то номер показывает какая именно часть распарсенных данных переносится в колонку. 
        public int NumberCategory; 
    }
}
