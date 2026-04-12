using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Psicho_Support.Models
{
    public class TestQuestion
    {
        public int QuestionID { get; set; }
        public string Text { get; set; }
        public ObservableCollection<TestAnswer> Answers { get; set; } = new ObservableCollection<TestAnswer>();
    }
}
