using Psicho_Support.Services.Interfaces;
using System.Threading.Tasks;
using System.Windows;

namespace Psicho_Support.Services
{
    public enum NotificationType
    {
        Reminder,
        Warning,
        Insight,
        Support
    }

    public class NotificationService : INotificationService
    {
        public Task ShowNotificationAsync(string title, string message)
        {
            // Пока используем MessageBox как базовую реализацию
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title);
            });

            return Task.CompletedTask;
        }

        public Task ShowNotificationAsync(string title, string message, NotificationType type)
        {
            string prefix = type == NotificationType.Warning ? "⚠ " :
                            type == NotificationType.Support ? "💙 " :
                            type == NotificationType.Insight ? "💡 " :
                            type == NotificationType.Reminder ? "⏰ " : "";

            return ShowNotificationAsync(prefix + title, message);
        }

        public void RequestPermission()
        {
            // Для WPF не требуется, но оставим для будущего (например, WinUI / мобильные)
        }
    }
}