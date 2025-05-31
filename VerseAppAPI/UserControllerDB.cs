using VerseAppAPI.Models;
using Oracle.ManagedDataAccess.Client;
using DBAccessLibrary.Models;
using Microsoft.Extensions.Configuration;

namespace VerseAppAPI
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

        public async Task<UserModel> GetUserDBAsync(string username)
        {
            UserModel currentUser = new UserModel();

            string query = @"SELECT * FROM USERS WHERE USERNAME = :username";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

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
                currentUser.PasswordHash = reader.GetString(reader.GetOrdinal("HASHED_PASSWORD"));
                currentUser.Status = reader.GetString(reader.GetOrdinal("STATUS"));
                currentUser.DateRegistered = reader.GetDateTime(reader.GetOrdinal("DATE_REGISTERED"));
                currentUser.LastSeen = reader.GetDateTime(reader.GetOrdinal("LAST_SEEN"));
                if (!reader.IsDBNull(reader.GetOrdinal("DESCRIPTION")))
                    currentUser.Description = reader.GetString(reader.GetOrdinal("DESCRIPTION"));
                if (!reader.IsDBNull(reader.GetOrdinal("LAST_READ_PASSAGE")))
                    currentUser.LastReadPassage = reader.GetString(reader.GetOrdinal("LAST_READ_PASSAGE"));
                if (!reader.IsDBNull(reader.GetOrdinal("CURRENT_READING_PLAN")))
                    currentUser.CurrentReadingPlan = reader.GetInt32(reader.GetOrdinal("CURRENT_READING_PLAN"));
                if (!reader.IsDBNull(reader.GetOrdinal("LAST_PRACTICED_VERSE_ID")))
                    currentUser.LastPracticedVerse = reader.GetInt32(reader.GetOrdinal("LAST_PRACTICED_VERSE_ID"));
                currentUser.IsDeleted = reader.GetInt32(reader.GetOrdinal("IS_DELETED"));
                if (!reader.IsDBNull(reader.GetOrdinal("REASON_DELETED")))
                    currentUser.ReasonDeleted = reader.GetString(reader.GetOrdinal("REASON_DELETED"));
                currentUser.AppTheme = reader.GetInt32(reader.GetOrdinal("APP_THEME"));
                currentUser.ShowVersesSaved = reader.GetInt32(reader.GetOrdinal("SHOW_VERSES_SAVED"));
                currentUser.ShowPopularHighlights = reader.GetInt32(reader.GetOrdinal("SHOW_POPULAR_HIGHLIGHTS"));
                currentUser.Flagged = reader.GetInt32(reader.GetOrdinal("FLAGGED"));
                currentUser.AllowPushNotifications = reader.GetInt32(reader.GetOrdinal("ALLOW_PUSH_NOTIFICATIONS"));
                currentUser.FollowVerseOfTheDay = reader.GetInt32(reader.GetOrdinal("FOLLOW_VERSE_OF_THE_DAY"));
                currentUser.Visibility = reader.GetInt32(reader.GetOrdinal("VISIBILITY"));
            }

            conn.Close();
            conn.Dispose();

            return currentUser;
        }

        public async Task AddUserDBAsync(UserModel user)
        {
            string query = @"INSERT INTO USERS (USERNAME, FIRST_NAME, LAST_NAME, EMAIL, HASHED_PASSWORD, STATUS, DATE_REGISTERED, LAST_SEEN)
                             VALUES (:username, :firstName, :lastName, :email, :userPassword, :status, SYSDATE, SYSDATE)";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);

            cmd.Parameters.Add(new OracleParameter("username", user.Username));
            cmd.Parameters.Add(new OracleParameter("firstName", user.FirstName));
            cmd.Parameters.Add(new OracleParameter("lastName", user.LastName));
            if (user.Email != null)
                cmd.Parameters.Add(new OracleParameter("email", user.Email));
            cmd.Parameters.Add(new OracleParameter("userPassword", user.PasswordHash));
            cmd.Parameters.Add(new OracleParameter("status", user.Status));

            await cmd.ExecuteNonQueryAsync();

            currentUser = await GetUserDBAsync(user.Username);
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
            //List<string> usernamesUnsorted = new List<string>();
            usernames = new List<string>();

            string query = @"SELECT USERNAME FROM USERS";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            OracleCommand cmd = new OracleCommand(query, conn);
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string username = reader.GetString(reader.GetOrdinal("USERNAME"));

                usernames.Add(username);
            }

            //usernames = Algorithms.QuickSortUsernames(usernamesUnsorted);

            conn.Close();
            conn.Dispose();
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

        public async Task ResetUserPasswordDBAsync(int userId, string token)
        {
            string query = @"INSERT INTO PASSWORD_RESETS (TOKEN, USER_ID, SENT)
                             VALUES (:token, :userId, SYSDATE)";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);

            cmd.Parameters.Add(new OracleParameter("token", token));
            cmd.Parameters.Add(new OracleParameter("userId", userId));

            await cmd.ExecuteNonQueryAsync();
            await AddUserForgotPasswordDBAsync(userId);
        }
        public async Task AddUserForgotPasswordDBAsync(int userId)
        {
            string query = @"UPDATE USERS 
                             SET FORGOT_PASSWORD = FORGOT_PASSWORD + 1 
                             WHERE USER_ID = :userId";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("userId", userId));
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

        public async Task<bool> VerifyTokenDBAsync(int userId, string token)
        {
            currentUserCategories = new List<string>();

            string query = @"SELECT USER_ID, SENT FROM PASSWORD_RESETS 
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

            int userIdToken = reader.GetInt32(reader.GetOrdinal("USER_ID"));
            if (userIdToken != userId)
                return false;

            conn.Close();
            conn.Dispose();

            return true;
        }

        public void LogoutUser()
        {
            this.currentUser = null;
            this.currentUserCategories = new List<string>();
            this.currentUserFriends = new Dictionary<UserModel, int>();
        }
    }
}
