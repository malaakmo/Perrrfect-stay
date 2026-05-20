using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PurrfectStayAPI.Data;
using PurrfectStayAPI.Models;

namespace PurrfectStayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CafeController : ControllerBase
    {
        private readonly Database _db;
        public CafeController(Database db) { _db = db; }

        // GET api/cafe/tables
        [HttpGet("tables")]
        public IActionResult GetTables()
        {
            var tables = new List<CafeTable>();
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("SELECT * FROM cafe_tables ORDER BY number", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tables.Add(new CafeTable { Id = reader.GetInt32("id"), Number = reader.GetInt32("number"), Capacity = reader.GetInt32("capacity") });
            }
            return Ok(tables);
        }

        // GET api/cafe/tables/available?date=2025-06-01&timeSlot=14:00&guests=2
        [HttpGet("tables/available")]
        public IActionResult GetAvailableTables([FromQuery] DateTime date, [FromQuery] string timeSlot, [FromQuery] int guests = 1)
        {
            var tables = new List<CafeTable>();
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"SELECT * FROM cafe_tables WHERE capacity >= @guests
                AND id NOT IN (SELECT table_id FROM cafe_reservations
                WHERE visit_date = @date AND time_slot = @timeSlot AND status = 'confirmed')
                ORDER BY number", conn);
            cmd.Parameters.AddWithValue("@guests", guests);
            cmd.Parameters.AddWithValue("@date", date.Date);
            cmd.Parameters.AddWithValue("@timeSlot", timeSlot);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tables.Add(new CafeTable { Id = reader.GetInt32("id"), Number = reader.GetInt32("number"), Capacity = reader.GetInt32("capacity") });
            }
            return Ok(tables);
        }

        // GET api/cafe/reservations/user/any
        [HttpGet("reservations/user/{userId}")]
        public IActionResult GetReservationsByUser(int userId)
        {
            var reservations = new List<CafeReservation>();
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("SELECT * FROM cafe_reservations WHERE user_id = @userId ORDER BY visit_date DESC", conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                reservations.Add(new CafeReservation
                {
                    Id = reader.GetInt32("id"),
                    UserId = reader.GetInt32("user_id"),
                    TableId = reader.GetInt32("table_id"),
                    VisitDate = reader.GetDateTime("visit_date"),
                    TimeSlot = reader.GetString("time_slot"),
                    NumGuests = reader.GetInt32("num_guests"),
                    Status = reader.GetString("status"),
                    CreatedAt = reader.GetDateTime("created_at")
                });
            }
            return Ok(reservations);
        }

        // POST api/cafe/reservations
        [HttpPost("reservations")]
        public IActionResult CreateReservation([FromBody] CreateCafeReservationRequest request)
        {
            if (request.NumGuests < 1) return BadRequest(new { message = "At least 1 guest required." });
            using var conn = _db.GetConnection();
            conn.Open();

            var tableCmd = new MySqlCommand(@"SELECT id FROM cafe_tables WHERE capacity >= @guests
                AND id NOT IN (SELECT table_id FROM cafe_reservations
                WHERE visit_date = @date AND time_slot = @timeSlot AND status = 'confirmed')
                ORDER BY capacity ASC LIMIT 1", conn);
            tableCmd.Parameters.AddWithValue("@guests", request.NumGuests);
            tableCmd.Parameters.AddWithValue("@date", request.VisitDate.Date);
            tableCmd.Parameters.AddWithValue("@timeSlot", request.TimeSlot);
            var tableResult = tableCmd.ExecuteScalar();
            if (tableResult == null) return BadRequest(new { message = "No tables available for that date and time." });

            int tableId = Convert.ToInt32(tableResult);
            var cmd = new MySqlCommand("INSERT INTO cafe_reservations (user_id, table_id, visit_date, time_slot, num_guests) VALUES (@userId, @tableId, @date, @timeSlot, @guests)", conn);
            cmd.Parameters.AddWithValue("@userId", request.UserId);
            cmd.Parameters.AddWithValue("@tableId", tableId);
            cmd.Parameters.AddWithValue("@date", request.VisitDate.Date);
            cmd.Parameters.AddWithValue("@timeSlot", request.TimeSlot);
            cmd.Parameters.AddWithValue("@guests", request.NumGuests);
            cmd.ExecuteNonQuery();
            return Ok(new { tableId, message = "Reservation confirmed." });
        }

        // PATCH api/cafe/reservations/5/cancel
        [HttpPatch("reservations/{id}/cancel")]
        public IActionResult CancelReservation(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("UPDATE cafe_reservations SET status = 'cancelled' WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            int rows = cmd.ExecuteNonQuery();
            if (rows == 0) return NotFound(new { message = "Reservation not found." });
            return Ok(new { message = "Reservation cancelled." });
        }
    }
}