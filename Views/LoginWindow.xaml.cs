using Microsoft.Extensions.DependencyInjection;
using Psicho_Support.Core;
using Psicho_Support.Data;
using Psicho_Support.Enums;
using Psicho_Support.Services;
using Psicho_Support.Services.Interfaces;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Psicho_Support.Views
{
    public partial class LoginWindow : Window
    {
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly AppSession _session;

        public LoginWindow(
            IDialogService dialogService,
            INavigationService navigationService,
            AppSession session)
        {
            InitializeComponent();
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholders();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginInput.Text.Trim();
            string password = PasswordInput.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                _dialogService.Show(
                    "Ошибка",
                    "Введите логин и пароль.",
                    DialogType.Warning,
                    this);
                return;
            }

            Users user = null;

            try
            {
                using (var db = new HealthPsicho_DBEntities())
                {
                    user = db.Users
                        .FirstOrDefault(u => u.Username == login && u.PasswordHash == password);
                }
            }
            catch (Exception ex)
            {
                _dialogService.Show(
                    "Ошибка",
                    $"Ошибка подключения к базе данных: {ex.Message}",
                    DialogType.Error,
                    this);
                return;
            }

            if (user == null)
            {
                _dialogService.Show(
                    "Ошибка входа",
                    "Неверный логин или пароль.",
                    DialogType.Error,
                    this);
                return;
            }

            // Запускаем сессию
            _session.StartSession(user);

            // ✅ Сохраняем пользователя в AppState (если используете)
            var appState = App.Services.GetRequiredService<AppState>();
            appState.CurrentUser = user;

            // ✅ Сначала показываем сообщение об успехе
            _dialogService.Show(
                "Успех",
                $"Добро пожаловать, {user.Username}!",
                DialogType.Success,
                this);

            // ✅ Затем открываем главное окно
            var mainWindow = App.Services.GetRequiredService<UserWindow>();
            mainWindow.Show();

            // ✅ Закрываем окно входа
            Close();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = App.Services.GetRequiredService<RegisterWindow>();
            registerWindow.Show();
            Close();
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholders();
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholders();
        }

        private void UpdatePlaceholders()
        {
            LoginPlaceholder.Visibility = string.IsNullOrEmpty(LoginInput.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordInput.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}