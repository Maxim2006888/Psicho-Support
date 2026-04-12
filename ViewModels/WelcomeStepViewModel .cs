using System.Threading.Tasks;

namespace Psicho_Support.ViewModels
{
    public class WelcomeStepViewModel : BaseOnboardingStepViewModel
    {
        public override string StepTitle => "Добро пожаловать";
        public override string StepDescription => "Это пространство, где можно быть собой";

        public override Task OnEnterAsync()
        {
            // Можно потом добавить анимации / звук / логирование
            return Task.CompletedTask;
        }
    }
}