namespace VerseAppAPI.Models
{
    public class Reference
    {
        public string Book { get; set; }
        public int Chapter { get; set; }
        public List<int> Verses { get; set; }
    }
}
