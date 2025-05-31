using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DBAccessLibrary.Models;
//using static VerseAppAPI.VerseService;
using VerseAppAPI.Models;

namespace VerseAppAPI
{
    public static class BibleAPI
    {
        public static async Task<VerseModel> GetAPIVerseAsync(string reference, string translation)
        {
            VerseModel returnVerse = new VerseModel();

            string baseUrl = "https://bible-api.com/";
            string url = $"{baseUrl}{reference}?translation={translation}";

            using HttpClient httpClient = new();

            HttpResponseMessage response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Critical error getting data from Bible API.");

            string json = await response.Content.ReadAsStringAsync();

            Root root = JsonSerializer.Deserialize<Root>(json);
            returnVerse.Reference = root.Reference;
            foreach (var verse in root.Verses)
            {
                JsonVerse jsonVerse = new JsonVerse();
                returnVerse.Text += verse.Text;
            }

            return returnVerse;
        }

        public class Root
        {
            [JsonPropertyName("reference")]
            public string Reference { get; set; }

            [JsonPropertyName("verses")]
            public List<JsonVerse> Verses { get; set; }
        }

        public class JsonVerse
        {
            [JsonPropertyName("book_id")]
            public string BookId { get; set; }

            [JsonPropertyName("book_name")]
            public string BookName { get; set; }

            [JsonPropertyName("chapter")]
            public int Chapter { get; set; }

            [JsonPropertyName("verse")]
            public int VerseNumber { get; set; }

            [JsonPropertyName("text")]
            public string Text { get; set; }

            [JsonPropertyName("translation_id")]
            public string Translation { get; set; }

            [JsonPropertyName("translation_name")]
            public string TranslationName { get; set; }

            [JsonPropertyName("translation_note")]
            public string TranslationNote { get; set; }
        }
    }
}
