using Psicho_Support.Data;
using Psicho_Support.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Psicho_Support.Services
{
    public class TimelineAnalyticsService
    {
        private readonly EmotionMemoryService _memory;
        private readonly EmotionTrendAnalyzer _trendAnalyzer;

        public TimelineAnalyticsService(EmotionMemoryService memory, EmotionTrendAnalyzer trendAnalyzer)
        {
            _memory = memory;
            _trendAnalyzer = trendAnalyzer;
        }

        public EmotionalProfile BuildProfile(int userId, int days = 7)
        {
            var snapshots = GetSnapshots(userId, days);
            if (!snapshots.Any())
            {
                return new EmotionalProfile
                {
                    AverageStress = 0,
                    DominantEmotion = EmotionType.Neutral,
                    StabilityIndex = 100,
                    FluctuationIndex = 0,
                    StressLoadIndex = 0,
                    BurnoutRisk = 0,
                    CurrentPhase = EmotionalPhase.Calm,
                    PredictionReason = "Недостаточно данных, состояние принято как спокойное."
                };
            }

            var stress = snapshots.Select(s => (double)s.Stress).ToList();
            var avgStress = stress.Average();
            var variance = stress.Count > 1 ? stress.Select(s => Math.Pow(s - avgStress, 2)).Average() : 0;
            var stdev = Math.Sqrt(variance);
            var stability = Clamp(100 - (float)(stdev * 2), 0, 100);

            var jumps = 0;
            for (int i = 1; i < stress.Count; i++)
            {
                if (Math.Abs(stress[i] - stress[i - 1]) >= 15)
                    jumps++;
            }
            var fluctuation = stress.Count > 1
                ? Clamp((float)jumps / (stress.Count - 1) * 100, 0, 100)
                : 0;

            var stressLoad = Clamp((float)avgStress, 0, 100);

            var trendTimeline = new UserEmotionTimeline { History = snapshots };
            var trend = _trendAnalyzer.CalculateTrend(trendTimeline);
            var burnoutRisk = Clamp((float)(avgStress * 0.5 + (1 - stability / 100f) * 35 + Math.Max(0, trend) * 30), 0, 100);

            var phase = ResolvePhase(avgStress, stability, trend, burnoutRisk, out var reason);

            return new EmotionalProfile
            {
                AverageStress = (float)avgStress,
                DominantEmotion = snapshots
                    .GroupBy(s => s.Emotion)
                    .OrderByDescending(g => g.Count())
                    .First().Key,
                StabilityIndex = stability,
                FluctuationIndex = fluctuation,
                StressLoadIndex = stressLoad,
                BurnoutRisk = burnoutRisk,
                CurrentPhase = phase,
                PredictionReason = reason
            };
        }

        public List<StressPoint> BuildStressDynamics(int userId, int days = 7)
        {
            var snapshots = GetSnapshots(userId, days);
            if (!snapshots.Any()) return new List<StressPoint>();

            return snapshots
                .GroupBy(s => s.Timestamp.Date)
                .OrderBy(g => g.Key)
                .Select(g => new StressPoint
                {
                    DayOfWeek = g.Key.ToString("dd.MM"),
                    Stress = (int)Math.Round(g.Average(x => x.Stress))
                })
                .ToList();
        }

        private List<EmotionSnapshot> GetSnapshots(int userId, int days)
        {
            var from = DateTime.Now.AddDays(-Math.Max(1, days));
            var timeline = _memory.Timeline?.History?
                .Where(h => h.Timestamp >= from)
                .OrderBy(h => h.Timestamp)
                .ToList() ?? new List<EmotionSnapshot>();

            if (timeline.Any())
                return timeline;

            using (var db = new HealthPsicho_DBEntities())
            {
                return db.Notes
                    .Where(n => n.UserID == userId && n.CreatedAt.HasValue && n.CreatedAt >= from && n.StressLevel.HasValue)
                    .OrderBy(n => n.CreatedAt)
                    .Select(n => new EmotionSnapshot
                    {
                        Timestamp = n.CreatedAt.Value,
                        Stress = n.StressLevel.Value,
                        Score = 1 - (n.StressLevel.Value / 100.0),
                        Emotion = EmotionType.Neutral,
                        State = EmotionalState.Neutral,
                        SourceText = n.Content
                    })
                    .ToList();
            }
        }

        private static float Clamp(float value, float min, float max) => Math.Max(min, Math.Min(max, value));

        private static EmotionalPhase ResolvePhase(double avgStress, float stability, double trend, float risk, out string reason)
        {
            if (risk >= 70 || (avgStress >= 70 && trend > 0.2))
            {
                reason = "Высокий риск: высокий средний стресс и нарастающий тренд.";
                return EmotionalPhase.BurnoutRisk;
            }

            if (avgStress >= 55 || trend > 0.15)
            {
                reason = "Наблюдается напряжение: стресс выше нормы или положительный тренд стресса.";
                return EmotionalPhase.Tension;
            }

            if (trend < -0.1 && stability >= 55)
            {
                reason = "Есть восстановление: стресс снижается, стабильность приемлемая.";
                return EmotionalPhase.Recovery;
            }

            reason = "Состояние стабильно, без признаков роста напряжения.";
            return EmotionalPhase.Calm;
        }
    }
}