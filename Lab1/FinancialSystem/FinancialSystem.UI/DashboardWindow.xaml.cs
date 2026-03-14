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
            SetupAccess();
        }

        private void SetupAccess()
        {
            SalaryBtn.Visibility = _user.Role == UserRole.Client ? Visibility.Visible : Visibility.Collapsed;

            if (_user.Role == UserRole.Manager || _user.Role == UserRole.Admin)
            {
                AdminSectionHeader.Visibility = Visibility.Visible;
                ManagerPanelBtn.Visibility = Visibility.Visible;
            }

            if (_user.Role == UserRole.Admin)
            {
                AdminPanelBtn.Visibility = Visibility.Visible;
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

        // Обработчик для Зарплатного проекта
        private void SalaryBtn_Click(object sender, RoutedEventArgs e)
        {
            SalaryWindow salaryWin = new SalaryWindow(_user);
            salaryWin.ShowDialog();
            RefreshData(); // Обновляем баланс, если клиент получил деньги
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

            // Открываем Текущий счет в первом банке (для упрощения)
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
            ManagerWindow managerWin = new ManagerWindow();
            managerWin.ShowDialog();
            RefreshData();
        }

        private void AdminPanelBtn_Click(object sender, RoutedEventArgs e)
        {
            // Окно логов теперь активно!
            AdminLogsWindow adminWin = new AdminLogsWindow();
            adminWin.ShowDialog();
            RefreshData();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow loginWin = new MainWindow();
            loginWin.Show();
            this.Close();
        }

        private void TransactionsBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowHistory();
        }

        private void TransferBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedAccount = AccountsList.SelectedItem as BankAccount;

            if (selectedAccount == null)
            {
                MessageBox.Show("Выберите счет для перевода.");
                return;
            }

            // ПРОВЕРКА БЛОКИРОВКИ
            if (selectedAccount.IsBlocked)
            {
                MessageBox.Show("Этот счет заблокирован менеджером. Вы не можете переводить с него средства.");
                return;
            }

            TransferWindow transferWin = new TransferWindow(selectedAccount.Id, _bankService);
            transferWin.Owner = this;

            if (transferWin.ShowDialog() == true)
            {
                RefreshData();
            }
        }


        // 3. Логика открытия истории
        private void HistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowHistory();
        }

        // Вспомогательный метод для показа истории
        private void ShowHistory()
        {
            var historyWin = new HistoryWindow();
            historyWin.Owner = this;

            // Получаем историю транзакций через сервис
            var transactions = _bankService.GetTransactionHistory(_user.Id);
            historyWin.SetHistoryData(transactions);

            historyWin.ShowDialog();
        }

        // НАКОПЛЕНИЕ (Начисление процентов)

        private void OpenDepositBtn_Click(object sender, RoutedEventArgs e)
        {
            // Создаем экземпляр окна, которое мы правили в прошлом шаге
            OpenDepositWindow openDepWin = new OpenDepositWindow(_user.Id, _bankService);

            // Указываем владельца окна, чтобы оно открылось по центру родителя
            openDepWin.Owner = this;

            // Показываем окно. Если пользователь нажмет "Открыть вклад", ShowDialog вернет true
            if (openDepWin.ShowDialog() == true)
            {
                // Обновляем таблицу, чтобы новый вклад появился в списке
                RefreshData();
            }
        }

        private void AccrueBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = AccountsList.SelectedItem as BankAccount;
            if (selected == null || selected.Type != AccountType.Deposit)
            {
                MessageBox.Show("Выберите вклад (депозит) для начисления процентов.");
                return;
            }

            // ПРОВЕРКА БЛОКИРОВКИ
            if (selected.IsBlocked)
            {
                MessageBox.Show("Счет заблокирован банком. Начисление процентов приостановлено.", "Отказ", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            _bankService.AccrueInterest(selected.Id);
            RefreshData();
            MessageBox.Show("Проценты успешно начислены!");
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedAccount = AccountsList.SelectedItem as BankAccount;
            if (selectedAccount == null)
            {
                MessageBox.Show("Выберите счет для закрытия.");
                return;
            }

            // ПРОВЕРКА БЛОКИРОВКИ
            if (selectedAccount.IsBlocked)
            {
                MessageBox.Show("Нельзя закрыть заблокированный счет. Обратитесь к менеджеру для разблокировки.");
                return;
            }

            if (selectedAccount.Balance > 0)
            {
                MessageBox.Show($"На счету осталось {selectedAccount.Balance:N2} ₽. Сначала выведите средства.");
                return;
            }

            var result = MessageBox.Show($"Вы действительно хотите закрыть счет {selectedAccount.AccountNumber}?",
                         "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _bankService.CloseAccount(selectedAccount.Id);
                RefreshData();
            }
        }
    }
}
