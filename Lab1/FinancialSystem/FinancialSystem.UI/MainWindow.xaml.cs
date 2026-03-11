using System;
using System.Windows;
using FinancialSystem.Application.Services;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Infrastructure;

namespace FinancialSystem.WPF
{
    public partial class MainWindow : Window
    {
        private readonly AuthService _authService;

        public MainWindow()
        {
            InitializeComponent();
            // Создаем контекст и сервис (в идеале тут нужен Dependency Injection, но для лабы так проще)
            var db = new FinanceDbContext();
            _authService = new AuthService(db);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text;
            string password = PasswordBox.Password;

            try
            {
                var user = _authService.Login(login, password);
                if (user != null)
                {
                    MessageBox.Show($"Добро пожаловать, {user.Login}!\nВаша роль: {user.Role}", "Успех");
                    
                    // Тут мы позже откроем новое окно в зависимости от роли
                    // Например: OpenDashboard(user);
                }
                else
                {
                    StatusLabel.Text = "Неверный логин или пароль.";
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = ex.Message; // Выведет "Ваша регистрация еще не подтверждена"
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                StatusLabel.Text = "Введите логин и пароль для регистрации.";
                return;
            }

            if (_authService.Register(login, password))
            {
                MessageBox.Show("Регистрация успешна! Дождитесь подтверждения менеджером.", "Инфо");
                StatusLabel.Foreground = System.Windows.Media.Brushes.Green;
                StatusLabel.Text = "Регистрация прошла успешно.";
            }
            else
            {
                StatusLabel.Foreground = System.Windows.Media.Brushes.Red;
                StatusLabel.Text = "Логин уже занят.";
            }
        }
    }
}

