using System;
using System.Collections.Generic;

namespace EnglishTraining
{
    public class WordLocalizationMapper : IWordLocalizationMapper
    {
        public VmWordLocalization MapToVmWordLocalization(WordLocalization localization)
        {
            VmWordLocalization result = new VmWordLocalization
            {
                Id = localization.Id,
                LangDictionary = new Dictionary<string, string>
                {
                    { "pl", localization.Name_pl },
                    { "en", localization.Name_en },
                    { "ru", localization.Name_ru }
                }
            };

            return result;
        }

        public List<VmWordLocalization> MapToVmWordLocalization(List<WordLocalization> localizationList)
        {
            List<VmWordLocalization> result = new List<VmWordLocalization>();
            foreach (var item in localizationList)
            {
                result.Add(MapToVmWordLocalization(item));
            }
            return result;
        }
    }
}
