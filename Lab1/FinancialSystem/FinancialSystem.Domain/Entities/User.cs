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
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        // Статус подтверждения 
        public bool IsApproved { get; set; }

        // Связи
        public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
        public virtual ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();
    }
}
