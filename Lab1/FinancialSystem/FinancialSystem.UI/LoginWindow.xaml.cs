using System.Windows;
using System.Windows.Controls;
using FinancialSystem.Infrastructure.Data;

namespace FinancialSystem.UI
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = (PasswordBox.Visibility == Visibility.Visible) ? PasswordBox.Password : PasswordRevealTextBox.Text;

            using (var db = new FinancialDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == password);

                if (user != null)
                {
                    if (!user.IsApproved)
                    {
                        MessageBox.Show("Ваш аккаунт еще не подтвержден администратором.");
                        return;
                    }

                    // Открываем главное окно и передаем пользователя
                    MainWindow mainWin = new MainWindow(user);
                    mainWin.Show();
                    this.Close(); // Закрываем окно входа
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль.");
                }
            }
        }


        // Этот метод вызывается при нажатии на кнопку "Регистрация"
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно регистрации
            RegisterWindow regWindow = new RegisterWindow();
            regWindow.ShowDialog(); // ShowDialog заблокирует окно входа, пока регистрация не закроется
        }

        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Visibility == Visibility.Visible)
            {
                PasswordRevealTextBox.Text = PasswordBox.Password;

                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordRevealTextBox.Visibility = Visibility.Visible;

                TogglePasswordButton.Content = "🔓";
            }
            else
            {
                PasswordBox.Password = PasswordRevealTextBox.Text;

                PasswordRevealTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;

                TogglePasswordButton.Content = "🔒";
            }
        }
    }
}

