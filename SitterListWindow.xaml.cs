using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Configuration;

using System.Windows;

namespace Perrrfect_stay
{
    public partial class SitterListWindow : Window
    {
        private User _user;

        public SitterListWindow(User user)
        {
            InitializeComponent();
            _user = user;
            LoadSitters();
        }

        private void LoadSitters()
        {
            List<Sitter> sitters = new List<Sitter>();
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "SELECT * FROM sitters";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    sitters.Add(new Sitter
                    {
                        Id = (int)reader["id"],
                        Name = reader["name"].ToString(),
                        Location = reader["location"].ToString(),
                        PricePerDay = reader["price_per_day"].ToString(),
                        Bio = reader["bio"].ToString()
                    });
                }
            }
            SitterList.ItemsSource = sitters;
        }

        private void ViewProfile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            Sitter sitter = btn.DataContext as Sitter;
            SitterProfileWindow profile = new SitterProfileWindow(sitter, _user);
            profile.Show();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            HomeWindow home = new HomeWindow(_user);
            home.Show();
            this.Close();
        }
    }
}