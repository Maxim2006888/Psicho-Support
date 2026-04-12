using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Psicho_Support.Views.Onboarding.Steps
{
    /// <summary>
    /// Логика взаимодействия для NotificationStepView.xaml
    /// </summary>
    public partial class NotificationStepView : UserControl
    {
        public NotificationStepView()
        {
            InitializeComponent();
        }

        private void EnableNotifications_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.NotificationStepViewModel;
            vm?.EnableNotificationsCommand?.Execute(null);
        }
    }
}
