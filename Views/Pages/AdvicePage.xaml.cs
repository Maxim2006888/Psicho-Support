using Microsoft.Extensions.DependencyInjection;
using Psicho_Support.Services;
using Psicho_Support.ViewModels;
using System.Windows.Controls;

namespace Psicho_Support.Views.Pages
{
    public partial class AdvicePage : UserControl
    {
        private AdviceViewModel _viewModel;

        public AdvicePage()
        {
            InitializeComponent();
        }

        // Метод для установки ViewModel через DI
        public void SetViewModel(AdviceViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = _viewModel;
        }
    }
}