using Microsoft.Extensions.DependencyInjection;
using Psicho_Support.Services;
using Psicho_Support.Services.Interfaces;
using Psicho_Support.ViewModels;
using Psicho_Support.Core;
using System.Windows.Controls;

namespace Psicho_Support.Views.Pages
{
    public partial class WelcomePage : UserControl
    {
        public WelcomePage()
        {
            InitializeComponent();

            var dialogService = App.Services.GetRequiredService<IDialogService>();
            var navigationService = App.Services.GetRequiredService<INavigationService>();
            var session = App.Services.GetRequiredService<AppSession>();
            var appState = App.Services.GetRequiredService<AppState>();
            var stateService = App.Services.GetRequiredService<UserStateService>();

            DataContext = new WelcomeViewModel(dialogService, navigationService, session, appState, stateService);
        }
    }
}