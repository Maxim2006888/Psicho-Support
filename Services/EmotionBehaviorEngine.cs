using System;
using Psicho_Support.Models;

namespace Psicho_Support.Services
{
    public class EmotionBehaviorEngine
    {
        private readonly EmotionStateMachine _stateMachine;
        private readonly EmotionTrendAnalyzer _trendAnalyzer;
        private readonly EmotionMemoryService _memory;
        private readonly EmotionPredictor _predictor;

        public EmotionBehaviorEngine(
            EmotionStateMachine stateMachine,
            EmotionTrendAnalyzer trendAnalyzer,
            EmotionMemoryService memory,
            EmotionPredictor predictor)
        {
            _stateMachine = stateMachine;
            _trendAnalyzer = trendAnalyzer;
            _memory = memory;
            _predictor = predictor;
        }

        public EmotionSnapshot Process(EmotionResult result, string text)
        {
            var trend = _trendAnalyzer.CalculateTrend(_memory.Timeline);

            var state = _stateMachine.GetState(result.StressLevel, trend);

            var snapshot = new EmotionSnapshot
            {
                Timestamp = DateTime.Now,
                Score = result.Score,
                Stress = result.StressLevel,
                Emotion = result.DominantEmotion,
                State = state,
                SourceText = text
            };

            _memory.AddSnapshot(snapshot);

            return snapshot;
        }
    }
}