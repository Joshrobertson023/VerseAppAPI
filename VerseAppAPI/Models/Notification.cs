using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerseAppAPI;

namespace DBAccessLibrary.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public Enums.NotificationType Type { get; set; }
        public string Text { get; set; }
        public DateTime DateCreated { get; set; }
        public string SentBy { get; set; }
        public string ReceivingUser { get; set; }
    }
}
