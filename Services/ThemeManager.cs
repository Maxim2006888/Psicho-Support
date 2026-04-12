// Services/ThemeManager.cs
using Psicho_Support.Properties;
using Psicho_Support.Services.Interfaces;
using System.Windows;
using System.Windows.Media;

namespace Psicho_Support.Services
{
    public class ThemeManager : IThemeService
    {
        private bool _isDarkTheme;
        private SolidColorBrush _accentColor;

        public ThemeManager()
        {
            _isDarkTheme = Settings.Default.IsDarkTheme;

            try
            {
                var savedColor = Settings.Default.AccentColor;
                if (!string.IsNullOrEmpty(savedColor))
                {
                    var color = (Color)ColorConverter.ConvertFromString(savedColor);
                    _accentColor = new SolidColorBrush(color);
                }
                else
                {
                    _accentColor = new SolidColorBrush(Color.FromRgb(90, 79, 207));
                }
            }
            catch
            {
                _accentColor = new SolidColorBrush(Color.FromRgb(90, 79, 207));
            }
        }

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (_isDarkTheme != value)
                {
                    _isDarkTheme = value;
                    Settings.Default.IsDarkTheme = value;
                    Settings.Default.Save();
                    ApplyTheme();
                }
            }
        }

        public SolidColorBrush AccentColor
        {
            get => _accentColor;
            set
            {
                if (_accentColor != value)
                {
                    _accentColor = value;
                    Settings.Default.AccentColor = value.Color.ToString();
                    Settings.Default.Save();
                    ApplyTheme();
                }
            }
        }

        public void ApplyTheme()
        {
            // Применяем тему ко всем открытым окнам
            foreach (Window window in Application.Current.Windows)
            {
                UpdateWindowTheme(window);
            }
        }

        private void UpdateWindowTheme(Window window)
        {
            var resources = window.Resources;
            if (resources.MergedDictionaries.Count > 0)
            {
                foreach (var dict in resources.MergedDictionaries)
                {
                    if (dict.Source?.OriginalString?.Contains("Colors.xaml") == true)
                    {
                        dict["AccentBrush"] = _accentColor;
                        dict["BackgroundBrush"] = _isDarkTheme
                            ? new SolidColorBrush(Color.FromRgb(30, 30, 47))
                            : new SolidColorBrush(Color.FromRgb(245, 245, 250));
                        dict["SurfaceBrush"] = _isDarkTheme
                            ? new SolidColorBrush(Color.FromRgb(42, 42, 61))
                            : new SolidColorBrush(Color.FromRgb(255, 255, 255));
                        dict["TextPrimaryBrush"] = _isDarkTheme
                            ? new SolidColorBrush(Colors.White)
                            : new SolidColorBrush(Color.FromRgb(30, 30, 47));
                        dict["TextSecondaryBrush"] = _isDarkTheme
                            ? new SolidColorBrush(Color.FromRgb(170, 170, 170))
                            : new SolidColorBrush(Color.FromRgb(100, 100, 100));
                        break;
                    }
                }
            }
        }
    }
}