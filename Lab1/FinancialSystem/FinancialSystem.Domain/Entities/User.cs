using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using FinancialSystem.Domain.Enums;

namespace FinancialSystem.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }

        // Для связи с предприятием (может быть null, если клиент не сотрудник)
        public int? EnterpriseId { get; set; }
        public virtual Enterprise? Enterprise { get; set; }

        public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
        public bool IsApproved { get; set; } = false; // По умолчанию менеджер должен подтвердить
    }
}
