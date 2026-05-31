using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PerrrfectStayAPI;
using PerrrfectStayAPI.Data;

[Route("api/[controller]")]
[ApiController]
public class CatsController : ControllerBase
{
    private Database db = new Database();

    [HttpGet]
    public IActionResult GetCats()
    {
        List<Cat> cats = new List<Cat>();

        using (var conn = db.GetConnection())
        {
            conn.Open();

            string query = "SELECT * FROM cats";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                cats.Add(new Cat
                {
                    id = reader.GetInt32("id"),
                    name = reader.GetString("name"),
                    breed = reader.GetString("breed"),
                    age_years = reader.GetDecimal("age_years"),
                    gender = reader.GetString("gender"),
                    personality = reader.IsDBNull(reader.GetOrdinal("personality")) ? "" : reader.GetString("personality"),
                    photo = reader.IsDBNull(reader.GetOrdinal("photo")) ? "" : reader.GetString("photo"),
                    health_status = reader.GetString("health_status"),
                    adoption_status = reader.IsDBNull(reader.GetOrdinal("adoption_status")) ? "" : reader.GetString("adoption_status")
                });
            }
        }

        return Ok(cats);
    }

    // GET api/cats/suggestions/2
    [HttpGet("suggestions/{userId}")]
    public IActionResult GetSuggestions(int userId)
    {
        var cats = new List<object>();
        using var conn = db.GetConnection();
        conn.Open();
        var cmd = new MySqlCommand(@"
        SELECT DISTINCT c.* FROM cats c
        WHERE c.breed IN (
            SELECT c2.breed FROM favourites f
            JOIN cats c2 ON f.cat_id = c2.id
            WHERE f.user_id = @userId
        )
        AND c.id NOT IN (
            SELECT cat_id FROM favourites WHERE user_id = @userId
        )
        LIMIT 5", conn);
        cmd.Parameters.AddWithValue("@userId", userId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            cats.Add(new
            {
                id = reader.GetInt32("id"),
                name = reader.GetString("name"),
                breed = reader.GetString("breed"),
                personality = reader.IsDBNull(reader.GetOrdinal("personality")) ? "" : reader.GetString("personality")
            });
        return Ok(cats);
    }
}
