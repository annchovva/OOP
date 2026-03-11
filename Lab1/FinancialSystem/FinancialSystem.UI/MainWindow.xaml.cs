using System;
using System.Windows;
using FinancialSystem.Application.Interfaces; // Добавили интерфейсы
using FinancialSystem.Application.Services;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Interfaces;
using FinancialSystem.Infrastructure;

namespace FinancialSystem.UI
{
    public partial class MainWindow : Window
    {
        // Используем интерфейс вместо конкретного класса
        private readonly IAuthService _authService;

        public MainWindow()
        {
            InitializeComponent();

            // Создаем контекст БД
            var db = new FinanceDbContext();
            DbInitializer.Initialize(db);

            // Инициализируем сервис через интерфейс
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
                    // Передаем объект пользователя в следующее окно
                    DashboardWindow dashboard = new DashboardWindow(user);
                    dashboard.Show();

                    this.Close();
                }
                else
                {
                    StatusLabel.Text = "Неверный логин или пароль.";
                }
            }
            catch (Exception ex)
            {
                // Сообщение об ошибке (например, если аккаунт не подтвержден)
                StatusLabel.Text = ex.Message;
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


