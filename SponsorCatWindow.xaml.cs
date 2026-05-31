
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Windows;


namespace Perrrfect_stay
{

    public partial class SponsorCatWindow : Window
    {
        private User _user;
        public SponsorCatWindow(User user)
        {
            InitializeComponent();
            _user = user;
            LoadCats();
        }

        private void LoadCats()
        {
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "SELECT id, name FROM cats";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    CatCombo.Items.Add(reader["name"].ToString());
                }
            }
        }

            private void Sponsor_Click(object sender, RoutedEventArgs e)
        {
            if (CatCombo.SelectedItem == null || AmountBox.Text== "" )
            {
                MessageBox.Show("Please fill all fields!");
                return;
            }
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "INSERT INTO sponsorship_requests  (user_id, cat_name, monthly_amount) VALUES (@user, @cat, @amount)";
                MySqlCommand cmd = new MySqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@cat", CatCombo.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@user", _user.Id);
                cmd.Parameters.AddWithValue("@amount", decimal.Parse(AmountBox.Text));
                
                cmd.ExecuteNonQuery();


            }

            MessageBox.Show("Sponsorship request sent!!");
            this.Close();
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); 
        }
    }
}
