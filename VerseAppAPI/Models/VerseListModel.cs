using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerseAppAPI.Models
{
    public class VerseListModel
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public string Passages { get; set; }
        public int ViewedBy { get; set; }
    }
}
