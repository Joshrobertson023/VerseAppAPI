using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using VerseAppAPI.Models;
using Oracle.ManagedDataAccess.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace VerseAppAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private UserControllerDB userDB;

        public UserController(UserControllerDB UserDB)
        {
            userDB = UserDB;
        }

        [HttpGet("usernames")]
        public async Task<IActionResult> GetUsernames()
        {
            try
            {
                await userDB.GetAllUsernamesDBAsync();
                return Ok(userDB.usernames);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get usernames ", error = ex.Message });
            }
        }

        [HttpGet("currentUser")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            string username = User.Identity.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { message = "Unable to authorize user." });

            UserModel currentUser = await GetUserDBAsync(User.Identity.Name);
            if (currentUser == null)
                return NotFound(new { message = "User not found." });

            return Ok(currentUser);
        }











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
    }
}
