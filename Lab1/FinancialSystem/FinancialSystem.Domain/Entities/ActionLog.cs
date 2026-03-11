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
        public int UserId { get; set; }
        public string ActionType { get; set; } = string.Empty; // Имя класса команды
        public string SerializedCommandData { get; set; } = string.Empty; // JSON с данными для отмены
        public DateTime Date { get; set; } = DateTime.Now;
        public bool IsUndone { get; set; } // Отменено ли это действие
    }
}
