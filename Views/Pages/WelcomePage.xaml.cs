using Microsoft.Extensions.DependencyInjection;
using Psicho_Support.Services;
using Psicho_Support.Services.Interfaces;
using Psicho_Support.ViewModels;
using System.Windows.Controls;

namespace Psicho_Support.Views.Pages
{
    public partial class WelcomePage : UserControl
    {
        public WelcomePage()
        {
            InitializeComponent();

            var dialogService = App.Services.GetRequiredService<IDialogService>();
            var session = App.Services.GetRequiredService<AppSession>();

            // ⚠️ временно без NavigationService
            DataContext = new WelcomeViewModel(dialogService, null, session);
        }
    }
}