using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PerrrfectStayAPI.Data;
using PerrrfectStayAPI.Models;

namespace PerrrfectStayAPI.Controllers
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

        // PUT api/users/5/change-password
        [HttpPost("{id}/change-password")]
        public IActionResult ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("SELECT password_hash FROM users WHERE id = @id AND is_deleted = 0", conn);
            cmd.Parameters.AddWithValue("@id", id);
            var hash = cmd.ExecuteScalar()?.ToString();
            if (hash == null) return NotFound(new { message = "User not found." });
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, hash))
                return Unauthorized(new { message = "Current password is incorrect." });
            string newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            var update = new MySqlCommand("UPDATE users SET password_hash = @hash WHERE id = @id", conn);
            update.Parameters.AddWithValue("@hash", newHash);
            update.Parameters.AddWithValue("@id", id);
            update.ExecuteNonQuery();
            return Ok(new { message = "Password changed successfully." });
        }

        // PUT api/users/5/forgot-password
        [HttpPost("forgot-password")]
        public IActionResult ChangePassword(int id, [FromBody] ForgotPasswordRequest request)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("SELECT id FROM users WHERE email = @email AND is_deleted = 0", conn);
            cmd.Parameters.AddWithValue("@email", request.Email);
            var userId = cmd.ExecuteScalar();
            if (userId == null) return NotFound(new { message = "No account found with that email." });
            string token = Guid.NewGuid().ToString();
            var tokenCmd = new MySqlCommand("UPDATE users SET reset_token = @token WHERE id = @id", conn);
            tokenCmd.Parameters.AddWithValue("@token", token);
            tokenCmd.Parameters.AddWithValue("@id", userId);
            tokenCmd.ExecuteNonQuery();
            return Ok(new { message = "Password reset token generated.", resetToken = token });
        }

        // POST api/users/reset-password
        [HttpPost("reset-password")]
        public IActionResult ChangePassword(int id, [FromBody] ResetPasswordRequest request)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("SELECT id FROM users WHERE reset_token = @token AND is_deleted = 0", conn);
            cmd.Parameters.AddWithValue("@token", request.Token);
            var userId = cmd.ExecuteScalar();
            if (userId == null) return BadRequest(new { message = "Invalid or expired reset token." });
            string newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            var update = new MySqlCommand("UPDATE users SET password_hash = @hash, reset_token = NULL WHERE id = @id", conn);
            update.Parameters.AddWithValue("@hash", newHash);
            update.Parameters.AddWithValue("@id", userId);
            update.ExecuteNonQuery();
            return Ok(new { message = "Password reset successfully." });
        }

        // POST api/users/uploadpfp/5
        [HttpPost("upload-profile-picture/{id}")]
        public async Task<IActionResult> UploadProfilePicture(int id, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest(new { message = "No file uploaded." });
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
            string ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(ext)) return BadRequest(new { message = "Invalid file type." });
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/profile-pictures");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            string uniqueFileName = $"{Guid.NewGuid()}{ext}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("UPDATE users SET profile_picture_path = @path WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@path", $"/profile-pictures/{uniqueFileName}");
            cmd.Parameters.AddWithValue("@id", id);
            int rows = cmd.ExecuteNonQuery();
            if (rows == 0) return NotFound(new { message = "User not found." });
            return Ok(new { message = "Profile picture uploaded successfully.", path = $"/profile-pictures/{uniqueFileName}" });

        }
    }
}
