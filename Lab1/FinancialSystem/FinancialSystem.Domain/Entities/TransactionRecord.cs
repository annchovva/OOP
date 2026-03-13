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
        // НОВОЕ: Навигационное свойство для связи
        public virtual BankAccount FromAccount { get; set; }

        public int? ToAccountId { get; set; }
        // НОВОЕ: Навигационное свойство для связи
        public virtual BankAccount ToAccount { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty; // Добавь это для описания
        public DateTime Date { get; set; } = DateTime.Now;
    }
}