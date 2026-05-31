using System.Configuration;
using System.Windows;
using MySql.Data.MySqlClient;
using System.Collections.Generic;



namespace Perrrfect_stay
{

    public partial class AdminCatsWindow : Window
    {
        public AdminCatsWindow()
        {
            InitializeComponent();
            LoadCats();
        }
        private void LoadCats()
        {

            List<Cat> cats = new List<Cat>();
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connStr))


            {
                conn.Open();
                string query = "SELECT * FROM cats";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    cats.Add(new Cat
                    {
                        Id = (int)reader["id"],
                        Name = reader["name"].ToString(),
                        Breed = reader["breed"].ToString(),
                        AdoptionStatus = reader["adoption_status"].ToString()
                    });
                }
            }
            CatsGrid.ItemsSource = cats;
        }
        private void AddCat_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Add Cat - Coming Soon!");
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {

            AdminDashboardWindow dashboard = new AdminDashboardWindow();
            dashboard.Show();
            this.Close();
        }
    }
}
