using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Psicho_Support.Models;
using Psicho_Support.Helpers;

namespace Psicho_Support.Helpers
{
    public class TextEmotionAnalyzer
    {
        private readonly Dictionary<string, (double score, EmotionType type)> _words =
            new Dictionary<string, (double, EmotionType)>
        {
            // Позитив
            { "счастлив", (0.9, EmotionType.Happiness) },
            { "рад", (0.85, EmotionType.Happiness) },
            { "радость", (0.9, EmotionType.Happiness) },

            { "спокойно", (0.75, EmotionType.Calm) },
            { "тихо", (0.7, EmotionType.Calm) },
            { "хорошо", (0.7, EmotionType.Calm) },

            // Негатив
            { "тревожно", (0.2, EmotionType.Anxiety) },
            { "страшно", (0.15, EmotionType.Anxiety) },

            { "устал", (0.3, EmotionType.Burnout) },
            { "выгорел", (0.1, EmotionType.Burnout) },

            { "грустно", (0.25, EmotionType.Sadness) },
            { "плохо", (0.2, EmotionType.Stress) },

            { "злюсь", (0.2, EmotionType.Anger) },
            { "злость", (0.2, EmotionType.Anger) }
        };

        private readonly HashSet<string> _intensifiers = new HashSet<string>
        {
            "очень", "сильно", "крайне", "ужасно", "жутко"
        };

        private readonly HashSet<string> _negations = new HashSet<string>
        {
            "не", "нет", "ни", "никогда"
        };

        public double Analyze(string text)
        {
            return AnalyzeAdvanced(text).Score;
        }

        public EmotionResult AnalyzeAdvanced(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return NeutralResult();

            var words = Regex.Split(text.ToLower(), @"\W+")
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .ToList();

            double weightedSum = 0;
            double totalWeight = 0;

            var emotionCounter = new Dictionary<EmotionType, int>();

            for (int i = 0; i < words.Count; i++)
            {
                var word = words[i];

                if (!_words.TryGetValue(word, out var data))
                    continue;

                double weight = 1.0;

                // 🔥 1. проверка negation в окне 2 слов назад
                if (IsNegated(words, i))
                    data.score = 1 - data.score;

                // 🔥 2. усилители перед словом
                if (i > 0 && _intensifiers.Contains(words[i - 1]))
                    weight *= 1.6;

                // 🔥 3. повтор слова усиливает эффект
                int repeats = CountRepeats(words, word, i);
                weight *= (1 + repeats * 0.15);

                // 🔥 4. добавляем вклад
                weightedSum += data.score * weight;
                totalWeight += weight;

                if (!emotionCounter.ContainsKey(data.type))
                    emotionCounter[data.type] = 0;

                emotionCounter[data.type]++;
            }

            double finalScore = totalWeight == 0
                ? 0.5
                : weightedSum / totalWeight;

            int stress = ConvertScoreToStress(finalScore);

            return new EmotionResult
            {
                Score = Math.Round(finalScore, 2),
                StressLevel = stress,
                DominantEmotion = GetDominantEmotion(emotionCounter),
                Confidence = CalculateConfidence(emotionCounter, words.Count)
            };
        }

        // 🔥 negation теперь работает в окне 3 слов назад
        private bool IsNegated(List<string> words, int index)
        {
            for (int i = Math.Max(0, index - 3); i < index; i++)
            {
                if (_negations.Contains(words[i]))
                    return true;
            }
            return false;
        }

        // 🔥 повтор слова
        private int CountRepeats(List<string> words, string word, int index)
        {
            int count = 0;

            for (int i = Math.Max(0, index - 3); i < index; i++)
            {
                if (words[i] == word)
                    count++;
            }

            return count;
        }

        private EmotionType GetDominantEmotion(Dictionary<EmotionType, int> emotions)
        {
            if (!emotions.Any())
                return EmotionType.Neutral;

            return emotions.OrderByDescending(e => e.Value).First().Key;
        }

        private double CalculateConfidence(Dictionary<EmotionType, int> emotions, int totalWords)
        {
            if (totalWords == 0) return 0;

            int matched = emotions.Values.Sum();
            return Math.Round((double)matched / totalWords, 2);
        }

        private int ConvertScoreToStress(double score)
        {
            return MathHelper.Clamp((int)((1 - score) * 100), 0, 100);
        }

        private EmotionResult NeutralResult()
        {
            return new EmotionResult
            {
                Score = 0.5,
                StressLevel = 50,
                DominantEmotion = EmotionType.Neutral,
                Confidence = 0
            };
        }
    }
}