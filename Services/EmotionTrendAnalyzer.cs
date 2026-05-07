using System;
using System.Linq;
using Psicho_Support.Models;

namespace Psicho_Support.Services
{
    public class EmotionTrendAnalyzer
    {
        public double CalculateTrend(UserEmotionTimeline timeline)
        {
            if (timeline.History.Count < 2)
                return 0;

            var last = timeline.History
                .Skip(Math.Max(0, timeline.History.Count - 5))
                .ToList();

            double first = last.First().Stress;
            double lastValue = last.Last().Stress;

            return (lastValue - first) / 100.0;
        }
    }
}