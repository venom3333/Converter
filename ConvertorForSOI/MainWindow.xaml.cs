using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Globalization;
using System.Data.SqlClient;
using Forms = System.Windows.Forms;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.IO;
using System.Configuration;
using ConvertorForSOI.SQLs;


namespace ConvertorForSOI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private string sourceFileRus = string.Empty; // Путь к файлу с источником данных c Рус - содержимым.
        private string sourceFileEng = string.Empty; // Путь к файлу с источником данных c Eng - содержимым.
        private string dataPathRus = string.Empty;   // Пукть к файлу где будет храниться результирующая таблица для Рус - содержимого.
        private string dataPathEng = string.Empty;   // Пукть к файлу где будет храниться результирующая таблица для Eng - содержимого.
        private string photoPath = string.Empty; // Путь к папке где лежат фотографии соответствующие данному файлу.

        private BackgroundWorker bw = new BackgroundWorker();
        Popup pop = new Popup();


        public MainWindow()
        {
            InitializeComponent();
			//var aa = ConfigurationManager.AppSettings;
			// включаем кнопку Truncate Real если тестовая машина!
			if (ConfigurationManager.ConnectionStrings["Nake_data"].ConnectionString.Contains("Data Source=194.168.0.140"))
            {
                btnTruncateReal.IsEnabled = true;
            }
        }

        // Клик по "Закрыть" в меню.
        private void miClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Клик по "Источник данных - Рус".
        private void miSourceRus_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            
            if (!string.IsNullOrWhiteSpace(Config.SourceFolder))
            {
                ofd.InitialDirectory = Config.SourceFolder;
            }
            if (!string.IsNullOrWhiteSpace(Convertor.GetFilterSource()))
            {
                ofd.Filter = Convertor.GetFilterSource();
            }
            ofd.CheckFileExists = true;
            if (ofd.ShowDialog() == true)
            {
                sourceFileRus = ofd.FileName;
                Config.SourceFolder = ofd.FileName;

                // Выбор папки с фотографиями.
                Forms.FolderBrowserDialog fbd = new Forms.FolderBrowserDialog();
                fbd.Description = "Выберете папку в которой находятся фотографии соответствующие данному файлу.";
                fbd.SelectedPath = Config.SourceFolder;
                Forms.DialogResult result = fbd.ShowDialog();
                if (result == Forms.DialogResult.OK)
                {
                    photoPath = fbd.SelectedPath;
                }
            }
        }

        // Клик по "Источник данных - Eng".
        private void miSourceEng_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (!string.IsNullOrWhiteSpace(Config.SourceFolder))
            {
                ofd.InitialDirectory = Config.SourceFolder;
            }
            if (!string.IsNullOrWhiteSpace(Convertor.GetFilterSource()))
            {
                ofd.Filter = Convertor.GetFilterSource();
            }
            ofd.CheckFileExists = true;
            if (ofd.ShowDialog() == true)
            {
                sourceFileEng = ofd.FileName;
                Config.SourceFolder = ofd.FileName;
            }
        }

        // Клик по кнопке "Закрыть".
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Клик по кнопке "Truncate TEMPS".
        private void btnTruncateTemps_Click(object sender, RoutedEventArgs e)
        {
            // Configure message box
            string message = "Это действие очистит ваши таблицы! Вы уверены?";
            string caption = "Очистить таблицы!";
            MessageBoxButton buttons = MessageBoxButton.OKCancel;
            MessageBoxImage icon = MessageBoxImage.Information;
            MessageBoxResult defaultResult = MessageBoxResult.OK;
            //MessageBoxOptions options = MessageBoxOptions.RtlReading;
            // Show message box
            MessageBoxResult result = MessageBox.Show(message, caption, buttons, icon, defaultResult);
            if (result.ToString() == "OK")
            {
                if (BaseSQL.TruncateTemps(checkBox_IsMissing.IsChecked ?? false))
                {
                    MessageBox.Show("Темповые таблицы очищены!");
                }
                else
                {
                    MessageBox.Show("Ошибка при очистке темповых таблиц!");
                }
            }
        }

        // Клик по кнопке "Truncate REAL".
        private void btnTruncateReal_Click(object sender, RoutedEventArgs e)
        {
            // Configure message box
            string message = "Это действие очистит ваши таблицы! Вы уверены?";
            string caption = "Очистить таблицы!";
            MessageBoxButton buttons = MessageBoxButton.OKCancel;
            MessageBoxImage icon = MessageBoxImage.Information;
            MessageBoxResult defaultResult = MessageBoxResult.OK;
            //MessageBoxOptions options = MessageBoxOptions.RtlReading;
            // Show message box
            MessageBoxResult result = MessageBox.Show(message, caption, buttons, icon, defaultResult);
            if (result.ToString() == "OK")
            {
                WantedSQL.TruncateReal();
            }
        }

        // Клик по кнопке "Загрузить".
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            // TODO: вернуть сообщение что данные загружены, не отображается прогрес бар после перехода на Thread
            // Запускаем прогресс бар. 

            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("isMissing", checkBox_IsMissing.IsChecked.ToString().ToLower());

            //List<string> args = new List<string>();
            //args.Add(dataPathRus);
            //args.Add(photoPath);
            //args.Add(checkBox_IsMissing.IsChecked.ToString().ToLower());

            btnOK.Visibility = Visibility.Hidden;
            progressBar.Visibility = Visibility.Visible;
            label1.Visibility = Visibility.Visible;
            label1.Content = "Идет загрузка данных, Пожалуйста подождите.";

            //App.Current.MainWindow.InvalidateVisual();
            //App.Current.MainWindow.UpdateLayout();

            // Удалить файлы с информацией от предыдущей загрузки.
            ParseHelper.DeleteInfoFiles();

            // Делаем кнопку "Загрузить данные во временные таблицы" недоступной.
            btnAddPhotos.IsEnabled = true;                                          // Вся приблуда ниже для выполнения программы в параллельном потоке:
            //BackgroundWorker worker = new BackgroundWorker();                       // создается экземпляр класса BackgroundWorker для этого

            // Стартуем трэд
            Thread t = new Thread(() => worker_DoWork(args));
            // Включаем режим STA (для работы с клипборд)
            t.SetApartmentState(ApartmentState.STA);

            //progressBar.Visibility = System.Windows.Visibility.Visible;
            //progressBar.InvalidateVisual();
            //progressBar.UpdateLayout();
            //btnOK.Visibility = Visibility.Hidden;
            //label1.Visibility = System.Windows.Visibility.Hidden;

            this.UpdateLayout();

            // старт
            t.Start();
            // Ждем завершения трэда и выполним все что потом
            // t.Join();

            //label1.Content = e.Result;
            //progressBar.Visibility = System.Windows.Visibility.Hidden;
            //btnOK.Visibility = Visibility.Visible;
            //label1.Visibility = System.Windows.Visibility.Visible;
            //worker.WorkerReportsProgress = true;                                    // сообщать о ходе выполнения
            //worker.ProgressChanged += worker_ProgressChanged;                       // происходит при вызове ReportProgress (пока что не работает)
            ////worker.DoWork += worker_DoWork;                                         // происходит при вызове RunWorkerAsync
            //worker.RunWorkerCompleted += worker_RunWorkerCompleted;                 // происходит, когда фоновая операция завершена, была отменена или вызвала исключение
            //worker.RunWorkerAsync(args);                                       // собственно сам запуск выполнения фоновой 
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            label1.Content = e.Result;
            progressBar.Visibility = System.Windows.Visibility.Hidden;
            label1.Visibility = System.Windows.Visibility.Visible;
        }

        void worker_DoWork(Dictionary<string, string> args)
        {
            // Очищаем список ненайденных типов розыска и инициаторов
            ParseHelper.ClearMissedLists();

            bool isMissingPersons = args["isMissing"] == "true" ? true : false;

            if (Convertor.ConvertToXLS(sourceFileRus, sourceFileEng, photoPath, isMissingPersons))
                MessageBox.Show("ГОТОВО!");
            else
                MessageBox.Show("Что-то не так!");
        }

        // Вызываем метод для прикрепления фото.
        private void btnAddPhotos_Click(object sender, RoutedEventArgs e)
        {
            if (!WantedSQL.FillPhotosAndAdded())
            {
                MessageBox.Show("Не удалось загрузить добавочные данные.");
            }
            else
            {
                MessageBox.Show("Фотографии и/или доп. данные добавлены.");
            }
            btnOK.IsEnabled = true;
            //btnAddPhotos.IsEnabled = false;
        }

        // Вызываем метод для заполнения дополнительных типов розыска.
        private void btnAddWTS_Click(object sender, RoutedEventArgs e)
        {
            if (!WantedSQL.FillExtraWTS())
            {
                MessageBox.Show("Не удалось загрузить дополнительные типы розыска.");
            }
            else
            {
                MessageBox.Show("Дополнительные типы розыска добавлены.");
            }
            btnOK.IsEnabled = true;
            //btnAddWTS.IsEnabled = false;
        }

        // Метод вывода списка несуществующих типов розыска.
        private void btnSTList_Click(object sender, RoutedEventArgs e)
        {
            int counter = 0;
            string stList = "Нераспознанные типы розыска:\n\n";
            stList += ParseHelper.missedSearchTypes.Aggregate(new StringBuilder(), (a, i) =>
            {
                a.Append((++counter).ToString() + ". " + i + "\n");
                return a;
            }).ToString();

            if (stList != "Нераспознанные типы розыска:\n\n")
            {
                //MessageBox.Show(stList);
                MessageBox.Show("Имеются нераспознанные типы розыска в словаре программы! Смотри SearchTypes.txt", "ВНИМАНИЕ", MessageBoxButton.OK, MessageBoxImage.Warning);
                string path = Directory.GetCurrentDirectory();
                File.WriteAllText(System.IO.Path.Combine(path, "SearchTypes.txt"), stList);
            }
            else
            {
                MessageBox.Show("Нераспознанных типов розыска не встречено!", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Метод вывода списка несуществующих инициаторов розыска.
        private void btnInitiatorsList_Click(object sender, RoutedEventArgs e)
        {
            int counter = 0;
            string initList = "Нераспознанные инициаторы розыска:\n\n";
            initList += ParseHelper.missedWantedInitiators.Aggregate(new StringBuilder(), (a, i) =>
            {
                a.Append((++counter).ToString() + ". " + i + "\n");
                return a;
            }).ToString();

            if (initList != "Нераспознанные инициаторы розыска:\n\n")
            {
                //MessageBox.Show(initList);
                MessageBox.Show("Имеются нераспознанные инициаторы розыска в в словаре программы!\nСмотри WantedInitiators.txt", "ВНИМАНИЕ", MessageBoxButton.OK, MessageBoxImage.Warning);
                string path = Directory.GetCurrentDirectory();
                File.WriteAllText(System.IO.Path.Combine(path, "WantedInitiators.txt"), initList);
            }
            else
            {
                MessageBox.Show("Нераспознанных инициаторов розыска не встречено!", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Метод вывода списка несуществующих в БД инициаторов розыска.
        private void btnInitiatorsListDB_Click(object sender, RoutedEventArgs e)
        {
            int counter = 0;
            string initListDB = "В БД не найдены следующие инициаторы розыска:\n\n";
            initListDB += ParseHelper.missedWantedInitiatorsDB.Aggregate(new StringBuilder(), (a, i) =>
            {
                a.Append((++counter).ToString() + ". " + i + "\n");
                return a;
            }).ToString();

            if (initListDB != "В БД не найдены следующие инициаторы розыска:\n\n")
            {
                //MessageBox.Show(initListDB);
                MessageBox.Show("Имеются не найденные инициаторы розыска в БД! Смотри WantedInitiatorsDB.txt", "ВНИМАНИЕ", MessageBoxButton.OK, MessageBoxImage.Warning);
                string path = Directory.GetCurrentDirectory();
                File.WriteAllText(System.IO.Path.Combine(path, "WantedInitiatorsDB.txt"), initListDB);
            }
            else
            {
                MessageBox.Show("Все необходимые инициаторы розыска имеются в БД!", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Метод вывода списка несуществующих в БД типов розыска.
        private void btnSTListDB_Click(object sender, RoutedEventArgs e)
        {
            int counter = 0;
            string stListDB = "В БД не найдены следующие типы розыска:\n\n";
            stListDB += ParseHelper.missedSearchTypesDB.Aggregate(new StringBuilder(), (a, i) =>
            {
                a.Append((++counter).ToString() + ". " + i + "\n");
                return a;
            }).ToString();

            if (stListDB != "В БД не найдены следующие типы розыска:\n\n")
            {
                //MessageBox.Show(stListDB);
                MessageBox.Show("Имеются не найденные типы розыска в БД! Смотри SearchTypesDB.txt", "ВНИМАНИЕ", MessageBoxButton.OK, MessageBoxImage.Warning);
                string path = Directory.GetCurrentDirectory();
                File.WriteAllText(System.IO.Path.Combine(path, "SearchTypesDB.txt"), stListDB);
            }
            else
            {
                MessageBox.Show("Все необходимые типы розыска имеются в БД!", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
