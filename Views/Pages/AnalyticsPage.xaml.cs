using Psicho_Support.Services;
using System;
using System.Windows.Threading;

namespace Psicho_Support.Views.Pages
{
    public partial class AnalyticsPage : BasePage
    {
        private readonly AnalyticsService _analyticsService;
        private DispatcherTimer _liveTimer;

        public AnalyticsPage()
        {
            InitializeComponent();

            _liveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _liveTimer.Tick += (s, e) => LoadAnalytics();
            _liveTimer.Start();


            if (CurrentUser == null)
            {
                ShowErrorMessage("Пользователь не определен. Пожалуйста, войдите заново.");
                return;
            }

            _analyticsService = new AnalyticsService(AppSession.Db);
            LoadAnalytics();
        }

        private void LoadAnalytics()
        {
            try
            {
                // Используем CurrentUser из BasePage (который берет из AppSession)
                var user = CurrentUser;

                var totalTimeSpent = _analyticsService.GetTotalTimeSpent(user.UserID);
                var sessionCount = _analyticsService.GetSessionCount(user.UserID);
                var averageSessionDuration = _analyticsService.GetAverageSessionDuration(user.UserID);
                var activityLast7Days = _analyticsService.GetActivityForLast7Days(user.UserID);


                TotalTimeSpentText.Text = $"Общее время в приложении: {FormatTimeSpan(totalTimeSpent)}";
                SessionCountText.Text = $"Количество сессий: {sessionCount}";
                AverageSessionDurationText.Text = $"Средняя длительность сессии: {FormatTimeSpan(averageSessionDuration)}";
                ActivityLast7DaysText.Text = $"Активность за последние 7 дней: {activityLast7Days}";
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка загрузки аналитики: {ex.Message}");
            }
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours}ч {timeSpan.Minutes}мин";
            else if (timeSpan.TotalMinutes >= 1)
                return $"{timeSpan.Minutes}мин {timeSpan.Seconds}сек";
            else
                return $"{timeSpan.Seconds}сек";
        }

        private void ShowErrorMessage(string message)
        {
            TotalTimeSpentText.Text = message;
            SessionCountText.Text = "";
            AverageSessionDurationText.Text = "";
            ActivityLast7DaysText.Text = "";


            System.Diagnostics.Debug.WriteLine($"AnalyticsPage Error: {message}");
        }
    }
}