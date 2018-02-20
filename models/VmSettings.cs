using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnglishTraining.models
{
    public class VmSettings
    {
        public int Id { get; set; }
        public string LearningLanguage { get; set; }
        public int? DailyWordsAmount { get; set; }
        public int? DailyTimeAmount { get; set; }
    }
}
