// Views/Pages/SettingsPage.xaml.cs
using Microsoft.Extensions.DependencyInjection;
using Psicho_Support.Services;
using Psicho_Support.Services.Interfaces;
using Psicho_Support.Views;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Psicho_Support.Views.Pages
{
    public partial class SettingsPage : UserControl
    {
        private readonly IThemeService _themeManager;
        private readonly AppSession _session;

        public SettingsPage()
        {
            InitializeComponent();

            _themeManager = App.Services.GetRequiredService<IThemeService>();
            _session = App.Services.GetRequiredService<AppSession>();

            LoadData();
        }

        private void LoadData()
        {
            var user = _session?.CurrentUser;
            if (user == null) return;

            UsernameBox.Text = user.Username;
            SessionTimeText.Text = _session.CurrentSessionDuration.ToString(@"mm\:ss");

            ThemeToggle.IsChecked = _themeManager.IsDarkTheme;
        }

        private void ThemeToggle_Checked(object sender, RoutedEventArgs e)
        {
            _themeManager.IsDarkTheme = true;
        }

        private void ThemeToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            _themeManager.IsDarkTheme = false;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _session?.EndSession();

            var loginWindow = App.Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();

            Window.GetWindow(this)?.Close();
        }
    }
}