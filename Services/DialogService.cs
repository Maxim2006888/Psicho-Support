using Psicho_Support.Enums;
using Psicho_Support.Services.Interfaces;
using Psicho_Support.Views.Dialogs;
using System.Threading.Tasks;
using System.Windows;

namespace Psicho_Support.Services
{
    public class DialogService : IDialogService
    {
        public void Show(string title, string message, DialogType type = DialogType.Info, object owner = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var dialog = new CustomDialogWindow(title, message, type);

                // Безопасная установка Owner
                if (owner is Window ownerWindow && ownerWindow.IsLoaded)
                {
                    dialog.Owner = ownerWindow;
                }
                else if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsLoaded)
                {
                    dialog.Owner = Application.Current.MainWindow;
                }
                // Если нет окна-владельца, не устанавливаем Owner

                dialog.ShowDialog();
            });
        }

        public async Task<bool> ShowConfirmationAsync(string title, string message)
        {
            bool result = false;

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                result = ShowConfirmInternal(title, message);
            });

            return result;
        }

        public async Task ShowMessageAsync(string title, string message, DialogType type = DialogType.Info)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Show(title, message, type);
            });
        }

        private bool ShowConfirmInternal(string title, string message, Window owner = null)
        {
            var dialog = new CustomDialogWindow(title, message, DialogType.Warning, true);

            // Безопасная установка Owner
            if (owner != null && owner.IsLoaded)
            {
                dialog.Owner = owner;
            }
            else if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsLoaded)
            {
                dialog.Owner = Application.Current.MainWindow;
            }
            // Если нет окна-владельца, не устанавливаем Owner

            return dialog.ShowDialog() == true;
        }
    }
}