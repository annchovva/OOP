using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSystem.Domain.Entities
{
    public class Deposit
    {
        public int Id { get; set; }

        // Сумма вклада
        public decimal Amount { get; set; }

        // Процентная ставка (например, 0.05 для 5%)
        public decimal InterestRate { get; set; }

        // Дата открытия и срок в месяцах
        public DateTime StartDate { get; set; }
        public int DurationMonths { get; set; }

        // Активен ли вклад (или уже закрыт/выплачен)
        public bool IsActive { get; set; }

        // Чей это вклад
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        // В каком банке открыт вклад
        public int BankId { get; set; }
        public virtual Bank Bank { get; set; } = null!;

        // Метод для расчета даты окончания (удобно иметь под рукой)
        public DateTime EndDate => StartDate.AddMonths(DurationMonths);
    }
}
