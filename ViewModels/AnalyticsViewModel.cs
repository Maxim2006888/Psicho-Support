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
using Psicho_Support.Models;

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
        private readonly TimelineAnalyticsService _timelineAnalytics;
        private string _stressDynamicsDebugSummary;

        private AnalyticsData _data;

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
        private string _chartDebugText;
        private bool _hasChartData;

        // Секции
        private bool _isSection1Expanded = true;
        private bool _isSection2Expanded = false;
        private bool _isSection3Expanded = false;

        private int _todayNotesCount;
        private int _todayTestsCount;
        private string _phaseAccentColor = "#5A4FCF";
        private double _phaseGlowOpacity = 0.22;
        private double _phaseAnimationSpeed = 0.6;
        private int _previousWeekStress;
        private int _currentWeekStress;

        public string ChartDebugText
        {
            get => _chartDebugText;
            set => SetProperty(ref _chartDebugText, value);

        }

        public bool HasChartData
        {
            get => _hasChartData;
            set => SetProperty(ref _hasChartData, value);
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

        public int TodayNotesCount
        {
            get => _todayNotesCount;
            set => SetProperty(ref _todayNotesCount, value);
        }

        public int TodayTestsCount
        {
            get => _todayTestsCount;
            set => SetProperty(ref _todayTestsCount, value);
        }

        public string PhaseAccentColor
        {
            get => _phaseAccentColor;
            set => SetProperty(ref _phaseAccentColor, value);
        }

        public double PhaseGlowOpacity
        {
            get => _phaseGlowOpacity;
            set => SetProperty(ref _phaseGlowOpacity, value);
        }

        public double PhaseAnimationSpeed
        {
            get => _phaseAnimationSpeed;
            set => SetProperty(ref _phaseAnimationSpeed, value);
        }

        public int PreviousWeekStress
        {
            get => _previousWeekStress;
            set => SetProperty(ref _previousWeekStress, value);
        }

        public int CurrentWeekStress
        {
            get => _currentWeekStress;
            set => SetProperty(ref _currentWeekStress, value);
        }

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

        public bool IsInitialized
        {
            get => _isInitialized;
            set => SetProperty(ref _isInitialized, value);
        }

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



        public ICommand ToggleSection1Command { get; }
        public ICommand ToggleSection2Command { get; }
        public ICommand ToggleSection3Command { get; }
        public ICommand RefreshCommand { get; }

        public AnalyticsViewModel(
            AnalyticsService analyticsService,
            AppSession session,
            AppState appState,
            UserStateService stateService,
            TimelineAnalyticsService timelineAnalytics,
            IDialogService dialogService,
            INavigationService navigationService)
            : base(dialogService, navigationService)
        {
            Title = "Аналитика";

            _analyticsService = analyticsService;
            _session = session;
            _appState = appState ?? throw new ArgumentNullException(nameof(appState));
            _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));
            _timelineAnalytics = timelineAnalytics ?? throw new ArgumentNullException(nameof(timelineAnalytics));

            // Инициализация полей (можно передать через DI, если они зарегистрированы)
            _memory = null; // Замените на реальную инициализацию
            _trend = null;  // Замените на реальную инициализацию

            ToggleSection1Command = new RelayCommand(() => IsSection1Expanded = !IsSection1Expanded);
            ToggleSection2Command = new RelayCommand(() => IsSection2Expanded = !IsSection2Expanded);
            ToggleSection3Command = new RelayCommand(() => IsSection3Expanded = !IsSection3Expanded);

            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };

            _refreshTimer.Tick += async (s, e) => await RefreshDataAsync();

            RefreshCommand = new RelayCommand(async () => await RefreshDataAsync());

            if (_appState != null)
            {
                _appState.OnUserChanged += OnUserChanged;
            }
        }

        private void OnUserChanged(object sender, Users user)
        {
            if (user != null)
            {
                _ = LoadAnalyticsAsync();
            }
        }

        public override async Task InitializeAsync(object parameter = null)
        {
            if (!_isInitialized || _currentUserId != _appState?.CurrentUser?.UserID)
            {
                await LoadAnalyticsAsync();
                _isInitialized = true;
                _currentUserId = _appState?.CurrentUser?.UserID ?? -1;
            }

            if (!_refreshTimer.IsEnabled)
            {
                _refreshTimer.Start();
            }
        }

        private async Task RefreshDataAsync()
        {
            if (_appState != null && _appState.IsAuthenticated && !IsBusy)
            {
                await LoadAnalyticsAsync();
            }
        }

        private async Task LoadAnalyticsAsync()
        {
            if (_appState == null || !_appState.IsAuthenticated || IsBusy)
                return;

            IsBusy = true;

            try
            {
                var userId = _appState.CurrentUser.UserID;
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                var todayCounters = await Task.Run(() =>
                {
                    using (var db = new HealthPsicho_DBEntities())
                    {
                        var notes = db.Notes.Count(n => n.UserID == userId && n.CreatedAt >= today && n.CreatedAt < tomorrow);
                        var tests = db.TestResults.Count(t => t.UserID == userId && t.Date >= today && t.Date < tomorrow);
                        return (notes, tests);
                    }
                });

                TodayNotesCount = todayCounters.notes;
                TodayTestsCount = todayCounters.tests;   

                var data = await Task.Run(() =>
                {
                    return new AnalyticsData
                    {
                        TotalTimeSpent = _analyticsService.GetTotalTimeSpent(userId),
                        SessionCount = _analyticsService.GetSessionCount(userId),
                        AverageSessionDuration = _analyticsService.GetAverageSessionDuration(userId),
                        ActivityLast7Days = _analyticsService.GetActivityForLast7Days(userId),
                        CurrentStateValue = _analyticsService.GetCurrentStateValue(userId),
                        PreviousStateValue = _analyticsService.GetPreviousStateValue(userId),
                        AverageStressLastWeek = _analyticsService.GetAverageStressLastWeek(userId),
                        AverageStressPreviousWeek = _analyticsService.GetAverageStressPreviousWeek(userId)
                    };
                });

                Data = data;

                var profile = await Task.Run(() => _timelineAnalytics.BuildProfile(userId, 7));
                PreviousWeekStress = data.AverageStressPreviousWeek;
                CurrentWeekStress = (int)Math.Round(profile.AverageStress);
                StateAdvice = BuildProfileAdvice(profile);


                var stressDiagnostics = await Task.Run(() => _analyticsService.GetStressDynamicsDebugSummary(userId, 7)); 
                var stressData = await Task.Run(() => _timelineAnalytics.BuildStressDynamics(userId, 7));

                _stressDynamicsDebugSummary = stressDiagnostics;
                UpdateStressChart(stressData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadAnalyticsAsync error: {ex}");
                TodayNotesCount = 0;
                TodayTestsCount = 0;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void UpdateStressChart(List<StressPoint> data)
        {
            try
            {

                System.Diagnostics.Debug.WriteLine($"📊 UpdateStressChart: получено {(data?.Count ?? 0)} точек");


                var sourceData = data ?? new List<StressPoint>();
                var chartData = sourceData
                    .Where(d => d.HasData) 
                    .OrderBy(d => d.Date)
                    .ToList();

                var sourceCount = sourceData.Count;
                var realCount = chartData.Count;

                if (!chartData.Any())
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Нет данных для графика, создаем заглушку");
                    HasChartData = false;
                    StressSeries = new ISeries[0];
                    XAxes = new Axis[0];
                    YAxes = new Axis[0];
                    ChartDebugText = $"Источник: {sourceCount} дней, с данными: 0. Нет заметок с заполненным StressLevel за последние 7 дней. {_stressDynamicsDebugSummary}";
                    System.Diagnostics.Debug.WriteLine($"Analytics chart data: {ChartDebugText}");
                    return;
                }

                HasChartData = true;

                var stateValues = chartData
                    .Select(d => (double)(100 - d.Stress))
                    .ToArray();

                ChartDebugText = $"Источник: {sourceCount} дней, с данными: {realCount}, точек на графике: {stateValues.Length}, значения: {string.Join(", ", stateValues.Select(v => v.ToString("F0")))}. {_stressDynamicsDebugSummary}";
                System.Diagnostics.Debug.WriteLine($"Analytics chart data: {ChartDebugText}");

                StressSeries = new ISeries[]
                {
                    new LineSeries<double>
                    {
                        Values = stateValues,
                        GeometrySize = chartData.Count == 1 ? 12 : 9,
                        LineSmoothness = 0.4,
                        Fill = new SolidColorPaint(new SKColor(90, 79, 207, 35)),
                        Stroke = new SolidColorPaint(SKColor.Parse("#5A4FCF"), 3),
                        GeometryStroke = new SolidColorPaint(SKColor.Parse("#FFFFFF"), 2),
                        GeometryFill = new SolidColorPaint(SKColor.Parse("#5A4FCF")),
                        Name = "Состояние"
                    }
                };


                XAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = chartData.Select(d => d.DayOfWeek).ToArray(),
                        LabelsRotation = 0,
                        LabelsPaint = new SolidColorPaint(SKColor.Parse("#C2C2D6")),
                        TextSize = 11,
                        SeparatorsPaint = null,
                        TicksPaint = new SolidColorPaint(new SKColor(255, 255, 255, 35))
                    }
                };


                YAxes = new Axis[]
                {
                    new Axis
                    {
                        MinLimit = 0,
                        MaxLimit = 100,
                        MinStep = 20,
                        Name = "Состояние, %",
                        NamePaint = new SolidColorPaint(SKColor.Parse("#AAAAAA")),
                        LabelsPaint = new SolidColorPaint(SKColor.Parse("#AAAAAA")),
                        SeparatorsPaint = new SolidColorPaint(new SKColor(255, 255, 255, 24)),
                        TicksPaint = new SolidColorPaint(SKColor.Parse("#555555"))
                    }
                };
            

                

                
                OnPropertyChanged(nameof(StressSeries));
                OnPropertyChanged(nameof(XAxes));
                OnPropertyChanged(nameof(YAxes));
                

                System.Diagnostics.Debug.WriteLine($"✅ График обновлен: {chartData.Count} точек");
            }
            catch (Exception ex)
            {
                HasChartData = false;
                ChartDebugText = $"Ошибка подготовки графика: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"UpdateStressChart error: {ex}");




                StressSeries = new ISeries[]
                {
                    new LineSeries<double>
                    {
                        Values = new double[] { 0 },
                        GeometrySize = 0,
                        Fill = null,
                        Stroke = new SolidColorPaint(SKColor.Parse("#5A4FCF"), 2),
                        Name = "Ошибка загрузки"
                    }
                };

                XAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = new[] { "Нет данных" },
                        LabelsPaint = new SolidColorPaint(SKColor.Parse("#AAAAAA"))
                    }
                };

                YAxes = new Axis[]
                {
                    new Axis
                    {
                        MinLimit = 0,
                        MaxLimit = 100,
                        LabelsPaint = new SolidColorPaint(SKColor.Parse("#AAAAAA"))
                    }
                };
            }
        }

        private string GetDayOfWeekName(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday: return "Пн";
                case DayOfWeek.Tuesday: return "Вт";
                case DayOfWeek.Wednesday: return "Ср";
                case DayOfWeek.Thursday: return "Чт";
                case DayOfWeek.Friday: return "Пт";
                case DayOfWeek.Saturday: return "Сб";
                case DayOfWeek.Sunday: return "Вс";
                default: return day.ToString();
            }
        }

        private void ApplyPhaseVisuals(EmotionalPhase phase)
        {
            switch (phase)
            {
                case EmotionalPhase.BurnoutRisk:
                    PhaseAccentColor = "#6FA8FF";
                    PhaseGlowOpacity = 0.12;
                    PhaseAnimationSpeed = 0.35;
                    break;
                case EmotionalPhase.Tension:
                    PhaseAccentColor = "#7B74E8";
                    PhaseGlowOpacity = 0.18;
                    PhaseAnimationSpeed = 0.45;
                    break;
                case EmotionalPhase.Recovery:
                    PhaseAccentColor = "#64C7B8";
                    PhaseGlowOpacity = 0.24;
                    PhaseAnimationSpeed = 0.65;
                    break;
                default:
                    PhaseAccentColor = "#8B7CFF";
                    PhaseGlowOpacity = 0.28;
                    PhaseAnimationSpeed = 0.75;
                    break;
            }
        }

        private string BuildProfileAdvice(EmotionalProfile profile)
        {
            if (profile == null) return StateAdvice;

            return $"Фаза: {profile.CurrentPhase} · Стабильность: {profile.StabilityIndex:F0}% · " +
                   $"Колебания: {profile.FluctuationIndex:F0}% · Риск выгорания: {profile.BurnoutRisk:F0}%\n" +
                   $"Почему: {profile.PredictionReason}";
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