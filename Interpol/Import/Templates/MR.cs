using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SZI.Import.Mapping;

namespace SZI.Import.Templates
{
    public class MR : Template
    {
        public MR()
        {
            /// Основная таблица
            MainTableRow = new RowItem
            {
                TableName = "public.liczo",
                RowTemplate = new List<ColumnItem>
            {
                //////////////// Из файла ////////////////
                // 1. Фамилия_ru (для сравнения на дубль)
                new ColumnItem
                {
                    ColumnType = DataType.PlainText,
                    FileColumnName = "Фамилия_ru",
                    TableColumnName = "familiya_ru",
                    RawValue = string.Empty,
                    ForCompare = true
                },

                // 2. Имя_ru (для сравнения на дубль)
                new ColumnItem
                {
                    ColumnType = DataType.PlainText,
                    FileColumnName = "Имя_ru",
                    TableColumnName = "imya_ru",
                    RawValue = string.Empty,
                    ForCompare = true
                },

                // 3. Отчество_ru (для сравнения на дубль)
                new ColumnItem
                {
                    ColumnType = DataType.PlainText,
                    FileColumnName = "Отчество_ru",
                    TableColumnName = "otchestvo_ru",
                    RawValue = string.Empty,
                    ForCompare = true
                },

                // 4. Фамилия_en
                new ColumnItem
                {
                    ColumnType = DataType.PlainText,
                    FileColumnName = "Фамилия_en",
                    TableColumnName = "familiya_en",
                    RawValue = string.Empty
                },

                // 5. Имя_en
                new ColumnItem
                {
                    ColumnType = DataType.PlainText,
                    FileColumnName = "Имя_en",
                    TableColumnName = "imya_en",
                    RawValue = string.Empty
                },

                // 6. Отчество_ru (для сравнения на дубль)
                new ColumnItem
                {
                    ColumnType = DataType.PlainText,
                    FileColumnName = "Отчество_en",
                    TableColumnName = "otchestvo_en",
                    RawValue = string.Empty,
                    ForCompare = true
                },

                // 7. Дата рождения (для сравнения на дубль)
                new ColumnItem
                {
                    ColumnType = DataType.Date,
                    FileColumnName = "Дата рождения",
                    TableColumnName = "data_rozhdeniya",
                    RawValue = string.Empty,
                    ForCompare = true
                },

                // 8. Гражданство
                new ColumnItem
                {
                    ColumnType = DataType.Dictionary,
                    DictionaryDType = "Grazhdanstva",
                    FileColumnName = "гражданство",
                    TableColumnName = "grazhdanstvo_id",
                    RawValue = string.Empty
                },

                // 9. Дополнительная информация_en
                new ColumnItem
                {
                    ColumnType = DataType.PlainText,
                    FileColumnName = "Дополнительная информация_en",
                    TableColumnName = "dopolnitelnaya_informacziya_en",
                    RawValue = string.Empty
                },

                // 10. Дополнительная информация_ru
                new ColumnItem
                {
                    ColumnType = DataType.PlainText,
                    FileColumnName = "Дополнительная информация_ru",
                    TableColumnName = "dopolnitelnaya_informacziya_ru",
                    RawValue = string.Empty
                },

                //////////////// С Формулой //////////////////
                // 11. Полное имя ru
                new ColumnItem
                {
                    ColumnType = DataType.FormulaText,
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
                            "Отчество_ru"
                        }
                    }
                },

                // 12. Полное имя en
                new ColumnItem
                {
                    ColumnType = DataType.FormulaText,
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

                // 13. ID инициатора розыска (Генеральный секретариат Интерпола (ГС Интерпол))
                new ColumnItem
                {
                    ColumnType = DataType.Dictionary,
                    FileColumnName = "Источник",
                    DictionaryTableName = "objectsource",
                    DictionaryColumnName = "name_ru",
                    TableColumnName = "objectsource_id",
                    RawValue = string.Empty
                },

                // 14. Список "ИТБ"
                new ColumnItem
                {
                    ColumnType = DataType.IdReference,
                    TableColumnName = "belongs_to_id",
                    RawValue = "2"
                },

                // 15. dtype = "Liczo"
                new ColumnItem
                {
                    ColumnType = DataType.ExactText,
                    TableColumnName = "dtype",
                    RawValue = "Liczo"
                },

                // 16. Категория = "Террористы"
                new ColumnItem
                {
                    ColumnType = DataType.IdReference,
                    TableColumnName = "kategoriya_licza_id",
                    RawValue = "1"
                },

                // 17. objectstate = "PUBLISHED"
                new ColumnItem
                {
                    ColumnType = DataType.ExactText,
                    TableColumnName = "objectstate",
                    RawValue = "PUBLISHED"
                },
            }
            };

            /// Связанные таблицы
            AdditionalTablesRows = new List<RowItem>();

            // basis_for_inclusion (Международный розыск) 
            AdditionalTablesRows.Add(new RowItem
            {
                TableName = "public.basis_for_inclusion",
                RowTemplate = new List<ColumnItem>
                {
                    // 1. target_type
                new ColumnItem
                {
                    ColumnType = DataType.ExactText,
                    TableColumnName = "target_type",
                    RawValue = "liczo"
                },

                // 2. target_id
                new ColumnItem
                {
                    ColumnType = DataType.MainObjectId,
                    TableColumnName = "target_id"
                },

                // 3. dictionary_item
                new ColumnItem
                {
                    ColumnType = DataType.Dictionary,
                    DictionaryDType = "Basis_for_inclusion",
                    FileColumnName = "Основание для включения",
                    TableColumnName = "dictionaryitem_id",
                    RawValue = string.Empty
                }
                }
            });

            // Фотографии
            AdditionalTablesRows.Add(new RowItem
            {
                TableName = "public.media",
                RowTemplate = new List<ColumnItem>
                {
                    // 1. target_type
                new ColumnItem
                {
                    ColumnType = DataType.ExactText,
                    TableColumnName = "target_type",
                    RawValue = "liczo"
                },

                // 2. target_id
                new ColumnItem
                {
                    ColumnType = DataType.MainObjectId,
                    TableColumnName = "target_id"
                },

                new ColumnItem
                {
                    ColumnType = DataType.IdReference,
                    TableColumnName = "media_type",
                    RawValue = "2" // Фотография 
                },

                new ColumnItem
                {
                    ColumnType = DataType.Media,
                    TableColumnName = "src_path",
                },

                new ColumnItem
                {
                    ColumnType = DataType.IdReference,
                    TableColumnName = "weight",
                    RawValue = "1"
                }
                }
            });

            // Связь с организацией 
            AdditionalTablesRows.Add(new RowItem
            {
                TableName = "public.svyaz_liczo_organizacziyarelat",
                RowTemplate = new List<ColumnItem>
                {
                    // 1. source_id
                new ColumnItem
                {
                    ColumnType = DataType.Dictionary,
                    DictionaryTableName = "organizacziya",
                    DictionaryColumnName = "naimenovanie_ru",
                    DictionaryDType = "Organizacziya",
                    FileColumnName = "Связь с организацией",
                    TableColumnName = "source_id",
                    RawValue = string.Empty
                },

                // 2. target_id
                new ColumnItem
                {
                    ColumnType = DataType.MainObjectId,
                    TableColumnName = "target_id"
                },
                }
            });
        }
    }
}
