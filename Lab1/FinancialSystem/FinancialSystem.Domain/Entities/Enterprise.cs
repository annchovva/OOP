using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSystem.Domain.Entities
{
    public class Enterprise
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TIN { get; set; } = string.Empty; // ИНН

        // Список сотрудников (клиентов банка)
        public virtual ICollection<User> Employees { get; set; } = new List<User>();
    }
}
