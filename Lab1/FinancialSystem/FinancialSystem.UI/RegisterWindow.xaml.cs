using System.Windows;
using FinancialSystem.Application.Interfaces;

namespace FinancialSystem.UI
{
    public partial class RegisterWindow : Window
    {
        private readonly IAuthService _authService;

        public RegisterWindow(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        private void RegisterConfirm_Click(object sender, RoutedEventArgs e)
        {
            string login = RegLoginBox.Text.Trim();
            string pass = RegPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) || pass.Length < 4)
            {
                CustomMessageBox.Show("Логин не может быть пустым, а пароль должен содержать минимум 4 символа.",
                    "Ошибка валидации", this);
                return;
            }

            if (_authService.Register(login, pass))
            {
                CustomMessageBox.Show(
                    "Заявка на регистрацию успешно отправлена менеджерам на подтверждение!",
                    "Успех",
                    this
                );
                this.Close();
            }
            else
            {
                CustomMessageBox.Show("Этот логин уже занят. Пожалуйста, попробуйте другой вариант.",
                    "Внимание", this);
            }
        }

        private void BtnShowPass_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RegPasswordVisibleBox.Text = RegPasswordBox.Password;
            RegPasswordBox.Visibility = Visibility.Collapsed;
            RegPasswordVisibleBox.Visibility = Visibility.Visible;
        }

        private void BtnShowPass_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RegPasswordVisibleBox.Visibility = Visibility.Collapsed;
            RegPasswordBox.Visibility = Visibility.Visible;
            RegPasswordBox.Focus();
        }

        private void Back_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
