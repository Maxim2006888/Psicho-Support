using Microsoft.Extensions.DependencyInjection;
using Psicho_Support.Core;
using Psicho_Support.Data;
using Psicho_Support.Services;
using Psicho_Support.Services.Interfaces;
using Psicho_Support.ViewModels;
using Psicho_Support.Views;
using Psicho_Support.Views.Onboarding;
using Psicho_Support.Views.Onboarding.Steps;
using Psicho_Support.Views.Pages;
using System;
using System.Configuration;
using System.Windows;
using Psicho_Support.Properties;
using Psicho_Support.Helpers;


namespace Psicho_Support
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += App_DispatcherUnhandledException;

            try
            {
                var services = new ServiceCollection();
                ConfigureServices(services);

                Services = services.BuildServiceProvider();

                // Проверяем, нужно ли показывать онбординг
                var onboardingCompleted = Settings.Default.OnboardingCompleted;

                if (!onboardingCompleted)
                {
                    var onboardingWindow = Services.GetRequiredService<OnboardingWindow>();
                    onboardingWindow.Show();
                }
                else
                {
                    var mainWindow = Services.GetRequiredService<HallowWindow>();
                    mainWindow.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске: {ex.Message}\n{ex.StackTrace}",
                    "Критическая ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                $"Произошла необработанная ошибка:\n{e.Exception.Message}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            e.Handled = true;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var session = Services?.GetService<AppSession>();
            session?.Dispose();

            // Сохраняем настройки
            Settings.Default.Save();

            base.OnExit(e);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // База данных
            services.AddTransient<HealthPsicho_DBEntities>();

            // Сервисы
            services.AddSingleton<AppSession>();
            services.AddSingleton<AnalyticsService>();
            services.AddSingleton<UserStateService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IThemeService, ThemeManager>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<INotificationService, NotificationService>();
            
            //Эмоциональная система 
            services.AddSingleton<TextEmotionAnalyzer>();
            services.AddSingleton<EmotionMemoryService>();
            services.AddSingleton<EmotionTrendAnalyzer>();
            services.AddSingleton<EmotionStateMachine>();
            services.AddSingleton<EmotionPredictor>();
            services.AddSingleton<EmotionBehaviorEngine>();

            // Core
            services.AddSingleton<AppState>();

            // ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegisterViewModel>();
            services.AddTransient<WelcomeViewModel>();
            services.AddTransient<NotesViewModel>();
            services.AddTransient<TestsViewModel>();
            services.AddSingleton<AnalyticsViewModel>();
            services.AddTransient<AchievementsViewModel>();
            services.AddTransient<AdviceViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<UserWindowViewModel>();
            services.AddTransient<OnboardingViewModel>(); 
            services.AddTransient<WelcomeStepViewModel>();
            services.AddTransient<PrivacyStepViewModel>();
            services.AddTransient<ThemeStepViewModel>();
            services.AddTransient<NotificationStepViewModel>();
            services.AddTransient<CompleteStepViewModel>();
            services.AddTransient<OnboardingViewModel>();

            // Windows
            services.AddSingleton<HallowWindow>();
            services.AddTransient<UserWindow>();
            services.AddTransient<LoginWindow>();
            services.AddTransient<RegisterWindow>();
            services.AddTransient<OnboardingWindow>(); 

            // Pages (UserControl)
            services.AddTransient<WelcomePage>();
            services.AddTransient<NotesPage>();
            services.AddTransient<TestsPage>();
            services.AddTransient<AnalyticsPage>();
            services.AddTransient<AchievementsPage>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<AdvicePage>();

            // Онбординг шаги
            services.AddTransient<WelcomeStepView>();
            services.AddTransient<PrivacyStepView>();
            services.AddTransient<ThemeStepView>();
            services.AddTransient<NotificationStepView>();
            services.AddTransient<CompleteStepView>();
        }
    }
}