using System.Configuration;
using System.Windows;
using MySql.Data.MySqlClient;


namespace Perrrfect_stay
{
   
    public partial class CafeReservationWindow : Window
    {
        public CafeReservationWindow()
        {
            InitializeComponent();
            LoadTables();
            LoadTimeSlots();
        }
        private void LoadTables()
        {
            TableCombo.Items.Add("Table 1");
            TableCombo.Items.Add("Table 2");
            TableCombo.Items.Add("Table 3");
            TableCombo.Items.Add("Table 4");
            TableCombo.Items.Add("Table 5");

        }
        private void LoadTimeSlots()
        {
            TimeSlotCombo.Items.Add("10:00 - 11:00");
            TimeSlotCombo.Items.Add("11:00 - 12:00");
            TimeSlotCombo.Items.Add("12:00 - 13:00");
            TimeSlotCombo.Items.Add("13:00 - 14:00");
            TimeSlotCombo.Items.Add("14:00 - 15:00");

        }
        private void ReserveTable_Click(object sender, RoutedEventArgs e)
        {
            if (TableCombo.SelectedItem == null || TimeSlotCombo.SelectedItem == null || PeopleBox.Text == "")
            {
                MessageBox.Show("Please fill all fields!");
                return;
            }


            string table = TableCombo.SelectedItem.ToString();
            string timeSlot = TimeSlotCombo.SelectedItem.ToString();
            int people = int.Parse(PeopleBox.Text);
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connStr))


            {
                conn.Open();
                string query = "INSERT INTO cafe_reservations (table_number, time_slot, number_of_people) VALUES (@table, @timeSlot, @people)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@table", table);
                cmd.Parameters.AddWithValue("@timeSlot", timeSlot);
                cmd.Parameters.AddWithValue("@people", people);

                cmd.ExecuteNonQuery();
            }
            MessageBox.Show("Reservation confirmed!");
            this.Close();
        }
    }
}
