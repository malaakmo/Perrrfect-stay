using MySql.Data.MySqlClient;
using System.Configuration;

namespace Perrrfect_stay
{
    public class DatabaseHelper
    {
        private string connectionString = ConfigurationManager
            .ConnectionStrings["MySqlConnection"].ConnectionString;

        public void AddUser(User user)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO users (first_name, last_name, email, password_hash, role) " + "VALUES (@firstName, @lastName, @email, @password, @role)";

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@firstName", user.FirstName);
                cmd.Parameters.AddWithValue("@lastName", user.LastName);
                cmd.Parameters.AddWithValue("@email", user.Email);
                cmd.Parameters.AddWithValue("@password", user.PasswordHash);
                cmd.Parameters.AddWithValue("@role", user.Role);

                cmd.ExecuteNonQuery();
            }
        }
    }
}

