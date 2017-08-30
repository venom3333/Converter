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
    /// Класс для работы с базой данных (Пропавшие без вести).
    /// </summary>
    public static class MissingSQL
    {

        // Проверяет существует ли человек в таблице NAK_ADJUST_MISSING в БД
        public static bool PersonExists(string lastName = "", string firstName = "", string secondName1 = "", int monthBirthDate = 0, int yearBirthDate = 0, int dayBirthDate = 0)
        {
            string sql = String.Format(@"SELECT COUNT(A_ID) FROM [nak_data].[dbo].[NAK_ADJUST_MISSING]
                                         WHERE Upper(A_LASTNAME) = '{0}'
                                         AND Upper(A_FIRSTNAME) = '{1}'
                                         AND Upper(A_SECONDNAME1) = '{2}'
                                         AND A_MONTHBIRTHDATE = '{3}'
                                         AND A_YEARBIRTHDATE = '{4}'
                                         AND A_DAYBIRTHDATE = '{5}';",
                                         lastName, firstName, secondName1, monthBirthDate, yearBirthDate, dayBirthDate);
            int person = 0;

            // Соединиться с БД
            using (SqlConnection conn = new SqlConnection(Config.ConnectionStringNakData))
            {
                SqlCommand sqlCommand = new SqlCommand(sql, conn);
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
            return person > 0 ? true : false;
        }

        // Возвращает A_ID таблицы NAK_WANTED по информации из NAK_AJUSTINGDATA
        public static int GetMissingSearchID(string lastName = "", string firstName = "", string secondName1 = "", int monthBirthDate = 0, int yearBirthDate = 0, int dayBirthDate = 0)
        {
            string sql = String.Format(@"
SELECT     s.A_ID
FROM         dbo.NAK_ADJUST_MISSING mis
INNER JOIN 
                      dbo.NAK_MISSING a ON mis.A_ID = a.A_MAINADJUSTINGDATA INNER JOIN
                      dbo.NAK_MISSING_SEARCH s ON a.A_ID = s.A_MISSING
WHERE (UPPER(mis.A_LASTNAME) = '{0}')
AND (UPPER(mis.A_FIRSTNAME) = '{1}')
AND (UPPER(mis.A_SECONDNAME1) = '{2}')
AND (mis.A_MONTHBIRTHDATE = '{3}')
AND (mis.A_YEARBIRTHDATE = '{4}')
AND (mis.A_DAYBIRTHDATE = '{5}');",
lastName, firstName, secondName1, monthBirthDate, yearBirthDate, dayBirthDate);
            int wantedID = 0;

            // Соединиться с БД
            using (SqlConnection conn = new SqlConnection(Config.ConnectionStringNakData))
            {
                SqlCommand sqlCommand = new SqlCommand(sql, conn);
                conn.Open();
                SqlDataReader reader = sqlCommand.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        //MessageBox.Show(String.Format("{0}", reader[0]));
                        wantedID = (int)reader[0];
                        //person[1] = (int)reader[1];
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

        // Добавляем информацию о снятии с розыска + обновление полей TS в NAK_WANTED и NAK_PERSON
        public static bool AddSearchEndInfo(int missingSearchID, BirthDate endDate, string endNote)
        {
            int yearEndDate = int.Parse(endDate.BirthDates[2]);
            int monthEndDate = int.Parse(endDate.BirthDates[3]);
            int dayEndDate = int.Parse(endDate.BirthDates[0]);

            string addSearchEndInfo = String.Format(@"
UPDATE dbo.NAK_MISSING_SEARCH
SET A_YEARENDDATE = {0},
    A_MONTHENDDATE = {1},
    A_DAYENDDATE = {2},
    A_NOTE = '{3}',
    TS = getdate()
WHERE A_ID = {4};
UPDATE dbo.NAK_MISSING
SET TS = getdate()
WHERE A_ID = (  SELECT A_MISSING
                FROM dbo.NAK_MISSING_SEARCH
                WHERE A_ID = {4})",
        yearEndDate, monthEndDate, dayEndDate, endNote, missingSearchID);

            // Соединиться с БД
            using (SqlConnection conn = new SqlConnection(Config.ConnectionStringNakData))
            {
                conn.Open();

                SqlCommand sqlCommand = new SqlCommand(addSearchEndInfo, conn);

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