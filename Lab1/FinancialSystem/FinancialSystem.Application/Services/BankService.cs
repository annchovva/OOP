using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;
using FinancialSystem.Infrastructure;
using FinancialSystem.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization; // Важно для корректной записи сумм в логи

namespace FinancialSystem.Application.Services
{
    public class BankService : IBankService
    {
        private readonly FinanceDbContext _db;
        private readonly ILogService _logService; // Добавили сервис логов

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

            // ЛОГ: Тип OpenAccount, ТехДанные: ID нового счета
            _logService.Log(userId, "OpenAccount", $"Открытие счета {account.AccountNumber}", account.Id.ToString());
        }

        public bool TransferMoney(int fromAccountId, string toAccountNumber, decimal amount)
        {
            var fromAccount = _db.BankAccounts.Find(fromAccountId);
            var toAccount = _db.BankAccounts.FirstOrDefault(a => a.AccountNumber == toAccountNumber);

            // 1. Проверка существования
            if (fromAccount == null || toAccount == null) return false;

            // 2. Проверка перевода самому себе
            if (fromAccountId == toAccount.Id) return false;

            // 3. Проверка отправителя
            if (fromAccount.IsBlocked)
                throw new Exception("Операция невозможна: ваш счет заблокирован.");

            // 4. ДОБАВЛЕНО: Проверка получателя
            if (toAccount.IsBlocked)
                throw new Exception("Операция невозможна: счет получателя заблокирован и не может принимать средства.");

            // 5. Проверка баланса
            if (fromAccount.Balance < amount) return false;

            // Проведение транзакции
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

            // ЛОГ: Тип Transfer, ТехДанные: ОтКого;Кому;Сумма
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

            // ЛОГ: Используем OpenAccount для вклада, чтобы админ мог его закрыть отменой
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

                // ЛОГ: Тип AccrueInterest, ТехДанные: ID счета;СуммаПроцентов
                _logService.Log(account.UserId, "AccrueInterest",
                    $"Начисление процентов {interest} на счет {account.AccountNumber}",
                    $"{account.Id};{interest.ToString(CultureInfo.InvariantCulture)}");
            }
        }

        public void CloseAccount(int accountId)
        {
            var account = _db.BankAccounts.FirstOrDefault(a => a.Id == accountId);
            if (account == null) return;

            string oldAccNumber = account.AccountNumber;
            int userId = account.UserId;

            // 1. Удаляем транзакции (связи Restrict)
            var transactions = _db.Transactions
                .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
                .ToList();

            if (transactions.Any())
            {
                _db.Transactions.RemoveRange(transactions);
            }

            // 2. Удаляем счет
            _db.BankAccounts.Remove(account);
            _db.SaveChanges();

            // ЛОГ: Тип CloseAccount, ТехДанные: ID счета (в истории останется для справки)
            _logService.Log(userId, "CloseAccount", $"Закрытие счета {oldAccNumber}", accountId.ToString());
        }

        public void ToggleBlock(int accountId)
        {
            var account = _db.BankAccounts.Find(accountId);
            if (account != null)
            {
                account.IsBlocked = !account.IsBlocked;
                _db.SaveChanges();

                // ЛОГ: Тип ToggleBlock
                _logService.Log(account.UserId, "ToggleBlock",
                    $"Изменение блокировки счета {account.AccountNumber} (Статус: {account.IsBlocked})",
                    account.Id.ToString());
            }
        }

        // --- Вспомогательные методы получения данных ---

        public List<BankAccount> GetAllAccounts()
        {
            return _db.BankAccounts
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Bank)
                .ToList();
        }

        public List<TransactionRecord> GetTransactionHistory(int userId)
        {
            var userAccountIds = _db.BankAccounts
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .Select(a => a.Id)
                .ToList();

            return _db.Transactions
                .AsNoTracking()
                .Include(t => t.FromAccount)
                .Include(t => t.ToAccount)
                .Where(t => (t.FromAccountId != null && userAccountIds.Contains(t.FromAccountId.Value)) ||
                            (t.ToAccountId != null && userAccountIds.Contains(t.ToAccountId.Value)))
                .OrderByDescending(t => t.Date)
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
