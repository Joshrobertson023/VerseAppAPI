using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerseAppAPI.Models
{
    public class PasswordResetInfo
    {
        public int UserId { get; set; }
        public bool ValidToken { get; set; }
    }
}
