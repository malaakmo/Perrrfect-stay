using System.Configuration;
using System.Windows;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace Perrrfect_stay
{
    
    public partial class AdminRequestsWindow : Window
    {
        public AdminRequestsWindow()
        {
            InitializeComponent();
            LoadRequests();
        }
        private void LoadRequests()
        {
            List<AdoptionRequest> requests = new List<AdoptionRequest>();
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connStr))


            {
                conn.Open();
                string query = "SELECT * FROM adoption_requests WHERE status = 'pending'";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    requests.Add(new AdoptionRequest
                    {
                        Id = (int)reader["id"],
                        UserId = (int)reader["user_id"],
                        CatId = (int)reader["cat_id"],
                        Status = reader["status"].ToString(),
                        RequestDate = reader["request_date"].ToString()
                    });


                }

            }
            RequestGrid.ItemsSource = requests;
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            AdminDashboardWindow dashboard = new AdminDashboardWindow();
            dashboard.Show();
            this.Close();
        }
    }
}
