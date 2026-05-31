
using System.Windows;


namespace Perrrfect_stay
{
   
    public partial class AdminDashboardWindow : Window
    {
        public AdminDashboardWindow()
        {
            InitializeComponent();
        }

        private void ManageUsers_Click(object sender, RoutedEventArgs e)
        {
            AdminUsersWindow users = new AdminUsersWindow();
            users.Show();
            this.Close();


        }

        private void PendingRequests_Click(object sender, RoutedEventArgs e)
        {
            AdminRequestsWindow requests = new AdminRequestsWindow();
            requests.Show();
            this.Close();


        }

        private void ManageCats_Click(object sender, RoutedEventArgs e)
        {
            AdminCatsWindow cats = new AdminCatsWindow();
            cats.Show();
            this.Close();


        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();


        }
    }
}
