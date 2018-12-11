using System;
using System.Collections.Generic;
using System.Linq;

namespace EnglishTraining
{
    public class VmWordMapper : IVmWordMapper
    {
        public WordWithLangDictionary MapToSomething(Word word)
        {
            return new WordWithLangDictionary
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
            };
        }

        public VmWord MapToVmWord(WordWithLangDictionary word, Dictionary<string, IList<VmDictor>> dictors)
        {
            return new VmWord
            {
                Id = word.Id,
                LearnDay = word.LearnDay?.ToDictionary(p => p.Key, v => v.Value),
                FourDaysLearnPhase = word.FourDaysLearnPhase?.ToDictionary(p => p.Key, v => v.Value),
                RepeatIterationNum = word.RepeatIterationNum?.ToDictionary(p => p.Key, v => v.Value),
                NextRepeatDate = word.NextRepeatDate?.ToDictionary(p => p.Key, v => v.Value),
                DailyReapeatCount = word.DailyReapeatCount?.ToDictionary(p => p.Key, v => v.Value),
                LangDictionary = word.LangDictionary?.ToDictionary(p => p.Key, v => v.Value),
                Dictors = dictors,
                Collocation = word.Collocation
            };
        }
    }
}
