using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PerrrfectStayAPI.Data;
using PerrrfectStayAPI.Models;

namespace PerrrfectStayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SittingController : ControllerBase
    {
        private readonly Database _db;
        public SittingController(Database db) { _db = db; }

        // GET api/sitting/sitters
        [HttpGet("sitters")]
        public IActionResult GetSitters()
        {
            var sitters = new List<SitterProfile>();
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"SELECT sp.*, u.first_name, u.last_name 
                FROM sitter_profiles sp 
                JOIN users u ON sp.user_id = u.id 
                WHERE sp.is_available = 1", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                sitters.Add(new SitterProfile
                {
                    Id = reader.GetInt32("id"),
                    UserId = reader.GetInt32("user_id"),
                    Bio = reader.IsDBNull(reader.GetOrdinal("bio")) ? null : reader.GetString("bio"),
                    PricePerDay = reader.GetDecimal("price_per_day"),
                    Rating = reader.GetDecimal("rating"),
                    IsAvailable = reader.GetBoolean("is_available"),
                    FirstName = reader.GetString("first_name"),
                    LastName = reader.GetString("last_name")
                });
            }
            return Ok(sitters);
        }

        // GET api/sitting/bookings/owner/3
        [HttpGet("bookings/owner/{ownerId}")]
        public IActionResult GetBookingsByOwner(int ownerId)
        {
            var bookings = new List<SittingBooking>();
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("SELECT * FROM sitting_bookings WHERE owner_id = @ownerId ORDER BY created_at DESC", conn);
            cmd.Parameters.AddWithValue("@ownerId", ownerId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                bookings.Add(MapBooking(reader));
            }
            return Ok(bookings);
        }

        // GET api/sitting/bookings/sitter/3
        [HttpGet("bookings/sitter/{sitterId}")]
        public IActionResult GetBookingsBySitter(int sitterId)
        {
            var bookings = new List<SittingBooking>();
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("SELECT * FROM sitting_bookings WHERE sitter_id = @sitterId ORDER BY created_at DESC", conn);
            cmd.Parameters.AddWithValue("@sitterId", sitterId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                bookings.Add(MapBooking(reader));
            }
            return Ok(bookings);
        }

        // POST api/sitting/bookings
        [HttpPost("bookings")]
        public IActionResult CreateBooking([FromBody] CreateSittingBookingRequest request)
        {
            int days = (request.EndDate.Date - request.StartDate.Date).Days;
            if (days <= 0) return BadRequest(new { message = "End date must be after start date." });

            using var conn = _db.GetConnection();
            conn.Open();

            var priceCmd = new MySqlCommand("SELECT price_per_day FROM sitter_profiles WHERE user_id = @sitterId", conn);
            priceCmd.Parameters.AddWithValue("@sitterId", request.SitterId);
            var priceResult = priceCmd.ExecuteScalar();
            if (priceResult == null) return NotFound(new { message = "Sitter not found." });

            decimal total = (decimal)priceResult * days;

            var cmd = new MySqlCommand(@"INSERT INTO sitting_bookings 
                (owner_id, sitter_id, cat_id, start_date, end_date, total_price, message) 
                VALUES (@ownerId, @sitterId, @catId, @start, @end, @total, @message)", conn);
            cmd.Parameters.AddWithValue("@ownerId", request.OwnerId);
            cmd.Parameters.AddWithValue("@sitterId", request.SitterId);
            cmd.Parameters.AddWithValue("@catId", request.CatId);
            cmd.Parameters.AddWithValue("@start", request.StartDate.Date);
            cmd.Parameters.AddWithValue("@end", request.EndDate.Date);
            cmd.Parameters.AddWithValue("@total", total);
            cmd.Parameters.AddWithValue("@message", (object?)request.Message ?? DBNull.Value);
            cmd.ExecuteNonQuery();

            return Ok(new { totalPrice = total, message = "Booking request sent." });
        }

        // PATCH api/sitting/bookings/5/status
        [HttpPatch("bookings/{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] string status)
        {
            var allowed = new[] { "pending", "accepted", "rejected", "completed" };
            if (!allowed.Contains(status)) return BadRequest(new { message = "Invalid status." });

            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("UPDATE sitting_bookings SET status = @status WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@id", id);
            int rows = cmd.ExecuteNonQuery();
            if (rows == 0) return NotFound(new { message = "Booking not found." });
            return Ok(new { message = $"Booking status updated to '{status}'." });
        }

        // POST api/sitting/reviews
        [HttpPost("reviews")]
        public IActionResult CreateReview([FromBody] CreateReviewRequest request)
        {
            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest(new { message = "Rating must be between 1 and 5." });

            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"INSERT INTO reviews (booking_id, reviewer_id, rating, comment) 
                VALUES (@bookingId, @reviewerId, @rating, @comment)", conn);
            cmd.Parameters.AddWithValue("@bookingId", request.BookingId);
            cmd.Parameters.AddWithValue("@reviewerId", request.ReviewerId);
            cmd.Parameters.AddWithValue("@rating", request.Rating);
            cmd.Parameters.AddWithValue("@comment", (object?)request.Comment ?? DBNull.Value);
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Review submitted." });
        }

        private static SittingBooking MapBooking(MySqlDataReader reader) => new SittingBooking
        {
            Id = reader.GetInt32("id"),
            OwnerId = reader.GetInt32("owner_id"),
            SitterId = reader.GetInt32("sitter_id"),
            CatId = reader.GetInt32("cat_id"),
            StartDate = reader.GetDateTime("start_date"),
            EndDate = reader.GetDateTime("end_date"),
            TotalPrice = reader.GetDecimal("total_price"),
            Status = reader.GetString("status"),
            Message = reader.IsDBNull(reader.GetOrdinal("message")) ? null : reader.GetString("message"),
            CreatedAt = reader.GetDateTime("created_at")
        };
    }
}
