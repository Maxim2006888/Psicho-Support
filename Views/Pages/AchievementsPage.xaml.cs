using Psicho_Support.Data;
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

namespace Psicho_Support.Views.Pages
{
    public partial class AchievementsPage : BasePage
    {
        public AchievementsPage() : base()
        {
            InitializeComponent();
            LoadAchievements();
        }

        private void LoadAchievements()
        {
            using (var db = new HealthPsicho_DBEntities())
            {
                var achievements = db.UserAchievements
                    .Where(a => a.UserID == CurrentUser.UserID)
                    .Select(a => a.Achievements.Title)
                    .ToList();

                AchievementsListBox.Items.Clear();

                if (achievements.Any())
                {
                    foreach (var a in achievements)
                        AchievementsListBox.Items.Add($"🏅 {a}");
                }
                else
                {
                    AchievementsListBox.Items.Add("Пока нет достижений, но всё впереди!");
                }
            }
        }
    }
}
