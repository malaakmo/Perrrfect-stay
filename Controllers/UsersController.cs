using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PurrfectStayAPI.Data;
using PurrfectStayAPI.Models;

namespace PurrfectStayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly Database _db;
        public UsersController(Database db) { _db = db; }

        // GET api/users
        [HttpGet]
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

        // GET api/users/5
        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("SELECT id, first_name, last_name, email, role, is_verified, created_at FROM users WHERE id = @id AND is_deleted = 0", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return NotFound(new { message = "User not found." });
            return Ok(new User
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

        //users/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var check = new MySqlCommand("SELECT id FROM users WHERE email = @email", conn);
            check.Parameters.AddWithValue("@email", request.Email);
            if (check.ExecuteScalar() != null)
                return Conflict(new { message = "Email already exists." });

            string hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var cmd = new MySqlCommand("INSERT INTO users (first_name, last_name, email, password_hash, role) VALUES (@first, @last, @email, @hash, @role)", conn);
            cmd.Parameters.AddWithValue("@first", request.FirstName);
            cmd.Parameters.AddWithValue("@last", request.LastName);
            cmd.Parameters.AddWithValue("@email", request.Email);
            cmd.Parameters.AddWithValue("@hash", hash);
            cmd.Parameters.AddWithValue("@role", request.Role);
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Account created successfully." });
        }

        //users/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("SELECT id, first_name, last_name, email, password_hash, role FROM users WHERE email = @email AND is_deleted = 0", conn);
            cmd.Parameters.AddWithValue("@email", request.Email);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return Unauthorized(new { message = "Invalid email or password." });
            string storedHash = reader.GetString("password_hash");
            if (!BCrypt.Net.BCrypt.Verify(request.Password, storedHash))
                return Unauthorized(new { message = "Invalid email or password." });
            return Ok(new { id = reader.GetInt32("id"), firstName = reader.GetString("first_name"), lastName = reader.GetString("last_name"), email = reader.GetString("email"), role = reader.GetString("role") });
        }

        // DELETE api/users/5
        [HttpDelete("{id}")]
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
    }
}