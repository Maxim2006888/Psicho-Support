// Services/Interfaces/INotificationService.cs
using System.Threading.Tasks;

namespace Psicho_Support.Services.Interfaces
{
    public interface INotificationService
    {
        Task ShowNotificationAsync(string title, string message);
        void RequestPermission();
    }
}