using VerseAppAPI.Models;
using Oracle.ManagedDataAccess.Client;
using DBAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace VerseAppAPI.Controllers
{
    public class UserControllerDB
    {
        public UserModel currentUser;
        public List<string> currentUserCategories = new List<string>();
        public List<string> usernames;
        public Dictionary<UserModel, int> currentUserFriends = new Dictionary<UserModel, int>(); // friend, type
        public List<UserModel> otherUserFriends = new List<UserModel>();

        #region connectionString
        private string connectionString;
        private IConfiguration _config;
        public UserControllerDB(IConfiguration config)
        {
            _config = config;
            connectionString = _config.GetConnectionString("Default");
        }
        #endregion

        public async Task Warmup()
        {
            try
            {
                // 1) Warm up the pool: open one connection
                OracleConnection conn = new OracleConnection(connectionString);
                await conn.OpenAsync();
                await conn.OpenAsync();  // pulls a session out of the pool

                // 2) Do a minimal SELECT to bring index blocks into cache
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT /*WARMUP*/ USERNAME FROM USERS WHERE ROWNUM = 1";
                // If you truly have only one row, ROWNUM=1 is fine; if you might have 0, use "SELECT 1 FROM DUAL"
                _ = await cmd.ExecuteScalarAsync();
                await WarmupAll();
            }
            catch
            {
                // swallow any errors—this is just a best‐effort warm‐up
            }
        }

        public async Task WarmupAll()
        {
            try
            {
                // 1) Warm up the pool: open one connection
                OracleConnection conn = new OracleConnection(connectionString);
                await conn.OpenAsync();
                await conn.OpenAsync();  // pulls a session out of the pool

                // 2) Do a minimal SELECT to bring index blocks into cache
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT /*WARMUP*/ * FROM USERS WHERE ROWNUM = 1";
                // If you truly have only one row, ROWNUM=1 is fine; if you might have 0, use "SELECT 1 FROM DUAL"
                _ = await cmd.ExecuteScalarAsync();
            }
            catch
            {
                // swallow any errors—this is just a best‐effort warm‐up
            }
        }

        public async Task<UserModel> GetUserDBAsync(string username)
        {
            var connSw = Stopwatch.StartNew();
            UserModel currentUser = new UserModel();

            string query = @"SELECT * FROM USERS WHERE USERNAME = :username";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();
            connSw.Stop();
            Console.WriteLine($"OpenAsync took {connSw.ElapsedMilliseconds} ms");

            var cmdSw = Stopwatch.StartNew();
            OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("username", username));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                currentUser.Id = reader.GetInt32(reader.GetOrdinal("USER_ID"));
                currentUser.Username = reader.GetString(reader.GetOrdinal("USERNAME"));
                currentUser.FirstName = reader.GetString(reader.GetOrdinal("FIRST_NAME"));
                currentUser.LastName = reader.GetString(reader.GetOrdinal("LAST_NAME"));
                if (!reader.IsDBNull(reader.GetOrdinal("EMAIL")))
                    currentUser.Email = reader.GetString(reader.GetOrdinal("EMAIL"));
                if (!reader.IsDBNull(reader.GetOrdinal("SECURITY_QUESTION")))
                    currentUser.SecurityQuestion = reader.GetString(reader.GetOrdinal("SECURITY_QUESTION"));
                if (!reader.IsDBNull(reader.GetOrdinal("SECURITY_ANSWER")))
                    currentUser.SecurityAnswer = reader.GetString(reader.GetOrdinal("SECURITY_ANSWER"));
                currentUser.PasswordHash = reader.GetString(reader.GetOrdinal("HASHED_PASSWORD"));
                currentUser.Status = reader.GetString(reader.GetOrdinal("STATUS"));
                currentUser.DateRegistered = reader.GetDateTime(reader.GetOrdinal("DATE_REGISTERED"));
                currentUser.LastSeen = reader.GetDateTime(reader.GetOrdinal("LAST_SEEN"));
                if (!reader.IsDBNull(reader.GetOrdinal("DESCRIPTION")))
                    currentUser.Description = reader.GetString(reader.GetOrdinal("DESCRIPTION"));
                if (!reader.IsDBNull(reader.GetOrdinal("CURRENT_READING_PLAN")))
                    currentUser.CurrentReadingPlan = reader.GetInt32(reader.GetOrdinal("CURRENT_READING_PLAN"));
                currentUser.IsDeleted = reader.GetInt32(reader.GetOrdinal("IS_DELETED"));
                if (!reader.IsDBNull(reader.GetOrdinal("REASON_DELETED")))
                    currentUser.ReasonDeleted = reader.GetString(reader.GetOrdinal("REASON_DELETED"));
                currentUser.Flagged = reader.GetInt32(reader.GetOrdinal("FLAGGED"));
                currentUser.FollowVerseOfTheDay = reader.GetInt32(reader.GetOrdinal("FOLLOW_VERSE_OF_THE_DAY"));
                currentUser.Visibility = reader.GetInt32(reader.GetOrdinal("VISIBILITY"));
                currentUser.AuthToken = reader.GetString(reader.GetOrdinal("AUTH_TOKEN"));
            }

            cmdSw.Stop();
            Console.WriteLine($"Command.ExecuteReaderAsync + read took {cmdSw.ElapsedMilliseconds} ms");

            conn.Close();
            conn.Dispose();

            return currentUser;
        }

        public async Task<UserModel> GetUserByTokenDBAsync(string token)
        {
            UserModel currentUser = new UserModel();

            string query = @"SELECT * FROM USERS WHERE AUTH_TOKEN = :token";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("token", token));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                currentUser.Id = reader.GetInt32(reader.GetOrdinal("USER_ID"));
                currentUser.Username = reader.GetString(reader.GetOrdinal("USERNAME"));
                currentUser.FirstName = reader.GetString(reader.GetOrdinal("FIRST_NAME"));
                currentUser.LastName = reader.GetString(reader.GetOrdinal("LAST_NAME"));
                if (!reader.IsDBNull(reader.GetOrdinal("EMAIL")))
                    currentUser.Email = reader.GetString(reader.GetOrdinal("EMAIL"));
                if (!reader.IsDBNull(reader.GetOrdinal("SECURITY_QUESTION")))
                    currentUser.SecurityQuestion = reader.GetString(reader.GetOrdinal("SECURITY_QUESTION"));
                if (!reader.IsDBNull(reader.GetOrdinal("SECURITY_ANSWER")))
                    currentUser.SecurityAnswer = reader.GetString(reader.GetOrdinal("SECURITY_ANSWER"));
                currentUser.PasswordHash = reader.GetString(reader.GetOrdinal("HASHED_PASSWORD"));
                currentUser.Status = reader.GetString(reader.GetOrdinal("STATUS"));
                currentUser.DateRegistered = reader.GetDateTime(reader.GetOrdinal("DATE_REGISTERED"));
                currentUser.LastSeen = reader.GetDateTime(reader.GetOrdinal("LAST_SEEN"));
                if (!reader.IsDBNull(reader.GetOrdinal("DESCRIPTION")))
                    currentUser.Description = reader.GetString(reader.GetOrdinal("DESCRIPTION"));
                if (!reader.IsDBNull(reader.GetOrdinal("CURRENT_READING_PLAN")))
                    currentUser.CurrentReadingPlan = reader.GetInt32(reader.GetOrdinal("CURRENT_READING_PLAN"));
                currentUser.IsDeleted = reader.GetInt32(reader.GetOrdinal("IS_DELETED"));
                if (!reader.IsDBNull(reader.GetOrdinal("REASON_DELETED")))
                    currentUser.ReasonDeleted = reader.GetString(reader.GetOrdinal("REASON_DELETED"));
                currentUser.Flagged = reader.GetInt32(reader.GetOrdinal("FLAGGED"));
                currentUser.FollowVerseOfTheDay = reader.GetInt32(reader.GetOrdinal("FOLLOW_VERSE_OF_THE_DAY"));
                currentUser.Visibility = reader.GetInt32(reader.GetOrdinal("VISIBILITY"));
                currentUser.AuthToken = reader.GetString(reader.GetOrdinal("AUTH_TOKEN"));
            }

            conn.Close();
            conn.Dispose();

            return currentUser;
        }

        public async Task AddUserDBAsync(UserModel user)
        {
            string query = @"INSERT INTO USERS (USERNAME, FIRST_NAME, LAST_NAME, EMAIL, SECURITY_QUESTION, SECURITY_ANSWER, HASHED_PASSWORD, AUTH_TOKEN, STATUS, DATE_REGISTERED, LAST_SEEN)
                             VALUES (:username, :firstName, :lastName, :email, :securityQuestion, :securityAnswer, :userPassword, :token, :status, SYSDATE, SYSDATE)";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);

            cmd.Parameters.Add(new OracleParameter("username", user.Username));
            cmd.Parameters.Add(new OracleParameter("firstName", user.FirstName));
            cmd.Parameters.Add(new OracleParameter("lastName", user.LastName));
            cmd.Parameters.Add(new OracleParameter("email", user.Email != null ? user.Email : DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("securityQuestion", user.SecurityQuestion != null ? user.SecurityQuestion : DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("securityAnswer", user.SecurityAnswer != null ? user.SecurityAnswer : DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("userPassword", user.PasswordHash));
            cmd.Parameters.Add(new OracleParameter("token", user.AuthToken));
            cmd.Parameters.Add(new OracleParameter("status", user.Status));

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task GetUserFriendsDBAsync(int userId)
        {
            // Update this function to get friends of user who's id is passed as parameter ^^^

            currentUserFriends = new Dictionary<UserModel, int>(); // friend, type

            string query = @"SELECT * FROM USERS";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            OracleCommand cmd = new OracleCommand(query, conn);
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                UserModel user = new UserModel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("USER_ID")),
                    Username = reader.GetString(reader.GetOrdinal("USERNAME")),
                    PasswordHash = reader.GetString(reader.GetOrdinal("USERPASSWORD")),
                    DateRegistered = reader.GetDateTime(reader.GetOrdinal("DATEREGISTERED")),
                    LastSeen = reader.GetDateTime(reader.GetOrdinal("LASTSEEN"))
                };

                currentUserFriends.Add(user, 0);
            }
            conn.Close();
            conn.Dispose();
        }

        public async Task GetAllUsernamesDBAsync()
        {
            usernames = new List<string>();
            string query = @"SELECT USERNAME FROM USERS";
            int retries = 0;

            while (true)
            {
                try
                {
                    using var conn = new OracleConnection(connectionString);
                    await conn.OpenAsync();

                    using var cmd = new OracleCommand(query, conn)
                    {
                        CommandTimeout = 60  // raise to 60s
                    };

                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        usernames.Add(reader.GetString(reader.GetOrdinal("USERNAME")));
                    }
                    // If we got this far, it succeeded—break out of retry loop:
                    break;
                }
                catch (Oracle.ManagedDataAccess.Client.OracleException oraEx)
                {
                    // If it’s a timeout (ORA-01013) or some known transient error, retry:
                    if (oraEx.Number == 1013 /* ORA-01013: user requested cancel of current operation */
                     || oraEx.Number == 12170 /* ORA-12170: TNS: Connect timeout */
                     || oraEx.Number == 25408 /* ORA-25408: could not send message to service (listener) - timeout */
                     || retries < 2)  // try up to 2 additional times
                    {
                        retries++;
                        await Task.Delay(1000 * retries); // exponential backoff (1s, then 2s)
                        usernames.Clear();                // clear any partial results
                        continue;
                    }

                    // Otherwise, bubble up the exception so your controller will catch it:
                    throw new Exception($"Oracle error fetching usernames (after {retries + 1} tries): {oraEx.Message}", oraEx);
                }
                catch (Exception ex)
                {
                    // For any other exception, just rethrow
                    throw new Exception($"Unexpected error in GetAllUsernamesDBAsync: {ex.Message}", ex);
                }
            }
        }


        public async Task<List<RecoveryInfo>> GetRecoveryInfoDBAsync()
        {
            List<RecoveryInfo> recoveryInfo = new List<RecoveryInfo>();
            RecoveryInfo _recoveryInfo = new RecoveryInfo();

            string query = @"SELECT USER_ID, FIRST_NAME, LAST_NAME, USERNAME, HASHED_PASSWORD, EMAIL FROM USERS";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            OracleCommand cmd = new OracleCommand(query, conn);
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                _recoveryInfo.Id = reader.GetInt32(reader.GetOrdinal("USER_ID"));
                _recoveryInfo.FirstName = reader.GetString(reader.GetOrdinal("FIRST_NAME"));
                _recoveryInfo.LastName = reader.GetString(reader.GetOrdinal("LAST_NAME"));
                _recoveryInfo.Username = reader.GetString(reader.GetOrdinal("USERNAME"));
                _recoveryInfo.PasswordHash = reader.GetString(reader.GetOrdinal("HASHED_PASSWORD"));
                if (!reader.IsDBNull(reader.GetOrdinal("EMAIL")))
                    _recoveryInfo.Email = reader.GetString(reader.GetOrdinal("EMAIL"));

                recoveryInfo.Add(_recoveryInfo);
            }

            conn.Close();
            conn.Dispose();

            return recoveryInfo;
        }

        public async Task<string> GetSecurityQuestionDBAsync(string username)
        {
            string returnString = "";

            string query = @"SELECT SECURITY_QUESTION FROM USERS WHERE USERNAME = :username";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("username", username));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                returnString = reader.GetString(reader.GetOrdinal("SECURITY_QUESTION"));
            }

            return returnString;
        }

        public async Task<string> GetPasswordHashDBAsync(string username)
        {
            string returnString = "";

            string query = @"SELECT HASHED_PASSWORD FROM USERS WHERE USERNAME = :username";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("username", username));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                returnString = reader.GetString(reader.GetOrdinal("HASHED_PASSWORD"));
            }

            conn.Close();
            conn.Dispose();
            return returnString;
        }

        public async Task ResetUserPasswordDBAsync(string username, string token)
        {
            string query = @"INSERT INTO PASSWORD_RESETS (TOKEN, USERNAME, SENT)
                             VALUES (:token, :username, SYSDATE)";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);

            cmd.Parameters.Add(new OracleParameter("token", token));
            cmd.Parameters.Add(new OracleParameter("username", username));

            await cmd.ExecuteNonQueryAsync();
            await AddUserForgotPasswordDBAsync(username);
        }
        public async Task AddUserForgotPasswordDBAsync(string username)
        {
            string query = @"UPDATE USERS 
                             SET FORGOT_PASSWORD = FORGOT_PASSWORD + 1 
                             WHERE USERNAME = :username";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("username", username));
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task GetUserCategoriesDBAsync(int userId)
        {
            //userVerses = new Dictionary<Verse, string>();
            currentUserCategories = new List<string>();

            string query = @"SELECT DISTINCT CATEGORY_NAME FROM USER_VERSES 
                             WHERE USER_ID = :userId";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            try
            {
                OracleCommand cmd = new OracleCommand(query, conn);
                cmd.Parameters.Add(new OracleParameter("userId", userId));
                OracleDataReader reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    string category = reader.GetString(reader.GetOrdinal("CATEGORY"));

                    currentUserCategories.Add(category);
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                conn.Dispose();
                return;
            }
            conn.Close();
            conn.Dispose();
        }

        public async Task SetUserActiveDBAsync(int userId)
        {
            string query = @"UPDATE USERS 
                             SET LAST_SEEN = SYSDATE 
                             WHERE USER_ID = :userId";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("userId", userId));
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateUserPasswordDBAsync(int userId, string passwordHash)
        {
            if (passwordHash == null)
                throw new ArgumentException("Fatal error updating password. Value was null.");

            string query = @"UPDATE USERS 
                             SET HASHED_PASSWORD = :newHash 
                             WHERE USER_ID = :userId";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("newHash", passwordHash));
            cmd.Parameters.Add(new OracleParameter("userId", userId));
            await cmd.ExecuteNonQueryAsync();
            await UpdateUserChangedPasswordDBAsync(userId);
        }

        public async Task<int> GetIdFromUsername(string username)
        {
            int userId = 0;
            int rowCount = 0;

            string query = @"SELECT USER_ID FROM USERS 
                             WHERE USERNAME = :username";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("username", username));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();


            while (await reader.ReadAsync())
            {
                rowCount++;
                if (rowCount == 1)
                {
                    userId = reader.GetInt32(reader.GetOrdinal("USER_ID"));
                }
                else
                {
                    throw new InvalidOperationException($"Expected at most one row for username='{username}', but found multiple.");
                }
                userId = reader.GetInt32(reader.GetOrdinal("USER_ID"));
            }

            conn.Close();
            conn.Dispose();
            return userId;
        }

        public async Task UpdateUserChangedPasswordDBAsync(int userId)
        {
            if (userId == null)
                throw new ArgumentException("Fatal error setting user as active. User was null.");

            string query = @"UPDATE USERS 
                             SET CHANGED_PASSWORD = CHANGED_PASSWORD + 1 
                             WHERE USER_ID = :userId";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("userId", userId));
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> VerifyTokenDBAsync(string username, string token)
        {
            currentUserCategories = new List<string>();

            string query = @"SELECT USERNAME, SENT FROM PASSWORD_RESETS 
                             WHERE TOKEN = :token";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("token", token));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return false;

            DateTime sent = reader.GetDateTime(reader.GetOrdinal("SENT"));
            if (DateTime.Now > sent.AddHours(1))
                return false;

            string usernameToken = reader.GetString(reader.GetOrdinal("USERNAME"));
            if (usernameToken != username)
                return false;

            conn.Close();
            conn.Dispose();

            return true;
        }

        public void LogoutUser()
        {
            currentUser = null;
            currentUserCategories = new List<string>();
            currentUserFriends = new Dictionary<UserModel, int>();
        }
    }
}
