using DBAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Diagnostics;
using System.Xml.Linq;
using VerseAppAPI.Models;

namespace VerseAppAPI.Controllers
{
    public class UserControllerDB
    {
        public User currentUser;
        public List<string> currentUserCategories = new List<string>();
        public List<string> usernames;
        public Dictionary<User, int> currentUserFriends = new Dictionary<User, int>(); // friend, type
        public List<User> otherUserFriends = new List<User>();

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
                OracleConnection conn = new OracleConnection(connectionString);
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT USERNAME FROM USERS WHERE ROWNUM = 1";
                _ = await cmd.ExecuteScalarAsync();
                await WarmupAll();
            }
            catch
            {
                // ignore errors
            }
        }

        public async Task WarmupAll()
        {
            try
            {
                OracleConnection conn = new OracleConnection(connectionString);
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM USERS WHERE ROWNUM = 1";
                _ = await cmd.ExecuteScalarAsync();
            }
            catch
            {

            }
        }

        public async Task<User> GetUserAsync(string username)
        {
            User currentUser = new User();

            string query = @"SELECT * FROM USERS WHERE USERNAME = :username";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("username", username));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                if (!reader.HasRows)
                    throw new Exception("No rows found for table USERS.");
                if (!reader.IsDBNull(reader.GetOrdinal("USERNAME")))
                    currentUser.Username = reader.GetString(reader.GetOrdinal("USERNAME"));
                if (!reader.IsDBNull(reader.GetOrdinal("F_NAME")))
                    currentUser.FName = reader.GetString(reader.GetOrdinal("F_NAME"));
                if (!reader.IsDBNull(reader.GetOrdinal("L_NAME")))
                    currentUser.LName = reader.GetString(reader.GetOrdinal("L_NAME"));
                if (!reader.IsDBNull(reader.GetOrdinal("EMAIL")))
                    currentUser.Email = reader.GetString(reader.GetOrdinal("EMAIL"));
                if (!reader.IsDBNull(reader.GetOrdinal("AUTH_TOKEN")))
                    currentUser.AuthToken = reader.GetString(reader.GetOrdinal("AUTH_TOKEN"));
                if (!reader.IsDBNull(reader.GetOrdinal("COLLECTIONS_SORT")))
                    currentUser.CollectionsSort = reader.GetInt32(reader.GetOrdinal("COLLECTIONS_SORT"));
                if (!reader.IsDBNull(reader.GetOrdinal("COLLECTIONS_ORDER")))
                    currentUser.CollectionsOrder = reader.GetString(reader.GetOrdinal("COLLECTIONS_ORDER"));
                if (!reader.IsDBNull(reader.GetOrdinal("HASHED_PASSWORD")))
                    currentUser.HashedPassword = reader.GetString(reader.GetOrdinal("HASHED_PASSWORD"));
                if (!reader.IsDBNull(reader.GetOrdinal("STATUS")))
                    currentUser.Status = (Enums.Status)reader.GetInt32(reader.GetOrdinal("STATUS"));
                if (!reader.IsDBNull(reader.GetOrdinal("DATE_REGISTERED")))
                    currentUser.DateRegistered = reader.GetDateTime(reader.GetOrdinal("DATE_REGISTERED"));
                if (!reader.IsDBNull(reader.GetOrdinal("LAST_SEEN")))
                    currentUser.LastSeen = reader.GetDateTime(reader.GetOrdinal("LAST_SEEN"));
                if (!reader.IsDBNull(reader.GetOrdinal("DESCRIPTION")))
                    currentUser.Description = reader.GetString(reader.GetOrdinal("DESCRIPTION"));
                if (!reader.IsDBNull(reader.GetOrdinal("CURRENT_READING_PLAN")))
                    currentUser.CurrentReadingPlan = reader.GetInt32(reader.GetOrdinal("CURRENT_READING_PLAN"));
                if (!reader.IsDBNull(reader.GetOrdinal("ACCOUNT_DELETED")))
                    currentUser.AccountDeleted = reader.GetInt32(reader.GetOrdinal("ACCOUNT_DELETED")) == 1 ? true : false;
                if (!reader.IsDBNull(reader.GetOrdinal("REASON_DELETED")))
                    currentUser.ReasonDeleted = reader.GetString(reader.GetOrdinal("REASON_DELETED"));
                if (!reader.IsDBNull(reader.GetOrdinal("FLAGGED")))
                    currentUser.Flagged = reader.GetInt32(reader.GetOrdinal("FLAGGED")) == 1 ? true : false;
                if (!reader.IsDBNull(reader.GetOrdinal("PROFILE_VISIBILITY")))
                    currentUser.ProfileVisibility = reader.GetInt32(reader.GetOrdinal("PROFILE_VISIBILITY"));
            }

            conn.Close();
            conn.Dispose();

            return currentUser;
        }

        public async Task<User> GetUserByTokenAsync(string token)
        {
            User currentUser = new User();

            string query = @"SELECT * FROM USERS WHERE AUTH_TOKEN = :token";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("token", token));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                if (!reader.HasRows)
                    throw new Exception("No rows found for table USERS.");
                if (!reader.IsDBNull(reader.GetOrdinal("USERNAME")))
                    currentUser.Username = reader.GetString(reader.GetOrdinal("USERNAME"));
                if (!reader.IsDBNull(reader.GetOrdinal("F_NAME")))
                    currentUser.FName = reader.GetString(reader.GetOrdinal("F_NAME"));
                if (!reader.IsDBNull(reader.GetOrdinal("L_NAME")))
                    currentUser.LName = reader.GetString(reader.GetOrdinal("L_NAME"));
                if (!reader.IsDBNull(reader.GetOrdinal("EMAIL")))
                    currentUser.Email = reader.GetString(reader.GetOrdinal("EMAIL"));
                if (!reader.IsDBNull(reader.GetOrdinal("AUTH_TOKEN")))
                    currentUser.AuthToken = reader.GetString(reader.GetOrdinal("AUTH_TOKEN"));
                if (!reader.IsDBNull(reader.GetOrdinal("COLLECTIONS_SORT")))
                    currentUser.CollectionsSort = reader.GetInt32(reader.GetOrdinal("COLLECTIONS_SORT"));
                if (!reader.IsDBNull(reader.GetOrdinal("COLLECTIONS_ORDER")))
                    currentUser.CollectionsOrder = reader.GetString(reader.GetOrdinal("COLLECTIONS_ORDER"));
                if (!reader.IsDBNull(reader.GetOrdinal("HASHED_PASSWORD")))
                    currentUser.HashedPassword = reader.GetString(reader.GetOrdinal("HASHED_PASSWORD"));
                if (!reader.IsDBNull(reader.GetOrdinal("STATUS")))
                    currentUser.Status = (Enums.Status)reader.GetInt32(reader.GetOrdinal("STATUS"));
                if (!reader.IsDBNull(reader.GetOrdinal("DATE_REGISTERED")))
                    currentUser.DateRegistered = reader.GetDateTime(reader.GetOrdinal("DATE_REGISTERED"));
                if (!reader.IsDBNull(reader.GetOrdinal("LAST_SEEN")))
                    currentUser.LastSeen = reader.GetDateTime(reader.GetOrdinal("LAST_SEEN"));
                if (!reader.IsDBNull(reader.GetOrdinal("DESCRIPTION")))
                    currentUser.Description = reader.GetString(reader.GetOrdinal("DESCRIPTION"));
                if (!reader.IsDBNull(reader.GetOrdinal("CURRENT_READING_PLAN")))
                    currentUser.CurrentReadingPlan = reader.GetInt32(reader.GetOrdinal("CURRENT_READING_PLAN"));
                if (!reader.IsDBNull(reader.GetOrdinal("ACCOUNT_DELETED")))
                    currentUser.AccountDeleted = reader.GetInt32(reader.GetOrdinal("ACCOUNT_DELETED")) == 1 ? true : false;
                if (!reader.IsDBNull(reader.GetOrdinal("REASON_DELETED")))
                    currentUser.ReasonDeleted = reader.GetString(reader.GetOrdinal("REASON_DELETED"));
                if (!reader.IsDBNull(reader.GetOrdinal("FLAGGED")))
                    currentUser.Flagged = reader.GetInt32(reader.GetOrdinal("FLAGGED")) == 1 ? true : false;
                if (!reader.IsDBNull(reader.GetOrdinal("PROFILE_VISIBILITY")))
                    currentUser.ProfileVisibility = reader.GetInt32(reader.GetOrdinal("PROFILE_VISIBILITY"));
            }

            conn.Close();
            conn.Dispose();

            return currentUser;
        }

        public async Task AddUserDBAsync(User user)
        {
            string query = @"INSERT INTO USERS (USERNAME, F_NAME, L_NAME, EMAIL, HASHED_PASSWORD, AUTH_TOKEN, STATUS, DATE_REGISTERED, LAST_SEEN, COLLECTIONS_ORDER, COLLECTIONS_SORT)
                             VALUES (:username, :fName, :lName, :email, :userPassword, :token, :status, SYSDATE, SYSDATE, :defaultOrder, defaultSort)";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.BindByName = true;

            cmd.Parameters.Add(new OracleParameter("username", user.Username));
            cmd.Parameters.Add(new OracleParameter("firstName", user.FName));
            cmd.Parameters.Add(new OracleParameter("lastName", user.LName));
            cmd.Parameters.Add(new OracleParameter("email", user.Email));
            cmd.Parameters.Add(new OracleParameter("userPassword", user.HashedPassword));
            cmd.Parameters.Add(new OracleParameter("token", user.AuthToken));
            cmd.Parameters.Add(new OracleParameter("status", user.Status));
            cmd.Parameters.Add(new OracleParameter("defaultOrder", Enums.DefaultCollectionsOrder));
            cmd.Parameters.Add(new OracleParameter("defaultSort", Enums.DefaultCollectionsSort));

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<string>> GetAllUsernamesAsync()
        {
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
            conn.Close();
            conn.Dispose();
            return usernames;
        }

        public async Task<int> CheckUsernameExists(string username)
        {
            int exists = 0;
            string query = @"SELECT 1 FROM USERS WHERE USERNAME = :username AND
                                IS_DELETED = 0";
            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();
            OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("username", username));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) 
            {
                exists = 1; // If we read a row, the username exists
            }
            conn.Close();
            conn.Dispose();
            return exists;
        }

        public async Task GetUserFriendsDBAsync(int userId)
        {
            // Update this function to get friends of user who's id is passed as parameter ^^^

            currentUserFriends = new Dictionary<User, int>(); // friend, type

            string query = @"SELECT * FROM USERS";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            OracleCommand cmd = new OracleCommand(query, conn);
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                User user = new User
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

        public async Task<List<RecoveryInfo>> GetRecoveryInfoDBAsync()
        {
            List<RecoveryInfo> recoveryInfo = new List<RecoveryInfo>();

            string query = @"SELECT USER_ID, FIRST_NAME, LAST_NAME, USERNAME, HASHED_PASSWORD, EMAIL FROM USERS WHERE IS_DELETED = 0";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            OracleCommand cmd = new OracleCommand(query, conn);
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var rec = new RecoveryInfo
                {
                    Id = reader.GetInt32(reader.GetOrdinal("USER_ID")),
                    FirstName = reader.GetString(reader.GetOrdinal("FIRST_NAME")),
                    LastName = reader.GetString(reader.GetOrdinal("LAST_NAME")),
                    Username = reader.GetString(reader.GetOrdinal("USERNAME")),
                    PasswordHash = reader.GetString(reader.GetOrdinal("HASHED_PASSWORD")),
                    Email = reader.IsDBNull(reader.GetOrdinal("EMAIL"))
                                   ? null
                                   : reader.GetString(reader.GetOrdinal("EMAIL"))
                };
                recoveryInfo.Add(rec);
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
            cmd.BindByName = true;

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

        public string everyoneUsername = "jUW=f<9m!'2kgegw3g";

        public async Task<List<Notification>> GetUserNotifications(string username)
        {
            var notifications = new List<Notification>();
            const string query = @"
                                    SELECT *
                                      FROM USER_NOTIFICATIONS
                                     WHERE USERNAME         = :username
                                        OR USERNAME         = :everyoneUsername
                                     ORDER BY CREATED_DATE DESC";

            using var conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("username", username));
            cmd.Parameters.Add(new OracleParameter("everyoneUsername", everyoneUsername));

            using var reader = await cmd.ExecuteReaderAsync();

            int idxId = reader.GetOrdinal("NOTIFICATION_ID");
            int idxTitle = reader.GetOrdinal("NOTIFICATION_TITLE");
            int idxText = reader.GetOrdinal("NOTIFICATION_TEXT");
            int idxUser = reader.GetOrdinal("USERNAME");
            int idxSeen = reader.GetOrdinal("SEEN");
            int idxSentBy = reader.GetOrdinal("SENT_BY");
            int idxCreated = reader.GetOrdinal("CREATED_DATE");

            while (await reader.ReadAsync())
            {
                var not = new Notification
                {
                    Id = reader.GetInt32(idxId),
                    Title = reader.IsDBNull(idxTitle) ? null : reader.GetString(idxTitle),
                    Message = reader.IsDBNull(idxText) ? null : reader.GetString(idxText),
                    Username = reader.IsDBNull(idxUser) ? null : reader.GetString(idxUser),
                    Seen = reader.GetInt32(idxSeen),
                    SentBy = reader.IsDBNull(idxSentBy) ? null : reader.GetString(idxSentBy),
                    DateCreated = reader.GetDateTime(idxCreated)
                };
                notifications.Add(not);
            }

            return notifications;
        }

        public async Task MarkNotificationAsRead(int notificationId)
        {
            string query = @"UPDATE USER_NOTIFICATIONS 
                             SET SEEN = 1 
                             WHERE NOTIFICATION_ID = :notificationId";
            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();
            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("notificationId", notificationId));
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<User>> GetAllUsers()
        {
            List<User> users = new List<User>();

            string query = @"SELECT * FROM USERS";
            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();
            using OracleCommand cmd = new OracleCommand(query, conn);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var user = new User();

                user.Id = reader.GetInt32(reader.GetOrdinal("USER_ID"));
                user.Username = reader.GetString(reader.GetOrdinal("USERNAME"));
                user.FirstName = reader.GetString(reader.GetOrdinal("FIRST_NAME"));
                user.LastName = reader.GetString(reader.GetOrdinal("LAST_NAME"));
                user.Email = reader.GetString(reader.GetOrdinal("EMAIL"));
                user.DateRegistered = reader.GetDateTime(reader.GetOrdinal("DATE_REGISTERED"));
                user.LastSeen = reader.GetDateTime(reader.GetOrdinal("LAST_SEEN"));
                user.Description = reader.IsDBNull(reader.GetOrdinal("DESCRIPTION"))
                               ? null
                               : reader.GetString(reader.GetOrdinal("DESCRIPTION"));
                user.IsDeleted = reader.GetInt32(reader.GetOrdinal("IS_DELETED"));
                user.ReasonDeleted = reader.IsDBNull(reader.GetOrdinal("REASON_DELETED"))
                               ? null
                               : reader.GetString(reader.GetOrdinal("REASON_DELETED"));


                users.Add(user);
            }

            conn.Close();
            conn.Dispose();

            return users;
        }

        public async Task DeleteNotification(int notificationId)
        {
            string query = @"DELETE FROM USER_NOTIFICATIONS 
                             WHERE NOTIFICATION_ID = :notificationId";
            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();
            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("notificationId", notificationId));
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task SendNotification(Notification notification)
        {
            string query = @"INSERT INTO USER_NOTIFICATIONS (NOTIFICATION_TITLE, NOTIFICATION_TEXT, USERNAME, SENT_BY, SEEN, CREATED_DATE) 
                             VALUES (:title, :message, :username, :sentBy, 0, SYSDATE)";
            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();
            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("title", notification.Title));
            cmd.Parameters.Add(new OracleParameter("message", notification.Message));
            cmd.Parameters.Add(new OracleParameter("username", notification.Username));
            cmd.Parameters.Add(new OracleParameter("sentBy", notification.SentBy));
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<EmailNotification>> GetAllUserEmailsAsync()
        {
            List<EmailNotification> emails = new List<EmailNotification>();
            string query = @"SELECT EMAIL, USERNAME, FIRST_NAME, LAST_NAME FROM USERS WHERE IS_DELETED = 0 AND EMAIL IS NOT NULL";
            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();
            OracleCommand cmd = new OracleCommand(query, conn);
            OracleDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string fName = reader.GetString(reader.GetOrdinal("FIRST_NAME"));
                string lName = reader.GetString(reader.GetOrdinal("LAST_NAME"));
                var email = new EmailNotification
                {
                    Email = reader.GetString(reader.GetOrdinal("EMAIL")),
                    Username = reader.GetString(reader.GetOrdinal("USERNAME")),
                    FullName = $"{fName.Substring(0, 1).ToUpper() + fName.Substring(1)} {lName.Substring(0, 1).ToUpper() + lName.Substring(1)}"
                };
                emails.Add(email);
            }
            conn.Close();
            conn.Dispose();
            return emails;
        }

        public async Task SetUserActive(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID provided.");
            string query = @"UPDATE USERS 
                             SET LAST_SEEN = SYSDATE 
                             WHERE USER_ID = :userId";
            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();
            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("userId", userId));
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
