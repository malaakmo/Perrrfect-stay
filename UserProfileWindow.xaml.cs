
using System.Windows;
using MySql.Data.MySqlClient;
using System.Configuration;



namespace Perrrfect_stay
{
    
    public partial class UserProfileWindow : Window
    {
        private User _user;
        public UserProfileWindow(User user)
        {
            InitializeComponent();
            _user = user;
            LoadProfile();
        }
        private void LoadProfile()
        {
            NameText.Text = _user.FirstName + " " + _user.LastName;
            EmailText.Text = _user.Email;

        }
        private void MyBookings_Click(object sender, RoutedEventArgs e)
        {
            MyBookingsWindow booking = new MyBookingsWindow(_user);
            booking.Show();
            this.Close();
        }
        private void MyFavorites_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("My Favorites - ComingSoon!");
            

        }
        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            ForgotPasswordWindow forgot = new ForgotPasswordWindow();
            forgot.Show();


        }
        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure?", "Delete Account", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            
            {
                string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = "UPDATE users SET is_deleted = 1 WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", _user.Id);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Account deleted!");
                MainWindow login = new MainWindow();
                login.Show();
                this.Close();


            }

        }
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
            

        }

    }
}
