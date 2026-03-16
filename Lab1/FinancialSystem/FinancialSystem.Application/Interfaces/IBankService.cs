using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;
using System.Collections.Generic;

namespace FinancialSystem.Application.Interfaces
{
    public interface IBankService
    {
        List<Bank> GetAllBanks();
        List<BankAccount> GetUserAccounts(int userId);
        void OpenAccount(int userId, int bankId, AccountType type);
        void CloseAccount(int accountId);
        bool TransferMoney(int fromAccountId, string toAccountNumber, decimal amount);
        void OpenDeposit(int userId, int bankId, decimal initialAmount, decimal interestRate);
        void AccrueInterest(int accountId);
        List<BankAccount> GetAllAccounts(); 
        void ToggleBlock(int accountId);
        List<TransactionRecord> GetAccountHistory(int accountId);
    }
}
