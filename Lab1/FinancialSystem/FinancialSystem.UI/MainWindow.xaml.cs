using System.Windows;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;

namespace FinancialSystem.UI
{
    public partial class MainWindow : Window
    {
        private User _currentUser;

        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            SetupInterface();
        }

        private void SetupInterface()
        {
            WelcomeLabel.Text = $"Добро пожаловать, {_currentUser.Username}!";
            RoleLabel.Text = $"Роль в системе: {_currentUser.Role}";

            // Настройка видимости в зависимости от роли
            if (_currentUser.Role == UserRole.Administrator || _currentUser.Role == UserRole.Manager)
            {
                AdminPanel.Visibility = Visibility.Visible;
            }

            if (_currentUser.Role == UserRole.Client)
            {
                ClientPanel.Visibility = Visibility.Visible;
            }
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Вы на главной странице дашборда");
        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            // Здесь позже откроем список пользователей
            MessageBox.Show("Раздел управления пользователями");
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWin = new LoginWindow();
            loginWin.Show();
            this.Close();
        }
    }
}
