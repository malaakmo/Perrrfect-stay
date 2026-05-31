
using System.Windows;

namespace Perrrfect_stay
{
   
    public partial class CatDetailsWindow : Window
    {
        private Cat _cat;
        private User _user;
        public CatDetailsWindow(Cat cat, User user)
        {
            InitializeComponent();
            _cat = cat;
            _user = user;
            LoadCatDetails();
        }
        private void LoadCatDetails()
        {
            CatName.Text = _cat.Name;
            CatBreed.Text = _cat.Breed;
            CatStatus.Text = "Status: " + _cat.AdoptionStatus;
            CatPersonality.Text = _cat.Personality;
        }

        private void Adopt_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Adoption request sent for  " + _cat.Name + "!");

        }

        private void Sponsor_Click(object sender, RoutedEventArgs e)
        {
            SponsorCatWindow sponsor = new SponsorCatWindow(_user);
            sponsor.Show();
        }
    }
}
