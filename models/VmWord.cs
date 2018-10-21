using System;
using System.Collections.Generic;

namespace EnglishTraining
{
    public class VmWord
    {
        public int Id { get; set; }
        public string Name_en { get; set; }
        public string Name_ru { get; set; }
        public VmWordLocalization Localization { get; set; }

        public IList<LearnDay> LearnDay { get; set; }
        public IList<FourDaysLearnPhase> FourDaysLearnPhase { get; set; }
        public IList<RepeatIterationNum> RepeatIterationNum { get; set; }
        public IList<NextRepeatDate> NextRepeatDate { get; set; }
        public IList<DailyReapeatCount> DailyReapeatCount { get; set; }

        public bool FourDaysLearnPhaseOld { get; set; }
        public int LearnDayOld { get; set; }
        public int RepeatIterationNumOld { get; set; }
        public DateTime NextRepeatDateOld { get; set; }
        public int DailyReapeatCountForEngOld { get; set; }
        public int DailyReapeatCountForRusOld { get; set; }
    }


    // Lets map it
    //public class WordWithLandDictionary : VmWord
    public class WordWithLandDictionary
    {
        public int Id { get; set; }
        // is it needed?
        //public VmWordLocalization Localization { get; set; }

        public IList<LearnDay> LearnDay { get; set; }
        public IList<FourDaysLearnPhase> FourDaysLearnPhase { get; set; }
        public IList<RepeatIterationNum> RepeatIterationNum { get; set; }
        public IList<NextRepeatDate> NextRepeatDate { get; set; }
        public IList<DailyReapeatCount> DailyReapeatCount { get; set; }

        public Dictionary<string, string> LangDictionary { get; set; }
        public Dictionary<string, IList<VmDictor>> Dictors { get; set; }
        public IList<VmCollocation> Collocation { get; set; }
    }

    public class VmWordWithDictors : VmWord
    {
        public IList<VmDictor> Dictors_en { get; set; }
        public IList<VmDictor> Dictors_ru { get; set; }
        public IList<VmCollocation> Collocation { get; set; }
    }
    public class LearnDay
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public int Value { get; set; }
        public VmWord Word { get; set; }
    }
    public class FourDaysLearnPhase
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public bool Value { get; set; }
        public VmWord Word { get; set; }
    }
    public class RepeatIterationNum
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public int Value { get; set; }
        public VmWord Word { get; set; }
    }
    public class NextRepeatDate
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public DateTime Value { get; set; }
        public VmWord Word { get; set; }
    }
    public class DailyReapeatCount
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public int Value { get; set; }
        public VmWord Word { get; set; }
    }

    public class Word
    {
        public Dictionary<string, string> Langs { get; set; }
    }

    public class WordWithLandDictionaryOutput
    {
        public int Id { get; set; }

        public IList<LearnDayOutput> LearnDay { get; set; }
        public IList<FourDaysLearnPhaseOutput> FourDaysLearnPhase { get; set; }
        public IList<RepeatIterationNumOutput> RepeatIterationNum { get; set; }
        public IList<NextRepeatDateOutput> NextRepeatDate { get; set; }
        public IList<DailyReapeatCountOutput> DailyReapeatCount { get; set; }

        public Dictionary<string, string> LangDictionary { get; set; }
        public Dictionary<string, IList<VmDictor>> Dictors { get; set; }
        public IList<VmCollocation> Collocation { get; set; }
    }

    public class LearnDayOutput
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public int Value { get; set; }
    }
    public class FourDaysLearnPhaseOutput
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public bool Value { get; set; }
    }
    public class RepeatIterationNumOutput
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public int Value { get; set; }
    }
    public class NextRepeatDateOutput
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public DateTime Value { get; set; }
    }
    public class DailyReapeatCountOutput
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public int Value { get; set; }
    }
}
