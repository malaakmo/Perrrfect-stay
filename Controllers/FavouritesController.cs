using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PerrrfectStayAPI.Data;

namespace PerrrfectStayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavouritesController : ControllerBase
    {
        private readonly Database _db;
        public FavouritesController(Database db) { _db = db; }

        // GET api/favourites/user/2
        [HttpGet("user/{userId}")]
        public IActionResult GetFavourites(int userId)
        {
            var cats = new List<object>();
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"
                SELECT c.* FROM cats c
                JOIN favourites f ON c.id = f.cat_id
                WHERE f.user_id = @userId", conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                cats.Add(new
                {
                    id = reader.GetInt32("id"),
                    name = reader.GetString("name"),
                    breed = reader.GetString("breed")
                });
            }
            return Ok(cats);
        }

        // POST api/favourites
        [HttpPost]
        public IActionResult AddFavourite([FromBody] FavouriteRequest request)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("INSERT IGNORE INTO favourites (user_id, cat_id) VALUES (@userId, @catId)", conn);
            cmd.Parameters.AddWithValue("@userId", request.UserId);
            cmd.Parameters.AddWithValue("@catId", request.CatId);
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Added to favourites." });
        }

        // DELETE api/favourites
        [HttpDelete]
        public IActionResult RemoveFavourite([FromBody] FavouriteRequest request)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("DELETE FROM favourites WHERE user_id = @userId AND cat_id = @catId", conn);
            cmd.Parameters.AddWithValue("@userId", request.UserId);
            cmd.Parameters.AddWithValue("@catId", request.CatId);
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Removed from favourites." });
        }
    }

    public class FavouriteRequest
    {
        public int UserId { get; set; }
        public int CatId { get; set; }
    }
}