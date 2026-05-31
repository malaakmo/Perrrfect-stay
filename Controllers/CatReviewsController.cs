using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PerrrfectStayAPI.Data;

namespace PerrrfectStayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatReviewsController : ControllerBase
    {
        private readonly Database _db;
        public CatReviewsController(Database db) { _db = db; }

        // GET api/catreviews/cat/1
        [HttpGet("cat/{catId}")]
        public IActionResult GetByCat(int catId)
        {
            var list = new List<object>();
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"
                SELECT cr.*, u.first_name, u.last_name FROM cat_reviews cr
                JOIN users u ON cr.user_id = u.id
                WHERE cr.cat_id = @catId
                ORDER BY cr.created_at DESC", conn);
            cmd.Parameters.AddWithValue("@catId", catId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new
                {
                    id = reader.GetInt32("id"),
                    userName = $"{reader.GetString("first_name")} {reader.GetString("last_name")}",
                    rating = reader.GetInt32("rating"),
                    note = reader.IsDBNull(reader.GetOrdinal("note")) ? "" : reader.GetString("note"),
                    createdAt = reader.GetDateTime("created_at")
                });
            return Ok(list);
        }

        // POST api/catreviews
        [HttpPost]
        public IActionResult CreateReview([FromBody] CatReviewRequest request)
        {
            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest(new { message = "Rating must be between 1 and 5." });
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("INSERT INTO cat_reviews (user_id, cat_id, rating, note) VALUES (@userId, @catId, @rating, @note)", conn);
            cmd.Parameters.AddWithValue("@userId", request.UserId);
            cmd.Parameters.AddWithValue("@catId", request.CatId);
            cmd.Parameters.AddWithValue("@rating", request.Rating);
            cmd.Parameters.AddWithValue("@note", (object?)request.Note ?? DBNull.Value);
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Review submitted." });
        }

        // DELETE api/catreviews/5
        [HttpDelete("{id}")]
        public IActionResult DeleteReview(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("DELETE FROM cat_reviews WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Review deleted." });
        }
    }

    public class CatReviewRequest
    {
        public int UserId { get; set; }
        public int CatId { get; set; }
        public int Rating { get; set; }
        public string? Note { get; set; }
    }
}