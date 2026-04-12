namespace Psicho_Support.Models
{
    public enum EmotionType
    {
        Neutral,
        Happiness,
        Calm,
        Anxiety,
        Stress,
        Sadness,
        Anger,
        Burnout
    }

    public class EmotionResult
    {
        public double Score { get; set; }          // 0..1
        public int StressLevel { get; set; }       // 0..100
        public EmotionType DominantEmotion { get; set; }
        public double Confidence { get; set; }
    }
}