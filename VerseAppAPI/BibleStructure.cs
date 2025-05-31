using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VerseAppAPI.Models;

namespace VerseAppAPI
{
    public static class BibleStructure
    {
        public static int GetNumberChapters(string book)
        {
            int numChapters = 0;

            Dictionary<string, Dictionary<string, int>> numChaptersVerses;

            using (StreamReader reader = new StreamReader(Path.Combine(AppContext.BaseDirectory, "bible_structure.json")))
            {
                string json = reader.ReadToEnd();
                numChaptersVerses = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(json);
            }

            if (numChaptersVerses.ContainsKey(book))
            {
                numChapters = numChaptersVerses[book].Count;
            }

            return numChapters;
        }

        public static int GetNumberVerses(string book, int chapter)
        {
            int numVerses = 0;

            Dictionary<string, Dictionary<string, int>> numChaptersVerses;

            using (StreamReader reader = new StreamReader(Path.Combine(AppContext.BaseDirectory, "bible_structure.json")))
            {
                string json = reader.ReadToEnd();
                numChaptersVerses = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(json);
            }

            if (numChaptersVerses.ContainsKey(book))
            {
                if (numChaptersVerses[book].ContainsKey(chapter.ToString()))
                {
                    numVerses = numChaptersVerses[book][chapter.ToString()];
                }
            }

            return numVerses;
        }
    }
}
