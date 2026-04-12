using Microsoft.Extensions.DependencyInjection;
using Psicho_Support.Core;
using Psicho_Support.Enums;
using Psicho_Support.Services;
using Psicho_Support.Services.Interfaces;
using Psicho_Support.ViewModels;
using Psicho_Support.Views.Pages;
using System;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Psicho_Support.Views
{
    public partial class UserWindow : Window
    {
        private readonly UserWindowViewModel _viewModel;
        private readonly INavigationService _navigation;
        private readonly DispatcherTimer _timer;
        private TimeSpan _sessionTime;
        private readonly IDialogService _dialogService;
        private readonly AppSession _session;
        private readonly AppState _appState;
        private readonly IThemeService _themeManager;
        private readonly IServiceProvider _serviceProvider;

        private Point _gestureStart;
        private bool _gestureActive;
        private bool _gestureHandled;
        private const double SwipeThreshold = 100;
        private TranslateTransform _contentTransform;

        private readonly Type[] _mainPages =
        {
            typeof(NotesPage),
            typeof(TestsPage),
            typeof(WelcomePage),
            typeof(AdvicePage),
            typeof(AnalyticsPage)
        };

        public UserWindow(
            AppSession session,
            AppState appState,
            IDialogService dialogService,
            IThemeService themeManager,
            UserStateService stateService,
            INavigationService navigationService,
            IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _session = session ?? throw new ArgumentNullException(nameof(session));
            _appState = appState ?? throw new ArgumentNullException(nameof(appState));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
            _navigation = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            if (Application.Current.MainWindow == null)
            {
                Application.Current.MainWindow = this;
            }

            if (!_appState.IsAuthenticated)
            {
                _dialogService.Show(
                    "Ошибка",
                    "Ошибка инициализации. Пожалуйста, войдите заново.",
                    DialogType.Error,
                    this);
                Close();
                return;
            }

            _themeManager.ApplyTheme();

            _navigation.Initialize(MainContent);

            _viewModel = new UserWindowViewModel(
                _session,
                _appState,
                stateService,
                _themeManager,
                _dialogService,
                _navigation);
            DataContext = _viewModel;

            _viewModel.PageNavigationRequested += OnPageNavigationRequested;
            _viewModel.PageChangeRequested += OnPageChangeRequested;

            // Инициализация трансформации после того как MainContent готов
            _contentTransform = new TranslateTransform();

            _sessionTime = TimeSpan.Zero;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += OnTimerTick;
            _timer.Start();

            PreviewMouseDown += OnGestureStart;
            PreviewMouseMove += OnGestureMove;
            PreviewMouseUp += OnGestureEnd;
            Closed += OnWindowClosed;

            // Загружаем страницу после полной инициализации
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Убеждаемся что MainContent готов
            if (MainContent != null)
            {
                MainContent.RenderTransform = _contentTransform;
                MainContent.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            // Открываем начальную страницу
            OpenPage(2);
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            _sessionTime += TimeSpan.FromSeconds(1);
            _viewModel.UpdateSessionTime(_sessionTime);
        }

        private void OnPageNavigationRequested(object sender, int pageIndex)
        {
            OpenPage(pageIndex);
        }

        private void OnPageChangeRequested(object sender, Type pageType)
        {
            try
            {
                NavigateToPageType(pageType);
            }
            catch (Exception ex)
            {
                _dialogService.Show(
                    "Ошибка навигации",
                    $"Не удалось открыть страницу: {ex.Message}",
                    DialogType.Error,
                    this);
            }
        }

        private void NavigateToPageType(Type pageType)
        {
            UserControl page = null;

            if (pageType == typeof(NotesPage))
            {
                page = _serviceProvider.GetRequiredService<NotesPage>();
                var viewModel = _serviceProvider.GetRequiredService<NotesViewModel>();
                ((NotesPage)page).SetViewModel(viewModel);
            }
            else if (pageType == typeof(TestsPage))
            {
                page = _serviceProvider.GetRequiredService<TestsPage>();
                var viewModel = _serviceProvider.GetRequiredService<TestsViewModel>();
                page.DataContext = viewModel;
            }
            else if (pageType == typeof(WelcomePage))
            {
                page = _serviceProvider.GetRequiredService<WelcomePage>();
                var viewModel = _serviceProvider.GetRequiredService<WelcomeViewModel>();
                page.DataContext = viewModel;
            }
            else if (pageType == typeof(AdvicePage))
            {
                page = _serviceProvider.GetRequiredService<AdvicePage>();
                var viewModel = _serviceProvider.GetRequiredService<AdviceViewModel>();
                page.DataContext = viewModel;
            }
            else if (pageType == typeof(AnalyticsPage))
            {
                page = _serviceProvider.GetRequiredService<AnalyticsPage>();
                var viewModel = _serviceProvider.GetRequiredService<AnalyticsViewModel>();
                page.DataContext = viewModel;
            }
            else if (pageType == typeof(AchievementsPage))
            {
                page = _serviceProvider.GetRequiredService<AchievementsPage>();
                var viewModel = _serviceProvider.GetRequiredService<AchievementsViewModel>();
                page.DataContext = viewModel;
            }
            else if (pageType == typeof(SettingsPage))
            {
                page = _serviceProvider.GetRequiredService<SettingsPage>();
                var viewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
                page.DataContext = viewModel;
            }

            if (page != null)
            {
                _navigation.Navigate(page);
                AnimatePageAppear();
            }
        }

        private void OpenPage(int index)
        {
            if (index < 0) index = _mainPages.Length - 1;
            if (index >= _mainPages.Length) index = 0;

            _viewModel.CurrentPageIndex = index;

            try
            {
                NavigateToPageType(_mainPages[index]);
            }
            catch (Exception ex)
            {
                _dialogService.Show(
                    "Ошибка навигации",
                    $"Не удалось открыть страницу: {ex.Message}",
                    DialogType.Error,
                    this);
            }
        }

        private void OnGestureStart(object sender, MouseButtonEventArgs e)
        {
            _gestureStart = e.GetPosition(this);
            _gestureActive = true;
            _gestureHandled = false;
        }

        private void OnGestureMove(object sender, MouseEventArgs e)
        {
            if (!_gestureActive || _gestureHandled || _contentTransform == null)
                return;

            Point current = e.GetPosition(this);
            double dx = current.X - _gestureStart.X;
            double dy = current.Y - _gestureStart.Y;

            if (Math.Abs(dx) > Math.Abs(dy))
                _contentTransform.X = dx * 0.5;
        }

        private void OnGestureEnd(object sender, MouseButtonEventArgs e)
        {
            if (!_gestureActive)
                return;

            Point end = e.GetPosition(this);
            double dx = end.X - _gestureStart.X;
            double dy = end.Y - _gestureStart.Y;

            _gestureActive = false;

            if (Math.Abs(dx) > SwipeThreshold && Math.Abs(dx) > Math.Abs(dy))
            {
                if (dx > 0)
                    _viewModel.SwipeRightCommand?.Execute(null);
                else
                    _viewModel.SwipeLeftCommand?.Execute(null);
                _gestureHandled = true;
            }
            else
            {
                AnimateReturn();
            }
        }

        private void AnimateReturn()
        {
            if (_contentTransform == null) return;

            var animX = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            _contentTransform.BeginAnimation(TranslateTransform.XProperty, animX);
        }

        private void AnimatePageAppear()
        {
            // Проверяем, что все необходимые объекты существуют
            if (_contentTransform == null || MainContent == null) return;

            try
            {
                _contentTransform.X = 60;
                MainContent.Opacity = 0;

                var slide = new DoubleAnimation(60, 0, TimeSpan.FromMilliseconds(250))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                _contentTransform.BeginAnimation(TranslateTransform.XProperty, slide);
                MainContent.BeginAnimation(UIElement.OpacityProperty, fade);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Animation error: {ex.Message}");
                // Если анимация не удалась, просто показываем контент
                if (MainContent != null)
                    MainContent.Opacity = 1;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                _viewModel.HideWelcomeMessage();
            };
            timer.Start();

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            this.BeginAnimation(OpacityProperty, fadeIn);
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            _session?.EndSession();
            _appState?.Logout();  // ✅ Очищаем AppState
        }

        public void NavigateToPage(int pageIndex)
        {
            OpenPage(pageIndex);
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer?.Stop();
            _viewModel?.Dispose();
            base.OnClosed(e);
        }
    }
}