using System;
using System.Windows;

namespace SteelInventory
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (LoginTextBox.Text == "Admin" && PasswordBox.Password == "")
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid login or password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
