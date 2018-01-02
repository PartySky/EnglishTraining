using System.Collections.Generic;

namespace EnglishTraining
{
    public class VmResponseWord
    {
        public VmResponseWordAttributes attributes { get; set; }
        public IList<VmResponseWordItem> items { get; set; }
    }
}
