using FinancialSystem.Domain.Entities;
using System.Collections.Generic;

namespace FinancialSystem.Application.Interfaces
{
    public interface IEnterpriseService
    {
        // 1. ДЛЯ КЛИЕНТА (СОТРУДНИКА)
        List<Enterprise> GetAllEnterprises();

        // Вступление в штат и увольнение
        void SendJoinRequest(int userId, int enterpriseId);
        void ResignFromEnterprise(int userId); // <-- ДОБАВЛЕНО: Увольнение по собственному желанию

        // Работа с зарплатой
        void SendSalaryPaymentRequest(int userId);
        List<SalaryRequest> GetMyApprovedPayments(int userId);
        bool ClaimSalary(int requestId, int targetAccountId);

        // 2. ДЛЯ МЕНЕДЖЕРА (АДМИНИСТРАТОРА)
        List<SalaryRequest> GetPendingJoinRequests();
        List<SalaryRequest> GetPendingPaymentRequests();

        void ApproveJoinRequest(int requestId);
        void ApprovePaymentRequest(int requestId, decimal amount);

        // Просмотр всех предприятий с сотрудниками (для формирования списков штата)
        List<Enterprise> GetEnterprisesWithEmployees();
    }
}
