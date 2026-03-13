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
                // Необязательно, но логично: запретить закрывать, если есть деньги
                if (account.Balance > 0)
                    throw new Exception("Сначала переведите остаток средств на другой счет.");

                _db.BankAccounts.Remove(account);
                _db.SaveChanges();
            }
        }

        // Исправленная версия для соответствия интерфейсу
        public bool TransferMoney(int fromAccountId, string toAccountNumber, decimal amount)
        {
            var fromAccount = _db.BankAccounts.Find(fromAccountId);
            // Ищем получателя по строковому НОМЕРУ счета
            var toAccount = _db.BankAccounts.FirstOrDefault(a => a.AccountNumber == toAccountNumber);

            if (fromAccount == null || toAccount == null) return false;

            if (fromAccount.IsBlocked)
                throw new Exception("Операция невозможна: ваш счет заблокирован.");

            if (fromAccount.Balance < amount) return false;

            fromAccount.Balance -= amount;
            toAccount.Balance += amount;

            _db.Transactions.Add(new TransactionRecord
            {
                FromAccountId = fromAccountId,
                ToAccountId = toAccount.Id, // Берем ID найденного счета
                Amount = amount,
                Date = DateTime.Now,
                Description = "Перевод по номеру счета"
            });

            _db.SaveChanges();
            return true;
        }



        public List<TransactionRecord> GetTransactionHistory(int userId)
        {
            var userAccountIds = _db.BankAccounts
                .Where(a => a.UserId == userId)
                .Select(a => a.Id)
                .ToList();

            return _db.Transactions
                .Include(t => t.FromAccount) // Загружаем объект счета отправителя
                .Include(t => t.ToAccount)   // Загружаем объект счета получателя
                .Where(t => (t.FromAccountId != null && userAccountIds.Contains(t.FromAccountId.Value)) ||
                            (t.ToAccountId != null && userAccountIds.Contains(t.ToAccountId.Value)))
                .OrderByDescending(t => t.Date)
                .ToList();
        }

        public void OpenDeposit(int userId, int bankId, decimal initialAmount, double interestRate)
        {
            // 1. Создаем объект (называем его account)
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
            _db.SaveChanges(); // Сохраняем здесь, чтобы БД сгенерировала Id для account

            // 2. Создаем запись о транзакции
            // ВАЖНО: используем account.Id (то же имя, что и выше)
            _db.Transactions.Add(new TransactionRecord
            {
                ToAccountId = account.Id, // Исправлено с deposit.Id на account.Id
                Amount = initialAmount,
                Description = "Открытие вклада",
                Date = DateTime.Now
            });

            _db.SaveChanges();
        }

        public void AccrueInterest(int accountId)
        {
            var account = _db.BankAccounts.Find(accountId);

            // ДОБАВЛЕНО: проверка !account.IsBlocked
            // Теперь проценты начисляются только если счет НЕ заблокирован
            if (account != null && account.Type == AccountType.Deposit && account.InterestRate > 0 && !account.IsBlocked)
            {
                decimal interest = account.Balance * (decimal)(account.InterestRate / 100);
                account.Balance += interest;

                _db.Transactions.Add(new TransactionRecord
                {
                    ToAccountId = account.Id,
                    Amount = interest,
                    Description = "Начисление процентов (накопление)",
                    Date = DateTime.Now
                });

                _db.SaveChanges();
            }
        }

        public List<BankAccount> GetAllAccounts()
        {
            // Используем Include, чтобы менеджер видел имена владельцев
            return _db.BankAccounts.Include(a => a.User).Include(a => a.Bank).ToList();
        }

        public void ToggleBlock(int accountId)
        {
            var account = _db.BankAccounts.Find(accountId);
            if (account != null)
            {
                account.IsBlocked = !account.IsBlocked; // Инвертируем статус
                _db.SaveChanges();
            }
        }

        public List<TransactionRecord> GetAccountHistory(int accountId)
        {
            return _db.Transactions
                .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
                .OrderByDescending(t => t.Date)
                .ToList();
        }
    }
}
