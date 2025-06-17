using DBAccessLibrary.Models;
using Oracle.ManagedDataAccess.Client;
using System.Collections.ObjectModel;
using System.Reflection.PortableExecutable;
using VerseAppAPI.Models;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
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

        public async Task<UserVerse> GetUserVerseFromReference(Reference reference)
        {
            UserVerse userVerse = new();
            List<string> references = new();

            foreach (var _verse in reference.Verses)
            {
                references.Add(reference.Book + " " + reference.Chapter.ToString() + ":" + _verse.ToString());
            }

            List<Verse> resultVerses = new();
            foreach (var _reference in references)
            {
                resultVerses.Add(await GetVerse(_reference));
            }
            userVerse.Verses = resultVerses;

            return userVerse;
        }

        public async Task<Verse> GetVerse(string reference)
        {
            Verse verse = new();
            string query = @"SELECT * FROM VERSES WHERE VERSE_REFERENCE = :reference";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();
            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("reference", reference));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                verse.Id = reader.GetInt32(reader.GetOrdinal("VERSE_ID"));
                verse.Reference = reader.GetString(reader.GetOrdinal("VERSE_REFERENCE"));
                verse.UsersSaved = reader.GetInt32(reader.GetOrdinal("USERS_SAVED_VERSE"));
                verse.UsersMemorized = reader.GetInt32(reader.GetOrdinal("USERS_MEMORIZED"));
                verse.Text = reader.GetString(reader.GetOrdinal("TEXT"));
            }

            conn.Close();
            conn.Dispose();
            return verse;
        }

        public async Task<List<Verse>> SingleKeyword(string keyword)
        {
            List<Verse> verses = new List<Verse>();

            string query = @"SELECT * FROM VERSES WHERE CONTAINS(TEXT, :keyword, 1) > 0 ORDER BY SCORE(1) DESC, INSTR(UPPER(TEXT), UPPER(:keyword)) ASC FETCH FIRST 100 ROWS ONLY";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();
            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("keyword", "%" + keyword + "%"));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Verse verse = new Verse
                {
                    Id = reader.GetInt32(reader.GetOrdinal("VERSE_ID")),
                    Reference = reader.GetString(reader.GetOrdinal("VERSE_REFERENCE")),
                    UsersSaved = reader.GetInt32(reader.GetOrdinal("USERS_SAVED_VERSE")),
                    UsersMemorized = reader.GetInt32(reader.GetOrdinal("USERS_MEMORIZED")),
                    Text = reader.GetString(reader.GetOrdinal("TEXT"))
                };
                verses.Add(verse);
            }

            if (verses.Count < 20)
            {
                verses.Add(new Verse { Id = 0, Reference = "No more results found; please check your spelling.", Text = "", UsersSaved = 0, UsersMemorized = 0 });
            }

            conn.Close();
            conn.Dispose();
            return verses;
        }

        public async Task<List<Verse>> ExactPhraseVerses(List<string> keywords)
        {
            List<Verse> verses = new List<Verse>();

            string phrase = string.Join(" ", keywords);

            string query = @"SELECT * FROM VERSES WHERE CONTAINS(TEXT, :phrase, 1) > 0 ORDER BY SCORE(1) DESC FETCH FIRST 1 ROWS ONLY";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("phrase", phrase));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Verse verse = new Verse
                {
                    Id = reader.GetInt32(reader.GetOrdinal("VERSE_ID")),
                    Reference = reader.GetString(reader.GetOrdinal("VERSE_REFERENCE")),
                    UsersSaved = reader.GetInt32(reader.GetOrdinal("USERS_SAVED_VERSE")),
                    UsersMemorized = reader.GetInt32(reader.GetOrdinal("USERS_MEMORIZED")),
                    Text = reader.GetString(reader.GetOrdinal("TEXT"))
                };
                verses.Add(verse);
            }

            conn.Close();
            conn.Dispose();
            return verses;
        }

        public async Task<List<Verse>> NearExactAndVerses(List<string> keywords)
        {
            List<Verse> verses = new List<Verse>();

            string query = @"SELECT * FROM VERSES WHERE CONTAINS(TEXT, :keywords, 1) > 0 ORDER BY SCORE(1) DESC FETCH FIRST 10 ROWS ONLY";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);

            List<string> parts = new List<string>();
            foreach (var _keyword in keywords)
            {
                parts.Add(_keyword.ToLower());
            }
            string andKeywords = string.Join(" NEAR ", parts);

            cmd.Parameters.Add(new OracleParameter("keywords", andKeywords));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Verse verse = new Verse
                {
                    Id = reader.GetInt32(reader.GetOrdinal("VERSE_ID")),
                    Reference = reader.GetString(reader.GetOrdinal("VERSE_REFERENCE")),
                    UsersSaved = reader.GetInt32(reader.GetOrdinal("USERS_SAVED_VERSE")),
                    UsersMemorized = reader.GetInt32(reader.GetOrdinal("USERS_MEMORIZED")),
                    Text = reader.GetString(reader.GetOrdinal("TEXT"))
                };
                verses.Add(verse);
            }

            conn.Close();
            conn.Dispose();
            return verses;
        }

        public async Task<List<Verse>> ExactAndVerses(List<string> keywords)
        {
            List<Verse> verses = new List<Verse>();

            string query = @"SELECT * FROM VERSES WHERE CONTAINS(TEXT, :keywords, 1) > 0 ORDER BY SCORE(1) DESC FETCH FIRST 10 ROWS ONLY";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);

            List<string> parts = new List<string>();
            foreach (var _keyword in keywords)
            {
                parts.Add(_keyword.ToLower());
            }
            string andKeywords = string.Join(" AND ", parts);

            cmd.Parameters.Add(new OracleParameter("keywords", andKeywords));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Verse verse = new Verse
                {
                    Id = reader.GetInt32(reader.GetOrdinal("VERSE_ID")),
                    Reference = reader.GetString(reader.GetOrdinal("VERSE_REFERENCE")),
                    UsersSaved = reader.GetInt32(reader.GetOrdinal("USERS_SAVED_VERSE")),
                    UsersMemorized = reader.GetInt32(reader.GetOrdinal("USERS_MEMORIZED")),
                    Text = reader.GetString(reader.GetOrdinal("TEXT"))
                };
                verses.Add(verse);
            }

            conn.Close();
            conn.Dispose();
            return verses;
        }

        public async Task<List<Verse>> NearAnd(List<string> keywords)
        {
            List<Verse> verses = new List<Verse>();

            string query = @"SELECT * FROM VERSES WHERE CONTAINS(TEXT, :keywords, 1) > 0 ORDER BY SCORE(1) DESC FETCH FIRST 50 ROWS ONLY";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);

            List<string> parts = new List<string>();
            foreach (var _keyword in keywords)
            {
                parts.Add("%" + _keyword.ToLower() + "%");
            }
            string andKeywords = string.Join(" NEAR ", parts);

            cmd.Parameters.Add(new OracleParameter("keywords", andKeywords));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Verse verse = new Verse
                {
                    Id = reader.GetInt32(reader.GetOrdinal("VERSE_ID")),
                    Reference = reader.GetString(reader.GetOrdinal("VERSE_REFERENCE")),
                    UsersSaved = reader.GetInt32(reader.GetOrdinal("USERS_SAVED_VERSE")),
                    UsersMemorized = reader.GetInt32(reader.GetOrdinal("USERS_MEMORIZED")),
                    Text = reader.GetString(reader.GetOrdinal("TEXT"))
                };
                verses.Add(verse);
            }

            conn.Close();
            conn.Dispose();
            return verses;
        }

        public async Task<List<Verse>> AndVerses(List<string> keywords)
        {
            List<Verse> verses = new List<Verse>();

            string query = @"SELECT * FROM VERSES WHERE CONTAINS(TEXT, :keywords, 1) > 0 ORDER BY SCORE(1) DESC FETCH FIRST 49 ROWS ONLY";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);

            List<string> parts = new List<string>();
            foreach (var _keyword in keywords)
            {
                parts.Add("%" + _keyword.ToLower() + "%");
            }
            string andKeywords = string.Join(" AND ", parts);

            cmd.Parameters.Add(new OracleParameter("keywords", andKeywords));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Verse verse = new Verse
                {
                    Id = reader.GetInt32(reader.GetOrdinal("VERSE_ID")),
                    Reference = reader.GetString(reader.GetOrdinal("VERSE_REFERENCE")),
                    UsersSaved = reader.GetInt32(reader.GetOrdinal("USERS_SAVED_VERSE")),
                    UsersMemorized = reader.GetInt32(reader.GetOrdinal("USERS_MEMORIZED")),
                    Text = reader.GetString(reader.GetOrdinal("TEXT"))
                };
                verses.Add(verse);
            }

            conn.Close();
            conn.Dispose();
            return verses;
        }

        public async Task<List<Verse>> OrVerses(List<string> keywords)
        {
            List<Verse> verses = new List<Verse>();

            string query = @"SELECT * FROM VERSES WHERE CONTAINS(TEXT, :keywords, 1) > 0 ORDER BY SCORE(1) DESC FETCH FIRST 20 ROWS ONLY";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);

            List<string> parts = new List<string>();
            foreach (var _keyword in keywords)
            {
                parts.Add("%" + _keyword.ToLower() + "%");
            }
            string orKeywords = string.Join(" OR ", parts);

            cmd.Parameters.Add(new OracleParameter("keywords", orKeywords));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Verse verse = new Verse
                {
                    Id = reader.GetInt32(reader.GetOrdinal("VERSE_ID")),
                    Reference = reader.GetString(reader.GetOrdinal("VERSE_REFERENCE")),
                    UsersSaved = reader.GetInt32(reader.GetOrdinal("USERS_SAVED_VERSE")),
                    UsersMemorized = reader.GetInt32(reader.GetOrdinal("USERS_MEMORIZED")),
                    Text = reader.GetString(reader.GetOrdinal("TEXT"))
                };
                verses.Add(verse);
            }

            conn.Close();
            conn.Dispose();
            return verses;
        }

        public async Task<List<Verse>> GetUserVerseByKeywords(List<string> keywords)
        {
            List<Verse> verses = new List<Verse>();
            HashSet<string> added = new HashSet<string>();

            if (keywords.Count == 1)
                return await SingleKeyword(keywords[0]);

            List<Verse> nearExactAnd = new List<Verse>();
            List<Verse> nearAnd = new List<Verse>();
            List<Verse> andVerses = new List<Verse>();
            List<Verse> exactPhrase = new List<Verse>();

            //if (keywords.Count == 2)
            //    nearExactAnd = await NearExactAndVerses(keywords);
            //List<Verse> exactAnd = await ExactAndVerses(keywords);
            if (keywords.Count == 2)
                nearAnd = await NearAnd(keywords);
            if (keywords.Count > 2 && keywords.Count < 5)
            {
                exactPhrase = await ExactPhraseVerses(keywords);
                andVerses = await AndVerses(keywords);
            }
            if (keywords.Count >= 5)
                exactPhrase = await ExactPhraseVerses(keywords);
            //List<Verse> orVerses = await OrVerses(keywords);

            foreach (var verse in exactPhrase)
                if (added.Add(verse.Reference))
                    verses.Add(verse);

            //foreach (var verse in nearExactAnd)
            //    if (added.Add(verse.Reference))
            //        verses.Add(verse);

            //foreach (var verse in exactAnd)
            //    if (added.Add(verse.Reference))
            //        verses.Add(verse);

            foreach (var verse in nearAnd)
                if (added.Add(verse.Reference))
                    verses.Add(verse);

            foreach (var verse in andVerses)
                if (added.Add(verse.Reference))
                    verses.Add(verse);

            //foreach (var verse in orVerses)
            //    if (added.Add(verse.Reference))
            //        verses.Add(verse);

            verses.Add(new Verse { Reference = "No more results. Check your spelling." });

            return verses;
        }

        public async Task AddNewCollection(Collection collection)
        {
            string query = @"INSERT INTO COLLECTIONS (AUTHOR, TITLE, NUM_VERSES, VISIBILITY, IS_PUBLISHED, NUM_SAVES, USER_ID) VALUES (:author, :title, :numVerses, :visibility, :isPublished, :numSaves, :userId)";
            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();
            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("author", collection.Author));
            cmd.Parameters.Add(new OracleParameter("title", collection.Title));
            cmd.Parameters.Add(new OracleParameter("numVerses", collection.NumVerses));
            cmd.Parameters.Add(new OracleParameter("visibility", collection.Visibility));
            cmd.Parameters.Add(new OracleParameter("userId", collection.UserId));
            cmd.Parameters.Add(new OracleParameter("isPublished", (object)0));
            cmd.Parameters.Add(new OracleParameter("numSaves", (object)0));
            await cmd.ExecuteNonQueryAsync();
            conn.Close();
            conn.Dispose();
        }

        public async Task<List<Collection>> GetUserCollections(int userId)
        {
            List<Collection> collections = new List<Collection>();

            string query =
                @"      SELECT
                        c.collection_id,
                        c.date_created,
                        c.last_practiced   AS c_last_practiced,
                        c.progress_percent AS c_progress_percent,
                        c.author,
                        c.title,
                        c.num_verses,
                        c.visibility,
                        c.is_published,
                        c.num_saves,
                        c.user_id,
                        uv.verse_id,
                        uv.user_id,
                        uv.reference,
                        uv.last_practiced,
                        uv.date_memorized,
                        uv.progress_percent,
                        uv.times_reviewed,
                        uv.times_memorized,
                        uv.date_saved
                      FROM collections c
                      LEFT JOIN user_verses uv
                        ON c.collection_id = uv.collection_id
                        AND uv.user_id      = :userId
                      WHERE c.user_id    = :userId
                      ORDER BY c.collection_id";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();
            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("userId", userId));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            Collection newCollection = new Collection();
            List<int> collectionIds = new List<int>();
            while (await reader.ReadAsync())
            {
                int collectionId = reader.GetInt32(reader.GetOrdinal("COLLECTION_ID"));
                if (!collectionIds.Contains(collectionId))
                {
                    newCollection = new Collection
                    {
                        Id = collectionId,
                        Author = reader.GetString(reader.GetOrdinal("AUTHOR")),
                        UserId = reader.GetInt32(reader.GetOrdinal("USER_ID")),
                        DateCreated = reader.GetDateTime(reader.GetOrdinal("DATE_CREATED")),
                        LastPracticed = reader.IsDBNull(reader.GetOrdinal("C_LAST_PRACTICED")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("C_LAST_PRACTICED")),
                        ProgressPercent = reader.IsDBNull(reader.GetOrdinal("C_PROGRESS_PERCENT")) ? 0 : reader.GetFloat(reader.GetOrdinal("PROGRESS_PERCENT")),
                        Title = reader.GetString(reader.GetOrdinal("TITLE")),
                        NumVerses = reader.GetInt32(reader.GetOrdinal("NUM_VERSES")),
                        Visibility = reader.GetInt32(reader.GetOrdinal("VISIBILITY")),
                        IsPublished = reader.GetInt32(reader.GetOrdinal("IS_PUBLISHED")),
                        NumSaves = reader.GetInt32(reader.GetOrdinal("NUM_SAVES"))
                    };
                    collections.Add(newCollection);
                    collectionIds.Add(collectionId);
                }

                int index = collectionIds.IndexOf(collectionId);
                Collection collection = collections[index];

                UserVerse userVerse = new UserVerse()
                {
                    VerseId = reader.GetInt32(reader.GetOrdinal("VERSE_ID")),
                    UserId = reader.GetInt32(reader.GetOrdinal("USER_ID")),
                    Reference = reader.GetString(reader.GetOrdinal("REFERENCE")),
                    DateAdded = reader.GetDateTime(reader.GetOrdinal("DATE_SAVED")),
                    LastPracticed = reader.IsDBNull(reader.GetOrdinal("LAST_PRACTICED")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("LAST_PRACTICED")),
                    DateMemorized = reader.IsDBNull(reader.GetOrdinal("DATE_MEMORIZED")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("DATE_MEMORIZED")),
                    ProgressPercent = reader.IsDBNull(reader.GetOrdinal("PROGRESS_PERCENT")) ? 0 : reader.GetFloat(reader.GetOrdinal("PROGRESS_PERCENT")),
                    TimesReviewed = reader.IsDBNull(reader.GetOrdinal("TIMES_REVIEWED")) ? 0 : reader.GetInt32(reader.GetOrdinal("TIMES_REVIEWED")),
                    TimesMemorized = reader.IsDBNull(reader.GetOrdinal("TIMES_MEMORIZED")) ? 0 : reader.GetInt32(reader.GetOrdinal("TIMES_MEMORIZED")),
                };
                collection.UserVerses.Add(userVerse);
            }
            conn.Close();
            conn.Dispose();
            return collections;
        }

        public async Task DeleteCollection(int collectionId)
        {
            using var conn = new OracleConnection(connectionString);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;

            cmd.CommandText = "DELETE FROM USER_VERSES WHERE COLLECTION_ID = :collectionId";
            cmd.Parameters.Add(new OracleParameter("collectionId", collectionId));
            await cmd.ExecuteNonQueryAsync();

            cmd.CommandText = "DELETE FROM COLLECTIONS WHERE COLLECTION_ID = :collectionId";
            cmd.Parameters.Clear();
            cmd.Parameters.Add(new OracleParameter("collectionId", collectionId));
            await cmd.ExecuteNonQueryAsync();

            conn.Close();
            conn.Dispose();
        }

        public async Task<List<Verse>> GetVersesByReferences(List<string> references)
        {
            List<Verse> verses = new List<Verse>();

            string inParams = string.Join(",", references.Select((r, i) => $":r{i}"));

            string query = $@"
                            SELECT verse_id,
                                   verse_reference,
                                   text,
                                   users_saved_verse,
                                   users_memorized
                              FROM verses
                             WHERE verse_reference IN ({inParams})";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();
            using OracleCommand cmd = new OracleCommand(query, conn);
            for (int i = 0; i < references.Count; i++)
            {
                cmd.Parameters.Add(new OracleParameter($"r{i}", references[i]));
            }
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Verse newVerse = new Verse()
                {
                    Id = reader.GetInt32(reader.GetOrdinal("VERSE_ID")),
                    Reference = reader.GetString(reader.GetOrdinal("VERSE_REFERENCE")),
                    Text = reader.GetString(reader.GetOrdinal("TEXT")),
                    UsersSaved = reader.GetInt32(reader.GetOrdinal("USERS_SAVED_VERSE")),
                    UsersMemorized = reader.GetInt32(reader.GetOrdinal("USERS_MEMORIZED"))

                };
                verses.Add(newVerse);
            }
            conn.Close();
            conn.Dispose();
            return verses;
        }

        public async Task AddUserVersestonewcollection(List<UserVerse> userVerses)
        {
            int collectionId = await GetLatestCollectionId();

            List<Verse> versesAdded = new List<Verse>();
            foreach (var userVerse in userVerses)
            {
                foreach (var verse in userVerse.Verses)
                {
                    versesAdded.Add(verse);
                }
            }
            await SetVersesSaved(versesAdded);

            string query = @"INSERT INTO USER_VERSES (USER_ID, REFERENCE, COLLECTION_ID, DATE_SAVED) 
                             VALUES (:userId, :reference, :collectionId, SYSDATE)";
            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();
            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.BindByName = true;
            foreach (var userVerse in userVerses)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add(new OracleParameter("collectionId", collectionId));
                cmd.Parameters.Add(new OracleParameter("userId", userVerse.UserId));
                cmd.Parameters.Add(new OracleParameter("reference", userVerse.Reference));
                await cmd.ExecuteNonQueryAsync();
            }
            conn.Close();
            conn.Dispose();
        }

        public async Task<int> GetLatestCollectionId()
        {
            int collectionId = 0;
            string query = @"SELECT MAX(COLLECTION_ID) FROM COLLECTIONS";
            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();
            using OracleCommand cmd = new OracleCommand(query, conn);
            object result = await cmd.ExecuteScalarAsync();
            if (result != DBNull.Value)
                collectionId = Convert.ToInt32(result);
            conn.Close();
            conn.Dispose();
            return collectionId;
        }

        public async Task SetVersesSaved(List<Verse> verses)
        {
            string query = @"UPDATE VERSES SET USERS_SAVED_VERSE = USERS_SAVED_VERSE + 1 WHERE VERSE_ID = :verseId";
            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();
            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.BindByName = true;
            foreach (var verse in verses)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add(new OracleParameter("verseId", verse.Id));
                await cmd.ExecuteNonQueryAsync();
            }
            conn.Close();
            conn.Dispose();
        }
    }
}
