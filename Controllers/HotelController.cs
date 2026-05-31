using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PerrrfectStayAPI.Data;
using PerrrfectStayAPI.Models;

namespace PerrrfectStayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelController : ControllerBase
    {
        private readonly Database _db;
        public HotelController(Database db) { _db = db; }

        // GET api/hotel/rooms
        [HttpGet("rooms")]
        public IActionResult GetRooms()
        {
            var rooms = new List<Room>();
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("SELECT * FROM rooms", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                rooms.Add(new Room
                {
                    Id = reader.GetInt32("id"),
                    RoomNumber = reader.GetString("room_number"),
                    Type = reader.GetString("type"),
                    PricePerDay = reader.GetDecimal("price_per_day"),
                    IsAvailable = reader.GetBoolean("is_available"),
                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString("description")
                });
            }
            return Ok(rooms);
        }

        // GET api/hotel/rooms/available?checkIn=2025-06-01&checkOut=2025-06-05
        [HttpGet("rooms/available")]
        public IActionResult GetAvailableRooms([FromQuery] DateTime checkIn, [FromQuery] DateTime checkOut)
        {
            var rooms = new List<Room>();
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"SELECT * FROM rooms WHERE is_available = 1 AND id NOT IN (
                SELECT room_id FROM hotel_bookings WHERE status != 'cancelled'
                AND check_in < @checkOut AND check_out > @checkIn)", conn);
            cmd.Parameters.AddWithValue("@checkIn", checkIn.Date);
            cmd.Parameters.AddWithValue("@checkOut", checkOut.Date);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                rooms.Add(new Room
                {
                    Id = reader.GetInt32("id"),
                    RoomNumber = reader.GetString("room_number"),
                    Type = reader.GetString("type"),
                    PricePerDay = reader.GetDecimal("price_per_day"),
                    IsAvailable = reader.GetBoolean("is_available"),
                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString("description")
                });
            }
            return Ok(rooms);
        }

        // GET api/hotel/services
        [HttpGet("services")]
        public IActionResult GetExtraServices()
        {
            var services = new List<ExtraService>();
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("SELECT * FROM extra_services", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                services.Add(new ExtraService
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Price = reader.GetDecimal("price")
                });
            }
            return Ok(services);
        }

        // GET api/hotel/bookings/user/3
        [HttpGet("bookings/user/{userId}")]
        public IActionResult GetBookingsByUser(int userId)
        {
            var bookings = new List<HotelBooking>();
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("SELECT * FROM hotel_bookings WHERE user_id = @userId ORDER BY created_at DESC", conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                bookings.Add(new HotelBooking
                {
                    Id = reader.GetInt32("id"),
                    UserId = reader.GetInt32("user_id"),
                    CatId = reader.GetInt32("cat_id"),
                    RoomId = reader.GetInt32("room_id"),
                    CheckIn = reader.GetDateTime("check_in"),
                    CheckOut = reader.GetDateTime("check_out"),
                    TotalPrice = reader.GetDecimal("total_price"),
                    Status = reader.GetString("status"),
                    CreatedAt = reader.GetDateTime("created_at")
                });
            }
            return Ok(bookings);
        }

        // POST api/hotel/bookings
        [HttpPost("bookings")]
        public IActionResult CreateBooking([FromBody] CreateHotelBookingRequest request)
        {
            int days = (request.CheckOut.Date - request.CheckIn.Date).Days;
            if (days <= 0) return BadRequest(new { message = "Check-out must be after check-in." });

            using var conn = _db.GetConnection();
            conn.Open();

            var priceCmd = new MySqlCommand("SELECT price_per_day FROM rooms WHERE id = @id", conn);
            priceCmd.Parameters.AddWithValue("@id", request.RoomId);
            var priceResult = priceCmd.ExecuteScalar();
            if (priceResult == null) return NotFound(new { message = "Room not found." });

            decimal total = (decimal)priceResult * days;

            if (request.ExtraServiceIds.Any())
            {
                string ids = string.Join(",", request.ExtraServiceIds);
                var svcCmd = new MySqlCommand($"SELECT SUM(price) FROM extra_services WHERE id IN ({ids})", conn);
                var svcResult = svcCmd.ExecuteScalar();
                if (svcResult != DBNull.Value && svcResult != null)
                    total += Convert.ToDecimal(svcResult);
            }

            var cmd = new MySqlCommand("INSERT INTO hotel_bookings (user_id, cat_id, room_id, check_in, check_out, total_price) VALUES (@userId, @catId, @roomId, @checkIn, @checkOut, @total)", conn);
            cmd.Parameters.AddWithValue("@userId", request.UserId);
            cmd.Parameters.AddWithValue("@catId", request.CatId);
            cmd.Parameters.AddWithValue("@roomId", request.RoomId);
            cmd.Parameters.AddWithValue("@checkIn", request.CheckIn.Date);
            cmd.Parameters.AddWithValue("@checkOut", request.CheckOut.Date);
            cmd.Parameters.AddWithValue("@total", total);
            cmd.ExecuteNonQuery();
            long bookingId = cmd.LastInsertedId;

            foreach (var serviceId in request.ExtraServiceIds)
            {
                var linkCmd = new MySqlCommand("INSERT INTO hotel_booking_services (booking_id, service_id) VALUES (@bId, @sId)", conn);
                linkCmd.Parameters.AddWithValue("@bId", bookingId);
                linkCmd.Parameters.AddWithValue("@sId", serviceId);
                linkCmd.ExecuteNonQuery();
            }

            return Ok(new { id = bookingId, totalPrice = total, message = "Booking confirmed." });
        }

        // PATCH api/hotel/bookings/5/status
        [HttpPatch("bookings/{id}/status")]
        public IActionResult UpdateBookingStatus(int id, [FromBody] string status)
        {
            var allowed = new[] { "confirmed", "checked_in", "checked_out", "cancelled" };
            if (!allowed.Contains(status)) return BadRequest(new { message = "Invalid status." });
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("UPDATE hotel_bookings SET status = @status WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@id", id);
            int rows = cmd.ExecuteNonQuery();
            if (rows == 0) return NotFound(new { message = "Booking not found." });
            return Ok(new { message = $"Status updated to '{status}'." });
        }

        // GET api/hotel/bookings/all
        [HttpGet("bookings/all")]
        public IActionResult GetAllBookings()
        {
            var bookings = new List<object>();
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"
                SELECT hb.*, u.first_name, u.last_name, r.room_number, c.name as cat_name
                FROM hotel_bookings hb
                JOIN users u ON hb.user_id = u.id
                JOIN rooms r ON hb.room_id = r.id
                JOIN cats c ON hb.cat_id = c.id
                ORDER BY hb.check_in DESC", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                bookings.Add(new
                {
                    id = reader.GetInt32("id"),
                    userName = $"{reader.GetString("first_name")} {reader.GetString("last_name")}",
                    catName = reader.GetString("cat_name"),
                    roomNumber = reader.GetString("room_number"),
                    checkIn = reader.GetDateTime("check_in"),
                    checkOut = reader.GetDateTime("check_out"),
                    totalPrice = reader.GetDecimal("total_price"),
                    status = reader.GetString("status")
                });
            return Ok(bookings);
        }

        // POST api/hotel/cats/5/photo
        [HttpPost("cats/{catId}/photo")]
        public async Task<IActionResult> UploadDailyPhoto(int catId, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest(new { message = "No file uploaded." });
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "daily");
            Directory.CreateDirectory(folder);
            var fileName = $"cat_{catId}_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid()}.jpg";
            var filePath = Path.Combine(folder, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            return Ok(new { message = "Photo uploaded.", fileName });
        }
    }
}
