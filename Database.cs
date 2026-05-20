using MySql.Data.MySqlClient;

namespace PurrfectStayAPI
{
    public class Database
    {
        private string connectionString = "Server=127.0.0.1;Database=perfect_stay;Uid=root;Pwd=;SslMode=None";

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}
