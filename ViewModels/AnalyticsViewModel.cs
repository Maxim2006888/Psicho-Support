using Psicho_Support.Enums;
using Psicho_Support.Services;
using Psicho_Support.Services.Interfaces;
using Psicho_Support.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Psicho_Support.Core;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;
using Psicho_Support.Data;

namespace Psicho_Support.ViewModels
{
    public class AnalyticsData
    {
        public TimeSpan TotalTimeSpent { get; set; }
        public int SessionCount { get; set; }
        public TimeSpan AverageSessionDuration { get; set; }
        public int ActivityLast7Days { get; set; }

        public int CurrentStateValue { get; set; }
        public int PreviousStateValue { get; set; }
        public int AverageStressLastWeek { get; set; }
        public int AverageStressPreviousWeek { get; set; }
    }

    public class AnalyticsViewModel : BaseViewModel, IDisposable
    {
        private readonly EmotionMemoryService _memory;
        private readonly EmotionTrendAnalyzer _trend;
        private readonly AnalyticsService _analyticsService;
        private readonly DispatcherTimer _refreshTimer;
        private readonly AppSession _session;
        private readonly AppState _appState;
        private readonly UserStateService _stateService;

        private AnalyticsData _data;

        // ✅ Добавляем недостающие поля
        private bool _isInitialized = false;
        private int _currentUserId = -1;

        // Состояние пользователя
        private int _currentStateValue;
        private string _currentStateLevel;
        private string _currentStateColor;
        private string _stateAdvice;

        // Тенденция
        private string _trendText;
        private string _trendIcon;
        private string _trendColor;
        private string _trendDescription;

        // Прогрессия
        private string _improvementText;
        private string _improvementColor;
        private string _improvementDescription;
        private double _improvementPercentage;
        private double _weekActivityPercentage;

        // Графики
        private ISeries[] _stressSeries;
        private Axis[] _xAxes;
        private Axis[] _yAxes;

        // Секции
        private bool _isSection1Expanded = true;
        private bool _isSection2Expanded = false;
        private bool _isSection3Expanded = false;

        public ISeries[] StressSeries
        {
            get => _stressSeries;
            set => SetProperty(ref _stressSeries, value);
        }

        public Axis[] XAxes
        {
            get => _xAxes;
            set => SetProperty(ref _xAxes, value);
        }

        public Axis[] YAxes
        {
            get => _yAxes;
            set => SetProperty(ref _yAxes, value);
        }

        public ICommand ToggleSection1Command { get; }
        public ICommand ToggleSection2Command { get; }
        public ICommand ToggleSection3Command { get; }

        // ✅ Добавляем свойство IsInitialized
        public bool IsInitialized
        {
            get => _isInitialized;
            set => SetProperty(ref _isInitialized, value);
        }

        public AnalyticsViewModel(
            AnalyticsService analyticsService,
            AppSession session,
            AppState appState,
            UserStateService stateService,
            IDialogService dialogService,
            INavigationService navigationService)
            : base(dialogService, navigationService)
        {
            Title = "Аналитика";

            _analyticsService = analyticsService;
            _session = session;
            _appState = appState ?? throw new ArgumentNullException(nameof(appState));
            _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));

