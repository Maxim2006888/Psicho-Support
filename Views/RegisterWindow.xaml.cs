// Views/RegisterWindow.xaml.cs
using Microsoft.Extensions.DependencyInjection;
using Psicho_Support.Data;
using Psicho_Support.Enums;
using Psicho_Support.Services.Interfaces;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Psicho_Support.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        public RegisterWindow(IDialogService dialogService, INavigationService navigationService)
        {
            InitializeComponent();
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholders();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginInput.Text.Trim();
            string password = PasswordInput.Password.Trim();
            string confirmPassword = ConfirmPasswordInput.Password.Trim();

            if (string.IsNullOrEmpty(login))
            {
                _dialogService.Show("Ошибка", "Введите логин.", DialogType.Warning, this);
                return;
            }

            if (login.Length < 3)
            {
                _dialogService.Show("Ошибка", "Логин должен содержать не менее 3 символов.", DialogType.Warning, this);
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                _dialogService.Show("Ошибка", "Введите пароль.", DialogType.Warning, this);
                return;
            }

            if (password.Length < 5)
            {
                _dialogService.Show("Ошибка", "Пароль должен содержать не менее 5 символов.", DialogType.Warning, this);
                return;
            }

            if (!string.IsNullOrEmpty(confirmPassword) && password != confirmPassword)
            {
                _dialogService.Show("Ошибка", "Пароли не совпадают.", DialogType.Warning, this);
                return;
            }

            using (var db = new HealthPsicho_DBEntities())
            {
                try
                {
                    if (db.Users.Any(u => u.Username == login))
                    {
                        _dialogService.Show("Ошибка", "Пользователь с таким логином уже существует.", DialogType.Error, this);
                        return;
                    }

                    var user = new Users
                    {
                        Username = login,
                        PasswordHash = password,
                        CreatedAt = DateTime.Now
                    };

                    db.Users.Add(user);
                    db.SaveChanges();

                    _dialogService.Show("Успех", "Регистрация завершена успешно! Теперь вы можете войти.", DialogType.Success, this);

                    _navigationService.SwitchToWindow<LoginWindow>();
                    Close();
                }
                catch (Exception ex)
                {
                    _dialogService.Show("Ошибка", $"Не удалось завершить регистрацию: {ex.Message}", DialogType.Error, this);
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _navigationService.SwitchToWindow<LoginWindow>();
            Close();
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholders();
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholders();
            CheckPasswordStrength();
        }

        private void ConfirmPasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholders();
            CheckPasswordMatch();
        }

        private void UpdatePlaceholders()
        {
            LoginPlaceholder.Visibility = string.IsNullOrEmpty(LoginInput.Text) ? Visibility.Visible : Visibility.Collapsed;
            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordInput.Password) ? Visibility.Visible : Visibility.Collapsed;

            if (ConfirmPasswordPlaceholder != null)
            {
                ConfirmPasswordPlaceholder.Visibility = string.IsNullOrEmpty(ConfirmPasswordInput.Password) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void CheckPasswordStrength()
        {
            if (PasswordStrengthIndicator == null) return;

            string password = PasswordInput.Password;

            if (string.IsNullOrEmpty(password))
            {
                PasswordStrengthIndicator.Text = "";
                PasswordStrengthIndicator.Visibility = Visibility.Collapsed;
                return;
            }

            PasswordStrengthIndicator.Visibility = Visibility.Visible;

            int strength = 0;

            if (password.Length >= 5) strength++;
            if (password.Length >= 8) strength++;
            if (password.Any(char.IsDigit)) strength++;
            if (password.Any(char.IsUpper)) strength++;
            if (password.Any(ch => !char.IsLetterOrDigit(ch))) strength++;

            switch (strength)
            {
                case 0:
                case 1:
                    PasswordStrengthIndicator.Text = "Сложность: очень слабый";
                    PasswordStrengthIndicator.Foreground = System.Windows.Media.Brushes.Red;
                    break;
                case 2:
                    PasswordStrengthIndicator.Text = "Сложность: слабый";
                    PasswordStrengthIndicator.Foreground = System.Windows.Media.Brushes.Orange;
                    break;
                case 3:
                    PasswordStrengthIndicator.Text = "Сложность: средний";
                    PasswordStrengthIndicator.Foreground = System.Windows.Media.Brushes.Yellow;
                    break;
                case 4:
                    PasswordStrengthIndicator.Text = "Сложность: хороший";
                    PasswordStrengthIndicator.Foreground = System.Windows.Media.Brushes.LightGreen;
                    break;
                case 5:
                    PasswordStrengthIndicator.Text = "Сложность: отличный";
                    PasswordStrengthIndicator.Foreground = System.Windows.Media.Brushes.Green;
                    break;
            }
        }

        private void CheckPasswordMatch()
        {
            if (PasswordMatchIndicator == null) return;

            string password = PasswordInput.Password;
            string confirm = ConfirmPasswordInput.Password;

            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirm))
            {
                PasswordMatchIndicator.Text = "";
                PasswordMatchIndicator.Visibility = Visibility.Collapsed;
                return;
            }

            PasswordMatchIndicator.Visibility = Visibility.Visible;

            if (password == confirm)
            {
                PasswordMatchIndicator.Text = "✓ Пароли совпадают";
                PasswordMatchIndicator.Foreground = System.Windows.Media.Brushes.LightGreen;
            }
            else
            {
                PasswordMatchIndicator.Text = "✗ Пароли не совпадают";
                PasswordMatchIndicator.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
    }
}