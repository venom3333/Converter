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
    // Категории для соответствия данных для результирующей таблицы.
    public enum ConvertCategory
    {
        // Без категории.
        NoCategory,
        
        // Значит нужно распарсить полное имя на состовные ( фамилия, имя и отчетсво).
        FullName,

        // Надо распарсить дату рождения на день, месяц и год. 
        BirthDate,

        // Надо распарсить обвинение по статьям, выясняем законы какой страны и подставляем нужный тип. 
        SearchType,

        // Нужно получить адекватный номер code, в соответствии с тем, который получен и записан в таблицу Header.
        Code,

        // Нужно получить адекватный номер num, в соответствии с номером code, который получен и записан в таблицу Header.
        Num,

        // Категория пол.
        Sex, 

        // Наименование файла с фотографией.
        Foto, 

        // Наименование типа документа
        PersonDocumentType,

        // Код документа.
        PersonDocumentData,

        // Проверяем дату на корректность.
        StringDateString,

        // Примечание, записываем сюда все непонятные поля. Известен так же как, в скобках рядом с именем.
        PersonNote,

        // Инициатор розыска
        WantedInitiator
    }
}
