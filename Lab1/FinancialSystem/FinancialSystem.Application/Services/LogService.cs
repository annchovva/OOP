using FinancialSystem.Domain.Entities;
using FinancialSystem.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Globalization; // Добавь для работы с числами
using FinancialSystem.Application.Interfaces;
using System;

namespace FinancialSystem.Application.Services
{
    public class LogService : ILogService
    {
        private readonly FinanceDbContext _db;

        public LogService(FinanceDbContext db)
        {
            _db = db;
        }

        public void Log(int userId, string type, string details, string techData = "")
        {
            var log = new ActionLog
            {
                UserId = userId,
                ActionType = type,
                Details = details,
                Timestamp = DateTime.Now, // Явно укажем время
                TechnicalData = techData,
                IsReversed = false
            };
            _db.ActionLogs.Add(log);
            _db.SaveChanges();
        }

        public List<ActionLog> GetAllLogs() => _db.ActionLogs.OrderByDescending(l => l.Timestamp).ToList();

        public bool UndoAction(int logId)
        {
            var log = _db.ActionLogs.Find(logId);
            if (log == null || log.IsReversed) return false;

            try
            {
                if (log.ActionType == "Transfer")
                {
                    var parts = log.TechnicalData.Split(';');
                    if (parts.Length < 3) return false;

                    int fromId = int.Parse(parts[0]);
                    int toId = int.Parse(parts[1]);

                    // Используем InvariantCulture, чтобы точка/запятая не ломали код
                    decimal amount = decimal.Parse(parts[2], CultureInfo.InvariantCulture);

                    var sourceAcc = _db.BankAccounts.Find(fromId);
                    var targetAcc = _db.BankAccounts.Find(toId);

                    if (sourceAcc != null && targetAcc != null)
                    {
                        targetAcc.Balance -= amount;
                        sourceAcc.Balance += amount;

                        log.IsReversed = true;
                        _db.SaveChanges();
                        return true;
                    }
                }
            }
            catch
            {
                return false; // Если данные в TechnicalData были битые
            }

            return false;
        }
    }
}
