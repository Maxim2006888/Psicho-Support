using Microsoft.Extensions.DependencyInjection;
using Psicho_Support.Core;
using Psicho_Support.Data;
using Psicho_Support.Helpers;
using Psicho_Support.Services;
using Psicho_Support.Services.Interfaces;
using Psicho_Support.Views;
using Psicho_Support.Views.Onboarding;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Psicho_Support.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AppSession _session;
        private readonly AppState _appState;

        private string _username;
        private string _password;

        public LoginViewModel(
            AppSession session,
            AppState appState,  // ✅ Добавляем AppState в параметры
            IDialogService dialogService,
            INavigationService navigationService)
            : base(dialogService, navigationService)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _appState = appState ?? throw new ArgumentNullException(nameof(appState));

            Title = "Вход";
            LoginCommand = new RelayCommand(async () => await LoginAsync());
            GoToRegisterCommand = new RelayCommand(GoToRegister);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }

        // LoginViewModel.cs - LoginAsync метод
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                await DialogService.ShowMessageAsync("Ошибка", "Заполните все поля");
                return;
            }

            IsBusy = true;

            try
            {
                var user = await Task.Run(() =>
                {
                    using (var db = new HealthPsicho_DBEntities())
                    {
                        var foundUser = db.Users
                            .FirstOrDefault(u => u.Username == Username);

                        if (foundUser != null && foundUser.PasswordHash == Password)
                        {
                            return foundUser;
                        }

                        return null;
                    }
                });

                if (user != null)
                {
                    // Сохраняем пользователя в AppState
                    _appState.CurrentUser = user;

                    // Запускаем сессию
                    _session.StartSession(user);

                    // ✅ Создаём новое окно через DI (теперь Transient)
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        bool completed = Properties.Settings.Default.OnboardingCompleted;

                        if (!completed)
                        {
                            var onboarding = App.Services.GetRequiredService<OnboardingWindow>();
                            onboarding.Show();
                        }
                        else
                        {
                            var userWindow = App.Services.GetRequiredService<UserWindow>();
                            userWindow.Show();
                        }

                        foreach (Window window in Application.Current.Windows)
                        {
                            if (window is LoginWindow)
                            {
                                window.Close();
                                break;
                            }
                        }
                    });
                }
                else
                {
                    await DialogService.ShowMessageAsync("Ошибка", "Неверное имя пользователя или пароль");
                }
            }
            catch (Exception ex)
            {
                await DialogService.ShowMessageAsync("Ошибка", $"Ошибка входа: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void GoToRegister()
        {
            NavigationService.SwitchToWindow<RegisterWindow>();
        }
    }
}