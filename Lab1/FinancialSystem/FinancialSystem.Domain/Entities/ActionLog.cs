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

        // Тип действия: "Transfer", "OpenAccount", "CloseAccount"
        public string ActionType { get; set; }

        // Описание для человека: "Перевод 500 руб со счета 123 на 456"
        public string Details { get; set; }

        public int UserId { get; set; }

        // Флаг: было ли действие уже отменено
        public bool IsReversed { get; set; } = false;

        // Технические данные для "отката" (например: "FromAccId;ToAccId;Amount")
        public string TechnicalData { get; set; }
    }
}
