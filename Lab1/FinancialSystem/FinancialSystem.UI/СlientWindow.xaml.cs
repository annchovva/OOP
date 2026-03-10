using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FinancialSystem.Domain.Enums;
using Microsoft.VisualBasic; // Нужно для InputBox

namespace FinancialSystem.UI
{
    public partial class ClientWindow : Window
    {
        private User _currentUser;

        public ClientWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;

            // Первичная загрузка данных
            LoadBanks();
            LoadAccounts();
            UpdateTransferComboBoxes();
            UpdateDashboard();
        }

        #region НАВИГАЦИЯ

        private void Nav_Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 0;
            UpdateDashboard();
        }

        private void Nav_Banks_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 1;
            LoadBanks();
        }

        private void Nav_Accounts_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 2;
            LoadAccounts();
        }

        private void Nav_Transfers_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 3;
            UpdateTransferComboBoxes();
        }

        private void Nav_History_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 4;
            UpdateTransferComboBoxes();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var loginWin = new LoginWindow();
            loginWin.Show();
            this.Close();
        }

        #endregion

        #region ЗАГРУЗКА ДАННЫХ И ОБНОВЛЕНИЕ UI

        private void UpdateDashboard()
        {
            using (var db = new FinancialDbContext())
            {
                var total = db.Accounts
                    .Where(a => a.UserId == _currentUser.Id)
                    .Select(a => a.Balance)
                    .ToList()
                    .Sum();

                TotalBalanceText.Text = $"{total:N2} ₽";
                WelcomeText.Text = $"Добро пожаловать, {_currentUser.Username}!";
            }
        }

        private void LoadBanks()
        {
            using (var db = new FinancialDbContext())
            {
                var banks = db.Banks.ToList();
                BanksDataGrid.ItemsSource = banks;
                BanksComboBox.ItemsSource = banks;
            }
        }

        private void LoadAccounts()
        {
            using (var db = new FinancialDbContext())
            {
                var myAccounts = db.Accounts
                    .Include(a => a.Bank)
                    .Where(a => a.UserId == _currentUser.Id)
                    .ToList();

                AccountsDataGrid.ItemsSource = myAccounts;
            }
        }

        private void UpdateTransferComboBoxes()
        {
            using (var db = new FinancialDbContext())
            {
                var myAccounts = db.Accounts
                    .Where(a => a.UserId == _currentUser.Id)
                    .ToList();

                SourceAccountComboBox.ItemsSource = myAccounts;
                HistoryAccountComboBox.ItemsSource = myAccounts;
            }
        }

        #endregion

        #region ОПЕРАЦИИ СО СЧЕТАМИ

        private void OpenAccount_Click(object sender, RoutedEventArgs e)
        {
            if (BanksComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите банк.");
                return;
            }

            int selectedBankId = (int)BanksComboBox.SelectedValue;

            using (var db = new FinancialDbContext())
            {
                Random rnd = new Random();
                string newAccountNumber = "40817810" + rnd.Next(100, 999).ToString() + rnd.Next(100000, 999999).ToString();

                var newAccount = new Account
                {
                    UserId = _currentUser.Id,
                    BankId = selectedBankId,
                    Number = newAccountNumber,
                    Balance = 0,
                    IsBlocked = false
                };

                db.Accounts.Add(newAccount);
                db.SaveChanges();
            }

            MessageBox.Show("Счет успешно открыт!");
            LoadAccounts();
            UpdateTransferComboBoxes();
        }

        private void CloseAccount_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null) return;

            int accountId = (int)button.Tag;

            var result = MessageBox.Show("Вы уверены, что хотите закрыть счет?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No) return;

            using (var db = new FinancialDbContext())
            {
                var account = db.Accounts.Find(accountId);
                if (account != null)
                {
                    if (account.Balance > 0)
                    {
                        MessageBox.Show($"На счету остались средства ({account.Balance} ₽). Сначала снимите их.");
                        return;
                    }

                    db.Accounts.Remove(account);
                    db.SaveChanges();

                    LoadAccounts();
                    UpdateTransferComboBoxes();
                    UpdateDashboard();
                }
            }
        }

        // МЕТОД ПОПОЛНЕНИЯ (+)
        private void Deposit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int accountId)
            {
                string input = Interaction.InputBox("Введите сумму для пополнения:", "Пополнение счета", "1000");

                if (decimal.TryParse(input, out decimal amount) && amount > 0)
                {
                    using (var db = new FinancialDbContext())
                    {
                        var account = db.Accounts.Find(accountId);
                        if (account != null)
                        {
                            account.Balance += amount;

                            db.Transactions.Add(new Transaction
                            {
                                ToAccountId = account.Id,
                                Amount = amount,
                                Date = DateTime.Now,
                                Type = TransactionType.Deposit,
                                Description = "Пополнение через банкомат"
                            });

                            db.SaveChanges();
                            MessageBox.Show("Баланс пополнен!");

                            LoadAccounts();
                            UpdateDashboard();
                        }
                    }
                }
            }
        }

        // МЕТОД СНЯТИЯ (-)
        private void Withdraw_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int accountId)
            {
                string input = Interaction.InputBox("Введите сумму для снятия:", "Снятие наличных", "500");

                if (decimal.TryParse(input, out decimal amount) && amount > 0)
                {
                    using (var db = new FinancialDbContext())
                    {
                        var account = db.Accounts.Find(accountId);
                        if (account != null)
                        {
                            if (account.Balance < amount)
                            {
                                MessageBox.Show("Недостаточно средств на счету!");
                                return;
                            }

                            account.Balance -= amount;

                            db.Transactions.Add(new Transaction
                            {
                                FromAccountId = account.Id,
                                Amount = amount,
                                Date = DateTime.Now,
                                Type = TransactionType.Withdrawal,
                                Description = "Снятие наличных"
                            });

                            db.SaveChanges();
                            MessageBox.Show("Деньги выданы!");

                            LoadAccounts();
                            UpdateDashboard();
                        }
                    }
                }
            }
        }

        #endregion

        #region ПЕРЕВОДЫ И ИСТОРИЯ

        private void TransferMoney_Click(object sender, RoutedEventArgs e)
        {
            if (SourceAccountComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(TargetAccountNumberTextBox.Text) ||
                !decimal.TryParse(TransferAmountTextBox.Text, out decimal amount))
            {
                MessageBox.Show("Проверьте правильность заполнения полей.");
                return;
            }

            if (amount <= 0) return;

            int sourceId = (int)SourceAccountComboBox.SelectedValue;
            string targetNumber = TargetAccountNumberTextBox.Text.Trim();

            using (var db = new FinancialDbContext())
            {
                var sourceAcc = db.Accounts.Find(sourceId);
                var targetAcc = db.Accounts.FirstOrDefault(a => a.Number == targetNumber);

                if (targetAcc == null || sourceAcc.Id == targetAcc.Id || sourceAcc.Balance < amount)
                {
                    MessageBox.Show("Ошибка: проверьте номер получателя и баланс.");
                    return;
                }

                sourceAcc.Balance -= amount;
                targetAcc.Balance += amount;

                db.Transactions.Add(new Transaction
                {
                    FromAccountId = sourceAcc.Id,
                    ToAccountId = targetAcc.Id,
                    Amount = amount,
                    Date = DateTime.Now,
                    Type = TransactionType.Transfer,
                    Description = $"Перевод на {targetAcc.Number}"
                });

                db.SaveChanges();
                MessageBox.Show("Перевод выполнен!");

                TransferAmountTextBox.Clear();
                TargetAccountNumberTextBox.Clear();
                LoadAccounts();
                UpdateDashboard();
            }
        }

        private void HistoryAccountComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HistoryAccountComboBox.SelectedValue == null) return;

            int accountId = (int)HistoryAccountComboBox.SelectedValue;

            using (var db = new FinancialDbContext())
            {
                var history = db.Transactions
                    .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
                    .OrderByDescending(t => t.Date)
                    .ToList();

                TransactionsDataGrid.ItemsSource = history;
            }
        }

        #endregion
    }
}

