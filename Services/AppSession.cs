using Psicho_Support.Data;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;

namespace Psicho_Support.Services
{
    public class AppSession : INotifyPropertyChanged, IDisposable
    {
        private Users _currentUser;
        private DateTime _sessionStart;
        private Timer _sessionTimer;
        private int? _currentSessionId;
        private bool _disposed;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<UserChangedEventArgs> UserChanged;

        public AppSession()
        {
            _sessionTimer = new Timer(1000);
            _sessionTimer.Elapsed += (s, e) => OnPropertyChanged(nameof(CurrentSessionDuration));
        }

        public Users CurrentUser
        {
            get => _currentUser;
            private set
            {
                if (_currentUser != value)
                {
                    var oldUser = _currentUser;
                    _currentUser = value;
                    OnPropertyChanged();
                    UserChanged?.Invoke(this, new UserChangedEventArgs(oldUser, value));
                }
            }
        }

        public DateTime SessionStart => _sessionStart;

        public TimeSpan CurrentSessionDuration
        {
            get
            {
                if (_sessionStart == DateTime.MinValue || CurrentUser == null)
                    return TimeSpan.Zero;

                return DateTime.Now - _sessionStart;
            }
        }

        public bool IsActive => CurrentUser != null;

        // 🚀 СТАРТ СЕССИИ
        public void StartSession(Users user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (IsActive)
                EndSession();

            CurrentUser = user;
            _sessionStart = DateTime.Now;

            try
            {
                using (var db = new HealthPsicho_DBEntities())
                {
                    var stat = new AppUsageStats
                    {
                        UserID = user.UserID,
                        StartTime = _sessionStart
                    };

                    db.AppUsageStats.Add(stat);
                    db.SaveChanges();

                    _currentSessionId = stat.StatID; // ✅ ВОТ ИСПРАВЛЕНИЕ
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка старта сессии: {ex.Message}");
            }

            _sessionTimer?.Start();
        }

        // 🛑 ЗАВЕРШЕНИЕ СЕССИИ
        public void EndSession()
        {
            if (!IsActive)
                return;

            var endTime = DateTime.Now;
            var duration = endTime - _sessionStart;

            try
            {
                if (_currentSessionId.HasValue)
                {
                    using (var db = new HealthPsicho_DBEntities())
                    {
                        var stat = db.AppUsageStats.Find(_currentSessionId.Value);

                        if (stat != null)
                        {
                            stat.EndTime = endTime;
                            stat.TotalMinutes = (int)Math.Round(duration.TotalMinutes);

                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка завершения сессии: {ex.Message}");
            }

            _currentSessionId = null;
            CurrentUser = null;
            _sessionStart = DateTime.MinValue;

            _sessionTimer?.Stop();
            OnPropertyChanged(nameof(CurrentSessionDuration));
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            EndSession();

            if (_sessionTimer != null)
            {
                _sessionTimer.Stop();
                _sessionTimer.Dispose();
                _sessionTimer = null;
            }

            _disposed = true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class UserChangedEventArgs : EventArgs
    {
        public Users OldUser { get; }
        public Users NewUser { get; }

        public UserChangedEventArgs(Users oldUser, Users newUser)
        {
            OldUser = oldUser;
            NewUser = newUser;
        }
    }
}