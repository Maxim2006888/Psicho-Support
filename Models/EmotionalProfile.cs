using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Psicho_Support.Models
{
    public class EmotionalProfile
    {
        public float AverageStress { get; set; }
        public EmotionType DominantEmotion { get; set; }
        public float StabilityIndex { get; set; }
        public float FluctuationIndex { get; set; }
        public float StressLoadIndex { get; set; }
        public float BurnoutRisk { get; set; }
        public EmotionalPhase CurrentPhase { get; set; }

        public string PredictionReason { get; set; }
    }
}
