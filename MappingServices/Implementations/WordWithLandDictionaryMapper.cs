using System;
using System.Collections.Generic;

namespace EnglishTraining
{
    public class WordWithLandDictionaryMapper : IWordWithLandDictionaryMapper
    {
        public WordWithLandDictionary MapToWordWithLandDictionary(VmWord word)
        {
            return new WordWithLandDictionary
            {
                Id = word.Id,
                LangDictionary = new Dictionary<string, string>
                        {
                            {"pl", word.Localization?.Name_pl},
                            {"en", word.Localization?.Name_en},
                            {"ru", word.Localization?.Name_ru},
                        },
                LearnDay = word.LearnDay,
                FourDaysLearnPhase = word.FourDaysLearnPhase,
                RepeatIterationNum = word.RepeatIterationNum,
                NextRepeatDate = word.NextRepeatDate,
                DailyReapeatCount = word.DailyReapeatCount,
                // ?
                Dictors = null

                // ?
                //    public Dictionary<string, IList<VmDictor>> Dictors { get; set; }
                //    public IList<VmCollocation> Collocation { get; set; }
            };
        }
    }
}
