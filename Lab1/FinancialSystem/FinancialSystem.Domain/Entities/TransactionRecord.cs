using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinancialSystem.Domain.Enums;

namespace FinancialSystem.Domain.Entities
{
    public class TransactionRecord
    {
        public int Id { get; set; }
        public int? FromAccountId { get; set; }
        public virtual BankAccount FromAccount { get; set; }
        public int? ToAccountId { get; set; }
        public virtual BankAccount ToAccount { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty; // описание транзакции
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
