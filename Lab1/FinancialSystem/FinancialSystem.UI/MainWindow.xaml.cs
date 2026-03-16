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

            // Инициализация БД и сервисов
            var db = new FinanceDbContext();
            DbInitializer.Initialize(db);
            _authService = new AuthService(db);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            StatusLabel.Text = "";
            string login = LoginBox.Text.Trim(); // Добавил Trim для чистоты данных
            string password = PasswordBox.Password;

            try
            {
                var user = _authService.Login(login, password);

                if (user != null)
                {
                    // Переход в личный кабинет
                    DashboardWindow dashboard = new DashboardWindow(user);
                    dashboard.Show();
                    this.Close();
                }
                else
                {
                    // Ошибка ввода (выводим текст в StatusLabel)
                    StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60));
                    StatusLabel.Text = "Неверный логин или пароль.";
                }
            }
            catch (Exception ex)
            {
                // Техническая ошибка (выводим через CustomMessageBox)
                CustomMessageBox.Show("Ошибка при попытке входа: " + ex.Message, "Системная ошибка", this);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow regWindow = new RegisterWindow(_authService);
            regWindow.Owner = this; // Чтобы окно регистрации открылось ровно по центру этого окна
            regWindow.ShowDialog();
        }

        // Логика "глазика" для показа пароля
        private void BtnShowPass_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PasswordVisibleBox.Text = PasswordBox.Password;
            PasswordBox.Visibility = Visibility.Collapsed;
            PasswordVisibleBox.Visibility = Visibility.Visible;
        }

        private void BtnShowPass_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PasswordVisibleBox.Visibility = Visibility.Collapsed;
            PasswordBox.Visibility = Visibility.Visible;
            PasswordBox.Focus();
        }

        // Полезное дополнение: если мышка ушла с кнопки, пароль должен снова скрыться
        private void BtnShowPass_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            PasswordVisibleBox.Visibility = Visibility.Collapsed;
            PasswordBox.Visibility = Visibility.Visible;
        }
    }
}
