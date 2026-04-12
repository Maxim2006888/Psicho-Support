using Microsoft.Extensions.DependencyInjection;
using Psicho_Support.Services.Interfaces;
using System;
using System.Windows;

namespace Psicho_Support.Views
{
    public partial class HallowWindow : Window
    {
        private readonly INavigationService _navigationService;

        public HallowWindow(INavigationService navigationService)
        {
            InitializeComponent();
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            if (Application.Current.MainWindow == null)
            {
                Application.Current.MainWindow = this;
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            // ✅ Создаём новое окно входа
            _navigationService.SwitchToWindow<LoginWindow>();
            Close();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            // ✅ Создаём новое окно регистрации
            _navigationService.SwitchToWindow<RegisterWindow>();
            Close();
        }
    }
}