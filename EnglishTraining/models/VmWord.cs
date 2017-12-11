using System.Collections.Generic;

namespace EnglishTraining
{
    public class VmWord
    {
        public string Name_en { get; set; }
        public string Name_ru { get; set; }
        public bool FourDausLearnPhase { get; set; }
        public int LearnDay { get; set; }
        public int RepeatIterationNum { get; set; }
        public string NextRepeatDate { get; set; }
        public int DailyReapeatCountForEng { get; set; }
        public int DailyReapeatCountForRus { get; set; }
        public IList<VmDictor> Dictors_en { get; set; }
        public IList<VmDictor> Dictors_ru { get; set; }
    }
}
