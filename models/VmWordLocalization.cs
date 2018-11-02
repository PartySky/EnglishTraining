using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishTraining
{
    public class VmWordLocalization
    {
        [ForeignKey("Word")]
        public int Id { get; set; }

        public string Name_pl { get; set; }
        public string Name_en { get; set; }
        public string Name_ru { get; set; }
        //public string Name_es { get; set; }


        public Word word { get; set; }
    }
}
