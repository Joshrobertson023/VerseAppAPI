using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerseAppAPI.Models
{
    public class UserVerseModel
    {
        public int VerseId { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public string Reference { get; set; }
        public DateTime LastPracticed { get; set; }
        public DateTime DateMemorized { get; set; }
        public string Translation { get; set; }
        public float ProgressPercent { get; set; }
        public int TimesReviewed { get; set; }
        public int TimesMemorized { get; set; }
        public int Visibility { get; set; }
    }
}
