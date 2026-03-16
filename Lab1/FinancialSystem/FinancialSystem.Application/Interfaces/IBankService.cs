using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;
using System.Collections.Generic;

namespace FinancialSystem.Application.Interfaces
{
    public interface IBankService
    {
        // Для банков
        List<Bank> GetAllBanks();

        // Для счетов
        List<BankAccount> GetUserAccounts(int userId);
        void OpenAccount(int userId, int bankId, AccountType type);
        void CloseAccount(int accountId);

        // Переводы и история
        bool TransferMoney(int fromAccountId, string toAccountNumber, decimal amount);
        List<TransactionRecord> GetTransactionHistory(int userId);
        void AccrueInterest(int accountId); // Накопление (начисление %)
        void OpenDeposit(int userId, int bankId, decimal initialAmount, decimal interestRate);
        List<BankAccount> GetAllAccounts(); // Для менеджера
        void ToggleBlock(int accountId);      // Блокировка/разблокировка
        List<TransactionRecord> GetAccountHistory(int accountId);


    }
}

