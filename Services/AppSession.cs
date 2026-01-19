using Psicho_Support.Data;
using Psicho_Support.Services;
using System;

namespace Psicho_Support
{
    public static class AppSession
    {
        public static Users CurrentUser { get; private set; }
        public static HealthPsicho_DBEntities Db { get; private set; }
        private static SessionService _sessionService;
        private static DateTime _startTime;


        public static TimeSpan CurrentSessionDuration => CurrentUser == null ? TimeSpan.Zero : DateTime.Now - _startTime;

        public static void Initialize(Users user)
        {
            CurrentUser = user;
            Db = new HealthPsicho_DBEntities();

            _sessionService = new SessionService(Db);
            _sessionService.Start(user.UserID);

            _startTime = DateTime.Now;
        }


        public static void Start(Users user)
        {
            Initialize(user); 
        }

        public static void End()
        {
            if (_sessionService != null)
            {
                var duration = DateTime.Now - _startTime;
                _sessionService.Stop(duration);
            }

            Db?.Dispose();

            CurrentUser = null;
            _sessionService = null;
            Db = null;
        }
    }
}