using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerseAppAPI.Models;

namespace VerseAppAPI.Models
{
    public class VerseModel
    {
        public int Id { get; set; }
        public string Reference { get; set; }
        public string Text { get; set; }
        public int UsersSaved { get; set; }
        public int UsersHighlighted { get; set; }
        //private VerseService _verseservice;

        //public VerseModel(string reference, VerseService verseservice)
        //{
        //    Reference = reference;
        //    GetInfo();
        //    _verseservice = verseservice;
        //}

        //public VerseModel(string reference, string text, VerseService verseservice)
        //{
        //    Reference = reference;
        //    Text = text;
        //    GetInfo();
        //    _verseservice = verseservice;
        //}

        //public VerseModel() { }

        //private async Task GetInfo()
        //{
        //    (UsersSaved, UsersHighlighted) = await _verseservice.GetVerseInteractionInfoDBAsync(Reference);
        //}
    }

}
