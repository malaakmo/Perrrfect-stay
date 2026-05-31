using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Configuration;
using System.Windows.Controls;

namespace Perrrfect_stay
{
   
    public partial class CatListWindow : Window
    {
        private User _user;
        public CatListWindow(User user)
        {
            InitializeComponent();
            _user = user;
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
                        Breed = reader["breed"].ToString()
                    });

                }

                  
            }
            CatList.ItemsSource = cats;
        }


        private void Details_Click(object sender, RoutedEventArgs e)
            {
            Button btn = sender as Button;
            Cat cat = btn.DataContext as Cat;

            CatDetailsWindow details = new CatDetailsWindow(cat, _user);
            details.Show();
        
         } 
    }
}
