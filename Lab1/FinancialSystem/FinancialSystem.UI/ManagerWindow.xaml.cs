using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FinancialSystem.Application.Interfaces;
using FinancialSystem.Application.Services;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;
using FinancialSystem.Infrastructure;
using Microsoft.EntityFrameworkCore;

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
            _bankService = new BankService(_db, logService);

            RefreshAll();
        }

        private void RefreshAll()
        {
            try
            {
                _db.ChangeTracker.Clear();

                // новые пользователи
                PendingUsersGrid.ItemsSource = _db.Users.Where(u => u.IsApproved == false).ToList();

                // заявки на трудоустройство
                JoinRequestsGrid.ItemsSource = _enterpriseService.GetPendingJoinRequests();

                // заявки на выплату
                SalaryRequestsGrid.ItemsSource = _enterpriseService.GetPendingPaymentRequests();

                // список штата
                EnterpriseStaffGrid.ItemsSource = _db.Users
                    .Include(u => u.Enterprise)
                    .Where(u => u.EnterpriseId != null)
                    .ToList();

                // все счета
                if (AllAccountsGrid != null)
                {
                    AllAccountsGrid.ItemsSource = _bankService.GetAllAccounts();
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при обновлении данных: {ex.Message}", "Ошибка", this);
            }
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsLoaded) return;
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
                    dbUser.IsApproved = true;
                    _db.SaveChanges();

                    var logService = new LogService(_db);
                    logService.Log(user.Id, "ConfirmUser",
                        $"Регистрация пользователя {user.Login} подтверждена",
                        user.Id.ToString());

                    CustomMessageBox.Show($"Пользователь {user.Login} успешно активирован.", "Активация", this);
                    RefreshAll();
                }
            }
        }

        private void ApproveJoin_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalaryRequest req)
            {
                _enterpriseService.ApproveJoinRequest(req.Id);
                CustomMessageBox.Show("Заявка на вступление в штат одобрена.", "Персонал", this);
                RefreshAll();
            }
        }

        private void ApproveSalary_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalaryRequest req)
            {
                _enterpriseService.ApprovePaymentRequest(req.Id, 50000);
                RefreshAll();
                CustomMessageBox.Show("Выплата одобрена и ожидает зачисления пользователем.", "Зарплата", this);
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
                    string status = !selectedAccount.IsBlocked ? "заблокирован" : "разблокирован";
                    CustomMessageBox.Show($"Счет успешно {status}.", "Статус счета", this);
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(ex.Message, "Ошибка", this);
                }
            }
        }

        private void RemoveEmployee_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is User employee)
            {
                bool result = CustomMessageBox.ShowQuestion(
                    $"Вы уверены, что хотите уволить сотрудника {employee.Login}?",
                    "Подтверждение увольнения", this);

                if (result)
                {
                    try
                    {
                        var dbUser = _db.Users.Find(employee.Id);
                        if (dbUser != null && dbUser.EnterpriseId != null)
                        {
                            int oldEntId = dbUser.EnterpriseId.Value; 

                            var logService = new LogService(_db);
                            logService.Log(dbUser.Id, "Resign",
                                $"Сотрудник {dbUser.Login} уволен менеджером",
                                $"{dbUser.Id};{oldEntId}");

                            dbUser.EnterpriseId = null;

                            var pendingRequests = _db.SalaryRequests
                                .Where(r => r.UserId == employee.Id && r.Status != SalaryRequestStatus.Completed);
                            _db.SalaryRequests.RemoveRange(pendingRequests);

                            _db.SaveChanges();

                            CustomMessageBox.Show($"Сотрудник {employee.Login} успешно уволен.", "Готово", this);
                            RefreshAll();
                        }
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show($"Ошибка при увольнении: {ex.Message}", "Ошибка", this);
                    }
                }
            }
        }


        private void ViewHistory_Click(object sender, RoutedEventArgs e)
        {
            if (AllAccountsGrid.SelectedItem is BankAccount selectedAccount)
            {
                var historyWin = new HistoryWindow();
                historyWin.Owner = this;
                var history = _bankService.GetAccountHistory(selectedAccount.Id);
                historyWin.SetHistoryData(history);
                historyWin.ShowDialog();
            }
            else
            {
                CustomMessageBox.Show("Выберите счет в таблице для просмотра истории.", "Внимание", this);
            }
        }
    }
}
