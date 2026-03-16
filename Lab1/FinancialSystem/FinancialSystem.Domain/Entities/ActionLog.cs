using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSystem.Domain.Entities
{
    public class ActionLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string ActionType { get; set; }
        public string Details { get; set; }

        public int UserId { get; set; }
        public bool IsReversed { get; set; } = false; // было ли действие отменено
        public string TechnicalData { get; set; } 
    }
}
