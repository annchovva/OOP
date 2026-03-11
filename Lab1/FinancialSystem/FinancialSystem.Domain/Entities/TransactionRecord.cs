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
        // Откуда (может быть null, если это пополнение наличными/зарплата извне)
        public int? FromAccountId { get; set; }
        // Куда (может быть null, если это снятие)
        public int? ToAccountId { get; set; }

        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }
}