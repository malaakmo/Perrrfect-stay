using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PurrfectStayAPI.Data;
using PurrfectStayAPI.Models;

namespace PurrfectStayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly Database _db;
        public AdminController(Database db) { _db = db; }

        // GET api/admin/users
        [HttpGet("users")]
        public IActionResult GetAllUsers()
        {
            var users = new List<User>();
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("SELECT id, first_name, last_name, email, role, is_verified, created_at FROM users WHERE is_deleted = 0", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new User
                {
                    Id = reader.GetInt32("id"),
                    FirstName = reader.GetString("first_name"),
                    LastName = reader.GetString("last_name"),
                    Email = reader.GetString("email"),
                    Role = reader.GetString("role"),
                    IsVerified = reader.GetBoolean("is_verified"),
                    CreatedAt = reader.GetDateTime("created_at")
                });
            }
            return Ok(users);
        }

        // PUT api/admin/users/5
        [HttpPut("users/{id}")]
        public IActionResult EditUser(int id, [FromBody] RegisterRequest request)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("UPDATE users SET first_name=@first, last_name=@last, email=@email, role=@role WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("@first", request.FirstName);
            cmd.Parameters.AddWithValue("@last", request.LastName);
            cmd.Parameters.AddWithValue("@email", request.Email);
            cmd.Parameters.AddWithValue("@role", request.Role);
            cmd.Parameters.AddWithValue("@id", id);
            int rows = cmd.ExecuteNonQuery();
            if (rows == 0) return NotFound(new { message = "User not found." });
            return Ok(new { message = "User updated." });
        }

        // DELETE api/admin/users/5
        [HttpDelete("users/{id}")]
        public IActionResult DeleteUser(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("UPDATE users SET is_deleted = 1 WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            int rows = cmd.ExecuteNonQuery();
            if (rows == 0) return NotFound(new { message = "User not found." });
            return Ok(new { message = "User deleted." });
        }

        // POST api/admin/users/5/reset-password
        [HttpPost("users/{id}/reset-password")]
        public IActionResult AdminResetPassword(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var check = new MySqlCommand("SELECT id FROM users WHERE id = @id AND is_deleted = 0", conn);
            check.Parameters.AddWithValue("@id", id);
            if (check.ExecuteScalar() == null) return NotFound(new { message = "User not found." });
            string token = Guid.NewGuid().ToString();
            var cmd = new MySqlCommand("UPDATE users SET reset_token = @token WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@token", token);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Password reset token generated.", resetToken = token });
        }

        // GET api/admin/stats
        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var stats = new Dictionary<string, object>();

            var queries = new Dictionary<string, string>
            {
                { "totalUsers", "SELECT COUNT(*) FROM users WHERE is_deleted = 0" },
                { "verifiedUsers", "SELECT COUNT(*) FROM users WHERE is_verified = 1 AND is_deleted = 0" },
                { "totalCats", "SELECT COUNT(*) FROM cats" },
                { "totalHotelBookings", "SELECT COUNT(*) FROM hotel_bookings" },
                { "totalCafeReservations", "SELECT COUNT(*) FROM cafe_reservations" },
                { "totalSittingBookings", "SELECT COUNT(*) FROM sitting_bookings" },
                { "hotelRevenue", "SELECT IFNULL(SUM(total_price),0) FROM hotel_bookings WHERE status != 'cancelled'" },
                { "sittingRevenue", "SELECT IFNULL(SUM(total_price),0) FROM sitting_bookings WHERE status = 'completed'" }
            };

            foreach (var q in queries)
            {
                var cmd = new MySqlCommand(q.Value, conn);
                stats[q.Key] = cmd.ExecuteScalar() ?? 0;
            }

            return Ok(stats);
        }

        // GET api/admin/stats/download
        [HttpGet("stats/download")]
        public IActionResult DownloadStats()
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("PurrfectStay - User Stats Report");
            sb.AppendLine($"Generated: {DateTime.Now}");
            sb.AppendLine("-----------------------------------");

            var cmd = new MySqlCommand("SELECT id, first_name, last_name, email, role, is_verified, created_at FROM users WHERE is_deleted = 0", conn);
            using var reader = cmd.ExecuteReader();
            sb.AppendLine("ID,First Name,Last Name,Email,Role,Verified,Created At");
            while (reader.Read())
            {
                sb.AppendLine($"{reader["id"]},{reader["first_name"]},{reader["last_name"]},{reader["email"]},{reader["role"]},{reader["is_verified"]},{reader["created_at"]}");
            }

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "user_stats.csv");
        }
    }
}