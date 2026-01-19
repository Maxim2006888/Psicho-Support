using Psicho_Support.Views.Pages;
using System.Windows;
using System.Windows.Controls;

namespace Psicho_Support.Services
{
    public class NavigationService
    {
        private readonly ContentControl _contentControl;

        public NavigationService(ContentControl contentControl)
        {
            _contentControl = contentControl;
        }

        public void Navigate(BasePage page)
        {
            _contentControl.Content = null;
            _contentControl.Content = page;
        }
    }
}