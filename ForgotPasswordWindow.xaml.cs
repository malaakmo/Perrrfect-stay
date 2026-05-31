using BCrypt.Net;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Windows;


namespace Perrrfect_stay
{

    public partial class ForgotPasswordWindow : Window
    {
        public ForgotPasswordWindow()
        {
            InitializeComponent();
        }
        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailBox.Text;
            if (email == "")
            {
                MessageBox.Show("Please enter your email!");
                return;
            }
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();

                string checkQuery = "SELECT id FROM users WHERE email = @email";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@email", email);
                object result = checkCmd.ExecuteScalar();

                if (result == null)
                {
                    MessageBox.Show("Email not found!");
                    return;
                }

                string newPassword = BCrypt.Net.BCrypt.HashPassword("1234");
                string updateQuery = "UPDATE users SET password_hash = @password WHERE email = @email";
                MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn);
                updateCmd.Parameters.AddWithValue("@password", newPassword);
                updateCmd.Parameters.AddWithValue("@email", email);
                updateCmd.ExecuteNonQuery();

            }
            MessageBox.Show("Password reset to '1234' ! Please login and change it.");
            this.Close();
        }
    }
}
