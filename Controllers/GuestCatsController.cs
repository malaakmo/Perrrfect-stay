using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PerrrfectStayAPI.Data;

namespace PerrrfectStayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GuestCatsController : ControllerBase
    {
        private readonly Database _db;
        public GuestCatsController(Database db) { _db = db; }

        // GET api/guestcats/user/2
        [HttpGet("user/{ownerId}")]
        public IActionResult GetByOwner(int ownerId)
        {
            var list = new List<object>();
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("SELECT * FROM guest_cats WHERE owner_id = @ownerId", conn);
            cmd.Parameters.AddWithValue("@ownerId", ownerId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new
                {
                    id = reader.GetInt32("id"),
                    name = reader.GetString("name"),
                    breed = reader.IsDBNull(reader.GetOrdinal("breed")) ? "" : reader.GetString("breed"),
                    diet = reader.IsDBNull(reader.GetOrdinal("diet")) ? "" : reader.GetString("diet"),
                    vetInfo = reader.IsDBNull(reader.GetOrdinal("vet_info")) ? "" : reader.GetString("vet_info"),
                    vaccineStatus = reader.GetString("vaccine_status")
                });
            return Ok(list);
        }

        // POST api/guestcats
        [HttpPost]
        public IActionResult CreateGuestCat([FromBody] GuestCatRequest request)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"INSERT INTO guest_cats (owner_id, name, breed, diet, vet_info, vaccine_status) 
                VALUES (@ownerId, @name, @breed, @diet, @vetInfo, @vaccineStatus)", conn);
            cmd.Parameters.AddWithValue("@ownerId", request.OwnerId);
            cmd.Parameters.AddWithValue("@name", request.Name);
            cmd.Parameters.AddWithValue("@breed", (object?)request.Breed ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@diet", (object?)request.Diet ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@vetInfo", (object?)request.VetInfo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@vaccineStatus", request.VaccineStatus ?? "unknown");
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Guest cat registered." });
        }

        // PUT api/guestcats/5
        [HttpPut("{id}")]
        public IActionResult UpdateGuestCat(int id, [FromBody] GuestCatRequest request)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"UPDATE guest_cats SET name=@name, breed=@breed, diet=@diet, 
                vet_info=@vetInfo, vaccine_status=@vaccineStatus WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("@name", request.Name);
            cmd.Parameters.AddWithValue("@breed", (object?)request.Breed ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@diet", (object?)request.Diet ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@vetInfo", (object?)request.VetInfo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@vaccineStatus", request.VaccineStatus ?? "unknown");
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Guest cat updated." });
        }

        // DELETE api/guestcats/5
        [HttpDelete("{id}")]
        public IActionResult DeleteGuestCat(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("DELETE FROM guest_cats WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Guest cat removed." });
        }
    }

    public class GuestCatRequest
    {
        public int OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Breed { get; set; }
        public string? Diet { get; set; }
        public string? VetInfo { get; set; }
        public string? VaccineStatus { get; set; }
    }
}