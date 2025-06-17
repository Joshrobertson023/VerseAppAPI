using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerseAppAPI.Models;

namespace VerseAppAPI
{
    public static class ReferenceParse
    {
        public static List<string> ConvertToReferenceParts(string reference)
        {
            List<string> components = new List<string>();
            string builder = "";

            for (int i = 0; i < reference.Length; i++)
            {
                builder += reference[i];

                if (i < reference.Length - 1)
                {
                    if (reference[i+1] == ' ' || reference[i+1] == ':')
                    {
                        components.Add(builder);
                        builder = "";
                        i++;
                    }
                }
            }
            components.Add(builder);

            return components;
        }

        public static string ConvertToReferenceString(string book, int chapter, List<int> verses)
        {
            string returnString = "";

            returnString += book + " " + chapter.ToString() + ":";

            if (verses.Count > 1)
            {
                for (int i = 0; i < verses.Count; i++)
                {
                    returnString += verses[i].ToString();
                    if (i < verses.Count - 1)
                        returnString += ",";
                }
            }
            else
            {
                returnString += verses[0].ToString();
            }

            return returnString;
        }

        public static string ConvertToReferenceString(string book, int chapter, int verse)
        {
            string returnString = "";

            returnString += book + " " + chapter.ToString() + ":";

            returnString += verse.ToString();

            return returnString;
        }

        public static string ConvertToReadableReference(string book, int chapter, List<int> verses)
        {
            string returnString = "";
            verses.Sort();
            List<int> consecutiveVerses = new List<int>();

            returnString += book + " " + chapter.ToString() + ":";

            if (verses.Count > 1)
            {
                for (int i = 0; i < verses.Count-1; i++)
                {
                    if (verses[i] == verses[i + 1])
                    {
                        if (!consecutiveVerses.Contains(verses[i]))
                            consecutiveVerses.Add(verses[i]);
                        consecutiveVerses.Add(verses[i + 1]);
                        i++;
                    }
                    else
                    {
                        returnString += verses[i].ToString();
                        if (i < verses.Count - 1)
                            returnString += ", ";
                    }

                }
            }
            else
            {
                returnString += verses[0].ToString();
            }

            return returnString;
        }

        public static List<int> GetIndividualVerses(string reference)
        {
            List<int> returnList = new List<int>();
            string verses = ConvertToReferenceParts(reference)[2];

            if (verses.Length > 1)
                for (int i = 0; i < verses.Length-1; i += 2)
                    returnList.Add(Convert.ToInt32(verses[i]));
            returnList.Add(Convert.ToInt32(verses[verses.Length - 1]));

            return returnList;
        }

        public static List<string> GetIndividualVersesWithReference(string reference)
        {
            List<string> references = new List<string>();

            List<string> parts = ConvertToReferenceParts(reference);
            string book = parts[0];
            string chapter = parts[1];
            string versesPart = parts[2];

            string[] segments = versesPart.Split(',');
            for (int i = 0; i < segments.Length; i++)
            {
                string seg = segments[i].Trim();

                if (seg.Contains('-'))
                {
                    string[] bounds = seg.Split('-');
                    int start = Convert.ToInt32(bounds[0]);
                    int end = Convert.ToInt32(bounds[1]);

                    for (int v = start; v <= end; v++)
                    {
                        references.Add($"{book} {chapter}:{v}");
                    }
                }
                else
                {
                    int v = Convert.ToInt32(seg);
                    references.Add($"{book} {chapter}:{v}");
                }
            }

            return references;
        }
    }
}
