using MySql.Data.MySqlClient;
using System.Configuration;
using System.Windows;
using BCrypt.Net;

namespace Perrrfect_stay
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
          
            string email = EmailBox.Text;
            string password = PasswordBox.Password;
            

            string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();

                string query = "SELECT * FROM users WHERE email = @email";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@email", email);


                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read()) 
                {
                    string dbPassword = reader["password_hash"].ToString();
                    if (BCrypt.Net.BCrypt.Verify(password, dbPassword))
                    {
                        MessageBox.Show("Login successful!");
                        User user = new User
                        {
                            Id = (int)reader["id"],
                            FirstName = reader["first_name"].ToString(),
                            LastName = reader["last_name"].ToString(),
                            Email = reader["email"].ToString(),
                            Role = reader["role"].ToString(),

                        };

                        if (user.Role == "admin")
                        {
                            AdminDashboardWindow admin = new AdminDashboardWindow();
                            admin.Show();
                            this.Close();
                        }
                        else
                        {
                            HomeWindow home = new HomeWindow(user);
                            home.Show();
                            this.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Wrong password!");
                    }
                }
                else
                {
                    MessageBox.Show("Email not found!");
                }
            }
   
        }
        private void CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow register = new RegisterWindow();
            register.Show();
            this.Close();
        }

        private void ForgotPassword_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ForgotPasswordWindow forgot = new ForgotPasswordWindow();
            forgot.Show();
            this.Close();
        }

    }


}