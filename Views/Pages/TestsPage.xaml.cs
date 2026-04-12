using Microsoft.Extensions.DependencyInjection;
using Psicho_Support.Services;
using Psicho_Support.ViewModels;
using System.Windows.Controls;

namespace Psicho_Support.Views.Pages
{
    public partial class TestsPage : UserControl
    {
        public TestsPage()
        {
            InitializeComponent();

            var session = App.Services.GetRequiredService<AppSession>();
            if (session?.CurrentUser == null)
                return;

            DataContext = App.Services.GetRequiredService<TestsViewModel>();
        }
    }
}