using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinancialSystem.Domain.Enums;

namespace FinancialSystem.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public TransactionType Type { get; set; }
        public string Description { get; set; } = string.Empty;

        // С какого счета на какой (могут быть null, если это просто пополнение наличными)
        public int? FromAccountId { get; set; }
        public virtual Account? FromAccount { get; set; }

        public int? ToAccountId { get; set; }
        public virtual Account? ToAccount { get; set; }
    }
}