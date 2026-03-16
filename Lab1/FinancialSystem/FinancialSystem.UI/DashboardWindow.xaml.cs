using System;
using System.Linq;
using System.Windows;
using FinancialSystem.Application.Interfaces;
using FinancialSystem.Application.Services;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;
using FinancialSystem.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinancialSystem.UI
{
    public partial class DashboardWindow : Window
    {
        private readonly User _user;
        private readonly IBankService _bankService;
        private readonly FinanceDbContext _db;

        public DashboardWindow(User user)
        {
            InitializeComponent();
            _user = user;

            _db = new FinanceDbContext();
            var logService = new LogService(_db);
            _bankService = new BankService(_db, logService);

            // информация
            UserInfoLabel.Text = $"Привет, {_user.Login}";
            UserRoleLabel.Text = $"Ваша роль: {_user.Role}";
            string statusDisplay = _user.IsApproved ? "Активен" : "Ожидает подтверждения менеджером";
            UserStatusText.Text = $"Статус: {statusDisplay}";

            SetupAccess();
        }

        private void SetupAccess()
        {
            SalaryBtn.Visibility = _user.Role == UserRole.Client ? Visibility.Visible : Visibility.Collapsed;

            if (_user.Role == UserRole.Manager)
            {
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
                _db.ChangeTracker.Clear();

                var accounts = _db.BankAccounts
                    .Include(a => a.Bank)
                    .Where(a => a.UserId == _user.Id)
                    .ToList();

                AccountsList.ItemsSource = null;
                AccountsList.ItemsSource = accounts;
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Ошибка при обновлении данных: " + ex.Message, "Ошибка", this);
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
            try
            {
                var banks = _bankService.GetAllBanks();
                var dialog = new OpenAccountWindow(banks);
                dialog.Owner = this;

                if (dialog.ShowDialog() == true)
                {
                    var selectedBank = dialog.SelectedBank;
                    if (selectedBank == null) return;

                    _bankService.OpenAccount(_user.Id, selectedBank.Id, AccountType.Current);

                    CustomMessageBox.Show($"Счет в банке «{selectedBank.Name}» успешно открыт!", "Успех", this);
                    RefreshData();
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при открытии счета: {ex.Message}", "Ошибка", this);
            }
        }

        private void ViewBanksBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var banks = _bankService.GetAllBanks();
                string names = string.Join("\n", banks.Select(b => $"• {b.Name}"));
                CustomMessageBox.Show(names, "Доступные банки", this);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Ошибка: " + ex.Message, "Ошибка", this);
            }
        }

        private void AccrueBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!(AccountsList.SelectedItem is BankAccount selected))
            {
                CustomMessageBox.Show("Пожалуйста, выберите вклад в списке для начисления.", "Внимание", this);
                return;
            }

            try
            {
                _bankService.AccrueInterest(selected.Id);
                RefreshData();
                CustomMessageBox.Show("Проценты успешно начислены на остаток.", "Операция завершена", this);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Ошибка начисления: " + ex.Message, "Ошибка", this);
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!(AccountsList.SelectedItem is BankAccount selected)) return;

            if (selected.Balance > 0)
            {
                CustomMessageBox.Show("Нельзя закрыть счет, на котором есть деньги. Сначала снимите или переведите остаток.", "Действие невозможно", this);
                return;
            }

            bool confirm = CustomMessageBox.ShowQuestion(
                "Вы действительно хотите навсегда закрыть этот счет?",
                "Подтверждение закрытия",
                this
            );

            if (confirm)
            {
                try
                {
                    _bankService.CloseAccount(selected.Id);
                    RefreshData();
                    CustomMessageBox.Show("Счет успешно закрыт и удален из системы.", "Готово", this);
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show("Ошибка при закрытии: " + ex.Message, "Ошибка", this);
                }
            }
        }

        private void TransferBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!(AccountsList.SelectedItem is BankAccount selected))
            {
                CustomMessageBox.Show("Выберите счет, с которого хотите сделать перевод.", "Внимание", this);
                return;
            }

            if (selected.IsBlocked)
            {
                CustomMessageBox.Show("Этот счет заблокирован менеджером. Операции запрещены.", "Доступ ограничен", this);
                return;
            }

            TransferWindow transferWin = new TransferWindow(selected.Id, _bankService);
            transferWin.Owner = this;

            if (transferWin.ShowDialog() == true)
            {
                RefreshData();
            }
        }

        private void OpenDepositBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenDepositWindow openDepWin = new OpenDepositWindow(_user.Id, _bankService);
            openDepWin.Owner = this;

            if (openDepWin.ShowDialog() == true)
            {
                RefreshData();
            }
        }

        private void SalaryBtn_Click(object sender, RoutedEventArgs e)
        {
            SalaryWindow salaryWin = new SalaryWindow(_user);
            salaryWin.Owner = this;
            salaryWin.ShowDialog();
            RefreshData();
        }

        private void HistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!(AccountsList.SelectedItem is BankAccount selected))
            {
                CustomMessageBox.Show("Выберите счет для просмотра истории транзакций.", "Внимание", this);
                return;
            }

            var historyWin = new HistoryWindow();
            historyWin.Owner = this;
            var transactions = _bankService.GetAccountHistory(selected.Id);
            historyWin.SetHistoryData(transactions);
            historyWin.ShowDialog();
        }

        private void ManagerPanelBtn_Click(object sender, RoutedEventArgs e)
        {
            ManagerWindow managerWin = new ManagerWindow();
            managerWin.Owner = this;
            managerWin.ShowDialog();
            RefreshData();
        }

        private void AdminPanelBtn_Click(object sender, RoutedEventArgs e)
        {
            AdminLogsWindow adminWin = new AdminLogsWindow();
            adminWin.Owner = this;
            adminWin.ShowDialog();
            RefreshData();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow loginWin = new MainWindow();
            loginWin.Show();
            this.Close();
        }
    }
}
