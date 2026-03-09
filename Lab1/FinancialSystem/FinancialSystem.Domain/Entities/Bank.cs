using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSystem.Domain.Entities
{
    public class Bank
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BIC { get; set; } = string.Empty;

        public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
    }
}