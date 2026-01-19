using Psicho_Support.Data;
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
using System.Windows.Shapes;
using System.Data.Entity;
using System.Windows.Threading;
using Psicho_Support.Services;

namespace Psicho_Support.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Пример: имитация загрузки
            DetailsText.Visibility = Visibility.Visible;
            DetailsText.Text = "Проверка подключения...";

            // Используем DispatcherTimer для имитации загрузки
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                DetailsText.Visibility = Visibility.Collapsed;
            };
            timer.Start();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginInput.Text.Trim();
            string password = PasswordInput.Password.Trim();

            // Проверка на пустые поля
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new HealthPsicho_DBEntities())
            {
                var user = db.Users.FirstOrDefault(u => u.Username == login && u.PasswordHash == password);

                if (user != null)
                {
                    MessageBox.Show($"Добро пожаловать, {user.Username}!", "Вход выполнен");

                    // Инициализируем сессию
                    AppSession.Start(user);

                    // Создаем UserWindow БЕЗ параметра
                    UserWindow main = new UserWindow();
                    main.Show();
                    Close();
                }
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow register = new RegisterWindow();
            register.Show();
            Close();
        }

        // Опционально: небольшая визуальная реакция на изменение текста
        private void Input_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Можно добавить анимацию placeholder или подсветку, если будет нужно позже
        }
    }
}

