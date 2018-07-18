using System;
namespace EnglishTraining
{
    public class VmCollocation
    {
        public int Id { get; set; }
        public string Lang { get; set; }
        public string AudioUrl { get; set; }
        public bool NotUsedToday { get; set; }
        public DateTime NextRepeatDate { get; set; }
    }
}
