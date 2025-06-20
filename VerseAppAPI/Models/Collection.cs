using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerseAppAPI.Models;

namespace DBAccessLibrary.Models
{
    public class Collection
    {
        public int Id { get; set; }
        public string Author { get; set; }
        public int UserId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastPracticed { get; set; }
        public float ProgressPercent { get; set; }
        public string Title { get; set; }
        public int NumVerses { get; set; }
        public int Visibility { get; set; }
        public int IsPublished { get; set; }
        public int NumSaves { get; set; }
        public int Pinned { get; set; }
        public string VerseOrder { get; set; }
        public List<UserVerse> UserVerses { get; set; } = new List<UserVerse>();
    }
}
