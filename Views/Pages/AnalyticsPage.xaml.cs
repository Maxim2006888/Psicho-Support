using Microsoft.Extensions.DependencyInjection;
using Psicho_Support.ViewModels;
using System.Windows.Controls;

namespace Psicho_Support.Views.Pages
{
    public partial class AnalyticsPage : UserControl
    {
        public AnalyticsPage()
        {
            InitializeComponent();

            // ✅ Получаем ViewModel из DI
            var vm = App.Services.GetRequiredService<AnalyticsViewModel>();

            DataContext = vm;

            _ = vm.InitializeAsync();
        }


    }
}