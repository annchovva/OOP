using System;
using System.Collections.Generic;
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

        // ШАГ 1: Клиент просится в компанию
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

        // ШАГ 2 (Менеджер): Одобряет сотрудника
        public void ApproveJoinRequest(int requestId)
        {
            var req = _db.SalaryRequests.Include(r => r.User).FirstOrDefault(r => r.Id == requestId);
            if (req != null)
            {
                req.Status = SalaryRequestStatus.Completed;
                req.User.EnterpriseId = req.EnterpriseId; // Привязываем юзера к фирме
                _db.SaveChanges();
            }
        }

        // ШАГ 3 (Клиент): Просит зарплату
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

        // ШАГ 4 (Менеджер): Одобряет конкретную сумму
        public void ApprovePaymentRequest(int requestId, decimal amount)
        {
            var req = _db.SalaryRequests.Find(requestId);
            if (req != null)
            {
                req.Status = SalaryRequestStatus.Approved;
                req.Amount = amount;
                _db.SaveChanges();
            }
        }

        // ШАГ 5 (Клиент): Видит одобренные выплаты
        public List<SalaryRequest> GetMyApprovedPayments(int userId)
        {
            return _db.SalaryRequests
                .Where(r => r.UserId == userId && r.Status == SalaryRequestStatus.Approved && r.Type == SalaryRequestType.Payment)
                .Include(r => r.Enterprise)
                .ToList();
        }

        // ШАГ 6 (Клиент): Выбирает счет и получает деньги
        public bool ClaimSalary(int requestId, int targetAccountId)
        {
            var req = _db.SalaryRequests.Find(requestId);
            var account = _db.BankAccounts.Find(targetAccountId);

            if (req == null || account == null || req.Status != SalaryRequestStatus.Approved)
                return false;

            if (account.IsBlocked) throw new Exception("Счет заблокирован менеджером.");

            // Начисляем деньги
            account.Balance += req.Amount;
            req.Status = SalaryRequestStatus.Completed;

            // Логируем
            _logService.Log(req.UserId, "Salary",
                $"Получена зарплата {req.Amount} от {req.EnterpriseId} на счет {account.AccountNumber}",
                $"0;{account.Id};{req.Amount}"); // 0 - отправитель (система)

            _db.SaveChanges();
            return true;
        }

        public List<SalaryRequest> GetPendingJoinRequests() =>
            _db.SalaryRequests.Where(r => r.Type == SalaryRequestType.Join && r.Status == SalaryRequestStatus.Pending).Include(r => r.User).Include(r => r.Enterprise).ToList();

        public List<SalaryRequest> GetPendingPaymentRequests() =>
            _db.SalaryRequests.Where(r => r.Type == SalaryRequestType.Payment && r.Status == SalaryRequestStatus.Pending).Include(r => r.User).Include(r => r.Enterprise).ToList();

        public List<Enterprise> GetEnterprisesWithEmployees() =>
            _db.Enterprises.Include(e => e.Employees).ToList();
    }
}
