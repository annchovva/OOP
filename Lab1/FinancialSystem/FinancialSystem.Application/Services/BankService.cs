using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;
using FinancialSystem.Infrastructure;
using FinancialSystem.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinancialSystem.Application.Services
{
    public class BankService : IBankService
    {
        private readonly FinanceDbContext _db;

        public BankService(FinanceDbContext db)
        {
            _db = db;
        }

        public List<Bank> GetAllBanks() => _db.Banks.ToList();

        public List<BankAccount> GetUserAccounts(int userId)
        {
            return _db.BankAccounts
                .Where(a => a.UserId == userId)
                .Include(a => a.Bank)
                .ToList();
        }

        // Реализация того самого OpenAccount, на который ругается ошибка
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
        }

        public void CloseAccount(int accountId)
        {
            var account = _db.BankAccounts.Find(accountId);
            if (account != null)
            {
                _db.BankAccounts.Remove(account);
                _db.SaveChanges();
            }
        }

        public bool TransferMoney(int fromAccountId, string toAccountNumber, decimal amount)
        {
            var source = _db.BankAccounts.Find(fromAccountId);
            var destination = _db.BankAccounts.FirstOrDefault(a => a.AccountNumber == toAccountNumber);

            if (source == null || destination == null || source.Balance < amount || amount <= 0 || source.IsBlocked || destination.IsBlocked)
                return false;

            source.Balance -= amount;
            destination.Balance += amount;

            var transaction = new TransactionRecord
            {
                FromAccountId = source.Id,
                ToAccountId = destination.Id,
                Amount = amount,
                Date = DateTime.Now
                // Если в классе TransactionRecord есть поле Description, раскомментируй ниже:
                // Description = $"Перевод на счет {toAccountNumber}"
            };

            _db.Transactions.Add(transaction);
            _db.SaveChanges();
            return true;
        }

        public List<TransactionRecord> GetTransactionHistory(int userId)
        {
            // Получаем ID всех счетов этого пользователя
            var userAccountIds = _db.BankAccounts
                .Where(a => a.UserId == userId)
                .Select(a => a.Id)
                .ToList();

            // Ищем транзакции, где эти счета были либо отправителями, либо получателями
            return _db.Transactions
                .Where(t => (t.FromAccountId != null && userAccountIds.Contains(t.FromAccountId.Value)) ||
                            (t.ToAccountId != null && userAccountIds.Contains(t.ToAccountId.Value)))
                .OrderByDescending(t => t.Date)
                .ToList();
        }
    }
}
