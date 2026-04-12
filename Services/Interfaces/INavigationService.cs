// Services/Interfaces/INavigationService.cs
using System.Windows;
using System.Windows.Controls;

namespace Psicho_Support.Services.Interfaces
{
    public interface INavigationService
    {
        void Initialize(ContentControl contentControl);
        void Navigate(UserControl page);
        void NavigateTo<T>() where T : UserControl, new();
        void GoBack();
        void SwitchToWindow<TWindow>() where TWindow : Window;
    }
}