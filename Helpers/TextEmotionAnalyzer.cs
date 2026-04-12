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
            { "рад", (0.8, EmotionType.Happiness) },
            { "спокойно", (0.7, EmotionType.Calm) },
            { "хорошо", (0.7, EmotionType.Calm) },

            // Негатив
            { "тревожно", (0.2, EmotionType.Anxiety) },
            { "страшно", (0.2, EmotionType.Anxiety) },
            { "устал", (0.3, EmotionType.Burnout) },
            { "выгорел", (0.1, EmotionType.Burnout) },
            { "грустно", (0.3, EmotionType.Sadness) },
            { "плохо", (0.3, EmotionType.Stress) },
            { "злюсь", (0.2, EmotionType.Anger) }
        };

        private readonly HashSet<string> _intensifiers = new HashSet<string>
        {
            "очень", "сильно", "крайне", "ужасно"
        };

        private readonly HashSet<string> _negations = new HashSet<string>
        {
            "не", "нет", "ни"
        };

        // 🔹 Старый метод (оставляем!)
        public double Analyze(string text)
        {
            return AnalyzeAdvanced(text).Score;
        }

        // 🔥 Новый метод
        public EmotionResult AnalyzeAdvanced(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return NeutralResult();

            var words = Regex.Split(text.ToLower(), @"\W+")
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .ToList();

            double score = 0.5;
            int count = 0;
            double multiplier = 1.0;

            var emotionCounter = new Dictionary<EmotionType, int>();

            for (int i = 0; i < words.Count; i++)
            {
                var word = words[i];

                if (_intensifiers.Contains(word))
                {
                    multiplier = 1.5;
                    continue;
                }

                bool isNegated = i > 0 && _negations.Contains(words[i - 1]);

                if (_words.TryGetValue(word, out var data))
                {
                    double wordScore = data.score;

                    // 🔁 Инверсия при "не"
                    if (isNegated)
                        wordScore = 1 - wordScore;

                    wordScore = ApplyMultiplier(wordScore, multiplier);
                    multiplier = 1.0;

                    score = (score * count + wordScore) / (count + 1);
                    count++;

                    if (!emotionCounter.ContainsKey(data.type))
                        emotionCounter[data.type] = 0;

                    emotionCounter[data.type]++;
                }
            }

            int stress = ConvertScoreToStress(score);

            return new EmotionResult
            {
                Score = Math.Round(score, 2),
                StressLevel = stress,
                DominantEmotion = GetDominantEmotion(emotionCounter),
                Confidence = CalculateConfidence(count, words.Count)
            };
        }

        private EmotionType GetDominantEmotion(Dictionary<EmotionType, int> emotions)
        {
            if (!emotions.Any())
                return EmotionType.Neutral;

            return emotions.OrderByDescending(e => e.Value).First().Key;
        }

        private double CalculateConfidence(int matchedWords, int totalWords)
        {
            if (totalWords == 0) return 0;
            return Math.Round((double)matchedWords / totalWords, 2);
        }

        private double ApplyMultiplier(double score, double multiplier)
        {
            double deviation = score - 0.5;
            return MathHelper.Clamp(0.5 + deviation * multiplier, 0.1, 0.9);
        }

        private int ConvertScoreToStress(double score)
        {
            int stress = (int)((1 - score) * 100);
            return MathHelper.Clamp(stress, 0, 100);
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