            ToggleSection1Command = new RelayCommand(() => IsSection1Expanded = !IsSection1Expanded);
            ToggleSection2Command = new RelayCommand(() => IsSection2Expanded = !IsSection2Expanded);
            ToggleSection3Command = new RelayCommand(() => IsSection3Expanded = !IsSection3Expanded);

            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30) // ✅ Изменяем на 30 секунд для фонового обновления
            };

            _refreshTimer.Tick += async (s, e) => await RefreshDataAsync();

            RefreshCommand = new RelayCommand(async () => await RefreshDataAsync());

            _appState.OnUserChanged += OnUserChanged;
        }

        public AnalyticsData Data
        {
            get => _data;
            set
            {
                if (SetProperty(ref _data, value))
                {
                    OnPropertyChanged(nameof(TotalTimeFormatted));
                    OnPropertyChanged(nameof(AverageTimeFormatted));
                    OnPropertyChanged(nameof(SessionCount));
                    OnPropertyChanged(nameof(Activity));

                    UpdateStateProperties();
                    UpdateTrendProperties();
                    UpdateImprovementProperties();
                    CalculateWeekActivityPercentage();
                }
            }
        }

        public string TotalTimeFormatted => Data == null ? "0м" : FormatTimeSpan(Data.TotalTimeSpent);
        public string AverageTimeFormatted => Data == null ? "0м" : FormatTimeSpan(Data.AverageSessionDuration);
        public int SessionCount => Data?.SessionCount ?? 0;
        public int Activity => Data?.ActivityLast7Days ?? 0;

        public int CurrentStateValue
        {
            get => _currentStateValue;
            set => SetProperty(ref _currentStateValue, value);
        }

        public string CurrentStateLevel
        {
            get => _currentStateLevel;
            set => SetProperty(ref _currentStateLevel, value);
        }

        public string CurrentStateColor
        {
            get => _currentStateColor;
            set => SetProperty(ref _currentStateColor, value);
        }

        public string StateAdvice
        {
            get => _stateAdvice;
            set => SetProperty(ref _stateAdvice, value);
        }

        public string TrendText
        {
            get => _trendText;
            set => SetProperty(ref _trendText, value);
        }

        public string TrendIcon
        {
            get => _trendIcon;
            set => SetProperty(ref _trendIcon, value);
        }

        public string TrendColor
        {
            get => _trendColor;
            set => SetProperty(ref _trendColor, value);
        }

        public string TrendDescription
        {
            get => _trendDescription;
            set => SetProperty(ref _trendDescription, value);
        }

        public string ImprovementText
        {
            get => _improvementText;
            set => SetProperty(ref _improvementText, value);
        }

        public string ImprovementColor
        {
            get => _improvementColor;
            set => SetProperty(ref _improvementColor, value);
        }

        public string ImprovementDescription
        {
            get => _improvementDescription;
            set => SetProperty(ref _improvementDescription, value);
        }

        public double ImprovementPercentage
        {
            get => _improvementPercentage;
            set => SetProperty(ref _improvementPercentage, value);
        }

        public double WeekActivityPercentage
        {
            get => _weekActivityPercentage;
            set => SetProperty(ref _weekActivityPercentage, value);
        }

        public ICommand RefreshCommand { get; }

        public bool IsSection1Expanded
        {
            get => _isSection1Expanded;
            set => SetProperty(ref _isSection1Expanded, value);
        }

        public bool IsSection2Expanded
        {
            get => _isSection2Expanded;
            set => SetProperty(ref _isSection2Expanded, value);
        }

        public bool IsSection3Expanded
        {
            get => _isSection3Expanded;
            set => SetProperty(ref _isSection3Expanded, value);
        }

        public int PreviousWeekStress { get; set; }
        public int CurrentWeekStress { get; set; }

        private void OnUserChanged(object sender, Users user)
        {
            // При смене пользователя обновляем данные
            if (user != null)
            {
                _ = LoadAnalyticsAsync();
            }
        }

        public override async Task InitializeAsync(object parameter = null)
        {
            // Загружаем данные только если они ещё не загружены или пользователь изменился
            if (!_isInitialized || _currentUserId != _appState.CurrentUser?.UserID)
            {
                await LoadAnalyticsAsync();
                _isInitialized = true;
                _currentUserId = _appState.CurrentUser?.UserID ?? -1;
            }

            // Запускаем таймер фонового обновления
            if (!_refreshTimer.IsEnabled)
            {
                _refreshTimer.Start();
            }
        }

        private async Task RefreshDataAsync()
        {
            // Фоновое обновление данных
            if (_appState.IsAuthenticated && !IsBusy)
            {
                await LoadAnalyticsAsync();
            }
        }

        private async Task LoadAnalyticsAsync()
        {
            if (!_appState.IsAuthenticated || IsBusy)
                return;

            IsBusy = true;

            try
            {
                var userId = _appState.CurrentUser.UserID;

                // 🔹 Загружаем данные в фоне
                var data = await Task.Run(() =>
                {
                    return new AnalyticsData
                    {
                        TotalTimeSpent = _analyticsService.GetTotalTimeSpent(userId),
                        SessionCount = _analyticsService.GetSessionCount(userId),
                        AverageSessionDuration = _analyticsService.GetAverageSessionDuration(userId),
                        ActivityLast7Days = _analyticsService.GetActivityForLast7Days(userId),

                        // 🔥 текущее состояние берём напрямую
                        CurrentStateValue = _stateService.CurrentValue,

                        PreviousStateValue = _analyticsService.GetPreviousStateValue(userId),
                        AverageStressLastWeek = _analyticsService.GetAverageStressLastWeek(userId),
                        AverageStressPreviousWeek = _analyticsService.GetAverageStressPreviousWeek(userId)
                    };
                });

                // 🔹 Обновляем UI
                Data = data;
                PreviousWeekStress = data.AverageStressPreviousWeek;
                CurrentWeekStress = data.AverageStressLastWeek;

                // 🔥 График отдельно (не блокируем основной поток)
                _ = Task.Run(() => LoadStressChart(userId));

                // 🔥 (опционально) — сюда можно добавить:
                // ModelConfidence = _memory.GetConfidence(userId);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadAnalyticsAsync error: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void LoadStressChart(int userId)
        {
            try
            {
                var data = _analyticsService.GetStressDynamics(userId, 7);

                if (data == null || !data.Any())
                {
                    System.Diagnostics.Debug.WriteLine("No stress data available");
                    return;
                }

                StressSeries = new ISeries[]
                {
                    new LineSeries<double>
                    {
                        Values = data.Select(d => (double)d.Stress).ToArray(),
                        GeometrySize = 8,
                        LineSmoothness = 0.6,
                        Fill = null,
                        Stroke = new SolidColorPaint(SKColor.Parse("#5A4FCF"), 2),
                        GeometryStroke = new SolidColorPaint(SKColor.Parse("#5A4FCF")),
                        GeometryFill = new SolidColorPaint(SKColor.Parse("#5A4FCF")),
                        Name = "Уровень стресса"
                    }
                };

                XAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = data.Select(d => d.DayOfWeek).ToArray(),
                        LabelsRotation = 0,
                        Name = "День недели",
                        NamePaint = new SolidColorPaint(SKColor.Parse("#AAAAAA")),
                        LabelsPaint = new SolidColorPaint(SKColor.Parse("#AAAAAA")),
                        TicksPaint = new SolidColorPaint(SKColor.Parse("#555555"))
                    }
                };

                YAxes = new Axis[]
                {
                    new Axis
                    {
                        MinLimit = 0,
                        MaxLimit = 100,
                        Name = "Уровень стресса (%)",
                        NamePaint = new SolidColorPaint(SKColor.Parse("#AAAAAA")),
                        LabelsPaint = new SolidColorPaint(SKColor.Parse("#AAAAAA")),
                        TicksPaint = new SolidColorPaint(SKColor.Parse("#555555"))
                    }
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadStressChart error: {ex.Message}");
            }
        }

        private void UpdateStateProperties()
        {
            if (Data == null) return;

            CurrentStateValue = Data.CurrentStateValue;
            CurrentStateLevel = GetStateLevelName(CalculateLevel(CurrentStateValue));
            CurrentStateColor = GetStateLevelColor(CalculateLevel(CurrentStateValue));
            StateAdvice = GetStateAdvice(CurrentStateValue);
        }

        private void UpdateTrendProperties()
        {
            if (Data == null) return;

            int current = Data.AverageStressLastWeek;
            int previous = Data.AverageStressPreviousWeek;

            int difference = current - previous;
            int percentChange = previous > 0 ? (int)((double)difference / previous * 100) : 0;

            if (Math.Abs(difference) < 5)
            {
                TrendText = "Стабильно";
                TrendIcon = "➡️";
                TrendColor = "#FFC107";
                TrendDescription = "Уровень стресса остаётся стабильным. Продолжайте в том же духе!";
            }
            else if (difference < 0)
            {
                TrendText = $"↓ {Math.Abs(percentChange)}%";
                TrendIcon = "📉";
                TrendColor = "#4CAF50";
                TrendDescription = "Уровень стресса снижается. Отличная динамика! Так держать!";
            }
            else
            {
                TrendText = $"↑ {percentChange}%";
                TrendIcon = "📈";
                TrendColor = "#F44336";
                TrendDescription = "Уровень стресса повышается. Рекомендуется обратить внимание на отдых.";
            }
        }

        private void UpdateImprovementProperties()
        {
            if (Data == null) return;

            int improvement = Data.PreviousStateValue - Data.CurrentStateValue;
            ImprovementPercentage = Math.Max(0, Math.Min(100, improvement + 50));

            if (improvement > 20)
            {
                ImprovementText = $"🏆 +{improvement}%";
                ImprovementColor = "#4CAF50";
                ImprovementDescription = "Значительное улучшение! Вы отлично работаете над собой!";
            }
            else if (improvement > 0)
            {
                ImprovementText = $"📈 +{improvement}%";
                ImprovementColor = "#8BC34A";
                ImprovementDescription = "Есть положительная динамика. Продолжайте!";
            }
            else if (improvement == 0)
            {
                ImprovementText = "➡️ 0%";
                ImprovementColor = "#FFC107";
                ImprovementDescription = "Состояние стабильное. Работайте над собой дальше.";
            }
            else
            {
                ImprovementText = $"📉 {improvement}%";
                ImprovementColor = "#FF9800";
                ImprovementDescription = "Требуется внимание. Попробуйте техники релаксации.";
            }
        }

        private void CalculateWeekActivityPercentage()
        {
            if (Data == null) return;
            WeekActivityPercentage = (Data.ActivityLast7Days / 7.0) * 100;
        }

        private string GetStateAdvice(int value)
        {
            if (value >= 80)
                return "Отличное состояние! Поддерживайте его с помощью заметок и тестов.";
            if (value >= 60)
                return "Хорошее состояние. Продолжайте следить за эмоциями.";
            if (value >= 40)
                return "Среднее состояние. Сделайте перерыв или запишите свои мысли.";
            if (value >= 20)
                return "Пониженное состояние. Попробуйте дыхательные упражнения.";
            return "Требуется внимание. Обратитесь к специалисту или позвоните на линию помощи.";
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

        private UserStateLevel CalculateLevel(int value)
        {
            if (value <= 20) return UserStateLevel.Critical;
            if (value <= 40) return UserStateLevel.Low;
            if (value <= 60) return UserStateLevel.Stable;
            if (value <= 80) return UserStateLevel.Good;
            return UserStateLevel.Excellent;
        }

        private string FormatTimeSpan(TimeSpan ts)
        {
            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours}ч {ts.Minutes}м";
            return $"{ts.Minutes}м {ts.Seconds}с";
        }

        public void Dispose()
        {
            _refreshTimer?.Stop();
            if (_appState != null)
            {
                _appState.OnUserChanged -= OnUserChanged;
            }
        }
    }
}