using System;
namespace EnglishTraining
{
    public interface IWordWithLandDictionaryMapper
    {
		WordWithLandDictionary MapToWordWithLandDictionary(VmWord word);
    }
}
