using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;

namespace FinancialSystem.Domain.Entities // Добавь это!
{
    public class SalaryRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int EnterpriseId { get; set; }
        public virtual Enterprise Enterprise { get; set; } = null!;

        public SalaryRequestType Type { get; set; }
        public SalaryRequestStatus Status { get; set; } = SalaryRequestStatus.Pending;

        // Сумма (заполняется менеджером при одобрении выплаты)
        public decimal Amount { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.Now;
    }
}
