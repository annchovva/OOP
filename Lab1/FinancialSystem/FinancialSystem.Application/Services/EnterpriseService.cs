using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FinancialSystem.Application.Interfaces;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;
using FinancialSystem.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinancialSystem.Application.Services
{
    public class EnterpriseService : IEnterpriseService
    {
        private readonly FinanceDbContext _db;
        private readonly ILogService _logService;

        public EnterpriseService(FinanceDbContext db, ILogService logService)
        {
            _db = db;
            _logService = logService;
        }

        public List<Enterprise> GetAllEnterprises() => _db.Enterprises.ToList();

        public void SendJoinRequest(int userId, int enterpriseId)
        {
            var request = new SalaryRequest
            {
                UserId = userId,
                EnterpriseId = enterpriseId,
                Type = SalaryRequestType.Join,
                Status = SalaryRequestStatus.Pending
            };
            _db.SalaryRequests.Add(request);
            _db.SaveChanges();
        }

        public void ApproveJoinRequest(int requestId)
        {
            var req = _db.SalaryRequests
                .Include(r => r.User)
                .Include(r => r.Enterprise)
                .FirstOrDefault(r => r.Id == requestId);

            if (req != null)
            {
                req.Status = SalaryRequestStatus.Completed;
                req.User.EnterpriseId = req.EnterpriseId;

                _db.SaveChanges();

                _logService.Log(req.UserId, "ApproveJoin",
                    $"Сотрудник {req.User.Login} принят в организацию {req.Enterprise?.Name}",
                    $"{req.Id};{req.UserId}");
            }
        }

        public void SendSalaryPaymentRequest(int userId)
        {
            var user = _db.Users.Find(userId);
            if (user?.EnterpriseId == null) throw new Exception("Вы не являетесь сотрудником предприятия.");

            var request = new SalaryRequest
            {
                UserId = userId,
                EnterpriseId = user.EnterpriseId.Value,
                Type = SalaryRequestType.Payment,
                Status = SalaryRequestStatus.Pending
            };
            _db.SalaryRequests.Add(request);
            _db.SaveChanges();
        }

        public void ApprovePaymentRequest(int requestId, decimal amount)
        {
            var req = _db.SalaryRequests.Find(requestId);
            if (req != null)
            {
                req.Status = SalaryRequestStatus.Approved;
                req.Amount = amount;
                _db.SaveChanges();
            }

            string userName = req.User?.Login ?? $"ID:{req.UserId}";
            string enterpriseName = req.Enterprise?.Name ?? $"ID:{req.EnterpriseId}";
        }

        public List<SalaryRequest> GetMyApprovedPayments(int userId)
        {
            return _db.SalaryRequests
                .Where(r => r.UserId == userId && r.Status == SalaryRequestStatus.Approved && r.Type == SalaryRequestType.Payment)
                .Include(r => r.Enterprise)
                .ToList();
        }

        public bool ClaimSalary(int requestId, int targetAccountId)
        {
            var req = _db.SalaryRequests
                .Include(r => r.Enterprise)
                .FirstOrDefault(r => r.Id == requestId);

            var account = _db.BankAccounts.Find(targetAccountId);

            if (req == null || account == null || req.Status != SalaryRequestStatus.Approved)
                return false;

            if (account.IsBlocked)
                throw new Exception("Операция невозможна: ваш счет заблокирован менеджером.");

            account.Balance += req.Amount;
            req.Status = SalaryRequestStatus.Completed;

            _db.Transactions.Add(new TransactionRecord
            {
                ToAccountId = account.Id,
                FromAccountId = null,
                Amount = req.Amount,
                Date = DateTime.Now,
                Description = $"Зарплатный проект: {req.Enterprise?.Name ?? "Предприятие"}"
            });

            _db.SaveChanges();

            _logService.Log(req.UserId, "ClaimSalary",
                $"Получена зарплата {req.Amount} от {req.Enterprise?.Name} на счет {account.AccountNumber}",
                $"{account.Id};{req.Amount.ToString(CultureInfo.InvariantCulture)};{req.Id}");

            return true;
        }

        public void ResignFromEnterprise(int userId)
        {
            var user = _db.Users.Include(u => u.Enterprise).FirstOrDefault(u => u.Id == userId);
            if (user != null && user.EnterpriseId != null)
            {
                string enterpriseName = user.Enterprise?.Name ?? "Неизвестно";
                int? oldEnterpriseId = user.EnterpriseId;

                user.EnterpriseId = null;

                var pendingRequests = _db.SalaryRequests
                    .Where(r => r.UserId == userId && r.Status == SalaryRequestStatus.Pending);

                // удаляем только неодобренные заявки
                _db.SalaryRequests.RemoveRange(pendingRequests);

                _db.SaveChanges();

                _logService.Log(userId, "Resign",
                    $"Пользователь уволился из организации {enterpriseName}",
                    $"{oldEnterpriseId}");
            }
        }

        public List<SalaryRequest> GetPendingJoinRequests() =>
            _db.SalaryRequests.Where(r => r.Type == SalaryRequestType.Join && r.Status == SalaryRequestStatus.Pending).Include(r => r.User).Include(r => r.Enterprise).ToList();

        public List<SalaryRequest> GetPendingPaymentRequests() =>
            _db.SalaryRequests.Where(r => r.Type == SalaryRequestType.Payment && r.Status == SalaryRequestStatus.Pending).Include(r => r.User).Include(r => r.Enterprise).ToList();

        public List<Enterprise> GetEnterprisesWithEmployees() =>
            _db.Enterprises.Include(e => e.Employees).ToList();
    }
}

