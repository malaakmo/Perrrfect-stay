using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PerrrfectStayAPI.Data;

namespace PerrrfectStayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdoptionController : ControllerBase
    {
        private readonly Database _db;
        public AdoptionController(Database db) { _db = db; }

        // GET api/adoption/user/2
        [HttpGet("user/{userId}")]
        public IActionResult GetByUser(int userId)
        {
            var list = new List<object>();
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"
                SELECT ar.*, c.name as cat_name FROM adoption_requests ar
                JOIN cats c ON ar.cat_id = c.id
                WHERE ar.user_id = @userId", conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new
                {
                    id = reader.GetInt32("id"),
                    catName = reader.GetString("cat_name"),
                    status = reader.GetString("status"),
                    requestDate = reader.GetDateTime("request_date")
                });
            return Ok(list);
        }

        // GET api/adoption/pending
        [HttpGet("pending")]
        public IActionResult GetPending()
        {
            var list = new List<object>();
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"
                SELECT ar.*, c.name as cat_name, u.first_name, u.last_name 
                FROM adoption_requests ar
                JOIN cats c ON ar.cat_id = c.id
                JOIN users u ON ar.user_id = u.id
                WHERE ar.status = 'pending'", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new
                {
                    id = reader.GetInt32("id"),
                    catName = reader.GetString("cat_name"),
                    userName = $"{reader.GetString("first_name")} {reader.GetString("last_name")}",
                    requestDate = reader.GetDateTime("request_date")
                });
            return Ok(list);
        }

        // POST api/adoption
        [HttpPost]
        public IActionResult CreateRequest([FromBody] AdoptionRequest request)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("INSERT INTO adoption_requests (user_id, cat_id) VALUES (@userId, @catId)", conn);
            cmd.Parameters.AddWithValue("@userId", request.UserId);
            cmd.Parameters.AddWithValue("@catId", request.CatId);
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Adoption request submitted." });
        }

        // PATCH api/adoption/5/status
        [HttpPatch("{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] string status)
        {
            var allowed = new[] { "approved", "rejected" };
            if (!allowed.Contains(status)) return BadRequest(new { message = "Invalid status." });
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("UPDATE adoption_requests SET status = @status WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            return Ok(new { message = $"Request {status}." });
        }
    }

    public class AdoptionRequest
    {
        public int UserId { get; set; }
        public int CatId { get; set; }
    }
}