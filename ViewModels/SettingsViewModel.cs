// ViewModels/SettingsViewModel.cs
using Microsoft.Extensions.DependencyInjection;
using Psicho_Support.Core;
using Psicho_Support.Helpers;
using Psicho_Support.Services;
using Psicho_Support.Services.Interfaces;
using Psicho_Support.Views;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Psicho_Support.ViewModels
{
    public class SettingsViewModel : BaseViewModel, IDisposable
    {
        private readonly IThemeService _themeService;
        private readonly AppSession _session;
        private readonly AppState _appState;
        private string _username;
        private string _sessionTime;
        private bool _isDarkTheme;
        private DispatcherTimer _timer;
        private bool _disposed = false;

        public SettingsViewModel(
            IDialogService dialogService,
            INavigationService navigationService,
            IThemeService themeService,
            AppSession session,
            AppState appState)
            : base(dialogService, navigationService)
        {
            _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _appState = appState ?? throw new ArgumentNullException(nameof(appState));
            Title = "Настройки";

            ToggleThemeCommand = new RelayCommand(ToggleTheme);
            LogoutCommand = new RelayCommand(Logout);
            SaveSettingsCommand = new RelayCommand(SaveSettings);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string SessionTime
        {
            get => _sessionTime;
            set => SetProperty(ref _sessionTime, value);
        }

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set => SetProperty(ref _isDarkTheme, value);
        }

        public ICommand ToggleThemeCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand SaveSettingsCommand { get; }

        public override async Task InitializeAsync(object parameter = null)
        {
            var user = _appState.CurrentUser;
            if (user != null)
            {
                Username = user.Username;
                SessionTime = _session.CurrentSessionDuration.ToString(@"mm\:ss");
                IsDarkTheme = _themeService.IsDarkTheme;
            }

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) =>
            {
                if (_appState.IsAuthenticated)
                    SessionTime = _session.CurrentSessionDuration.ToString(@"mm\:ss");
            };
            _timer.Start();

            await Task.CompletedTask;
        }

        private async void ToggleTheme()
        {
            // Исправлено: используем свойство вместо метода
            _themeService.IsDarkTheme = !_themeService.IsDarkTheme;
            IsDarkTheme = _themeService.IsDarkTheme;
            await Task.CompletedTask;
        }

        private async void SaveSettings()
        {
            await Task.Delay(100);
            await DialogService.ShowMessageAsync("Успех", "Настройки сохранены", Enums.DialogType.Success);
        }

        private async void Logout()
        {
            var confirmed = await DialogService.ShowConfirmationAsync("Выход", "Вы уверены, что хотите выйти?");

            if (confirmed)
            {
                _session?.EndSession();
                _appState.CurrentUser = null;
                _timer?.Stop();

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var loginWindow = App.Services.GetRequiredService<LoginWindow>();
                    loginWindow.Show();

                    foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                    {
                        if (window is Views.UserWindow)
                        {
                            window.Close();
                            break;
                        }
                    }
                });
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _timer?.Stop();
                _disposed = true;
            }
        }
    }
}