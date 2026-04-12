using Psicho_Support.Services;
using Psicho_Support.Services.Interfaces;
using Psicho_Support.ViewModels;
using System.Windows.Controls;

namespace Psicho_Support.Views.Pages
{
    public partial class NotesPage : UserControl
    {
        private NotesViewModel _viewModel;

        public NotesPage()
        {
            InitializeComponent();
        }

        public void SetViewModel(NotesViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_viewModel != null)
            {
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}