
using System.Windows;

using System.Windows.Input;

namespace Perrrfect_stay
{
    
    public partial class HomeWindow : Window
    {
        private User _user;
        public HomeWindow(User user)
        {
            InitializeComponent();
            _user = user;
        }
        private void CatCafe_Click(object sender, RoutedEventArgs e)
        {
            CafeReservationWindow cafe = new CafeReservationWindow();
            cafe.Show();
            this.Close();
        }
        private void AdoptCat_Click(object sender, RoutedEventArgs e)
        {
            CatListWindow cafeList = new CatListWindow(_user);
            cafeList.Show();
            this.Close();
        }

        private void CatHotel_Click(object sender, RoutedEventArgs e)
        {
            HotelBookingWindow hotel = new HotelBookingWindow();
            hotel.Show();
            this.Close();
        }
        private void Sitting_Click(object sender, RoutedEventArgs e)
        {
            SitterListWindow sitters = new SitterListWindow(_user);
            sitters.Show();
            this.Close();
        }

        private void Profile_Click(object sender, MouseButtonEventArgs e)
        {
            UserProfileWindow profile = new UserProfileWindow(_user);
            profile.Show();
            this.Close();
        }

        private void Messages_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MessagesWindow messages = new MessagesWindow(_user);
            messages.Show();
            this.Close();
        }
    }
}
