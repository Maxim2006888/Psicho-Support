using System.Threading.Tasks;

namespace Psicho_Support.ViewModels
{
    public class CompleteStepViewModel : BaseOnboardingStepViewModel
    {
        public override string StepTitle => "Ты готов";
        public override string StepDescription => "Давай начнём этот путь вместе";

        public override Task OnEnterAsync()
        {
            // Можно триггернуть финальную анимацию
            return Task.CompletedTask;
        }
    }
}