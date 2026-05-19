// ViewModels/WelcomeViewModel.cs (обновленная версия)
using Psicho_Support.Data;
using Psicho_Support.Enums;
using Psicho_Support.Helpers;
using Psicho_Support.Services;
using Psicho_Support.Core;
using Psicho_Support.Services.Interfaces;
using Psicho_Support.Views.Pages;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Psicho_Support.ViewModels
{
    public class WelcomeViewModel : BaseViewModel, IDisposable
    {
        private readonly UserStateService _stateService;
        private readonly AppSession _appSession;
        private readonly AppState _appState;
        private DispatcherTimer _updateTimer;
        private bool _disposed = false;

        // Команды
        public ICommand CreateNoteCommand { get; }
        public ICommand TakeTestCommand { get; }

        // Конструктор с поддержкой NavigationService
        public WelcomeViewModel(
            IDialogService dialogService,
            INavigationService navigationService,
            AppSession appSession,
            AppState appState,
            UserStateService stateService)
            : base(dialogService, navigationService)
        {
            _appSession = appSession ?? throw new ArgumentNullException(nameof(appSession));
            _appState = appState ?? throw new ArgumentNullException(nameof(appState));
            _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));
            _stateService.StateChanged += OnStateChanged;

            CreateNoteCommand = new RelayCommand(ExecuteCreateNote);
            TakeTestCommand = new RelayCommand(ExecuteTakeTest);

            Title = "Главная";
            LoadUserData();
            LoadDailyQuote();
            LoadStatisticsAsync();
            InitializeCurrentState();
            UpdateAdvice();

            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(30);
            _updateTimer.Tick += (s, e) => LoadStatisticsAsync();
            _updateTimer.Start();
        }

        private void ExecuteCreateNote()
        {
            if (Application.Current.MainWindow is Views.UserWindow mainWindow)
            {
                mainWindow.NavigateToPage(0);
                return;
            }
            NavigationService?.NavigateTo<NotesPage>();
        }

        private void ExecuteTakeTest()
        {
            if (Application.Current.MainWindow is Views.UserWindow mainWindow)
            {
                mainWindow.NavigateToPage(1);
                return;
            }
            NavigationService?.NavigateTo<TestsPage>();
        }

        private void OnStateChanged(int newValue)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StateValue = newValue;
                UpdateStateProperties();
                UpdateAdvice();
            });
        }

        private void LoadUserData()
        {
            if (_appSession.CurrentUser != null)
            {
                UserName = _appSession.CurrentUser.Username ?? "Пользователь";

                var hour = DateTime.Now.Hour;
                if (hour < 12)
                    GreetingMessage = "Доброе утро! Желаем продуктивного дня.";
                else if (hour < 18)
                    GreetingMessage = "Добрый день! Как ваше настроение?";
                else
                    GreetingMessage = "Добрый вечер! Отличное время для рефлексии.";
            }
        }

        private void InitializeCurrentState()
        {
            if (_appState?.CurrentUser == null)
            {
                StateValue = _stateService.CurrentValue;
                UpdateStateProperties();
                return;
            }

            _stateService.RecalculateState(_appState.CurrentUser.UserID);
            StateValue = _stateService.CurrentValue;
            UpdateStateProperties();
        }

        private void LoadDailyQuote()
        {
            var quotes = new[]
            {
                new { Text = "Забота о себе — это не эгоизм, это необходимость.", Author = "未知" },
                new { Text = "Маленькие шаги каждый день приводят к большим результатам.", Author = "未知" },
                new { Text = "Твои чувства важны. Не забывай о них.", Author = "未知" },
                new { Text = "Сегодня — отличный день, чтобы стать лучше, чем вчера.", Author = "未知" },
                new { Text = "Слушай своё сердце, но не забывай включать разум.", Author = "未知" },
                new { Text = "Ты сильнее, чем думаешь. Ты справишься.", Author = "未知" },
                new { Text = "Каждая мысль — это семя. Что ты хочешь вырастить?", Author = "未知" },
                new { Text = "Позволь себе чувствовать. Это делает тебя человеком.", Author = "未知" },
                new { Text = "Сегодняшние заботы — завтрашние истории.", Author = "未知" },
                new { Text = "Ты не один. Мы рядом.", Author = "Команда Psicho Support" }
            };

            int dayOfYear = DateTime.Now.DayOfYear;
            var quote = quotes[dayOfYear % quotes.Length];

            DailyQuote = quote.Text;
            QuoteAuthor = quote.Author;
        }

        private async void LoadStatisticsAsync()
        {
            try
            {
                IsBusy = true;

                if (_appSession.CurrentUser == null) return;

                var userId = _appSession.CurrentUser.UserID;

                // Вычисляем даты за пределами LINQ запроса
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                await System.Threading.Tasks.Task.Run(() =>
                {
                    using (var db = new HealthPsicho_DBEntities())
                    {
                        // Исправленный запрос - используем готовые значения дат
                        int notesCount = db.Notes
                            .Count(n => n.UserID == userId &&
                                   n.CreatedAt >= today &&
                                   n.CreatedAt < tomorrow);

                        int testsCount = db.TestResults
                            .Count(t => t.UserID == userId &&
                                   t.Date >= today &&
                                   t.Date < tomorrow);

                        var todayNotes = db.Notes
                            .Where(n => n.UserID == userId &&
                                   n.CreatedAt >= today &&
                                   n.CreatedAt < tomorrow &&
                                   n.StressLevel.HasValue)
                            .Select(n => n.StressLevel.Value)
                            .ToList();

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            TodayNotesCount = notesCount;
                            TodayTestsCount = testsCount;

                            if (todayNotes.Any())
                            {
                                var avg = todayNotes.Average();
                                AvgStressToday = $"{(int)avg}%";

                                if (avg <= 30)
                                    AvgStressColor = "#4CAF50";
                                else if (avg <= 50)
                                    AvgStressColor = "#FFC107";
                                else
                                    AvgStressColor = "#F44336";
                            }
                            else
                            {
                                AvgStressToday = "Нет данных";
                                AvgStressColor = "#AAAAAA";
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки статистики: {ex.Message}");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TodayNotesCount = 0;
                    TodayTestsCount = 0;
                    AvgStressToday = "Ошибка";
                    AvgStressColor = "#AAAAAA";
                });
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() => IsBusy = false);
            }
        }

        private void UpdateAdvice()
        {
            var currentValue = _stateService.CurrentValue;

            if (currentValue <= 30)
            {
                DailyAdvice = "Ваше состояние требует внимания. Попробуйте сделать глубокий вдох, выпить воды и отдохнуть. Забота о себе сейчас важна как никогда.";
                AdviceSource = "Рекомендация системы";
            }
            else if (currentValue <= 50)
            {
                DailyAdvice = "Неплохо, но можно лучше. Короткая прогулка или медитация помогут улучшить самочувствие.";
                AdviceSource = "Рекомендация системы";
            }
            else if (currentValue <= 70)
            {
                DailyAdvice = "У вас стабильное состояние. Самое время записать свои мысли или поделиться настроением с близкими.";
                AdviceSource = "Рекомендация системы";
            }
            else
            {
                DailyAdvice = "Отличное состояние! Ловите момент и делитесь позитивом. Ваша энергия может вдохновить других.";
                AdviceSource = "Рекомендация системы";
            }
        }

        private void UpdateStateProperties()
        {
            var level = _stateService.CurrentLevel;
            StateLevel = GetLevelName(level);
            StateDescription = _stateService.GetStateDescription(_stateService.CurrentValue);
            StateColor = GetLevelColor(level);
        }

        private string GetLevelName(UserStateLevel level)
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

        private string GetLevelColor(UserStateLevel level)
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

        // Свойства (оставляем без изменений)
        private string _userName;
        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        private string _greetingMessage;
        public string GreetingMessage
        {
            get => _greetingMessage;
            set => SetProperty(ref _greetingMessage, value);
        }

        private int _stateValue;
        public int StateValue
        {
            get => _stateValue;
            set => SetProperty(ref _stateValue, value);
        }

        private string _stateLevel;
        public string StateLevel
        {
            get => _stateLevel;
            set => SetProperty(ref _stateLevel, value);
        }

        private string _stateDescription;
        public string StateDescription
        {
            get => _stateDescription;
            set => SetProperty(ref _stateDescription, value);
        }

        private string _stateColor;
        public string StateColor
        {
            get => _stateColor;
            set => SetProperty(ref _stateColor, value);
        }

        private string _dailyQuote;
        public string DailyQuote
        {
            get => _dailyQuote;
            set => SetProperty(ref _dailyQuote, value);
        }

        private string _quoteAuthor;
        public string QuoteAuthor
        {
            get => _quoteAuthor;
            set => SetProperty(ref _quoteAuthor, value);
        }

        private int _todayNotesCount;
        public int TodayNotesCount
        {
            get => _todayNotesCount;
            set => SetProperty(ref _todayNotesCount, value);
        }

        private int _todayTestsCount;
        public int TodayTestsCount
        {
            get => _todayTestsCount;
            set => SetProperty(ref _todayTestsCount, value);
        }

        private string _avgStressToday;
        public string AvgStressToday
        {
            get => _avgStressToday;
            set => SetProperty(ref _avgStressToday, value);
        }

        private string _avgStressColor;
        public string AvgStressColor
        {
            get => _avgStressColor;
            set => SetProperty(ref _avgStressColor, value);
        }

        private string _dailyAdvice;
        public string DailyAdvice
        {
            get => _dailyAdvice;
            set => SetProperty(ref _dailyAdvice, value);
        }

        private string _adviceSource;
        public string AdviceSource
        {
            get => _adviceSource;
            set => SetProperty(ref _adviceSource, value);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _updateTimer?.Stop();
                _updateTimer = null;

                if (_stateService != null)
                {
                    _stateService.StateChanged -= OnStateChanged;
                }

                _disposed = true;
            }
        }
    }
}