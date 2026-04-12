using Microsoft.Extensions.DependencyInjection;
using Psicho_Support.Enums;
using Psicho_Support.Helpers;
using Psicho_Support.Services;
using Psicho_Support.Services.Interfaces;
using Psicho_Support.Views;
using Psicho_Support.Views.Pages;
using System;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Psicho_Support.Core;

namespace Psicho_Support.ViewModels
{
    public class UserWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly AppSession _session;
        private readonly AppState _appState;  
        private readonly UserStateService _stateService;
        private readonly IThemeService _themeService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        private TimeSpan _sessionTime;
        private int _currentStateValue;
        private string _currentStateLevel;
        private string _currentStateColor;
        private string _welcomeMessage;
        private bool _showWelcomeMessage;
        private int _currentPageIndex = 2;
        private bool _disposed = false;

        // События для View
        public event EventHandler<int> PageNavigationRequested;
        public event EventHandler<Type> PageChangeRequested;

        public UserWindowViewModel(
            AppSession session,
            AppState appState,  
            UserStateService stateService,
            IThemeService themeService,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _appState = appState ?? throw new ArgumentNullException(nameof(appState));  // ✅ Инициализируем _appState
            _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));
            _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            _stateService.StateChanged += OnStateChanged;

            // Инициализация
            InitializeWelcomeMessage();
            UpdateStateDisplay();

            // Команды - все в одном месте
            NavigateHomeCommand = new RelayCommand(() => RequestPageNavigation(2));
            NavigateNotesCommand = new RelayCommand(() => RequestPageNavigation(0));
            NavigateTestsCommand = new RelayCommand(() => RequestPageNavigation(1));
            NavigateAdviceCommand = new RelayCommand(() => RequestPageNavigation(3));
            NavigateAnalyticsCommand = new RelayCommand(() => RequestPageChange(typeof(AnalyticsPage)));
            NavigateSettingsCommand = new RelayCommand(() => RequestPageChange(typeof(SettingsPage)));
            NavigateAchievementsCommand = new RelayCommand(() => RequestPageChange(typeof(AchievementsPage)));
            LogoutCommand = new RelayCommand(async () => await OnLogout());

            // Жесты
            SwipeLeftCommand = new RelayCommand(() => RequestPageNavigation(_currentPageIndex + 1));
            SwipeRightCommand = new RelayCommand(() => RequestPageNavigation(_currentPageIndex - 1));
            SwipeUpCommand = new RelayCommand(() => RequestPageChange(typeof(AnalyticsPage)));
            SwipeDownCommand = new RelayCommand(() => RequestPageChange(typeof(AchievementsPage)));
        }

        // Команды
        public ICommand NavigateHomeCommand { get; }
        public ICommand NavigateNotesCommand { get; }
        public ICommand NavigateTestsCommand { get; }
        public ICommand NavigateAdviceCommand { get; }
        public ICommand NavigateAnalyticsCommand { get; }
        public ICommand NavigateSettingsCommand { get; }
        public ICommand NavigateAchievementsCommand { get; }
        public ICommand LogoutCommand { get; }

        public ICommand SwipeLeftCommand { get; }
        public ICommand SwipeRightCommand { get; }
        public ICommand SwipeUpCommand { get; }
        public ICommand SwipeDownCommand { get; }

        public int CurrentPageIndex
        {
            get => _currentPageIndex;
            set
            {
                if (_currentPageIndex != value)
                {
                    _currentPageIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        public TimeSpan SessionTime
        {
            get => _sessionTime;
            set
            {
                if (_sessionTime != value)
                {
                    _sessionTime = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SessionTimeFormatted));
                }
            }
        }

        public string SessionTimeFormatted => _sessionTime.ToString(@"mm\:ss");

        public int CurrentStateValue
        {
            get => _currentStateValue;
            set
            {
                if (_currentStateValue != value)
                {
                    _currentStateValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CurrentStateLevel
        {
            get => _currentStateLevel;
            set
            {
                if (_currentStateLevel != value)
                {
                    _currentStateLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CurrentStateColor
        {
            get => _currentStateColor;
            set
            {
                if (_currentStateColor != value)
                {
                    _currentStateColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set
            {
                if (_welcomeMessage != value)
                {
                    _welcomeMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowWelcomeMessage
        {
            get => _showWelcomeMessage;
            set
            {
                if (_showWelcomeMessage != value)
                {
                    _showWelcomeMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsUserLoggedIn => _appState?.IsAuthenticated ?? false;
        public string Username => _appState?.CurrentUser?.Username ?? "Гость";

        private void InitializeWelcomeMessage()
        {
            if (_appState?.IsAuthenticated == true)
            {
                WelcomeMessage = $"Добро пожаловать, {_appState.CurrentUser.Username}!";
                ShowWelcomeMessage = true;
            }
        }

        private void OnStateChanged(int newValue)
        {
            CurrentStateValue = newValue;
            UpdateStateDisplay();
        }

        private void UpdateStateDisplay()
        {
            CurrentStateLevel = GetStateLevelName(_stateService.CurrentLevel);
            CurrentStateColor = GetStateLevelColor(_stateService.CurrentLevel);
        }

        private string GetStateLevelName(UserStateLevel level)
        {
            switch (level)
            {
                case UserStateLevel.Critical: return "Критическое";
                case UserStateLevel.Low: return "Пониженное";
                case UserStateLevel.Stable: return "Стабильное";
                case UserStateLevel.Good: return "Хорошее";
                case UserStateLevel.Excellent: return "Отличное";
                default: return "Не определено";
            }
        }

        private string GetStateLevelColor(UserStateLevel level)
        {
            switch (level)
            {
                case UserStateLevel.Critical: return "#F44336";
                case UserStateLevel.Low: return "#FF9800";
                case UserStateLevel.Stable: return "#FFC107";
                case UserStateLevel.Good: return "#8BC34A";
                case UserStateLevel.Excellent: return "#4CAF50";
                default: return "#AAAAAA";
            }
        }

        private void RequestPageNavigation(int pageIndex)
        {
            PageNavigationRequested?.Invoke(this, pageIndex);
        }

        private void RequestPageChange(Type pageType)
        {
            PageChangeRequested?.Invoke(this, pageType);
        }

        public void UpdateSessionTime(TimeSpan newTime)
        {
            SessionTime = newTime;
        }

        public void HideWelcomeMessage()
        {
            ShowWelcomeMessage = false;
        }

        private async Task OnLogout()
        {
            try
            {
                var result = await _dialogService.ShowConfirmationAsync("Выход", "Вы уверены, что хотите выйти?");
                if (result)
                {
                    _session?.EndSession();
                    _appState?.Logout();

                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        // Закрываем текущее окно
                        foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                        {
                            if (window is UserWindow)
                            {
                                window.Close();
                                break;
                            }
                        }

                        // ✅ Создаём новое окно входа (Transient)
                        var loginWindow = App.Services.GetRequiredService<LoginWindow>();
                        loginWindow.Show();

                        // Сбрасываем MainWindow
                        if (System.Windows.Application.Current.MainWindow == null ||
                            System.Windows.Application.Current.MainWindow is UserWindow)
                        {
                            System.Windows.Application.Current.MainWindow = loginWindow;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowMessageAsync("Ошибка", $"Ошибка при выходе: {ex.Message}", DialogType.Error);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_stateService != null)
                {
                    _stateService.StateChanged -= OnStateChanged;
                }
                _disposed = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}