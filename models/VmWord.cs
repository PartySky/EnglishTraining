using System;
using System.Collections.Generic;

namespace EnglishTraining
{
    public class Word
    {
        public int Id { get; set; }
        // TODO: remove it
        public string Name_en { get; set; }
        // TODO: find out is it needed there
        public string Name_ru { get; set; }
        public WordLocalization Localization { get; set; }

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
    public class WordWithLangDictionary
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
        public IList<VmCollocation> Collocation { get; set; }
    }

    public class VmWordWithDictors : Word
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
        public Word Word { get; set; }
    }
    public class FourDaysLearnPhase
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public bool Value { get; set; }
        public Word Word { get; set; }
    }
    public class RepeatIterationNum
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public int Value { get; set; }
        public Word Word { get; set; }
    }
    public class NextRepeatDate
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public DateTime Value { get; set; }
        public Word Word { get; set; }
    }
    public class DailyReapeatCount
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public int Value { get; set; }
        public Word Word { get; set; }
    }

    public class WordTest
    {
        public Dictionary<string, string> Langs { get; set; }
    }

    public class VmWord
    {
        public int Id { get; set; }
        public Dictionary<string, int> LearnDay { get; set; }
        // public IList<VmFourDaysLearnPhase> FourDaysLearnPhase { get; set; }
        public Dictionary<string, bool> FourDaysLearnPhase { get; set; }
        // public IList<VmRepeatIterationNum> RepeatIterationNum { get; set; }
        public Dictionary<string, int> RepeatIterationNum { get; set; }
        // public IList<VmNextRepeatDate> NextRepeatDate { get; set; }
        public Dictionary<string, DateTime> NextRepeatDate { get; set; }
        // public IList<VmDailyReapeatCount> DailyReapeatCount { get; set; }
        public Dictionary<string, int> DailyReapeatCount { get; set; }
        public Dictionary<string, string> LangDictionary { get; set; }
        public Dictionary<string, IList<VmDictor>> Dictors { get; set; }
        // TODO: find out should I use dictionary there?
        public IList<VmCollocation> Collocation { get; set; }
        // public Dictionary<string, IList<VmCollocation>> Collocation { get; set; }
    }

    // class depricated, to dell
    // public class VmLearnDay
    // {
    //     public int Id { get; set; }
    //     public string Key { get; set; }
    //     public int Value { get; set; }
    // }
    // class depricated, to dell
    // public class VmFourDaysLearnPhase
    // {
    //     public int Id { get; set; }
    //     public string Key { get; set; }
    //     public bool Value { get; set; }
    // }
    // class depricated, to dell
    // public class VmRepeatIterationNum
    // {
    //     public int Id { get; set; }
    //     public string Key { get; set; }
    //     public int Value { get; set; }
    // }
    // class depricated, to dell
    // public class VmNextRepeatDate
    // {
    //     public int Id { get; set; }
    //     public string Key { get; set; }
    //     public DateTime Value { get; set; }
    // }
    // class depricated, to dell
    // public class VmDailyReapeatCount
    // {
    //     public int Id { get; set; }
    //     public string Key { get; set; }
    //     public int Value { get; set; }
    // }
}
