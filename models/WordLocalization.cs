using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishTraining
{
    public class WordLocalization
    {
        [ForeignKey("Word")]
        public int Id { get; set; }

        public string Name_pl { get; set; }
        public string Name_en { get; set; }
        public string Name_ru { get; set; }

        public Word word { get; set; }
    }

    public class LocalizationImport
    {
        public int Id { get; set; }
        public string Name_pl { get; set; }
        public string Name_en { get; set; }
        public string Name_ru { get; set; }
    }
}
