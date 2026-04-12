// ViewModels/ThemeStepViewModel.cs
using Psicho_Support.Services.Interfaces;
using System.Windows.Input;
using System.Windows.Media;

namespace Psicho_Support.ViewModels
{
    public class ThemeStepViewModel : BaseOnboardingStepViewModel
    {
        private readonly IThemeService _themeService;

        public ThemeStepViewModel(IThemeService themeService)
        {
            _themeService = themeService;
            SetColorCommand = new RelayCommand(ExecuteSetColor);
        }

        public bool IsDarkTheme
        {
            get => _themeService.IsDarkTheme;
            set
            {
                if (_themeService.IsDarkTheme != value)
                {
                    _themeService.IsDarkTheme = value;
                    OnPropertyChanged(nameof(IsDarkTheme));
                }
            }
        }

        public SolidColorBrush SelectedAccentColor
        {
            get => _themeService.AccentColor;
            set
            {
                if (_themeService.AccentColor != value)
                {
                    _themeService.AccentColor = value;
                    OnPropertyChanged(nameof(SelectedAccentColor));
                }
            }
        }

        public ICommand SetColorCommand { get; }

        private void ExecuteSetColor(object parameter)
        {
            if (parameter is string colorString)
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorString);
                    SelectedAccentColor = new SolidColorBrush(color);
                }
                catch { }
            }
        }

        public override string StepTitle => "Настрой внешний вид";
        public override string StepDescription => "Выбери цветовую гамму, которая тебе комфортна";
    }
}