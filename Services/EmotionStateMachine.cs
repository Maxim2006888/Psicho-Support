using Psicho_Support.Models;

namespace Psicho_Support.Services
{
    public class EmotionStateMachine
    {
        public EmotionalState GetState(int stress, double trend)
        {
            // trend: изменение (−1..+1)

            if (stress < 30)
                return trend < -0.2 ? EmotionalState.Recovery : EmotionalState.Stable;

            if (stress < 50)
                return EmotionalState.MildStress;

            if (stress < 70)
                return EmotionalState.HighStress;

            if (stress < 85)
                return EmotionalState.AnxietyPeak;

            return EmotionalState.Burnout;
        }
    }
}