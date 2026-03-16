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
            StatusLabel.Text = "";
            string login = LoginBox.Text.Trim(); // без учета пробелов
            string password = PasswordBox.Password;

            try
            {
                var user = _authService.Login(login, password);

                if (user != null)
                {
                    new DashboardWindow(user).Show();
                    this.Close();
                }
                else
                {
                    StatusLabel.Foreground = Brushes.Red;
                    StatusLabel.Text = "Неверный логин или пароль.";
                }
            }
            catch (InvalidOperationException ex) when (ex.Message == "NOT_APPROVED")
            {
                StatusLabel.Foreground = Brushes.Orange; 
                StatusLabel.Text = "Аккаунт еще не подтвержден менеджером.";
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Ошибка системы: " + ex.Message, "Ошибка", this);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow regWindow = new RegisterWindow(_authService);
            regWindow.Owner = this; 
            regWindow.ShowDialog();
        }

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

        private void BtnShowPass_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            PasswordVisibleBox.Visibility = Visibility.Collapsed;
            PasswordBox.Visibility = Visibility.Visible;
        }
    }
}
