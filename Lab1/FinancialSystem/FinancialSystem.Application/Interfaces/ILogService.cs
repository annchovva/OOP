using System.Collections.Generic;
using FinancialSystem.Domain.Entities;

namespace FinancialSystem.Application.Interfaces
{
    public interface ILogService
    {
        void Log(int userId, string type, string details, string techData = "");
        List<ActionLog> GetAllLogs();
        bool UndoAction(int logId);
    }
}


