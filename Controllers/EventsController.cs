using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PerrrfectStayAPI.Data;

namespace PerrrfectStayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly Database _db;
        public EventsController(Database db) { _db = db; }

        // GET api/events
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = new List<object>();
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("SELECT * FROM events ORDER BY event_date ASC", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new
                {
                    id = reader.GetInt32("id"),
                    title = reader.GetString("title"),
                    description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description"),
                    eventDate = reader.GetDateTime("event_date"),
                    capacity = reader.GetInt32("capacity"),
                    theme = reader.IsDBNull(reader.GetOrdinal("theme")) ? "" : reader.GetString("theme")
                });
            return Ok(list);
        }

        // POST api/events
        [HttpPost]
        public IActionResult CreateEvent([FromBody] EventRequest request)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("INSERT INTO events (title, description, event_date, capacity, theme) VALUES (@title, @desc, @date, @cap, @theme)", conn);
            cmd.Parameters.AddWithValue("@title", request.Title);
            cmd.Parameters.AddWithValue("@desc", (object?)request.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@date", request.EventDate);
            cmd.Parameters.AddWithValue("@cap", request.Capacity);
            cmd.Parameters.AddWithValue("@theme", (object?)request.Theme ?? DBNull.Value);
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Event created." });
        }

        // PUT api/events/5
        [HttpPut("{id}")]
        public IActionResult UpdateEvent(int id, [FromBody] EventRequest request)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("UPDATE events SET title=@title, description=@desc, event_date=@date, capacity=@cap, theme=@theme WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("@title", request.Title);
            cmd.Parameters.AddWithValue("@desc", (object?)request.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@date", request.EventDate);
            cmd.Parameters.AddWithValue("@cap", request.Capacity);
            cmd.Parameters.AddWithValue("@theme", (object?)request.Theme ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Event updated." });
        }

        // DELETE api/events/5
        [HttpDelete("{id}")]
        public IActionResult DeleteEvent(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("DELETE FROM events WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Event deleted." });
        }

        // POST api/events/5/register
        [HttpPost("{id}/register")]
        public IActionResult Register(int id, [FromBody] int userId)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("INSERT IGNORE INTO event_registrations (user_id, event_id) VALUES (@userId, @eventId)", conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@eventId", id);
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Registered for event." });
        }

        // DELETE api/events/5/register/2
        [HttpDelete("{id}/register/{userId}")]
        public IActionResult CancelRegistration(int id, int userId)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("DELETE FROM event_registrations WHERE user_id=@userId AND event_id=@eventId", conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@eventId", id);
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Registration cancelled." });
        }

        // GET api/events/5/attendees
        [HttpGet("{id}/attendees")]
        public IActionResult GetAttendees(int id)
        {
            var list = new List<object>();
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"
                SELECT u.id, u.first_name, u.last_name, u.email, er.registered_at
                FROM event_registrations er
                JOIN users u ON er.user_id = u.id
                WHERE er.event_id = @eventId", conn);
            cmd.Parameters.AddWithValue("@eventId", id);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new
                {
                    userId = reader.GetInt32("id"),
                    name = $"{reader.GetString("first_name")} {reader.GetString("last_name")}",
                    email = reader.GetString("email"),
                    registeredAt = reader.GetDateTime("registered_at")
                });
            return Ok(list);
        }
    }

    public class EventRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public int Capacity { get; set; } = 20;
        public string? Theme { get; set; }
    }
}