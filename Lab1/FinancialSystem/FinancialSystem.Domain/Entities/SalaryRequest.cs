using FinancialSystem.Domain.Entities;

namespace FinancialSystem.Domain.Entities // Добавь это!
{
    public class SalaryRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int EnterpriseId { get; set; }
        public virtual Enterprise Enterprise { get; set; } = null!;

        public bool IsApproved { get; set; } = false;
        public DateTime RequestedAt { get; set; } = DateTime.Now;
    }
}
