using Psicho_Support.Models;

namespace Psicho_Support.Services
{
    public class EmotionPredictor
    {
        public int PredictStress(int current, double trend)
        {
            
            int prediction = current + (int)(trend * 10);

            if (prediction < 0) return 0;
            if (prediction > 100) return 100;

            return prediction;
        }
    }
}