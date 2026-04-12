using Psicho_Support.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Psicho_Support.Services
{
    // Модель для динамики стресса
    public class StressPoint
    {
        public DateTime Date { get; set; }
        public int Stress { get; set; }
        public string DayOfWeek { get; set; }
    }

    public class AnalyticsService
    {
        private readonly AppSession _appSession;

        public AnalyticsService(AppSession appSession)
        {
            _appSession = appSession ?? throw new ArgumentNullException(nameof(appSession));
        }

        public TimeSpan GetTotalTimeSpent(int userId)
        {
            try
            {
                using (var db = new HealthPsicho_DBEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var totalMinutes = db.AppUsageStats
                        .Where(s => s.UserID == userId && s.TotalMinutes.HasValue)
                        .Sum(s => (double?)s.TotalMinutes) ?? 0;

                    return TimeSpan.FromMinutes(totalMinutes);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTotalTimeSpent error: {ex.Message}");
                return TimeSpan.Zero;
            }
        }

        public int GetSessionCount(int userId)
        {
            try
            {
                using (var db = new HealthPsicho_DBEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    return db.AppUsageStats
                        .Count(s => s.UserID == userId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetSessionCount error: {ex.Message}");
                return 0;
            }
        }

        public TimeSpan GetAverageSessionDuration(int userId)
        {
            try
            {
                using (var db = new HealthPsicho_DBEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var completedSessions = db.AppUsageStats
                        .Where(s => s.UserID == userId && s.StartTime != null && s.EndTime != null)
                        .ToList();

                    double totalMinutes = 0;
                    int count = completedSessions.Count;

                    foreach (var session in completedSessions)
                    {
                        if (session.StartTime.HasValue && session.EndTime.HasValue)
                        {
                            totalMinutes += (session.EndTime.Value - session.StartTime.Value).TotalMinutes;
                        }
                    }

                    if (_appSession.IsActive)
                    {
                        totalMinutes += _appSession.CurrentSessionDuration.TotalMinutes;
                        count++;
                    }

                    if (count == 0)
                        return TimeSpan.Zero;

                    return TimeSpan.FromMinutes(totalMinutes / count);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAverageSessionDuration error: {ex.Message}");
                return TimeSpan.Zero;
            }
        }

        public int GetActivityForLast7Days(int userId)
        {
            try
            {
                var fromDate = DateTime.Now.AddDays(-7);

                using (var db = new HealthPsicho_DBEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var result = db.AppUsageStats
                        .Where(s => s.UserID == userId && s.StartTime >= fromDate)
                        .Select(s => DbFunctions.TruncateTime(s.StartTime))
                        .Distinct()
                        .Count();

                    return result;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetActivityForLast7Days error: {ex.Message}");
                return 0;
            }
        }

        public int GetPreviousStateValue(int userId)
        {
            try
            {
                using (var db = new HealthPsicho_DBEntities())
                {
                    var weekAgo = DateTime.Now.AddDays(-7);

                    var notes = db.Notes
                        .Where(n => n.UserID == userId && n.CreatedAt <= weekAgo && n.StressLevel.HasValue)
                        .OrderByDescending(n => n.CreatedAt)
                        .Take(5)
                        .ToList();

                    if (!notes.Any()) return 50;

                    double avgStress = notes.Average(n => n.StressLevel.Value);
                    return 100 - (int)avgStress;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetPreviousStateValue error: {ex.Message}");
                return 50;
            }
        }

        public int GetAverageStressLastWeek(int userId)
        {
            try
            {
                using (var db = new HealthPsicho_DBEntities())
                {
                    var weekAgo = DateTime.Now.AddDays(-7);

                    var notes = db.Notes
                        .Where(n => n.UserID == userId && n.CreatedAt >= weekAgo && n.StressLevel.HasValue)
                        .ToList();

                    if (!notes.Any()) return 50;

                    double avgStress = notes.Average(n => n.StressLevel.Value);
                    return (int)avgStress;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAverageStressLastWeek error: {ex.Message}");
                return 50;
            }
        }

        public int GetAverageStressPreviousWeek(int userId)
        {
            try
            {
                using (var db = new HealthPsicho_DBEntities())
                {
                    var endDate = DateTime.Now.AddDays(-7);
                    var startDate = endDate.AddDays(-7);

                    var notes = db.Notes
                        .Where(n => n.UserID == userId && n.CreatedAt >= startDate && n.CreatedAt <= endDate && n.StressLevel.HasValue)
                        .ToList();

                    if (!notes.Any()) return 50;

                    double avgStress = notes.Average(n => n.StressLevel.Value);
                    return (int)avgStress;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAverageStressPreviousWeek error: {ex.Message}");
                return 50;
            }
        }

        /// <summary>
        /// Получает динамику стресса за указанное количество дней
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="days">Количество дней (по умолчанию 7)</param>
        /// <returns>Список точек стресса по дням</returns>
        public List<StressPoint> GetStressDynamics(int userId, int days = 7)
        {
            try
            {
                using (var db = new HealthPsicho_DBEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var startDate = DateTime.Now.AddDays(-days + 1).Date;
                    var endDate = DateTime.Now.Date;

                    // Получаем все заметки за период
                    var notes = db.Notes
                        .Where(n => n.UserID == userId
                                 && n.CreatedAt >= startDate
                                 && n.CreatedAt <= endDate.AddDays(1)
                                 && n.StressLevel.HasValue)
                        .OrderBy(n => n.CreatedAt)
                        .ToList();

                    // Создаём словарь для группировки по дням
                    var stressByDay = new Dictionary<DateTime, List<int>>();

                    // Инициализируем все дни
                    for (int i = 0; i < days; i++)
                    {
                        var currentDate = startDate.AddDays(i);
                        stressByDay[currentDate] = new List<int>();
                    }

                    // Группируем заметки по дням
                    foreach (var note in notes)
                    {
                        var noteDate = note.CreatedAt.Value.Date;
                        if (stressByDay.ContainsKey(noteDate))
                        {
                            stressByDay[noteDate].Add(note.StressLevel.Value);
                        }
                    }

                    // Вычисляем средний стресс за каждый день
                    var result = new List<StressPoint>();
                    foreach (var day in stressByDay)
                    {
                        int avgStress = day.Value.Any()
                            ? (int)day.Value.Average()
                            : 50; // Если нет заметок, ставим нейтральное значение

                        result.Add(new StressPoint
                        {
                            Date = day.Key,
                            Stress = avgStress,
                            DayOfWeek = GetDayOfWeekName(day.Key.DayOfWeek)
                        });
                    }

                    return result.OrderBy(r => r.Date).ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetStressDynamics error: {ex.Message}");
                return new List<StressPoint>();
            }
        }

        /// <summary>
        /// Получает динамику стресса за последние 7 дней с усреднением по дням
        /// </summary>
        public List<StressPoint> GetWeeklyStressDynamics(int userId)
        {
            return GetStressDynamics(userId, 7);
        }

        /// <summary>
        /// Получает динамику стресса за последние 30 дней
        /// </summary>
        public List<StressPoint> GetMonthlyStressDynamics(int userId)
        {
            return GetStressDynamics(userId, 30);
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
    }
}