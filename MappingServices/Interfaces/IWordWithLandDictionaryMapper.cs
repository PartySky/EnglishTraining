using System;
using System.Collections.Generic;

namespace EnglishTraining
{
    public interface IVmWordMapper
    {
        WordWithLangDictionary MapToSomething(Word word);
        VmWord MapToVmWord(WordWithLangDictionary word, Dictionary<string, IList<VmDictor>> dictors);
    }
}
