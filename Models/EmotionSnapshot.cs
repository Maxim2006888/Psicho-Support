using System;

namespace Psicho_Support.Models
{
    public class EmotionSnapshot
    {
        public DateTime Timestamp { get; set; }

        public double Score { get; set; }          // 0..1
        public int Stress { get; set; }            // 0..100

        public EmotionType Emotion { get; set; }
        public EmotionalState State { get; set; }

        public string SourceText { get; set; }
    }
}