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
        public string ActionType { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public int UserId { get; set; }
        public bool IsReversed { get; set; } = false; // отменено ли действие
        public string TechnicalData { get; set; } = string.Empty;// строка для отмены
    }
}
