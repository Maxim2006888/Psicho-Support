using Psicho_Support.Data;
using Psicho_Support.Enums;
using System;
using System.Linq;

namespace Psicho_Support.Services
{
    public class UserStateService : IDisposable
    {
        private readonly HealthPsicho_DBEntities _db;
        private int _currentValue;
        private bool _disposed = false;

        public int CurrentValue => _currentValue;

        public UserStateLevel CurrentLevel => CalculateLevel(_currentValue);

        public event Action<int> StateChanged;

        public UserStateService()
        {
            _db = new HealthPsicho_DBEntities();
        }

        public void Initialize(int userId)
        {
            RecalculateState(userId);
        }

        public void RecalculateState(int userId)
        {
            var notes = _db.Notes
                .Where(n => n.UserID == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToList();

            if (!notes.Any())
            {
                UpdateState(75);
                return;
            }

            var stressNotes = notes
                .Where(n => n.StressLevel.HasValue)
                .Select(n => n.StressLevel.Value)
                .ToList();

            if (!stressNotes.Any())
            {
                UpdateState(75);
                return;
            }

            double avgStress = stressNotes.Average();

            var recentNotes = stressNotes.Take(3).ToList();
            double recentAvg = recentNotes.Any() ? recentNotes.Average() : avgStress;

            double finalAvg = (avgStress * 0.6) + (recentAvg * 0.4);
            int indicatorValue = 100 - (int)finalAvg;

            if (_currentValue > 0)
            {
                indicatorValue = (int)((_currentValue * 0.3) + (indicatorValue * 0.7));
            }

            UpdateState(indicatorValue);
        }

        public void UpdateState(int newValue)
        {
            newValue = Clamp(newValue);

            if (_currentValue == newValue)
                return;

            _currentValue = newValue;

            StateChanged?.Invoke(_currentValue);
        }

        private int Clamp(int value)
        {
            if (value < 0) return 0;
            if (value > 100) return 100;
            return value;
        }

        private UserStateLevel CalculateLevel(int value)
        {
            if (value <= 20) return UserStateLevel.Critical;
            if (value <= 40) return UserStateLevel.Low;
            if (value <= 60) return UserStateLevel.Stable;
            if (value <= 80) return UserStateLevel.Good;
            return UserStateLevel.Excellent;
        }

        public string GetStateDescription(int value)
        {
            var level = CalculateLevel(value);

            switch (level)
            {
                case UserStateLevel.Critical:
                    return "Критическое состояние. Рекомендуется обратиться к специалисту.";
                case UserStateLevel.Low:
                    return "Пониженное состояние. Уделите время отдыху.";
                case UserStateLevel.Stable:
                    return "Стабильное состояние. Вы держитесь хорошо.";
                case UserStateLevel.Good:
                    return "Хорошее состояние. Позитивный настрой.";
                case UserStateLevel.Excellent:
                    return "Отличное состояние! Так держать!";
                default:
                    return "Состояние в норме.";
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _db?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}