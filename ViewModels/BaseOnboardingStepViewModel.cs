using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Psicho_Support.ViewModels
{
    public abstract class BaseOnboardingStepViewModel : BaseViewModel
    {
        public virtual string StepTitle => string.Empty;
        public virtual string StepDescription => string.Empty;

        public virtual bool CanProceed => true;
        public virtual bool IsSkippable => true;

        public virtual Task OnEnterAsync() => Task.CompletedTask;
        public virtual Task OnLeaveAsync() => Task.CompletedTask;
}

       

        
    
}