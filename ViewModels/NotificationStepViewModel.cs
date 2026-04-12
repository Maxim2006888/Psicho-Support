// ViewModels/NotificationStepViewModel.cs
using Psicho_Support.Services.Interfaces;
using System.Windows.Input;

namespace Psicho_Support.ViewModels
{
    public class NotificationStepViewModel : BaseOnboardingStepViewModel
    {
        private readonly INotificationService _notificationService;
        private bool _notificationsEnabled;

        public NotificationStepViewModel(INotificationService notificationService)
        {
            _notificationService = notificationService;
            EnableNotificationsCommand = new RelayCommand(ExecuteEnableNotifications);
        }

        public bool NotificationsEnabled
        {
            get => _notificationsEnabled;
            set => SetProperty(ref _notificationsEnabled, value);
        }

        public ICommand EnableNotificationsCommand { get; }

        private void ExecuteEnableNotifications()
        {
            _notificationService.RequestPermission();
            NotificationsEnabled = true;
        }

        public override string StepTitle => "Уведомления";
        public override string StepDescription => "Получай напоминания и поддержку";
    }
}