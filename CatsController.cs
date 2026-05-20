using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PurrfectStayAPI;

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
                    age_years = reader.GetInt32("age_years"),
                    gender = reader.GetString("gender"),
                    personality = reader.GetString("personality"),
                    photo = reader.IsDBNull(reader.GetOrdinal("photo")) ? "" : reader.GetString("photo"),
                    health_status = reader.GetString("health_status"),
                    adoption_status = reader.GetString("adoption_status")
                });
            }
        }

        return Ok(cats);
    }
}