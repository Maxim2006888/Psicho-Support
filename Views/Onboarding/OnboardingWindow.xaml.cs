// Views/Onboarding/OnboardingWindow.xaml.cs
using Psicho_Support.ViewModels;
using System.Windows;
using System.Windows.Media.Animation;
using System.ComponentModel;

namespace Psicho_Support.Views.Onboarding
{
    public partial class OnboardingWindow : Window
    {
        public OnboardingWindow(OnboardingViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.PropertyChanged += Vm_PropertyChanged;
        }

        private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OnboardingViewModel.CurrentStep))
            {
                Dispatcher.Invoke(() =>
                {
                    var sb = (Storyboard)FindResource("FadeInAnimation");
                    sb.Begin();
                });
            }
        }
    }
}