using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Oracle.ManagedDataAccess.Client;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using VerseAppAPI.Models;

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

        [HttpPost("usernames")]
        public async Task<IActionResult> CheckForUsername([FromBody] string username)
        {
            try
            {
                bool result = await userDB.CheckUsernameExists(username);
                return Ok(result);
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

        [HttpPost("putresettoken")]
        public async Task<IActionResult> PutResetToken([FromBody] RecoveryInfo recovery)
        {
            await userDB.ResetUserPasswordDBAsync(recovery.Username, recovery.Token);
            return NoContent();
        }

        [HttpPost("verifytoken")]
        public async Task<IActionResult> VerifyToken([FromBody] ResetPassword reset)
        {
            return Ok(await userDB.VerifyTokenDBAsync(reset.Username, reset.Token));
        }

        [HttpPost("updatepassword")]
        public async Task<IActionResult> UpdateUserPassword([FromBody] ResetPassword reset)
        {
            await userDB.UpdateUserPasswordDBAsync(reset.Id, reset.PasswordHash);
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

        [HttpPost("sendresetlink")]
        public async Task<IActionResult> SendResetLink([FromBody] ResetPassword reset)
        {
            try
            {
                if (reset == null)
                    return BadRequest("User not found.");

                var token = Guid.NewGuid().ToString();
                var resetUrl = $"https://localhost:7025/authentication/forgot/resetpassword/{token}/{reset.Username}";

                var emailMessage = new MailMessage("therealjoshrobertson@gmail.com", reset.Email)
                {
                    Subject = "Reset Your Password",
                    Body = $"Click the link below to reset your password:\n\n{resetUrl}\n\nThis link expires in one hour."
                };

                var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential("therealjoshrobertson@gmail.com", "tofp kaki lkuv nffh")
                };

                await smtp.SendMailAsync(emailMessage);

                await userDB.ResetUserPasswordDBAsync(reset.Username, token);
                return Ok();
            }
            catch (SmtpException smtpEx)
            {
                Console.Write(smtpEx.ToString());
                return StatusCode(500, new { message = "Failed to send reset link via email", error = smtpEx.Message });
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return StatusCode(500, new { message = "Failed to send reset link ", error = ex.Message });
            }
        }
    }
}
