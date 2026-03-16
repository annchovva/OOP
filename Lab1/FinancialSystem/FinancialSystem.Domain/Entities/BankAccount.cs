using System;
using FinancialSystem.Domain.Enums;

namespace FinancialSystem.Domain.Entities
{
    public class BankAccount
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public int BankId { get; set; }
        public virtual Bank Bank { get; set; } = null!;

        public string AccountNumber { get; set; } = string.Empty;
        public AccountType Type { get; set; } 
        public decimal Balance { get; set; }
        public decimal InterestRate { get; set; }
        public DateTime? LastInterestAccrual { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
