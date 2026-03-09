using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSystem.Domain.Entities
{
    public class Account
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty; // Номер счета (напр. 20 цифр)

        // Используем decimal для денег!
        public decimal Balance { get; set; }
        public bool IsBlocked { get; set; }

        // Чей это счет и в каком банке
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int BankId { get; set; }
        public virtual Bank Bank { get; set; } = null!;
    }
}
