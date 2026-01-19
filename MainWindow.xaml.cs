using Psicho_Support.Data;
using Psicho_Support.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Psicho_Support
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            

            // Пример: имитация загрузки
            DetailsText.Text = "Инициализация данных...";

            // Используем DispatcherTimer для имитации загрузки
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                OpenWindow(); // Переход на основное окно пользователя
            };
            timer.Start();
        }

        private void OpenWindow()
        {
            var userWindow = new HallowWindow();
            userWindow.Show();
            this.Close();
        }
    }
}
