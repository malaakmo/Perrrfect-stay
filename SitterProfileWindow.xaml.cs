using System.Windows;

namespace Perrrfect_stay
{
    public partial class SitterProfileWindow : Window
    {
        private Sitter _sitter;
        private User _user;

        public SitterProfileWindow(Sitter sitter, User user)
        {
            InitializeComponent();
            _sitter = sitter;
            _user = user;
            LoadProfile();
        }

        private void LoadProfile()
        {
            SitterName.Text = _sitter.Name;
            SitterPrice.Text = "€" + _sitter.PricePerDay + " / day";
            SitterLocation.Text = "📍 " + _sitter.Location;
            SitterBio.Text = _sitter.Bio;
        }

        private void BookSitter_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Booking request sent for " + _sitter.Name + "!");
        }
    }
}