using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;
using FinancialSystem.Infrastructure.Data;

namespace FinancialSystem.UI
{
    public partial class RegisterWindow : Window
    {
        private bool isUsernameValid = false;
        private bool isPasswordValid = false;

        public RegisterWindow()
        {
            InitializeComponent();
        }

        // 1. Динамическая проверка логина (занят или свободен)
        private void RegUsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string username = RegUsernameTextBox.Text.Trim();

            if (username.Length < 3)
            {
                UsernameStatusLabel.Text = "Слишком короткий логин";
                UsernameStatusLabel.Foreground = Brushes.Red;
                isUsernameValid = false;
            }
            else
            {
                using (var db = new FinancialDbContext())
                {
                    bool exists = db.Users.Any(u => u.Username == username);
                    if (exists)
                    {
                        UsernameStatusLabel.Text = "❌ Логин уже занят";
                        UsernameStatusLabel.Foreground = Brushes.Red;
                        isUsernameValid = false;
                    }
                    else
                    {
                        UsernameStatusLabel.Text = "✅ Логин свободен";
                        UsernameStatusLabel.Foreground = Brushes.Green;
                        isUsernameValid = true;
                    }
                }
            }
            ValidateForm();
        }

        // 2. Динамическая проверка пароля (длина)
        private void RegPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ValidatePassword(RegPasswordBox.Password);
        }

        private void RegPasswordRevealTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidatePassword(RegPasswordRevealTextBox.Text);
        }

        private void ValidatePassword(string password)
        {
            if (password.Length >= 6)
            {
                PasswordStatusLabel.Text = "✅ Пароль подходит";
                PasswordStatusLabel.Foreground = Brushes.Green;
                isPasswordValid = true;
            }
            else
            {
                PasswordStatusLabel.Text = "❌ Минимум 6 символов";
                PasswordStatusLabel.Foreground = Brushes.Red;
                isPasswordValid = false;
            }
            ValidateForm();
        }

        // 3. Общая проверка формы (активирует кнопку)
        private void ValidateForm()
        {
            RegisterConfirmButton.IsEnabled = isUsernameValid && isPasswordValid;
        }

        // Логика "Глазика"
        private void ToggleRegPassword_Click(object sender, RoutedEventArgs e)
        {
            if (RegPasswordBox.Visibility == Visibility.Visible)
            {
                RegPasswordRevealTextBox.Text = RegPasswordBox.Password;
                RegPasswordBox.Visibility = Visibility.Collapsed;
                RegPasswordRevealTextBox.Visibility = Visibility.Visible;
                ToggleRegPasswordButton.Content = "🔓";
            }
            else
            {
                RegPasswordBox.Password = RegPasswordRevealTextBox.Text;
                RegPasswordRevealTextBox.Visibility = Visibility.Collapsed;
                RegPasswordBox.Visibility = Visibility.Visible;
                ToggleRegPasswordButton.Content = "🔒";
            }
        }

        private void RegisterConfirm_Click(object sender, RoutedEventArgs e)
        {
            string username = RegUsernameTextBox.Text.Trim();
            string password = (RegPasswordBox.Visibility == Visibility.Visible) ? RegPasswordBox.Password : RegPasswordRevealTextBox.Text;

            try
            {
                using (var db = new FinancialDbContext())
                {
                    var selectedRoleItem = (ComboBoxItem)RoleComboBox.SelectedItem;
                    UserRole role = (UserRole)Enum.Parse(typeof(UserRole), selectedRoleItem.Content.ToString());

                    db.Users.Add(new User
                    {
                        Username = username,
                        PasswordHash = password,
                        Role = role,
                        IsApproved = (role != UserRole.Client)
                    });
                    db.SaveChanges();
                }
                MessageBox.Show("Регистрация успешна!");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}


