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

        [HttpPost("getuserversebykeywords")]
        public async Task<IActionResult> GetUserVerseByKeywords([FromBody] List<string> keywords)
        {
            try
            {
                var result = await verseDB.GetUserVerseByKeywords(keywords);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get user verse by keywords ", error = ex.Message });
            }
        }

        [HttpPost("addnewcollection")]
        public async Task<IActionResult> AddNewCollection([FromBody] Collection collection)
        {
            try
            {
                await verseDB.AddNewCollection(collection);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to add new collection ", error = ex.Message });
            }
        }

        [HttpPost("getusercollections")]
        public async Task<IActionResult> GetUserCollections([FromBody] string username)
        {
            try
            {
                var result = await verseDB.GetUserCollections(username);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get user collections ", error = ex.Message });
            }
        }

        [HttpPost("deletecollection")]
        public async Task<IActionResult> DeleteCollection([FromBody] int collectionId)
        {
            try
            {
                await verseDB.DeleteCollection(collectionId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete collection ", error = ex.Message });
            }
        }

        [HttpPost("getcollection")]
        public async Task<IActionResult> GetCollection([FromBody] int collectionId)
        {
            try
            {
                var result = await verseDB.GetCollectionById(collectionId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get collection ", error = ex.Message });
            }
        }

        [HttpPost("adduserversestonewcollection")]
        public async Task<IActionResult> AddUserVerses([FromBody] List<UserVerse> userVerses)
        {
            try
            {
                await verseDB.AddUserVersestonewcollection(userVerses);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to add user verses ", error = ex.Message });
            }
        }
    }
}
