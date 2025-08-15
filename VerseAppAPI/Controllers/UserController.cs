using DBAccessLibrary.Models;
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

        [HttpPost("getuser")]
        public async Task<IActionResult> GetUser([FromBody] string username)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                User returnUser = new User();
                returnUser = await userDB.GetUserAsync(username);
                sw.Stop();
                Console.WriteLine($"GetUser total took {sw.ElapsedMilliseconds} ms");
                return Ok(returnUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get user ", error = ex.Message });
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

        [HttpPost("adduser")]
        public async Task<IActionResult> AddUser([FromBody] User newUser)
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
            User returnUser = new User();

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

        [HttpPost("getusernotifications")]
        public async Task<IActionResult> GetUserNotifications([FromBody] string username)
        {
            try
            {
                var result = await userDB.GetUserNotifications(username);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get user notifications ", error = ex.Message });
            }
        }

        [HttpPost("marknotificationasread")]
        public async Task<IActionResult> MarkNotificationAsRead([FromBody] int notificationId)
        {
            try
            {
                await userDB.MarkNotificationAsRead(notificationId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to mark notification as read ", error = ex.Message });
            }
        }

        [HttpPost("deletenotification")]
        public async Task<IActionResult> DeleteNotification([FromBody] int notificationId)
        {
            try
            {
                await userDB.DeleteNotification(notificationId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete notification ", error = ex.Message });
            }
        }

        [HttpPost("sendnotification")]
        public async Task<IActionResult> SendNotification([FromBody] Notification notification)
        {
            try
            {
                await userDB.SendNotification(notification);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to send notification ", error = ex.Message });
            }
        }

        [HttpPost("sendemailnotification")]
        public async Task<IActionResult> SendEmailNotification([FromBody] Notification notification)
        {
            try
            {
                List<EmailNotification> allUserEmails = await userDB.GetAllUserEmailsAsync();

                foreach (var email in allUserEmails)
                {
                    string notificationTitleWithUsername = notification.Title.Replace("<username>", email.FullName);
                    string notificationBodyWithUsername = notification.Message.Replace("<username>", email.FullName);

                    var emailMessage = new MailMessage("therealjoshrobertson@gmail.com", email.Email)
                    {
                        Subject = notificationTitleWithUsername,
                        Body = notificationBodyWithUsername + "\n\nSincerely,\nThe VerseApp Team"
                    };

                    var smtp = new SmtpClient("smtp.gmail.com", 587)
                    {
                        EnableSsl = true,
                        Credentials = new NetworkCredential("therealjoshrobertson@gmail.com", "tofp kaki lkuv nffh")
                    };

                    await smtp.SendMailAsync(emailMessage);
                }

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

        [HttpGet("getallusernames")]
        public async Task<IActionResult> GetAllUsernames()
        {
            try
            {
                List<string> usernames = new List<string>();
                usernames = await userDB.GetAllUsernamesAsync();
                return Ok(usernames);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get all usernames ", error = ex.Message });
            }
        }

        [HttpPost("checkifusernameexists")]
        public async Task<IActionResult> CheckIfUsernameExists([FromBody] string username)
        {
            try
            {
                int exists = await userDB.CheckUsernameExists(username);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to check if username exists ", error = ex.Message });
            }
        }

        [HttpPost("setuseractive")]
        public async Task<IActionResult> SetUserActive([FromBody] int userId)
        {
            try
            {
                await userDB.SetUserActive(userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to set user active ", error = ex.Message });
            }
        }

        [HttpGet("getalluseres")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                List<User> users = await userDB.GetAllUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get all users ", error = ex.Message });
            }
        }
    }
}
