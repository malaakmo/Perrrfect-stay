
using System.Windows;



namespace Perrrfect_stay
{
   
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string firstName = FirstNameBox.Text;
            string lastName = LastNameBox.Text;
            string email = EmailBox.Text;
            string password = PasswordBox.Password;

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            User user = new User();
            user.FirstName = firstName;
            user.LastName = lastName;
            user.Email = email;
            user.PasswordHash = hashedPassword;

            DatabaseHelper db = new DatabaseHelper();
            db.AddUser(user);

            MessageBox.Show("Account created!");

            MainWindow login = new MainWindow();
            login.Show();
            this.Close();

        }
    }
}
