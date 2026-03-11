using System;
using System.Linq;
using System.Windows;
using FinancialSystem.Application.Interfaces;
using FinancialSystem.Application.Services;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;
using FinancialSystem.Infrastructure;

namespace FinancialSystem.UI
{
    public partial class DashboardWindow : Window
    {
        private readonly User _user;
        private readonly IBankService _bankService;

        public DashboardWindow(User user)
        {
            InitializeComponent();
            _user = user;

            var db = new FinanceDbContext();
            _bankService = new BankService(db);

            // 1. Настройка информации о пользователе
            UserInfoLabel.Text = $"Привет, {_user.Login}";
            UserRoleLabel.Text = $"Ваша роль: {_user.Role}";
            UserStatusText.Text = $"Статус: {_user.Status}";

            // 2. Разделение доступа к кнопкам (Строго по ТЗ)
            if (_user.Role == UserRole.Manager)
            {
                ManagerPanelBtn.Visibility = Visibility.Visible;
            }
            else if (_user.Role == UserRole.Admin)
            {
                AdminPanelBtn.Visibility = Visibility.Visible;
                // По желанию: Админ тоже может видеть панель менеджера
                // ManagerPanelBtn.Visibility = Visibility.Visible; 
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
                MessageBox.Show("В системе еще нет банков.");
                return;
            }

            // Для примера открываем в первом доступном банке
            _bankService.OpenAccount(_user.Id, banks[0].Id, AccountType.Current);
            RefreshData();
            MessageBox.Show("Счет успешно открыт!");
        }

        private void ViewBanksBtn_Click(object sender, RoutedEventArgs e)
        {
            var banks = _bankService.GetAllBanks();
            string names = string.Join("\n", banks.Select(b => $"• {b.Name}"));
            MessageBox.Show(names, "Доступные банки");
        }

        private void ManagerPanelBtn_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно менеджера (подтверждение регистраций, управление фирмами)
            ManagerWindow managerWin = new ManagerWindow();
            managerWin.ShowDialog();
            RefreshData();
        }

        private void AdminPanelBtn_Click(object sender, RoutedEventArgs e)
        {
            // Сюда добавим окно логов для Админа (FunctionalAdmin.ViewLogs)
            MessageBox.Show("Окно логов и отмены действий в разработке");
            // AdminLogsWindow adminWin = new AdminLogsWindow();
            // adminWin.ShowDialog();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow loginWin = new MainWindow();
            loginWin.Show();
            this.Close();
        }

        private void TransactionsBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Раздел переводов будет доступен в следующей итерации");
        }
    }
}
