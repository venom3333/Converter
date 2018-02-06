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
    // Класс для даты рождения, которая распарсивается на составляющие.
    public class BirthDate
    {
        public Dictionary<int, string> BirthDates = new Dictionary<int, string>()
        {
            {0, ""}, // День, даты рождения.
            {1, ""}, // Месяц, даты рождения.
            {2, ""},  // Год, даты рождения.
            {3, ""}  // Номер месяца.
        };

        public DateTime Date { get; set; }

        public BirthDate(string birthDate)
        {
            TryParse(birthDate);
        }

        public bool TryParse(string birthDate)
        {
            try
            {
                // Для получения названия месяца в виде строки по русски.
                CultureInfo ci = null;
                DateTimeFormatInfo dtfi = null;
                ci = new System.Globalization.CultureInfo("ru-RU");
                dtfi = ci.DateTimeFormat;

                Regex rgxBefore = new Regex("^([^\\d]+)");
                Regex rgxAfter = new Regex("([^\\d]+)$");
                birthDate = rgxBefore.Replace(birthDate, "").Trim('.', ' ');
                birthDate = rgxAfter.Replace(birthDate, "").Trim('.', ' ');
                

                DateTime dtBirthDate;
                if (DateTime.TryParse(birthDate, out dtBirthDate))
                {
                    BirthDates[0] = dtBirthDate.Day.ToString();
                    BirthDates[1] = dtfi.GetMonthName(dtBirthDate.Month);
                    BirthDates[2] = dtBirthDate.Year.ToString();
                    BirthDates[3] = dtBirthDate.Month.ToString();
                    Date = dtBirthDate;
                }
                else
                {
                    string[] bDates = birthDate.Split('.');
                    if (bDates.Length == 3)
                    {
                        int day;
                        int month;
                        int year;
                        if (int.TryParse(bDates[0], out day) && day > 0 && day < 32)
                        {

                            BirthDates[0] = day.ToString();
                        }
                        if (int.TryParse(bDates[1], out month) && month > 0 && month <= 12)
                        {
                            BirthDates[1] = dtfi.GetMonthName(month);
                            BirthDates[3] = month.ToString();
                        }
                        if (int.TryParse(bDates[2], out year) && year > 1900 && year < 2100)
                        {
                            BirthDates[2] = year.ToString();
                        }
                    } else
                    {
                        BirthDates[2] = birthDate;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
            return true;
        }
    }
}