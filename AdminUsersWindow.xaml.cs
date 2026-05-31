using System.Configuration;
using System.Windows;
using MySql.Data.MySqlClient;
using System.Collections.Generic;


namespace Perrrfect_stay
{

    public partial class AdminUsersWindow : Window
    {
        public AdminUsersWindow()
        {
            InitializeComponent();
            LoadUsers();
        }
        private void LoadUsers()
        {
            List<User> users = new List<User>();
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connStr))


            {
                conn.Open();
                string query = "SELECT * FROM users WHERE is_deleted = 0";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    users.Add(new User
                    {
                        Id = (int)reader["id"],
                        FirstName = reader["first_name"].ToString(),
                        LastName = reader["last_name"].ToString(),
                        Email = reader["email"].ToString(),
                        Role = reader["role"].ToString()
                    });


                }

            }
            UserGrid.ItemsSource = users;
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            AdminDashboardWindow dashboard = new AdminDashboardWindow();
            dashboard.Show();
            this.Close();
        }
    }
}
