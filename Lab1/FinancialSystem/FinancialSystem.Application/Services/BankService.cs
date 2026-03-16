using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;
using FinancialSystem.Infrastructure;
using FinancialSystem.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization; 

namespace FinancialSystem.Application.Services
{
    public class BankService : IBankService
    {
        private readonly FinanceDbContext _db;
        private readonly ILogService _logService;

        public BankService(FinanceDbContext db, ILogService logService)
        {
            _db = db;
            _logService = logService;
        }

        public List<Bank> GetAllBanks() => _db.Banks.ToList();

        public List<BankAccount> GetUserAccounts(int userId)
        {
            return _db.BankAccounts
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .Include(a => a.Bank)
                .ToList();
        }

        public void OpenAccount(int userId, int bankId, AccountType type)
        {
            var account = new BankAccount
            {
                UserId = userId,
                BankId = bankId,
                Type = type,
                Balance = 0,
                IsBlocked = false,
                AccountNumber = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                CreatedAt = DateTime.Now
            };

            _db.BankAccounts.Add(account);
            _db.SaveChanges();

            _logService.Log(userId, "OpenAccount", $"Открытие счета {account.AccountNumber}", account.Id.ToString());
        }

        public void CloseAccount(int accountId)
        {
            var account = _db.BankAccounts.FirstOrDefault(a => a.Id == accountId);
            if (account == null) return;

            // сохраняем данные для лога
            string oldAccNumber = account.AccountNumber;
            int userId = account.UserId;

            var transactions = _db.Transactions
                .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
                .ToList();

            if (transactions.Any())
            {
                _db.Transactions.RemoveRange(transactions);
            }

            _db.BankAccounts.Remove(account);
            _db.SaveChanges();

            _logService.Log(userId, "CloseAccount", $"Закрытие счета {oldAccNumber}", accountId.ToString());
        }

        public bool TransferMoney(int fromAccountId, string toAccountNumber, decimal amount)
        {
            var fromAccount = _db.BankAccounts.Find(fromAccountId);
            var toAccount = _db.BankAccounts.FirstOrDefault(a => a.AccountNumber == toAccountNumber);

            if (fromAccount == null || toAccount == null) return false;

            if (fromAccountId == toAccount.Id) return false;

            if (fromAccount.IsBlocked) throw new Exception("Операция невозможна: ваш счет заблокирован.");

            if (toAccount.IsBlocked) throw new Exception("Операция невозможна: счет получателя заблокирован.");

            if (fromAccount.Balance < amount) return false;

            fromAccount.Balance -= amount;
            toAccount.Balance += amount;

            _db.Transactions.Add(new TransactionRecord
            {
                FromAccountId = fromAccountId,
                ToAccountId = toAccount.Id,
                Amount = amount,
                Date = DateTime.Now,
                Description = "Перевод по номеру счета"
            });

            _db.SaveChanges();

            _logService.Log(fromAccount.UserId, "Transfer",
                $"Перевод {amount} на счет {toAccountNumber}",
                $"{fromAccount.Id};{toAccount.Id};{amount.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

            return true;
        }

        public void OpenDeposit(int userId, int bankId, decimal initialAmount, decimal interestRate)
        {
            var account = new BankAccount
            {
                UserId = userId,
                BankId = bankId,
                AccountNumber = "DEP-" + new Random().Next(1000, 9999).ToString(),
                Balance = initialAmount,
                Type = AccountType.Deposit,
                InterestRate = interestRate,
                IsBlocked = false,
                CreatedAt = DateTime.Now
            };

            _db.BankAccounts.Add(account);
            _db.SaveChanges();

            _db.Transactions.Add(new TransactionRecord
            {
                ToAccountId = account.Id,
                Amount = initialAmount,
                Description = "Открытие вклада",
                Date = DateTime.Now
            });

            _db.SaveChanges();

            _logService.Log(userId, "OpenAccount", $"Открытие вклада {account.AccountNumber}", account.Id.ToString());
        }

        public void AccrueInterest(int accountId)
        {
            var account = _db.BankAccounts.FirstOrDefault(a => a.Id == accountId);
            if (account == null) return;

            if (account.Type == AccountType.Deposit && !account.IsBlocked)
            {
                decimal interest = account.Balance * (decimal)(account.InterestRate / 100);
                if (interest <= 0) return;

                account.Balance += interest;

                _db.Transactions.Add(new TransactionRecord
                {
                    ToAccountId = account.Id,
                    Amount = interest,
                    Description = "Проценты",
                    Date = DateTime.Now
                });

                _db.SaveChanges();

                _logService.Log(account.UserId, "AccrueInterest",
                    $"Начисление процентов {interest} на счет {account.AccountNumber}",
                    $"{account.Id};{interest.ToString(CultureInfo.InvariantCulture)}");
            }
        }

        public void ToggleBlock(int accountId)
        {
            var account = _db.BankAccounts.Find(accountId);
            if (account != null)
            {
                account.IsBlocked = !account.IsBlocked;
                _db.SaveChanges();

                _logService.Log(account.UserId, "ToggleBlock",
                    $"Изменение блокировки счета {account.AccountNumber} (Статус: {account.IsBlocked})",
                    account.Id.ToString());
            }
        }

        public List<BankAccount> GetAllAccounts()
        {
            return _db.BankAccounts
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Bank)
                .ToList();
        }

        public List<TransactionRecord> GetAccountHistory(int accountId)
        {
            return _db.Transactions
                .AsNoTracking()
                .Include(t => t.FromAccount)
                .Include(t => t.ToAccount)
                .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
                .OrderByDescending(t => t.Date)
                .ToList();
        }
    }
}
