using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishTraining
{
    public class VmWordLocalization
    {
        [ForeignKey("Word")]
        public int Id { get; set; }
        public Dictionary<string, string> LangDictionary { get; set; }
        public Word word { get; set; }
    }
}
