using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Psicho_Support.Models
{
    public class TestAnswer
    {
        public string Text { get; set; }
        public int Score { get; set; }

        public TestAnswer() { }

        public TestAnswer(string text, int score)
        {
            Text = text;
            Score = score;
        }
    }
}
