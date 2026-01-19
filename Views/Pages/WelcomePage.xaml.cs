namespace Psicho_Support.Views.Pages
{
    public partial class WelcomePage : BasePage
    {
        public WelcomePage()
        {
            InitializeComponent();

            if (CurrentUser != null)
            {
                DataContext = $"Добро пожаловать, {CurrentUser.Username}!\n\n" +
                             $"Ваш психотип: {CurrentUser.PsychoTypes?.TypeName ?? "не определён"}";
            }
            else
            {
                DataContext = "Ошибка: пользователь не найден";
            }
        }
    }
}