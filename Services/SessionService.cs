using Psicho_Support.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Psicho_Support.Services
{
    internal class SessionService
    {
        private readonly HealthPsicho_DBEntities _db;
        private AppUsageStats _currentSession;

        public DateTime StartTime { get; private set; }

        public SessionService(HealthPsicho_DBEntities db)
        {
            _db = db;
        }

        /// <summary>
        /// Запуск пользовательской сессии
        /// </summary>
        public void Start(int userId)
        {
            StartTime = DateTime.Now;

            _currentSession = new AppUsageStats
            {
                UserID = userId,
                StartTime = StartTime,
                EndTime = null,
                TotalMinutes = 0
            };

            _db.AppUsageStats.Add(_currentSession);
            _db.SaveChanges();
        }

        /// <summary>
        /// Завершение сессии
        /// </summary>
        public void Stop(TimeSpan duration)
        {
            if (_currentSession == null)
                return;

            _currentSession.EndTime = DateTime.Now;
            _currentSession.TotalMinutes = (int)duration.TotalMinutes;
            _db.SaveChanges();
        }
    }
}