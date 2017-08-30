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
    // Класс для полного имени человека, которое распарсивается на составляющие.
    public class FullName
    {
        public string AddedName;  // Если в скобках рядом с именем что-то написано, то считаем что это дополнительное имя и заполняем примечание.
        public Dictionary<int, string> Names = new Dictionary<int, string>(3)
        {
            {0, " "}, // LastName.
            {1, " "}, // FirstName.
            {2, " "}  // SecondName.
        };

        private char[] SplitChar = { ' ' };
        private char[] SplitCharAdd = { '(' };
        public FullName(string fullName)
        {
            TryParse(fullName);
        }
        public bool TryParse(string fullName)
        {
            try
            {
                string[] addedNicknames = fullName.Split(SplitCharAdd, StringSplitOptions.RemoveEmptyEntries);
                if (addedNicknames.Length > 1)
                {
                    AddedName = addedNicknames[1].Replace("(", "").Replace(")", "").Trim();
                }

                string[] names = fullName.Split(SplitChar, 3, StringSplitOptions.RemoveEmptyEntries);
                if (names.Length > 2)
                {
                    Names[0] = names[0];
                    Names[1] = names[1];
                    Names[2] = names[2];
                }
                else if (names.Length == 2)
                {
                    Names[0] = names[0];
                    Names[1] = names[1];
                }
                else if (names.Length == 1)
                {
                    Names[0] = names[0];
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