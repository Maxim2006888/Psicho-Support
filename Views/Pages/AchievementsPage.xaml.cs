using Microsoft.Extensions.DependencyInjection;
using Psicho_Support.Services;
using Psicho_Support.Services.Interfaces;
using Psicho_Support.ViewModels;
using System.Windows.Controls;

namespace Psicho_Support.Views.Pages
{
    public partial class AchievementsPage : UserControl
    {
        public AchievementsPage()
        {
            InitializeComponent();

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;

            var session = App.Services.GetRequiredService<AppSession>();

            if (session?.CurrentUser == null)
                return;

            IDialogService dialogService = App.Services.GetRequiredService<IDialogService>();

            // 🔥 временно без NavigationService
            DataContext = new AchievementsViewModel(dialogService, null, session);
        }
    }
}