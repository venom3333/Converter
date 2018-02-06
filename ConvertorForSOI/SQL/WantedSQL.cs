using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ConvertorForSOI.SQLs
{
    /// <summary>
    /// Класс для работы с базой данных (Розыск).
    /// </summary>
    public static class WantedSQL
    {
        // Запрос для добавления дополнительных типов розыска.
        private static string InsertExtraTypeSearch = @"INSERT INTO NAK_WANTEDTYPESEARCH
SELECT     dbo.NAK_WANTED.A_ID AS A_FROMID, dbo.NAK_SEARCHTYPE.A_ID AS A_TOID
FROM         dbo.NAK_TEMP_WANTEDTYPESEARCH INNER JOIN
                      dbo.NAK_SEARCHTYPE ON dbo.NAK_TEMP_WANTEDTYPESEARCH.searchType = dbo.NAK_SEARCHTYPE.A_NAME INNER JOIN
                      dbo.NAK_PERSON ON dbo.NAK_TEMP_WANTEDTYPESEARCH.Code = dbo.NAK_PERSON.A_CODE INNER JOIN
                      dbo.NAK_WANTED ON dbo.NAK_PERSON.A_ID = dbo.NAK_WANTED.A_PERSON
";

        // Запрос для добавления фотографий в таблицу NAK_PERSONPHOTO.
        private static string InsertPersonPhoto = @"INSERT INTO NAK_PERSONPHOTO
SELECT A.person, A.photo, null, null, 0, null, GETDATE(), NEWID(), 1, null
FROM 
(
SELECT p.A_ID as person, ph.A_ID as photo 
FROM NAK_PERSON p INNER JOIN 
NAK_TEMP_PHOTO tph on p.A_CODE = tph.CODE INNER JOIN NAK_PHOTO ph on ph.A_FILENAME = tph.[FILENAME]
where p.a_id in 
	(
		select  max(p1.a_id)  from Nak_person as p1 inner join 
		NAK_TEMP_WANTED AS w1 on p1.a_code = w1.code
		group by p1.a_code
	)
UNION
SELECT p.A_ID as person, ph.A_ID as photo 
FROM NAK_PERSON p INNER JOIN NAK_TEMP_WANTED w ON p.A_CODE = w.code  
INNER JOIN NAK_PHOTO ph on ph.A_FILENAME = w.PHOTOLIST
where p.a_id in 
	(
		select  max(p1.a_id)  from Nak_person as p1 inner join 
		NAK_TEMP_WANTED AS w1 on p1.a_code = w1.code
		group by p1.a_code
	)
EXCEPT 
select pph.A_PERSON, pph.A_PHOTO from  NAK_PERSONPHOTO pph
) A";

        // Запрос для обновления лица, показывающего, что у лица есть фото.
        private static string UpdatePersonIsPhoto = @"UPDATE [nak_data].[dbo].[NAK_PERSON]
SET A_ISPHOTO = 1
FROM NAK_PERSON p INNER JOIN NAK_PERSONPHOTO pph on p.A_ID = pph.A_PERSON
WHERE p.A_ISPHOTO = 0";

        // Запрос для обновления лица, из дополнительной таблицы NAK_TEMP_WANTED_ADDED.
        private static string UpdatePersonAdded = @"update dbo.nak_person
set dbo.nak_person.a_note = wa.personnote,
    dbo.nak_person.a_note_en = wa.personnote_en
from nak_temp_wanted_added as wa inner join nak_person p on p.a_code = wa.code
where wa.personnote is not null and p.a_id in (
select  max(p1.a_id)  from Nak_person as p1 inner join 
NAK_TEMP_WANTED AS w1 on p1.a_code = w1.code
group by p1.a_code
)";

        /// <summary>
        /// Метод загружает дополнительные типы розыска в базу. 
        /// </summary>
        /// <returns>True в случае успеха. False в случае ошибки загрузки.</returns>
        public static bool FillExtraWTS()
        {
            try
            {
                // Делаем подключение к базе данных.
                using (SqlConnection conn = new SqlConnection(Config.ConnectionStringNakData))
                {
                    conn.Open();
                    // Вызываем команду которая добавляет дополнительные типы розыска.
                    var sqlCommand = new SqlCommand(InsertExtraTypeSearch, conn)
                    {
                        CommandTimeout = 0
                    };
                    sqlCommand.ExecuteNonQuery();

                    // Закрываем подключение к базе.
                    sqlCommand.Dispose();
                    conn.Close();
                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace + "\n" + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Метод подключает фото к лицам и записывает дополнительные данные в базу. 
        /// </summary>
        /// <returns>True в случае успеха. False в случае ошибки загрузки.</returns>
        public static bool FillPhotosAndAdded()
        {
            try
            {
                // Делаем подключение к базе данных.
                using (SqlConnection conn = new SqlConnection(Config.ConnectionStringNakData))
                {
                    conn.Open();

                    var sqlCommand = new SqlCommand
                    {
                        CommandTimeout = 0
                    };

                    // Вызываем команду, которая добавляет фотографии в таблицу NAK_PERSONPHOTO, что бы фотографии отображались в карточках лиц. (Сам Sitex по какой-то причине не пристыковывает их к лицам)
                    sqlCommand = new SqlCommand(InsertPersonPhoto, conn)
                    {
                        CommandTimeout = 0
                    };
                    sqlCommand.ExecuteNonQuery();

                    // Обновляем таблицу NAK_PERSON поле a_IsPhoto, записываем туда 1, показывающее, что у лица есть фотографии. (Сам Sitex по какой-то причине не включает флаг)
                    sqlCommand = new SqlCommand(UpdatePersonIsPhoto, conn)
                    {
                        CommandTimeout = 0
                    };
                    sqlCommand.ExecuteNonQuery();

                    // Обновляем таблицу NAK_PERSON данными из дополнительной таблицы NAK_TEMP_WANTED_ADDED.
                    sqlCommand = new SqlCommand(UpdatePersonAdded, conn)
                    {
                        CommandTimeout = 0
                    };
                    sqlCommand.ExecuteNonQuery();

                    // Закрываем подключение к базе.
                    sqlCommand.Dispose();
                    conn.Close();
                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace + "\n" + e.Message);
                return false;
            }
        }

        // Проверяет существует ли Person в БД
        public static bool PersonExists(string lastName = "", string firstName = "", string secondName1 = "", int monthBirthDate = 0, int yearBirthDate = 0, int dayBirthDate = 0)
        {
            var sql = string.Empty;

            sql += $@"SELECT COUNT(A_ID) FROM [nak_data].[dbo].[NAK_ADJUSTINGDATA]
                    WHERE LTRIM(RTRIM(Upper(A_LASTNAME))) = LTRIM(RTRIM(Upper('{lastName}')))";

            if (!string.IsNullOrWhiteSpace(firstName))
            {
                sql += $@"AND LTRIM(RTRIM(Upper(A_FIRSTNAME))) = LTRIM(RTRIM(Upper('{firstName}')))";
            }

            if (!string.IsNullOrWhiteSpace(secondName1))
            {
                sql += $@"AND LTRIM(RTRIM(Upper(A_SECONDNAME1))) = LTRIM(RTRIM(Upper('{secondName1}')))";
            }

            if (monthBirthDate != 0)
            {
                sql += $@"AND LTRIM(RTRIM(Upper(A_MONTHBIRTHDATE))) = LTRIM(RTRIM(Upper('{monthBirthDate}')))";
            }

            if (yearBirthDate != 0)
            {
                sql += $@"AND LTRIM(RTRIM(Upper(A_YEARBIRTHDATE))) = LTRIM(RTRIM(Upper('{yearBirthDate}')))";
            }

            if (dayBirthDate != 0)
            {
                sql += $@"AND LTRIM(RTRIM(Upper(A_DAYBIRTHDATE))) = LTRIM(RTRIM(Upper('{dayBirthDate}')))";
            }

            int person = 0;

            // Соединиться с БД
            using (SqlConnection conn = new SqlConnection(Config.ConnectionStringNakData))
            {
                var sqlCommand = new SqlCommand(sql, conn);
                conn.Open();
                SqlDataReader reader = sqlCommand.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        person = (int)reader[0];
                    }
                }
                finally
                {
                    // Always call Close when done reading.
                    reader.Close();
                    // Закрыть соединение
                    sqlCommand.Dispose();
                    conn.Close();
                }
            }

            // возврат
            return person > 0;
        }

        internal static bool AddTranslateInfo(int personId, int wantedID, DataRow newRow, DataRow addedRow)
        {

            //Находим personId по wantedID
            if (personId == 0 && wantedID > 0)
            {
                personId = 0;

                string adjustingIdQuery = $@"
SELECT p.A_ID FROM NAK_PERSON AS p
INNER JOIN NAK_WANTED AS w ON w.A_PERSON = p.A_ID
WHERE w.A_ID = {wantedID}";
                // Соединиться с БД
                using (SqlConnection conn = new SqlConnection(Config.ConnectionStringNakData))
                {
                    conn.Open();

                    using (var sqlCommand = new SqlCommand(adjustingIdQuery, conn))
                    {
                        var obj = sqlCommand.ExecuteScalar();
                        if (obj != null)
                        {
                            personId = (int)obj;
                        }
                    }
                }
            }
            // 8 - lastName_en (NAK_ADJUSTINGDATA.A_LASTNAME_EN)
            // 9 - firstName_en (NAK_ADJUSTINGDATA.A_FIRSTNAME_EN)
            // 10 - secondName1_en (NAK_ADJUSTINGDATA.A_SECONDNAME1_EN)
            // 13 - birthplace_en   (NAK_PERSON.A_BIRTHPLACE_EN)
            // addedrow 2 - notes (NAK_PERSON.A_NOTE_EN)
            // 19 - accusation_en (NAK_WANTED.A_ACCUSATION_EN)

            string lastName_en = newRow.ItemArray[8]?.ToString().Replace("'", "''");
            string firstName_en = newRow.ItemArray[9]?.ToString().Replace("'", "''");
            string secondName_en = newRow.ItemArray[10]?.ToString().Replace("'", "''");
            string birthplace_en = newRow.ItemArray[13]?.ToString().Replace("'", "''");
            string notes_en = addedRow.ItemArray[2]?.ToString().Replace("'", "''");
            string accusation_en = newRow.ItemArray[19]?.ToString().Replace("'", "''");

            string mainFullName_en = $"{lastName_en} {firstName_en} {secondName_en}";

            StringBuilder addTranslationQuery = new StringBuilder();

            // Имя, фамилия, отчество (NAK_ADJUSTINGDATA)
            if (!string.IsNullOrWhiteSpace(lastName_en) || !string.IsNullOrWhiteSpace(firstName_en) || !string.IsNullOrWhiteSpace(secondName_en))
            {
                addTranslationQuery.Append($@"
UPDATE NAK_ADJUSTINGDATA
    SET");
                // Фамилия
                if (!string.IsNullOrWhiteSpace(lastName_en))
                {
                    addTranslationQuery.Append($@"
        A_LASTNAME_EN = '{lastName_en}',");
                }

                // Имя
                if (!string.IsNullOrWhiteSpace(firstName_en))
                {
                    addTranslationQuery.Append($@"
        A_FIRSTNAME_EN = '{firstName_en}',");
                }

                // Отчество
                if (!string.IsNullOrWhiteSpace(secondName_en))
                {
                    addTranslationQuery.Append($@"
        A_SECONDNAME1_EN = '{secondName_en}',");
                }

                // Если осталась висячая запятая
                addTranslationQuery.Replace(",", string.Empty, addTranslationQuery.Length - 2, addTranslationQuery.Length - (addTranslationQuery.Length - 2));

                addTranslationQuery.Append($@"
    WHERE A_PERSON = {personId}");

                addTranslationQuery.Append(";");
            }

            // NAK_PERSON
            if (!string.IsNullOrWhiteSpace(birthplace_en) || !string.IsNullOrWhiteSpace(notes_en) || !string.IsNullOrWhiteSpace(mainFullName_en))
            {
                addTranslationQuery.Append($@"
UPDATE NAK_PERSON
    SET");
                // Место рождения
                if (!string.IsNullOrWhiteSpace(birthplace_en))
                {
                    addTranslationQuery.Append($@"
        A_BIRTHPLACE_EN = '{birthplace_en}',");
                }

                // Доп инфо
                if (!string.IsNullOrWhiteSpace(notes_en))
                {
                    addTranslationQuery.Append($@"
        A_NOTE_EN = '{notes_en}',");
                }

                // Полное имя
                if (!string.IsNullOrWhiteSpace(mainFullName_en))
                {
                    addTranslationQuery.Append($@"
        A_MAINFULLNAME_EN = '{mainFullName_en}',");
                }

                // Если осталась висячая запятая
                addTranslationQuery.Replace(",", string.Empty, addTranslationQuery.Length - 2, addTranslationQuery.Length - (addTranslationQuery.Length - 2));

                addTranslationQuery.Append($@"
    WHERE A_ID = {personId}");

                addTranslationQuery.Append(";");
            }

            // NAK_WANTED
            if (!string.IsNullOrWhiteSpace(accusation_en))
            {
                addTranslationQuery.Append($@"
UPDATE NAK_WANTED
    SET");
                // Основания розыска
                if (!string.IsNullOrWhiteSpace(accusation_en))
                {
                    addTranslationQuery.Append($@"
        A_ACCUSATION_EN = '{accusation_en}',");
                }

                // Если осталась висячая запятая
                addTranslationQuery.Replace(",", string.Empty, addTranslationQuery.Length - 2, addTranslationQuery.Length - (addTranslationQuery.Length - 2));

                addTranslationQuery.Append($@"
    WHERE A_PERSON = {personId}");

                addTranslationQuery.Append(";");
            }

            // Соединиться с БД
            using (SqlConnection conn = new SqlConnection(Config.ConnectionStringNakData))
            {
                conn.Open();

                using (var sqlCommand = new SqlCommand(addTranslationQuery.ToString(), conn))
                {
                    try
                    {
                        sqlCommand.ExecuteNonQuery();
                        return true;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.StackTrace + "\n" + e.Message);
                        return false;
                    }
                }
            }

        }

        // Возвращает A_ID таблицы NAK_WANTED по информации из NAK_AJUSTINGDATA
        public static int GetWantedID(out int personId, string lastName = "", string firstName = "", string secondName1 = "", int monthBirthDate = 0, int yearBirthDate = 0, int dayBirthDate = 0)
        {
            personId = 0;
            string sql = String.Format(@"
SELECT wanted.A_ID as wantedId, person.A_ID as personId
FROM dbo.NAK_PERSON AS person
INNER JOIN dbo.NAK_ADJUSTINGDATA AS adj
ON person.A_ID = adj.A_PERSON
INNER JOIN dbo.NAK_WANTED as wanted
ON person.A_ID = wanted.A_PERSON
WHERE (UPPER(adj.A_LASTNAME) = '{0}')
AND (UPPER(adj.A_FIRSTNAME) = '{1}')
AND (UPPER(adj.A_SECONDNAME1) = '{2}')
AND (adj.A_MONTHBIRTHDATE = '{3}')
AND (adj.A_YEARBIRTHDATE = '{4}')
AND (adj.A_DAYBIRTHDATE = '{5}');",
        lastName, firstName, secondName1, monthBirthDate, yearBirthDate, dayBirthDate);
            int wantedID = 0;

            // Соединиться с БД
            using (SqlConnection conn = new SqlConnection(Config.ConnectionStringNakData))
            {
                var sqlCommand = new SqlCommand(sql, conn);
                conn.Open();
                SqlDataReader reader = sqlCommand.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        //MessageBox.Show(String.Format("{0}", reader[0]));
                        wantedID = (int)reader[0];
                        personId = (int)reader[1];
                    }
                }
                finally
                {
                    // Always call Close when done reading.
                    reader.Close();
                    // Закрыть соединение
                    sqlCommand.Dispose();
                    conn.Close();
                }
            }

            // возврат
            return wantedID == 0 ? 0 : wantedID;
        }

        // Возвращает A_ID таблицы NAK_SEARCHTYPE по названию типа розыска
        public static int GetSearchTypeID(string searchType = "")
        {
            string sql = String.Format(@"
SELECT A_ID
FROM dbo.NAK_SEARCHTYPE
WHERE (UPPER(A_NAME) = '{0}');",
        searchType);

            int searchTypeID = 0;

            // Соединиться с БД
            using (SqlConnection conn = new SqlConnection(Config.ConnectionStringNakData))
            {
                var sqlCommand = new SqlCommand(sql, conn);
                conn.Open();
                SqlDataReader reader = sqlCommand.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        searchTypeID = (int)reader[0];
                    }
                }
                finally
                {
                    // Always call Close when done reading.
                    reader.Close();
                    // Закрыть соединение
                    sqlCommand.Dispose();
                    conn.Close();
                }
            }

            // возврат
            return searchTypeID == 0 ? 0 : searchTypeID;
        }

        // Проверяем существует ли связка в таблице NAK_WANTEDTYPESEARCH и если нет, добавляем
        public static bool AddExtraWantedTypeSearch(int wantedID, int searchTypeID, string accusation)
        {
            // Существует ли связка в таблице
            string sqlSelect = String.Format(@"
SELECT count(A_FROMID)
FROM dbo.NAK_WANTEDTYPESEARCH
WHERE A_FROMID = '{0}'
AND A_TOID = '{1}';",
        wantedID, searchTypeID);

            // Добавление связки
            string sqlInsert = String.Format(@"
INSERT INTO dbo.NAK_WANTEDTYPESEARCH
VALUES ({0}, {1});",
        wantedID, searchTypeID);

            // Добавление оснований розыска
            string sqlAddAccusation = String.Format(@"
UPDATE dbo.NAK_WANTED
SET A_ACCUSATION = CAST(A_ACCUSATION AS nvarchar(max)) + ' ' + CAST('{0}' as nvarchar(max))
WHERE A_ID = {1};",
        accusation, wantedID);

            int result = 0;

            // Соединиться с БД
            using (SqlConnection conn = new SqlConnection(Config.ConnectionStringNakData))
            {
                SqlCommand sqlCommand = new SqlCommand(sqlSelect, conn);
                conn.Open();
                SqlDataReader reader = sqlCommand.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        result = (int)reader[0];
                    }
                }
                finally
                {
                    // Always call Close when done reading.
                    reader.Close();
                    // Закрыть соединение
                    sqlCommand.Dispose();
                    conn.Close();
                }
                if (result > 0)
                {
                    return false;
                }
                else
                {
                    conn.Open();

                    sqlCommand = new SqlCommand(sqlAddAccusation, conn);
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand = new SqlCommand(sqlInsert, conn);
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                    conn.Close();
                    return true;
                }
            }

        }

        // Очищает РЕАЛЬНЫЕ таблицы РОЗЫСКА от данных.
        public static bool TruncateReal()
        {
            try
            {
                string truncateREAL = @"TRUNCATE TABLE [nak_data].[dbo].[NAK_ADJUSTINGDATA];
                                        TRUNCATE TABLE [nak_data].[dbo].[NAK_PERSON];
                                        TRUNCATE TABLE [nak_data].[dbo].[NAK_PERSONPHOTO];
                                        TRUNCATE TABLE [nak_data].[dbo].[NAK_PERSONDOCUMENT];
                                        TRUNCATE TABLE [nak_data].[dbo].[NAK_PHOTO];
                                        TRUNCATE TABLE [nak_data].[dbo].[NAK_WANTED];
                                        TRUNCATE TABLE [nak_data].[dbo].[NAK_WANTEDTYPESEARCH];
                                       ";

                SqlConnection conn = new SqlConnection(Config.ConnectionStringNakData);
                conn.Open();
                SqlCommand sqlTruncate = new SqlCommand(truncateREAL, conn);
                sqlTruncate.ExecuteNonQuery();
                sqlTruncate.Dispose();
                conn.Close();
                MessageBox.Show("ПЕРВИЧНЫЕ таблицы очищены!");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при очистке ПЕРВИЧНЫХ таблиц!\n" + ex.Message);
                return false;
            }
        }

        public static bool FillInitiatorsTable(string initiator)
        {
            try
            {
                // Делаем подключение к базе данных.
                using (SqlConnection conn = new SqlConnection(Config.ConnectionStringNakData))
                {
                    conn.Open();
                    SqlCommand sqlCommand = new SqlCommand();

                    sqlCommand = new SqlCommand(String.Format("INSERT INTO NAK_TEMP_WANTED_INIT VALUES('{0}', '')", initiator), conn);
                    sqlCommand.ExecuteNonQuery();

                    // Закрываем подключение к базе.
                    sqlCommand.Dispose();
                    conn.Close();
                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace + "\n" + e.Message);
                return false;
            }
        }

        // Проверяем существует ли инициатор розыска в базе (NAK_WANTEDINITIATOR), сверяем со словарем программы (WantedInitiatorsMap)
        public static bool InitiatorExists(string initiator)
        {
            string sqlSelect = String.Format(@"
SELECT count(A_ID)
FROM dbo.NAK_WANTEDINITIATOR
WHERE A_NAME = '{0}'
;",
        initiator);


            int result = 0;

            // Соединиться с БД
            using (SqlConnection conn = new SqlConnection(Config.ConnectionStringNakData))
            {
                SqlCommand sqlCommand = new SqlCommand(sqlSelect, conn);
                conn.Open();
                SqlDataReader reader = sqlCommand.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        result = (int)reader[0];
                    }
                }
                finally
                {
                    // Always call Close when done reading.
                    reader.Close();
                    // Закрыть соединение
                    sqlCommand.Dispose();
                    conn.Close();
                }
                if (result > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        // Проверяем существует ли тип розыска в базе (NAK_SEARCHTYPE), сверяем со словарем программы (missedSearchTypesDB)
        public static bool SearchTypeExists(string searchType)
        {
            string sqlSelect = String.Format(@"
SELECT count(A_ID)
FROM dbo.NAK_SEARCHTYPE
WHERE A_NAME = '{0}'
;",
        searchType);


            int result = 0;

            // Соединиться с БД
            using (SqlConnection conn = new SqlConnection(Config.ConnectionStringNakData))
            {
                var sqlCommand = new SqlCommand(sqlSelect, conn);
                conn.Open();
                SqlDataReader reader = sqlCommand.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        result = (int)reader[0];
                    }
                }
                finally
                {
                    // Always call Close when done reading.
                    reader.Close();
                    // Закрыть соединение
                    sqlCommand.Dispose();
                    conn.Close();
                }
                return result > 0;
            }
        }

        // Добавляем информацию о снятии с розыска + обновление полей TS в NAK_WANTED и NAK_PERSON
        public static bool AddSearchEndInfo(int wantedID, BirthDate endDate, string endNote, string endNote_en)
        {
            int yearEndDate = int.Parse(endDate.BirthDates[2]);
            int monthEndDate = int.Parse(endDate.BirthDates[3]);
            int dayEndDate = int.Parse(endDate.BirthDates[0]);

            string addSearchEndInfo = String.Format(@"
UPDATE dbo.NAK_WANTED
SET A_YEARENDDATE = {0},
    A_MONTHENDDATE = {1},
    A_DAYENDDATE = {2},
    A_NOTE = '{3}',
    A_NOTE_EN = '{4}',
    TS = getdate()
WHERE A_ID = {5};
UPDATE dbo.NAK_PERSON
SET TS = getdate()
WHERE A_ID = (SELECT A_PERSON
                FROM dbo.NAK_WANTED
                WHERE A_ID = {5})",
        yearEndDate, monthEndDate, dayEndDate, endNote, endNote_en, wantedID);

            // Соединиться с БД
            using (SqlConnection conn = new SqlConnection(Config.ConnectionStringNakData))
            {
                conn.Open();

                var sqlCommand = new SqlCommand(addSearchEndInfo, conn);

                try
                {
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                    conn.Close();
                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.StackTrace + "\n" + e.Message);
                    return false;
                }
            }

        }
    }
}