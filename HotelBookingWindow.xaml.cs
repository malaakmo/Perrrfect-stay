using MySql.Data.MySqlClient;
using System.Configuration;
using System.Windows;


namespace Perrrfect_stay
{

    public partial class HotelBookingWindow : Window
    {
        public HotelBookingWindow()
        {
            InitializeComponent();
            LoadCats();
            LoadRoomTypes();
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
        private void LoadRoomTypes()
        {
            TypeCombo.Items.Add("Standard Room");
            TypeCombo.Items.Add("VIP Suite");

        }
        private void ConfirmBooking_Click(object sender, RoutedEventArgs e)
        {
            if (CatCombo.SelectedItem == null || TypeCombo.SelectedItem == null || StartDate.SelectedDate == null || EndDate.SelectedDate == null)
            {
                MessageBox.Show("Please fill all fields!");
                return;
            }
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "INSERT INTO hotel_bookings (cat_name, room_type, end_date, special_note) VALUES (@cat, @room, @start, @end, @notes)";
                MySqlCommand cmd = new MySqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@cat", CatCombo.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@room", TypeCombo.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@pstart", StartDate.SelectedDate.Value);
                cmd.Parameters.AddWithValue("@end", EndDate.SelectedDate.Value);
                cmd.Parameters.AddWithValue("@note", NotesBox.Text);
                cmd.ExecuteNonQuery();


            }

            MessageBox.Show("Booking confirmed!");
            this.Close();


        }
    }
}
