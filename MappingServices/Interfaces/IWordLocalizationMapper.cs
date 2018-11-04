using System;
using System.Collections.Generic;

namespace EnglishTraining
{
    public interface IWordLocalizationMapper
    {
        VmWordLocalization MapToVmWordLocalization(WordLocalization localization);
        List<VmWordLocalization> MapToVmWordLocalization(List<WordLocalization> localization);
    }
}
