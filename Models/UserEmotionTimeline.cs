using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Psicho_Support.Models
{
    public class UserEmotionTimeline
    {
        public List<EmotionSnapshot> History { get; set; }

        public UserEmotionTimeline()
        {
            History = new List<EmotionSnapshot>();
        }
    }
}
