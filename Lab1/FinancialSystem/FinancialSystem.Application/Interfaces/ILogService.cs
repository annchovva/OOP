using System.Collections.Generic;
using FinancialSystem.Domain.Entities;

namespace FinancialSystem.Application.Interfaces
{
    public interface ILogService
    {
        // Записать новое действие в систему
        void Log(int userId, string type, string details, string techData = "");

        // Получить список всех логов для Администратора
        List<ActionLog> GetAllLogs();

        // Отменить конкретное действие по его ID
        bool UndoAction(int logId);
    }
}

