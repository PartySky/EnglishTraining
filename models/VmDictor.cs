namespace EnglishTraining
{
    public class Dictor
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Sex { get; set; }
        public string Country { get; set; }
        public string Langname { get; set; }
    }

    public class VmDictor
    {
        public string username { get; set; }
        public string sex { get; set; }
        public string country { get; set; }
        public string langname { get; set; }
        public string AudioType { get; set; }
    }
}
