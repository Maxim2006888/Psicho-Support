namespace Psicho_Support.ViewModels
{
    public class PrivacyStepViewModel : BaseOnboardingStepViewModel
    {
        public override string StepTitle => "Конфиденциальность";
        public override string StepDescription => "Твои данные остаются только у тебя";

        public bool IsAccepted { get; set; }

        public override bool CanProceed => IsAccepted;
    }
}