using System;
using System.Linq;
using System.Windows;
using FinancialSystem.Application.Interfaces;
using FinancialSystem.Application.Services;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;
using FinancialSystem.Infrastructure;

namespace FinancialSystem.UI // Проверь, чтобы это совпадало с x:Class в XAML
{
    public partial class DashboardWindow : Window
    {
        private readonly User _user;
        private readonly IBankService _bankService;

        public DashboardWindow(User user)
        {
            InitializeComponent();
            _user = user;

            // Инициализация сервиса
            var db = new FinanceDbContext();
            _bankService = new BankService(db);

            // Настройка текста
            UserInfoLabel.Text = $"Пользователь: {_user.Login}";
            UserStatusText.Text = $"Ваша роль: {_user.Role}";

            // Показываем кнопку админа, если роль позволяет
            if (_user.Role == UserRole.Admin || _user.Role == UserRole.Manager)
            {
                AdminBtn.Visibility = Visibility.Visible;
            }
        }

        private void RefreshData()
        {
            try
            {
                var accounts = _bankService.GetUserAccounts(_user.Id);
                AccountsList.ItemsSource = accounts;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении данных: " + ex.Message);
            }
        }

        private void AccountsBtn_Click(object sender, RoutedEventArgs e)
        {
            WelcomeArea.Visibility = Visibility.Collapsed;
            AccountsPanel.Visibility = Visibility.Visible;
            RefreshData();
        }

        private void OpenAccountBtn_Click(object sender, RoutedEventArgs e)
        {
            var banks = _bankService.GetAllBanks();
            if (banks.Count == 0)
            {
                MessageBox.Show("Сначала добавьте банки в базу данных!");
                return;
            }

            // Открываем счет в первом банке для теста
            _bankService.OpenAccount(_user.Id, banks[0].Id, AccountType.Current);
            RefreshData();
            MessageBox.Show("Счет успешно открыт!");
        }

        private void ViewBanksBtn_Click(object sender, RoutedEventArgs e)
        {
            var banks = _bankService.GetAllBanks();
            string names = string.Join("\n", banks.Select(b => b.Name));
            MessageBox.Show(names, "Список банков");
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow loginWin = new MainWindow(); // Вместо LoginWindow
            loginWin.Show();
            this.Close();
        }

        private void TransactionsBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Раздел переводов в разработке");
        }

        private void AdminBtn_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно управления
            ManagerWindow managerWin = new ManagerWindow();
            managerWin.ShowDialog(); // ShowDialog заблокирует основное окно, пока это открыто

            // После закрытия окна менеджера можно обновить данные
            RefreshData();
        }
    }
}
