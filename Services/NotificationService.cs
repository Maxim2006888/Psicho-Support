// Services/NotificationService.cs
using Psicho_Support.Services.Interfaces;
using System.Threading.Tasks;

namespace Psicho_Support.Services
{
    public class NotificationService : INotificationService
    {
        public Task ShowNotificationAsync(string title, string message)
        {
            // Временно просто возвращаем завершенную задачу
            return Task.CompletedTask;
        }

        public void RequestPermission()
        {
            // Заглушка
        }
    }
}