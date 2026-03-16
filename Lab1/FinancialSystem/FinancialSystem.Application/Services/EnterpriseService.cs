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
        public bool ClaimSalary(int requestId, int targetAccountId)
        {
            // 1. Ищем заявку и счет
            var req = _db.SalaryRequests
                .Include(r => r.Enterprise)
                .FirstOrDefault(r => r.Id == requestId);

            var account = _db.BankAccounts.Find(targetAccountId);

            // 2. Проверки
            if (req == null || account == null || req.Status != SalaryRequestStatus.Approved)
                return false;

            if (account.IsBlocked)
                throw new Exception("Операция невозможна: ваш счет заблокирован менеджером.");

            // 3. Начисляем деньги и меняем статус
            account.Balance += req.Amount;
            req.Status = SalaryRequestStatus.Completed;

            // 4. Создаем транзакцию для истории операций по счету
            _db.Transactions.Add(new TransactionRecord
            {
                ToAccountId = account.Id,
                FromAccountId = null, // Источник - внешняя выплата
                Amount = req.Amount,
                Date = DateTime.Now,
                Description = $"Зарплатный проект: {req.Enterprise?.Name ?? "Предприятие"}"
            });

            // 5. ИСПРАВЛЕННЫЙ ЛОГ ДЛЯ АДМИНИСТРАТОРА
            // Тип: ClaimSalary
            // ТехДанные: "ID_Счета;Сумма;ID_Заявки" (сумма через InvariantCulture для точки)
            _logService.Log(req.UserId, "ClaimSalary",
                $"Получена зарплата {req.Amount} от {req.Enterprise?.Name} на счет {account.AccountNumber}",
                $"{account.Id};{req.Amount.ToString(CultureInfo.InvariantCulture)};{req.Id}");

            _db.SaveChanges();
            return true;
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
        public void ApproveJoinRequest(int requestId)
        {
            var req = _db.SalaryRequests
                .Include(r => r.User)
                .Include(r => r.Enterprise)
                .FirstOrDefault(r => r.Id == requestId);

            if (req != null)
            {
                req.Status = SalaryRequestStatus.Completed;
                req.User.EnterpriseId = req.EnterpriseId; // Привязываем юзера к фирме

                // ДОБАВЛЕННЫЙ ЛОГ: Чтобы админ мог отменить прием сотрудника
                // Тип: ApproveJoin
                // ТехДанные: "ID_Заявки;ID_Пользователя"
                _logService.Log(req.UserId, "ApproveJoin",
                    $"Сотрудник {req.User.Login} принят в организацию {req.Enterprise?.Name}",
                    $"{req.Id};{req.UserId}");

                _db.SaveChanges();
            }
        }

        public void ResignFromEnterprise(int userId)
        {
            var user = _db.Users.Find(userId);
            if (user != null)
            {
                user.EnterpriseId = null; // Убираем привязку к фирме

                // (Опционально) Удаляем все текущие ожидающие заявки на зарплату или вступление
                var pendingRequests = _db.SalaryRequests
                    .Where(r => r.UserId == userId && r.Status == SalaryRequestStatus.Pending);

                _db.SalaryRequests.RemoveRange(pendingRequests);

                _db.SaveChanges();
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
