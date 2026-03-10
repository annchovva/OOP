using System.Windows;
using System.Windows.Controls;
using FinancialSystem.Domain.Enums;
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
            // Получаем данные из текстовых полей (проверь, чтобы названия TextBox совпадали с твоими в XAML)
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password; // Если используешь стандартный PasswordBox

            try
            {
                using (var db = new FinancialDbContext())
                {
                    // 1. Ищем пользователя в базе данных (обрати внимание, я использую PasswordHash, как у тебя в регистрации)
                    var user = db.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == password);

                    // 2. Если пользователь не найден
                    if (user == null)
                    {
                        MessageBox.Show("Неверный логин или пароль!");
                        return; // Прерываем выполнение
                    }

                    // 3. Если пользователь найден, проверяем, подтвержден ли он
                    if (!user.IsApproved)
                    {
                        MessageBox.Show("Ваш аккаунт еще не подтвержден менеджером. Пожалуйста, ожидайте.");
                        return; // Прерываем выполнение
                    }

                    // 4. Если всё отлично, проверяем роль и открываем нужное окно
                    if (user.Role == UserRole.Client)
                    {
                        // Открываем окно клиента и передаем туда данные пользователя
                        ClientWindow clientWindow = new ClientWindow(user);
                        clientWindow.Show();

                        // Закрываем окно авторизации
                        this.Close();
                    }
                    else if (user.Role == UserRole.Manager)
                    {
                        // Тут в будущем будет открытие окна менеджера
                        // new ManagerWindow(user).Show();
                        // this.Close();
                        MessageBox.Show("Окно менеджера еще в разработке.");
                    }
                    else if (user.Role == UserRole.Administrator)
                    {
                        // Тут в будущем будет открытие окна админа
                        MessageBox.Show("Окно администратора еще в разработке.");
                    }
                    // ... (другие роли, если есть)
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе: {ex.Message}");
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

