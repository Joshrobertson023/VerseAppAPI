using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBAccessLibrary.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public int Seen { get; set; }
        public DateTime DateCreated { get; set; }
        public string Username { get; set; }
        public string SentBy { get; set; }
    }
}
