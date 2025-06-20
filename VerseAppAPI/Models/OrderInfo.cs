using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBAccessLibrary.Models
{
    public class OrderInfo
    {
        public int UserId { get; set; }
        public int OrderBy { get; set; }
        public string Order { get; set; }
    }
}
