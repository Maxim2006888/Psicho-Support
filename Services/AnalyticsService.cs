using Psicho_Support.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Psicho_Support.Services
{
    internal class AnalyticsService
    {
        private readonly HealthPsicho_DBEntities _db;

        public AnalyticsService(HealthPsicho_DBEntities db)
        {
            _db = db;
        }

        /// <summary>
        /// Подсчитывает общее время всех сессий пользователя
        /// </summary>
        public TimeSpan GetTotalTimeSpent(int userId)
        {
            var totalMinutes = _db.AppUsageStats
                .Where(s => s.UserID == userId && s.EndTime != null)
                .Sum(s => (int?)s.TotalMinutes) ?? 0;

            var total = TimeSpan.FromMinutes(totalMinutes);
            total += AppSession.CurrentSessionDuration;


            return total;
        }


        /// <summary>
        /// Подсчитывает количество сессий пользователя
        /// </summary>
        public int GetSessionCount(int userId)
        {
            var count = _db.AppUsageStats
        .Count(s => s.UserID == userId);

            return count;
        }


        /// <summary>
        /// Подсчитывает среднюю длительность сессии
        /// </summary>
        public TimeSpan GetAverageSessionDuration(int userId)
        {
            var completedSessions = _db.AppUsageStats
      .Where(s => s.UserID == userId && s.EndTime.HasValue)
      .ToList();

            var total = completedSessions
                .Sum(s => s.TotalMinutes ?? 0);

            var count = completedSessions.Count;

            if (count == 0)
                return AppSession.CurrentSessionDuration;

            var avg = TimeSpan.FromMinutes(total / count);

            return avg;
        }


        /// <summary>
        /// Подсчитывает активность пользователя за последние 7 дней
        /// </summary>
        public int GetActivityForLast7Days(int userId)
        {
            var sevenDaysAgo = DateTime.Now.AddDays(-7);

            return _db.AppUsageStats
                .Count(s => s.UserID == userId && s.StartTime >= sevenDaysAgo);
        }

    }
}
