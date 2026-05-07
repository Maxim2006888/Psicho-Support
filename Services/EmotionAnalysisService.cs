using Psicho_Support.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Psicho_Support.Services
{
    public class EmotionAnalysisService
    {
        private readonly List<EmotionResult> _history = new List<EmotionResult>();

        // 🔍 Анализ текста заметки
        public EmotionResult AnalyzeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new EmotionResult
                {
                    StressLevel = 0,
                    DominantEmotion = EmotionType.Neutral,
                    Confidence = 0.5
                };
            }

            text = text.ToLower();

            int stress = 0;
            EmotionType emotion = EmotionType.Neutral;

            // 🔥 Простейший анализ (можно улучшать)
            if (text.Contains("устал") || text.Contains("давит") || text.Contains("не могу"))
            {
                stress += 40;
                emotion = EmotionType.Stress;
            }

            if (text.Contains("тревога") || text.Contains("переживаю"))
            {
                stress += 30;
                emotion = EmotionType.Anxiety;
            }

            if (text.Contains("грусть") || text.Contains("плохо"))
            {
                stress += 25;
                emotion = EmotionType.Sadness;
            }

            if (text.Contains("злюсь") || text.Contains("бесит"))
            {
                stress += 35;
                emotion = EmotionType.Anger;
            }

            if (text.Contains("рад") || text.Contains("счастлив"))
            {
                stress -= 30;
                emotion = EmotionType.Happiness;
            }

            // Нормализация
            stress = Math.Max(0, Math.Min(100, stress));

            var result = new EmotionResult
            {
                StressLevel = stress,
                DominantEmotion = emotion,
                Confidence = 0.7,
                Timestamp = DateTime.Now,
                SourceText = text
            };

            _history.Add(result);

            return result;
        }

        // 📈 История
        public List<EmotionResult> GetHistory()
        {
            return _history.ToList();
        }

        // 📊 Средний стресс
        public double GetAverageStress()
        {
            if (_history.Count == 0) return 0;
            return _history.Average(x => x.StressLevel);
        }

        // 📈 Тренд
        public string GetTrend()
        {
            if (_history.Count < 2) return "Недостаточно данных";

            var last = _history.Last().StressLevel;
            var prev = _history[_history.Count - 2].StressLevel;

            if (last > prev) return "Рост стресса";
            if (last < prev) return "Снижение стресса";
            return "Без изменений";
        }

        // 💡 Инсайт
        public string GetInsight()
        {
            if (_history.Count < 3) return "Пока мало данных";

            var lastItems = _history.Skip(Math.Max(0, _history.Count - 5)).Take(5).Select(x => x.StressLevel).ToList();

            if (lastItems.Count >= 3 && lastItems.Take(3).All(x => x > 60))
            {
                return "Последние записи показывают высокий уровень стресса";
            }

            if (lastItems.Count >= 3 && lastItems.Take(3).All(x => x < 30))
            {
                return "Ты в стабильном состоянии";
            }

            return "Состояние меняется — обрати внимание на триггеры";
        }
    }
}