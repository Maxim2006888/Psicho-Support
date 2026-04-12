// Services/Interfaces/IThemeService.cs
using System.Windows.Media;

namespace Psicho_Support.Services.Interfaces
{
    public interface IThemeService
    {
        bool IsDarkTheme { get; set; }
        SolidColorBrush AccentColor { get; set; }
        void ApplyTheme();
    }
}