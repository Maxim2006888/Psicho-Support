using Microsoft.Extensions.DependencyInjection;
using Psicho_Support.Core;
using Psicho_Support.Data;
using Psicho_Support.Helpers;
using Psicho_Support.Services;
using Psicho_Support.Services.Interfaces;
using Psicho_Support.Views;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Psicho_Support.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly AppSession _session;
        private readonly AppState _appState;

        private string _username;
        private string _password;
        private string _confirmPassword;
        private bool _isLoading;

        public RegisterViewModel(
            AppSession session,
            AppState appState,  // ✅ Добавляем AppState
            IDialogService dialogService,
            INavigationService navigationService)
            : base(dialogService, navigationService)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _appState = appState ?? throw new ArgumentNullException(nameof(appState));

            Title = "Регистрация";

            RegisterCommand = new RelayCommand(async () => await RegisterAsync(), () => CanRegister);
            BackToLoginCommand = new RelayCommand(BackToLogin);
        }

        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    OnPropertyChanged(nameof(CanRegister));
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    OnPropertyChanged(nameof(CanRegister));
                    OnPropertyChanged(nameof(PasswordStrength));
                    OnPropertyChanged(nameof(PasswordStrengthText));
                    OnPropertyChanged(nameof(PasswordStrengthColor));
                    OnPropertyChanged(nameof(DoPasswordsMatch));
                }
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (SetProperty(ref _confirmPassword, value))
                {
                    OnPropertyChanged(nameof(CanRegister));
                    OnPropertyChanged(nameof(DoPasswordsMatch));
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool CanRegister =>
            !string.IsNullOrWhiteSpace(Username) &&
            Username.Length >= 3 &&
            !string.IsNullOrWhiteSpace(Password) &&
            Password.Length >= 5 &&
            Password == ConfirmPassword;

        public bool DoPasswordsMatch =>
            !string.IsNullOrWhiteSpace(Password) &&
            !string.IsNullOrWhiteSpace(ConfirmPassword) &&
            Password == ConfirmPassword;

        public int PasswordStrength
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Password)) return 0;

                int strength = 0;
                if (Password.Length >= 5) strength++;
                if (Password.Length >= 8) strength++;
                if (System.Text.RegularExpressions.Regex.IsMatch(Password, @"\d")) strength++;
                if (System.Text.RegularExpressions.Regex.IsMatch(Password, @"[A-Z]")) strength++;
                if (System.Text.RegularExpressions.Regex.IsMatch(Password, @"[^a-zA-Z0-9]")) strength++;
                return strength;
            }
        }

        public string PasswordStrengthText
        {
            get
            {
                switch (PasswordStrength)
                {
                    case 0:
                    case 1: return "Очень слабый";
                    case 2: return "Слабый";
                    case 3: return "Средний";
                    case 4: return "Хороший";
                    case 5: return "Отличный";
                    default: return "";
                }
            }
        }

        public string PasswordStrengthColor
        {
            get
            {
                switch (PasswordStrength)
                {
                    case 0:
                    case 1: return "#F44336";
                    case 2: return "#FF9800";
                    case 3: return "#FFC107";
                    case 4: return "#8BC34A";
                    case 5: return "#4CAF50";
                    default: return "#AAAAAA";
                }
            }
        }

        public ICommand RegisterCommand { get; }
        public ICommand BackToLoginCommand { get; }

        private async Task RegisterAsync()
        {
            if (!CanRegister) return;

            IsLoading = true;

            try
            {
                var user = await Task.Run(() =>
                {
                    using (var db = new HealthPsicho_DBEntities())
                    {
                        if (db.Users.Any(u => u.Username == Username))
                        {
                            return null;
                        }

                        var newUser = new Users
                        {
                            Username = Username,
                            PasswordHash = Password,
                            CreatedAt = DateTime.Now
                        };

                        db.Users.Add(newUser);
                        db.SaveChanges();

                        return newUser;
                    }
                });

                if (user != null)
                {
                    _appState.CurrentUser = user;
                    _session.StartSession(user);

                    await DialogService.ShowMessageAsync("Успех", "Регистрация завершена успешно!", Enums.DialogType.Success);

                    // ✅ Создаём новое окно (Transient)
                    NavigationService.SwitchToWindow<UserWindow>();
                }
                else
                {
                    await DialogService.ShowMessageAsync("Ошибка", "Пользователь с таким именем уже существует", Enums.DialogType.Error);
                }
            }
            catch (Exception ex)
            {
                await DialogService.ShowMessageAsync("Ошибка", $"Не удалось завершить регистрацию: {ex.Message}", Enums.DialogType.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void BackToLogin()
        {
            NavigationService.SwitchToWindow<LoginWindow>();
        }
    }
}