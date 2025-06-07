using Oracle.ManagedDataAccess.Client;
using VerseAppAPI.Models;
using static System.Reflection.Metadata.BlobBuilder;

namespace VerseAppAPI.Controllers
{
    public class VerseControllerDB
    {
        #region connectionString
        private string connectionString;
        private IConfiguration _config;
        public VerseControllerDB(IConfiguration config)
        {
            _config = config;
            connectionString = _config.GetConnectionString("Default");
        }
        #endregion

        public static string[] books { get; } =
{
            "Genesis", "Exodus", "Leviticus", "Numbers", "Deuteronomy",
            "Joshua", "Judges", "Ruth", "1 Samuel", "2 Samuel",
            "1 Kings", "2 Kings", "1 Chronicles", "2 Chronicles", "Ezra",
            "Nehemiah", "Esther", "Job", "Psalms", "Proverbs",
            "Ecclesiastes", "Song of Solomon", "Isaiah", "Jeremiah", "Lamentations",
            "Ezekiel", "Daniel", "Hosea", "Joel", "Amos",
            "Obadiah", "Jonah", "Micah", "Nahum", "Habakkuk",
            "Zephaniah", "Haggai", "Zechariah", "Malachi",
            "Matthew", "Mark", "Luke", "John", "Acts",
            "Romans", "1 Corinthians", "2 Corinthians", "Galatians", "Ephesians",
            "Philippians", "Colossians", "1 Thessalonians", "2 Thessalonians", "1 Timothy",
            "2 Timothy", "Titus", "Philemon", "Hebrews", "James",
            "1 Peter", "2 Peter", "1 John", "2 John", "3 John",
            "Jude", "Revelation"
        };

        public async Task AddAllVersesOfBibleDBAsync()
        {
            string query = @"
                            INSERT INTO VERSES 
                            (VERSE_REFERENCE, USERS_SAVED_VERSE, USERS_HIGHLIGHTED_VERSE)
                            VALUES 
                            (:reference, :saved, :highlighted)";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);

            string reference = "";
            int saved = 0;
            int highlighted = 0;
            string text = "";

            var referenceParameter = new OracleParameter("reference", reference);
            var savedParameter = new OracleParameter("saved", saved);
            var highlightedParameter = new OracleParameter("highlighted", highlighted);
            var textParameter = new OracleParameter("text", text);
            cmd.Parameters.Add(referenceParameter);
            cmd.Parameters.Add(savedParameter);
            cmd.Parameters.Add(highlightedParameter);
            cmd.Parameters.Add(textParameter);

            for (int i = 0; i < books.Count(); i++)
            {
                for (int j = 0; j <= BibleStructure.GetNumberChapters(books[i]); j++)
                {
                    for (int k = 0; k < BibleStructure.GetNumberVerses(books[i], j); k++)
                    {
                        List<int> verse = new List<int>() { k + 1 };
                        referenceParameter.Value = ReferenceParse.ConvertToReferenceString(books[i], j, verse);
                        savedParameter.Value = 0;
                        highlightedParameter.Value = 0;
                        VerseModel verseModel = await BibleAPI.GetAPIVerseAsync(ReferenceParse.ConvertToReferenceString(books[i], j, verse), "kjv");
                        textParameter.Value = verseModel.Text;

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }

            conn.Close();
            conn.Dispose();
        }
    }
}
