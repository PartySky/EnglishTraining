using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnglishTraining.models
{
    public class VmWord
    {
        //public IList<string> items { get; set; }
        //public object items { get; set; }
        public VmWordAttribute Attributes { get; set; }
        public VmWordItem[] Items { get; set; }
    }
}
