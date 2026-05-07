using System;

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

        // 🔥 НОВОЕ: время анализа
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // 🔥 НОВОЕ: исходный текст (для будущей аналитики)
        public string SourceText { get; set; }

        // 🔥 НОВОЕ: быстрый вывод состояния
        public string GetShortDescription()
        {
            switch (DominantEmotion)
            {
                case EmotionType.Happiness: return "Позитив";
                case EmotionType.Calm: return "Спокойствие";
                case EmotionType.Anxiety: return "Тревога";
                case EmotionType.Stress: return "Стресс";
                case EmotionType.Sadness: return "Грусть";
                case EmotionType.Anger: return "Раздражение";
                case EmotionType.Burnout: return "Выгорание";
                default: return "Нейтрально";
            }
        }
    }
}