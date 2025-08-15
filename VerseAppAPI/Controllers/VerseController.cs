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
        public async Task<IActionResult> GetUserCollections([FromBody] int userId)
        {
            try
            {
                var result = await verseDB.GetUserCollections(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get user collections ", error = ex.Message });
            }
        }

        [HttpPost("getversesbycollection")]
        public async Task<IActionResult> GetVersesByCollection([FromBody] Collection collection)
        {
            try
            {
                if (collection.NumVerses <= 0)
                    return Ok(collection);

                Collection returnCollection = new Collection();

                //// Parse reference and get list of all individual verses
                //// Loop through and get each verse

                //foreach (var userVerse in collection.UserVerses)
                //{
                //    List<string> individualReferences = ReferenceParse.GetIndividualVersesFromReference(userVerse.Reference);
                //    userVerse.Verses = await verseDB.GetVersesByReferences(individualReferences);
                //}

                //List<string> allReferences = ReferenceParse.GetIndividualVersesFromReference(collection)

                var allReferences = collection.UserVerses.SelectMany(uv => ReferenceParse.GetIndividualVersesFromReference(uv.Reference))
                                                         .Distinct().ToList();

                var allVerses = await verseDB.GetVersesByReferences(allReferences);

                var versesByReference = allVerses.ToDictionary(v => v.Reference, StringComparer.OrdinalIgnoreCase);

                foreach (var userVerse in collection.UserVerses)
                {
                    var reference = ReferenceParse.GetIndividualVersesFromReference(userVerse.Reference);
                    userVerse.Verses = reference.Where(r => versesByReference.TryGetValue(r, out _))
                                                .Select(r => versesByReference[r]).ToList();
                }

                return Ok(returnCollection);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get verses from collection ", error = ex.Message });
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

        [HttpPost("togglepincolllection")]
        public async Task<IActionResult> TogglePinCollection([FromBody] Collection collection)
        {
            try
            {
                await verseDB.TogglePinCollection(collection);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to toggle pin collection ", error = ex.Message });
            }
        }

        [HttpPost("updatecollectionsorder")]
        public async Task<IActionResult> UpdateCollectionsOrder([FromBody] OrderInfo order)
        {
            try
            {
                await verseDB.UpdateCollectionsOrder(order);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update collections order ", error = ex.Message });
            }
        }
    }
}
