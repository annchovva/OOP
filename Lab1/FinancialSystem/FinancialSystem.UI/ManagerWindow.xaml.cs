using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FinancialSystem.Application.Interfaces;
using FinancialSystem.Application.Services;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;
using FinancialSystem.Infrastructure;

namespace FinancialSystem.UI
{
    public partial class ManagerWindow : Window
    {
        private readonly FinanceDbContext _db;
        private readonly IEnterpriseService _enterpriseService;
        private readonly IBankService _bankService;

        public ManagerWindow()
        {
            InitializeComponent();
            _db = new FinanceDbContext();

            // Инициализация сервисов
            var logService = new LogService(_db);
            _enterpriseService = new EnterpriseService(_db, logService);
            _bankService = new BankService(_db);

            RefreshAll();
        }

        private void RefreshAll()
        {
            try
            {
                PendingUsersGrid.ItemsSource = _db.Users.Where(u => u.Status == UserStatus.Pending).ToList();
                JoinRequestsGrid.ItemsSource = _enterpriseService.GetPendingJoinRequests();
                SalaryRequestsGrid.ItemsSource = _enterpriseService.GetPendingPaymentRequests();

                if (AllAccountsGrid != null)
                {
                    AllAccountsGrid.ItemsSource = _bankService.GetAllAccounts();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}");
            }
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Это критически важная проверка: если UI еще не загружен, выходим
            if (!this.IsLoaded) return;

            // Обновляем данные только если событие пришло именно от TabControl
            if (e.Source is TabControl)
            {
                RefreshAll();
            }
        }

        private void ApproveUser_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is User user)
            {
                var dbUser = _db.Users.Find(user.Id);
                if (dbUser != null)
                {
                    dbUser.Status = UserStatus.Active;
                    dbUser.IsApproved = true;
                    _db.SaveChanges();
                    RefreshAll();
                }
            }
        }

        private void ApproveJoin_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalaryRequest req)
            {
                _enterpriseService.ApproveJoinRequest(req.Id);
                RefreshAll();
            }
        }

        private void ApproveSalary_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalaryRequest req)
            {
                // По ТЗ сумма выплаты может быть фиксированной или браться из заявки
                _enterpriseService.ApprovePaymentRequest(req.Id, 50000);
                RefreshAll();
                MessageBox.Show("Зарплата успешно выплачена.");
            }
        }

        private void ToggleBlock_Click(object sender, RoutedEventArgs e)
        {
            if (AllAccountsGrid.SelectedItem is BankAccount selectedAccount)
            {
                try
                {
                    _bankService.ToggleBlock(selectedAccount.Id);
                    RefreshAll();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
            else
            {
                MessageBox.Show("Выберите счет для блокировки/разблокировки.");
            }
        }

        private void ViewHistory_Click(object sender, RoutedEventArgs e)
        {
            if (AllAccountsGrid.SelectedItem is BankAccount selectedAccount)
            {
                // Предполагается, что окно AccountHistoryWindow создано по аналогии с HistoryWindow
                var historyWin = new HistoryWindow();
                var history = _bankService.GetTransactionHistory(selectedAccount.Id);
                historyWin.SetHistoryData(history);
                historyWin.Owner = this;
                historyWin.ShowDialog();
            }
        }
    }
}
