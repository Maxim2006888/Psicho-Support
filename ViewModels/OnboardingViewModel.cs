using Psicho_Support.Properties;
using Psicho_Support.Core;
using Psicho_Support.Views;
using Psicho_Support.Views.Onboarding;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Psicho_Support.ViewModels
{
    public class OnboardingViewModel : BaseViewModel
    {
        private readonly BaseOnboardingStepViewModel[] _steps;
        private readonly AppState _appState;
        private int _currentIndex;
        private BaseOnboardingStepViewModel _currentStep;

        public OnboardingViewModel(IServiceProvider sp, AppState appState)
        {
            _appState = appState ?? throw new ArgumentNullException(nameof(appState));
            _steps = new BaseOnboardingStepViewModel[]
            {
                sp.GetRequiredService<WelcomeStepViewModel>(),
                sp.GetRequiredService<PrivacyStepViewModel>(),
                sp.GetRequiredService<ThemeStepViewModel>(),
                sp.GetRequiredService<NotificationStepViewModel>(),
                sp.GetRequiredService<CompleteStepViewModel>()
            };

            _currentIndex = 0;
            _currentStep = _steps[0];

            NextCommand = new RelayCommand(async _ => await NextAsync(), _ => CanNext);
            PreviousCommand = new RelayCommand(_ => Previous(), _ => CanPrevious);
            SkipCommand = new RelayCommand(async _ => await CompleteAsync());
        }

        public BaseOnboardingStepViewModel CurrentStep
        {
            get => _currentStep;
            set => SetProperty(ref _currentStep, value);
        }

        public int StepNumber => _currentIndex + 1;
        public int TotalSteps => _steps.Length;
        public double Progress => (double)StepNumber / TotalSteps * 100;
        public bool CanNext => CurrentStep?.CanProceed ?? true;
        public bool CanPrevious => _currentIndex > 0;
        public bool IsLast => _currentIndex == _steps.Length - 1;
        public string NextText => IsLast ? "Начать" : "Далее";

        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand SkipCommand { get; }

        private async Task NextAsync()
        {
            await CurrentStep.OnLeaveAsync();

            if (IsLast)
            {
                await CompleteAsync();
                return;
            }

            _currentIndex++;
            CurrentStep = _steps[_currentIndex];
            await CurrentStep.OnEnterAsync();

            OnPropertyChanged(nameof(StepNumber));
            OnPropertyChanged(nameof(Progress));
            OnPropertyChanged(nameof(CanNext));
            OnPropertyChanged(nameof(CanPrevious));
            OnPropertyChanged(nameof(IsLast));
            OnPropertyChanged(nameof(NextText));
        }

        private void Previous()
        {
            if (!CanPrevious) return;

            _currentIndex--;
            CurrentStep = _steps[_currentIndex];

            OnPropertyChanged(nameof(StepNumber));
            OnPropertyChanged(nameof(Progress));
            OnPropertyChanged(nameof(CanNext));
            OnPropertyChanged(nameof(CanPrevious));
            OnPropertyChanged(nameof(IsLast));
            OnPropertyChanged(nameof(NextText));
        }

        private async Task CompleteAsync()
        {
            Settings.Default.OnboardingCompleted = true;
            Settings.Default.Save();

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var window = System.Windows.Application.Current.Windows
                    .OfType<OnboardingWindow>()
                    .FirstOrDefault();
                window?.Close();

                if (_appState.IsAuthenticated)
                {
                    var userWindow = App.Services.GetRequiredService<UserWindow>();
                    userWindow.Show();
                    return;
                }

                var mainWindow = App.Services.GetRequiredService<HallowWindow>();
                mainWindow.Show();
            });
        }
    }
}