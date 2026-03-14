using System;
using System.Windows;
using System.Windows.Media;
using FinancialSystem.Application.Interfaces;
using FinancialSystem.Application.Services;
using FinancialSystem.Infrastructure;

namespace FinancialSystem.UI
{
    public partial class MainWindow : Window
    {
        private readonly IAuthService _authService;

        public MainWindow()
        {
            InitializeComponent();
            var db = new FinanceDbContext();
            DbInitializer.Initialize(db);
            _authService = new AuthService(db);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            StatusLabel.Text = ""; // Сброс текста
            string login = LoginBox.Text;
            string password = PasswordBox.Password;

            try
            {
                var user = _authService.Login(login, password);
                if (user != null)
                {
                    DashboardWindow dashboard = new DashboardWindow(user);
                    dashboard.Show();
                    this.Close();
                }
                else
                {
                    StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60)); // Красный E74C3C
                    StatusLabel.Text = "Неверный логин или пароль.";
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60));
                StatusLabel.Text = ex.Message;
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60));
                StatusLabel.Text = "Введите логин и пароль для регистрации.";
                return;
            }

            if (_authService.Register(login, password))
            {
                MessageBox.Show("Регистрация успешна! Дождитесь подтверждения менеджером.", "Успех");
                StatusLabel.Foreground = (SolidColorBrush)System.Windows.Application.Current.Resources["EmeraldAccent"];
                StatusLabel.Text = "Регистрация прошла успешно. Ожидайте активации.";
            }
            else
            {
                StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60));
                StatusLabel.Text = "Логин уже занят.";
            }
        }
    }
}
