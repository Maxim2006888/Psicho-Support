using Microsoft.Extensions.DependencyInjection;
using Psicho_Support.Data;
using Psicho_Support.Services;
using System;
using System.Diagnostics;

namespace Psicho_Support.Core
{
    public class AppState
    {
        private Users _currentUser;

        public Users CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    Debug.WriteLine($"[AppState] CurrentUser changed: {value?.Username ?? "null"}");
                    OnUserChanged?.Invoke(this, value);
                }
            }
        }

        public bool IsAuthenticated => CurrentUser != null;

        public event EventHandler<Users> OnUserChanged;
        public event EventHandler OnUserLoggedOut;

        public void Login(Users user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            Debug.WriteLine($"[AppState] Login: {user.Username}");
            CurrentUser = user;

            var session = App.Services.GetRequiredService<AppSession>();
            session.StartSession(user);

            var stateService = App.Services.GetRequiredService<UserStateService>();
            stateService.RecalculateState(user.UserID);
        }

        public void Logout()
        {
            Debug.WriteLine("[AppState] Logout");

            var session = App.Services.GetRequiredService<AppSession>();
            session.EndSession();

            CurrentUser = null;
            OnUserLoggedOut?.Invoke(this, EventArgs.Empty);
        }
    }
}