using Psicho_Support.Data;
using Psicho_Support.Services;
using Psicho_Support.Views.Pages;
using System;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Psicho_Support.Views
{
    public partial class UserWindow : Window
    {

        private readonly NavigationService _navigation;
        private DispatcherTimer _timer;
        private TimeSpan _sessionTime;

        public UserWindow()
        {

            if (AppSession.CurrentUser == null)
            {
                MessageBox.Show("Ошибка инициализации. Пожалуйста, войдите заново.");
                Close();
                return;
            }

            InitializeComponent();

            _navigation = new NavigationService(MainContent);
            _navigation.Navigate(new WelcomePage());

            InitializeTimer();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DetailsText.Visibility = Visibility.Visible;
            DetailsText.Text = "Инициализация данных...";

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };

            timer.Tick += (s, args) =>
            {
                timer.Stop();
                DetailsText.Visibility = Visibility.Collapsed;
            };

            timer.Start();
        }

        private void InitializeTimer()
        {
            _sessionTime = TimeSpan.Zero;
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += (s, e) =>
            {
                _sessionTime = _sessionTime.Add(TimeSpan.FromSeconds(1));
                TimerText.Text = _sessionTime.ToString(@"mm\:ss");
            };

            _timer.Start();
        }

        // ---------- НАВИГАЦИЯ ----------



        private void Notes_Click(object sender, RoutedEventArgs e)
        {
            _navigation.Navigate(new NotesPage()); 
            StatusText.Text = "Открыт раздел «Заметки»";
        }

        private void Tests_Click(object sender, RoutedEventArgs e)
        {
            _navigation.Navigate(new TestsPage());
            StatusText.Text = "Открыт раздел «Тесты»";
        }

        private void Analytics_Click(object sender, RoutedEventArgs e)
        {
            _navigation.Navigate(new AnalyticsPage());
            StatusText.Text = "Открыт раздел «Аналитика»";
        }

        private void Advice_Click(object sender, RoutedEventArgs e)
        {
            _navigation.Navigate(new AdvicePage());
            StatusText.Text = "Открыт раздел «Рекомендации»";
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            _navigation.Navigate(new SettingsPage());
            StatusText.Text = "Открыт раздел «Настройки»";
        }

        private void Achievements_Click(object sender, RoutedEventArgs e)
        {
            _navigation.Navigate(new AchievementsPage());
            StatusText.Text = "Открыт раздел «Достижения»";
            
        }

        // ---------- ВЫХОД ----------

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                "Вы действительно хотите выйти?",
                "Выход",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _timer.Stop();
                AppSession.End();
                Close();
            }
        }
    }
}
