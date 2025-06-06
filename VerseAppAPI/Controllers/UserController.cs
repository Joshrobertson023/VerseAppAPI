using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using VerseAppAPI.Models;
using Oracle.ManagedDataAccess.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Diagnostics;

namespace VerseAppAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private UserControllerDB userDB; // To call Oracle commands

        private IConfiguration _config;
        private string connectionString;
        public UserController(IConfiguration config, UserControllerDB UserDB)
        {
            _config = config;
            connectionString = _config.GetConnectionString("Default");
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

        [HttpGet("warmup")]
        public async Task<IActionResult> Warmup()
        {
            try
            {
                await userDB.Warmup();
                return Ok(userDB.usernames);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to warmup ", error = ex.Message });
            }
        }

        [HttpPost("passwordhash")]
        public async Task<IActionResult> GetPasswordHash([FromBody] string username)
        {
            try
            {
                string passwordHash = await userDB.GetPasswordHashDBAsync(username);
                if (passwordHash == null)
                    return NotFound(new { message = "Cannot find user" });
                return new JsonResult(passwordHash);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Database failed", detail = ex.Message });
            }
        }

        [HttpPost("getuser")]
        public async Task<IActionResult> GetUser([FromBody] string username)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                UserModel returnUser = new UserModel();
                returnUser = await userDB.GetUserDBAsync(username);
                sw.Stop();
                Console.WriteLine($"GetUser total took {sw.ElapsedMilliseconds} ms");
                return Ok(returnUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get user ", error = ex.Message });
            }
        }

        [HttpPost("adduser")]
        public async Task<IActionResult> AddUser([FromBody] UserModel newUser)
        {
            try
            {
                await userDB.AddUserDBAsync(newUser);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to add user ", error = ex.Message });
            }
        }

        [HttpPost("loginwithtoken")]
        public async Task<IActionResult> LoginWithToken([FromBody] string token)
        {
            UserModel returnUser = new UserModel();

            returnUser = await userDB.GetUserByTokenDBAsync(token);

            return Ok(returnUser);
        }

        [HttpPost("securityquestion")]
        public async Task<IActionResult> GetSecurityQuestion([FromBody] string username)
        {
            return Ok(await userDB.GetSecurityQuestionDBAsync(username));
        }

        [HttpPost("putresettoken")]
        public async Task<IActionResult> PutResetToken([FromBody] RecoveryInfo recovery)
        {
            await userDB.ResetUserPasswordDBAsync(recovery.Username, recovery.Token);
            return NoContent();
        }

        [HttpPost("verifytoken")]
        public async Task<IActionResult> VerifyToken([FromBody] RecoveryInfo recovery)
        {
            return Ok(await userDB.VerifyTokenDBAsync(recovery.Username, recovery.Token));
        }

        [HttpPost("updatepassword")]
        public async Task<IActionResult> UpdateUserPassword([FromBody] RecoveryInfo recovery)
        {
            await userDB.UpdateUserPasswordDBAsync(recovery.Id, recovery.Token);
            return NoContent();
        }

        [HttpPost("getidfromusername")]
        public async Task<IActionResult> GetIdFromUsername([FromBody] string username)
        {
            try
            {
                return Ok(await userDB.GetIdFromUsername(username));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get userId ", error = ex.Message });
            }
        }

        [HttpGet("getrecoveryinfo")]
        public async Task<IActionResult> GetRecoveryInfo()
        {
            try
            {
                List<RecoveryInfo> recovery = new List<RecoveryInfo>();
                recovery = await userDB.GetRecoveryInfoDBAsync();
                return Ok(recovery);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get recovery info ", error = ex.Message });
            }
        }
    }
}
