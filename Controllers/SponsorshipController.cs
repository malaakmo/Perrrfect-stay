using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PerrrfectStayAPI.Data;

namespace PerrrfectStayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SponsorshipController : ControllerBase
    {
        private readonly Database _db;
        public SponsorshipController(Database db) { _db = db; }

        // GET api/sponsorship/user/2
        [HttpGet("user/{userId}")]
        public IActionResult GetByUser(int userId)
        {
            var list = new List<object>();
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"
                SELECT sr.*, c.name as cat_name FROM sponsorship_requests sr
                JOIN cats c ON sr.cat_id = c.id
                WHERE sr.user_id = @userId", conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new
                {
                    id = reader.GetInt32("id"),
                    catName = reader.GetString("cat_name"),
                    monthlyAmount = reader.GetDecimal("monthly_amount"),
                    status = reader.GetString("status"),
                    requestDate = reader.GetDateTime("request_date")
                });
            return Ok(list);
        }

        // GET api/sponsorship/pending
        [HttpGet("pending")]
        public IActionResult GetPending()
        {
            var list = new List<object>();
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"
                SELECT sr.*, c.name as cat_name, u.first_name, u.last_name 
                FROM sponsorship_requests sr
                JOIN cats c ON sr.cat_id = c.id
                JOIN users u ON sr.user_id = u.id
                WHERE sr.status = 'pending'", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new
                {
                    id = reader.GetInt32("id"),
                    catName = reader.GetString("cat_name"),
                    userName = $"{reader.GetString("first_name")} {reader.GetString("last_name")}",
                    monthlyAmount = reader.GetDecimal("monthly_amount"),
                    requestDate = reader.GetDateTime("request_date")
                });
            return Ok(list);
        }

        // POST api/sponsorship
        [HttpPost]
        public IActionResult CreateRequest([FromBody] SponsorshipRequest request)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("INSERT INTO sponsorship_requests (user_id, cat_id, monthly_amount) VALUES (@userId, @catId, @amount)", conn);
            cmd.Parameters.AddWithValue("@userId", request.UserId);
            cmd.Parameters.AddWithValue("@catId", request.CatId);
            cmd.Parameters.AddWithValue("@amount", request.MonthlyAmount);
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Sponsorship request submitted." });
        }

        // PATCH api/sponsorship/5/status
        [HttpPatch("{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] string status)
        {
            var allowed = new[] { "approved", "rejected" };
            if (!allowed.Contains(status)) return BadRequest(new { message = "Invalid status." });
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("UPDATE sponsorship_requests SET status = @status WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            return Ok(new { message = $"Request {status}." });
        }
    }

    public class SponsorshipRequest
    {
        public int UserId { get; set; }
        public int CatId { get; set; }
        public decimal MonthlyAmount { get; set; }
    }
}