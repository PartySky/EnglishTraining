﻿using System;
using System.Collections.Generic;

namespace EnglishTraining
{
    public class VmWord
    {
        public int Id { get; set; }
        public string Name_en { get; set; }
        public string Name_ru { get; set; }
        public bool FourDaysLearnPhase { get; set; }
        public int LearnDay { get; set; }
        public int RepeatIterationNum { get; set; }
        public DateTime NextRepeatDate { get; set; }
        public int DailyReapeatCountForEng { get; set; }
        public int DailyReapeatCountForRus { get; set; }
    }

    public class VmWordWithDictors : VmWord
    {
        public IList<VmDictor> Dictors_en { get; set; }
        public IList<VmDictor> Dictors_ru { get; set; }
        public IList<VmCollocation> Collocation { get; set; }
    }
}
