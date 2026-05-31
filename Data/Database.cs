using MySql.Data.MySqlClient;

namespace PerrrfectStayAPI.Data
{
    public class Database
    {
        private string _connectionString = "Server=localhost;Port=3308;Database=perrrfect_stay;Uid=root;Pwd=root;";

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
