using System;
using System.Collections.Generic;
using System.Linq;
using Psicho_Support.Models;

namespace Psicho_Support.Services
{
    public class EmotionMemoryService
    {
        private class MemoryRecord
        {
            public string Text { get; set; }
            public EmotionResult Result { get; set; }
        }

        // 🔥 Timeline (для аналитики и трендов)
        public UserEmotionTimeline Timeline { get; private set; }

        private readonly Dictionary<int, List<MemoryRecord>> _memory =
            new Dictionary<int, List<MemoryRecord>>();

        private readonly Dictionary<int, Dictionary<string, double>> _weights =
            new Dictionary<int, Dictionary<string, double>>();

        public EmotionMemoryService()
        {
            Timeline = new UserEmotionTimeline
            {
                History = new List<EmotionSnapshot>()
            };
        }

        // 🔹 Новый метод (ВАЖНО)
        public void AddSnapshot(EmotionSnapshot snapshot)
        {
            Timeline.History.Add(snapshot);

            // ограничение истории (например 100)
            if (Timeline.History.Count > 100)
                Timeline.History.RemoveAt(0);
        }

        // 🔹 Старый функционал (оставляем)
        public void AddRecord(int userId, string text, EmotionResult result)
        {
            if (!_memory.ContainsKey(userId))
                _memory[userId] = new List<MemoryRecord>();

            _memory[userId].Add(new MemoryRecord
            {
                Text = text,
                Result = result
            });
        }

        public void LearnFromFeedback(int userId, int realStress)
        {
            if (!_memory.ContainsKey(userId))
                return;

            var records = _memory[userId];
            if (records.Count == 0) return;

            var last = records.Last();

            double predicted = last.Result.StressLevel;
            double error = realStress - predicted;

            var words = Tokenize(last.Text);

            if (!_weights.ContainsKey(userId))
                _weights[userId] = new Dictionary<string, double>();

            var userWeights = _weights[userId];

            foreach (var word in words)
            {
                if (!userWeights.ContainsKey(word))
                    userWeights[word] = 0;

                userWeights[word] += error * 0.01;
            }
        }

        public double ApplyLearning(int userId, string text, double baseScore)
        {
            if (!_weights.ContainsKey(userId))
                return baseScore;

            var words = Tokenize(text);
            var userWeights = _weights[userId];

            double adjustment = 0;

            foreach (var word in words)
            {
                if (userWeights.ContainsKey(word))
                    adjustment += userWeights[word];
            }

            adjustment /= Math.Max(words.Count, 1);

            return Clamp(baseScore + adjustment, 0.1, 0.9);
        }

        private List<string> Tokenize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            return text.ToLower()
                .Split(new[] { ' ', ',', '.', '!', '?', '\n', '\r' },
                    StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }

        private double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}