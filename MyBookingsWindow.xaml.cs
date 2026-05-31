
using System.Windows;
using System.Collections.Generic;
using System.Configuration;
using MySql.Data.MySqlClient;



namespace Perrrfect_stay
{
    
    public partial class MyBookingsWindow : Window
    {

        private User _user;
        public MyBookingsWindow(User user)
        {
            InitializeComponent();
            _user = user;
            LoadBookings();
            
        }
        private void LoadBookings()
        {
            List<CafeBooking> bookings = new List<CafeBooking>();
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "SELECT * FROM cafe_reservations";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader  = cmd.ExecuteReader();
                while (reader.Read())
                {
                    bookings.Add(new CafeBooking
                    {
                        Table = reader["table_number"].ToString(),
                        TimeSlot = reader["time_slot"].ToString(),
                        NumberOfPeople = (int)reader["number_of_people"],
                        CreatedAt = reader["created_at"].ToString()
                    });
                        
                }
            }
            BookingGrid.ItemsSource = bookings;

        }
        private void Back_Click(object sender, RoutedEventArgs e )
        {
            UserProfileWindow profile = new UserProfileWindow(_user);
            profile.Show();
            this.Close();

        }
    }
}
