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

            var logService = new LogService(_db);
            _enterpriseService = new EnterpriseService(_db, logService);
            _bankService = new BankService(_db);

            RefreshAll();
        }

        private void RefreshAll()
        {
            PendingUsersGrid.ItemsSource = _db.Users.Where(u => u.Status == UserStatus.Pending).ToList();
            JoinRequestsGrid.ItemsSource = _enterpriseService.GetPendingJoinRequests();
            SalaryRequestsGrid.ItemsSource = _enterpriseService.GetPendingPaymentRequests();
            EnterprisesGrid.ItemsSource = _enterpriseService.GetEnterprisesWithEmployees();

            if (AllAccountsGrid != null)
            {
                AllAccountsGrid.ItemsSource = _bankService.GetAllAccounts();
            }
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainTabControl?.SelectedItem is TabItem selectedTab && selectedTab.Header.ToString() == "Счета клиентов")
            {
                RefreshAll();
            }
        }

        private void ToggleBlock_Click(object sender, RoutedEventArgs e)
        {
            var selectedAccount = AllAccountsGrid.SelectedItem as BankAccount;
            if (selectedAccount == null) return;

            try
            {
                _bankService.ToggleBlock(selectedAccount.Id);
                RefreshAll();
                MessageBox.Show("Статус счета изменен.");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void ViewHistory_Click(object sender, RoutedEventArgs e)
        {
            var selectedAccount = AllAccountsGrid.SelectedItem as BankAccount;
            if (selectedAccount == null) return;

            var historyWin = new AccountHistoryWindow(selectedAccount.Id, _bankService);
            historyWin.Owner = this;
            historyWin.ShowDialog();
        }

        // Старые методы одобрения
        private void ApproveUser_Click(object sender, RoutedEventArgs e)
        {
            var user = (sender as Button)?.DataContext as User;
            if (user == null) return;
            var dbUser = _db.Users.Find(user.Id);
            if (dbUser != null) { dbUser.Status = UserStatus.Active; dbUser.IsApproved = true; _db.SaveChanges(); RefreshAll(); }
        }

        private void ApproveJoin_Click(object sender, RoutedEventArgs e)
        {
            var req = (sender as Button)?.DataContext as SalaryRequest;
            if (req != null) { _enterpriseService.ApproveJoinRequest(req.Id); RefreshAll(); }
        }

        private void ApproveSalary_Click(object sender, RoutedEventArgs e)
        {
            var req = (sender as Button)?.DataContext as SalaryRequest;
            if (req != null) { _enterpriseService.ApprovePaymentRequest(req.Id, 50000); RefreshAll(); }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
