using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;
using FinancialSystem.Infrastructure;
using FinancialSystem.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System;
using Microsoft.EntityFrameworkCore;

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
                Timestamp = DateTime.Now,
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
                string[] parts = log.TechnicalData.Split(';');
                bool success = false;

                switch (log.ActionType)
                {
                    case "Transfer": success = UndoTransfer(parts); break;
                    case "OpenAccount": success = UndoOpenAccount(parts); break;
                    case "AccrueInterest": success = UndoAccrueInterest(parts); break;
                    case "ToggleBlock": success = UndoToggleBlock(parts); break;
                    case "ConfirmUser": success = UndoConfirmUser(parts); break;
                    case "ApproveJoin": success = UndoApproveJoin(parts); break;
                    case "ClaimSalary": success = UndoClaimSalary(parts); break;

                    // НОВЫЕ ТИПЫ:
                    case "Resign": success = UndoResign(parts); break;
                    case "JoinRequest": success = UndoJoinRequest(parts); break;
                    case "SalaryRequest": success = UndoSalaryRequest(parts); break;
                }

                if (success)
                {
                    log.IsReversed = true;
                    _db.SaveChanges();
                    return true;
                }
                return false;
            }
            catch { return false; }
        }

        // --- Новые методы отмены ---

        private bool UndoResign(string[] parts)
        {
            // ТехДанные: UserId;EnterpriseId
            int userId = int.Parse(parts[0]);
            int entId = int.Parse(parts[1]);

            var user = _db.Users.Find(userId);
            if (user != null && user.EnterpriseId == null) // Восстанавливаем только если еще никуда не вступил
            {
                user.EnterpriseId = entId;
                return true;
            }
            return false;
        }

        private bool UndoJoinRequest(string[] parts)
        {
            // ТехДанные: RequestId
            int requestId = int.Parse(parts[0]);
            var request = _db.SalaryRequests.Find(requestId); // Используем таблицу заявок
            if (request != null)
            {
                _db.SalaryRequests.Remove(request);
                return true;
            }
            return false;
        }

        private bool UndoSalaryRequest(string[] parts)
        {
            // ТехДанные: RequestId
            int requestId = int.Parse(parts[0]);
            var request = _db.SalaryRequests.Find(requestId);
            if (request != null && request.Status == SalaryRequestStatus.Pending)
            {
                _db.SalaryRequests.Remove(request);
                return true;
            }
            return false;
        }

        // --- Вспомогательные методы логики отмены ---

        private bool UndoTransfer(string[] parts)
        {
            int fromId = int.Parse(parts[0]);
            int toId = int.Parse(parts[1]);
            decimal amount = decimal.Parse(parts[2], CultureInfo.InvariantCulture);

            var source = _db.BankAccounts.Find(fromId);
            var target = _db.BankAccounts.Find(toId);

            if (source != null && target != null)
            {
                target.Balance -= amount;
                source.Balance += amount;
                return true;
            }
            return false;
        }

        private bool UndoOpenAccount(string[] parts)
        {
            int accId = int.Parse(parts[0]);
            var account = _db.BankAccounts.Find(accId);
            // Удаляем только если на счету 0 и он существует
            if (account != null && account.Balance == 0)
            {
                _db.BankAccounts.Remove(account);
                return true;
            }
            return false;
        }

        private bool UndoAccrueInterest(string[] parts)
        {
            int accId = int.Parse(parts[0]);
            decimal amount = decimal.Parse(parts[1], CultureInfo.InvariantCulture);

            var account = _db.BankAccounts.Find(accId);
            if (account != null)
            {
                account.Balance -= amount;
                return true;
            }
            return false;
        }

        private bool UndoToggleBlock(string[] parts)
        {
            int accId = int.Parse(parts[0]);
            var account = _db.BankAccounts.Find(accId);
            if (account != null)
            {
                account.IsBlocked = !account.IsBlocked;
                return true;
            }
            return false;
        }

        private bool UndoConfirmUser(string[] parts)
        {
            int userId = int.Parse(parts[0]);
            var user = _db.Users.Find(userId);
            if (user != null)
            {
                user.Status = UserStatus.Pending;
                user.IsApproved = false;
                return true;
            }
            return false;
        }

        private bool UndoApproveJoin(string[] parts)
        {
            int requestId = int.Parse(parts[0]);
            int userId = int.Parse(parts[1]);

            var request = _db.SalaryRequests.Find(requestId);
            var user = _db.Users.Find(userId);

            if (user != null)
            {
                user.EnterpriseId = null; // Увольняем обратно
                if (request != null) request.Status = SalaryRequestStatus.Pending;
                return true;
            }
            return false;
        }

        private bool UndoClaimSalary(string[] parts)
        {
            int accId = int.Parse(parts[0]);
            decimal amount = decimal.Parse(parts[1], CultureInfo.InvariantCulture);
            int reqId = int.Parse(parts[2]);

            var account = _db.BankAccounts.Find(accId);
            var request = _db.SalaryRequests.Find(reqId);

            if (account != null && request != null)
            {
                account.Balance -= amount; // Забираем деньги со счета
                request.Status = SalaryRequestStatus.Approved; // Возвращаем статус "Одобрено менеджером"
                return true;
            }
            return false;
        }
    }
}
