using System.Windows;
using System.Windows.Controls;

namespace Perrrfect_stay
{
    public partial class MessagesWindow : Window
    {
        private User _user;

        public MessagesWindow(User user)
        {
            InitializeComponent();
            _user = user;
            LoadChats();
        }

        private void LoadChats()
        {
            ChatList.Items.Add("🐾 Purrfect Stay Support");
            ChatList.Items.Add("👤 Admin");
        }

        private void ChatList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatList.SelectedItem != null)
            {
                ConversationTitle.Text = ChatList.SelectedItem.ToString();
                MessagesPanel.Children.Clear();

                TextBlock msg = new TextBlock
                {
                    Text = "Hello! How can we help you? 🐱",
                    Margin = new System.Windows.Thickness(10),
                    Foreground = System.Windows.Media.Brushes.Gray,
                    TextWrapping = System.Windows.TextWrapping.Wrap
                };
                MessagesPanel.Children.Add(msg);
            }
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (MsgBox.Text == "") return;

            TextBlock msg = new TextBlock
            {
                Text = MsgBox.Text,
                Margin = new System.Windows.Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Right,
                Foreground = System.Windows.Media.Brushes.Brown,
                TextWrapping = System.Windows.TextWrapping.Wrap
            };
            MessagesPanel.Children.Add(msg);
            MsgBox.Text = "";
        }
    }
}