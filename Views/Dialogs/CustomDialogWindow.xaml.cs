// Views/Dialogs/CustomDialogWindow.xaml.cs
using Psicho_Support.Enums;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;


namespace Psicho_Support.Views.Dialogs
{
    public partial class CustomDialogWindow : Window
    {
        public CustomDialogWindow(string title, string message, DialogType type, bool confirm = false)
        {
            InitializeComponent();
            Loaded += AnimateIn;


            TitleText.Text = title;
            MessageText.Text = message;

            ApplyDialogStyle(type);

            if (confirm)
                AddCancelButton();
        }

        private void ApplyDialogStyle(DialogType type)
        {
            Brush brush = null;

            switch (type)
            {
                case DialogType.Success:
                    brush = (Brush)Application.Current.Resources["SupportBrush"];
                    break;

                case DialogType.Error:
                    brush = (Brush)Application.Current.Resources["ErrorBrush"];
                    break;

                case DialogType.Warning:
                    brush = (Brush)Application.Current.Resources["WarningBrush"];
                    break;

                case DialogType.Question:
                    brush = (Brush)Application.Current.Resources["InfoBrush"];
                    break;

                case DialogType.Info:
                    brush = (Brush)Application.Current.Resources["InfoBrush"];
                    break;

                default:
                    brush = (Brush)Application.Current.Resources["AccentBrush"];
                    break;
            }

            RootBorder.Background = brush;
        }


        private void AnimateIn(object sender, RoutedEventArgs e)
        {
            var fade = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(220)
            };

            this.BeginAnimation(OpacityProperty, fade);
        }


        private void AddCancelButton()
        {
            var cancel = new Button
            {
                Content = "Отмена",
                Width = 110,
                Height = 36,
                Margin = new Thickness(5),
                Style = (Style)Application.Current.Resources["DialogButtonStyle"]
            };

            cancel.Click += (s, e) =>
            {
                DialogResult = false;
                Close();
            };

            ButtonsPanel.Children.Add(cancel);
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}