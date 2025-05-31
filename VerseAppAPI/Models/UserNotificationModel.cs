using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerseAppAPI.Models
{
    public class UserNotificationModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Type { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
