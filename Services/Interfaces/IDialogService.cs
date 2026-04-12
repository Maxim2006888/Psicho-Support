// Services/Interfaces/IDialogService.cs
using Psicho_Support.Enums;
using System.Threading.Tasks;

namespace Psicho_Support.Services.Interfaces
{
    public interface IDialogService
    {
        void Show(string title, string message, DialogType type = DialogType.Info, object owner = null);
        Task<bool> ShowConfirmationAsync(string title, string message);
        Task ShowMessageAsync(string title, string message, DialogType type = DialogType.Info);


    }
}