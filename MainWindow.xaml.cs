using Microsoft.Extensions.DependencyInjection;
using Psicho_Support.Services.Interfaces;
using Psicho_Support.Views;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Psicho_Support
{
    public partial class MainWindow : Window
    {
        private readonly INavigationService _navigationService;

        public MainWindow(INavigationService navigationService)
        {
            InitializeComponent();
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DetailsText.Text = "Инициализация данных...";

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                OpenWindow();
            };
            timer.Start();
        }

        private void OpenWindow()
        {
            _navigationService.SwitchToWindow<HallowWindow>();
            Close();
        }
    }
}