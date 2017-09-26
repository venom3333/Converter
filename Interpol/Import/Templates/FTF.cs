using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SZI.Import.Mapping;

namespace SZI.Import.Templates
{
    /// <summary>
    /// Шаблон для файла FTF
    /// </summary>
    public class FTF
    {
        /// <summary>
        /// Список строк (вхождений/лиц)
        /// </summary>
        public List<ColumnItem> TemplateRow { get; set; }

        public FTF()
        {
            TemplateRow = new List<ColumnItem>
            {
                //////////////// Из файла ////////////////
                // 1. Фамилия_ru
                new ColumnItem
                {
                    ColumnType = DataType.PlainText,
                    FileColumnName = "Фамилия_ru",
                    TableName = "liczo",
                    TableColumnName = "familiya_ru",
                    RawValue = string.Empty
                },

                // 2. Имя_ru
                new ColumnItem
                {
                    ColumnType = DataType.PlainText,
                    FileColumnName = "Имя_ru",
                    TableName = "liczo",
                    TableColumnName = "imya_ru",
                    RawValue = string.Empty
                },

                // 3. Фамилия_en
                new ColumnItem
                {
                    ColumnType = DataType.PlainText,
                    FileColumnName = "Фамилия_en",
                    TableName = "liczo",
                    TableColumnName = "familiya_en",
                    RawValue = string.Empty
                },

                // 4. Имя_en
                new ColumnItem
                {
                    ColumnType = DataType.PlainText,
                    FileColumnName = "Имя_en",
                    TableName = "liczo",
                    TableColumnName = "imya_en",
                    RawValue = string.Empty
                },

                // 5. Дата рождения
                new ColumnItem
                {
                    ColumnType = DataType.Date,
                    FileColumnName = "Дата рождения",
                    TableName = "liczo",
                    TableColumnName = "data_rozhdeniya",
                    RawValue = string.Empty
                },

                // 6. Место рождения
                new ColumnItem
                {
                    ColumnType = DataType.PlainText,
                    FileColumnName = "Место рождения",
                    TableName = "liczo",
                    TableColumnName = "mesto_rozhdeniya_text_en",
                    RawValue = string.Empty
                },

                // 7. Страна инициатор розыска
                new ColumnItem
                {
                    ColumnType = DataType.PlainText,
                    FileColumnName = "Место рождения",
                    TableName = "liczo",
                    TableColumnName = "mesto_rozhdeniya_text_en",
                    RawValue = "Search initiator: "
                },

                //////////////// С Формулой //////////////////
                // 8. Полное имя ru
                new ColumnItem
                {
                    ColumnType = DataType.FormulaText,
                    TableName = "liczo",
                    TableColumnName = "fullname_ru",
                    RawValue = string.Empty,
                    ColumnFormula = new Formula
                    {
                        Operator = FormulaOperator.concat,
                        Separator = new string[] {" "},
                        Fields = new List<string>
                        {
                            "Фамилия_ru",
                            "Имя_ru",
                        }
                    }
                },

                // 9. Полное имя en
                new ColumnItem
                {
                    ColumnType = DataType.FormulaText,
                    TableName = "liczo",
                    TableColumnName = "fullname_en",
                    RawValue = string.Empty,
                    ColumnFormula = new Formula
                    {
                        Operator = FormulaOperator.concat,
                        Separator = new string[] {" "},
                        Fields = new List<string>
                        {
                            "Фамилия_en",
                            "Имя_en",
                        }
                    }
                },

                // 10. ID инициатора розыска из словаря (Генеральный секретариат Интерпола (ГС Интерпол))
                new ColumnItem
                {
                    ColumnType = DataType.ExactIdReference,
                    TableName = "liczo",
                    TableColumnName = "objectsource_id",
                    RawValue = "10000004",
                    ComputedValue = "10000004"
                },

                // TODO: Найти где это обозначается!!!
                // 11. "Розыск по линии интерпола"
                new ColumnItem
                {
                    ColumnType = DataType.ExactIdReference,
                    TableName = "liczo",
                }
            };
        }
    }
}
