using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using DBAccessLibrary.Models;
using DBAccessLibrary;
using static System.Net.WebRequestMethods;
using System.Net.Http.Headers;
using VerseAppAPI.Models;

namespace VerseAppAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VerseController : ControllerBase
    {
        private UserControllerDB userDB; // To call Oracle commands
        private VerseControllerDB verseDB;

        private IConfiguration _config;
        private string connectionString;
        public VerseController(IConfiguration config, UserControllerDB UserDB, VerseControllerDB verseDB)
        {
            _config = config;
            connectionString = _config.GetConnectionString("Default");
            userDB = UserDB;
            this.verseDB = verseDB;
        }

        [HttpPost("getuserversebyreference")]
        public async Task<IActionResult> CheckForUsername([FromBody] Reference reference)
        {
            try
            {
                var result = await verseDB.GetUserVerseFromReference(reference);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get user verse from reference ", error = ex.Message });
            }
        }
    }
}
