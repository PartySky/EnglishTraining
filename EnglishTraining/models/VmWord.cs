using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaveChart.models
{
    public class VmWord
    {
        //public IList<string> items { get; set; }
        //public object items { get; set; }
        public VmWordAttribute attributes { get; set; }
        public VmWordItem[] items { get; set; }
    }
}
