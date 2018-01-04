using System;
using System.Collections.Generic;

namespace EnglishTraining.models.Commonmodels
{
    public class VmCommonTableResponse<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
